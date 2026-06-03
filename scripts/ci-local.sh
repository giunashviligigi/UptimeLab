#!/usr/bin/env bash
# Runs the same checks as GitHub Actions CI — practice before opening a PR.
set -euo pipefail
cd "$(dirname "$0")/.."

echo "==> [CI] Backend build"
if command -v dotnet >/dev/null 2>&1; then
  (cd backend && dotnet restore && dotnet build -c Release --no-restore)
else
  echo "    dotnet not installed locally — using Docker (same as GitHub Actions runner)"
  docker compose build backend
fi

echo "==> [CI] Frontend build"
(cd frontend && npm ci && NEXT_PUBLIC_API_URL=http://localhost:5001 npm run build)

echo "==> [CI] Docker compose build"
docker compose build

echo "==> CI local: ALL PASSED"
