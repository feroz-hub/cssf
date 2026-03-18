import Link from "next/link";

import { listClientNames } from "@/lib/api/clients";
import { listRoles } from "@/lib/api/roles";
import { listUsers } from "@/lib/api/users";
import { listApiResources } from "@/lib/api/resources";
import { listIdentityResources } from "@/lib/api/identityResources";
import { searchAudit } from "@/lib/api/audit";
import { getLoadErrorInfo } from "@/lib/api/client";
import { SignInAgainButton } from "@/components/admin/SignInAgainButton";
import { DashboardClient } from "@/components/modules/dashboard/DashboardClient";
import {
  type AuditTrailModel,
  type UserDisplayModel,
  type RoleModel,
  type ApiResourcesModel,
  type IdentityResourcesModel
} from "@/lib/types/HCL.CS.SF";

export default async function AdminDashboardPage() {
  let users: UserDisplayModel[] = [];
  let roles: RoleModel[] = [];
  let clientsCount = 0;
  let apiResources: ApiResourcesModel[] = [];
  let identityResources: IdentityResourcesModel[] = [];
  let recentAudit: AuditTrailModel[] = [];
  let loadErrorMessage: string | null = null;
  let isUnauthorized = false;

  try {
    const [usersRes, rolesRes, clientNames, resourcesRes, idResourcesRes, audit] = await Promise.all([
      listUsers(),
      listRoles(),
      listClientNames(),
      listApiResources().catch(() => [] as ApiResourcesModel[]),
      listIdentityResources().catch(() => [] as IdentityResourcesModel[]),
      searchAudit({
        page: {
          TotalItems: 0,
          ItemsPerPage: 12,
          CurrentPage: 1,
          TotalPages: 0,
          TotalDisplayPages: 12
        }
      })
    ]);
    users = usersRes;
    roles = rolesRes;
    clientsCount = Object.keys(clientNames).length;
    apiResources = resourcesRes;
    identityResources = idResourcesRes;
    recentAudit = audit?.AuditList ?? [];
  } catch (error) {
    const info = getLoadErrorInfo(error);
    loadErrorMessage = info.message;
    isUnauthorized = info.isUnauthorized;
  }

  if (loadErrorMessage) {
    return (
      <section className="card">
        <header className="card-head">
          <div>
            <h2 className="text-heading">Control Center</h2>
          </div>
        </header>
        <div className="card-body" style={{ display: "grid", gap: "0.7rem" }}>
          <p className="inline-message error">{loadErrorMessage}</p>
          <div className="toolbar" style={{ gap: "0.5rem" }}>
            {isUnauthorized ? <SignInAgainButton /> : null}
            <Link href="/admin" className="btn btn-secondary">
              Retry
            </Link>
          </div>
        </div>
      </section>
    );
  }

  // Derive stats for the client component
  const totalScopes = apiResources.reduce((sum, r) => sum + (r.ApiScopes?.length ?? 0), 0);
  const totalClaims =
    apiResources.reduce((sum, r) => sum + (r.ApiResourceClaims?.length ?? 0), 0) +
    identityResources.reduce((sum, r) => sum + (r.IdentityClaims?.length ?? 0), 0);
  const enabledResources = apiResources.filter(r => r.Enabled).length;

  const mfaUsers = users.filter(u => u.LockoutEnabled).length;
  const lockedUsers = users.filter(u => u.LockoutEnd && new Date(u.LockoutEnd) > new Date()).length;

  // Audit breakdown by action type
  const auditByAction: Record<string, number> = {};
  for (const entry of recentAudit) {
    const action = entry.ActionName ?? "Unknown";
    auditByAction[action] = (auditByAction[action] ?? 0) + 1;
  }

  // Audit breakdown by table
  const auditByTable: Record<string, number> = {};
  for (const entry of recentAudit) {
    const table = entry.TableName ?? "Unknown";
    auditByTable[table] = (auditByTable[table] ?? 0) + 1;
  }

  return (
    <DashboardClient
      stats={{
        users: users.length,
        roles: roles.length,
        clients: clientsCount,
        apiResources: apiResources.length,
        identityResources: identityResources.length,
        scopes: totalScopes,
        claims: totalClaims,
        enabledResources,
        mfaUsers,
        lockedUsers,
      }}
      auditEntries={recentAudit}
      auditByAction={auditByAction}
      auditByTable={auditByTable}
      roleCounts={roles.map(r => ({ name: r.Name, claims: r.RoleClaims?.length ?? 0 }))}
    />
  );
}
