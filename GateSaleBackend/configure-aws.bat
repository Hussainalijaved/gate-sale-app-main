@echo off
echo ========================================
echo  AWS Configuration Setup
echo ========================================

echo Please enter your AWS credentials:
echo.

set /p ACCESS_KEY="AWS Access Key ID: "
set /p SECRET_KEY="AWS Secret Access Key: "

echo.
echo Configuring AWS CLI...

aws configure set aws_access_key_id %ACCESS_KEY%
aws configure set aws_secret_access_key %SECRET_KEY%
aws configure set default.region af-south-1
aws configure set default.output json

echo.
echo Testing AWS connection...
aws sts get-caller-identity

if %errorlevel% equ 0 (
    echo ✅ AWS configured successfully!
    echo Now run: deploy-no-docker.bat
) else (
    echo ❌ AWS configuration failed!
    echo Please check your credentials and try again.
)

pause