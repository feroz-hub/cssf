import { AuditModule } from "@/components/modules/audit/AuditModule";
import { getLoadErrorInfo } from "@/lib/api/client";
import { searchAudit } from "@/lib/api/audit";
import { type AuditResponseModel } from "@/lib/types/HCL.CS.SF";

const emptyAuditData: AuditResponseModel = {
  AuditList: [],
  PageInfo: {
    TotalItems: 0,
    ItemsPerPage: 20,
    CurrentPage: 1,
    TotalPages: 0,
    TotalDisplayPages: 0
  }
};

export default async function AuditPage() {
  let data: AuditResponseModel = emptyAuditData;
  let loadError: string | null = null;
  let loadErrorIsUnauthorized = false;

  try {
    const result = await searchAudit({
      actionType: 0,
      createdBy: "",
      searchValue: "",
      page: {
        TotalItems: 0,
        ItemsPerPage: 20,
        CurrentPage: 1,
        TotalPages: 0,
        TotalDisplayPages: 10
      }
    });
    data = result ?? emptyAuditData;
  } catch (error) {
    const info = getLoadErrorInfo(error);
    loadError = info.message;
    loadErrorIsUnauthorized = info.isUnauthorized;
  }

  return <AuditModule initialData={data} loadError={loadError ?? undefined} loadErrorIsUnauthorized={loadErrorIsUnauthorized} />;
}
