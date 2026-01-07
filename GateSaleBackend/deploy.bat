@echo off
echo Checking Docker installation...
docker --version >nul 2>&1
if %errorlevel% neq 0 (
    echo Docker not found. Please install Docker Desktop from https://www.docker.com/products/docker-desktop
    echo After installation, restart your computer and run this script again.
    pause
    exit /b 1
)

echo Checking AWS CLI installation...
aws --version >nul 2>&1
if %errorlevel% neq 0 (
    echo AWS CLI not found. Please install AWS CLI from https://aws.amazon.com/cli/
    echo After installation, run: aws configure
    pause
    exit /b 1
)

echo All tools are installed!
echo Running deployment...

REM Login to ECR
echo Logging into ECR...
aws ecr get-login-password --region af-south-1 | docker login --username AWS --password-stdin 357278409567.dkr.ecr.af-south-1.amazonaws.com

REM Create ECR repository
echo Creating ECR repository...
aws ecr create-repository --repository-name gatesale-backend --region af-south-1

REM Build and tag image
echo Building Docker image...
docker build -t gatesale-backend .
docker tag gatesale-backend:latest 357278409567.dkr.ecr.af-south-1.amazonaws.com/gatesale-backend:latest

REM Push image
echo Pushing to ECR...
docker push 357278409567.dkr.ecr.af-south-1.amazonaws.com/gatesale-backend:latest

echo Deployment completed!
pause