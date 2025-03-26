.PHONY: up down build rebuild logs clean prune help

# Default to help if no target is specified
.DEFAULT_GOAL := help

# Docker Compose Commands
up: ## Start all containers
	docker-compose up -d

down: ## Stop all containers
	docker-compose down

build: ## Build all containers
	docker-compose build

rebuild: ## Rebuild and restart all containers
	docker-compose build
	docker-compose up -d

logs: ## Show logs for all containers
	docker-compose logs -f

# Service Specific Commands
backend-logs: ## Show logs for backend container
	docker-compose logs -f backend

frontend-logs: ## Show logs for frontend container
	docker-compose logs -f frontend

db-logs: ## Show logs for database container
	docker-compose logs -f postgres

restart-backend: ## Restart backend container
	docker-compose restart backend

restart-frontend: ## Restart frontend container
	docker-compose restart frontend

# Health Check Commands
health: ## Run health check
	@bash ./healthcheck.sh

# Utility Commands
clean: ## Remove unused Docker resources
	docker system prune -f

prune: ## Remove all unused Docker resources including volumes (USE WITH CAUTION)
	docker system prune -a --volumes -f

setup: ## Run the setup script
	@bash ./setup.sh

# Help Command
help: ## Display this help message
	@echo "Usage: make [target]"
	@echo ""
	@echo "Targets:"
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | sort | awk 'BEGIN {FS = ":.*?## "}; {printf "\033[36m%-20s\033[0m %s\n", $$1, $$2}' 