Param(
    [Parameter(Mandatory=$true)]$namespace
)

$docker = "$PSScriptRoot\docker"
$publish = "$docker\publish"
Remove-Item -Recurse $docker -ErrorAction SilentlyContinue

dotnet restore
dotnet build -c Release
dotnet publish -c Release -o $publish

$Tag = $(Get-Date -Format yyyyMMdd)

Copy-Item Dockerfile, .dockerignore $DOCKER
Copy-Item start.sh $PUBLISH
docker build -t $namespace/dashboard-data-pusher $DOCKER
docker tag $namespace/dashboard-data-pusher $namespace/dashboard-data-pusher:$Tag
docker push $namespace/dashboard-data-pusher:$Tag

