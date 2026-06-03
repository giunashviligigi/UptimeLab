---
name: Incident / outage drill
about: Practice on-call style investigation
title: "[Incident] "
labels: incident
---

## Symptoms

What broke? Who noticed?

## Timeline

- Detected:
- Mitigated:

## Metrics / logs checked

- [ ] `docker compose logs backend`
- [ ] http://localhost:9090 (Prometheus)
- [ ] http://localhost:3002 (Grafana)
- [ ] `GET /health`

## Root cause

## Follow-up

- [ ] Prevent recurrence
- [ ] Update runbook in `docs/DEVOPS_LAB.md`
