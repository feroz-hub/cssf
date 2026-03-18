"use server";

import { z } from "zod";

import {
  searchNotificationLogs,
  saveProviderConfig,
  setActiveProvider,
  deleteProviderConfig,
  sendTestNotification
} from "@/lib/api/notifications";
import {
  type ActionResult,
  type NotificationLogResponseModel,
  type FrameworkResult
} from "@/lib/types/HCL.CS.SF";

const searchSchema = z.object({
  type: z.number().int().nullable().default(null),
  status: z.number().int().nullable().default(null),
  searchValue: z.string().default(""),
  fromDate: z.string().default(""),
  toDate: z.string().default(""),
  page: z.number().int().min(1).default(1),
  itemsPerPage: z.number().int().min(1).max(200).default(20)
});

export async function searchNotificationLogsAction(
  input: z.input<typeof searchSchema>
): Promise<ActionResult<NotificationLogResponseModel>> {
  const parsed = searchSchema.safeParse(input);
  if (!parsed.success) {
    return {
      ok: false,
      message: "Invalid search input.",
      errors: parsed.error.flatten().fieldErrors
    };
  }

  try {
    const result = await searchNotificationLogs({
      type: parsed.data.type,
      status: parsed.data.status,
      searchValue: parsed.data.searchValue,
      fromDate: parsed.data.fromDate || undefined,
      toDate: parsed.data.toDate || undefined,
      page: {
        TotalItems: 0,
        ItemsPerPage: parsed.data.itemsPerPage,
        CurrentPage: parsed.data.page,
        TotalPages: 0,
        TotalDisplayPages: 10
      }
    });

    const data: NotificationLogResponseModel = result ?? {
      Notifications: [],
      PageInfo: {
        TotalItems: 0,
        ItemsPerPage: parsed.data.itemsPerPage,
        CurrentPage: parsed.data.page,
        TotalPages: 0,
        TotalDisplayPages: 0
      }
    };

    return {
      ok: true,
      message: `${data.Notifications.length} notification(s) loaded.`,
      data
    };
  } catch (error) {
    return {
      ok: false,
      message: error instanceof Error ? error.message : "Notification log search failed."
    };
  }
}

const saveProviderSchema = z.object({
  id: z.string().nullable().default(null),
  providerName: z.string().min(1, "Provider name is required"),
  channelType: z.number().int().min(1).max(2),
  isActive: z.boolean().default(false),
  settings: z.record(z.string(), z.string())
});

export async function saveProviderConfigAction(
  input: z.input<typeof saveProviderSchema>
): Promise<ActionResult<FrameworkResult>> {
  const parsed = saveProviderSchema.safeParse(input);
  if (!parsed.success) {
    return {
      ok: false,
      message: "Invalid provider configuration.",
      errors: parsed.error.flatten().fieldErrors
    };
  }

  try {
    const result = await saveProviderConfig({
      Id: parsed.data.id ?? undefined,
      ProviderName: parsed.data.providerName,
      ChannelType: parsed.data.channelType,
      IsActive: parsed.data.isActive,
      Settings: parsed.data.settings
    });

    if (result && (result.Status === 0 || result.Status === "Succeeded")) {
      return { ok: true, message: "Provider configuration saved.", data: result };
    }

    const errorMsg = result?.Errors?.[0]?.Description ?? "Failed to save provider configuration.";
    return { ok: false, message: errorMsg };
  } catch (error) {
    return {
      ok: false,
      message: error instanceof Error ? error.message : "Save provider config failed."
    };
  }
}

const setActiveSchema = z.object({
  id: z.string().min(1, "Provider config ID is required")
});

export async function setActiveProviderAction(
  input: z.input<typeof setActiveSchema>
): Promise<ActionResult> {
  const parsed = setActiveSchema.safeParse(input);
  if (!parsed.success) {
    return { ok: false, message: "Invalid provider ID." };
  }

  try {
    const result = await setActiveProvider({ Id: parsed.data.id });

    if (result && (result.Status === 0 || result.Status === "Succeeded")) {
      return { ok: true, message: "Active provider updated." };
    }

    const errorMsg = result?.Errors?.[0]?.Description ?? "Failed to set active provider.";
    return { ok: false, message: errorMsg };
  } catch (error) {
    return {
      ok: false,
      message: error instanceof Error ? error.message : "Set active provider failed."
    };
  }
}

const deleteProviderSchema = z.object({
  id: z.string().min(1, "Provider config ID is required")
});

export async function deleteProviderConfigAction(
  input: z.input<typeof deleteProviderSchema>
): Promise<ActionResult> {
  const parsed = deleteProviderSchema.safeParse(input);
  if (!parsed.success) {
    return { ok: false, message: "Invalid provider ID." };
  }

  try {
    const result = await deleteProviderConfig({ Id: parsed.data.id });

    if (result && (result.Status === 0 || result.Status === "Succeeded")) {
      return { ok: true, message: "Provider configuration deleted." };
    }

    const errorMsg = result?.Errors?.[0]?.Description ?? "Failed to delete provider configuration.";
    return { ok: false, message: errorMsg };
  } catch (error) {
    return {
      ok: false,
      message: error instanceof Error ? error.message : "Delete provider config failed."
    };
  }
}

const sendTestSchema = z.object({
  type: z.number().int().min(1).max(2),
  recipient: z.string().min(1, "Recipient is required"),
  providerConfigId: z.string().nullable().default(null)
});

export async function sendTestNotificationAction(
  input: z.input<typeof sendTestSchema>
): Promise<ActionResult> {
  const parsed = sendTestSchema.safeParse(input);
  if (!parsed.success) {
    return {
      ok: false,
      message: "Invalid test notification input.",
      errors: parsed.error.flatten().fieldErrors
    };
  }

  try {
    const result = await sendTestNotification({
      Type: parsed.data.type,
      Recipient: parsed.data.recipient,
      ProviderConfigId: parsed.data.providerConfigId ?? undefined
    });

    if (result && (result.Status === 0 || result.Status === "Succeeded")) {
      return { ok: true, message: "Test notification sent successfully." };
    }

    const errorMsg = result?.Errors?.[0]?.Description ?? "Test notification failed.";
    return { ok: false, message: errorMsg };
  } catch (error) {
    return {
      ok: false,
      message: error instanceof Error ? error.message : "Send test notification failed."
    };
  }
}
