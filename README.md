# UptimeLab

Self-hosted website monitoring platform for DevOps practice.

**Real-life company lab:** follow **[docs/DEVOPS_LAB.md](docs/DEVOPS_LAB.md)** for branch workflow, staging/prod environments, and CI/CD. Quick start: `make help`.

**APM (Kibana):** `make elastic-up` — API latency, throughput, and error rates. See **[docs/ELASTIC_APM.md](docs/ELASTIC_APM.md)**.

**Stack:** Next.js (TypeScript) · .NET 8 Web API · PostgreSQL · EF Core · JWT · Docker Compose

## Project structure

```
UpTimeLab/
├── backend/          # .NET 8 Web API
│   ├── Controllers/
│   ├── Services/
│   ├── Models/
│   ├── DTOs/
│   ├── Data/
│   ├── BackgroundServices/
│   └── Migrations/
├── frontend/         # Next.js app
│   ├── app/          # Pages (App Router)
│   ├── components/
│   └── lib/          # API client & auth helpers
├── docs/DEVOPS_LAB.md    # Company-style lab exercises
├── scripts/              # ci-local, smoke-test
├── deploy/               # GHCR pull compose for servers
├── grafana/              # Dashboards & provisioning
├── nginx/                # Reverse proxy config (optional)
├── prometheus/           # Metrics scrape config
├── Makefile              # dev / staging / prod / obs commands
├── docker-compose.yml
├── docker-compose.staging.yml
└── .env.example
```

## Features

- User registration & login (JWT, BCrypt passwords)
- Dashboard with total / online / offline sites & avg response time
- Add, list, delete monitored URLs
- Background checks every **60 seconds** (status, HTTP code, response time)
- Per-site check history
- Public status page: `/status/{userId}`
- `GET /health` and Prometheus metrics at `/metrics`
- Admin-ready `Role` field on users (`User` / `Admin`)

## API endpoints

| Method | Path | Auth |
|--------|------|------|
| POST | `/api/auth/register` | No |
| POST | `/api/auth/login` | No |
| GET | `/api/sites` | JWT |
| GET | `/api/sites/stats` | JWT |
| POST | `/api/sites` | JWT |
| DELETE | `/api/sites/{id}` | JWT |
| GET | `/api/sites/{id}/history` | JWT |
| GET | `/api/public/status/{userId}` | No |
| GET | `/health` | No |
| GET | `/metrics` | No (Prometheus) |

---

## Quick start with Docker (recommended)

### Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (includes Docker Compose)

### Commands (VS Code terminal)

Open the integrated terminal in VS Code: **Terminal → New Terminal**, then:

```bash
cd /Users/gigi/Desktop/UpTimeLab

# 1. Copy environment file and edit JWT secret if you like
cp .env.example .env

# 2. Build and start all services
docker compose up --build -d

# 3. Check containers are running
docker compose ps

# 4. View backend logs (migrations + monitoring worker)
docker compose logs -f backend
```

Open in browser:

- **Frontend:** http://localhost:3000
- **API / Swagger (dev only):** http://localhost:5001/swagger (if running API in Development)
- **Health:** http://localhost:5001/health
- **Metrics:** http://localhost:5001/metrics

Stop everything:

```bash
docker compose down
```

Remove database volume:

```bash
docker compose down -v
```

---

## Local development (without Docker)

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/)
- PostgreSQL 16 (local or Docker only for DB)

### 1. PostgreSQL

```bash
# Option A: Postgres in Docker only
docker run -d --name uptimelab-db \
  -e POSTGRES_USER=uptime \
  -e POSTGRES_PASSWORD=uptime_secret \
  -e POSTGRES_DB=uptimelab \
  -p 5432:5432 \
  postgres:16-alpine
```

### 2. Backend

```bash
cd backend

# Restore packages
dotnet restore

# Apply database migrations (also runs automatically on API startup)
dotnet ef database update

# Run API on http://localhost:5001
dotnet run
```

If `dotnet ef` is not installed:

```bash
dotnet tool install --global dotnet-ef
```

Update `appsettings.json` connection string if your Postgres credentials differ.

### 3. Frontend

```bash
cd frontend

npm install

# Point to local API
echo "NEXT_PUBLIC_API_URL=http://localhost:5001" > .env.local

npm run dev
```

Open http://localhost:3000

---

## Usage flow

1. **Register** at `/register`
2. **Dashboard** — add a site via “Add site” (use real URLs like `https://google.com`)
3. Wait up to **60 seconds** for the first check
4. Open a site row to see **history**
5. Share your **public status** link from the dashboard (`/status/{userId}`)

JWT is stored in `localStorage` (`uptimelab_token`) for learning purposes. For production, prefer httpOnly cookies.

---

## Nginx reverse proxy

1. Uncomment the `nginx` service in `docker-compose.yml`
2. Use config in `nginx/conf.d/uptimelab.conf`
3. Access app on port **80** with `/api` proxied to the backend

---

## Prometheus / Grafana (later)

- Backend exposes metrics at `/metrics` (prometheus-net)
- Example scrape config: `prometheus/prometheus.yml`
- Point Grafana to your Prometheus datasource and build uptime dashboards

---

## Environment variables

See `.env.example` for all variables. Important:

| Variable | Description |
|----------|-------------|
| `JWT_SECRET` | Signing key for JWT (32+ chars in production) |
| `NEXT_PUBLIC_API_URL` | Browser-visible API URL for the frontend |
| `ConnectionStrings__DefaultConnection` | Set via Docker Compose for the backend |

---

## Troubleshooting

| Issue | Fix |
|-------|-----|
| Frontend cannot reach API | Ensure `NEXT_PUBLIC_API_URL` matches where the browser loads the API (e.g. `http://localhost:5001`) |
| CORS errors | Add your frontend origin to `Cors:Origins` in `appsettings.json` or `Cors__Origins__0` env var |
| No check results yet | Worker runs every 60s; ensure URL is reachable from the backend container |
| EF migration errors | Run `dotnet ef database update` from `backend/` folder |

---

## License

MIT — built for learning DevOps and full-stack patterns.
