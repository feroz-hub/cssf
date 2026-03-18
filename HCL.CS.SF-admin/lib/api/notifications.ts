import { ApiRoutes } from "@/lib/api/routes";
import { HCLCSSFPostWithSession } from "@/lib/api/client";
import {
  type NotificationLogResponseModel,
  type NotificationSearchRequestModel,
  type NotificationTemplateResponseModel,
  type ProviderConfigModel,
  type ProviderFieldDefinitionsResponse,
  type SaveProviderConfigRequest,
  type SetActiveProviderRequest,
  type DeleteProviderConfigRequest,
  type SendTestNotificationRequest,
  type FrameworkResult,
  type PagingModel
} from "@/lib/types/HCL.CS.SF";

export type NotificationLogQueryInput = {
  type?: number | null;
  status?: number | null;
  fromDate?: string;
  toDate?: string;
  searchValue?: string;
  page?: PagingModel;
};

export function buildNotificationSearchRequest(
  input: NotificationLogQueryInput
): NotificationSearchRequestModel {
  const page = input.page ?? {
    TotalItems: 0,
    ItemsPerPage: 20,
    CurrentPage: 1,
    TotalPages: 0,
    TotalDisplayPages: 10
  };

  return {
    Type: input.type ?? null,
    Status: input.status ?? null,
    FromDate: input.fromDate?.trim() || null,
    ToDate: input.toDate?.trim() || null,
    SearchValue: input.searchValue ?? "",
    Page: page
  };
}

export async function searchNotificationLogs(
  input: NotificationLogQueryInput
): Promise<NotificationLogResponseModel | null> {
  const payload = buildNotificationSearchRequest(input);
  const result = await HCLCSSFPostWithSession<
    NotificationLogResponseModel | null,
    NotificationSearchRequestModel
  >(ApiRoutes.notification.getNotificationLogs, payload);
  return result ?? null;
}

export async function getNotificationTemplates(): Promise<NotificationTemplateResponseModel | null> {
  return HCLCSSFPostWithSession<NotificationTemplateResponseModel | null, Record<string, never>>(
    ApiRoutes.notification.getNotificationTemplates,
    {}
  );
}

export async function getAllProviderConfigs(): Promise<ProviderConfigModel[]> {
  const result = await HCLCSSFPostWithSession<ProviderConfigModel[], Record<string, never>>(
    ApiRoutes.notification.getAllProviderConfigs,
    {}
  );
  return result ?? [];
}

export async function saveProviderConfig(
  request: SaveProviderConfigRequest
): Promise<FrameworkResult | null> {
  return HCLCSSFPostWithSession<FrameworkResult | null, SaveProviderConfigRequest>(
    ApiRoutes.notification.saveProviderConfig,
    request
  );
}

export async function setActiveProvider(
  request: SetActiveProviderRequest
): Promise<FrameworkResult | null> {
  return HCLCSSFPostWithSession<FrameworkResult | null, SetActiveProviderRequest>(
    ApiRoutes.notification.setActiveProvider,
    request
  );
}

export async function deleteProviderConfig(
  request: DeleteProviderConfigRequest
): Promise<FrameworkResult | null> {
  return HCLCSSFPostWithSession<FrameworkResult | null, DeleteProviderConfigRequest>(
    ApiRoutes.notification.deleteProviderConfig,
    request
  );
}

export async function getProviderFieldDefinitions(): Promise<ProviderFieldDefinitionsResponse | null> {
  return HCLCSSFPostWithSession<ProviderFieldDefinitionsResponse | null, Record<string, never>>(
    ApiRoutes.notification.getProviderFieldDefinitions,
    {}
  );
}

export async function sendTestNotification(
  request: SendTestNotificationRequest
): Promise<FrameworkResult | null> {
  return HCLCSSFPostWithSession<FrameworkResult | null, SendTestNotificationRequest>(
    ApiRoutes.notification.sendTestNotification,
    request
  );
}
