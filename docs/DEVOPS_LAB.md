# UptimeLab — Real-Life DevOps Lab

Practice how **developers** and **DevOps** collaborate in a real company, using this repo as your sandbox.

## Company simulation map

| Role | You play as | Main tools |
|------|-------------|------------|
| Developer | Feature branches, PRs, local dev | Git, `make dev`, VS Code |
| DevOps / Platform | CI/CD, Docker, monitoring | GitHub Actions, Compose, Prometheus/Grafana |
| Release manager | Promote staging → production | `develop` → `main`, tags, approvals |

## Branch strategy (like many teams)

```text
feature/login-ui  ──PR──►  develop  ──PR──►  main
                              │                  │
                         staging deploy    production deploy
                         (auto/lab)        (approval in GH)
```

| Branch | Purpose | Deploy target |
|--------|---------|---------------|
| `feature/*` | Your work | None (CI only) |
| `develop` | Integration / QA | Staging |
| `main` | Production-ready | Production |

---

## Module 0 — One-time setup (30 min)

1. Push repo to GitHub (if not done).
2. Copy env files:
   ```bash
   cp .env.example .env
   cp .env.staging.example .env.staging
   ```
3. Install tools: Docker, Git, .NET 8 SDK, Node 20.
4. Create `develop` branch:
   ```bash
   git checkout -b develop
   git push -u origin develop
   ```
5. On GitHub → **Settings → Branches** → add rule for `main`:
   - Require pull request
   - Require status check: **CI** (after first workflow run)

**Deliverable:** `main` and `develop` exist on GitHub; Actions enabled.

---

## Module 1 — Developer day (local, no Docker rebuild)

**Goal:** Fast feedback like a dev on a team (DB in Docker, app on host).

**No .NET SDK on your Mac?** Use Docker for the API instead:

```bash
make dev-db
make dev-backend-docker   # API in Docker
make stop-frontend        # free port 3000
make dev-frontend         # UI with hot reload
```

**With .NET SDK installed:**

```bash
make dev-db          # starts only Postgres
make dev-backend     # terminal 2: dotnet watch
make stop-frontend   # if Docker frontend uses port 3000
make dev-frontend    # terminal 3: npm run dev
```

### Troubleshooting Module 1

| Problem | Fix |
|---------|-----|
| `dotnet: command not found` | `make dev-backend-docker` or install .NET 8 SDK |
| Port 3000 in use | `make stop-frontend` then `make dev-frontend` |
| Port 3001 in use (staging fails) | You suspended `npm run dev` — run `make free-ports` or `kill %1` |
| `make ci-local` fails on dotnet | Updated script uses Docker for backend if dotnet missing |

1. Register at http://localhost:3000
2. Add a site, wait 60s for check
3. Change something small in `frontend/app/dashboard/page.tsx` (e.g. title text)
4. Save → browser hot-reloads **without** restarting Docker

**Discussion questions:**
- Why is this faster than `docker compose up --build`?
- What still requires a container restart? (env vars, backend deps, Dockerfile)

---

## Module 2 — Pull request & CI (developer + CI robot)

**Goal:** Nothing merges without green CI.

```bash
git checkout develop
git pull
git checkout -b feature/lab-banner
# Edit frontend/app/page.tsx — add a line of text
git add . && git commit -m "feat: add lab banner on landing page"
git push -u origin feature/lab-banner
```

1. Open **Pull Request** → base: `develop`
2. Watch **Actions** tab → workflow **CI** must pass
3. Run CI locally before push (optional):
   ```bash
   make ci-local
   ```
4. Merge PR (squash is fine for learning)

**Deliverable:** Merged PR with green CI check on GitHub.

---

## Module 3 — Staging environment (DevOps)

**Goal:** `develop` deploys to a **separate** stack (different ports & DB).

```bash
make staging-up
```

| Service | URL |
|---------|-----|
| Staging app | http://localhost:3100 |
| Staging API | http://localhost:5002/health |
| Staging DB | localhost:5433 |

1. Register a **different** user on staging (separate database).
2. Break something on `develop` (bad env) and see staging fail — fix in PR.

**On GitHub:** push to `develop` runs **CD Staging** (build + push `:staging` images).

```bash
git checkout develop
git push origin develop
# Check Actions → "CD Staging"
```

**Deliverable:** Staging running locally; CD Staging workflow green on GitHub.

---

## Module 4 — Observability (on-call simulation)

**Goal:** Use metrics like an on-call engineer.

```bash
make obs-up
```

| Tool | URL | Login |
|------|-----|-------|
| Prometheus | http://localhost:9090 | — |
| Grafana | http://localhost:3002 | admin / admin |

1. Confirm target **uptimelab-api** is UP in Prometheus → Status → Targets
2. Open Grafana → dashboard **UptimeLab Overview**
3. Generate traffic: refresh dashboard, register users
4. Find `http_requests_received_total` for `/health` and `/api/*`

**Incident drill:**
- Stop backend: `docker stop uptimelab-backend`
- Watch metrics / health checks fail
- Restart: `docker start uptimelab-backend`

**Deliverable:** Screenshot or notes: which metric proved the API was down?

---

## Module 5 — Production release (release manager)

**Goal:** Production only from `main`, with discipline.

```bash
git checkout develop
git pull
git checkout main
git pull
git merge develop
git push origin main
```

1. GitHub Actions → **CD Production** runs
2. Images tagged `latest` and commit SHA on GHCR
3. (Optional) Approve **production** environment if you enabled it in GitHub

**Production-like local stack** (separate ports):

```bash
make prod-local-up
# App http://localhost:3010  API http://localhost:5010
```

**Rollback drill:**

```bash
git checkout main
git revert HEAD
git push origin main
# New images build; redeploy pulls previous behavior via git revert
```

**Deliverable:** `main` has CD Production run; you can explain rollback in one sentence.

---

## Module 6 — Run CI/CD without GitHub (airplane mode)

```bash
make ci-local          # same steps as GitHub CI
make smoke-test        # health + optional API check
```

Compare duration to cloud CI. Why do companies use remote CI?

---

## Module 7 — Secrets & config (security review)

1. Open `.env.example` — what must **never** be committed?
2. Where should `JWT_SECRET` live in real prod? (GitHub Secrets, Vault, cloud KMS)
3. Why must `NEXT_PUBLIC_API_URL` be set at **frontend build** time?

**Deliverable:** Short checklist of 5 secrets/config rules for UptimeLab.

---

## Capstone — Full sprint (half day)

| Step | Action |
|------|--------|
| 1 | `feature/capstone` from `develop` |
| 2 | Add feature (e.g. show last error on dashboard) |
| 3 | `make ci-local` → PR → merge to `develop` |
| 4 | `make staging-up` → manual QA |
| 5 | `make obs-up` → verify metrics during QA |
| 6 | PR `develop` → `main` → CD Production |
| 7 | Write 5-line postmortem: what broke, what you checked, what you fixed |

---

## Commands cheat sheet

```bash
make help              # all targets
make dev-db            # Postgres only
make staging-up        # staging stack
make prod-local-up     # prod-like ports locally
make obs-up            # Prometheus + Grafana
make ci-local          # run CI pipeline locally
make smoke-test        # quick health checks
docker compose ps      # what's running
docker compose logs -f backend
```

---

## What real companies add (stretch goals)

- Kubernetes instead of Compose on server
- Terraform for cloud infra
- Automated tests in CI (unit + integration)
- SAST/Dependabot security scans
- PagerDuty alerts from Grafana
- Separate staging/prod AWS accounts

Use this lab until the branch → CI → staging → prod flow feels boring. That's when it starts to feel like work — in a good way.
