# Production Runbook

## Request Tracing (Correlation ID)

- Every request carries `X-Correlation-ID`.
- If the caller does not provide one, HCL.CS.SF generates one and returns it in the response header.
- Standard request completion logs include:
  - `correlationId`
  - `tenantId`
  - `userId` (redacted when unsafe)
  - `route`
  - `statusCode`
  - `latencyMs`

### How to Trace a Request

1. Capture `X-Correlation-ID` from the client response.
2. Search service logs for `correlationId=<value>`.
3. Follow all matching entries across gateway/API/endpoint processing to reconstruct the request path.

## Health Endpoints

- Liveness: `/health/live`
  - Indicates process-level health.
- Readiness: `/health/ready`
  - Includes dependency checks:
    - Database connectivity
    - Distributed cache round-trip
  - Checks are timeout-bounded to avoid hanging probes.

Health responses return status and check durations only, with no secrets or connection details.

## Common Failure Modes

### 1) DB Unreachable / Slow

- Symptom: `/health/ready` reports database unhealthy.
- Where to look:
  - Request logs for increased `latencyMs` and non-2xx responses.
  - DB connectivity/network from app host.
  - DB saturation (connections, locks, CPU).

### 2) Cache Unavailable

- Symptom: `/health/ready` reports cache unhealthy.
- Where to look:
  - Cache service/network reachability.
  - Cache timeouts and connection limits.

### 3) Back-Channel Logout Timeouts

- Symptom: warning logs for timeout/cancellation during back-channel logout.
- Where to look:
  - Downstream client logout URI availability.
  - TLS/network failures to remote client endpoints.
  - Request cancellation patterns under high load.

### 4) High Latency / Error Spikes

- Symptom: elevated `latencyMs`, rising 5xx/4xx counts.
- Where to look:
  - Correlated request logs by route and status.
  - Dependency health (`/health/ready`).
  - Resource pressure (CPU, memory, thread pool).

## Metrics Suggestions (Vendor-Neutral)

HCL.CS.SF emits built-in .NET meter instruments when enabled (`HCL.CS.SF:Observability:EnableMetrics=true`):

- Counter: `HCL.CS.SF.http.server.requests`
- Histogram: `HCL.CS.SF.http.server.duration.ms`
- Labels:
  - `method`
  - `route` (normalized low-cardinality route group)
  - `status_code`

Recommended dashboards:

1. Request Rate and Error Ratio by route group.
2. p50/p95/p99 latency by route group.
3. 4xx vs 5xx trend.
4. Health status timeline (`/health/live`, `/health/ready`).

Recommended alerts:

1. Readiness unhealthy for 3+ consecutive intervals.
2. 5xx ratio above threshold over rolling window.
3. p95 latency sustained above SLO threshold.
