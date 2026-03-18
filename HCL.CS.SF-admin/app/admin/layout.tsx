import { redirect } from "next/navigation";

import { Breadcrumb } from "@/components/layout/Breadcrumb";
import { Header } from "@/components/layout/Header";
import { Sidebar, SidebarMobileProvider } from "@/components/layout/Sidebar";
import { hasAdminRole } from "@/lib/auth";
import { auth } from "@/lib/auth";

export default async function AdminLayout({ children }: { children: React.ReactNode }) {
  const session = await auth();

  if (!session) {
    redirect("/login");
  }

  const roles = session.roles ?? [];
  if (!hasAdminRole(roles)) {
    return (
      <main className="forbidden-shell">
        <section className="forbidden-card">
          <p className="forbidden-code">403</p>
          <h1>Access denied</h1>
          <p>Authenticated session does not include the required role claim: admin.</p>
        </section>
      </main>
    );
  }

  return (
    <SidebarMobileProvider>
      <div className="admin-shell">
        <Sidebar />
        <div className="admin-main">
          <Header />
          <Breadcrumb />
          <section className="admin-content">{children}</section>
        </div>
      </div>
    </SidebarMobileProvider>
  );
}
