@echo off
echo Creating required IAM roles...

REM Create trust policy for CodeBuild
echo { > codebuild-trust-policy.json
echo   "Version": "2012-10-17", >> codebuild-trust-policy.json
echo   "Statement": [ >> codebuild-trust-policy.json
echo     { >> codebuild-trust-policy.json
echo       "Effect": "Allow", >> codebuild-trust-policy.json
echo       "Principal": { >> codebuild-trust-policy.json
echo         "Service": "codebuild.amazonaws.com" >> codebuild-trust-policy.json
echo       }, >> codebuild-trust-policy.json
echo       "Action": "sts:AssumeRole" >> codebuild-trust-policy.json
echo     } >> codebuild-trust-policy.json
echo   ] >> codebuild-trust-policy.json
echo } >> codebuild-trust-policy.json

REM Create CodeBuild service role
aws iam create-role --role-name codebuild-service-role --assume-role-policy-document file://codebuild-trust-policy.json --region af-south-1

REM Attach policies
aws iam attach-role-policy --role-name codebuild-service-role --policy-arn arn:aws:iam::aws:policy/CloudWatchLogsFullAccess --region af-south-1
aws iam attach-role-policy --role-name codebuild-service-role --policy-arn arn:aws:iam::aws:policy/AmazonEC2ContainerRegistryPowerUser --region af-south-1
aws iam attach-role-policy --role-name codebuild-service-role --policy-arn arn:aws:iam::aws:policy/AmazonS3FullAccess --region af-south-1

REM Create ECS task execution role trust policy
echo { > ecs-trust-policy.json
echo   "Version": "2012-10-17", >> ecs-trust-policy.json
echo   "Statement": [ >> ecs-trust-policy.json
echo     { >> ecs-trust-policy.json
echo       "Effect": "Allow", >> ecs-trust-policy.json
echo       "Principal": { >> ecs-trust-policy.json
echo         "Service": "ecs-tasks.amazonaws.com" >> ecs-trust-policy.json
echo       }, >> ecs-trust-policy.json
echo       "Action": "sts:AssumeRole" >> ecs-trust-policy.json
echo     } >> ecs-trust-policy.json
echo   ] >> ecs-trust-policy.json
echo } >> ecs-trust-policy.json

REM Create ECS task execution role
aws iam create-role --role-name ecsTaskExecutionRole --assume-role-policy-document file://ecs-trust-policy.json --region af-south-1
aws iam attach-role-policy --role-name ecsTaskExecutionRole --policy-arn arn:aws:iam::aws:policy/service-role/AmazonECSTaskExecutionRolePolicy --region af-south-1

REM Create ECS task role
aws iam create-role --role-name ecsTaskRole --assume-role-policy-document file://ecs-trust-policy.json --region af-south-1

echo ✅ IAM roles created successfully!
echo Now run: deploy-no-docker.bat
pause