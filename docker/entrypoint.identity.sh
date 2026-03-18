#!/usr/bin/env bash
set -euo pipefail

APP_DIR="/app"
DB_CONNECTION_STRING="${HCL.CS.SF_DB_CONNECTION_STRING:-}"
PSQL_CONNECTION_STRING=""
SEED_SCRIPT="${APP_DIR}/scripts/seed/PostgreSql/HCL.CS.SFPostgreSqlV1.sql"
MIGRATIONS_DIR="${APP_DIR}/scripts/migrations"
HTTPS_DIR="${HCL.CS.SF_HTTPS_DIR:-${APP_DIR}/https}"
HTTPS_CERT_PATH="${HCL.CS.SF_HTTPS_CERT_PATH:-${HTTPS_DIR}/HCL.CS.SF-devcert.pfx}"
HTTPS_CERT_PASSWORD="${HCL.CS.SF_HTTPS_CERT_PASSWORD:-HCL.CS.SF-dev-cert}"
HTTPS_CERT_PEM="${HTTPS_DIR}/HCL.CS.SF-devcert.crt"
HTTPS_KEY_PEM="${HTTPS_DIR}/HCL.CS.SF-devcert.key"
ASPNETCORE_URLS_VALUE="${ASPNETCORE_URLS:-}"

log() {
  printf '[HCL.CS.SF-entrypoint] %s\n' "$*"
}

configure_binding_url() {
  if [[ -n "${PORT:-}" ]] && [[ -z "${ASPNETCORE_URLS_VALUE}" || "${ASPNETCORE_URLS_VALUE}" == "https://+:8443" ]]; then
    ASPNETCORE_URLS_VALUE="http://+:${PORT}"
    export ASPNETCORE_URLS="${ASPNETCORE_URLS_VALUE}"
    log "Detected Railway-style PORT=${PORT}; binding HCL.CS.SF over internal HTTP."
  fi
}

build_psql_connection_string() {
  if [[ -z "${DB_CONNECTION_STRING}" ]]; then
    return 0
  fi

  local conn_parts=()
  local raw_parts=()
  local part=""

  IFS=';' read -r -a raw_parts <<< "${DB_CONNECTION_STRING}"
  for part in "${raw_parts[@]}"; do
    [[ -n "${part}" ]] || continue

    local key="${part%%=*}"
    local value="${part#*=}"

    key="$(printf '%s' "${key}" | xargs)"
    value="$(printf '%s' "${value}" | xargs)"

    case "${key,,}" in
      host)
        conn_parts+=("host=${value}")
        ;;
      port)
        conn_parts+=("port=${value}")
        ;;
      database)
        conn_parts+=("dbname=${value}")
        ;;
      username|user|user\ id|userid)
        conn_parts+=("user=${value}")
        ;;
      password)
        conn_parts+=("password=${value}")
        ;;
      sslmode)
        conn_parts+=("sslmode=${value}")
        ;;
    esac
  done

  PSQL_CONNECTION_STRING="${conn_parts[*]}"
}

wait_for_postgres() {
  if [[ -z "${DB_CONNECTION_STRING}" ]]; then
    log "HCL.CS.SF_DB_CONNECTION_STRING is not set. Skipping PostgreSQL bootstrap."
    return 0
  fi

  build_psql_connection_string

  if [[ -z "${PSQL_CONNECTION_STRING}" ]]; then
    if [[ "${DB_CONNECTION_STRING}" == postgres://* || "${DB_CONNECTION_STRING}" == postgresql://* ]]; then
      log "HCL.CS.SF_DB_CONNECTION_STRING is in URL format. Railway DATABASE_URL is not valid here."
      log "Use an Npgsql-style string: Host=...;Port=...;Database=...;Username=...;Password=...;SSL Mode=Require;"
    else
      log "HCL.CS.SF_DB_CONNECTION_STRING could not be parsed for PostgreSQL bootstrap."
    fi
    return 1
  fi

  local max_attempts=60
  local attempt=1

  until psql "${PSQL_CONNECTION_STRING}" -v ON_ERROR_STOP=1 -c "select 1" >/dev/null 2>&1; do
    if (( attempt >= max_attempts )); then
      log "PostgreSQL did not become ready after ${max_attempts} attempts."
      return 1
    fi

    log "Waiting for PostgreSQL (${attempt}/${max_attempts})..."
    sleep 2
    attempt=$((attempt + 1))
  done

  log "PostgreSQL is ready."
}

apply_sql_file() {
  local file_path="$1"

  if [[ ! -f "${file_path}" ]]; then
    log "Skipping missing SQL file: ${file_path}"
    return 0
  fi

  log "Applying SQL: ${file_path}"
  PGOPTIONS='-c client_min_messages=warning' \
    psql "${PSQL_CONNECTION_STRING}" -v ON_ERROR_STOP=1 -f "${file_path}" >/dev/null
}

apply_database_bootstrap() {
  if [[ -z "${DB_CONNECTION_STRING}" ]]; then
    return 0
  fi

  wait_for_postgres
  apply_sql_file "${SEED_SCRIPT}"

  if [[ -d "${MIGRATIONS_DIR}" ]]; then
    while IFS= read -r migration_file; do
      [[ -n "${migration_file}" ]] || continue
      apply_sql_file "${migration_file}"
    done < <(find "${MIGRATIONS_DIR}" -maxdepth 1 -type f -name '*_postgresql.sql' | sort)
  fi

  log "Database bootstrap and migrations completed."
}

ensure_https_certificate() {
  if [[ "${ASPNETCORE_URLS_VALUE}" != *"https://"* ]]; then
    log "ASPNETCORE_URLS does not require HTTPS. Skipping local Kestrel certificate setup."
    return 0
  fi

  local kestrel_cert_path="${ASPNETCORE_Kestrel__Certificates__Default__Path:-${HTTPS_CERT_PATH}}"
  local kestrel_cert_password="${ASPNETCORE_Kestrel__Certificates__Default__Password:-${HTTPS_CERT_PASSWORD}}"

  export ASPNETCORE_Kestrel__Certificates__Default__Path="${kestrel_cert_path}"
  export ASPNETCORE_Kestrel__Certificates__Default__Password="${kestrel_cert_password}"

  mkdir -p "$(dirname "${kestrel_cert_path}")"

  if [[ -f "${kestrel_cert_path}" ]]; then
    log "Using existing HTTPS certificate: ${kestrel_cert_path}"
    return 0
  fi

  log "Generating self-signed HTTPS certificate for HCL.CS.SF."
  openssl req \
    -x509 \
    -nodes \
    -newkey rsa:2048 \
    -keyout "${HTTPS_KEY_PEM}" \
    -out "${HTTPS_CERT_PEM}" \
    -days 365 \
    -subj "/CN=localhost" \
    -addext "subjectAltName=DNS:localhost,IP:127.0.0.1"

  openssl pkcs12 \
    -export \
    -out "${kestrel_cert_path}" \
    -inkey "${HTTPS_KEY_PEM}" \
    -in "${HTTPS_CERT_PEM}" \
    -password "pass:${kestrel_cert_password}"

  log "HTTPS certificate generated at ${kestrel_cert_path}."
}

main() {
  configure_binding_url
  apply_database_bootstrap
  ensure_https_certificate
  exec dotnet HCL.CS.SF.DemoServerApp.dll
}

main "$@"
