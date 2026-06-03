# UptimeLab — company-style commands (run from repo root)

COMPOSE := -f docker-compose.yml
COMPOSE_OBS := $(COMPOSE) -f docker-compose.observability.yml --profile observability
COMPOSE_ELASTIC := $(COMPOSE) -f docker-compose.elastic.yml --profile elastic
COMPOSE_ALL := $(COMPOSE) -f docker-compose.observability.yml -f docker-compose.elastic.yml --profile observability --profile elastic

.PHONY: help dev-db dev-backend dev-backend-docker dev-frontend dev ci-local smoke-test \
        up up-core up-all down down-all staging-up staging-down prod-local-up prod-local-down \
        obs-up elastic-up elastic-down stop-frontend free-ports

help:
	@echo "UptimeLab DevOps lab commands"
	@echo ""
	@echo "  Developer (fast loop)"
	@echo "    make dev-db              Postgres only"
	@echo "    make dev-backend         dotnet watch (needs .NET SDK)"
	@echo "    make dev-backend-docker  API in Docker (no .NET SDK on Mac)"
	@echo "    make dev-frontend        npm run dev"
	@echo "    make stop-frontend       Free port 3000 for npm run dev"
	@echo ""
	@echo "  Docker stacks"
	@echo "    make up              App + Prometheus + Grafana"
	@echo "    make elastic-up      App + Elasticsearch + Kibana APM"
	@echo "    make up-all          App + Grafana + Kibana (needs ~4GB RAM)"
	@echo "    make up-core         App only"
	@echo "    make down-all        Stop everything"
	@echo ""
	@echo "  Observability URLs (when running)"
	@echo "    App http://localhost:3000  API http://localhost:5001"
	@echo "    Grafana :3002  Prometheus :9090  Kibana :5601"
	@echo ""
	@echo "  Docs: docs/DEVOPS_LAB.md  docs/OBSERVABILITY.md  docs/ELASTIC_APM.md"

dev-db:
	docker compose up -d postgres
	@echo "Postgres ready. Connection: localhost:5432"

dev-backend:
	@command -v dotnet >/dev/null 2>&1 || { echo "Use: make dev-backend-docker"; exit 127; }
	cd backend && dotnet watch run

dev-backend-docker:
	docker compose up -d postgres backend
	@echo "API: http://localhost:5001/health"

dev-frontend:
	@echo "Tip: run 'make stop-frontend' if port 3000 is taken by Docker"
	cd frontend && test -f .env.local || cp .env.local.example .env.local; npm run dev

stop-frontend:
	docker compose stop frontend 2>/dev/null || true

free-ports:
	@-lsof -ti:3001 | xargs kill -9 2>/dev/null || true

up:
	docker compose $(COMPOSE_OBS) up -d --build
	@echo "Grafana http://localhost:3002  Prometheus http://localhost:9090"

up-core:
	docker compose up -d --build

elastic-up:
	docker compose $(COMPOSE_ELASTIC) up -d --build
	@echo "Wait 2-3 min, then Kibana APM: http://localhost:5601 → Observability → APM → uptimelab-api"

elastic-down:
	docker compose $(COMPOSE_ELASTIC) down

up-all:
	docker compose $(COMPOSE_ALL) up -d --build
	@echo "Full observability: Grafana :3002  Prometheus :9090  Kibana :5601"

down:
	docker compose $(COMPOSE_OBS) down

down-all:
	docker compose $(COMPOSE_ALL) down -v

staging-up:
	docker compose -f docker-compose.yml -f docker-compose.staging.yml --env-file .env.staging up -d --build

staging-down:
	docker compose -f docker-compose.yml -f docker-compose.staging.yml --env-file .env.staging down

prod-local-up:
	docker compose -f docker-compose.yml -f docker-compose.production.local.yml --env-file .env.production.local up -d --build

prod-local-down:
	docker compose -f docker-compose.yml -f docker-compose.production.local.yml --env-file .env.production.local down

obs-up:
	docker compose $(COMPOSE_OBS) up -d prometheus grafana blackbox postgres-exporter

ci-local:
	./scripts/ci-local.sh

smoke-test:
	./scripts/smoke-test.sh
