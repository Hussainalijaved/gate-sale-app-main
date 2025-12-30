$BackendPort = 5221
$ProjectRoot = "d:\Projects\Fiverrr\GateSale-App-main"
$BackendPath = "$ProjectRoot\GateSaleBackend\GateSale.API"
$CloudflaredPath = "C:\Program Files (x86)\cloudflared\cloudflared.exe"

Write-Host "🚀 Starting GateSale Stable Environment..." -ForegroundColor Cyan

# 1. Run Connection Automation
Write-Host "🔄 Synchronizing connection..." -ForegroundColor Yellow
& "$ProjectRoot\UpdateConnection.ps1"


# 2. Start Backend API
Write-Host "⚙️ Starting Backend API on port $BackendPort..." -ForegroundColor Yellow
Push-Location $BackendPath
try {
    dotnet run --urls "http://0.0.0.0:$BackendPort"
}
finally {
    Pop-Location
}
