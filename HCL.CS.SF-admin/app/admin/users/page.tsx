import { UsersModule } from "@/components/modules/users/UsersModule";
import { getLoadErrorInfo } from "@/lib/api/client";
import { listUsers } from "@/lib/api/users";

export default async function UsersPage() {
  let rows: { id: string; userName: string; email: string; mfaType: string; enabled: boolean; lockedOut: boolean; createdAt: string | null }[] = [];
  let loadError: string | null = null;
  let loadErrorIsUnauthorized = false;

  try {
    const users = await listUsers();
    rows = users.map((user) => ({
      id: user.Id,
      userName: user.UserName,
      email: user.Email,
      mfaType: "—",
      enabled: !user.LockoutEnabled,
      lockedOut: user.LockoutEnabled,
      createdAt: user.CreatedOn ?? null
    }));
  } catch (error) {
    const info = getLoadErrorInfo(error);
    loadError = info.message;
    loadErrorIsUnauthorized = info.isUnauthorized;
  }

  return <UsersModule rows={rows} loadError={loadError ?? undefined} loadErrorIsUnauthorized={loadErrorIsUnauthorized} />;
}
