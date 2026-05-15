$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root "Easydict.Windows\Easydict.Windows.csproj"

dotnet publish $project `
    --configuration Release `
    --runtime win-x64 `
    --self-contained true `
    /p:PublishProfile=win-x64-folder

