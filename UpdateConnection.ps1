$BackendPort = 5221
$ProjectRoot = "d:\Projects\Fiverrr\GateSale-App-main"
$MauiProgramPath = "$ProjectRoot\gate-sale-app-maui\MauiProgram.cs"
$AppSettingsPath = "$ProjectRoot\GateSaleBackend\GateSale.API\appsettings.json"
$CloudflaredPath = "C:\Program Files (x86)\cloudflared\cloudflared.exe"
$LogFile = "$ProjectRoot\tunnel.log"

Write-Host "Cleaning up old tunnel processes..."
Get-Process cloudflared -ErrorAction SilentlyContinue | Stop-Process -Force

if (Test-Path $LogFile) { Remove-Item $LogFile -Force }

Write-Host "Starting Cloudflare tunnel..."
Start-Process -FilePath $CloudflaredPath -ArgumentList "tunnel --url http://localhost:$BackendPort" -RedirectStandardError $LogFile -NoNewWindow

Write-Host "Waiting for tunnel URL..."
$url = $null
$retryCount = 0
while ($null -eq $url -and $retryCount -lt 20) {
    Start-Sleep -Seconds 2
    if (Test-Path $LogFile) {
        $logContent = Get-Content $LogFile -Raw
        if ($logContent -match "https://[a-z0-9-]+\.trycloudflare\.com") {
            $url = $Matches[0]
        }
    }
    $retryCount++
}

if ($null -eq $url) {
    Write-Host "Error: Failed to extract Cloudflare URL from log."
    exit 1
}

Write-Host "New Tunnel URL: $url"

# 1. Update MauiProgram.cs
Write-Host "Updating MauiProgram.cs..."
$mauiContent = Get-Content $MauiProgramPath -Raw
# Match both 'public static string' and 'public static readonly string'
$mauiSearch = 'public static (readonly )?string ApiBaseUrl = "https://[^"]+/"?'
$mauiReplace = 'public static readonly string ApiBaseUrl = "' + $url + '/"'
$newMauiContent = $mauiContent -replace $mauiSearch, $mauiReplace
$newMauiContent | Set-Content $MauiProgramPath

# 2. Update appsettings.json
Write-Host "Updating appsettings.json..."
$jsonContent = Get-Content $AppSettingsPath -Raw

$search1 = '"WebsiteUrl": "https://[^"]+"'
$replace1 = '"WebsiteUrl": "' + $url + '"'
$jsonContent = $jsonContent -replace $search1, $replace1

$search2 = '"ApiUrl": "https://[^"]+"'
$replace2 = '"ApiUrl": "' + $url + '"'
$jsonContent = $jsonContent -replace $search2, $replace2

$search3 = '"DefaultNotifyUrl": "https://[^"]+/api/Payment/webhook"'
$replace3 = '"DefaultNotifyUrl": "' + $url + '/api/Payment/webhook"'
$jsonContent = $jsonContent -replace $search3, $replace3

$jsonContent | Set-Content $AppSettingsPath

Write-Host "Connection synchronized successfully!"
