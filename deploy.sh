#!/bin/bash

# Exit on any error
set -e

echo "Starting .NET 8.0 deployment..."

# Restore packages
echo "Restoring NuGet packages..."
dotnet restore AikidoLive.csproj

# Build the project
echo "Building the project..."
dotnet build AikidoLive.csproj --configuration Release --no-restore

# Publish the project
echo "Publishing the project..."
dotnet publish AikidoLive.csproj --configuration Release --no-build --output $DEPLOYMENT_TARGET

echo "Deployment completed successfully!"
