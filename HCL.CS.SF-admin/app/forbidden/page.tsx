export default function ForbiddenPage() {
  return (
    <main className="forbidden-shell">
      <section className="forbidden-card">
        <p className="forbidden-code">403</p>
        <h1 className="text-display">Access Restricted</h1>
        <p className="text-body">
          Your session doesn&apos;t have the admin role required for this area. If you believe this is a mistake,
          contact your HCL.CS.SF administrator.
        </p>
      </section>
    </main>
  );
}
