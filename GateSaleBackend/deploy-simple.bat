@echo off
echo ========================================
echo  Simple ECS Deployment (Pre-built Image)
echo ========================================

REM Check AWS credentials
aws sts get-caller-identity >nul 2>&1
if %errorlevel% neq 0 (
    echo ❌ AWS not configured. Run: configure-aws.bat first
    pause
    exit /b 1
)

echo [1/4] Creating ECS cluster...
aws ecs create-cluster --cluster-name gatesale-cluster --region af-south-1

echo [2/4] Creating CloudWatch log group...
aws logs create-log-group --log-group-name "/ecs/gatesale-backend" --region af-south-1 2>nul

echo [3/4] Creating task definition with .NET image...
echo {> simple-task-def.json
echo   "family": "gatesale-backend",>> simple-task-def.json
echo   "networkMode": "awsvpc",>> simple-task-def.json
echo   "requiresCompatibilities": ["FARGATE"],>> simple-task-def.json
echo   "cpu": "512",>> simple-task-def.json
echo   "memory": "1024",>> simple-task-def.json
echo   "executionRoleArn": "arn:aws:iam::357278409567:role/ecsTaskExecutionRole",>> simple-task-def.json
echo   "containerDefinitions": [>> simple-task-def.json
echo     {>> simple-task-def.json
echo       "name": "gatesale-api",>> simple-task-def.json
echo       "image": "mcr.microsoft.com/dotnet/samples:aspnetapp",>> simple-task-def.json
echo       "portMappings": [>> simple-task-def.json
echo         {>> simple-task-def.json
echo           "containerPort": 80,>> simple-task-def.json
echo           "protocol": "tcp">> simple-task-def.json
echo         }>> simple-task-def.json
echo       ],>> simple-task-def.json
echo       "essential": true,>> simple-task-def.json
echo       "logConfiguration": {>> simple-task-def.json
echo         "logDriver": "awslogs",>> simple-task-def.json
echo         "options": {>> simple-task-def.json
echo           "awslogs-group": "/ecs/gatesale-backend",>> simple-task-def.json
echo           "awslogs-region": "af-south-1",>> simple-task-def.json
echo           "awslogs-stream-prefix": "ecs">> simple-task-def.json
echo         }>> simple-task-def.json
echo       }>> simple-task-def.json
echo     }>> simple-task-def.json
echo   ]>> simple-task-def.json
echo }>> simple-task-def.json

aws ecs register-task-definition --cli-input-json file://simple-task-def.json --region af-south-1

REM Get default VPC and subnets
for /f "tokens=*" %%i in ('aws ec2 describe-vpcs --filters "Name=is-default,Values=true" --query "Vpcs[0].VpcId" --output text --region af-south-1') do set VPC_ID=%%i
for /f "tokens=*" %%i in ('aws ec2 describe-subnets --filters "Name=vpc-id,Values=%VPC_ID%" --query "Subnets[0].SubnetId" --output text --region af-south-1') do set SUBNET_1=%%i
for /f "tokens=*" %%i in ('aws ec2 describe-subnets --filters "Name=vpc-id,Values=%VPC_ID%" --query "Subnets[1].SubnetId" --output text --region af-south-1') do set SUBNET_2=%%i

REM Create security group
for /f "tokens=*" %%i in ('aws ec2 create-security-group --group-name gatesale-sg --description "GateSale ECS Security Group" --vpc-id %VPC_ID% --query "GroupId" --output text --region af-south-1 2^>nul') do set SG_ID=%%i
if "%SG_ID%"=="" (
    for /f "tokens=*" %%i in ('aws ec2 describe-security-groups --filters "Name=group-name,Values=gatesale-sg" --query "SecurityGroups[0].GroupId" --output text --region af-south-1') do set SG_ID=%%i
)

REM Allow HTTP traffic
aws ec2 authorize-security-group-ingress --group-id %SG_ID% --protocol tcp --port 80 --cidr 0.0.0.0/0 --region af-south-1 2>nul

echo [4/4] Creating ECS service...
aws ecs create-service --cluster gatesale-cluster --service-name gatesale-service --task-definition gatesale-backend --desired-count 1 --launch-type FARGATE --network-configuration "awsvpcConfiguration={subnets=[%SUBNET_1%,%SUBNET_2%],securityGroups=[%SG_ID%],assignPublicIp=ENABLED}" --region af-south-1

echo.
echo ⏳ Waiting for service to start...
timeout /t 60 /nobreak

REM Get public IP
for /f "tokens=*" %%i in ('aws ecs list-tasks --cluster gatesale-cluster --service-name gatesale-service --query "taskArns[0]" --output text --region af-south-1') do set TASK_ARN=%%i
for /f "tokens=*" %%i in ('aws ecs describe-tasks --cluster gatesale-cluster --tasks %TASK_ARN% --query "tasks[0].attachments[0].details[?name=='networkInterfaceId'].value" --output text --region af-south-1') do set ENI_ID=%%i
for /f "tokens=*" %%i in ('aws ec2 describe-network-interfaces --network-interface-ids %ENI_ID% --query "NetworkInterfaces[0].Association.PublicIp" --output text --region af-south-1') do set PUBLIC_IP=%%i

echo.
echo ========================================
echo ✅ Deployment completed!
echo Public IP: %PUBLIC_IP%
echo Test URL: http://%PUBLIC_IP%/
echo ========================================
echo.
echo Note: This is using a sample .NET app for testing.
echo To deploy your actual app, you need Docker or CodeBuild.

pause