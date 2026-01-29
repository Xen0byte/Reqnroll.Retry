#!/usr/bin/env pwsh

# exit immediately if a command exits with a non-zero status
$ErrorActionPreference = "Stop"

# construct directory paths
$ScriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
$RootDirectory = Split-Path -Parent $ScriptDirectory
$SourceDirectory = Join-Path $RootDirectory "source"
$OutputDirectory = Join-Path $RootDirectory "nupkgs"

Write-Host "Packing Retry.Reqnroll Packages ..." -ForegroundColor Cyan

# purge output directory
if (Test-Path $OutputDirectory) {
    Remove-Item -Recurse -Force $OutputDirectory
}

# create output directory
New-Item -ItemType Directory -Path $OutputDirectory | Out-Null

# pack all generator projects
$projects = @(
    "Reqnroll.Retry.MSTest",
    "Reqnroll.Retry.NUnit",
    "Reqnroll.Retry.xUnit",
    "Reqnroll.Retry.TUnit"
)

foreach ($project in $projects) {
    Write-Host "Packing $project ..." -ForegroundColor Yellow

    dotnet pack "$SourceDirectory/$project/$project.csproj" `
        --configuration Release `
        --output $OutputDirectory

    if ($LASTEXITCODE -ne 0) {
        Write-Host "Failed To Pack $project" -ForegroundColor Red

        exit 1
    }
}

Write-Host "`nPackages Created In $OutputDirectory`:" -ForegroundColor Green

Get-ChildItem "$OutputDirectory/*.nupkg" | ForEach-Object { Write-Host "  $_" }
