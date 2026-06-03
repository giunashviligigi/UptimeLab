# UptimeLab — company-style commands (run from repo root)

.PHONY: help dev-db dev-backend dev-backend-docker dev-frontend dev ci-local smoke-test \
        up down staging-up staging-down prod-local-up prod-local-down obs-up obs-down \
        stop-frontend free-ports

help:
	@echo "UptimeLab DevOps lab commands"
	@echo ""
	@echo "  Developer (fast loop)"
	@echo "    make dev-db              Postgres only"
	@echo "    make dev-backend         dotnet watch (needs .NET SDK)"
	@echo "    make dev-backend-docker  API in Docker (no .NET SDK on Mac)"
	@echo "    make dev-frontend        npm run dev — stop Docker frontend first"
	@echo "    make stop-frontend       Free port 3000 for npm run dev"
	@echo ""
	@echo "  Docker stacks"
	@echo "    make up              Default stack (ports 3000 / 5001)"
	@echo "    make down            Stop default stack"
	@echo "    make staging-up      Staging (3001 / 5002 / 5433)"
	@echo "    make prod-local-up   Prod-like local (3010 / 5010)"
	@echo "    make obs-up          Prometheus + Grafana (9090 / 3002)"
	@echo ""
	@echo "  CI/CD practice"
	@echo "    make ci-local        Run same checks as GitHub CI"
	@echo "    make smoke-test      Hit /health endpoints"
	@echo ""
	@echo "  Lab guide: docs/DEVOPS_LAB.md"

# --- Developer hybrid (real company local workflow) ---
dev-db:
	docker compose up -d postgres
	@echo "Postgres ready. Connection: localhost:5432"

dev-backend:
	@command -v dotnet >/dev/null 2>&1 || { \
		echo "dotnet not found. Install: https://dotnet.microsoft.com/download"; \
		echo "Or run: make dev-backend-docker"; \
		exit 127; \
	}
	cd backend && dotnet watch run

# Use when .NET SDK is not installed — same API as Docker, no hot reload
dev-backend-docker:
	docker compose up -d postgres backend
	@echo "API: http://localhost:5001/health"

dev-frontend:
	@echo "Tip: run 'make stop-frontend' if port 3000 is taken by Docker"
	cd frontend && test -f .env.local || cp .env.local.example .env.local; npm run dev

stop-frontend:
	docker compose stop frontend 2>/dev/null || true
	@echo "Docker frontend stopped. Port 3000 free for npm run dev."

free-ports:
	@echo "Stopping suspended npm/next on 3001 (if any)..."
	@-lsof -ti:3001 | xargs kill -9 2>/dev/null || true
	@echo "Done. Retry: make staging-up"

# --- Default production-like compose ---
up:
	docker compose up -d --build

down:
	docker compose down

# --- Staging ---
staging-up:
	docker compose -f docker-compose.yml -f docker-compose.staging.yml --env-file .env.staging up -d --build

staging-down:
	docker compose -f docker-compose.yml -f docker-compose.staging.yml --env-file .env.staging down

# --- Local "production" instance (separate ports/volume) ---
prod-local-up:
	docker compose -f docker-compose.yml -f docker-compose.production.local.yml --env-file .env.production.local up -d --build

prod-local-down:
	docker compose -f docker-compose.yml -f docker-compose.production.local.yml --env-file .env.production.local down

# --- Observability ---
obs-up:
	docker compose -f docker-compose.yml -f docker-compose.observability.yml up -d prometheus grafana
	@echo "Prometheus http://localhost:9090  Grafana http://localhost:3002 (admin/admin)"

obs-down:
	docker compose -f docker-compose.yml -f docker-compose.observability.yml down

# --- CI locally ---
ci-local:
	./scripts/ci-local.sh

smoke-test:
	./scripts/smoke-test.sh
