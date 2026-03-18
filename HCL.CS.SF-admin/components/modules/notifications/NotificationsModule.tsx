"use client";

import { useRouter } from "next/navigation";
import { useState, useTransition } from "react";

import {
  searchNotificationLogsAction,
  saveProviderConfigAction,
  setActiveProviderAction,
  deleteProviderConfigAction,
  sendTestNotificationAction
} from "@/app/admin/notifications/actions";
import { SignInAgainButton } from "@/components/admin/SignInAgainButton";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Select } from "@/components/ui/select";
import { useToast } from "@/components/ui/toaster";
import {
  type NotificationLogResponseModel,
  type NotificationLogModel,
  type ProviderConfigModel,
  type ProviderFieldDefinitionsResponse,
  type ProviderFieldDefinition,
  type NotificationTemplateResponseModel,
  type EmailTemplateModel,
  type SmsTemplateModel
} from "@/lib/types/HCL.CS.SF";
import { formatUtcDateTime } from "@/lib/utils";

type Tab = "providers" | "logs" | "templates" | "test";

type Props = {
  initialLogData: NotificationLogResponseModel;
  initialProviderConfigs: ProviderConfigModel[];
  fieldDefinitions: ProviderFieldDefinitionsResponse | null;
  templates: NotificationTemplateResponseModel | null;
  loadError?: string;
  loadErrorIsUnauthorized?: boolean;
};

const emptyLogData: NotificationLogResponseModel = {
  Notifications: [],
  PageInfo: {
    TotalItems: 0,
    ItemsPerPage: 20,
    CurrentPage: 1,
    TotalPages: 0,
    TotalDisplayPages: 0
  }
};

function notificationTypeLabel(type: number): string {
  if (type === 1) return "Email";
  if (type === 2) return "SMS";
  return "Unknown";
}

function notificationStatusLabel(status: number): string {
  if (status === 1) return "Initiated";
  if (status === 2) return "Delivered";
  if (status === 3) return "Failed";
  if (status === 7) return "Queued";
  if (status === 9) return "Sent";
  return "Unknown";
}

function statusBadgeClass(status: number): string {
  if (status === 2 || status === 9) return "badge badge-success";
  if (status === 3) return "badge badge-error";
  if (status === 1 || status === 7) return "badge badge-warning";
  return "badge";
}

