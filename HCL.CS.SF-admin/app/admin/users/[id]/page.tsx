import { notFound, redirect } from "next/navigation";

import { UserDetailModule } from "@/components/modules/users/UserDetailModule";
import { HCLCSSFApiError } from "@/lib/api/client";
import { listRoles } from "@/lib/api/roles";
import { getUser, getUserRoles } from "@/lib/api/users";

export default async function UserDetailPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = await params;
  const userId = decodeURIComponent(id);

  try {
    const [user, roles, userRoles] = await Promise.all([
      getUser(userId),
      listRoles(),
      getUserRoles(userId).catch(() => [])
    ]);

    return <UserDetailModule user={user} roles={roles} userRoles={userRoles} />;
  } catch (error) {
    if (error instanceof HCLCSSFApiError) {
      if (error.statusCode === 401) {
        redirect("/login");
      }
      if (error.statusCode === 404) {
        notFound();
      }
    }

    // For any other error, surface it to the route error boundary
    // so you see the actual problem instead of a generic 404.
    throw error;
  }
}
