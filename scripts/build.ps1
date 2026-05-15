param(
    [string]$Configuration = "Debug"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root "Easydict.Windows\Easydict.Windows.csproj"

dotnet restore $project
dotnet build $project --configuration $Configuration --no-restore

