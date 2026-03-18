import { notFound, redirect } from "next/navigation";

import { UserSessionsModule } from "@/components/modules/users/UserSessionsModule";
import { HCLCSSFApiError } from "@/lib/api/client";
import { getUser, listUserActiveTokens } from "@/lib/api/users";

export default async function UserSessionsPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = await params;
  const userId = decodeURIComponent(id);

  try {
    const [user, sessions] = await Promise.all([getUser(userId), listUserActiveTokens([userId])]);

    return <UserSessionsModule userId={user.Id} userName={user.UserName} sessions={sessions} />;
  } catch (error) {
    if (error instanceof HCLCSSFApiError) {
      if (error.statusCode === 401) {
        redirect("/login");
      }
      if (error.statusCode === 404) {
        notFound();
      }
    }

    throw error;
  }
}
