#!/bin/bash
# One-liner installer for Kogase
# This script detects the platform and runs the appropriate installer

# URL for raw install scripts on GitHub
INSTALLER_URL_BASE="https://raw.githubusercontent.com/atqamz/kogase/main"

# Function to display an error message and exit
error_exit() {
    echo "Error: $1" >&2
    exit 1
}

# Detect the platform
case "$(uname -s)" in
    Linux*|Darwin*)
        echo "Detected Linux/macOS system"
        # Download and run the bash install script
        curl -fsSL "$INSTALLER_URL_BASE/install.sh" -o install-kogase-temp.sh || error_exit "Failed to download installer"
        chmod +x install-kogase-temp.sh
        ./install-kogase-temp.sh
        rm install-kogase-temp.sh
        ;;
    CYGWIN*|MINGW*|MSYS*|Windows*)
        echo "Detected Windows system"
        # Create a temporary PowerShell script to download and run the actual installer
        echo 'Invoke-Expression "& { $(Invoke-RestMethod https://raw.githubusercontent.com/atqamz/kogase/main/install.ps1) }"' > install-kogase-temp.ps1
        powershell -ExecutionPolicy Bypass -File install-kogase-temp.ps1
        rm install-kogase-temp.ps1
        ;;
    *)
        error_exit "Unsupported operating system"
        ;;
esac 