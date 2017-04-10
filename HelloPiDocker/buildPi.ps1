Param(
    [Parameter(Mandatory=$true)]$namespace
)

$name="hello-pi-docker"
$docker = "$PSScriptRoot\docker"
$publish = "$docker\publish"
Remove-Item -Recurse $docker -ErrorAction SilentlyContinue

dotnet restore
dotnet build -c Release
dotnet publish -c Release -o $publish

$Tag = $(Get-Date -Format yyyyMMdd)

Copy-Item Dockerfile, .dockerignore $DOCKER
docker build -t $namespace/$name $DOCKER
docker tag $namespace/$name $namespace/${name}:$Tag
docker push $namespace/${name}:$Tag
docker push $namespace/${name}:latest
