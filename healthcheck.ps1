# Exit on error
$ErrorActionPreference = "Stop"

Write-Host "Checking Kogase container health..." -ForegroundColor Cyan

# Check if Docker is running
try {
    docker info | Out-Null
}
catch {
    Write-Host "Docker is not running or you don't have permission to use it." -ForegroundColor Red
    exit 1
}

# Check if Docker Compose is installed
try {
    docker-compose --version | Out-Null
}
catch {
    Write-Host "Docker Compose is not installed or not in the PATH." -ForegroundColor Red
    exit 1
}

# Check container status
Write-Host "===== Container Status =====" -ForegroundColor Green
$containers = docker-compose ps -q
if (-not $containers) {
    Write-Host "No containers are running. Please start the services with 'docker-compose up -d'." -ForegroundColor Red
    exit 1
}

# Check each service
Write-Host "===== Service Health Checks =====" -ForegroundColor Green

# Backend check
$backendStatus = docker-compose ps backend
if ($backendStatus -match "Up") {
    Write-Host "✅ Backend service: Running" -ForegroundColor Green
    # Check API health
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:8080/api/v1/health" -Method Get -UseBasicParsing
        if ($response.StatusCode -eq 200) {
            Write-Host "  ✅ API responding correctly" -ForegroundColor Green
        }
        else {
            Write-Host "  ❌ API not responding properly" -ForegroundColor Red
        }
    }
    catch {
        Write-Host "  ❌ API not responding" -ForegroundColor Red
    }
}
else {
    Write-Host "❌ Backend service: Not running" -ForegroundColor Red
}

# Frontend check
$frontendStatus = docker-compose ps frontend
if ($frontendStatus -match "Up") {
    Write-Host "✅ Frontend service: Running" -ForegroundColor Green
    # Check frontend health
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:3000" -Method Get -UseBasicParsing
        if ($response.StatusCode -eq 200) {
            Write-Host "  ✅ Frontend responding correctly" -ForegroundColor Green
        }
        else {
            Write-Host "  ❌ Frontend not responding properly" -ForegroundColor Red
        }
    }
    catch {
        Write-Host "  ❌ Frontend not responding" -ForegroundColor Red
    }
}
else {
    Write-Host "❌ Frontend service: Not running" -ForegroundColor Red
}

# Database check
$dbStatus = docker-compose ps postgres
if ($dbStatus -match "Up") {
    Write-Host "✅ Database service: Running" -ForegroundColor Green
}
else {
    Write-Host "❌ Database service: Not running" -ForegroundColor Red
}

Write-Host "===== Resource Usage =====" -ForegroundColor Green
docker stats --no-stream $(docker-compose ps -q)

Write-Host "============================================" -ForegroundColor Green
Write-Host "Health check complete!" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Green 