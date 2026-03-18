import { RolesModule } from "@/components/modules/roles/RolesModule";
import { getLoadErrorInfo } from "@/lib/api/client";
import { getRole, listRoles, listUsersInRole } from "@/lib/api/roles";
import { type RoleModel, type UserModel } from "@/lib/types/HCL.CS.SF";

export type RoleRow = {
  role: RoleModel;
  claimsCount: number;
  usersCount: number;
  usersInRole: UserModel[];
};

export default async function RolesPage() {
  let rows: RoleRow[] = [];
  let loadError: string | null = null;
  let loadErrorIsUnauthorized = false;

  try {
    const rolesRaw = await listRoles();
    const roles = Array.isArray(rolesRaw) ? rolesRaw : [];
    rows = await Promise.all(
      roles.map(async (role) => {
        const [fullRole, usersInRoleRaw] = await Promise.all([
          getRole(role.Id).catch(() => role),
          listUsersInRole(role.Name).catch(() => [])
        ]);
        const usersInRole: UserModel[] = Array.isArray(usersInRoleRaw) ? usersInRoleRaw : [];
        return {
          role: fullRole,
          claimsCount: fullRole?.RoleClaims?.length ?? 0,
          usersCount: usersInRole.length,
          usersInRole
        };
      })
    );
  } catch (error) {
    const info = getLoadErrorInfo(error);
    loadError = info.message;
    loadErrorIsUnauthorized = info.isUnauthorized;
  }

  return <RolesModule rows={rows} loadError={loadError ?? undefined} loadErrorIsUnauthorized={loadErrorIsUnauthorized} />;
}
