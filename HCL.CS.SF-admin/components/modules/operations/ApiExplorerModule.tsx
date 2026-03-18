"use client";

import { useCallback, useEffect, useMemo, useRef, useState, useTransition } from "react";

import { callEndpointAction } from "@/app/admin/operations/api-explorer/actions";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { useToast } from "@/components/ui/toaster";
import { endpointSchemas, type FieldSchema, type EndpointSchema } from "@/lib/api/endpoint-schemas";
import { ApiRoutes } from "@/lib/api/routes";

type Endpoint = {
  group: string;
  name: string;
  path: string;
};

type FormPair = { id: string; key: string; value: string };

type FlowKey = "none" | "clientCredentials" | "authorizationCode" | "password" | "introspect" | "revocation";

function flattenRoutes(): Endpoint[] {
  const out: Endpoint[] = [];
  const top = ApiRoutes as Record<string, unknown>;
  for (const [group, value] of Object.entries(top)) {
    if (!value || typeof value !== "object") continue;
    for (const [name, path] of Object.entries(value as Record<string, unknown>)) {
      if (typeof path === "string" && path.startsWith("/")) {
        out.push({ group, name, path });
      }
    }
  }
  return out.sort((a, b) => `${a.group}.${a.name}`.localeCompare(`${b.group}.${b.name}`));
}

function groupByCategory(endpoints: Endpoint[]): Map<string, Endpoint[]> {
  const map = new Map<string, Endpoint[]>();
  for (const e of endpoints) {
    const key = e.group;
    if (!map.has(key)) map.set(key, []);
    map.get(key)!.push(e);
  }
  return map;
}

function formatGroupLabel(group: string): string {
  return group
    .replace(/([A-Z])/g, " $1")
    .replace(/^./, (s) => s.toUpperCase())
    .trim();
}

function parseValue(value: string): unknown {
  const t = value.trim();
  if (t === "") return "";
  if (t === "true") return true;
  if (t === "false") return false;
  if (t === "null") return null;
  if (/^-?\d+$/.test(t)) return parseInt(t, 10);
  if (/^-?\d*\.\d+$/.test(t)) return parseFloat(t);
  if ((t.startsWith("{") && t.endsWith("}")) || (t.startsWith("[") && t.endsWith("]"))) {
    try {
      return JSON.parse(t);
    } catch {
      return value;
    }
  }
  return value;
}

function formPairsToJson(pairs: FormPair[]): string {
  const obj: Record<string, unknown> = {};
  for (const { key, value } of pairs) {
    const k = key.trim();
    if (k === "") continue;
    obj[k] = parseValue(value);
  }
  return JSON.stringify(Object.keys(obj).length ? obj : {});
}

function jsonToFormPairs(jsonBody: string): FormPair[] {
  const id = () => crypto.randomUUID();
  const raw = jsonBody.trim();
  if (!raw || raw === '""') return [];
  try {
    const parsed = JSON.parse(raw);
    if (parsed === null || typeof parsed !== "object" || Array.isArray(parsed)) return [];
    return Object.entries(parsed).map(([key, value]) => ({
      id: id(),
      key,
      value: typeof value === "string" ? value : JSON.stringify(value)
    }));
  } catch {
    return [];
  }
}

/** Build form pairs from a schema's field definitions */
function schemaToFormPairs(schema: EndpointSchema): FormPair[] {
  return schema.fields.map((f) => ({
    id: crypto.randomUUID(),
    key: f.name,
    value: f.defaultValue ?? "",
  }));
}

/** Build a JSON template string from schema fields */
function schemaToJsonTemplate(schema: EndpointSchema): string {
  if (schema.fields.length === 0) return "{}";
  const obj: Record<string, unknown> = {};
  for (const f of schema.fields) {
    if (f.defaultValue !== undefined) {
      obj[f.name] = parseValue(f.defaultValue);
    } else if (f.type === "boolean") {
      obj[f.name] = false;
    } else if (f.type === "number") {
      obj[f.name] = 0;
    } else {
      obj[f.name] = "";
    }
  }
  return JSON.stringify(obj, null, 2);
}

const TYPE_BADGE_COLORS: Record<string, string> = {
  string: "#3b82f6",
  guid: "#8b5cf6",
  number: "#f59e0b",
  boolean: "#10b981",
  date: "#ec4899",
  enum: "#6366f1",
  object: "#64748b",
  array: "#0ea5e9",
};