export function NotificationsModule({
  initialLogData,
  initialProviderConfigs,
  fieldDefinitions,
  templates,
  loadError,
  loadErrorIsUnauthorized
}: Props) {
  const router = useRouter();
  const toast = useToast();
  const [isPending, startTransition] = useTransition();

  const [activeTab, setActiveTab] = useState<Tab>("providers");
  const [logData, setLogData] = useState(initialLogData);
  const [providerConfigs, setProviderConfigs] = useState(initialProviderConfigs);

  // Log filters
  const [logType, setLogType] = useState<string>("");
  const [logStatus, setLogStatus] = useState<string>("");
  const [logSearch, setLogSearch] = useState("");
  const [logFromDate, setLogFromDate] = useState("");
  const [logToDate, setLogToDate] = useState("");
  const [logPage, setLogPage] = useState(1);

  // Provider config form
  const [editingProvider, setEditingProvider] = useState<ProviderConfigModel | null>(null);
  const [showAddForm, setShowAddForm] = useState(false);
  const [addChannelType, setAddChannelType] = useState<number>(1);
  const [addProviderName, setAddProviderName] = useState("");
  const [formSettings, setFormSettings] = useState<Record<string, string>>({});
  const [formIsActive, setFormIsActive] = useState(false);

  // Test send
  const [testType, setTestType] = useState<number>(1);
  const [testRecipient, setTestRecipient] = useState("");
  const [testProviderId, setTestProviderId] = useState<string>("");

  if (loadError) {
    return (
      <section className="card">
        <div className="card-body" style={{ display: "grid", gap: "0.7rem" }}>
          <h2>Notifications</h2>
          <p className="inline-message error">{loadError}</p>
          <div className="toolbar" style={{ gap: "0.5rem" }}>
            {loadErrorIsUnauthorized ? <SignInAgainButton /> : null}
            <Button type="button" variant="secondary" onClick={() => router.refresh()}>
              Retry
            </Button>
          </div>
        </div>
      </section>
    );
  }

  // --- Log search ---
  function handleLogSearch() {
    startTransition(async () => {
      const result = await searchNotificationLogsAction({
        type: logType ? Number(logType) : null,
        status: logStatus ? Number(logStatus) : null,
        searchValue: logSearch,
        fromDate: logFromDate,
        toDate: logToDate,
        page: logPage,
        itemsPerPage: 20
      });
      if (result.ok && result.data) {
        setLogData(result.data);
      } else {
        toast.notify(result.message, "error");
      }
    });
  }

  function handleLogPageChange(newPage: number) {
    setLogPage(newPage);
    startTransition(async () => {
      const result = await searchNotificationLogsAction({
        type: logType ? Number(logType) : null,
        status: logStatus ? Number(logStatus) : null,
        searchValue: logSearch,
        fromDate: logFromDate,
        toDate: logToDate,
        page: newPage,
        itemsPerPage: 20
      });
      if (result.ok && result.data) {
        setLogData(result.data);
      }
    });
  }

  // --- Provider actions ---
  function openAddForm(channelType: number) {
    setAddChannelType(channelType);
    setAddProviderName("");
    setFormSettings({});
    setFormIsActive(false);
    setEditingProvider(null);
    setShowAddForm(true);
  }

  function openEditForm(config: ProviderConfigModel) {
    setEditingProvider(config);
    setAddChannelType(config.ChannelType);
    setAddProviderName(config.ProviderName);
    setFormSettings({ ...config.Settings });
    setFormIsActive(config.IsActive);
    setShowAddForm(true);
  }

  function cancelForm() {
    setShowAddForm(false);
    setEditingProvider(null);
  }

  function handleSaveProvider() {
    startTransition(async () => {
      const result = await saveProviderConfigAction({
        id: editingProvider?.Id ?? null,
        providerName: addProviderName,
        channelType: addChannelType,
        isActive: formIsActive,
        settings: formSettings
      });
      if (result.ok) {
        toast.notify(result.message, "success");
        cancelForm();
        router.refresh();
      } else {
        toast.notify(result.message, "error");
      }
    });
  }

  function handleSetActive(id: string) {
    startTransition(async () => {
      const result = await setActiveProviderAction({ id });
      if (result.ok) {
        toast.notify(result.message, "success");
        router.refresh();
      } else {
        toast.notify(result.message, "error");
      }
    });
  }

  function handleDeleteProvider(id: string) {
    startTransition(async () => {
      const result = await deleteProviderConfigAction({ id });
      if (result.ok) {
        toast.notify(result.message, "success");
        router.refresh();
      } else {
        toast.notify(result.message, "error");
      }
    });
  }

  // --- Test send ---
  function handleSendTest() {
    startTransition(async () => {
      const result = await sendTestNotificationAction({
        type: testType,
        recipient: testRecipient,
        providerConfigId: testProviderId || null
      });
      if (result.ok) {
        toast.notify(result.message, "success");
      } else {
        toast.notify(result.message, "error");
      }
    });
  }

  // --- CSV export ---
  function exportLogsCsv() {
    const headers = ["Timestamp", "Type", "Activity", "Sender", "Recipient", "Status", "MessageId"];
    const rows = logData.Notifications.map((n: NotificationLogModel) => [
      n.CreatedOn,
      notificationTypeLabel(n.Type),
      n.Activity,
      n.Sender,
      n.Recipient,
      notificationStatusLabel(n.Status),
      n.MessageId
    ]);
    const csv = [headers, ...rows].map((r) => r.map((v) => `"${String(v ?? "").replace(/"/g, '""')}"`).join(",")).join("\n");
    const blob = new Blob([csv], { type: "text/csv" });
    const url = URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = "notification-logs.csv";
    a.click();
    URL.revokeObjectURL(url);
  }

  // --- Field definitions helpers ---
  const emailProviderNames = fieldDefinitions ? Object.keys(fieldDefinitions.EmailProviders) : [];
  const smsProviderNames = fieldDefinitions ? Object.keys(fieldDefinitions.SmsProviders) : [];
  const emailConfigs = providerConfigs.filter((c) => c.ChannelType === 1);
  const smsConfigs = providerConfigs.filter((c) => c.ChannelType === 2);

  function getFieldDefs(): ProviderFieldDefinition[] {
    if (!fieldDefinitions || !addProviderName) return [];
    if (addChannelType === 1) return fieldDefinitions.EmailProviders[addProviderName] ?? [];
    return fieldDefinitions.SmsProviders[addProviderName] ?? [];
  }

  const availableProviders = addChannelType === 1 ? emailProviderNames : smsProviderNames;
  const currentFields = getFieldDefs();

  // --- Filtered test providers ---
  const testProviders = providerConfigs.filter((c) => c.ChannelType === testType);

  return (
    <section className="card">
      <header className="card-head">
        <h2>Notifications</h2>
      </header>

      {/* Tab bar */}
      <div className="toolbar" style={{ borderBottom: "1px solid var(--color-border)" }}>
        {(["providers", "logs", "templates", "test"] as Tab[]).map((tab) => (
          <button
            key={tab}
            type="button"
            className={activeTab === tab ? "btn btn-primary btn-sm" : "btn btn-ghost btn-sm"}
            onClick={() => setActiveTab(tab)}
          >
            {tab === "providers" ? "Providers" : tab === "logs" ? "Delivery Logs" : tab === "templates" ? "Templates" : "Send Test"}
          </button>
        ))}
      </div>

      <div className="card-body">
        {/* ======================== PROVIDERS TAB ======================== */}
        {activeTab === "providers" && (
          <div style={{ display: "grid", gap: "1.5rem" }}>
            {/* Email Providers */}
            <div>
              <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "0.75rem" }}>
                <h3>Email Providers</h3>
                <Button type="button" onClick={() => openAddForm(1)} disabled={isPending}>
                  + Add Email Provider
                </Button>
              </div>
              {emailConfigs.length === 0 ? (
                <p className="text-caption">No email providers configured. Add one to get started.</p>
              ) : (
                <div style={{ display: "grid", gap: "0.5rem" }}>
                  {emailConfigs.map((config) => (
                    <ProviderCard
                      key={config.Id}
                      config={config}
                      isPending={isPending}
                      onEdit={() => openEditForm(config)}
                      onSetActive={() => handleSetActive(config.Id!)}
                      onDelete={() => handleDeleteProvider(config.Id!)}
                    />
                  ))}
                </div>
              )}
            </div>

            {/* SMS Providers */}
            <div>
              <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "0.75rem" }}>
                <h3>SMS Providers</h3>
                <Button type="button" onClick={() => openAddForm(2)} disabled={isPending}>
                  + Add SMS Provider
                </Button>
              </div>
              {smsConfigs.length === 0 ? (
                <p className="text-caption">No SMS providers configured. Add one to get started.</p>
              ) : (
                <div style={{ display: "grid", gap: "0.5rem" }}>
                  {smsConfigs.map((config) => (
                    <ProviderCard
                      key={config.Id}
                      config={config}
                      isPending={isPending}
                      onEdit={() => openEditForm(config)}
                      onSetActive={() => handleSetActive(config.Id!)}
                      onDelete={() => handleDeleteProvider(config.Id!)}
                    />
                  ))}
                </div>
              )}
            </div>

            {/* Add/Edit Provider Form */}
            {showAddForm && (
              <div className="card" style={{ border: "1px solid var(--color-border)", padding: "1rem" }}>
                <h4 style={{ marginBottom: "0.75rem" }}>
                  {editingProvider ? `Edit ${editingProvider.ProviderName}` : `Add ${addChannelType === 1 ? "Email" : "SMS"} Provider`}
                </h4>
                <div className="form-grid" style={{ display: "grid", gap: "0.75rem" }}>
                  {!editingProvider && (
                    <div className="form-row">
                      <label>Provider</label>
                      <Select
                        value={addProviderName}
                        onChange={(e) => {
                          setAddProviderName(e.target.value);
                          setFormSettings({});
                        }}
                      >
                        <option value="">Select provider...</option>
                        {availableProviders.map((name) => (
                          <option key={name} value={name}>
                            {name}
                          </option>
                        ))}
                      </Select>
                    </div>
                  )}

                  {currentFields.map((field) => (
                    <div className="form-row" key={field.Key}>
                      <label>
                        {field.Label}
                        {field.Required && <span style={{ color: "var(--color-error)" }}> *</span>}
                      </label>
                      {field.InputType === "boolean" ? (
                        <input
                          type="checkbox"
                          checked={formSettings[field.Key] === "true"}
                          onChange={(e) =>
                            setFormSettings((s) => ({ ...s, [field.Key]: String(e.target.checked) }))
                          }
                        />
                      ) : (
                        <Input
                          type={field.InputType === "password" ? "password" : field.InputType === "number" ? "number" : field.InputType === "email" ? "email" : "text"}
                          value={formSettings[field.Key] ?? ""}
                          onChange={(e) =>
                            setFormSettings((s) => ({ ...s, [field.Key]: e.target.value }))
                          }
                          placeholder={field.Label}
                        />
                      )}
                    </div>
                  ))}

                  <div className="form-row">
                    <label>
                      <input
                        type="checkbox"
                        checked={formIsActive}
                        onChange={(e) => setFormIsActive(e.target.checked)}
                      />{" "}
                      Set as active provider
                    </label>
                  </div>

                  <div className="toolbar" style={{ gap: "0.5rem" }}>
                    <Button type="button" onClick={handleSaveProvider} disabled={isPending || !addProviderName}>
                      {isPending ? "Saving..." : "Save"}
                    </Button>
                    <Button type="button" onClick={cancelForm} disabled={isPending}>
                      Cancel
                    </Button>
                  </div>
                </div>
              </div>
            )}
          </div>
        )}

        {/* ======================== DELIVERY LOGS TAB ======================== */}
        {activeTab === "logs" && (
          <div style={{ display: "grid", gap: "1rem" }}>
            {/* Filters */}
            <div className="toolbar" style={{ flexWrap: "wrap", gap: "0.5rem" }}>
              <Select value={logType} onChange={(e) => setLogType(e.target.value)} style={{ minWidth: 100 }}>
                <option value="">All Types</option>
                <option value="1">Email</option>
                <option value="2">SMS</option>
              </Select>
              <Select value={logStatus} onChange={(e) => setLogStatus(e.target.value)} style={{ minWidth: 120 }}>
                <option value="">All Statuses</option>
                <option value="1">Initiated</option>
                <option value="2">Delivered</option>
                <option value="3">Failed</option>
                <option value="7">Queued</option>
                <option value="9">Sent</option>
              </Select>
              <Input
                type="date"
                value={logFromDate}
                onChange={(e) => setLogFromDate(e.target.value)}
                style={{ maxWidth: 160 }}
              />
              <Input
                type="date"
                value={logToDate}
                onChange={(e) => setLogToDate(e.target.value)}
                style={{ maxWidth: 160 }}
              />
              <Input
                type="text"
                placeholder="Search..."
                value={logSearch}
                onChange={(e) => setLogSearch(e.target.value)}
                style={{ maxWidth: 200 }}
              />
              <Button type="button" onClick={handleLogSearch} disabled={isPending}>
                {isPending ? "Searching..." : "Search"}
              </Button>
              <Button type="button" onClick={exportLogsCsv}>
                Export CSV
              </Button>
            </div>

            {/* Table */}
            <div className="table-wrap">
              <table className="table">
                <thead>
                  <tr>
                    <th>Timestamp</th>
                    <th>Type</th>
                    <th>Activity</th>
                    <th>Sender</th>
                    <th>Recipient</th>
                    <th>Status</th>
                    <th>Message ID</th>
                  </tr>
                </thead>
                <tbody>
                  {logData.Notifications.length === 0 ? (
                    <tr>
                      <td colSpan={7} style={{ textAlign: "center" }}>
                        No notification logs found.
                      </td>
                    </tr>
                  ) : (
                    logData.Notifications.map((n: NotificationLogModel) => (
                      <tr key={n.Id}>
                        <td>{formatUtcDateTime(n.CreatedOn)}</td>
                        <td>
                          <span className="badge">{notificationTypeLabel(n.Type)}</span>
                        </td>
                        <td>{n.Activity}</td>
                        <td>{n.Sender}</td>
                        <td>{n.Recipient}</td>
                        <td>
                          <span className={statusBadgeClass(n.Status)}>
                            {notificationStatusLabel(n.Status)}
                          </span>
                        </td>
                        <td style={{ fontSize: "0.8em", wordBreak: "break-all" }}>{n.MessageId}</td>
                      </tr>
                    ))
                  )}
                </tbody>
              </table>
            </div>

            {/* Pagination */}
            {logData.PageInfo && logData.PageInfo.TotalPages > 1 && (
              <div className="toolbar" style={{ justifyContent: "center", gap: "0.5rem" }}>
                <Button
                  type="button"
                  disabled={logPage <= 1 || isPending}
                  onClick={() => handleLogPageChange(logPage - 1)}
                >
                  Previous
                </Button>
                <span>
                  Page {logData.PageInfo.CurrentPage} of {logData.PageInfo.TotalPages}
                </span>
                <Button
                  type="button"
                  disabled={logPage >= logData.PageInfo.TotalPages || isPending}
                  onClick={() => handleLogPageChange(logPage + 1)}
                >
                  Next
                </Button>
              </div>
            )}
          </div>
        )}

        {/* ======================== TEMPLATES TAB ======================== */}
        {activeTab === "templates" && (
          <div style={{ display: "grid", gap: "1.5rem" }}>
            {/* Email Templates */}
            <div>
              <h3 style={{ marginBottom: "0.75rem" }}>Email Templates</h3>
              {!templates?.EmailTemplates?.length ? (
                <p className="text-caption">No email templates configured.</p>
              ) : (
                <div style={{ display: "grid", gap: "0.5rem" }}>
                  {templates.EmailTemplates.map((t: EmailTemplateModel) => (
                    <div key={t.Name} className="card" style={{ border: "1px solid var(--color-border)", padding: "0.75rem" }}>
                      <div style={{ display: "flex", justifyContent: "space-between", marginBottom: "0.5rem" }}>
                        <strong>{t.Name}</strong>
                        <span className="text-caption">
                          From: {t.FromName} &lt;{t.FromAddress}&gt;
                        </span>
                      </div>
                      <div style={{ marginBottom: "0.25rem" }}>
                        <span className="text-caption">Subject: </span>
                        {t.Subject}
                      </div>
                      {t.CC && (
                        <div style={{ marginBottom: "0.25rem" }}>
                          <span className="text-caption">CC: </span>
                          {t.CC}
                        </div>
                      )}
                      <div
                        style={{
                          background: "var(--color-surface-alt, #f5f5f5)",
                          padding: "0.5rem",
                          borderRadius: "4px",
                          fontSize: "0.85em",
                          maxHeight: "120px",
                          overflow: "auto",
                          whiteSpace: "pre-wrap"
                        }}
                      >
                        {highlightPlaceholders(t.TemplateFormat)}
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>

            {/* SMS Templates */}
            <div>
              <h3 style={{ marginBottom: "0.75rem" }}>SMS Templates</h3>
              {!templates?.SmsTemplates?.length ? (
                <p className="text-caption">No SMS templates configured.</p>
              ) : (
                <div style={{ display: "grid", gap: "0.5rem" }}>
                  {templates.SmsTemplates.map((t: SmsTemplateModel) => (
                    <div key={t.Name} className="card" style={{ border: "1px solid var(--color-border)", padding: "0.75rem" }}>
                      <strong>{t.Name}</strong>
                      <div
                        style={{
                          background: "var(--color-surface-alt, #f5f5f5)",
                          padding: "0.5rem",
                          borderRadius: "4px",
                          fontSize: "0.85em",
                          marginTop: "0.5rem",
                          whiteSpace: "pre-wrap"
                        }}
                      >
                        {highlightPlaceholders(t.TemplateFormat)}
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>
          </div>
        )}

        {/* ======================== SEND TEST TAB ======================== */}
        {activeTab === "test" && (
          <div style={{ display: "grid", gap: "1rem", maxWidth: 480 }}>
            <h3>Send Test Notification</h3>

            <div className="form-row">
              <label>Channel</label>
              <div style={{ display: "flex", gap: "1rem" }}>
                <label>
                  <input
                    type="radio"
                    name="testType"
                    value={1}
                    checked={testType === 1}
                    onChange={() => {
                      setTestType(1);
                      setTestProviderId("");
                    }}
                  />{" "}
                  Email
                </label>
                <label>
                  <input
                    type="radio"
                    name="testType"
                    value={2}
                    checked={testType === 2}
                    onChange={() => {
                      setTestType(2);
                      setTestProviderId("");
                    }}
                  />{" "}
                  SMS
                </label>
              </div>
            </div>

            <div className="form-row">
              <label>Provider</label>
              <Select
                value={testProviderId}
                onChange={(e) => setTestProviderId(e.target.value)}
              >
                <option value="">Active provider (default)</option>
                {testProviders.map((p) => (
                  <option key={p.Id} value={p.Id}>
                    {p.ProviderName} {p.IsActive ? "(Active)" : ""}
                  </option>
                ))}
              </Select>
            </div>

            <div className="form-row">
              <label>Recipient</label>
              <Input
                type={testType === 1 ? "email" : "text"}
                value={testRecipient}
                onChange={(e) => setTestRecipient(e.target.value)}
                placeholder={testType === 1 ? "email@example.com" : "+1234567890"}
              />
            </div>

            <Button
              type="button"
              onClick={handleSendTest}
              disabled={isPending || !testRecipient}
            >
              {isPending ? "Sending..." : "Send Test"}
            </Button>
          </div>
        )}
      </div>
    </section>
  );
}

function ProviderCard({
  config,
  isPending,
  onEdit,
  onSetActive,
  onDelete
}: {
  config: ProviderConfigModel;
  isPending: boolean;
  onEdit: () => void;
  onSetActive: () => void;
  onDelete: () => void;
}) {
  return (
    <div
      style={{
        display: "flex",
        justifyContent: "space-between",
        alignItems: "center",
        padding: "0.75rem",
        border: "1px solid var(--color-border)",
        borderRadius: "6px"
      }}
    >
      <div>
        <strong>{config.ProviderName}</strong>
        <span
          className={config.IsActive ? "badge badge-success" : "badge"}
          style={{ marginLeft: "0.5rem" }}
        >
          {config.IsActive ? "Active" : "Inactive"}
        </span>
        {config.LastTestedOn && (
          <span className="text-caption" style={{ marginLeft: "0.75rem" }}>
            Last tested: {formatUtcDateTime(config.LastTestedOn)}{" "}
            {config.LastTestSuccess ? "(passed)" : "(failed)"}
          </span>
        )}
      </div>
      <div className="toolbar" style={{ gap: "0.25rem" }}>
        <Button type="button" onClick={onEdit} disabled={isPending}>
          Configure
        </Button>
        {!config.IsActive && (
          <Button type="button" onClick={onSetActive} disabled={isPending}>
            Set Active
          </Button>
        )}
        <Button type="button" onClick={onDelete} disabled={isPending || config.IsActive}>
          Delete
        </Button>
      </div>
    </div>
  );
}

function highlightPlaceholders(template: string): React.ReactNode {
  if (!template) return null;
  const parts = template.split(/(\{[A-Z_]+\})/g);
  return parts.map((part, i) =>
    /^\{[A-Z_]+\}$/.test(part) ? (
      <span key={i} style={{ color: "var(--color-primary)", fontWeight: 600 }}>
        {part}
      </span>
    ) : (
      <span key={i}>{part}</span>
    )
  );
}
