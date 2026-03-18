param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

dotnet restore ./HCL.CS.SF.sln
dotnet build ./HCL.CS.SF.sln -c $Configuration --no-restore
dotnet test ./tests/HCL.CS.SF.IntegrationTests/IntegrationTests.csproj -c $Configuration --no-build
