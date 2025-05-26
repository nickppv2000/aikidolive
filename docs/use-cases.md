# Use Cases - Aikido Live System

This document describes the main use cases for the Aikido Live Interactive Library System using SysML notation and PlantUML diagrams.

## System Actors

- **Guest User**: Visitor who can browse public content
- **Registered User**: Authenticated user with access to library content
- **Administrator**: System administrator with full access
- **Vimeo System**: External video hosting service
- **Azure Cosmos DB**: Data persistence system

## Primary Use Cases

### UC-001: Browse Library Content

![Browse Library Use Case](images/uc-browse-library.png)

**Description**: Users can browse and view Aikido training content organized in libraries and chapters.

**Primary Actor**: Registered User, Guest User

**Preconditions**: 
- System is running and accessible
- Library content is available in the database

**Main Flow**:
1. User navigates to Library section
2. System displays available libraries
3. User selects a library
4. System displays chapters within the library
5. User selects a chapter
6. System displays chapter content with embedded video

**Alternative Flows**:
- If no content is available, system displays appropriate message
- If video fails to load, system shows error message

### UC-002: Manage Playlists

![Playlist Management Use Case](images/uc-manage-playlists.png)

**Description**: Users can view and interact with curated playlists of Aikido content.

**Primary Actor**: Registered User

**Preconditions**:
- User is authenticated
- Playlists exist in the system

**Main Flow**:
1. User navigates to Playlists section
2. System retrieves playlists from database
3. System displays available playlists
4. User selects a playlist
5. System displays tracks within the playlist
6. User selects a track
7. System displays track content

### UC-003: Video Content Integration

![Video Integration Use Case](images/uc-video-integration.png)

**Description**: System integrates with Vimeo API to provide video content seamlessly.

**Primary Actor**: System

**Secondary Actors**: Vimeo API, User

**Preconditions**:
- Vimeo API credentials are configured
- Video IDs are stored in the database

**Main Flow**:
1. User requests video content
2. System retrieves video metadata from database
3. System constructs Vimeo player URL
4. System embeds video player in page
5. User interacts with video player

### UC-004: User Authentication

![User Authentication Use Case](images/uc-user-authentication.png)

**Description**: System manages user authentication and access control.

**Primary Actor**: User

**Preconditions**:
- User has valid credentials
- User database is accessible

**Main Flow**:
1. User attempts to access protected content
2. System checks authentication status
3. If not authenticated, system redirects to login
4. User provides credentials
5. System validates credentials against database
6. If valid, system grants access

## Use Case Relationships

### Generalization Relationships
- Browse Library Content generalizes to Browse Chapters
- Manage Playlists generalizes to View Playlist Tracks

### Include Relationships
- Browse Library Content includes Video Content Integration
- Manage Playlists includes Video Content Integration
- All protected use cases include User Authentication

### Extend Relationships
- Video Content Integration extends with Vimeo API Authorization
- Browse Library Content extends with Search Functionality (future)

## Non-Functional Requirements

### Performance
- Page load time should be < 3 seconds
- Video content should start playing within 5 seconds

### Security
- All user data must be encrypted in transit
- API keys must be securely stored

### Scalability
- System should support up to 1000 concurrent users
- Database should handle up to 10,000 content items

### Reliability
- System availability should be 99.9%
- Data backup and recovery procedures must be in place
