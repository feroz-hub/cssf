# ---------------------------------------------------------------------------
# stop-zentra.ps1  –  Stop Zentra Server & Admin MVC and clear all caches
#                      (Windows)
# ---------------------------------------------------------------------------

$ErrorActionPreference = "Stop"

$RootDir = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)
$PidDir  = Join-Path $RootDir ".run-pids"
$LogDir  = Join-Path $RootDir ".run-logs"

# ── Stop a service by PID file ─────────────────────────────────────────────

function Stop-ZentraService {
    param([string]$Name, [string]$PidFile)

    if (-not (Test-Path $PidFile)) {
        Write-Host "${Name}: no PID file found - not running via start-zentra."
        return
    }

    $pid = (Get-Content $PidFile -Raw).Trim()

    try {
        $proc = Get-Process -Id $pid -ErrorAction Stop
        Write-Host "Stopping $Name (PID $pid)..."
        Stop-Process -Id $pid -Force -ErrorAction SilentlyContinue

        # Wait up to 15 seconds for exit
        $waited = 0
        while ($waited -lt 15) {
            try {
                Get-Process -Id $pid -ErrorAction Stop | Out-Null
                Start-Sleep -Seconds 1
                $waited++
            } catch {
                break
            }
        }
        Write-Host "  $Name stopped."
    } catch {
        Write-Host "${Name}: process $pid is not running (stale PID file)."
    }

    Remove-Item $PidFile -Force -ErrorAction SilentlyContinue
}

# ── Kill by port (fallback) ────────────────────────────────────────────────

function Stop-ByPort {
    param([int]$Port, [string]$Name)

    $connections = Get-NetTCPConnection -LocalPort $Port -ErrorAction SilentlyContinue
    if ($connections) {
        $pids = $connections | Select-Object -ExpandProperty OwningProcess -Unique
        foreach ($p in $pids) {
            Write-Host "Killing $Name process on port ${Port} (PID $p)..."
            Stop-Process -Id $p -Force -ErrorAction SilentlyContinue
        }
    }
}

Write-Host "====================================="
Write-Host "  Stopping Zentra services"
Write-Host "====================================="
Write-Host ""

# 1. Stop via PID files
Stop-ZentraService "Zentra Server"    (Join-Path $PidDir "zentra-server.pid")
Stop-ZentraService "Zentra Admin MVC" (Join-Path $PidDir "zentra-admin.pid")

# 2. Fallback – kill by port
Stop-ByPort 5001 "Zentra Server"
Stop-ByPort 3001 "Zentra Admin MVC"

Write-Host ""
Write-Host "====================================="
Write-Host "  Clearing caches"
Write-Host "====================================="
Write-Host ""

# ── 1. Redis cache (Docker) ───────────────────────────────────────────────

$dockerAvailable = Get-Command docker -ErrorAction SilentlyContinue
if ($dockerAvailable) {
    $redisRunning = docker ps --format '{{.Names}}' 2>$null | Where-Object { $_ -eq "HCL.CS.SF-redis" }
    if ($redisRunning) {
        Write-Host "Flushing Redis cache (Docker container HCL.CS.SF-redis)..."
        try {
            docker exec HCL.CS.SF-redis redis-cli FLUSHALL | Out-Null
            Write-Host "  Redis cache flushed."
        } catch {
            Write-Host "  Warning: could not flush Redis."
        }
    } else {
        Write-Host "Redis: container not running - skipped."
    }
} else {
    Write-Host "Redis: Docker not found - skipped."
}

# ── 2. ASP.NET DataProtection keys ────────────────────────────────────────

$dpKeysDir = Join-Path $RootDir ".data\DataProtection-Keys"
if (Test-Path $dpKeysDir) {
    Write-Host "Clearing ASP.NET DataProtection keys..."
    Remove-Item $dpKeysDir -Recurse -Force
    Write-Host "  Done."
}

# ── 3. .NET build caches (bin/obj) ────────────────────────────────────────

Write-Host "Clearing .NET build caches (bin/obj)..."
foreach ($searchDir in @("src", "demos")) {
    $fullPath = Join-Path $RootDir $searchDir
    if (Test-Path $fullPath) {
        Get-ChildItem -Path $fullPath -Recurse -Directory -Include "bin", "obj" -ErrorAction SilentlyContinue |
            ForEach-Object { Remove-Item $_.FullName -Recurse -Force -ErrorAction SilentlyContinue }
    }
}
Write-Host "  Done."

# ── 4. Next.js cache (.next) ──────────────────────────────────────────────

$nextDir = Join-Path $RootDir "HCL.CS.SF-admin\.next"
if (Test-Path $nextDir) {
    Write-Host "Clearing Next.js cache (.next)..."
    Remove-Item $nextDir -Recurse -Force
    Write-Host "  Done."
}

# ── 5. NuGet HTTP cache ───────────────────────────────────────────────────

Write-Host "Clearing NuGet HTTP cache..."
try {
    dotnet nuget locals http-cache --clear 2>$null | Out-Null
} catch { }
Write-Host "  Done."

# ── 6. Run logs ───────────────────────────────────────────────────────────

if (Test-Path $LogDir) {
    Write-Host "Clearing run logs..."
    Remove-Item $LogDir -Recurse -Force
    Write-Host "  Done."
}

# ── 7. Temp/session files ─────────────────────────────────────────────────

$dataDir = Join-Path $RootDir ".data"
if (Test-Path $dataDir) {
    Get-ChildItem -Path $dataDir -Recurse -File -Include "*.tmp", "*.cache" -ErrorAction SilentlyContinue |
        ForEach-Object { Remove-Item $_.FullName -Force -ErrorAction SilentlyContinue }
}

Write-Host ""
Write-Host "====================================="
Write-Host "  All Zentra services stopped."
Write-Host "  All caches cleared."
Write-Host "====================================="
