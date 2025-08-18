# Smart Baby Backend - VS Code Copilot Instructions

## Project Overview
This is a sophisticated **Smart Baby Backend** system built with **.NET 8.0** that provides AI-powered baby analysis capabilities through RESTful APIs and real-time SignalR connections. The system integrates with a Python-based gRPC AI service to analyze baby images, audio, video, and perform multimodal analysis for emotion detection, cry analysis, and behavioral assessment.

## Architecture & Design Patterns

### Clean Architecture Implementation
The project follows **Clean Architecture** principles with clear separation of concerns:

1. **SmartBaby.Core** - Domain Layer
   - Entities: `Baby`, `User`, `BabyAnalysis`, `RealtimeAnalysisSession`, etc.
   - Interfaces: `IBabyAnalysisService`, `IAnalysisHistoryService`, `IRealtimeSessionService`
   - DTOs: Data Transfer Objects for API communication
   - Enums: `AlertLevel`, `ModelStatus`, `UpdateType`, etc.

2. **SmartBaby.Application** - Application Layer
   - Services: Business logic implementation
   - Mappings: AutoMapper configurations
   - Validation: FluentValidation rules

3. **SmartBaby.Infrastructure** - Infrastructure Layer
   - Data: Entity Framework Core DbContext
   - Repositories: Generic repository pattern
   - Migrations: Database schema management

4. **SmartBaby.API** - Presentation Layer
   - Controllers: REST API endpoints
   - Hubs: SignalR real-time communication
   - Models: API-specific DTOs
   - Swagger: API documentation

### Key Technologies & Frameworks
- **.NET 8.0** - Latest .NET framework
- **ASP.NET Core** - Web API framework
- **Entity Framework Core** - ORM with PostgreSQL
- **gRPC** - High-performance RPC framework for AI service communication
- **SignalR** - Real-time web functionality
- **AutoMapper** - Object-to-object mapping
- **JWT Bearer Authentication** - Security
- **Swagger/OpenAPI** - API documentation
- **FluentValidation** - Input validation

## AI Integration Architecture

### gRPC Communication
The system communicates with a Python-based AI service through gRPC:

**Proto Definition**: `protos/baby_analyzer.proto`
- Defines service contracts for image, audio, video, and multimodal analysis
- Supports streaming for real-time analysis
- Includes health checks and model status monitoring

**Key Services**:
- `BabyAnalyzerService` - Main gRPC service interface
- Methods: `AnalyzeImage`, `AnalyzeAudio`, `AnalyzeVideo`, `AnalyzeMultimodal`
- Streaming: `StartRealtimeAnalysis`, `AnalyzeVideoStream`

### Analysis Types

1. **Image Analysis**
   - Emotion detection from baby photos
   - Mood categorization (happy, sad, distressed, calm)
   - Confidence scoring
   - Debug information support

2. **Audio Analysis**
   - Cry detection and classification
   - Cry reason identification (hunger, discomfort, tiredness)
   - Audio feature extraction (MFCC, spectral features)
   - Multiple audio format support

3. **Video Analysis**
   - Combined audio-visual analysis
   - Frame-by-frame emotion detection
   - Audio segment cry analysis
   - Temporal pattern recognition

4. **Multimodal Analysis**
   - Fusion of image and audio analysis
   - Enhanced accuracy through data fusion
   - Alert level determination
   - Recommendation generation

5. **Real-time Analysis**
   - Live video/audio stream processing
   - Real-time updates via SignalR
   - Session management
   - Performance monitoring

## Core Business Logic

### Baby Management
- **Baby Entity**: Core domain object representing a baby
- **User Association**: Each baby belongs to a user
- **Activity Tracking**: Sleep periods, feeding, crying episodes, notes
- **Analysis History**: Complete audit trail of all analyses

### Analysis Workflow
1. **Request Processing**: API receives analysis request (image/audio/video)
2. **Validation**: File type, size, and user permissions
3. **gRPC Call**: Forward to AI service with appropriate parameters
4. **Response Processing**: Map gRPC response to DTOs
5. **Persistence**: Save analysis results to database
6. **Alert Generation**: Create alerts based on analysis results
7. **Real-time Updates**: Broadcast updates via SignalR

### Data Persistence
- **BabyAnalysis**: Stores analysis results with JSON data
- **RealtimeAnalysisSession**: Manages real-time analysis sessions
- **AnalysisAlert**: Stores generated alerts and notifications
- **AnalysisTag**: Categorization and search functionality

## API Endpoints Structure

### Authentication
- JWT Bearer token authentication
- Role-based access control
- User ownership validation for baby data

### Main Controllers

1. **BabyAnalysisController** (`/api/babyanalysis`)
   - `POST /image` - Single image analysis
   - `POST /image/upload` - Image file upload analysis
   - `POST /audio` - Audio analysis
   - `POST /audio/upload` - Audio file upload analysis
   - `POST /video` - Video analysis
   - `POST /video/upload` - Video file upload analysis
   - `POST /multimodal` - Combined analysis
   - `POST /multimodal/upload` - Multimodal file upload
   - `GET /history/{babyId}` - Analysis history
   - `GET /health` - AI service health check

2. **BabyController** (`/api/baby`)
   - CRUD operations for baby management
   - Activity tracking (sleep, feeding, crying)

3. **AuthController** (`/api/auth`)
   - User authentication and registration
   - JWT token management

### SignalR Hub
**BabyAnalysisHub** (`/hubs/babyanalysis`)
- Real-time analysis updates
- Session management
- Group-based messaging
- Connection lifecycle management

