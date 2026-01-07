# ECS Infrastructure Setup Script
param(
    [string]$Region = "af-south-1",
    [string]$ClusterName = "gatesale-cluster",
    [string]$ServiceName = "gatesale-backend-service",
    [string]$VpcId = "YOUR_VPC_ID",
    [string]$SubnetId1 = "YOUR_SUBNET_ID_1", 
    [string]$SubnetId2 = "YOUR_SUBNET_ID_2"
)

Write-Host "🏗️ Setting up ECS Infrastructure..." -ForegroundColor Green

try {
    # Create ECS Cluster
    Write-Host "📦 Creating ECS Cluster..." -ForegroundColor Yellow
    aws ecs create-cluster --cluster-name $ClusterName --region $Region

    # Create CloudWatch Log Group
    Write-Host "📊 Creating CloudWatch Log Group..." -ForegroundColor Yellow
    aws logs create-log-group --log-group-name "/ecs/gatesale-backend" --region $Region

    # Create Security Group for ALB
    Write-Host "🔒 Creating Security Group for ALB..." -ForegroundColor Yellow
    $albSgId = aws ec2 create-security-group --group-name gatesale-alb-sg --description "Security group for GateSale ALB" --vpc-id $VpcId --region $Region --query 'GroupId' --output text
    
    # Allow HTTP and HTTPS traffic to ALB
    aws ec2 authorize-security-group-ingress --group-id $albSgId --protocol tcp --port 80 --cidr 0.0.0.0/0 --region $Region
    aws ec2 authorize-security-group-ingress --group-id $albSgId --protocol tcp --port 443 --cidr 0.0.0.0/0 --region $Region

    # Create Security Group for ECS Tasks
    Write-Host "🔒 Creating Security Group for ECS Tasks..." -ForegroundColor Yellow
    $ecsSgId = aws ec2 create-security-group --group-name gatesale-ecs-sg --description "Security group for GateSale ECS tasks" --vpc-id $VpcId --region $Region --query 'GroupId' --output text
    
    # Allow traffic from ALB to ECS tasks
    aws ec2 authorize-security-group-ingress --group-id $ecsSgId --protocol tcp --port 80 --source-group $albSgId --region $Region

    # Create Application Load Balancer
    Write-Host "⚖️ Creating Application Load Balancer..." -ForegroundColor Yellow
    $albArn = aws elbv2 create-load-balancer --name gatesale-alb --subnets $SubnetId1 $SubnetId2 --security-groups $albSgId --region $Region --query 'LoadBalancers[0].LoadBalancerArn' --output text

    # Create Target Group
    Write-Host "🎯 Creating Target Group..." -ForegroundColor Yellow
    $tgArn = aws elbv2 create-target-group --name gatesale-tg --protocol HTTP --port 80 --vpc-id $VpcId --target-type ip --health-check-path "/api/test" --region $Region --query 'TargetGroups[0].TargetGroupArn' --output text

    # Create ALB Listener
    Write-Host "👂 Creating ALB Listener..." -ForegroundColor Yellow
    aws elbv2 create-listener --load-balancer-arn $albArn --protocol HTTP --port 80 --default-actions Type=forward,TargetGroupArn=$tgArn --region $Region

    # Create ECS Service
    Write-Host "🚀 Creating ECS Service..." -ForegroundColor Yellow
    $serviceConfig = @"
{
    "serviceName": "$ServiceName",
    "cluster": "$ClusterName",
    "taskDefinition": "gatesale-backend",
    "desiredCount": 1,
    "launchType": "FARGATE",
    "networkConfiguration": {
        "awsvpcConfiguration": {
            "subnets": ["$SubnetId1", "$SubnetId2"],
            "securityGroups": ["$ecsSgId"],
            "assignPublicIp": "ENABLED"
        }
    },
    "loadBalancers": [
        {
            "targetGroupArn": "$tgArn",
            "containerName": "gatesale-api",
            "containerPort": 80
        }
    ]
}
"@
    
    $serviceConfig | Out-File "ecs-service-config.json" -Encoding UTF8
    aws ecs create-service --cli-input-json file://ecs-service-config.json --region $Region

    # Get ALB DNS name
    $albDns = aws elbv2 describe-load-balancers --load-balancer-arns $albArn --region $Region --query 'LoadBalancers[0].DNSName' --output text

    Write-Host "✅ Infrastructure setup completed!" -ForegroundColor Green
    Write-Host "🌐 ALB DNS: $albDns" -ForegroundColor Cyan
    Write-Host "📝 Update your mobile app ApiBaseUrl to: http://$albDns/" -ForegroundColor Yellow

} catch {
    Write-Host "❌ Infrastructure setup failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}