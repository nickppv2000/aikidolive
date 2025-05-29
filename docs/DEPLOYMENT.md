# 🥋 Aikido Live - Deployment Instructions

## Overview

This document provides comprehensive instructions for deploying the Aikido Live .NET 8 web application to Azure App Service. The application has been configured for reliable zip-based deployment to avoid common Oryx build system issues with .NET 8.

## 📋 Prerequisites

Before deploying, ensure you have the following installed and configured:

### Required Software
- **.NET 8 SDK** - Download from [Microsoft .NET](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Azure CLI** - Download from [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
- **PowerShell 7+** (recommended) or Windows PowerShell

### Azure Requirements
- **Azure Subscription** with appropriate permissions
- **Azure App Service** already configured (aikidolibrary.azurewebsites.net)
- **Azure CLI Authentication** - Run `az login` if not already authenticated

### Verify Prerequisites
```powershell
# Check .NET version
dotnet --version

# Check Azure CLI
az --version

# Verify Azure login
az account show
```

## 🏗️ Azure App Service Configuration

The Azure App Service (`aikidolibrary`) is pre-configured with the following settings:

### Runtime Configuration
```
Runtime Stack: .NET 8 (DOTNETCORE|8.0)
Framework Version: v8.0
Startup Command: dotnet AikidoLive.dll
```

### Application Settings
```
SCM_DO_BUILD_DURING_DEPLOYMENT: false
ENABLE_ORYX_BUILD: false
WEBSITE_RUN_FROM_PACKAGE: 1
ORYX_DISABLE_AUTO_DETECT: true
WEBSITES_ENABLE_APP_SERVICE_STORAGE: true
```

> ⚠️ **Important**: Do not modify these settings as they are specifically configured to prevent .NET 8 deployment issues.

## 🚀 Deployment Methods

### Method 1: Automated Deployment Scripts (Recommended)

The easiest way to deploy is using the provided deployment scripts. Choose based on your platform:

#### Option A: PowerShell Script (Windows/Cross-platform)

```powershell
# Navigate to project directory
cd c:\projects\websites\Aikido\aikidolive

# Standard deployment
.\deploy.ps1

# Skip build (use existing artifacts)
.\deploy.ps1 -SkipBuild

# Deploy without restart
.\deploy.ps1 -SkipRestart

# Get help
.\deploy.ps1 -Help
```

#### Option B: Bash Script (Linux/macOS/WSL)

```bash
# Navigate to project directory
cd /path/to/aikidolive

# Standard deployment
./scripts/deploy.sh

# Skip build (use existing artifacts)
./scripts/deploy.sh --skip-build

# Deploy without restart
./scripts/deploy.sh --skip-restart

# Get help
./scripts/deploy.sh --help
```

#### Script Features (Both PowerShell and Bash)
- ✅ Automated build and packaging
- ✅ Error handling and validation
- ✅ Colored output with progress indicators
- ✅ Deployment verification
- ✅ Optional restart and testing
- ✅ Cross-platform compatibility

### Method 2: Manual Deployment

If you prefer manual control over each step:

#### Step 1: Build the Application
```powershell
cd c:\projects\websites\Aikido\aikidolive
dotnet publish AikidoLive.csproj -c Release -o ./clean-deploy --self-contained false --runtime linux-x64
```

#### Step 2: Create Deployment Package
```powershell
# Remove existing package if it exists
if (Test-Path "./clean-app-deploy.zip") { Remove-Item "./clean-app-deploy.zip" -Force }

# Create new deployment package
Compress-Archive -Path "./clean-deploy/*" -DestinationPath "./clean-app-deploy.zip" -Force
```

#### Step 3: Deploy to Azure
```powershell
az webapp deploy --resource-group aikidolibraryrsrcgrp --name aikidolibrary --src-path "./clean-app-deploy.zip" --type zip
```

#### Step 4: Restart App Service (Optional)
```powershell
az webapp restart --name aikidolibrary --resource-group aikidolibraryrsrcgrp
```

### Method 3: VS Code Deployment

Using Visual Studio Code with Azure extensions:

1. **Build the application first**:
   ```powershell
   dotnet publish AikidoLive.csproj -c Release -o ./clean-deploy --self-contained false --runtime linux-x64
   ```

2. **Use VS Code Azure Extension**:
   - Install the "Azure App Service" extension
   - Right-click the `clean-deploy` folder
   - Select "Deploy to Web App..."
   - Choose your subscription and `aikidolibrary` app service

## 🔍 Verification Steps

After deployment, verify the application is working correctly:

### 1. Check Deployment Status
```powershell
# Check app service status
az webapp show --name aikidolibrary --resource-group aikidolibraryrsrcgrp --query "state" --output table
```

### 2. Test Application Response
```powershell
# Test if application is responding
Invoke-WebRequest -Uri "https://aikidolibrary.azurewebsites.net" -Method Head
```

### 3. Access Application
Open your browser and navigate to: **https://aikidolibrary.azurewebsites.net**

### 4. Check Application Logs (if needed)
```powershell
# Stream live logs
az webapp log tail --name aikidolibrary --resource-group aikidolibraryrsrcgrp

# Download logs
az webapp log download --name aikidolibrary --resource-group aikidolibraryrsrcgrp
```

## 🚨 Troubleshooting

### Common Issues and Solutions

#### Issue: Build Fails Locally
**Symptoms**: `dotnet publish` command fails
**Solution**:
```powershell
# Clean and restore packages
dotnet clean
dotnet restore
dotnet build

# Try publish again
dotnet publish AikidoLive.csproj -c Release -o ./clean-deploy --self-contained false --runtime linux-x64
```

#### Issue: Azure CLI Not Authenticated
**Symptoms**: `az webapp deploy` fails with authentication error
**Solution**:
```powershell
# Login to Azure
az login

# Set correct subscription (if multiple)
az account set --subscription "your-subscription-id"
```

#### Issue: Application Not Starting
**Symptoms**: Deployment succeeds but application returns 500 errors
**Solutions**:
1. **Restart the app service**:
   ```powershell
   az webapp restart --name aikidolibrary --resource-group aikidolibraryrsrcgrp
   ```

2. **Check application logs**:
   ```powershell
   az webapp log tail --name aikidolibrary --resource-group aikidolibraryrsrcgrp
   ```

3. **Verify startup command**:
   - Ensure startup command is set to: `dotnet AikidoLive.dll`

#### Issue: Old Version Still Loading
**Symptoms**: Changes not reflected after deployment
**Solutions**:
1. **Clear browser cache** (Ctrl+F5)
2. **Restart app service**
3. **Wait 2-3 minutes** for deployment to fully complete

### Getting Help

If you encounter issues:

1. **Check the logs**:
   ```powershell
   az webapp log tail --name aikidolibrary --resource-group aikidolibraryrsrcgrp
   ```

2. **Verify configuration**:
   ```powershell
   az webapp config show --name aikidolibrary --resource-group aikidolibraryrsrcgrp
   ```

3. **Test local build**:
   ```powershell
   cd ./clean-deploy
   dotnet AikidoLive.dll
   ```

## 📁 Project Structure

### Key Files and Folders
```
aikidolive/
├── AikidoLive.csproj          # Project file (targets net8.0)
├── global.json                # SDK version configuration
├── deploy.ps1                 # Automated deployment script
├── clean-deploy/              # Build output (deployment source)
├── clean-app-deploy.zip       # Deployment package
└── DEPLOYMENT.md              # This file
```

### Important Notes
- **Never deploy the entire project folder** - Always use `clean-deploy` output
- **The `clean-deploy` folder** excludes test files and conflicting dependencies
- **Deployment packages** are created from `clean-deploy` folder contents only

## 🔐 Security Considerations

### Sensitive Information
- **Connection strings** are stored in Azure App Service configuration
- **Application secrets** use Azure Key Vault integration
- **Authentication** handled via Azure AD integration

### Best Practices
- ✅ Never commit secrets to source control
- ✅ Use managed identities when possible
- ✅ Regularly rotate access keys
- ✅ Monitor application logs for security events

## 🎯 Quick Reference

### Essential Commands

#### PowerShell
```powershell
# Full automated deployment
.\deploy.ps1

# Manual build only
dotnet publish AikidoLive.csproj -c Release -o ./clean-deploy --self-contained false --runtime linux-x64

# Manual deploy only
az webapp deploy --resource-group aikidolibraryrsrcgrp --name aikidolibrary --src-path "./clean-app-deploy.zip" --type zip

# Restart app service
az webapp restart --name aikidolibrary --resource-group aikidolibraryrsrcgrp

# Check application
Invoke-WebRequest -Uri "https://aikidolibrary.azurewebsites.net" -Method Head
```

#### Bash/Linux
```bash
# Full automated deployment
./scripts/deploy.sh

# Manual build only
dotnet publish AikidoLive.csproj -c Release -o ./clean-deploy --self-contained false --runtime linux-x64

# Manual deploy only
az webapp deploy --resource-group aikidolibraryrsrcgrp --name aikidolibrary --src-path "./clean-app-deploy.zip" --type zip

# Restart app service
az webapp restart --name aikidolibrary --resource-group aikidolibraryrsrcgrp

# Check application
curl -I "https://aikidolibrary.azurewebsites.net"
```

### URLs and Resources
- **Application URL**: https://aikidolibrary.azurewebsites.net
- **Azure Portal**: https://portal.azure.com
- **Resource Group**: aikidolibraryrsrcgrp
- **App Service**: aikidolibrary

## 📚 Additional Resources

- [.NET 8 Deployment Guide](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/azure-apps/)
- [Azure App Service Documentation](https://docs.microsoft.com/en-us/azure/app-service/)
- [Azure CLI Reference](https://docs.microsoft.com/en-us/cli/azure/webapp)
- [Troubleshooting Azure App Service](https://docs.microsoft.com/en-us/azure/app-service/troubleshoot-dotnet-visual-studio)

---

## 🎉 Success!

Your Aikido Live .NET 8 application is now successfully configured for reliable Azure deployment!

For any questions or issues, refer to the troubleshooting section above or check the application logs in Azure.

**Happy deploying! 🥋**
