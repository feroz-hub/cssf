import { ApiRoutes } from "@/lib/api/routes";
import { HCLCSSFPostWithSession } from "@/lib/api/client";
import {
  type ExternalAuthProviderConfigModel,
  type ExternalAuthFieldDefinitionsResponse,
  type SaveExternalAuthProviderRequest,
  type DeleteExternalAuthProviderRequest,
  type TestExternalAuthProviderRequest,
  type FrameworkResult
} from "@/lib/types/HCL.CS.SF";

export async function getAllExternalAuthProviders(): Promise<ExternalAuthProviderConfigModel[]> {
  const result = await HCLCSSFPostWithSession<
    ExternalAuthProviderConfigModel[],
    Record<string, never>
  >(ApiRoutes.externalAuth.getAllProviders, {});
  return result ?? [];
}

export async function getExternalAuthProvider(
  id: string
): Promise<ExternalAuthProviderConfigModel | null> {
  return HCLCSSFPostWithSession<ExternalAuthProviderConfigModel | null, { Id: string }>(
    ApiRoutes.externalAuth.getProvider,
    { Id: id }
  );
}

export async function saveExternalAuthProvider(
  request: SaveExternalAuthProviderRequest
): Promise<FrameworkResult | null> {
  return HCLCSSFPostWithSession<FrameworkResult | null, SaveExternalAuthProviderRequest>(
    ApiRoutes.externalAuth.saveProvider,
    request
  );
}

export async function deleteExternalAuthProvider(
  request: DeleteExternalAuthProviderRequest
): Promise<FrameworkResult | null> {
  return HCLCSSFPostWithSession<FrameworkResult | null, DeleteExternalAuthProviderRequest>(
    ApiRoutes.externalAuth.deleteProvider,
    request
  );
}

export async function testExternalAuthProvider(
  request: TestExternalAuthProviderRequest
): Promise<FrameworkResult | null> {
  return HCLCSSFPostWithSession<FrameworkResult | null, TestExternalAuthProviderRequest>(
    ApiRoutes.externalAuth.testProvider,
    request
  );
}

export async function getExternalAuthFieldDefinitions(): Promise<ExternalAuthFieldDefinitionsResponse | null> {
  return HCLCSSFPostWithSession<ExternalAuthFieldDefinitionsResponse | null, Record<string, never>>(
    ApiRoutes.externalAuth.getFieldDefinitions,
    {}
  );
}
