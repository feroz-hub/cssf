# ---------------------------------------------------------------------------
# start-zentra.ps1  –  Start Zentra Server and Zentra Admin MVC (Windows)
# ---------------------------------------------------------------------------

param(
    [string]$DbConnection = ""
)

$ErrorActionPreference = "Stop"

$RootDir = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)
$LogDir  = Join-Path $RootDir ".run-logs"
$PidDir  = Join-Path $RootDir ".run-pids"

$ServerUrl = "https://localhost:5001"
$AdminUrl  = "https://localhost:3001"

# Defaults
if (-not $DbConnection) {
    $DbConnection = "Data Source=$RootDir\.data\HCL.CS.SF_identity.db;Mode=ReadWriteCreate;Cache=Shared;"
}

# ── Pre-flight checks ─────────────────────────────────────────────────────

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Error "dotnet SDK is not installed or not on PATH."
    exit 1
}

# Check if already running
foreach ($svc in @("zentra-server", "zentra-admin")) {
    $pidFile = Join-Path $PidDir "$svc.pid"
    if (Test-Path $pidFile) {
        $oldPid = Get-Content $pidFile -Raw | ForEach-Object { $_.Trim() }
        try {
            $proc = Get-Process -Id $oldPid -ErrorAction Stop
            Write-Error "$svc is already running (PID $oldPid). Run scripts\stop-zentra.ps1 first."
            exit 1
        } catch {
            # Stale PID file – remove it
            Remove-Item $pidFile -Force
        }
    }
}

# Create directories
foreach ($dir in @((Join-Path $RootDir ".data"), $LogDir, $PidDir)) {
    if (-not (Test-Path $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }
}

# ── Environment ────────────────────────────────────────────────────────────

$env:HCL_CS_SF_DB_CONNECTION_STRING = $DbConnection
$env:OAuth__Authority            = $ServerUrl
$env:OAuth__MetadataAddress      = "$ServerUrl/.well-known/openid-configuration"
$env:OAuth__TokenEndpoint        = "$ServerUrl/security/token"
$env:OAuth__RevocationEndpoint   = "$ServerUrl/security/revocation"
$env:OAuth__ResourceApiBaseUrl   = $ServerUrl

# ── Wait helper ────────────────────────────────────────────────────────────

function Wait-ForUrl {
    param([string]$Url, [string]$Name, [int]$MaxAttempts = 90)

    Write-Host "Waiting for $Name ($Url)..."

    # Bypass self-signed cert errors in dev
    if (-not ([System.Net.ServicePointManager]::ServerCertificateValidationCallback)) {
        [System.Net.ServicePointManager]::ServerCertificateValidationCallback = { $true }
    }

    for ($i = 1; $i -le $MaxAttempts; $i++) {
        try {
            $response = Invoke-WebRequest -Uri $Url -UseBasicParsing -TimeoutSec 3 `
                -SkipCertificateCheck -ErrorAction Stop
            if ($response.StatusCode -lt 500) {
                Write-Host "  $Name is ready."
                return
            }
        } catch { }
        Start-Sleep -Seconds 1
    }
    Write-Error "$Name did not start in time. Check logs in $LogDir."
    exit 1
}

# ── Start Zentra Server ────────────────────────────────────────────────────

$ServerLog = Join-Path $LogDir "zentra-server.log"
Write-Host "Starting Zentra Server..."

$serverProj = Join-Path $RootDir "demos\HCL.CS.SF.Demo.Server\HCL.CS.SF.DemoServerApp.csproj"
$serverProc = Start-Process -FilePath "dotnet" `
    -ArgumentList "run", "--project", $serverProj, "--launch-profile", "HCL.CS.SF.DemoServerApp" `
    -RedirectStandardOutput $ServerLog -RedirectStandardError (Join-Path $LogDir "zentra-server-err.log") `
    -PassThru -WindowStyle Hidden

$serverProc.Id | Out-File -FilePath (Join-Path $PidDir "zentra-server.pid") -NoNewline

Wait-ForUrl "$ServerUrl/.well-known/openid-configuration" "Zentra Server"

# ── Start Zentra Admin MVC ─────────────────────────────────────────────────

$AdminLog = Join-Path $LogDir "zentra-admin.log"
Write-Host "Starting Zentra Admin MVC..."

$adminProj = Join-Path $RootDir "src\Admin\HCL.CS.SF.Admin.UI\HCL.CS.SF.Admin.UI.csproj"
$adminProc = Start-Process -FilePath "dotnet" `
    -ArgumentList "run", "--project", $adminProj, "--launch-profile", "https" `
    -RedirectStandardOutput $AdminLog -RedirectStandardError (Join-Path $LogDir "zentra-admin-err.log") `
    -PassThru -WindowStyle Hidden

$adminProc.Id | Out-File -FilePath (Join-Path $PidDir "zentra-admin.pid") -NoNewline

Wait-ForUrl $AdminUrl "Zentra Admin MVC"

# ── Summary ────────────────────────────────────────────────────────────────

Write-Host ""
Write-Host "====================================="
Write-Host "  Zentra services are running"
Write-Host "====================================="
Write-Host "  Server:    $ServerUrl   (PID $($serverProc.Id))"
Write-Host "  Admin MVC: $AdminUrl   (PID $($adminProc.Id))"
Write-Host ""
Write-Host "  Logs:"
Write-Host "    $ServerLog"
Write-Host "    $AdminLog"
Write-Host ""
Write-Host "  To stop:  scripts\stop-zentra.ps1"
Write-Host "====================================="
