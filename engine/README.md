# Kogase Engine

Kogase is a game telemetry and analytics platform for collecting and analyzing game data.

## Prerequisites

- [Docker](https://www.docker.com/products/docker-desktop/)
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) (if developing locally)

## Running with Docker

The easiest way to run the Kogase Engine is using Docker Compose:

```bash
docker-compose up
```

This will start both the API server and a PostgreSQL database. The API will be available at:

- http://localhost:8080 (HTTP)
- https://localhost:8443 (HTTPS)

## Development

### Running locally

1. Install the .NET 9 SDK
2. Make sure you have PostgreSQL running (or use the Docker Compose file with just the database)
3. Run the application:

```bash
dotnet run --project Kogase.Engine/Kogase.Engine.csproj
```

### Building Docker images manually

```bash
docker build -t kogase-engine .
```

## Project Structure

- `Kogase.Engine/` - Main ASP.NET Core Web API project
  - `Controllers/` - API endpoint controllers
  - `Models/` - Database and domain models
  - `Data/` - Database contexts and repositories
  - `Services/` - Business logic
  - `Middleware/` - Custom HTTP pipeline components 