param(
    [ValidateSet("2022", "2024")]
    [string[]] $Versions = @("2022", "2024"),
    [ValidateSet("Debug", "Release")]
    [string] $Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$root = Split-Path $PSScriptRoot -Parent

foreach ($version in $Versions) {
    dotnet build (Join-Path $root "RevitNumbering.sln") -c $Configuration -p:RevitVersion=$version
    if ($LASTEXITCODE -ne 0) {
        throw "Сборка под Revit $version завершилась с ошибкой."
    }

    $source = Join-Path $root "artifacts\$version\$Configuration"
    $addins = Join-Path $env:APPDATA "Autodesk\Revit\Addins\$version"
    $target = Join-Path $addins "RevitNumbering"

    New-Item -ItemType Directory -Force -Path $target | Out-Null
    Copy-Item (Join-Path $source "*") $target -Recurse -Force
    Copy-Item (Join-Path $root "addin\RevitNumbering.addin") $addins -Force

    Write-Host "Установлено для Revit $version -> $target"
}
