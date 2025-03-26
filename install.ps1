# Kogase Installer Script for Windows
# This script clones the Kogase repository and sets up the project

# Exit on error
$ErrorActionPreference = "Stop"

Write-Host @"

██╗  ██╗ ██████╗  ██████╗  █████╗ ███████╗███████╗
██║ ██╔╝██╔═══██╗██╔════╝ ██╔══██╗██╔════╝██╔════╝
█████╔╝ ██║   ██║██║  ███╗███████║███████╗█████╗  
██╔═██╗ ██║   ██║██║   ██║██╔══██║╚════██║██╔══╝  
██║  ██╗╚██████╔╝╚██████╔╝██║  ██║███████║███████╗
╚═╝  ╚═╝ ╚═════╝  ╚═════╝ ╚═╝  ╚═╝╚══════╝╚══════╝
                                                   
Game Analytics & Telemetry
"@ -ForegroundColor Cyan

# Check if Git is installed
try {
    git --version | Out-Null
}
catch {
    Write-Host "Git is not installed. Please install Git and try again." -ForegroundColor Red
    exit 1
}

# Check if Docker is installed
try {
    docker --version | Out-Null
}
catch {
    Write-Host "Docker is not installed. Please install Docker and try again." -ForegroundColor Red
    exit 1
}

# Check if Docker Compose is installed
try {
    docker-compose --version | Out-Null
}
catch {
    Write-Host "Docker Compose is not installed. Please install Docker Compose and try again." -ForegroundColor Red
    exit 1
}

# Set the repository URL
$REPO_URL = "https://github.com/atqamz/kogase.git"

# Ask for installation directory or use default
Write-Host "Where would you like to install Kogase? (default: .\kogase)" -ForegroundColor Yellow
$INSTALL_DIR = Read-Host
if ([string]::IsNullOrWhiteSpace($INSTALL_DIR)) {
    $INSTALL_DIR = ".\kogase"
}

# Clone the repository
Write-Host "Cloning Kogase repository..." -ForegroundColor Green
git clone $REPO_URL $INSTALL_DIR

# Change to the installation directory
Push-Location $INSTALL_DIR

# Run the setup script
Write-Host "Setting up Kogase..." -ForegroundColor Green
try {
    # Run the PowerShell setup script
    .\setup.ps1
}
finally {
    # Return to the original directory
    Pop-Location
}

Write-Host @"

Installation complete! 🎉

Access Kogase at:
  - Frontend: http://localhost:3000
  - Backend API: http://localhost:8080/api/v1
  - API Documentation: http://localhost:8080/swagger/index.html

To check the health of your installation:
  > cd $INSTALL_DIR
  > .\healthcheck.ps1

To manage your Kogase installation:
  > cd $INSTALL_DIR
  > docker-compose up -d   # Start all containers
  > docker-compose down    # Stop all containers
  > docker-compose logs -f # View logs
"@ -ForegroundColor Green 