function TypeBadge({ type }: { type: string }) {
  return (
    <span
      style={{
        display: "inline-block",
        padding: "0.1rem 0.4rem",
        borderRadius: 4,
        fontSize: "0.6875rem",
        fontWeight: 600,
        fontFamily: "var(--font-mono)",
        color: "#fff",
        background: TYPE_BADGE_COLORS[type] ?? "#6b7280",
        lineHeight: 1.4,
        textTransform: "uppercase",
        letterSpacing: "0.02em",
      }}
    >
      {type}
    </span>
  );
}

function SchemaFieldInput({
  field,
  value,
  onChange,
}: {
  field: FieldSchema;
  value: string;
  onChange: (v: string) => void;
}) {
  if (field.type === "boolean") {
    const checked = value === "true";
    return (
      <label style={{ display: "flex", alignItems: "center", gap: "0.5rem", cursor: "pointer" }}>
        <input
          type="checkbox"
          checked={checked}
          onChange={(e) => onChange(e.target.checked ? "true" : "false")}
          style={{ width: 16, height: 16, accentColor: "var(--accent)" }}
        />
        <span style={{ fontSize: "0.8125rem", color: "var(--text-secondary)" }}>{checked ? "true" : "false"}</span>
      </label>
    );
  }

  if (field.type === "enum" && field.enumValues) {
    return (
      <select
        value={value}
        onChange={(e) => onChange(e.target.value)}
        className="input"
        style={{ padding: "0.45rem 0.65rem", borderRadius: 8, fontSize: "0.8125rem", fontFamily: "var(--font-mono)" }}
      >
        <option value="">— select —</option>
        {field.enumValues.map((ev) => {
          const numericVal = ev.split(" - ")[0]?.trim() ?? ev;
          return (
            <option key={ev} value={numericVal}>
              {ev}
            </option>
          );
        })}
      </select>
    );
  }

  if (field.type === "date") {
    return (
      <Input
        type="datetime-local"
        value={value}
        onChange={(e) => onChange(e.target.value)}
        style={{ fontFamily: "var(--font-mono)", fontSize: "0.8125rem" }}
      />
    );
  }

  return (
    <Input
      value={value}
      onChange={(e) => onChange(e.target.value)}
      placeholder={field.placeholder ?? ""}
      style={{ fontFamily: "var(--font-mono)", fontSize: "0.8125rem" }}
    />
  );
}

function CopyButton({ text, label }: { text: string; label: string }) {
  const [copied, setCopied] = useState(false);
  const { notify } = useToast();

  const copy = useCallback(() => {
    if (!text) return;
    navigator.clipboard.writeText(text).then(
      () => {
        setCopied(true);
        notify(`${label} copied to clipboard`, "success");
        setTimeout(() => setCopied(false), 2000);
      },
      () => notify("Copy failed", "error")
    );
  }, [text, label, notify]);

  if (!text) return null;
  return (
    <Button type="button" variant="ghost" onClick={copy} disabled={copied} style={{ fontSize: "0.8125rem" }}>
      {copied ? "Copied" : "Copy"}
    </Button>
  );
}

const TAB_STYLE = {
  padding: "0.5rem 0.75rem",
  fontSize: "0.8125rem",
  border: "1px solid var(--border-default)",
  background: "var(--bg-overlay)",
  cursor: "pointer" as const
};

