# Complete GateSale Backend Deployment
param(
    [Parameter(Mandatory=$true)]
    [string]$AccountId,
    
    [string]$Region = "af-south-1",
    [string]$VpcId,
    [string]$SubnetId1,
    [string]$SubnetId2
)

Write-Host "🚀 GateSale Backend - Complete AWS Deployment" -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Green

# Validate AWS CLI
if (!(Get-Command aws -ErrorAction SilentlyContinue)) {
    Write-Host "❌ AWS CLI not found. Please install AWS CLI first." -ForegroundColor Red
    exit 1
}

# Validate Docker
if (!(Get-Command docker -ErrorAction SilentlyContinue)) {
    Write-Host "❌ Docker not found. Please install Docker first." -ForegroundColor Red
    exit 1
}

try {
    # Step 1: Run Database Migrations
    Write-Host "`n🗄️ Step 1: Database Migrations" -ForegroundColor Cyan
    .\migrate-database.ps1
    
    # Step 2: Setup ECS Infrastructure (if VPC details provided)
    if ($VpcId -and $SubnetId1 -and $SubnetId2) {
        Write-Host "`n🏗️ Step 2: Setting up ECS Infrastructure" -ForegroundColor Cyan
        .\setup-ecs-infrastructure.ps1 -VpcId $VpcId -SubnetId1 $SubnetId1 -SubnetId2 $SubnetId2
        
        # Wait for infrastructure to be ready
        Write-Host "⏳ Waiting for infrastructure to be ready..." -ForegroundColor Yellow
        Start-Sleep -Seconds 30
    } else {
        Write-Host "`n⚠️ Skipping infrastructure setup. Provide VPC and Subnet IDs to create infrastructure." -ForegroundColor Yellow
    }
    
    # Step 3: Deploy Application
    Write-Host "`n🚀 Step 3: Deploying Application to ECS" -ForegroundColor Cyan
    .\deploy-to-ecs.ps1 -AccountId $AccountId
    
    Write-Host "`n✅ Deployment completed successfully!" -ForegroundColor Green
    Write-Host "📱 Update your mobile app's ApiBaseUrl in MauiProgram.cs" -ForegroundColor Yellow
    
} catch {
    Write-Host "`n❌ Deployment failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host "`n📋 Next Steps:" -ForegroundColor Cyan
Write-Host "1. Get ALB DNS name from AWS Console" -ForegroundColor White
Write-Host "2. Update mobile app ApiBaseUrl" -ForegroundColor White
Write-Host "3. Test API endpoints" -ForegroundColor White
Write-Host "4. Generate APK with new backend URL" -ForegroundColor White