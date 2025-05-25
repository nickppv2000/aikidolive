# Diagram Index - Aikido Live System

This index provides quick access to all system diagrams organized by category.

## Use Case Diagrams

| Diagram | Description | Source | Image |
|---------|-------------|---------|-------|
| Browse Library | User browsing library content | [uc-browse-library.puml](diagrams/uc-browse-library.puml) | ![UC](images/uc-browse-library.png) |
| Manage Playlists | Playlist management operations | [uc-manage-playlists.puml](diagrams/uc-manage-playlists.puml) | ![UC](images/uc-manage-playlists.png) |
| Video Integration | Vimeo API integration | [uc-video-integration.puml](diagrams/uc-video-integration.puml) | ![UC](images/uc-video-integration.png) |
| User Authentication | User login and access control | [uc-user-authentication.puml](diagrams/uc-user-authentication.puml) | ![UC](images/uc-user-authentication.png) |

## Activity Diagrams

| Diagram | Description | Source | Image |
|---------|-------------|---------|-------|
| Library Browsing | Content browsing workflow | [ac-library-browsing.puml](diagrams/ac-library-browsing.puml) | ![AC](images/ac-library-browsing.png) |
| Playlist Management | Playlist operations workflow | [ac-playlist-management.puml](diagrams/ac-playlist-management.puml) | ![AC](images/ac-playlist-management.png) |
| System Initialization | Application startup process | [ac-system-initialization.puml](diagrams/ac-system-initialization.puml) | ![AC](images/ac-system-initialization.png) |
| Data Synchronization | External data sync process | [ac-data-synchronization.puml](diagrams/ac-data-synchronization.puml) | ![AC](images/ac-data-synchronization.png) |

## Sequence Diagrams

| Diagram | Description | Source | Image |
|---------|-------------|---------|-------|
| Library Access | Library content access flow | [sq-library-access.puml](diagrams/sq-library-access.puml) | ![SQ](images/sq-library-access.png) |
| Playlist Management | Playlist interaction sequence | [sq-playlist-management.puml](diagrams/sq-playlist-management.puml) | ![SQ](images/sq-playlist-management.png) |
| System Initialization | System startup sequence | [sq-system-initialization.puml](diagrams/sq-system-initialization.puml) | ![SQ](images/sq-system-initialization.png) |
| Vimeo Integration | Vimeo API interaction flow | [sq-vimeo-integration.puml](diagrams/sq-vimeo-integration.puml) | ![SQ](images/sq-vimeo-integration.png) |
| Error Handling | Error management sequence | [sq-error-handling.puml](diagrams/sq-error-handling.puml) | ![SQ](images/sq-error-handling.png) |

## Architecture Diagrams

| Diagram | Description | Source | Image |
|---------|-------------|---------|-------|
| System Overview | High-level architecture | [arch-system-overview.puml](diagrams/arch-system-overview.puml) | ![ARCH](images/arch-system-overview.png) |
| Component Diagram | Component relationships | [arch-component-diagram.puml](diagrams/arch-component-diagram.puml) | ![ARCH](images/arch-component-diagram.png) |
| Deployment Diagram | System deployment view | [arch-deployment-diagram.puml](diagrams/arch-deployment-diagram.puml) | ![ARCH](images/arch-deployment-diagram.png) |
| Data Model | Database structure | [arch-data-model.puml](diagrams/arch-data-model.puml) | ![ARCH](images/arch-data-model.png) |
| Security Architecture | Security layers and controls | [arch-security-diagram.puml](diagrams/arch-security-diagram.puml) | ![ARCH](images/arch-security-diagram.png) |

## Diagram Generation

All diagrams are generated using PlantUML from source files in the `diagrams/` directory. To regenerate the images:

```bash
java -jar plantuml.jar -tpng -o images diagrams/*.puml
```

## SysML Notation

The diagrams follow SysML (Systems Modeling Language) conventions where applicable:
- **Use Case Diagrams**: Standard UML use case notation
- **Activity Diagrams**: UML activity notation with decision points and parallel flows
- **Sequence Diagrams**: UML sequence notation with lifelines and messages
- **Architecture Diagrams**: Component and deployment diagrams with SysML extensions

## Viewing Diagrams

- **In Documentation**: Diagrams are embedded in markdown files
- **Standalone**: PNG files can be viewed directly
- **Source**: PlantUML files can be edited and regenerated
- **Online**: Use PlantUML online editor for modifications

---

**Total Diagrams**: 18  
**Categories**: 4 (Use Cases, Activities, Sequences, Architecture)  
**Generated**: May 25, 2025
