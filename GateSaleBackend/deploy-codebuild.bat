@echo off
echo Creating CodeBuild project for Docker-less deployment...

REM Create buildspec.yml for CodeBuild
echo version: 0.2 > buildspec.yml
echo phases: >> buildspec.yml
echo   pre_build: >> buildspec.yml
echo     commands: >> buildspec.yml
echo       - echo Logging in to Amazon ECR... >> buildspec.yml
echo       - aws ecr get-login-password --region af-south-1 ^| docker login --username AWS --password-stdin 357278409567.dkr.ecr.af-south-1.amazonaws.com >> buildspec.yml
echo   build: >> buildspec.yml
echo     commands: >> buildspec.yml
echo       - echo Build started on `date` >> buildspec.yml
echo       - echo Building the Docker image... >> buildspec.yml
echo       - docker build -t gatesale-backend . >> buildspec.yml
echo       - docker tag gatesale-backend:latest 357278409567.dkr.ecr.af-south-1.amazonaws.com/gatesale-backend:latest >> buildspec.yml
echo   post_build: >> buildspec.yml
echo     commands: >> buildspec.yml
echo       - echo Build completed on `date` >> buildspec.yml
echo       - echo Pushing the Docker image... >> buildspec.yml
echo       - docker push 357278409567.dkr.ecr.af-south-1.amazonaws.com/gatesale-backend:latest >> buildspec.yml

REM Create ECR repository
echo Creating ECR repository...
aws ecr create-repository --repository-name gatesale-backend --region af-south-1

REM Create S3 bucket for source code
echo Creating S3 bucket for source code...
aws s3 mb s3://gatesale-codebuild-source-357278409567 --region af-south-1

REM Zip source code
echo Zipping source code...
powershell -Command "Compress-Archive -Path * -DestinationPath source.zip -Force"

REM Upload to S3
echo Uploading source to S3...
aws s3 cp source.zip s3://gatesale-codebuild-source-357278409567/ --region af-south-1

REM Create CodeBuild project
echo Creating CodeBuild project...
aws codebuild create-project --name gatesale-backend-build --source type=S3,location=gatesale-codebuild-source-357278409567/source.zip --artifacts type=NO_ARTIFACTS --environment type=LINUX_CONTAINER,image=aws/codebuild/amazonlinux2-x86_64-standard:3.0,computeType=BUILD_GENERAL1_MEDIUM,privilegedMode=true --service-role arn:aws:iam::357278409567:role/service-role/codebuild-service-role --region af-south-1

REM Start build
echo Starting CodeBuild...
aws codebuild start-build --project-name gatesale-backend-build --region af-south-1

echo Build started! Check AWS Console for progress.
echo After build completes, run: create-ecs-service.bat
pause