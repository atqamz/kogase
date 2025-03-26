# PowerShell script for Windows users

# Stop and remove existing containers
docker-compose -f docker-compose.dev.yaml down

# Build and start containers in development mode
docker-compose -f docker-compose.dev.yaml up --build 