#!/bin/bash

# Automated deployment script for Aikido Live .NET 8 application to Azure App Service
#
# This script automates the deployment process for the Aikido Live application:
# 1. Builds the .NET 8 application locally
# 2. Creates a clean deployment package
# 3. Deploys to Azure App Service using zip deployment
# 4. Optionally restarts the service and verifies deployment
#
# Usage:
#   ./scripts/deploy.sh                    # Standard deployment with build, deploy, and restart
#   ./scripts/deploy.sh --skip-build       # Deploy using existing build artifacts
#   ./scripts/deploy.sh --skip-restart     # Deploy without restart
#   ./scripts/deploy.sh --help             # Show help

set -e  # Exit on any error

# Configuration
RESOURCE_GROUP_NAME="aikidolibraryrsrcgrp"
APP_SERVICE_NAME="aikidolibrary"
PROJECT_FILE="AikidoLive.csproj"
BUILD_OUTPUT="./clean-deploy"
DEPLOYMENT_PACKAGE="./clean-app-deploy.zip"
APP_URL="https://aikidolibrary.azurewebsites.net"

# Colors for output
GREEN='\033[32m'
YELLOW='\033[33m'
RED='\033[31m'
BLUE='\033[34m'
RESET='\033[0m'

# Options
SKIP_BUILD=false
SKIP_RESTART=false
SHOW_HELP=false

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --skip-build)
            SKIP_BUILD=true
            shift
            ;;
        --skip-restart)
            SKIP_RESTART=true
            shift
            ;;
        --help|-h)
            SHOW_HELP=true
            shift
            ;;
        *)
            echo -e "${RED}❌ Unknown parameter: $1${RESET}"
            exit 1
            ;;
    esac
done

# Helper functions
print_status() {
    echo -e "${BLUE}🚀 $1${RESET}"
}

print_success() {
    echo -e "${GREEN}✅ $1${RESET}"
}

print_warning() {
    echo -e "${YELLOW}⚠️  $1${RESET}"
}

print_error() {
    echo -e "${RED}❌ $1${RESET}"
}

show_help() {
    echo "Aikido Live .NET 8 Deployment Script"
    echo ""
    echo "Usage: ./scripts/deploy.sh [OPTIONS]"
    echo ""
    echo "OPTIONS:"
    echo "  --skip-build       Skip the build step and use existing clean-deploy folder"
    echo "  --skip-restart     Skip the automatic restart after deployment"
    echo "  --help, -h         Show this help message"
    echo ""
    echo "Examples:"
    echo "  ./scripts/deploy.sh                    # Standard deployment"
    echo "  ./scripts/deploy.sh --skip-build       # Deploy existing artifacts"
    echo "  ./scripts/deploy.sh --skip-restart     # Deploy without restart"
    echo ""
    exit 0
}

check_prerequisites() {
    print_status "Checking prerequisites..."
    
    # Check if dotnet is available
    if ! command -v dotnet &> /dev/null; then
        print_error ".NET SDK not found. Please install .NET 8 SDK."
        exit 1
    fi
    
    # Check if Azure CLI is available
    if ! command -v az &> /dev/null; then
        print_error "Azure CLI not found. Please install Azure CLI."
        exit 1
    fi
    
    # Check if project file exists
    if [ ! -f "$PROJECT_FILE" ]; then
        print_error "Project file '$PROJECT_FILE' not found. Are you in the correct directory?"
        exit 1
    fi
    
    # Check Azure CLI login status
    if ! az account show &> /dev/null; then
        print_error "Not logged into Azure CLI. Please run 'az login'."
        exit 1
    fi
    
    local account_info=$(az account show --query "user.name" -o tsv 2>/dev/null)
    print_success "Prerequisites check passed. Logged in as: $account_info"
}

build_application() {
    print_status "Building .NET 8 application..."
    
    # Clean previous build
    if [ -d "$BUILD_OUTPUT" ]; then
        rm -rf "$BUILD_OUTPUT"
        echo "  • Cleaned previous build output"
    fi
    
    # Build the application
    local build_cmd="dotnet publish $PROJECT_FILE -c Release -o $BUILD_OUTPUT --self-contained false --runtime linux-x64"
    echo "  • Running: $build_cmd"
    
    if ! $build_cmd; then
        print_error "Build failed"
        exit 1
    fi
    
    print_success "Application built successfully"
}