export function ApiExplorerModule() {
  const { notify } = useToast();
  const [pending, startTransition] = useTransition();

  const endpoints = useMemo(() => flattenRoutes(), []);
  const [base, setBase] = useState<"api" | "installer" | "demo">("api");
  const [method, setMethod] = useState<"GET" | "POST">("POST");
  const [query, setQuery] = useState("");
  const [selected, setSelected] = useState<Endpoint | null>(endpoints[0] ?? null);
  const [bodyMode, setBodyMode] = useState<"form" | "raw">("form");
  const [formPairs, setFormPairs] = useState<FormPair[]>([]);
  const [rawBody, setRawBody] = useState("{}");
  const [result, setResult] = useState<unknown>(null);
  const [status, setStatus] = useState<number | null>(null);
  const [contentType, setContentType] = useState<string | null>(null);
  const [collapsedGroups, setCollapsedGroups] = useState<Set<string>>(new Set());
  const [flow, setFlow] = useState<FlowKey>("none");

  // Track the current schema for the selected endpoint
  const currentSchema = useMemo<EndpointSchema | null>(
    () => (selected ? endpointSchemas[selected.path] ?? null : null),
    [selected]
  );

  // Track whether this is a flow-preset selection to skip auto-populate
  const flowPresetRef = useRef(false);

  // Auto-populate form when endpoint changes (unless triggered by a flow preset)
  useEffect(() => {
    if (flowPresetRef.current) {
      flowPresetRef.current = false;
      return;
    }
    if (!currentSchema) return;

    if (bodyMode === "form") {
      setFormPairs(schemaToFormPairs(currentSchema));
    } else {
      setRawBody(schemaToJsonTemplate(currentSchema));
    }
  }, [selected?.path]); // eslint-disable-line react-hooks/exhaustive-deps

  const filtered = useMemo(() => {
    const q = query.trim().toLowerCase();
    if (!q) return endpoints;
    return endpoints.filter(
      (e) =>
        e.group.toLowerCase().includes(q) ||
        e.name.toLowerCase().includes(q) ||
        e.path.toLowerCase().includes(q)
    );
  }, [endpoints, query]);

  const grouped = useMemo(() => groupByCategory(filtered), [filtered]);

  const jsonBody = useMemo(() => {
    if (bodyMode === "form") return formPairsToJson(formPairs);
    return rawBody.trim() || "{}";
  }, [bodyMode, formPairs, rawBody]);

  const execute = useCallback(() => {
    if (!selected) {
      notify("Select an endpoint first.", "error");
      return;
    }
    const bodyToSend = bodyMode === "form" ? formPairsToJson(formPairs) : rawBody.trim() || "{}";
    if (method === "POST") {
      try {
        JSON.parse(bodyToSend || '""');
      } catch {
        notify("Request body must be valid JSON.", "error");
        return;
      }
    }
    startTransition(async () => {
      const response = await callEndpointAction({
        base,
        method,
        path: selected.path,
        jsonBody: method === "POST" ? bodyToSend : ""
      });
      if (!response.ok) {
        notify(response.message, "error");
        setResult(response.data?.body ?? null);
        setStatus((response.data as { status?: number })?.status ?? null);
        setContentType((response.data as { contentType?: string })?.contentType ?? null);
        return;
      }
      notify("Request completed.", "success");
      setResult(response.data?.body ?? null);
      setStatus(response.data?.status ?? null);
      setContentType(response.data?.contentType ?? null);
    });
  }, [base, method, selected, bodyMode, formPairs, rawBody, notify]);

  useEffect(() => {
    const handler = (e: KeyboardEvent) => {
      if ((e.ctrlKey || e.metaKey) && e.key === "Enter") {
        e.preventDefault();
        execute();
      }
    };
    window.addEventListener("keydown", handler);
    return () => window.removeEventListener("keydown", handler);
  }, [execute]);

  const responseText = useMemo(() => {
    if (result == null) return "";
    return typeof result === "string" ? result : JSON.stringify(result, null, 2);
  }, [result]);

  const accessToken = useMemo(() => {
    if (!result || typeof result !== "object") return "";
    const data = result as Record<string, unknown>;
    const token = data.access_token;
    return typeof token === "string" ? token : "";
  }, [result]);

  const applyFlow = (key: FlowKey) => {
    setFlow(key);
    flowPresetRef.current = true;

    if (key === "none") {
      return;
    }

    if (key === "clientCredentials") {
      setBase("api");
      setMethod("POST");
      setBodyMode("raw");
      setSelected({ group: "endpoint", name: "token", path: ApiRoutes.endpoint.token });
      setRawBody(
        JSON.stringify(
          {
            grant_type: "client_credentials",
            client_id: "<client-id>",
            client_secret: "<client-secret>",
            scope: "HCL.CS.SF.apiresource HCL.CS.SF.client"
          },
          null,
          2
        )
      );
      return;
    }

    if (key === "authorizationCode") {
      setBase("api");
      setMethod("POST");
      setBodyMode("raw");
      setSelected({ group: "endpoint", name: "token", path: ApiRoutes.endpoint.token });
      setRawBody(
        JSON.stringify(
          {
            grant_type: "authorization_code",
            code: "<auth-code>",
            redirect_uri: "<redirect-uri>",
            client_id: "<client-id>",
            client_secret: "<client-secret>"
          },
          null,
          2
        )
      );
      return;
    }

    if (key === "password") {
      setBase("api");
      setMethod("POST");
      setBodyMode("raw");
      setSelected({
        group: "authentication",
        name: "ropValidateCredentials",
        path: ApiRoutes.authentication.ropValidateCredentials
      });
      setRawBody(
        JSON.stringify(
          {
            username: "<user-name>",
            password: "<password>",
            client_id: "<client-id>",
            client_secret: "<client-secret>",
            scope: "HCL.CS.SF.apiresource HCL.CS.SF.client"
          },
          null,
          2
        )
      );
      return;
    }

    if (key === "introspect") {
      setBase("api");
      setMethod("POST");
      setBodyMode("raw");
      setSelected({ group: "endpoint", name: "introspect", path: ApiRoutes.endpoint.introspect });
      setRawBody(
        JSON.stringify(
          {
            token: "<access-or-refresh-token>",
            token_type_hint: "access_token",
            client_id: "<client-id>",
            client_secret: "<client-secret>"
          },
          null,
          2
        )
      );
      return;
    }

    if (key === "revocation") {
      setBase("api");
      setMethod("POST");
      setBodyMode("raw");
      setSelected({ group: "endpoint", name: "revocation", path: ApiRoutes.endpoint.revocation });
      setRawBody(
        JSON.stringify(
          {
            token: "<access-or-refresh-token>",
            token_type_hint: "access_token",
            client_id: "<client-id>",
            client_secret: "<client-secret>"
          },
          null,
          2
        )
      );
    }
  };

  const toggleGroup = (group: string) => {
    setCollapsedGroups((prev) => {
      const next = new Set(prev);
      if (next.has(group)) next.delete(group);
      else next.add(group);
      return next;
    });
  };

  const addFormPair = () => {
    setFormPairs((prev) => [...prev, { id: crypto.randomUUID(), key: "", value: "" }]);
  };

  const updateFormPair = (id: string, field: "key" | "value", value: string) => {
    setFormPairs((prev) => prev.map((p) => (p.id === id ? { ...p, [field]: value } : p)));
  };

  const removeFormPair = (id: string) => {
    setFormPairs((prev) => prev.filter((p) => p.id !== id));
  };

  const switchToForm = () => {
    if (currentSchema && currentSchema.fields.length > 0) {
      // When switching to form with schema, merge current raw JSON values into schema fields
      const currentValues: Record<string, string> = {};
      try {
        const parsed = JSON.parse(bodyMode === "raw" ? rawBody : formPairsToJson(formPairs));
        if (parsed && typeof parsed === "object" && !Array.isArray(parsed)) {
          for (const [k, v] of Object.entries(parsed)) {
            currentValues[k] = typeof v === "string" ? v : JSON.stringify(v);
          }
        }
      } catch { /* ignore */ }

      const pairs = currentSchema.fields.map((f) => ({
        id: crypto.randomUUID(),
        key: f.name,
        value: currentValues[f.name] ?? f.defaultValue ?? "",
      }));
      // Add any extra keys not in schema
      for (const [k, v] of Object.entries(currentValues)) {
        if (!currentSchema.fields.some((f) => f.name === k)) {
          pairs.push({ id: crypto.randomUUID(), key: k, value: v });
        }
      }
      setFormPairs(pairs);
    } else {
      const pairs = jsonToFormPairs(bodyMode === "raw" ? rawBody : jsonBody);
      if (pairs.length > 0) setFormPairs(pairs);
    }
    setBodyMode("form");
  };

  const switchToRaw = () => {
    const str = bodyMode === "form" ? formPairsToJson(formPairs) : rawBody;
    try {
      // Pretty-print when switching to raw
      const parsed = JSON.parse(str);
      setRawBody(JSON.stringify(parsed, null, 2));
    } catch {
      setRawBody(str === '""' ? "{}" : str);
    }
    setBodyMode("raw");
  };

  const baseLabel =
    base === "api"
      ? "Gateway / API"
      : base === "installer"
        ? "Installer"
        : "Demo server";

  const statusBadgeClass =
    status !== null
      ? status >= 200 && status < 300
        ? "badge badge-success"
        : status >= 300 && status < 400
          ? "badge"
          : "badge badge-danger"
      : "";

  // Helper: find the schema field definition for a form pair key
  const getFieldSchema = (key: string): FieldSchema | undefined => {
    return currentSchema?.fields.find((f) => f.name === key);
  };

  const requiredFieldCount = currentSchema?.fields.filter((f) => f.required).length ?? 0;

  return (
    <div style={{ display: "flex", flexDirection: "column", gap: "1.25rem" }}>
      {/* Header */}
      <section className="card">
        <header className="card-head">
          <div>
            <h2 className="text-heading" style={{ margin: 0 }}>API Explorer</h2>
            <p className="text-caption" style={{ marginTop: "0.35rem" }}>
              Call any HCL.CS.SF API. Build the request with <strong>Form</strong> fields (no JSON needed) or switch to <strong>Raw JSON</strong>. Your session token is sent automatically.{" "}
              <kbd style={{ padding: "0.15rem 0.4rem", borderRadius: 4, background: "var(--bg-overlay)", border: "1px solid var(--border-default)", fontFamily: "var(--font-mono)", fontSize: "0.75rem" }}>
                Ctrl+Enter
              </kbd>{" "}
              to execute.
            </p>
          </div>
        </header>
      </section>

      {/* Toolbar */}
      <section className="card">
        <div className="card-body" style={{ display: "flex", flexWrap: "wrap", alignItems: "center", gap: "0.75rem" }}>
          <Input
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            placeholder="Search endpoints…"
            style={{ maxWidth: 280, fontFamily: "var(--font-mono)", fontSize: "0.875rem" }}
          />
          <select
            value={base}
            onChange={(e) => setBase(e.target.value as typeof base)}
            className="input"
            style={{ padding: "0.5rem 0.75rem", borderRadius: 10, minWidth: 140 }}
          >
            <option value="api">Gateway / API</option>
            <option value="installer">Installer</option>
            <option value="demo">Demo server</option>
          </select>
          <div style={{ display: "flex", flexDirection: "column", gap: "0.25rem" }}>
            <div style={{ display: "flex", borderRadius: 10, overflow: "hidden", border: "1px solid var(--border-default)" }}>
              <button
                type="button"
                onClick={() => setMethod("GET")}
                className={method === "GET" ? "btn btn-primary" : "btn btn-ghost"}
                style={{ borderRadius: 0, padding: "0.5rem 0.75rem", fontSize: "0.875rem" }}
              >
                GET
              </button>
              <button
                type="button"
                onClick={() => setMethod("POST")}
                className={method === "POST" ? "btn btn-primary" : "btn btn-ghost"}
                style={{ borderRadius: 0, padding: "0.5rem 0.75rem", fontSize: "0.875rem" }}
              >
                POST
              </button>
            </div>
            {base === "api" && (
              <span className="text-caption" style={{ fontSize: "0.7rem" }}>Gateway accepts POST only; sent as POST.</span>
            )}
          </div>
          <Button type="button" onClick={execute} disabled={pending}>
            {pending ? "Running…" : "Execute"}
          </Button>
        </div>
      </section>

      <div style={{ display: "grid", gridTemplateColumns: "minmax(0, 320px) 1fr", gap: "1.25rem" }}>
        {/* Endpoint list */}
        <div className="card" style={{ margin: 0, display: "flex", flexDirection: "column" }}>
          <div className="card-head" style={{ borderBottom: "1px solid var(--border-default)" }}>
            <h3 className="text-heading" style={{ margin: 0, fontSize: "0.875rem" }}>Endpoints ({filtered.length})</h3>
          </div>
          <div style={{ overflowY: "auto", maxHeight: 520, padding: "0.5rem", display: "flex", flexDirection: "column", gap: "0.25rem" }}>
            {Array.from(grouped.entries()).map(([group, items]) => {
              const isCollapsed = collapsedGroups.has(group);
              return (
                <div key={group} style={{ marginBottom: "0.25rem" }}>
                  <button
                    type="button"
                    onClick={() => toggleGroup(group)}
                    className="btn btn-ghost"
                    style={{ width: "100%", justifyContent: "space-between", textAlign: "left", fontSize: "0.8125rem" }}
                  >
                    <span>{formatGroupLabel(group)}</span>
                    <span style={{ opacity: 0.7 }}>{isCollapsed ? "▶" : "▼"}</span>
                  </button>
                  {!isCollapsed &&
                    items.map((e) => {
                      const active = selected?.path === e.path;
                      const hasSchema = !!endpointSchemas[e.path];
                      return (
                        <button
                          key={`${e.group}.${e.name}`}
                          type="button"
                          onClick={() => setSelected(e)}
                          className={active ? "btn btn-primary" : "btn btn-ghost"}
                          style={{
                            width: "100%",
                            textAlign: "left",
                            justifyContent: "flex-start",
                            flexDirection: "column",
                            alignItems: "flex-start",
                            padding: "0.5rem 0.75rem",
                            fontSize: "0.8125rem"
                          }}
                        >
                          <span style={{ display: "flex", alignItems: "center", gap: "0.35rem" }}>
                            <span style={{ fontWeight: 600 }}>{e.name}</span>
                            {hasSchema && (
                              <span
                                style={{
                                  width: 6,
                                  height: 6,
                                  borderRadius: "50%",
                                  background: active ? "rgba(255,255,255,0.7)" : "var(--accent)",
                                  flexShrink: 0,
                                }}
                                title="Schema available"
                              />
                            )}
                          </span>
                          <span className="text-caption" style={{ fontFamily: "var(--font-mono)", marginTop: "0.15rem" }}>{e.path}</span>
                        </button>
                      );
                    })}
                </div>
              );
            })}
          </div>
        </div>

        {/* Request & Response */}
        <div style={{ display: "flex", flexDirection: "column", gap: "1.25rem" }}>
          {/* Request */}
          <div className="card" style={{ margin: 0 }}>
            <div className="card-head" style={{ borderBottom: "1px solid var(--border-default)", flexWrap: "wrap", gap: "0.5rem" }}>
              <h3 className="text-heading" style={{ margin: 0, fontSize: "0.875rem" }}>Request</h3>
              <span className="text-caption" style={{ fontFamily: "var(--font-mono)", fontSize: "0.75rem" }}>
                {baseLabel} · {selected?.path ?? "—"}
              </span>
            </div>
            <div className="card-body" style={{ display: "grid", gap: "1rem" }}>
              {/* Schema info banner */}
              {currentSchema && (
                <div
                  style={{
                    display: "flex",
                    alignItems: "center",
                    gap: "0.75rem",
                    padding: "0.75rem 1rem",
                    borderRadius: 10,
                    background: "var(--bg-overlay)",
                    border: "1px solid var(--border-default)",
                    flexWrap: "wrap",
                  }}
                >
                  <span
                    style={{
                      padding: "0.15rem 0.5rem",
                      borderRadius: 4,
                      fontSize: "0.6875rem",
                      fontWeight: 700,
                      fontFamily: "var(--font-mono)",
                      color: "#fff",
                      background: currentSchema.method === "POST" ? "#f59e0b" : "#3b82f6",
                    }}
                  >
                    {currentSchema.method}
                  </span>
                  <span style={{ fontSize: "0.8125rem", color: "var(--text-primary)", fontWeight: 500 }}>
                    {currentSchema.description}
                  </span>
                  <span style={{ marginLeft: "auto", display: "flex", gap: "0.5rem", alignItems: "center" }}>
                    {currentSchema.fields.length > 0 ? (
                      <>
                        <span className="badge" style={{ fontSize: "0.6875rem" }}>
                          {currentSchema.fields.length} field{currentSchema.fields.length !== 1 ? "s" : ""}
                        </span>
                        {requiredFieldCount > 0 && (
                          <span className="badge badge-danger" style={{ fontSize: "0.6875rem" }}>
                            {requiredFieldCount} required
                          </span>
                        )}
                      </>
                    ) : (
                      <span className="badge" style={{ fontSize: "0.6875rem" }}>No body</span>
                    )}
                  </span>
                </div>
              )}

              {/* Request data summary */}
              <div style={{ padding: "1rem", borderRadius: 10, background: "var(--bg-overlay)", border: "1px solid var(--border-default)" }}>
                <div className="text-heading" style={{ fontSize: "0.8125rem", marginBottom: "0.75rem", color: "var(--text-primary)" }}>
                  Request data (what will be sent)
                </div>
                <table style={{ width: "100%", borderCollapse: "collapse", fontSize: "0.8125rem" }}>
                  <tbody>
                    <tr>
                      <td style={{ padding: "0.4rem 0.6rem 0.4rem 0", color: "var(--text-secondary)", verticalAlign: "top", width: "38%" }}>Method</td>
                      <td style={{ padding: "0.4rem 0", fontFamily: "var(--font-mono)", color: "var(--text-primary)" }}>{method}</td>
                    </tr>
                    <tr>
                      <td style={{ padding: "0.4rem 0.6rem 0.4rem 0", color: "var(--text-secondary)", verticalAlign: "top" }}>URL</td>
                      <td style={{ padding: "0.4rem 0", color: "var(--text-primary)" }}>
                        <span className="text-caption" style={{ display: "block", marginBottom: "0.2rem" }}>Base: {baseLabel}</span>
                        <span style={{ fontFamily: "var(--font-mono)", wordBreak: "break-all" }}>{selected?.path ?? "—"}</span>
                      </td>
                    </tr>
                    <tr>
                      <td style={{ padding: "0.4rem 0.6rem 0.4rem 0", color: "var(--text-secondary)", verticalAlign: "top" }}>Authorization</td>
                      <td style={{ padding: "0.4rem 0", color: "var(--text-primary)" }}>
                        <span style={{ fontFamily: "var(--font-mono)" }}>Bearer ••••••••</span>
                        <span className="text-caption" style={{ display: "block", marginTop: "0.2rem" }}>
                          Set automatically from your session — no action needed. Sign in to include your token.
                        </span>
                      </td>
                    </tr>
                    <tr>
                      <td style={{ padding: "0.4rem 0.6rem 0.4rem 0", color: "var(--text-secondary)", verticalAlign: "top" }}>Content-Type</td>
                      <td style={{ padding: "0.4rem 0", fontFamily: "var(--font-mono)", color: "var(--text-primary)" }}>
                        {method === "POST" ? "application/json" : "—"}
                        {method === "POST" && (
                          <span className="text-caption" style={{ display: "block", marginTop: "0.2rem" }}>Set automatically for POST.</span>
                        )}
                      </td>
                    </tr>
                    <tr>
                      <td style={{ padding: "0.4rem 0.6rem 0.4rem 0", color: "var(--text-secondary)", verticalAlign: "top" }}>X-Correlation-ID</td>
                      <td style={{ padding: "0.4rem 0", color: "var(--text-primary)" }}>
                        <span className="text-caption">New UUID generated per request.</span>
                      </td>
                    </tr>
                    <tr>
                      <td style={{ padding: "0.4rem 0.6rem 0.4rem 0", color: "var(--text-secondary)", verticalAlign: "top" }}>Body</td>
                      <td style={{ padding: "0.4rem 0", color: "var(--text-primary)" }}>
                        {method === "GET" ? (
                          <span className="text-caption">No body (GET request).</span>
                        ) : (
                          <span className="text-caption">Built from the Form or Raw JSON below.</span>
                        )}
                      </td>
                    </tr>
                  </tbody>
                </table>
              </div>

              <div className="form-row">
                <label className="form-label">Path</label>
                <div style={{ padding: "0.5rem 0.75rem", borderRadius: 10, background: "var(--bg-overlay)", fontFamily: "var(--font-mono)", fontSize: "0.8125rem", border: "1px solid var(--border-default)" }}>
                  {selected?.path ?? "—"}
                </div>
              </div>

              {method === "GET" ? (
                <p className="text-caption" style={{ margin: 0 }}>GET requests do not send a body.</p>
              ) : (
                <>
                  <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", flexWrap: "wrap", gap: "0.5rem" }}>
                    <label className="form-label" style={{ marginBottom: 0 }}>Body</label>
                    <div style={{ display: "flex", gap: 0 }}>
                      <button
                        type="button"
                        onClick={switchToForm}
                        style={{
                          ...TAB_STYLE,
                          borderRight: "none",
                          borderTopLeftRadius: 8,
                          borderBottomLeftRadius: 8,
                          background: bodyMode === "form" ? "var(--accent-dim)" : undefined,
                          color: bodyMode === "form" ? "var(--accent)" : undefined
                        }}
                      >
                        Form
                      </button>
                      <button
                        type="button"
                        onClick={switchToRaw}
                        style={{
                          ...TAB_STYLE,
                          borderTopRightRadius: 8,
                          borderBottomRightRadius: 8,
                          background: bodyMode === "raw" ? "var(--accent-dim)" : undefined,
                          color: bodyMode === "raw" ? "var(--accent)" : undefined
                        }}
                      >
                        Raw JSON
                      </button>
                    </div>
                    <CopyButton text={bodyMode === "form" ? formPairsToJson(formPairs) : rawBody} label="Request body" />
                  </div>

                  {bodyMode === "form" ? (
                    <div style={{ display: "flex", flexDirection: "column", gap: "0.5rem" }}>
                      {/* No-body message for empty schema endpoints */}
                      {currentSchema && currentSchema.fields.length === 0 && formPairs.length === 0 ? (
                        <div
                          style={{
                            padding: "1rem",
                            borderRadius: 10,
                            background: "var(--bg-overlay)",
                            border: "1px solid var(--border-default)",
                            textAlign: "center",
                          }}
                        >
                          <p className="text-caption" style={{ margin: 0, fontSize: "0.8125rem" }}>
                            This endpoint does not require a request body. Click <strong>Execute</strong> to call it.
                          </p>
                        </div>
                      ) : (
                        <>
                          {/* Schema-aware fields */}
                          {formPairs.map((p) => {
                            const fieldDef = getFieldSchema(p.key);
                            const isSchemaField = !!fieldDef;

                            if (isSchemaField && fieldDef) {
                              return (
                                <div
                                  key={p.id}
                                  style={{
                                    display: "grid",
                                    gridTemplateColumns: "minmax(120px, 0.4fr) 1fr auto",
                                    gap: "0.5rem",
                                    alignItems: "start",
                                    padding: "0.5rem 0",
                                    borderBottom: "1px solid var(--border-default)",
                                  }}
                                >
                                  {/* Label column */}
                                  <div style={{ display: "flex", flexDirection: "column", gap: "0.2rem", paddingTop: "0.45rem" }}>
                                    <div style={{ display: "flex", alignItems: "center", gap: "0.35rem", flexWrap: "wrap" }}>
                                      <span style={{ fontFamily: "var(--font-mono)", fontSize: "0.8125rem", fontWeight: 600, color: "var(--text-primary)" }}>
                                        {fieldDef.name}
                                      </span>
                                      {fieldDef.required && (
                                        <span style={{ color: "#ef4444", fontWeight: 700, fontSize: "0.875rem", lineHeight: 1 }}>*</span>
                                      )}
                                    </div>
                                    <TypeBadge type={fieldDef.type} />
                                    {fieldDef.description && (
                                      <span className="text-caption" style={{ fontSize: "0.6875rem", lineHeight: 1.3, marginTop: "0.1rem" }}>
                                        {fieldDef.description}
                                      </span>
                                    )}
                                  </div>
                                  {/* Input column */}
                                  <SchemaFieldInput
                                    field={fieldDef}
                                    value={p.value}
                                    onChange={(v) => updateFormPair(p.id, "value", v)}
                                  />
                                  {/* Remove button */}
                                  <Button type="button" variant="ghost" onClick={() => removeFormPair(p.id)} style={{ minWidth: 36, marginTop: "0.25rem" }}>
                                    ×
                                  </Button>
                                </div>
                              );
                            }

                            // Fallback: generic key-value pair (custom/extra fields)
                            return (
                              <div
                                key={p.id}
                                style={{
                                  display: "grid",
                                  gridTemplateColumns: "minmax(120px, 0.4fr) 1fr auto",
                                  gap: "0.5rem",
                                  alignItems: "center",
                                  padding: "0.5rem 0",
                                  borderBottom: "1px solid var(--border-default)",
                                }}
                              >
                                <Input
                                  value={p.key}
                                  onChange={(e) => updateFormPair(p.id, "key", e.target.value)}
                                  placeholder="Field name"
                                  style={{ fontFamily: "var(--font-mono)", fontSize: "0.8125rem" }}
                                />
                                <Input
                                  value={p.value}
                                  onChange={(e) => updateFormPair(p.id, "value", e.target.value)}
                                  placeholder="Value"
                                  style={{ fontFamily: "var(--font-mono)", fontSize: "0.8125rem" }}
                                />
                                <Button type="button" variant="ghost" onClick={() => removeFormPair(p.id)} style={{ minWidth: 36 }}>
                                  ×
                                </Button>
                              </div>
                            );
                          })}
                        </>
                      )}
                      <Button type="button" variant="secondary" onClick={addFormPair} style={{ alignSelf: "flex-start" }}>
                        + Add custom field
                      </Button>
                      <p className="text-caption" style={{ margin: 0 }}>
                        Numbers and true/false are parsed automatically. Use Raw JSON for nested objects or arrays.
                      </p>
                    </div>
                  ) : (
                    <Textarea
                      value={rawBody}
                      onChange={(e) => setRawBody(e.target.value)}
                      placeholder='{"key": "value"}'
                      style={{ minHeight: 180, fontFamily: "var(--font-mono)", fontSize: "0.8125rem" }}
                      spellCheck={false}
                    />
                  )}
                </>
              )}
            </div>
          </div>

          {/* Response */}
          <div className="card" style={{ margin: 0 }}>
            <div
              className="card-head"
              style={{ borderBottom: "1px solid var(--border-default)", flexWrap: "wrap", gap: "0.5rem" }}
            >
              <h3 className="text-heading" style={{ margin: 0, fontSize: "0.875rem" }}>Response</h3>
              <div style={{ display: "flex", alignItems: "center", gap: "0.5rem", flexWrap: "wrap" }}>
                {status !== null && <span className={statusBadgeClass}>{status}</span>}
                {contentType && (
                  <span className="text-caption" style={{ fontSize: "0.75rem" }}>{contentType}</span>
                )}
                {accessToken && <CopyButton text={accessToken} label="Access token" />}
                <CopyButton text={responseText} label="Response" />
              </div>
            </div>
            <div className="card-body">
              <pre
                style={{
                  margin: 0,
                  minHeight: 220,
                  overflow: "auto",
                  padding: "1rem",
                  borderRadius: 10,
                  background: "var(--bg-overlay)",
                  border: "1px solid var(--border-default)",
                  fontFamily: "var(--font-mono)",
                  fontSize: "0.8125rem",
                  lineHeight: 1.6,
                  whiteSpace: "pre-wrap",
                  wordBreak: "break-word"
                }}
              >
                <code style={{ background: "none", padding: 0 }}>{responseText || "No response yet. Run a request."}</code>
              </pre>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
