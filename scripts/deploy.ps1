#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Automated deployment script for Aikido Live .NET 8 application to Azure App Service

.DESCRIPTION
    This script automates the deployment process for the Aikido Live application:
    1. Builds the .NET 8 application locally
    2. Creates a clean deployment package
    3. Deploys to Azure App Service using zip deployment
    4. Optionally restarts the service and verifies deployment

.PARAMETER SkipBuild
    Skip the build step and use existing clean-deploy folder

.PARAMETER SkipRestart
    Skip the automatic restart after deployment

.PARAMETER Verbose
    Enable verbose output for troubleshooting

.EXAMPLE
    .\deploy.ps1
    Standard deployment with build, deploy, and restart

.EXAMPLE
    .\deploy.ps1 -SkipBuild
    Deploy using existing build artifacts

.EXAMPLE
    .\deploy.ps1 -SkipRestart -Verbose
    Deploy with verbose output but skip restart
#>

[CmdletBinding()]
param(
    [switch]$SkipBuild,
    [switch]$SkipRestart,
    [switch]$Help
)

# Configuration
$ResourceGroupName = "aikidolibraryrsrcgrp"
$AppServiceName = "aikidolibrary"
$ProjectFile = "src\AikidoLive\AikidoLive.csproj"
$BuildOutput = "./clean-deploy"
$DeploymentPackage = "./clean-app-deploy.zip"
$AppUrl = "https://aikidolibrary.azurewebsites.net"

# Colors for output
$Green = "`e[32m"
$Yellow = "`e[33m"
$Red = "`e[31m"
$Blue = "`e[34m"
$Reset = "`e[0m"

function Write-Status {
    param([string]$Message, [string]$Color = $Blue)
    Write-Host "$Color🚀 $Message$Reset"
}

function Write-Success {
    param([string]$Message)
    Write-Host "$Green✅ $Message$Reset"
}

function Write-Warning {
    param([string]$Message)
    Write-Host "$Yellow⚠️  $Message$Reset"
}

function Write-Error {
    param([string]$Message)
    Write-Host "$Red❌ $Message$Reset"
}

function Show-Help {
    Get-Help $PSCommandPath -Detailed
    exit 0
}

function Test-Prerequisites {
    Write-Status "Checking prerequisites..."
    
    # Check if dotnet is available
    if (!(Get-Command "dotnet" -ErrorAction SilentlyContinue)) {
        Write-Error ".NET SDK not found. Please install .NET 8 SDK."
        exit 1
    }
    
    # Check if Azure CLI is available
    if (!(Get-Command "az" -ErrorAction SilentlyContinue)) {
        Write-Error "Azure CLI not found. Please install Azure CLI."
        exit 1
    }
    
    # Check if project file exists
    if (!(Test-Path $ProjectFile)) {
        Write-Error "Project file '$ProjectFile' not found. Are you in the correct directory?"
        exit 1
    }
    
    # Check Azure CLI login status
    try {
        $account = az account show 2>$null | ConvertFrom-Json
        if (!$account) {
            Write-Error "Not logged into Azure CLI. Please run 'az login'."
            exit 1
        }
        Write-Success "Prerequisites check passed. Logged in as: $($account.user.name)"
    }
    catch {
        Write-Error "Azure CLI not logged in. Please run 'az login'."
        exit 1
    }
}

function Build-Application {
    Write-Status "Building .NET 8 application..."
    
    # Clean previous build
    if (Test-Path $BuildOutput) {
        Remove-Item $BuildOutput -Recurse -Force
        Write-Host "  • Cleaned previous build output"
    }
    
    # Build the application
    $buildCmd = "dotnet publish $ProjectFile -c Release -o $BuildOutput --self-contained false --runtime linux-x64"
    Write-Host "  • Running: $buildCmd"
    
    try {
        Invoke-Expression $buildCmd
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Build failed with exit code $LASTEXITCODE"
            exit 1
        }
        Write-Success "Application built successfully"
    }
    catch {
        Write-Error "Build failed: $($_.Exception.Message)"
        exit 1
    }
}

