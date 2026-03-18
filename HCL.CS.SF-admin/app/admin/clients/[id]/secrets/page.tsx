import { notFound, redirect } from "next/navigation";

import { ClientSecretsModule } from "@/components/modules/clients/ClientSecretsModule";
import { HCLCSSFApiError } from "@/lib/api/client";
import { getClient } from "@/lib/api/clients";

export default async function ClientSecretsPage({ params }: { params: { id: string } }) {
  const clientId = decodeURIComponent(params.id);

  try {
    const client = await getClient(clientId);

    return (
      <ClientSecretsModule
        clientId={client.ClientId}
        clientName={client.ClientName}
        secretExpiresAt={client.ClientSecretExpiresAt}
      />
    );
  } catch (error) {
    if (error instanceof HCLCSSFApiError && error.statusCode === 401) {
      redirect("/login");
    }
    notFound();
  }
}
