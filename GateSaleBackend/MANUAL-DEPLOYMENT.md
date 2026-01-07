## Manual Deployment (No Docker Required)

### Step 1: Setup IAM Roles
```cmd
setup-iam-roles.bat
```

### Step 2: Deploy Application  
```cmd
deploy-no-docker.bat
```

### Step 3: Get Public IP
After deployment completes, the script will show your backend's public IP.

### Step 4: Update Mobile App
Update ApiBaseUrl in MauiProgram.cs:
```csharp
public static readonly string ApiBaseUrl = "http://YOUR_PUBLIC_IP/";
```

### Alternative: Use AWS Console
1. Go to AWS CodeBuild console
2. Check build progress
3. Go to ECS console  
4. Get service public IP
5. Update mobile app

### Test Your API
```
http://YOUR_PUBLIC_IP/api/test
```

This method uses AWS CodeBuild to build Docker image in the cloud, so you don't need Docker Desktop installed locally.