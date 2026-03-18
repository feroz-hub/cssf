import { ExternalAuthModule } from "@/components/modules/external-auth/ExternalAuthModule";
import { getLoadErrorInfo } from "@/lib/api/client";
import {
  getAllExternalAuthProviders,
  getExternalAuthFieldDefinitions
} from "@/lib/api/externalAuth";
import {
  type ExternalAuthProviderConfigModel,
  type ExternalAuthFieldDefinitionsResponse
} from "@/lib/types/HCL.CS.SF";

export default async function ExternalAuthPage() {
  let providers: ExternalAuthProviderConfigModel[] = [];
  let fieldDefinitions: ExternalAuthFieldDefinitionsResponse | null = null;
  let loadError: string | null = null;
  let loadErrorIsUnauthorized = false;

  try {
    const [providersResult, fieldsResult] = await Promise.all([
      getAllExternalAuthProviders(),
      getExternalAuthFieldDefinitions()
    ]);

    providers = providersResult ?? [];
    fieldDefinitions = fieldsResult;
  } catch (error) {
    const info = getLoadErrorInfo(error);
    loadError = info.message;
    loadErrorIsUnauthorized = info.isUnauthorized;
  }

  return (
    <ExternalAuthModule
      initialProviders={providers}
      fieldDefinitions={fieldDefinitions}
      loadError={loadError ?? undefined}
      loadErrorIsUnauthorized={loadErrorIsUnauthorized}
    />
  );
}
