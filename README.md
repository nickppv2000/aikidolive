# 🥋 Aikido Live - Interactive Aikido Library System

## Overview

The Aikido Live system is a comprehensive ASP.NET Core 8.0 web application that provides an interactive digital library for Aikido practitioners. The system integrates with Azure Cosmos DB for data storage and Vimeo API for video content management.

## Project Structure

```
Aikido-consolidated/
├── 📁 src/                          # Source code
│   └── AikidoLive/                  # Main web application
│       ├── AikidoLive.csproj
│       ├── Program.cs
│       ├── Pages/                   # Razor Pages
│       ├── Services/                # Business logic services
│       ├── DataModels/              # Data models and DTOs
│       ├── Properties/              # Assembly properties
│       └── wwwroot/                 # Static web assets
├── 📁 tests/                        # Test projects
│   └── AikidoLive.Tests/            # Unit and integration tests
├── 📁 scripts/                      # Deployment and utility scripts
│   ├── deploy.ps1                  # PowerShell deployment script
│   └── deploy.sh                   # Bash deployment script
├── 📁 docs/                         # Documentation
│   ├── README.md                   # Main documentation
│   ├── architecture.md             # System architecture
│   ├── DEPLOYMENT.md               # Deployment instructions
│   └── [other documentation files]
├── 📄 Aikido.sln                   # Main solution file
├── 📄 AikidoLive.sln               # Solution with tests
└── 📄 global.json                  # .NET SDK configuration
```

## Quick Start

### Prerequisites
- .NET 8.0 SDK
- Azure CLI (for deployment)
- Azure Cosmos DB instance
- Vimeo API credentials

### Building the Application

```powershell
# Build main project only
dotnet build Aikido.sln

# Build with tests
dotnet build AikidoLive.sln

# Run the application
dotnet run --project src\AikidoLive\AikidoLive.csproj
```

### Running Tests

```powershell
# Run all tests
dotnet test tests\AikidoLive.Tests\

# Run tests with coverage
dotnet test tests\AikidoLive.Tests\ --collect:"XPlat Code Coverage"
```

### Deployment

```powershell
# Automated deployment to Azure
.\scripts\deploy.ps1

# Build and deploy without restart
.\scripts\deploy.ps1 -SkipRestart

# Deploy existing build
.\scripts\deploy.ps1 -SkipBuild
```

## Technology Stack

- **Framework**: ASP.NET Core 8.0
- **Database**: Azure Cosmos DB
- **Video Platform**: Vimeo API
- **Frontend**: Razor Pages, Bootstrap, jQuery
- **Cloud**: Microsoft Azure App Service
- **Testing**: xUnit, Moq

## Development Workflow

1. **Setup**: Configure Azure Cosmos DB and Vimeo API credentials in `appsettings.json`
2. **Development**: Run `dotnet run --project src\AikidoLive\AikidoLive.csproj` for local development
3. **Testing**: Execute `dotnet test` to run unit tests
4. **Deployment**: Use `.\scripts\deploy.ps1` for automated Azure deployment

## Key Features

- **Library Management**: Browse Aikido training content organized by chapters
- **Playlist System**: Curated collections of training videos and materials
- **Video Integration**: Seamless integration with Vimeo for video content delivery
- **User Management**: Authentication and role-based access control
- **Responsive Design**: Modern web interface optimized for all devices

## Documentation

Comprehensive documentation is available in the `docs/` folder:

- [System Architecture](docs/architecture.md)
- [Deployment Guide](docs/DEPLOYMENT.md)
- [Use Cases](docs/use-cases.md)
- [API Integration](docs/sequences.md)

## Repository Organization

This repository follows the **Simplified Multi-Project Structure** pattern:
- ✅ Clear separation of source code, tests, and scripts
- ✅ Scalable for future additional projects
- ✅ Industry-standard folder organization
- ✅ Simplified development and deployment workflows

## Contributing

1. Clone the repository
2. Create a feature branch
3. Make your changes in the appropriate `src/` folder
4. Add tests in the `tests/` folder
5. Run the full test suite
6. Submit a pull request

## License

© 2024 - Aikido Library. Preserving the art and spirit of Aikido for future generations.
