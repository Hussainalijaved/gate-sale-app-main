# GateSale Backend ECS Deployment Script
param(
    [string]$AccountId = "357278409567",
    [string]$Region = "af-south-1",
    [string]$ClusterName = "gatesale-cluster",
    [string]$ServiceName = "gatesale-backend-service"
)

Write-Host "🚀 Starting GateSale Backend Deployment to ECS..." -ForegroundColor Green

# Set variables
$ECRRepo = "$AccountId.dkr.ecr.$Region.amazonaws.com/gatesale-backend"
$ImageTag = "latest"

try {
    # Step 1: Login to ECR
    Write-Host "📦 Logging into ECR..." -ForegroundColor Yellow
    aws ecr get-login-password --region $Region | docker login --username AWS --password-stdin $ECRRepo

    # Step 2: Create ECR repository if it doesn't exist
    Write-Host "🏗️ Creating ECR repository..." -ForegroundColor Yellow
    aws ecr create-repository --repository-name gatesale-backend --region $Region 2>$null

    # Step 3: Build Docker image
    Write-Host "🔨 Building Docker image..." -ForegroundColor Yellow
    docker build -t gatesale-backend .
    docker tag gatesale-backend:latest $ECRRepo:$ImageTag

    # Step 4: Push to ECR
    Write-Host "⬆️ Pushing image to ECR..." -ForegroundColor Yellow
    docker push $ECRRepo:$ImageTag

    # Step 5: Update task definition
    Write-Host "📝 Updating ECS task definition..." -ForegroundColor Yellow
    $taskDefContent = Get-Content "ecs-task-definition.json" -Raw
    $taskDefContent = $taskDefContent.Replace("YOUR_ACCOUNT_ID", $AccountId)
    $taskDefContent | Out-File "ecs-task-definition-updated.json" -Encoding UTF8

    # Step 6: Register task definition
    Write-Host "📋 Registering task definition..." -ForegroundColor Yellow
    aws ecs register-task-definition --cli-input-json file://ecs-task-definition-updated.json --region $Region

    # Step 7: Update service
    Write-Host "🔄 Updating ECS service..." -ForegroundColor Yellow
    aws ecs update-service --cluster $ClusterName --service $ServiceName --task-definition gatesale-backend --region $Region

    Write-Host "✅ Deployment completed successfully!" -ForegroundColor Green
    Write-Host "🌐 Your backend will be available at the ALB endpoint" -ForegroundColor Cyan

} catch {
    Write-Host "❌ Deployment failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}