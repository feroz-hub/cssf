# HCL.CS.SF.ArchitectureTests

Architecture regression tests that enforce layer boundaries:

- Domain and DomainServices cannot reference Application or Infrastructure assemblies.
- Application cannot reference Persistence assemblies.
- Persistence cannot reference Application assemblies.
- API source files cannot reference `HCL.CS.SF.Infrastructure.*` outside composition root files.
- Domain source files cannot reference `HCL.CS.SF.Infrastructure.*` namespaces.
