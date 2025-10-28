# Podcast Web App

**Authors:** Ricardo Burgos & [Partner Name]  
**Course:** COMP306 - API Engineering  
**Lab:** #3 - Podcast Web Application

ASP.NET Core Razor Pages web application for podcast management with dual database architecture. Optimized for Windows development.

## Features

- User registration and authentication with SHA256 password encryption
- Episode management with view tracking and search capabilities
- Comment system with 24-hour modification restriction
- AWS Parameter Store integration for RDS credentials
- Dual database approach: SQL Server (structured) + In-memory storage (unstructured)

## API Endpoints

### Authentication
- `POST /api/auth/register` - Register new user (SHA256 encrypted passwords)
- `POST /api/auth/login` - User login with encrypted password comparison

### Episodes
- `POST /api/episodes` - Add new episode
- `DELETE /api/episodes/{id}` - Delete existing episode
- `PUT /api/episodes/{id}` - Modify existing episode
- `PUT /api/episodes/{id}/view` - Update number of viewers
- `GET /api/episodes/popular` - List most popular episodes (by views)
- `GET /api/episodes/recent` - List episodes by release date (most recent first)
- `GET /api/episodes/search/topic/{topic}` - Search episodes by topic
- `GET /api/episodes/search/host/{host}` - Search episodes by host

### Comments (DynamoDB Simulation)
- `POST /api/comments` - Add comments to specific episode
- `GET /api/comments/episode/{episodeId}` - List all comments for episode
- `PUT /api/comments/{id}` - Modify comment (24-hour restriction, original author only)

## Architecture

- **Structured Data:** SQL Server via Entity Framework Core (Users, Episodes)
- **Unstructured Data:** In-memory storage simulating DynamoDB (Comments)
- **Configuration:** AWS Parameter Store for RDS credentials
- **Frontend Ready:** ASP.NET Core MVC + Razor Pages structure with API controllers for frontend development

## Setup

### Prerequisites (Windows)
1. Visual Studio 2022 or Visual Studio Code
2. .NET 9.0 SDK
3. SQL Server (LocalDB or Express)
4. AWS CLI configured with Parameter Store access

### Configuration
1. Store RDS credentials in AWS Parameter Store:
   ```bash
   aws ssm put-parameter --name "/podcast/db/server" --value "your-server" --type "String"
   aws ssm put-parameter --name "/podcast/db/name" --value "PodcastDB" --type "String"
   ```

2. Run the application (Windows):
   ```cmd
   dotnet run
   ```
   Or open in Visual Studio and press F5

## Database

Entity Framework Core with SQL Server. Database created automatically on first run.

## Deployment

Ready for AWS Elastic Beanstalk deployment with Parameter Store integration.