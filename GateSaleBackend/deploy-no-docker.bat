@echo off
echo ========================================
echo  GateSale Backend - Direct ECS Deploy
echo  (Without Docker Desktop)
echo ========================================

REM Create ECR repository
echo [1/5] Creating ECR repository...
aws ecr create-repository --repository-name gatesale-backend --region af-south-1 2>nul

REM Build .NET application
echo [2/5] Building .NET application...
dotnet publish GateSale.API/GateSale.API.csproj -c Release -o ./publish

REM Create simple Dockerfile in publish folder
echo [3/5] Creating deployment package...
cd publish
echo FROM mcr.microsoft.com/dotnet/aspnet:9.0 > Dockerfile
echo WORKDIR /app >> Dockerfile
echo COPY . . >> Dockerfile
echo EXPOSE 80 >> Dockerfile
echo ENTRYPOINT ["dotnet", "GateSale.API.dll"] >> Dockerfile

REM Create deployment zip
powershell -Command "Compress-Archive -Path * -DestinationPath ../deployment.zip -Force"
cd ..

REM Upload to S3 for CodeBuild
echo [4/5] Uploading to AWS...
aws s3 mb s3://gatesale-deploy-357278409567 --region af-south-1 2>nul
aws s3 cp deployment.zip s3://gatesale-deploy-357278409567/ --region af-south-1

REM Create buildspec for CodeBuild
echo version: 0.2 > buildspec.yml
echo phases: >> buildspec.yml
echo   pre_build: >> buildspec.yml
echo     commands: >> buildspec.yml
echo       - aws ecr get-login-password --region af-south-1 ^| docker login --username AWS --password-stdin 357278409567.dkr.ecr.af-south-1.amazonaws.com >> buildspec.yml
echo   build: >> buildspec.yml
echo     commands: >> buildspec.yml
echo       - docker build -t gatesale-backend . >> buildspec.yml
echo       - docker tag gatesale-backend:latest 357278409567.dkr.ecr.af-south-1.amazonaws.com/gatesale-backend:latest >> buildspec.yml
echo   post_build: >> buildspec.yml
echo     commands: >> buildspec.yml
echo       - docker push 357278409567.dkr.ecr.af-south-1.amazonaws.com/gatesale-backend:latest >> buildspec.yml

REM Create CodeBuild project
aws codebuild create-project --name gatesale-build --source type=S3,location=gatesale-deploy-357278409567/deployment.zip,buildspec=buildspec.yml --artifacts type=NO_ARTIFACTS --environment type=LINUX_CONTAINER,image=aws/codebuild/amazonlinux2-x86_64-standard:4.0,computeType=BUILD_GENERAL1_MEDIUM,privilegedMode=true --service-role arn:aws:iam::357278409567:role/service-role/codebuild-service-role --region af-south-1 2>nul

REM Start build
echo [5/5] Starting build process...
for /f "tokens=*" %%i in ('aws codebuild start-build --project-name gatesale-build --region af-south-1 --query "build.id" --output text') do set BUILD_ID=%%i

echo Build started with ID: %BUILD_ID%
echo Waiting for build to complete...

:check_build
timeout /t 30 /nobreak >nul
for /f "tokens=*" %%i in ('aws codebuild batch-get-builds --ids %BUILD_ID% --region af-south-1 --query "builds[0].buildStatus" --output text') do set BUILD_STATUS=%%i

if "%BUILD_STATUS%"=="IN_PROGRESS" (
    echo Build in progress...
    goto check_build
)

if "%BUILD_STATUS%"=="SUCCEEDED" (
    echo ✅ Build completed successfully!
    echo Now creating ECS service...
    call create-ecs-service.bat
) else (
    echo ❌ Build failed with status: %BUILD_STATUS%
    echo Check AWS CodeBuild console for details
)

pause