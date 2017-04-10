#define REUSE_EXISTING
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Polly;
using SceneSkope.PowerBI;
using SceneSkope.PowerBI.Authenticators;
using SceneSkope.PowerBI.Models;

namespace DashboardDataPusher
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("usage: MultiTableSample <configuration file>");
            }
            else
            {
                try
                {
                    using (var cts = new CancellationTokenSource())
                    {
                        Console.CancelKeyPress += (_, ev) =>
                        {
                            ev.Cancel = true;
                            cts.Cancel();
                        };

                        RunAsync(args[0], cts.Token).GetAwaiter().GetResult();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
            if (Debugger.IsAttached)
            {
                Debugger.Break();
            }
        }

        private static readonly Policy RetryPolicy =
            Policy
            .Handle<PowerBIClientException>()
            .WaitAndRetryAsync(3, _ => TimeSpan.FromSeconds(1),
                (ex, ts) => Console.WriteLine($"Delaying {ts} due to {ex.Message}"));

        private static async Task RunAsync(string configurationFile, CancellationToken ct)
        {
            using (var httpClient = new HttpClient())
            {
                var configuration = JsonConvert.DeserializeObject<ClientConfiguration>(File.ReadAllText(configurationFile));
                var authenticator = new DeviceCodeAuthenticator(configuration.ClientId, configuration.TokenCacheState);
                var powerBIClient = new PowerBIClient(httpClient, authenticator)
                {
                    UseBeta = true
                };
                var existingDatasets = await powerBIClient.ListAllDatasetsAsync(ct).ConfigureAwait(false);
                var dataset = CreateDatasetDefinition();
                var datasetId = await GetOrCreateDataset(powerBIClient, dataset, existingDatasets, ct).ConfigureAwait(false);
                var perMinuteStatus = dataset.Tables[0].Name;

                var liveDataset = CreateLiveDatasetDefinition();
                var liveDatasetId = await GetOrCreateDataset(powerBIClient, liveDataset, existingDatasets, ct).ConfigureAwait(false);
                var liveStatus = liveDataset.Tables[0].Name;

                var random = new Random();
                var activePlayers = random.Next(0, 10);
                var playersToday = activePlayers;
                var playersAllTime = activePlayers;
                var resetDaysDuration = TimeSpan.FromMinutes(10);

                var dayStart = DateTime.MinValue;
                var minuteStart = DateTime.MinValue;
                while (!ct.IsCancellationRequested)
                {
                    var now = DateTime.UtcNow;
                    if (now > dayStart + TimeSpan.FromMinutes(10))
                    {
                        playersToday = 0;
                        dayStart = now;
                    }

                    var newActivePlayers = random.Next(0, 10);
                    var delta = newActivePlayers - activePlayers;
                    if (delta > 0)
                    {
                        playersToday += delta;
                        playersAllTime += delta;
                    }
                    activePlayers = newActivePlayers;

                    var liveRow = new
                    {
                        TimestampUtc = now,
                        ActivePlayers = activePlayers,
                        PlayersToday = playersToday,
                        PlayersAllTime = playersAllTime
                    };
                    await RetryPolicy.ExecuteAsync(cancel =>
                        powerBIClient.AddRowsAsync(liveDatasetId, liveStatus, new[] { liveRow }, cancel),
                        ct, false
                    ).ConfigureAwait(false);

                    if (now > minuteStart + TimeSpan.FromMinutes(1))
                    {
                        var minuteRows = new object[random.Next(3, 10)];
                        for (var i = 0; i < minuteRows.Length; i++)
                        {
                            minuteRows[i] = new
                            {
                                TimestampUtc = now,
                                Name = $"Player {i + 1}",
                                Image = $"https://lorempixel.com/400/200/sports/{i + 1}",
                                Count = random.Next(10, 50),
                                Score = random.Next(50, 10000)
                            };
                        }
                        await RetryPolicy.ExecuteAsync(cancel =>
                            powerBIClient.AddRowsAsync(datasetId, perMinuteStatus, minuteRows, cancel),
                            ct, false
                        ).ConfigureAwait(false);
                        minuteStart = now;
                    }
                    await Task.Delay(5000, ct).ConfigureAwait(false);
                }
            }
        }

        private static async Task<string> GetOrCreateDataset(PowerBIClient powerBIClient, Dataset dataset, PowerBIIdentity[] existingDatasets, CancellationToken ct)
        {
            var existingDataset = existingDatasets.SingleOrDefault(d => d.Name == dataset.Name);
#if !REUSE_EXISTING
                if (existingDataset != null)
                {
                    await powerBIClient.DeleteDatasetAsync(existingDataset.Id, ct).ConfigureAwait(false);
                    existingDataset = null;
                }
#endif
            string datasetId;
            if (existingDataset == null)
            {
                var created = await powerBIClient.CreateDatasetAsync(dataset, DefaultRetentionPolicy.None, ct).ConfigureAwait(false);
                datasetId = created.Id;
            }
            else
            {
                datasetId = existingDataset.Id;
            }

            return datasetId;
        }

        public static Dataset CreateLiveDatasetDefinition() =>
            new Dataset
            {
                DefaultMode = DatasetMode.PushStreaming,
                Name = "MultiTableStatus",
                Tables = new[]
                {
                    new Table
                    {
                        Name = "LiveStatus",
                        Columns = new[]
                        {
                            new Column { Name = "TimestampUtc", DataType = DataType.DateTime },
                            new Column { Name = "ActivePlayers", DataType = DataType.Int64 },
                            new Column { Name = "PlayersToday", DataType = DataType.Int64 },
                            new Column { Name = "PlayersAllTime", DataType = DataType.Int64 }
                        }
                    }
                }
            };

        private static Dataset CreateDatasetDefinition() =>
            new Dataset
            {
                DefaultMode = DatasetMode.Push,
                Name = "MultiTableExample",
                Tables = new[]
                {
                    new Table
                    {
                        Name = "PerMinuteStatus",
                        Columns = new[]
                        {
                            new Column { Name = "TimestampUtc", DataType = DataType.DateTime },
                            new Column { Name = "Name", DataType = DataType.String },
                            new Column { Name = "Image", DataType = DataType.String, DataCategory = DataCategory.ImageUrl },
                            new Column { Name = "Count", DataType = DataType.Int64, SummarizeBy = AggregationMethod.Sum },
                            new Column { Name = "Score", DataType = DataType.Int64, SummarizeBy = AggregationMethod.Sum }
                        },
                        Measures = new[]
                        {
                            new Measure { Name = "AverageScore", Expression = "SUM(PerMinuteStatus[Score])/SUM(PerMinuteStatus[Count])" }
                        }
                    }
                }
            };
    }
}