

$docker = "$PSScriptRoot\docker"
$publish = "$docker\publish"
Remove-Item -Recurse $docker -ErrorAction SilentlyContinue

dotnet restore
dotnet build -c Release
dotnet publish -c Release -o $publish

$Tag = $(Get-Date -Format yyyyMMdd)

Copy-Item Dockerfile, .dockerignore $DOCKER
Copy-Item start.sh, configuration.json $PUBLISH
docker build -t sceneskope/dashboard-data-pusher $DOCKER
docker tag sceneskope/dashboard-data-pusher sceneskope/dashboard-data-pusher:$Tag
docker push sceneskope/dashboard-data-pusher:$Tag

