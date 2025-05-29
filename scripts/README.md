# Deployment Scripts

This directory contains deployment scripts for the Aikido Live application.

## Scripts

### `deploy.sh`

Bash deployment script for Linux/macOS/WSL environments.

**Usage:**
```bash
# Run from project root
./scripts/deploy.sh [OPTIONS]

# Options
--skip-build      Skip the build step and use existing clean-deploy folder
--skip-restart    Skip the automatic restart after deployment
--help, -h        Show help message
```

**Examples:**
```bash
# Standard deployment
./scripts/deploy.sh

# Deploy existing artifacts
./scripts/deploy.sh --skip-build

# Deploy without restart
./scripts/deploy.sh --skip-restart
```

**Prerequisites:**
- .NET 8 SDK
- Azure CLI (logged in with `az login`)
- Must be run from the project root directory

**Features:**
- ✅ Automated build and packaging
- ✅ Error handling and validation
- ✅ Colored output with progress indicators
- ✅ Deployment verification
- ✅ Optional restart and testing

This script provides the same functionality as the PowerShell `deploy.ps1` script but for Unix-like environments.