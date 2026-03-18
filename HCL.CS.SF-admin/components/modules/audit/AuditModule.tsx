"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useMemo, useState, useTransition } from "react";

import { searchAuditAction } from "@/app/admin/audit/actions";
import { SignInAgainButton } from "@/components/admin/SignInAgainButton";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Select } from "@/components/ui/select";
import { useToast } from "@/components/ui/toaster";
import { type AuditResponseModel, type AuditTrailModel } from "@/lib/types/HCL.CS.SF";
import { formatUtcDateTime } from "@/lib/utils";

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

type Props = {
  initialData: AuditResponseModel | null;
  loadError?: string;
  loadErrorIsUnauthorized?: boolean;
};

type AuditDerivedFields = {
  subject: string;
  clientId: string;
  ipAddress: string;
  result: string;
  correlationId: string;
};

function eventTypeLabel(type: number): string {
  if (type === 1) {
    return "Create";
  }

  if (type === 2) {
    return "Update";
  }

  if (type === 3) {
    return "Delete";
  }

  return "None";
}

function toCsvValue(value: string): string {
  return `"${value.replaceAll('"', '""')}"`;
}

function normalizeDerivedValue(value: unknown): string {
  if (value === null || value === undefined) {
    return "-";
  }

  const text = String(value).trim();
  return text ? text : "-";
}

function getCaseInsensitiveValue(source: Record<string, unknown>, keys: string[]): unknown {
  for (const key of keys) {
    if (key in source) {
      return source[key];
    }
  }

  const loweredKeys = Object.keys(source).reduce<Record<string, unknown>>((acc, key) => {
    acc[key.toLowerCase()] = source[key];
    return acc;
  }, {});

  for (const key of keys) {
    const found = loweredKeys[key.toLowerCase()];
    if (found !== undefined) {
      return found;
    }
  }

  return undefined;
}

function parseJsonObject(value: string | null): Record<string, unknown> | null {
  if (!value) {
    return null;
  }

  try {
    const parsed = JSON.parse(value) as unknown;
    if (parsed && typeof parsed === "object" && !Array.isArray(parsed)) {
      return parsed as Record<string, unknown>;
    }
    return null;
  } catch {
    return null;
  }
}

function deriveAuditFields(row: AuditTrailModel): AuditDerivedFields {
  const newValue = parseJsonObject(row.NewValue);
  const oldValue = parseJsonObject(row.OldValue);
  const merged = {
    ...(oldValue ?? {}),
    ...(newValue ?? {})
  };

  return {
    subject: normalizeDerivedValue(getCaseInsensitiveValue(merged, ["subject", "subjectId", "userId", "userName"])),
    clientId: normalizeDerivedValue(getCaseInsensitiveValue(merged, ["clientId", "client_id"])),
    ipAddress: normalizeDerivedValue(getCaseInsensitiveValue(merged, ["ipAddress", "ip_address", "remoteIpAddress"])),
    result: normalizeDerivedValue(getCaseInsensitiveValue(merged, ["result", "status", "outcome"])),
    correlationId: normalizeDerivedValue(
      getCaseInsensitiveValue(merged, ["correlationId", "correlation_id", "traceId", "requestId"])
    )
  };
}

function toResultBucket(value: string): "success" | "failure" | "unknown" {
  const normalized = value.toLowerCase();
  if (normalized.includes("success")) {
    return "success";
  }

  if (normalized.includes("fail") || normalized.includes("error") || normalized.includes("denied")) {
    return "failure";
  }

  return "unknown";
}

function downloadCsv(rows: AuditTrailModel[]) {
  const headers = [
    "timestamp",
    "event_type",
    "actor",
    "subject",
    "client_id",
    "ip_address",
    "result",
    "correlation_id"
  ];

  const csvRows = rows.map((row) => {
    const derived = deriveAuditFields(row);

    return [
      row.CreatedOn,
      eventTypeLabel(row.ActionType),
      row.CreatedBy ?? "",
      derived.subject,
      derived.clientId,
      derived.ipAddress,
      derived.result,
      derived.correlationId
    ]
      .map((value) => toCsvValue(String(value)))
      .join(",");
  });

  const csv = [headers.join(","), ...csvRows].join("\n");
  const blob = new Blob([csv], { type: "text/csv;charset=utf-8" });
  const url = URL.createObjectURL(blob);
  const anchor = document.createElement("a");
  anchor.href = url;
  anchor.download = `HCL.CS.SF-audit-${new Date().toISOString()}.csv`;
  anchor.click();
  URL.revokeObjectURL(url);
}