## File Upload Handling
- **Size Limits**: 10MB (images), 50MB (audio), 200MB (video)
- **Format Support**: JPEG, PNG, BMP (images), WAV, MP3 (audio), MP4, AVI (video)
- **Streaming**: Large file handling with chunked uploads
- **Validation**: Content type and size validation

## Real-time Features

### SignalR Integration
- **Connection Management**: User authentication and session tracking
- **Group Messaging**: Session-based communication
- **Event Types**: Analysis updates, progress notifications, errors
- **Scalability**: Support for multiple concurrent sessions

### Session Management
- **Session Creation**: Database-tracked analysis sessions
- **Status Tracking**: Created, Starting, Active, Paused, Stopped, Error
- **Update Streaming**: Real-time analysis result broadcasting
- **Cleanup**: Automatic session cleanup and resource management

## Database Schema

### Core Entities
- `Users` - ASP.NET Identity users
- `Babies` - Baby profiles with user associations
- `BabyAnalysis` - Analysis results with JSON data storage
- `RealtimeAnalysisSession` - Real-time session management
- `RealtimeAnalysisUpdate` - Individual session updates
- `AnalysisAlert` - Generated alerts and notifications
- `AnalysisTag` - Categorization and metadata

### JSON Storage
- **PostgreSQL JSONB**: Efficient JSON storage for analysis results
- **Flexible Schema**: Accommodate different analysis result formats
- **Queryable**: Support for JSON queries and indexing

## Error Handling & Logging

### Exception Management
- **Global Exception Handler**: Centralized error processing
- **Structured Logging**: Comprehensive logging with context
- **Error Responses**: Consistent error format across APIs
- **Validation Errors**: Clear validation error messages

### Health Monitoring
- **Health Checks**: AI service connectivity monitoring
- **Model Status**: AI model loading and readiness checks
- **Performance Metrics**: Response times and throughput tracking

## Security Considerations

### Authentication & Authorization
- **JWT Tokens**: Secure token-based authentication
- **Role-based Access**: User role management
- **Data Ownership**: Users can only access their own baby data
- **API Security**: Input validation and sanitization

### Data Privacy
- **Personal Data**: Secure handling of baby photos and audio
- **Audit Trail**: Complete analysis history tracking
- **Data Retention**: Configurable data retention policies

## Configuration & Deployment

### Configuration Keys
- `JWT:Secret` - JWT signing key
- `JWT:ValidIssuer` - Token issuer
- `JWT:ValidAudience` - Token audience
- `BabyAnalyzer:ServerAddress` - gRPC AI service endpoint
- `BabyAnalyzer:DisableSslValidation` - SSL validation toggle

### Environment Setup
- **Development**: Local development with SSL bypass
- **Production**: Secure HTTPS with SSL validation
- **Database**: PostgreSQL with connection string configuration

## Code Style & Conventions

### Naming Conventions
- **Controllers**: `{Entity}Controller` pattern
- **Services**: `{Entity}Service` pattern
- **DTOs**: `{Entity}Dto` suffix
- **Entities**: Plain names without suffixes
- **Interfaces**: `I{Name}` prefix

### Async/Await Patterns
- **All I/O Operations**: Async methods with cancellation tokens
- **Streaming**: `IAsyncEnumerable` for data streams
- **Resource Management**: Proper disposal with `using` statements

### Error Handling
- **Try-Catch**: Appropriate exception handling
- **Logging**: Comprehensive error logging
- **User-Friendly Messages**: Clear error responses for API consumers

## Common Development Patterns

### Repository Pattern
- **Generic Repository**: `IRepository<T>` with common CRUD operations
- **Unit of Work**: Transaction management
- **Dependency Injection**: Service registration and lifecycle management

### DTO Mapping
- **AutoMapper**: Automatic object mapping
- **Validation**: FluentValidation for input validation
- **Transformation**: Business logic in mapping profiles

### gRPC Client Management
- **Client Factory**: Managed gRPC client lifecycle
- **Connection Pooling**: Efficient resource utilization
- **Error Handling**: Robust error handling for network issues

## Testing Considerations

### Unit Testing
- **Service Layer**: Business logic testing
- **Repository Layer**: Data access testing
- **Controller Layer**: API endpoint testing

### Integration Testing
- **gRPC Integration**: AI service communication testing
- **Database Integration**: Entity Framework testing
- **SignalR Testing**: Real-time communication testing

## Performance Optimization

### Caching
- **Response Caching**: API response caching
- **Database Caching**: Entity Framework query caching
- **Memory Management**: Efficient resource utilization

### Streaming
- **File Uploads**: Chunked upload handling
- **Real-time Data**: Efficient streaming implementations
- **Memory Usage**: Proper stream disposal

## Monitoring & Observability

### Logging
- **Structured Logging**: JSON-formatted logs
- **Correlation IDs**: Request tracking
- **Performance Metrics**: Response time monitoring

### Health Checks
- **Database Health**: Connection and query health
- **AI Service Health**: gRPC service availability
- **System Resources**: Memory and CPU monitoring

## Development Guidelines

### When Working with This Project:

1. **Always validate user permissions** before accessing baby data
2. **Use cancellation tokens** for all async operations
3. **Implement proper error handling** with structured logging
4. **Follow the established DTO pattern** for API communication
5. **Use the repository pattern** for data access
6. **Implement proper disposal** for resources and streams
7. **Test gRPC integration** thoroughly
8. **Handle file uploads** with proper size and type validation
9. **Use SignalR groups** for targeted real-time updates
10. **Implement proper session management** for real-time features

This project demonstrates modern .NET development practices with AI integration, real-time communication, and robust architecture suitable for production deployment.
