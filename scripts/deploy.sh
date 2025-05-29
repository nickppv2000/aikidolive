#!/bin/bash
#
# Automated deployment script for Aikido Live .NET 8 application to Azure App Service
#
# This script automates the deployment process for the Aikido Live application:
# 1. Builds the .NET 8 application locally
# 2. Creates a clean deployment package
# 3. Deploys to Azure App Service using zip deployment
# 4. Optionally restarts the service and verifies deployment
#
# Usage:
#   ./deploy.sh                    # Standard deployment with build, deploy, and restart
#   ./deploy.sh --skip-build       # Deploy using existing build artifacts
#   ./deploy.sh --skip-restart     # Deploy with no restart
#   ./deploy.sh --help             # Show help information
#

set -e  # Exit on any error

# Configuration
RESOURCE_GROUP_NAME="aikidolibraryrsrcgrp"
APP_SERVICE_NAME="aikidolibrary"
PROJECT_FILE="src/AikidoLive/AikidoLive.csproj"
BUILD_OUTPUT="./clean-deploy"
DEPLOYMENT_PACKAGE="./clean-app-deploy.zip"
APP_URL="https://aikidolibrary.azurewebsites.net"

# Parse command line arguments
SKIP_BUILD=false
SKIP_RESTART=false
SHOW_HELP=false

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
            echo "Unknown option: $1"
            echo "Use --help for usage information"
            exit 1
            ;;
    esac
done

# Colors for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
BLUE='\033[0;34m'
RESET='\033[0m'

# Utility functions
write_status() {
    local message="$1"
    local color="${2:-$BLUE}"
    echo -e "${color}🚀 ${message}${RESET}"
}

write_success() {
    local message="$1"
    echo -e "${GREEN}✅ ${message}${RESET}"
}

write_warning() {
    local message="$1"
    echo -e "${YELLOW}⚠️  ${message}${RESET}"
}

write_error() {
    local message="$1"
    echo -e "${RED}❌ ${message}${RESET}"
}

show_help() {
    cat << EOF
Aikido Live .NET 8 Deployment Script

DESCRIPTION:
    This script automates the deployment process for the Aikido Live application:
    1. Builds the .NET 8 application locally
    2. Creates a clean deployment package
    3. Deploys to Azure App Service using zip deployment
    4. Optionally restarts the service and verifies deployment

USAGE:
    ./deploy.sh [OPTIONS]

OPTIONS:
    --skip-build       Skip the build step and use existing clean-deploy folder
    --skip-restart     Skip the automatic restart after deployment
    --help, -h         Show this help message

EXAMPLES:
    ./deploy.sh                    # Standard deployment with build, deploy, and restart
    ./deploy.sh --skip-build       # Deploy using existing build artifacts
    ./deploy.sh --skip-restart     # Deploy with no restart

REQUIREMENTS:
    - .NET 8 SDK
    - Azure CLI (logged in)
    - zip utility
    - curl (for deployment verification)

EOF
    exit 0
}

test_prerequisites() {
    write_status "Checking prerequisites..."
    
    # Check if dotnet is available
    if ! command -v dotnet &> /dev/null; then
        write_error ".NET SDK not found. Please install .NET 8 SDK."
        exit 1
    fi
    
    # Check if Azure CLI is available
    if ! command -v az &> /dev/null; then
        write_error "Azure CLI not found. Please install Azure CLI."
        exit 1
    fi
    
    # Check if zip is available
    if ! command -v zip &> /dev/null; then
        write_error "zip utility not found. Please install zip."
        exit 1
    fi
    
    # Check if curl is available
    if ! command -v curl &> /dev/null; then
        write_error "curl not found. Please install curl."
        exit 1
    fi
    
    # Check if project file exists
    if [[ ! -f "$PROJECT_FILE" ]]; then
        write_error "Project file '$PROJECT_FILE' not found. Are you in the correct directory?"
        exit 1
    fi
    
    # Check Azure CLI login status
    if ! az account show &> /dev/null; then
        write_error "Not logged into Azure CLI. Please run 'az login'."
        exit 1
    fi
    
    local account_info=$(az account show --query "user.name" -o tsv 2>/dev/null)
    if [[ -z "$account_info" ]]; then
        write_error "Azure CLI not logged in. Please run 'az login'."
        exit 1
    fi
    
    write_success "Prerequisites check passed. Logged in as: $account_info"
}

