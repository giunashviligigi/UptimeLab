#!/usr/bin/env bash
# Quick smoke tests — simulates post-deploy verification.
set -euo pipefail

check() {
  local name="$1" url="$2"
  if curl -sf "$url" > /dev/null; then
    echo "OK  $name ($url)"
  else
    echo "FAIL $name ($url)"
    return 1
  fi
}

echo "==> Smoke tests"

# Default dev stack
check "Dev API health" "http://localhost:5001/health" || true

# Staging (if running)
check "Staging API health" "http://localhost:5002/health" || true

# Prod-local (if running)
check "Prod-local API health" "http://localhost:5010/health" || true

echo "Done. Fix any FAIL lines or start the stack (make up / make staging-up)."
