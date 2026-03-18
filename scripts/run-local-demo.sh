#!/usr/bin/env bash

set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
LOG_DIR="${ROOT_DIR}/.run-logs"

DEFAULT_DB_CONNECTION="Data Source=${ROOT_DIR}/.data/HCL.CS.SF_identity.db;Mode=ReadWriteCreate;Cache=Shared;"
DEFAULT_AUTHORITY="https://localhost:5001"
DEFAULT_CLIENT_URL="https://localhost:5003"

CLIENT_ID="${HCL.CS.SF_OAUTH_CLIENT_ID:-}"
CLIENT_SECRET="${HCL.CS.SF_OAUTH_CLIENT_SECRET:-}"
DB_CONNECTION="${HCL.CS.SF_DB_CONNECTION_STRING:-${DEFAULT_DB_CONNECTION}}"
START_INSTALLER=0

SERVER_PID=""
CLIENT_PID=""
INSTALLER_PID=""

usage() {
  cat <<EOF
Usage:
  $(basename "$0") [options]

Options:
  --client-id <value>         OAuth client id (required if HCL.CS.SF_OAUTH_CLIENT_ID is not set)
  --client-secret <value>     OAuth client secret (required if HCL.CS.SF_OAUTH_CLIENT_SECRET is not set)
  --db-connection <value>     DB connection string (default: SQLite in .data/HCL.CS.SF_identity.db)
  --start-installer           Also start installer UI at https://localhost:7039
  -h, --help                  Show this help

Environment alternatives:
  HCL.CS.SF_OAUTH_CLIENT_ID
  HCL.CS.SF_OAUTH_CLIENT_SECRET
  HCL.CS.SF_DB_CONNECTION_STRING

Examples:
  $(basename "$0") --client-id "..." --client-secret "..."
  HCL.CS.SF_OAUTH_CLIENT_ID="..." HCL.CS.SF_OAUTH_CLIENT_SECRET="..." $(basename "$0")
EOF
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    --client-id)
      CLIENT_ID="${2:-}"
      shift 2
      ;;
    --client-secret)
      CLIENT_SECRET="${2:-}"
      shift 2
      ;;
    --db-connection)
      DB_CONNECTION="${2:-}"
      shift 2
      ;;
    --start-installer)
      START_INSTALLER=1
      shift
      ;;
    -h|--help)
      usage
      exit 0
      ;;
    *)
      echo "Unknown option: $1" >&2
      usage
      exit 1
      ;;
  esac
done

if [[ -z "${CLIENT_ID}" || -z "${CLIENT_SECRET}" ]]; then
  echo "Error: client id/secret are required." >&2
  echo "Set --client-id/--client-secret or export HCL.CS.SF_OAUTH_CLIENT_ID/HCL.CS.SF_OAUTH_CLIENT_SECRET." >&2
  exit 1
fi

if ! command -v dotnet >/dev/null 2>&1; then
  echo "Error: dotnet SDK is not installed or not on PATH." >&2
  exit 1
fi

if ! command -v curl >/dev/null 2>&1; then
  echo "Error: curl is required but not found on PATH." >&2
  exit 1
fi

mkdir -p "${ROOT_DIR}/.data" "${LOG_DIR}"

export HCL.CS.SF_DB_CONNECTION_STRING="${DB_CONNECTION}"
export HCL.CS.SF_OAUTH_CLIENT_ID="${CLIENT_ID}"
export HCL.CS.SF_OAUTH_CLIENT_SECRET="${CLIENT_SECRET}"

# Needed by the ASP.NET Core options binding path in Demo MVC client.
export OAuth__ClientId="${CLIENT_ID}"
export OAuth__ClientSecret="${CLIENT_SECRET}"
export OAuth__Authority="${DEFAULT_AUTHORITY}"
export OAuth__MetadataAddress="${DEFAULT_AUTHORITY}/.well-known/openid-configuration"
export OAuth__TokenEndpoint="${DEFAULT_AUTHORITY}/security/token"
export OAuth__RevocationEndpoint="${DEFAULT_AUTHORITY}/security/revocation"
export OAuth__ResourceApiBaseUrl="${DEFAULT_AUTHORITY}"

