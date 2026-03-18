import { ApiRoutes } from "@/lib/api/routes";
import { requireAccessToken, HCLCSSFPost, HCLCSSFPostWithSession } from "@/lib/api/client";
import { type ClientsModel, type FrameworkResult } from "@/lib/types/HCL.CS.SF";

type ClientNameLookup = Record<string, string>;

export async function listClientNames(): Promise<ClientNameLookup> {
  return HCLCSSFPostWithSession<ClientNameLookup>(ApiRoutes.client.getAllClient, "");
}

export async function getClient(clientId: string): Promise<ClientsModel> {
  return HCLCSSFPostWithSession<ClientsModel, string>(ApiRoutes.client.getClient, clientId);
}

export async function createClient(model: ClientsModel): Promise<ClientsModel> {
  return HCLCSSFPostWithSession<ClientsModel, ClientsModel>(ApiRoutes.client.registerClient, model);
}

export async function updateClient(model: ClientsModel): Promise<ClientsModel> {
  return HCLCSSFPostWithSession<ClientsModel, ClientsModel>(ApiRoutes.client.updateClient, model);
}

export async function deleteClient(clientId: string): Promise<FrameworkResult> {
  return HCLCSSFPostWithSession<FrameworkResult, string>(ApiRoutes.client.deleteClient, clientId);
}

export async function rotateClientSecret(clientId: string): Promise<ClientsModel> {
  return HCLCSSFPostWithSession<ClientsModel, string>(ApiRoutes.client.generateClientSecret, clientId);
}

/**
 * Fetches all clients with full details using a single access token for the whole batch.
 * This avoids 401s caused by many concurrent auth() calls when using HCLCSSFPostWithSession per request.
 */
export async function listDetailedClients(): Promise<ClientsModel[]> {
  const accessToken = await requireAccessToken();

  const pairs = await HCLCSSFPost<ClientNameLookup>(ApiRoutes.client.getAllClient, "", accessToken);
  const clientIds = Object.keys(pairs);

  const details = await Promise.all(
    clientIds.map(async (clientId) => {
      try {
        return await HCLCSSFPost<ClientsModel, string>(ApiRoutes.client.getClient, clientId, accessToken);
      } catch {
        return null;
      }
    })
  );

  return details.filter((client): client is ClientsModel => client !== null);
}
