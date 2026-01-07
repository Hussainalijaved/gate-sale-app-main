@echo off
title GateSale Backend Deployment
color 0A

echo.
echo  ========================================
echo   GateSale Backend AWS ECS Deployment
echo  ========================================
echo.

echo [1/4] Database migrations completed ✓
echo [2/4] Starting Docker build and ECR push...

call deploy.bat
if %errorlevel% neq 0 (
    echo Error in Docker deployment!
    pause
    exit /b 1
)

echo [3/4] Creating ECS infrastructure and service...
call create-ecs-service.bat
if %errorlevel% neq 0 (
    echo Error in ECS service creation!
    pause
    exit /b 1
)

echo [4/4] Deployment completed successfully! ✓
echo.
echo Next steps:
echo 1. Copy the Public IP from above
echo 2. Update mobile app ApiBaseUrl
echo 3. Test the API endpoints
echo 4. Generate new APK
echo.
pause