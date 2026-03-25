#!/usr/bin/env bash
# ---------------------------------------------------------------------------
# start-zentra.sh  –  Start Zentra Server and Zentra Admin MVC (macOS/Linux)
# ---------------------------------------------------------------------------

set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
LOG_DIR="${ROOT_DIR}/.run-logs"
PID_DIR="${ROOT_DIR}/.run-pids"

# Defaults
DEFAULT_DB_CONNECTION="Data Source=${ROOT_DIR}/.data/HCL.CS.SF_identity.db;Mode=ReadWriteCreate;Cache=Shared;"
SERVER_URL="https://localhost:5001"
ADMIN_URL="https://localhost:3001"

# Configuration (from env or defaults)
DB_CONNECTION="${HCL_CS_SF_DB_CONNECTION_STRING:-${DEFAULT_DB_CONNECTION}}"

SERVER_PID=""
ADMIN_PID=""

# ── Helpers ────────────────────────────────────────────────────────────────

usage() {
  cat <<EOF
Usage: $(basename "$0") [options]

Starts the Zentra Identity Server and the Zentra Admin MVC application.

Options:
  --db-connection <value>   DB connection string (default: SQLite)
  -h, --help                Show this help

Environment variables:
  HCL_CS_SF_DB_CONNECTION_STRING   Database connection string
EOF
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    --db-connection) DB_CONNECTION="${2:-}"; shift 2 ;;
    -h|--help) usage; exit 0 ;;
    *) echo "Unknown option: $1" >&2; usage; exit 1 ;;
  esac
done

# ── Pre-flight checks ─────────────────────────────────────────────────────

if ! command -v dotnet >/dev/null 2>&1; then
  echo "Error: dotnet SDK is not installed or not on PATH." >&2
  exit 1
fi

# Check if services are already running
if [[ -f "${PID_DIR}/zentra-server.pid" ]]; then
  OLD_PID=$(cat "${PID_DIR}/zentra-server.pid")
  if kill -0 "${OLD_PID}" 2>/dev/null; then
    echo "Zentra Server is already running (PID ${OLD_PID})."
    echo "Run scripts/stop-zentra.sh first to stop it."
    exit 1
  fi
fi

if [[ -f "${PID_DIR}/zentra-admin.pid" ]]; then
  OLD_PID=$(cat "${PID_DIR}/zentra-admin.pid")
  if kill -0 "${OLD_PID}" 2>/dev/null; then
    echo "Zentra Admin MVC is already running (PID ${OLD_PID})."
    echo "Run scripts/stop-zentra.sh first to stop it."
    exit 1
  fi
fi

mkdir -p "${ROOT_DIR}/.data" "${LOG_DIR}" "${PID_DIR}"

# ── Environment ────────────────────────────────────────────────────────────

export HCL_CS_SF_DB_CONNECTION_STRING="${DB_CONNECTION}"

# Admin MVC OAuth bindings
export OAuth__Authority="${SERVER_URL}"
export OAuth__MetadataAddress="${SERVER_URL}/.well-known/openid-configuration"
export OAuth__TokenEndpoint="${SERVER_URL}/security/token"
export OAuth__RevocationEndpoint="${SERVER_URL}/security/revocation"
export OAuth__ResourceApiBaseUrl="${SERVER_URL}"

# ── Wait helper ────────────────────────────────────────────────────────────

wait_for_url() {
  local url="$1" name="$2" max=90 attempt=1
  echo "Waiting for ${name} (${url})..."
  while (( attempt <= max )); do
    if curl -k -sS --max-time 3 -o /dev/null "${url}" 2>/dev/null; then
      echo "${name} is ready."
      return 0
    fi
    sleep 1
    attempt=$((attempt + 1))
  done
  echo "Error: ${name} did not start in time. Check logs in ${LOG_DIR}." >&2
  return 1
}

# ── Cleanup on unexpected exit ─────────────────────────────────────────────

cleanup() {
  for pid in "${SERVER_PID}" "${ADMIN_PID}"; do
    if [[ -n "${pid}" ]] && kill -0 "${pid}" 2>/dev/null; then
      kill "${pid}" 2>/dev/null || true
    fi
  done
  rm -f "${PID_DIR}/zentra-server.pid" "${PID_DIR}/zentra-admin.pid"
}

trap cleanup EXIT INT TERM

# ── Start Zentra Server ────────────────────────────────────────────────────

SERVER_LOG="${LOG_DIR}/zentra-server.log"
echo "Starting Zentra Server..."
dotnet run \
  --project "${ROOT_DIR}/demos/HCL.CS.SF.Demo.Server/HCL.CS.SF.DemoServerApp.csproj" \
  --launch-profile "HCL.CS.SF.DemoServerApp" \
  >"${SERVER_LOG}" 2>&1 &
SERVER_PID=$!
echo "${SERVER_PID}" > "${PID_DIR}/zentra-server.pid"

wait_for_url "${SERVER_URL}/.well-known/openid-configuration" "Zentra Server"

# ── Start Zentra Admin MVC ─────────────────────────────────────────────────

ADMIN_LOG="${LOG_DIR}/zentra-admin.log"
echo "Starting Zentra Admin MVC..."
dotnet run \
  --project "${ROOT_DIR}/src/Admin/HCL.CS.SF.Admin.UI/HCL.CS.SF.Admin.UI.csproj" \
  --launch-profile "https" \
  >"${ADMIN_LOG}" 2>&1 &
ADMIN_PID=$!
echo "${ADMIN_PID}" > "${PID_DIR}/zentra-admin.pid"

wait_for_url "${ADMIN_URL}" "Zentra Admin MVC"

# ── Summary ────────────────────────────────────────────────────────────────

# Remove the EXIT trap so PIDs persist (we want them for stop-zentra.sh)
trap - EXIT

echo
echo "====================================="
echo "  Zentra services are running"
echo "====================================="
echo "  Server:    ${SERVER_URL}   (PID ${SERVER_PID})"
echo "  Admin MVC: ${ADMIN_URL}   (PID ${ADMIN_PID})"
echo
echo "  Logs:"
echo "    ${SERVER_LOG}"
echo "    ${ADMIN_LOG}"
echo
echo "  To stop:  scripts/stop-zentra.sh"
echo "====================================="
echo
echo "Press Ctrl+C to stop all services."

# Re-attach trap for interactive Ctrl+C
trap cleanup INT TERM
wait
