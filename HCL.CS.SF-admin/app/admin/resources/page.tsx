import { ResourcesModule } from "@/components/modules/resources/ResourcesModule";
import { getLoadErrorInfo } from "@/lib/api/client";
import { listApiResources } from "@/lib/api/resources";
import { type ApiResourcesModel } from "@/lib/types/HCL.CS.SF";

export default async function ResourcesPage() {
  let resources: ApiResourcesModel[] = [];
  let loadError: string | null = null;
  let loadErrorIsUnauthorized = false;

  try {
    resources = await listApiResources();
  } catch (error) {
    const info = getLoadErrorInfo(error);
    loadError = info.message;
    loadErrorIsUnauthorized = info.isUnauthorized;
  }

  return <ResourcesModule resources={resources} loadError={loadError ?? undefined} loadErrorIsUnauthorized={loadErrorIsUnauthorized} />;
}
