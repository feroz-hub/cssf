import { ClientsModule } from "@/components/modules/clients/ClientsModule";
import { getLoadErrorInfo } from "@/lib/api/client";
import { listClientScopes } from "@/lib/api/clientScopes";
import { listDetailedClients } from "@/lib/api/clients";

export default async function ClientsPage() {
  let clients: Awaited<ReturnType<typeof listDetailedClients>> = [];
  let availableScopes: string[] = [];
  let loadError: string | null = null;
  let loadErrorIsUnauthorized = false;

  try {
    const [clientsResult, scopes] = await Promise.all([listDetailedClients(), listClientScopes()]);
    clients = clientsResult;
    availableScopes = scopes;
  } catch (error) {
    const info = getLoadErrorInfo(error);
    loadError = info.message;
    loadErrorIsUnauthorized = info.isUnauthorized;
  }

  return (
    <ClientsModule
      clients={clients}
      availableScopes={availableScopes}
      loadError={loadError ?? undefined}
      loadErrorIsUnauthorized={loadErrorIsUnauthorized}
    />
  );
}
