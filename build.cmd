@echo off
echo Starting .NET 8 build process...
dotnet restore
dotnet publish --configuration Release --output %DEPLOYMENT_TARGET%