export function AuditModule({ initialData, loadError, loadErrorIsUnauthorized }: Props) {
  const router = useRouter();
  const { notify } = useToast();

  const [data, setData] = useState<AuditResponseModel | null>(initialData ?? emptyAuditData);
  const [fromDate, setFromDate] = useState("");
  const [toDate, setToDate] = useState("");
  const [eventType, setEventType] = useState("0");
  const [actor, setActor] = useState("");
  const [result, setResult] = useState("all");
  const [searchValue, setSearchValue] = useState("");
  const [expandedId, setExpandedId] = useState<string | null>(null);
  const [pending, startTransition] = useTransition();

  const filteredRows = useMemo(() => {
    const rows = data?.AuditList ?? [];

    if (result === "all") {
      return rows;
    }

    return rows.filter((row) => toResultBucket(deriveAuditFields(row).result) === result);
  }, [data, result]);

  const runSearch = (page: number) => {
    startTransition(async () => {
      const response = await searchAuditAction({
        actionType: Number(eventType),
        actor,
        searchValue,
        fromDate,
        toDate,
        page,
        itemsPerPage: data?.PageInfo?.ItemsPerPage || 20
      });

      if (!response.ok) {
        notify(response.message, "error");
        return;
      }

      setData(response.data ?? emptyAuditData);
      notify(response.message, "success");
    });
  };

  const currentPage = data?.PageInfo?.CurrentPage ?? 1;
  const totalPages = Math.max(1, data?.PageInfo?.TotalPages ?? 1);

  return (
    <>
      {loadError ? (
        <section className="card" style={{ marginBottom: "1rem" }}>
          <div className="card-body" style={{ display: "grid", gap: "0.7rem" }}>
            <p className="inline-message error">{loadError}</p>
            <div className="toolbar" style={{ gap: "0.5rem" }}>
              {loadErrorIsUnauthorized ? <SignInAgainButton /> : null}
              <Button type="button" variant="secondary" onClick={() => router.refresh()}>
                Retry
              </Button>
            </div>
          </div>
        </section>
      ) : null}
      <section className="card">
        <header className="card-head">
          <div>
            <h2>Audit Log</h2>
            <p className="inline-message">Filter and inspect system audit events.</p>
          </div>
          <Button type="button" variant="secondary" onClick={() => downloadCsv(filteredRows)}>
            Export CSV
          </Button>
        </header>

      <div className="card-body" style={{ display: "grid", gap: "0.9rem" }}>
        <div className="form-grid" style={{ gridTemplateColumns: "repeat(auto-fit, minmax(180px, 1fr))" }}>
          <div className="form-row">
            <label>From Date</label>
            <Input type="date" value={fromDate} onChange={(event) => setFromDate(event.target.value)} />
          </div>
          <div className="form-row">
            <label>To Date</label>
            <Input type="date" value={toDate} onChange={(event) => setToDate(event.target.value)} />
          </div>
          <div className="form-row">
            <label>Event Type</label>
            <Select value={eventType} onChange={(event) => setEventType(event.target.value)}>
              <option value="0">All</option>
              <option value="1">Create</option>
              <option value="2">Update</option>
              <option value="3">Delete</option>
            </Select>
          </div>
          <div className="form-row">
            <label>Actor</label>
            <Input value={actor} onChange={(event) => setActor(event.target.value)} />
          </div>
          <div className="form-row">
            <label>Result</label>
            <Select value={result} onChange={(event) => setResult(event.target.value)}>
              <option value="all">All</option>
              <option value="success">Success</option>
              <option value="failure">Failure</option>
            </Select>
          </div>
          <div className="form-row">
            <label>Search</label>
            <Input value={searchValue} onChange={(event) => setSearchValue(event.target.value)} />
          </div>
        </div>

        <div className="toolbar">
          <Button type="button" onClick={() => runSearch(1)} disabled={pending}>
            {pending ? "Loading..." : "Apply Filters"}
          </Button>
        </div>

        <div className="table-wrap">
          <table className="table">
            <thead>
              <tr>
                <th>Timestamp</th>
                <th>Event Type</th>
                <th>Actor</th>
                <th>Subject</th>
                <th>Client ID</th>
                <th>IP Address</th>
                <th>Result</th>
                <th>Correlation ID</th>
                <th>Details</th>
              </tr>
            </thead>
            <tbody>
              {filteredRows.map((row) => {
                const derived = deriveAuditFields(row);

                return (
                  <tr key={row.Id} className="table-row">
                    <td>{formatUtcDateTime(row.CreatedOn)}</td>
                    <td>{eventTypeLabel(row.ActionType)}</td>
                    <td>{row.CreatedBy ?? "-"}</td>
                    <td>{derived.subject}</td>
                    <td>{derived.clientId}</td>
                    <td>{derived.ipAddress}</td>
                    <td>{derived.result}</td>
                    <td>{derived.correlationId}</td>
                    <td>
                      <Button
                        type="button"
                        variant="ghost"
                        onClick={() => setExpandedId(expandedId === row.Id ? null : row.Id)}
                      >
                        {expandedId === row.Id ? "Hide" : "View"}
                      </Button>
                      {expandedId === row.Id ? (
                        <pre
                          style={{
                            whiteSpace: "pre-wrap",
                            background: "#f7faf5",
                            border: "1px solid #d7e2d2",
                            borderRadius: 10,
                            padding: "0.65rem",
                            marginTop: "0.45rem"
                          }}
                        >
                          {JSON.stringify(row, null, 2)}
                        </pre>
                      ) : null}
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>

        <div className="toolbar" style={{ justifyContent: "space-between" }}>
          <span className="inline-message">Page {currentPage} of {totalPages}</span>
          <div className="toolbar">
            <Button type="button" variant="ghost" onClick={() => runSearch(Math.max(1, currentPage - 1))}>
              Previous
            </Button>
            <Button type="button" variant="ghost" onClick={() => runSearch(Math.min(totalPages, currentPage + 1))}>
              Next
            </Button>
          </div>
        </div>
      </div>
    </section>
    </>
  );
}
