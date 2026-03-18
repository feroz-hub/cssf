# HCL.CS.SF Enterprise Architecture

HCL.CS.SF is organized using layered boundaries:

- `src/Identity`: identity domain, application, infrastructure, persistence, and API composition.
- `src/Gateway`: gateway proxy components.
- `src/Admin`: administrative API/UI surface (scaffolded for next phase).
- `src/SharedKernel`: cross-cutting primitives and domain safety utilities.
- `src/Contracts`: external request/response/event contracts.

## Runtime topology

- Identity runtime is currently hosted by `demos/HCL.CS.SF.Demo.Server`.
- Installer runtime is hosted by `installer/HCL.CS.SF.Installer.Mvc`.
- Gateway is currently a library package and has a container placeholder for CI/CD completeness.

## Design priorities

- Keep domain and persistence decoupled.
- Keep transport contracts separated from domain entities.
- Keep deployment assets (`docker`, `k8s`) versioned with code.
