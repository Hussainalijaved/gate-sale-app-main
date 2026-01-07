# Database Migration Script for AWS RDS
param(
    [string]$ConnectionString = "Host=gatesale-db.c3uu08mis7yb.af-south-1.rds.amazonaws.com;Port=5432;Database=gatesale;Username=gatesaledb;Password=gatesaledb;"
)

Write-Host "🗄️ Starting Database Migration..." -ForegroundColor Green

try {
    # Set connection string for production
    $env:ConnectionStrings__ProductionConnection = $ConnectionString
    
    # Navigate to API project
    Set-Location "GateSale.API"
    
    # Run migrations
    Write-Host "📊 Applying database migrations..." -ForegroundColor Yellow
    dotnet ef database update --connection $ConnectionString --project ../GateSale.Infrastructure --startup-project .
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Database migrations completed successfully!" -ForegroundColor Green
    } else {
        throw "Migration failed with exit code $LASTEXITCODE"
    }
    
} catch {
    Write-Host "❌ Migration failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
} finally {
    Set-Location ".."
}