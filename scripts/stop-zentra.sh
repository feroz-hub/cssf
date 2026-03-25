#!/usr/bin/env bash
# ---------------------------------------------------------------------------
# stop-zentra.sh  –  Stop Zentra Server & Admin MVC and clear all caches
#                     (macOS/Linux)
# ---------------------------------------------------------------------------

set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PID_DIR="${ROOT_DIR}/.run-pids"
LOG_DIR="${ROOT_DIR}/.run-logs"

STOPPED=0

# ── Stop a service by PID file ─────────────────────────────────────────────

stop_service() {
  local name="$1" pid_file="$2"

  if [[ ! -f "${pid_file}" ]]; then
    echo "${name}: no PID file found – not running via start-zentra."
    return
  fi

  local pid
  pid=$(cat "${pid_file}")

  if kill -0 "${pid}" 2>/dev/null; then
    echo "Stopping ${name} (PID ${pid})..."
    kill "${pid}" 2>/dev/null || true
    # Wait up to 15 seconds for graceful shutdown
    local i=0
    while kill -0 "${pid}" 2>/dev/null && (( i < 15 )); do
      sleep 1
      i=$((i + 1))
    done
    # Force-kill if still alive
    if kill -0 "${pid}" 2>/dev/null; then
      echo "  Force-killing ${name} (PID ${pid})..."
      kill -9 "${pid}" 2>/dev/null || true
    fi
    echo "  ${name} stopped."
    STOPPED=$((STOPPED + 1))
  else
    echo "${name}: process ${pid} is not running (stale PID file)."
  fi

  rm -f "${pid_file}"
}

# ── Also kill any dotnet processes on the known ports ──────────────────────

kill_by_port() {
  local port="$1" name="$2"
  local pids
  pids=$(lsof -ti ":${port}" 2>/dev/null || true)
  if [[ -n "${pids}" ]]; then
    echo "Killing ${name} process(es) on port ${port}: ${pids}"
    echo "${pids}" | xargs kill 2>/dev/null || true
    sleep 1
    # Force-kill stragglers
    pids=$(lsof -ti ":${port}" 2>/dev/null || true)
    if [[ -n "${pids}" ]]; then
      echo "${pids}" | xargs kill -9 2>/dev/null || true
    fi
    STOPPED=$((STOPPED + 1))
  fi
}

echo "====================================="
echo "  Stopping Zentra services"
echo "====================================="
echo

# 1. Stop via PID files
stop_service "Zentra Server"    "${PID_DIR}/zentra-server.pid"
stop_service "Zentra Admin MVC" "${PID_DIR}/zentra-admin.pid"

# 2. Fallback – kill by port if PID files were missing
kill_by_port 5001 "Zentra Server"
kill_by_port 3001 "Zentra Admin MVC"

echo
echo "====================================="
echo "  Clearing caches"
echo "====================================="
echo

# ── 1. Redis cache (Docker) ───────────────────────────────────────────────

if command -v docker >/dev/null 2>&1 && docker ps --format '{{.Names}}' 2>/dev/null | grep -q "HCL.CS.SF-redis"; then
  echo "Flushing Redis cache (Docker container HCL.CS.SF-redis)..."
  docker exec HCL.CS.SF-redis redis-cli FLUSHALL >/dev/null 2>&1 && echo "  Redis cache flushed." || echo "  Warning: could not flush Redis."
elif command -v redis-cli >/dev/null 2>&1; then
  REDIS_PORT="${HCL_CS_SF_REDIS_PORT:-56380}"
  echo "Flushing Redis cache (localhost:${REDIS_PORT})..."
  redis-cli -p "${REDIS_PORT}" FLUSHALL >/dev/null 2>&1 && echo "  Redis cache flushed." || echo "  Warning: could not flush Redis (is it running?)."
else
  echo "Redis: skipped (no Docker container or redis-cli found)."
fi

# ── 2. ASP.NET in-memory / data-protection cache ──────────────────────────

if [[ -d "${ROOT_DIR}/.data/DataProtection-Keys" ]]; then
  echo "Clearing ASP.NET DataProtection keys..."
  rm -rf "${ROOT_DIR}/.data/DataProtection-Keys"
  echo "  Done."
fi

# ── 3. .NET build caches (obj/bin) ────────────────────────────────────────

echo "Clearing .NET build caches (bin/obj)..."
find "${ROOT_DIR}/src" "${ROOT_DIR}/demos" -type d \( -name bin -o -name obj \) -prune -exec rm -rf {} + 2>/dev/null || true
echo "  Done."

# ── 4. Next.js cache (HCL.CS.SF-admin) ────────────────────────────────────

NEXTJS_DIR="${ROOT_DIR}/HCL.CS.SF-admin"
if [[ -d "${NEXTJS_DIR}/.next" ]]; then
  echo "Clearing Next.js cache (.next)..."
  rm -rf "${NEXTJS_DIR}/.next"
  echo "  Done."
fi

# ── 5. NuGet HTTP cache ───────────────────────────────────────────────────

echo "Clearing NuGet HTTP cache..."
dotnet nuget locals http-cache --clear 2>/dev/null || true
echo "  Done."

# ── 6. Run logs ───────────────────────────────────────────────────────────

if [[ -d "${LOG_DIR}" ]]; then
  echo "Clearing run logs..."
  rm -rf "${LOG_DIR}"
  echo "  Done."
fi

# ── 7. Temp / session files ───────────────────────────────────────────────

if [[ -d "${ROOT_DIR}/.data" ]]; then
  # Remove ASP.NET temp session files but keep the SQLite DB
  find "${ROOT_DIR}/.data" -name "*.tmp" -delete 2>/dev/null || true
  find "${ROOT_DIR}/.data" -name "*.cache" -delete 2>/dev/null || true
fi

echo
echo "====================================="
echo "  All Zentra services stopped."
echo "  All caches cleared."
echo "====================================="
