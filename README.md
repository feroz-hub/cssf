# HCL.CS.SF

Enterprise-oriented repository structure for identity, gateway, admin, installer, demos, and tests.

## Layout

- `.github/workflows`: CI, docker build, security scan.
- `docker`: container build assets.
- `k8s`: Kubernetes deployment manifests.
- `scripts`: automation and migration scripts.
- `docs`: architecture, domain, threat, deployment, and API collection docs.
- `src`: production source code.
- `installer`: installer runtime.
- `demos`: runnable demo hosts.
- `tests`: integration/unit/architecture testing layout.

## Build

```bash
dotnet build HCL.CS.SF.sln
```

## Docker Quick Start

Run the standalone HCL.CS.SF identity server with PostgreSQL and Redis:

```bash
docker compose -f docker/docker-compose.yml up --build -d
```

Default local endpoints:

- HCL.CS.SF: `https://localhost:5180`
- PostgreSQL: `localhost:55433`
- Redis: `localhost:56380`

Default PostgreSQL credentials:

- Database: `HCL.CS.SF`
- User: `HCL.CS.SF`
- Password: `HCL.CS.SF`

Startup behavior in Docker:

- PostgreSQL starts first
- Redis starts first
- HCL.CS.SF waits for PostgreSQL to become ready
- HCL.CS.SF applies the PostgreSQL bootstrap SQL from `scripts/seed/PostgreSql/HCL.CS.SFPostgreSqlV1.sql`
- HCL.CS.SF applies every available PostgreSQL migration from `scripts/migrations/*_postgresql.sql`
- HCL.CS.SF generates a self-signed HTTPS certificate in Docker on first boot if none exists
- HCL.CS.SF then starts the identity server

Useful commands:

```bash
docker compose -f docker/docker-compose.yml logs -f HCL.CS.SF
docker compose -f docker/docker-compose.yml ps
docker compose -f docker/docker-compose.yml down
docker compose -f docker/docker-compose.yml down -v
```

Quick HTTPS check:

```bash
curl -k https://localhost:5180/.well-known/openid-configuration
```

Railway deployment:

- [`docs/RAILWAY_DEPLOYMENT.md`](docs/RAILWAY_DEPLOYMENT.md)

The default compose path starts only the identity server and its dependencies. Optional `installer` and `gateway`
services are available behind the `extras` profile:

```bash
docker compose -f docker/docker-compose.yml --profile extras up --build -d
```

Installer endpoint with the `extras` profile:

- Installer UI: `http://localhost:7039`
- Installer health: `http://localhost:7039/health`

Installer PostgreSQL connection strings:

- From the Dockerized installer to the bundled PostgreSQL service:
  `Host=postgres;Port=5432;Database=HCL.CS.SF;Username=HCL.CS.SF;Password=HCL.CS.SF;`
- From host tools such as Rider Database:
  `Host=localhost;Port=55433;Database=HCL.CS.SF;Username=HCL.CS.SF;Password=HCL.CS.SF;`

## Run demo identity host

```bash
dotnet run --project demos/HCL.CS.SF.Demo.Server/HCL.CS.SF.DemoServerApp.csproj
```