function Create-DeploymentPackage {
    Write-Status "Creating deployment package..."
    
    if (!(Test-Path $BuildOutput)) {
        Write-Error "Build output directory '$BuildOutput' not found. Run with build step first."
        exit 1
    }
    
    # Remove existing package
    if (Test-Path $DeploymentPackage) {
        Remove-Item $DeploymentPackage -Force
        Write-Host "  • Removed existing deployment package"
    }
    
    try {
        # Create zip package
        Compress-Archive -Path "$BuildOutput/*" -DestinationPath $DeploymentPackage -Force
        $packageSize = (Get-Item $DeploymentPackage).Length / 1MB
        Write-Success "Deployment package created: $DeploymentPackage ($([math]::Round($packageSize, 2)) MB)"
    }
    catch {
        Write-Error "Failed to create deployment package: $($_.Exception.Message)"
        exit 1
    }
}

function Deploy-ToAzure {
    Write-Status "Deploying to Azure App Service..."
    
    if (!(Test-Path $DeploymentPackage)) {
        Write-Error "Deployment package '$DeploymentPackage' not found."
        exit 1
    }
    
    try {
        $deployCmd = "az webapp deploy --resource-group $ResourceGroupName --name $AppServiceName --src-path `"$DeploymentPackage`" --type zip"
        Write-Host "  • Running: $deployCmd"
        
        Invoke-Expression $deployCmd
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Deployment failed with exit code $LASTEXITCODE"
            exit 1
        }
        Write-Success "Deployment completed successfully"
    }
    catch {
        Write-Error "Deployment failed: $($_.Exception.Message)"
        exit 1
    }
}

function Restart-AppService {
    Write-Status "Restarting App Service..."
    
    try {
        az webapp restart --name $AppServiceName --resource-group $ResourceGroupName --output none
        if ($LASTEXITCODE -ne 0) {
            Write-Warning "App Service restart failed, but deployment may still be successful"
        } else {
            Write-Success "App Service restarted successfully"
        }
    }
    catch {
        Write-Warning "App Service restart failed: $($_.Exception.Message)"
    }
}

function Test-Deployment {
    Write-Status "Verifying deployment..."
    
    Write-Host "  • Waiting for application to start..."
    Start-Sleep -Seconds 15
    
    try {
        $response = Invoke-WebRequest -Uri $AppUrl -Method Head -TimeoutSec 30 -ErrorAction Stop
        if ($response.StatusCode -eq 200) {
            Write-Success "Application is responding successfully"
            Write-Host "$Green🌐 Application URL: $AppUrl$Reset"
        } else {
            Write-Warning "Application responded with status code: $($response.StatusCode)"
        }
    }
    catch {
        Write-Warning "Could not verify application status. Please check manually: $AppUrl"
        Write-Host "  Error: $($_.Exception.Message)"
    }
}

function Show-Summary {
    Write-Host ""
    Write-Host "$Blue" + "="*60 + "$Reset"
    Write-Host "$Blue🎯 DEPLOYMENT SUMMARY$Reset"
    Write-Host "$Blue" + "="*60 + "$Reset"
    Write-Host "• Resource Group: $ResourceGroupName"
    Write-Host "• App Service: $AppServiceName"
    Write-Host "• Application URL: $AppUrl"
    Write-Host "• Deployment Package: $DeploymentPackage"
    Write-Host ""
    Write-Success "Aikido Live .NET 8 deployment completed! 🥋"
    Write-Host "$Blue" + "="*60 + "$Reset"
}

# Main execution
function Main {
    if ($Help) {
        Show-Help
    }
    
    Write-Host "$Blue" + "="*60 + "$Reset"
    Write-Host "$Blue🥋 AIKIDO LIVE DEPLOYMENT SCRIPT$Reset"
    Write-Host "$Blue" + "="*60 + "$Reset"
    Write-Host ""
    
    try {
        # Step 1: Prerequisites
        Test-Prerequisites
        
        # Step 2: Build (unless skipped)
        if (!$SkipBuild) {
            Build-Application
        } else {
            Write-Warning "Skipping build step - using existing build artifacts"
        }
        
        # Step 3: Package
        Create-DeploymentPackage
        
        # Step 4: Deploy
        Deploy-ToAzure
        
        # Step 5: Restart (unless skipped)
        if (!$SkipRestart) {
            Restart-AppService
        } else {
            Write-Warning "Skipping restart step"
        }
        
        # Step 6: Verify
        Test-Deployment
        
        # Step 7: Summary
        Show-Summary
        
    }
    catch {
        Write-Error "Deployment failed: $($_.Exception.Message)"
        exit 1
    }
}

# Execute main function
Main
