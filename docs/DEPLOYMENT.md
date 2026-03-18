# Deployment

## Local

1. Build the solution:
   - `dotnet build HCL.CS.SF.sln`
2. Run identity runtime:
   - `dotnet run --project demos/HCL.CS.SF.Demo.Server/HCL.CS.SF.DemoServerApp.csproj`
3. Run installer:
   - `dotnet run --project installer/HCL.CS.SF.Installer.Mvc/HCL.CS.SFInstallerMVC.csproj`

## Containers

- Compose file: `docker/docker-compose.yml`
- Dockerfiles:
  - `docker/identity-api.Dockerfile`
  - `docker/admin-api.Dockerfile`
  - `docker/gateway.Dockerfile`

## Kubernetes

Apply manifests from `k8s/`:

- `configmap.yaml`
- `identity-deployment.yaml`
- `identity-service.yaml`
- `ingress.yaml`
