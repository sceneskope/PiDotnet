
Remove-Item -Recurse obj
Remove-Item -Recurse bin
dotnet2 restore
dotnet2 build -c Release
