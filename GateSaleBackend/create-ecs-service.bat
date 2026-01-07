@echo off
echo Creating ECS infrastructure...

REM Create ECS cluster
aws ecs create-cluster --cluster-name gatesale-cluster --region af-south-1

REM Create CloudWatch log group
aws logs create-log-group --log-group-name "/ecs/gatesale-backend" --region af-south-1

REM Register task definition
aws ecs register-task-definition --cli-input-json file://ecs-task-definition.json --region af-south-1

REM Get default VPC and subnets
for /f "tokens=*" %%i in ('aws ec2 describe-vpcs --filters "Name=is-default,Values=true" --query "Vpcs[0].VpcId" --output text --region af-south-1') do set VPC_ID=%%i
for /f "tokens=*" %%i in ('aws ec2 describe-subnets --filters "Name=vpc-id,Values=%VPC_ID%" --query "Subnets[0].SubnetId" --output text --region af-south-1') do set SUBNET_1=%%i
for /f "tokens=*" %%i in ('aws ec2 describe-subnets --filters "Name=vpc-id,Values=%VPC_ID%" --query "Subnets[1].SubnetId" --output text --region af-south-1') do set SUBNET_2=%%i

REM Create security group
for /f "tokens=*" %%i in ('aws ec2 create-security-group --group-name gatesale-sg --description "GateSale ECS Security Group" --vpc-id %VPC_ID% --query "GroupId" --output text --region af-south-1') do set SG_ID=%%i

REM Allow HTTP traffic
aws ec2 authorize-security-group-ingress --group-id %SG_ID% --protocol tcp --port 80 --cidr 0.0.0.0/0 --region af-south-1

REM Create ECS service
aws ecs create-service --cluster gatesale-cluster --service-name gatesale-service --task-definition gatesale-backend --desired-count 1 --launch-type FARGATE --network-configuration "awsvpcConfiguration={subnets=[%SUBNET_1%,%SUBNET_2%],securityGroups=[%SG_ID%],assignPublicIp=ENABLED}" --region af-south-1

echo ECS service created successfully!
echo Getting service public IP...
timeout /t 30 /nobreak

REM Get task ARN and public IP
for /f "tokens=*" %%i in ('aws ecs list-tasks --cluster gatesale-cluster --service-name gatesale-service --query "taskArns[0]" --output text --region af-south-1') do set TASK_ARN=%%i
for /f "tokens=*" %%i in ('aws ecs describe-tasks --cluster gatesale-cluster --tasks %TASK_ARN% --query "tasks[0].attachments[0].details[?name=='networkInterfaceId'].value" --output text --region af-south-1') do set ENI_ID=%%i
for /f "tokens=*" %%i in ('aws ec2 describe-network-interfaces --network-interface-ids %ENI_ID% --query "NetworkInterfaces[0].Association.PublicIp" --output text --region af-south-1') do set PUBLIC_IP=%%i

echo.
echo ========================================
echo Backend deployed successfully!
echo Public IP: %PUBLIC_IP%
echo API URL: http://%PUBLIC_IP%/
echo Test URL: http://%PUBLIC_IP%/api/test
echo ========================================
echo.
echo Update your mobile app ApiBaseUrl to: http://%PUBLIC_IP%/

pause