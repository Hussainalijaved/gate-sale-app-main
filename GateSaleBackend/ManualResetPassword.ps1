# Manual Reset Password Script for GateSale
# This script calls the temporary admin endpoint to reset a user's password.

param (
    [Parameter(Mandatory=$true)]
    [string]$Email,

    [Parameter(Mandatory=$true)]
    [string]$NewPassword,

    [string]$BaseUrl = "http://localhost:5000" # Update this to your API URL
)

$url = "$BaseUrl/api/Auth/admin-manual-reset"
$body = @{
    email = $Email
    code = "000000" # Dummy code, not used by the admin endpoint
    newPassword = $NewPassword
} | ConvertTo-Json

try {
    Write-Host "Attempting to reset password for $Email..." -ForegroundColor Cyan
    $response = Invoke-RestMethod -Uri $url -Method Post -Body $body -ContentType "application/json"
    Write-Host "Success: $($response.message)" -ForegroundColor Green
}
catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $errorDetails = $_.Exception.Response.GetResponseStream()
        $reader = New-Object System.IO.StreamReader($errorDetails)
        $errorText = $reader.ReadToEnd()
        Write-Host "Details: $errorText" -ForegroundColor Yellow
    }
}
