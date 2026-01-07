# GateSale Backend AWS ECS Deployment

## Prerequisites
1. **Docker Desktop** - Download from https://www.docker.com/products/docker-desktop
2. **AWS CLI** - Download from https://aws.amazon.com/cli/
3. **AWS Account configured** - Run `aws configure` with your credentials

## Quick Deployment

### Step 1: Database Migration (Already Done ✓)
Database migrations have been applied to your RDS PostgreSQL instance.

### Step 2: Deploy to ECS
```cmd
cd GateSaleBackend
deploy-all.bat
```

This will:
- Build Docker image
- Push to ECR
- Create ECS cluster and service
- Deploy your backend
- Provide you with the public IP

### Step 3: Update Mobile App
After deployment, update the ApiBaseUrl in your mobile app:

File: `gate-sale-app-maui\MauiProgram.cs`
```csharp
public static readonly string ApiBaseUrl = "http://YOUR_PUBLIC_IP/";
```

### Step 4: Test API
Test your deployed API:
```
http://YOUR_PUBLIC_IP/api/test
```

### Step 5: Generate APK
Build your mobile app with the new backend URL.

## Manual Commands (if needed)

### Build and Push Docker Image
```cmd
aws ecr get-login-password --region af-south-1 | docker login --username AWS --password-stdin 357278409567.dkr.ecr.af-south-1.amazonaws.com
docker build -t gatesale-backend .
docker tag gatesale-backend:latest 357278409567.dkr.ecr.af-south-1.amazonaws.com/gatesale-backend:latest
docker push 357278409567.dkr.ecr.af-south-1.amazonaws.com/gatesale-backend:latest
```

### Create ECS Service
```cmd
aws ecs create-cluster --cluster-name gatesale-cluster --region af-south-1
aws ecs register-task-definition --cli-input-json file://ecs-task-definition.json --region af-south-1
aws ecs create-service --cluster gatesale-cluster --service-name gatesale-service --task-definition gatesale-backend --desired-count 1 --launch-type FARGATE --region af-south-1
```

## Troubleshooting

### If Docker is not installed:
1. Download Docker Desktop
2. Install and restart computer
3. Run deployment again

### If AWS CLI is not configured:
```cmd
aws configure
```
Enter your:
- AWS Access Key ID
- AWS Secret Access Key  
- Default region: af-south-1
- Default output format: json

### Check deployment status:
```cmd
aws ecs describe-services --cluster gatesale-cluster --services gatesale-service --region af-south-1
```

## Account Details
- Account ID: 357278409567
- Region: af-south-1 (Cape Town)
- ECR Repository: 357278409567.dkr.ecr.af-south-1.amazonaws.com/gatesale-backend