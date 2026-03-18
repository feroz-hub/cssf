import { listIdentityResources } from "@/lib/api/identityResources";
import { listApiResources } from "@/lib/api/resources";

const OFFLINE_ACCESS_SCOPE = "offline_access";

export async function listClientScopes(): Promise<string[]> {
  const [apiResources, identityResources] = await Promise.all([listApiResources(), listIdentityResources()]);

  return Array.from(
    new Set([
      ...identityResources.map((resource) => resource.Name.trim()).filter(Boolean),
      OFFLINE_ACCESS_SCOPE,
      ...apiResources.flatMap((resource) =>
        [resource.Name, ...resource.ApiScopes.map((scope) => scope.Name)].map((value) => value.trim()).filter(Boolean)
      )
    ])
  ).sort((left, right) => left.localeCompare(right));
}
