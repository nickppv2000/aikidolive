# Aikido Live - Interactive Aikido Library System

## Overview

The Aikido Live system is a comprehensive ASP.NET Core web application that provides an interactive digital library for Aikido practitioners. The system integrates with Azure Cosmos DB for data storage and Vimeo API for video content management, offering users access to Aikido training materials, playlists, and educational content.

## System Architecture

The system follows a layered architecture pattern with clear separation of concerns:

- **Presentation Layer**: Razor Pages for user interface
- **Service Layer**: Business logic and external API integration
- **Data Access Layer**: Azure Cosmos DB integration
- **Data Models**: Domain entities and data transfer objects

## Key Features

- **Library Management**: Browse and access Aikido training content organized by chapters
- **Playlist System**: Curated collections of training videos and materials
- **Video Integration**: Seamless integration with Vimeo for video content delivery
- **User Management**: User authentication and role-based access
- **Responsive Design**: Modern web interface optimized for all devices

## Technology Stack

- **Framework**: ASP.NET Core 7.0
- **Database**: Azure Cosmos DB
- **Video Platform**: Vimeo API
- **Frontend**: Razor Pages, Bootstrap, jQuery
- **Cloud**: Microsoft Azure

## Documentation Structure

- [Use Cases](use-cases.md) - Detailed use case specifications
- [System Activities](activities.md) - Activity flow documentation
- [Sequence Diagrams](sequences.md) - System interaction sequences
- [Architecture Overview](architecture.md) - System architecture documentation
- [Project Summary](project-summary.md) - Comprehensive documentation overview
- [Diagram Index](diagram-index.md) - Quick reference to all diagrams

## Diagrams

All system diagrams are created using PlantUML with SysML notation and are available in the following formats:

- **Source**: [diagrams/](diagrams/) - PlantUML source files
- **Images**: [images/](images/) - Exported PNG diagrams

## Getting Started

1. Configure Azure Cosmos DB connection in `appsettings.json`
2. Set up Vimeo API credentials
3. Run the application using `dotnet run`
4. Access the application at `https://localhost:7000`

## System Requirements

- .NET 7.0 or higher
- Azure Cosmos DB account
- Vimeo API access
- Modern web browser
