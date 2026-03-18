#!/usr/bin/env bash
set -euo pipefail

# Applies EF Core migrations for the persistence project.
dotnet ef database update \
  --project src/Identity/HCL.CS.SF.Identity.Persistence/HCL.CS.SF.Infrastructure.Data.csproj \
  --startup-project demos/HCL.CS.SF.Demo.Server/HCL.CS.SF.DemoServerApp.csproj \
  "$@"
