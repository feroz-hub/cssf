import { ApiRoutes } from "@/lib/api/routes";
import { HCLCSSFPostWithSession } from "@/lib/api/client";
import {
  type AuditResponseModel,
  type AuditSearchRequestModel,
  type PagingModel
} from "@/lib/types/HCL.CS.SF";

export type AuditQueryInput = {
  actionType?: number;
  createdBy?: string;
  fromDate?: string;
  toDate?: string;
  page?: PagingModel;
  searchValue?: string;
};

export function buildAuditSearchRequest(input: AuditQueryInput): AuditSearchRequestModel {
  const page = input.page ?? {
    TotalItems: 0,
    ItemsPerPage: 20,
    CurrentPage: 1,
    TotalPages: 0,
    TotalDisplayPages: 10
  };

  const fromDate = input.fromDate?.trim();
  const toDate = input.toDate?.trim();

  return {
    ActionType: (input.actionType ?? 0) as 0 | 1 | 2 | 3,
    CreatedBy: input.createdBy ?? "",
    FromDate: fromDate ? fromDate : null,
    ToDate: toDate ? toDate : null,
    Page: page,
    CreatedOn: null,
    SearchValue: input.searchValue ?? ""
  };
}

export async function searchAudit(input: AuditQueryInput): Promise<AuditResponseModel | null> {
  const payload = buildAuditSearchRequest(input);
  const result = await HCLCSSFPostWithSession<AuditResponseModel | null, AuditSearchRequestModel>(
    ApiRoutes.audit.getAuditDetails,
    payload
  );
  return result ?? null;
}

export async function addAuditTrail(payload: unknown): Promise<unknown> {
  return HCLCSSFPostWithSession<unknown, unknown>(ApiRoutes.audit.addAuditTrail, payload);
}

export async function addAuditTrailModel(payload: unknown): Promise<unknown> {
  return HCLCSSFPostWithSession<unknown, unknown>(ApiRoutes.audit.addAuditTrailModel, payload);
}
