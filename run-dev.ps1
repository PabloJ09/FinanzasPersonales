# Starts API and UI for local development
param(
  [string]$ApiUrl = "http://localhost:5158"
)

$ErrorActionPreference = "Stop"

Write-Host "Starting API..." -ForegroundColor Cyan
Start-Job -Name API -ScriptBlock {
  Set-Location "$PSScriptRoot\FinanzasPersonales"
  dotnet run --project ".\FinanzasPersonales.csproj"
}

Start-Sleep -Seconds 2

Write-Host "Starting UI at $ApiUrl ..." -ForegroundColor Cyan
Start-Job -Name UI -ScriptBlock {
  Set-Location "$PSScriptRoot\FinanzasPersonales.UI"
  $env:VITE_API_BASE = $using:ApiUrl
  npm run dev
}

Write-Host "Jobs started. Use 'Get-Job' and 'Receive-Job' to view output." -ForegroundColor Green
