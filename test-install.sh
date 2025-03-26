#!/bin/bash
# Script to test the Kogase installation process without cloning from GitHub

# Exit on error
set -e

# Temporary directory for testing
TEST_DIR=$(mktemp -d)
echo "Created temporary test directory: $TEST_DIR"

# Cleanup function to remove temporary directory on exit
cleanup() {
    echo "Cleaning up test directory..."
    rm -rf "$TEST_DIR"
}
trap cleanup EXIT

# Copy installation files to test directory
echo "Copying installation files to test directory..."
cp install.sh install.ps1 install-kogase.sh "$TEST_DIR/"
cp setup.sh setup.ps1 healthcheck.sh healthcheck.ps1 "$TEST_DIR/"
cp .env.example "$TEST_DIR/"
cp Makefile "$TEST_DIR/"

# Make scripts executable in test directory
chmod +x "$TEST_DIR"/*.sh

# Create a mock docker-compose.yaml for testing
cat > "$TEST_DIR/docker-compose.yaml" << 'EOF'
version: '3.8'
services:
  test:
    image: hello-world
EOF

# Switch to test directory
cd "$TEST_DIR"

# Test installation script (with modifications to avoid actually running Docker)
echo "Testing installation script..."
sed -i 's/docker-compose build/echo "Mock: docker-compose build"/g' setup.sh
sed -i 's/docker-compose up -d/echo "Mock: docker-compose up -d"/g' setup.sh

# Run the script in mock mode
./install.sh

echo "Installation script test completed successfully!"
echo "All scripts are ready for deployment." 