create_deployment_package() {
    print_status "Creating deployment package..."
    
    if [ ! -d "$BUILD_OUTPUT" ]; then
        print_error "Build output directory '$BUILD_OUTPUT' not found. Run with build step first."
        exit 1
    fi
    
    # Remove existing package
    if [ -f "$DEPLOYMENT_PACKAGE" ]; then
        rm -f "$DEPLOYMENT_PACKAGE"
        echo "  • Removed existing deployment package"
    fi
    
    # Create zip package
    if ! (cd "$BUILD_OUTPUT" && zip -r "../$(basename "$DEPLOYMENT_PACKAGE")" .); then
        print_error "Failed to create deployment package"
        exit 1
    fi
    
    local package_size=$(du -h "$DEPLOYMENT_PACKAGE" | cut -f1)
    print_success "Deployment package created: $DEPLOYMENT_PACKAGE ($package_size)"
}

deploy_to_azure() {
    print_status "Deploying to Azure App Service..."
    
    if [ ! -f "$DEPLOYMENT_PACKAGE" ]; then
        print_error "Deployment package '$DEPLOYMENT_PACKAGE' not found."
        exit 1
    fi
    
    local deploy_cmd="az webapp deploy --resource-group $RESOURCE_GROUP_NAME --name $APP_SERVICE_NAME --src-path \"$DEPLOYMENT_PACKAGE\" --type zip"
    echo "  • Running: $deploy_cmd"
    
    if ! eval $deploy_cmd; then
        print_error "Deployment failed"
        exit 1
    fi
    
    print_success "Deployment completed successfully"
}

restart_app_service() {
    print_status "Restarting App Service..."
    
    if az webapp restart --name "$APP_SERVICE_NAME" --resource-group "$RESOURCE_GROUP_NAME" --output none; then
        print_success "App Service restarted successfully"
    else
        print_warning "App Service restart failed, but deployment may still be successful"
    fi
}

test_deployment() {
    print_status "Verifying deployment..."
    
    echo "  • Waiting for application to start..."
    sleep 15
    
    if curl -s --head --max-time 30 "$APP_URL" > /dev/null; then
        print_success "Application is responding successfully"
        echo -e "${GREEN}🌐 Application URL: $APP_URL${RESET}"
    else
        print_warning "Could not verify application status. Please check manually: $APP_URL"
    fi
}

show_summary() {
    echo ""
    echo -e "${BLUE}============================================================${RESET}"
    echo -e "${BLUE}🎯 DEPLOYMENT SUMMARY${RESET}"
    echo -e "${BLUE}============================================================${RESET}"
    echo "• Resource Group: $RESOURCE_GROUP_NAME"
    echo "• App Service: $APP_SERVICE_NAME"
    echo "• Application URL: $APP_URL"
    echo "• Deployment Package: $DEPLOYMENT_PACKAGE"
    echo ""
    print_success "Aikido Live .NET 8 deployment completed! 🥋"
    echo -e "${BLUE}============================================================${RESET}"
}

# Main execution
main() {
    if [ "$SHOW_HELP" = true ]; then
        show_help
    fi
    
    echo -e "${BLUE}============================================================${RESET}"
    echo -e "${BLUE}🥋 AIKIDO LIVE DEPLOYMENT SCRIPT${RESET}"
    echo -e "${BLUE}============================================================${RESET}"
    echo ""
    
    # Step 1: Prerequisites
    check_prerequisites
    
    # Step 2: Build (unless skipped)
    if [ "$SKIP_BUILD" = false ]; then
        build_application
    else
        print_warning "Skipping build step - using existing build artifacts"
    fi
    
    # Step 3: Package
    create_deployment_package
    
    # Step 4: Deploy
    deploy_to_azure
    
    # Step 5: Restart (unless skipped)
    if [ "$SKIP_RESTART" = false ]; then
        restart_app_service
    else
        print_warning "Skipping restart step"
    fi
    
    # Step 6: Verify
    test_deployment
    
    # Step 7: Summary
    show_summary
}

# Execute main function
main "$@"