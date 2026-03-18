import { IdentityResourcesModule } from "@/components/modules/identity-resources/IdentityResourcesModule";
import { getLoadErrorInfo } from "@/lib/api/client";
import { listIdentityResources } from "@/lib/api/identityResources";
import { type IdentityResourcesModel } from "@/lib/types/HCL.CS.SF";

export default async function IdentityResourcesPage() {
  let resources: IdentityResourcesModel[] = [];
  let loadError: string | null = null;
  let loadErrorIsUnauthorized = false;

  try {
    resources = await listIdentityResources();
  } catch (error) {
    const info = getLoadErrorInfo(error);
    loadError = info.message;
    loadErrorIsUnauthorized = info.isUnauthorized;
  }

  return <IdentityResourcesModule resources={resources} loadError={loadError ?? undefined} loadErrorIsUnauthorized={loadErrorIsUnauthorized} />;
}