cleanup() {
  local pids=()
  [[ -n "${CLIENT_PID}" ]] && pids+=("${CLIENT_PID}")
  [[ -n "${SERVER_PID}" ]] && pids+=("${SERVER_PID}")
  [[ -n "${INSTALLER_PID}" ]] && pids+=("${INSTALLER_PID}")

  if [[ ${#pids[@]} -gt 0 ]]; then
    echo
    echo "Stopping demo processes..."
    for pid in "${pids[@]}"; do
      if kill -0 "${pid}" >/dev/null 2>&1; then
        kill "${pid}" >/dev/null 2>&1 || true
      fi
    done
    wait "${pids[@]}" 2>/dev/null || true
  fi
}

trap cleanup EXIT INT TERM

wait_for_url() {
  local url="$1"
  local name="$2"
  local max_attempts=90
  local attempt=1

  echo "Waiting for ${name} (${url})..."
  while (( attempt <= max_attempts )); do
    if curl -k -sS --max-time 3 -o /dev/null "${url}"; then
      echo "${name} is up."
      return 0
    fi
    sleep 1
    attempt=$((attempt + 1))
  done

  echo "Error: ${name} did not become ready in time. Check logs in ${LOG_DIR}." >&2
  return 1
}

start_service() {
  local name="$1"
  local project="$2"
  local profile="$3"
  local log_file="$4"

  echo "Starting ${name}..." >&2
  dotnet run --project "${project}" --launch-profile "${profile}" >"${log_file}" 2>&1 &
  echo $!
}

SERVER_LOG="${LOG_DIR}/demo-server.log"
CLIENT_LOG="${LOG_DIR}/demo-client.log"
INSTALLER_LOG="${LOG_DIR}/installer.log"

SERVER_PID="$(start_service \
  "Demo Server (Identity/Auth)" \
  "${ROOT_DIR}/demos/HCL.CS.SF.Demo.Server/HCL.CS.SF.DemoServerApp.csproj" \
  "HCL.CS.SF.DemoServerApp" \
  "${SERVER_LOG}")"

wait_for_url "${DEFAULT_AUTHORITY}/.well-known/openid-configuration" "Demo Server"

CLIENT_PID="$(start_service \
  "Demo MVC Client" \
  "${ROOT_DIR}/demos/HCL.CS.SF.Demo.Client.Mvc/HCL.CS.SF.DemoClientMvc.csproj" \
  "HCL.CS.SF.DemoClientCoreMvcApp" \
  "${CLIENT_LOG}")"

wait_for_url "${DEFAULT_CLIENT_URL}" "Demo MVC Client"

if [[ "${START_INSTALLER}" -eq 1 ]]; then
  INSTALLER_PID="$(start_service \
    "Installer MVC" \
    "${ROOT_DIR}/installer/HCL.CS.SF.Installer.Mvc/HCL.CS.SFInstallerMVC.csproj" \
    "https" \
    "${INSTALLER_LOG}")"
  wait_for_url "https://localhost:7039/health" "Installer MVC"
fi

echo
echo "Local demo is running:"
echo "  Identity/Auth Server: ${DEFAULT_AUTHORITY}"
echo "  Demo MVC Client:      ${DEFAULT_CLIENT_URL}"
if [[ "${START_INSTALLER}" -eq 1 ]]; then
  echo "  Installer MVC:        https://localhost:7039"
fi
echo
echo "Logs:"
echo "  ${SERVER_LOG}"
echo "  ${CLIENT_LOG}"
if [[ "${START_INSTALLER}" -eq 1 ]]; then
  echo "  ${INSTALLER_LOG}"
fi
echo
echo "Press Ctrl+C to stop all services."

wait
