#!/bin/bash

# Exit on error
set -e

echo "Checking Kogase container health..."

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo "Docker is not running or you don't have permission to use it."
    exit 1
fi

# Check if Docker Compose is installed
if ! command -v docker-compose > /dev/null 2>&1; then
    echo "Docker Compose is not installed or not in the PATH."
    exit 1
fi

# Check container status
echo "===== Container Status ====="
containers=$(docker-compose ps -q)
if [ -z "$containers" ]; then
    echo "No containers are running. Please start the services with 'docker-compose up -d'."
    exit 1
fi

# Check each service
echo "===== Service Health Checks ====="

# Backend check
backend_status=$(docker-compose ps backend | grep "Up" || echo "Down")
if [[ $backend_status == *"Up"* ]]; then
    echo "✅ Backend service: Running"
    # Check API health
    if curl -s -o /dev/null -w "%{http_code}" http://localhost:8080/api/v1/health > /dev/null 2>&1; then
        echo "  ✅ API responding correctly"
    else
        echo "  ❌ API not responding"
    fi
else
    echo "❌ Backend service: Not running"
fi

# Frontend check
frontend_status=$(docker-compose ps frontend | grep "Up" || echo "Down")
if [[ $frontend_status == *"Up"* ]]; then
    echo "✅ Frontend service: Running"
    # Check frontend health
    if curl -s -o /dev/null -w "%{http_code}" http://localhost:3000 > /dev/null 2>&1; then
        echo "  ✅ Frontend responding correctly"
    else
        echo "  ❌ Frontend not responding"
    fi
else
    echo "❌ Frontend service: Not running"
fi

# Database check
db_status=$(docker-compose ps postgres | grep "Up" || echo "Down")
if [[ $db_status == *"Up"* ]]; then
    echo "✅ Database service: Running"
else
    echo "❌ Database service: Not running"
fi

echo "===== Resource Usage ====="
docker stats --no-stream $(docker-compose ps -q)

echo "============================================"
echo "Health check complete!"
echo "============================================" 