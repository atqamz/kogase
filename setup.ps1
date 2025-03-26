# Exit on error
$ErrorActionPreference = "Stop"

Write-Host "Setting up Kogase development environment..." -ForegroundColor Cyan

# Copy environment files if they don't exist
if (-not (Test-Path .env)) {
    Write-Host "Creating .env file..." -ForegroundColor Green
    Copy-Item .env.example .env
}

Write-Host "Building Docker images..." -ForegroundColor Cyan
docker-compose build

Write-Host "Starting Docker containers..." -ForegroundColor Cyan
docker-compose up -d

Write-Host "Waiting for services to initialize..." -ForegroundColor Cyan
Start-Sleep -Seconds 5

Write-Host "============================================" -ForegroundColor Green
Write-Host "Kogase is now running!" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Green
Write-Host "Frontend: http://localhost:3000" -ForegroundColor Yellow
Write-Host "Backend API: http://localhost:8080/api/v1" -ForegroundColor Yellow
Write-Host "API Documentation: http://localhost:8080/swagger/index.html" -ForegroundColor Yellow
Write-Host "============================================" -ForegroundColor Green
Write-Host "Use 'docker-compose logs -f' to view logs" -ForegroundColor Cyan
Write-Host "Use 'make help' to see all available commands" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Green 