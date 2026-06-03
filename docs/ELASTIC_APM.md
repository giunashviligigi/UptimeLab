# Elastic APM + Kibana — API latency, throughput, errors

This stack adds **Elasticsearch**, **APM Server**, and **Kibana** so you can see real **APM** data for the UptimeLab API (not just Prometheus metrics).

## What you get in Kibana

| APM view | What it shows |
|----------|----------------|
| **Latency** | p50 / p95 / p99 per endpoint (`/api/auth/login`, `/api/sites`, …) |
| **Throughput** | Requests per minute per transaction |
| **Error rate** | Failed requests and exceptions |
| **Transactions** | Trace waterfall for slow calls |
| **Dependencies** | PostgreSQL, outbound HTTP (uptime checks) |

## Architecture

```text
UptimeLab API (.NET)  --traces-->  APM Server (:8200)
                                        |
                                        v
                                 Elasticsearch (:9200)
                                        |
                                        v
                                   Kibana (:5601)  →  Observability → APM
```

Prometheus/Grafana and Elastic can run **together** (`make up-all`) or separately.

## Quick start

```bash
# Elastic only (app + ELK APM) — ~2GB RAM for Elasticsearch
make elastic-up
```

Wait **2–3 minutes** for Elasticsearch + Kibana to go green.

| URL | Purpose |
|-----|---------|
| http://localhost:5601 | **Kibana** |
| http://localhost:8200 | APM Server intake |
| http://localhost:9200 | Elasticsearch API |

### Open APM in Kibana

1. Open http://localhost:5601  
2. Menu → **Observability** → **APM**  
3. Service: **uptimelab-api**  
4. Tabs: **Transactions**, **Errors**, **Metrics**

### Generate traffic

Use the app (register, login, add sites, refresh dashboard). APM needs HTTP traffic to show charts.

## Commands

```bash
make elastic-up    # App + Elasticsearch + Kibana + APM
make elastic-down  # Stop Elastic stack

make up-all        # App + Prometheus/Grafana + Elastic (heavy on RAM)
make down-all      # Stop everything
```

## How the API is instrumented

- NuGet: `Elastic.Apm.NetCoreAll`
- Enabled when env var `ELASTIC_APM_SERVER_URL` is set (Docker Elastic profile)
- Auto-instruments ASP.NET Core: controllers, EF Core, `HttpClient` (worker checks)

Environment variables (in `docker-compose.elastic.yml`):

```env
ELASTIC_APM_SERVER_URL=http://apm-server:8200
ELASTIC_APM_SERVICE_NAME=uptimelab-api
ELASTIC_APM_ENVIRONMENT=docker
ELASTIC_APM_TRANSACTION_SAMPLE_RATE=1.0
```

## Lab exercises

1. **Baseline** — login 10 times, note throughput on `POST /api/auth/login`.  
2. **Slow endpoint** — add many sites, open history (DB reads). Compare latency in APM.  
3. **Errors** — call a bad URL or trigger 401; check **Errors** tab in Kibana.  
4. **Compare** — same moment in Grafana (Prometheus) vs Kibana (APM traces).

## Troubleshooting

| Issue | Fix |
|-------|-----|
| Kibana empty | Wait 2–3 min; generate API traffic |
| No `uptimelab-api` service | `docker logs uptimelab-backend` — APM connection errors? |
| Elasticsearch OOM | Increase Docker memory to 4GB+ or use `make up-core` + only Prometheus |
| Port 5601 in use | Change Kibana port in `docker-compose.elastic.yml` |

## Security note

`xpack.security.enabled=false` is for **local lab only**. Never use this in production without TLS and authentication.
