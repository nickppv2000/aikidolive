# Sequence Diagrams - Aikido Live System

This document describes the system interactions and message flows using SysML sequence diagrams with PlantUML notation.

## Sequence Overview

The sequence diagrams show the temporal order of interactions between system components, external services, and users for key scenarios in the Aikido Live system.

## SQ-001: Library Content Access Sequence

![Library Content Access Sequence](images/sq-library-access.png)

**Description**: This sequence shows the complete interaction flow when a user accesses library content.

**Participants**:
- User (Browser)
- LibraryView Page
- DBServiceConnector
- Azure Cosmos DB
- VimeoAPI

**Scenario**: User navigates to library view and selects content

**Message Flow**:
1. User requests `/LibraryView` page
2. LibraryView calls `OnGetAsync()`
3. DBServiceConnector queries Cosmos DB for library documents
4. Cosmos DB returns library data
5. System processes chapter URLs for Vimeo content
6. VimeoAPI constructs player URLs
7. LibraryView renders page with embedded videos
8. Browser displays content to user

**Timing Constraints**:
- Database query should complete within 2 seconds
- Page rendering should complete within 3 seconds
- Video embedding should be non-blocking

## SQ-002: Playlist Management Sequence

![Playlist Management Sequence](images/sq-playlist-management.png)

**Description**: This sequence shows the interaction flow for playlist management operations.

**Participants**:
- User (Browser)
- Playlists Page
- DBServiceConnector
- Azure Cosmos DB
- JavaScript Client

**Scenario**: User selects and plays playlist content

**Message Flow**:
1. User requests `/Playlists` page
2. Playlists page calls `OnGetAsync()`
3. DBServiceConnector retrieves playlist documents
4. Cosmos DB returns playlist data
5. System processes track URLs
6. Page renders with playlist dropdown
7. User selects playlist via JavaScript
8. Client-side code displays tracks
9. User selects track
10. Video player loads content

## SQ-003: System Initialization Sequence

![System Initialization Sequence](images/sq-system-initialization.png)

**Description**: This sequence shows the system startup and service initialization process.

**Participants**:
- Application Host
- Program.cs
- Configuration System
- Dependency Injection Container
- Cosmos Client
- DBServiceConnector

**Scenario**: Application startup and service configuration

**Message Flow**:
1. Host starts application
2. Program.cs loads configuration
3. Services are registered with DI container
4. Cosmos Client is configured
5. DBServiceConnector is initialized
6. Database connection is tested
7. Web server starts listening
8. Application is ready for requests

## SQ-004: Vimeo API Integration Sequence

![Vimeo API Integration Sequence](images/sq-vimeo-integration.png)

**Description**: This sequence shows the integration with Vimeo API for video content management.

**Participants**:
- System
- VimeoAPI Service
- Vimeo Authorization Server
- Vimeo API Endpoint
- Configuration

**Scenario**: System authenticates and retrieves video information

**Message Flow**:
1. System initializes VimeoAPI service
2. VimeoAPI reads configuration
3. System requests video folders
4. VimeoAPI checks authentication status
5. If needed, VimeoAPI requests authorization
6. Authorization server validates credentials
7. Access token is returned
8. VimeoAPI makes authenticated request
9. Vimeo API returns video metadata
10. System processes and stores data

## SQ-005: Error Handling Sequence

![Error Handling Sequence](images/sq-error-handling.png)

**Description**: This sequence shows how the system handles errors and exceptions.

**Participants**:
- User
- Web Application
- Error Handler
- Logger
- Error Page

**Scenario**: System encounters an error during operation

**Message Flow**:
1. User makes request
2. Application processes request
3. Exception occurs
4. Error handler catches exception
5. Logger records error details
6. Error page is rendered
7. User sees friendly error message
8. Admin is notified (if critical)

## Interaction Patterns

### Synchronous vs Asynchronous
- Database operations use async/await pattern
- Video loading is asynchronous and non-blocking
- Error handling is synchronous for immediate response

### Message Types
- **Request/Response**: Standard HTTP request-response pattern
- **Callback**: JavaScript event handlers for user interactions
- **Event**: System events for logging and monitoring

### Timing Constraints
- Page load: < 3 seconds
- Database queries: < 2 seconds
- API calls: < 5 seconds with timeout
- Video start: < 5 seconds

### Error Recovery
- Automatic retry for transient failures
- Graceful degradation for external service outages
- User-friendly error messages
- Comprehensive logging for debugging

## Quality Attributes

### Performance
- Asynchronous operations prevent blocking
- Caching reduces database load
- Lazy loading improves initial page load

### Reliability
- Exception handling at all levels
- Timeout mechanisms prevent hanging
- Fallback mechanisms for service failures

### Security
- Secure API key storage
- Input validation and sanitization
- Secure communication protocols

### Maintainability
- Clear separation of concerns
- Consistent error handling patterns
- Comprehensive logging and monitoring
