msbuild .\Hdq.RestBus.csproj
nuget pack .\Hdq.RestBus.csproj $env.NUGET_KEY  -Source https://www.nuget.org/api/v2/package