build_application() {
    write_status "Building .NET 8 application..."
    
    # Clean previous build
    if [[ -d "$BUILD_OUTPUT" ]]; then
        rm -rf "$BUILD_OUTPUT"
        echo "  • Cleaned previous build output"
    fi
    
    # Build the application
    local build_cmd="dotnet publish $PROJECT_FILE -c Release -o $BUILD_OUTPUT --self-contained false --runtime linux-x64"
    echo "  • Running: $build_cmd"
    
    if ! eval "$build_cmd"; then
        write_error "Build failed"
        exit 1
    fi
    
    write_success "Application built successfully"
}

create_deployment_package() {
    write_status "Creating deployment package..."
    
    if [[ ! -d "$BUILD_OUTPUT" ]]; then
        write_error "Build output directory '$BUILD_OUTPUT' not found. Run with build step first."
        exit 1
    fi
    
    # Remove existing package
    if [[ -f "$DEPLOYMENT_PACKAGE" ]]; then
        rm -f "$DEPLOYMENT_PACKAGE"
        echo "  • Removed existing deployment package"
    fi
    
    # Create zip package
    if ! (cd "$BUILD_OUTPUT" && zip -r "../$(basename "$DEPLOYMENT_PACKAGE")" .); then
        write_error "Failed to create deployment package"
        exit 1
    fi
    
    local package_size=$(du -h "$DEPLOYMENT_PACKAGE" | cut -f1)
    write_success "Deployment package created: $DEPLOYMENT_PACKAGE ($package_size)"
}

deploy_to_azure() {
    write_status "Deploying to Azure App Service..."
    
    if [[ ! -f "$DEPLOYMENT_PACKAGE" ]]; then
        write_error "Deployment package '$DEPLOYMENT_PACKAGE' not found."
        exit 1
    fi
    
    local deploy_cmd="az webapp deploy --resource-group $RESOURCE_GROUP_NAME --name $APP_SERVICE_NAME --src-path \"$DEPLOYMENT_PACKAGE\" --type zip"
    echo "  • Running: $deploy_cmd"
    
    if ! eval "$deploy_cmd"; then
        write_error "Deployment failed"
        exit 1
    fi
    
    write_success "Deployment completed successfully"
}

restart_app_service() {
    write_status "Restarting App Service..."
    
    if az webapp restart --name "$APP_SERVICE_NAME" --resource-group "$RESOURCE_GROUP_NAME" --output none; then
        write_success "App Service restarted successfully"
    else
        write_warning "App Service restart failed, but deployment may still be successful"
    fi
}

test_deployment() {
    write_status "Verifying deployment..."
    
    echo "  • Waiting for application to start..."
    sleep 15
    
    local status_code=$(curl -s -o /dev/null -w "%{http_code}" -I "$APP_URL" --max-time 30 || echo "000")
    
    if [[ "$status_code" == "200" ]]; then
        write_success "Application is responding successfully"
        echo -e "${GREEN}🌐 Application URL: $APP_URL${RESET}"
    elif [[ "$status_code" == "000" ]]; then
        write_warning "Could not verify application status. Please check manually: $APP_URL"
    else
        write_warning "Application responded with status code: $status_code"
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
    write_success "Aikido Live .NET 8 deployment completed! 🥋"
    echo -e "${BLUE}============================================================${RESET}"
}

main() {
    if [[ "$SHOW_HELP" == true ]]; then
        show_help
    fi
    
    echo -e "${BLUE}============================================================${RESET}"
    echo -e "${BLUE}🥋 AIKIDO LIVE DEPLOYMENT SCRIPT${RESET}"
    echo -e "${BLUE}============================================================${RESET}"
    echo ""
    
    # Step 1: Prerequisites
    test_prerequisites
    
    # Step 2: Build (unless skipped)
    if [[ "$SKIP_BUILD" == false ]]; then
        build_application
    else
        write_warning "Skipping build step - using existing build artifacts"
    fi
    
    # Step 3: Package
    create_deployment_package
    
    # Step 4: Deploy
    deploy_to_azure
    
    # Step 5: Restart (unless skipped)
    if [[ "$SKIP_RESTART" == false ]]; then
        restart_app_service
    else
        write_warning "Skipping restart step"
    fi
    
    # Step 6: Verify
    test_deployment
    
    # Step 7: Summary
    show_summary
}

# Execute main function
main "$@"