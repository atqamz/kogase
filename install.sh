#!/bin/bash
# Kogase Installer Script for Linux/Mac
# This script clones the Kogase repository and sets up the project

# Exit on error
set -e

echo "
██╗  ██╗ ██████╗  ██████╗  █████╗ ███████╗███████╗
██║ ██╔╝██╔═══██╗██╔════╝ ██╔══██╗██╔════╝██╔════╝
█████╔╝ ██║   ██║██║  ███╗███████║███████╗█████╗  
██╔═██╗ ██║   ██║██║   ██║██╔══██║╚════██║██╔══╝  
██║  ██╗╚██████╔╝╚██████╔╝██║  ██║███████║███████╗
╚═╝  ╚═╝ ╚═════╝  ╚═════╝ ╚═╝  ╚═╝╚══════╝╚══════╝
                                                   
Game Analytics & Telemetry
"

# Check if Git is installed
if ! command -v git &> /dev/null; then
    echo "Git is not installed. Please install Git and try again."
    exit 1
fi

# Check if Docker is installed
if ! command -v docker &> /dev/null; then
    echo "Docker is not installed. Please install Docker and try again."
    exit 1
fi

# Check if Docker Compose is installed
if ! command -v docker-compose &> /dev/null; then
    echo "Docker Compose is not installed. Please install Docker Compose and try again."
    exit 1
fi

# Set the repository URL
REPO_URL="https://github.com/atqamz/kogase.git"

# Ask for installation directory or use default
echo "Where would you like to install Kogase? (default: ./kogase)"
read -r INSTALL_DIR
INSTALL_DIR=${INSTALL_DIR:-./kogase}

# Clone the repository
echo "Cloning Kogase repository..."
git clone "$REPO_URL" "$INSTALL_DIR"

# Change to the installation directory
cd "$INSTALL_DIR"

# Run the setup script
echo "Setting up Kogase..."
chmod +x setup.sh
./setup.sh

echo "
Installation complete! 🎉

Access Kogase at:
  - Frontend: http://localhost:3000
  - Backend API: http://localhost:8080/api/v1
  - API Documentation: http://localhost:8080/swagger/index.html

To check the health of your installation:
  $ cd $INSTALL_DIR
  $ ./healthcheck.sh

To manage your Kogase installation:
  $ cd $INSTALL_DIR
  $ make help
" 