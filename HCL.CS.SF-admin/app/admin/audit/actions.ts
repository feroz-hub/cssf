"use server";

import { z } from "zod";

import { searchAudit } from "@/lib/api/audit";
import { type ActionResult, type AuditResponseModel } from "@/lib/types/HCL.CS.SF";

const searchSchema = z.object({
  actionType: z.number().int().min(0).max(3).default(0),
  actor: z.string().default(""),
  searchValue: z.string().default(""),
  fromDate: z.string().default(""),
  toDate: z.string().default(""),
  page: z.number().int().min(1).default(1),
  itemsPerPage: z.number().int().min(1).max(200).default(20)
});

export async function searchAuditAction(
  input: z.input<typeof searchSchema>
): Promise<ActionResult<AuditResponseModel>> {
  const parsed = searchSchema.safeParse(input);
  if (!parsed.success) {
    return {
      ok: false,
      message: "Invalid audit search input.",
      errors: parsed.error.flatten().fieldErrors
    };
  }

  try {
    const result = await searchAudit({
      actionType: parsed.data.actionType,
      createdBy: parsed.data.actor,
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

    const data: AuditResponseModel = result ?? {
      AuditList: [],
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
      message: `${data.AuditList.length} audit event(s) loaded.`,
      data
    };
  } catch (error) {
    return {
      ok: false,
      message: error instanceof Error ? error.message : "Audit search failed."
    };
  }
}
