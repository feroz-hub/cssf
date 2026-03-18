"use client";

import { useRouter } from "next/navigation";
import { useState, useTransition } from "react";

import {
  saveExternalAuthProviderAction,
  deleteExternalAuthProviderAction,
  testExternalAuthProviderAction
} from "@/app/admin/external-auth/actions";
import { SignInAgainButton } from "@/components/admin/SignInAgainButton";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Select } from "@/components/ui/select";
import { useToast } from "@/components/ui/toaster";
import {
  type ExternalAuthProviderConfigModel,
  type ExternalAuthFieldDefinitionsResponse,
  type ProviderFieldDefinition
} from "@/lib/types/HCL.CS.SF";
import { formatUtcDateTime } from "@/lib/utils";

type Props = {
  initialProviders: ExternalAuthProviderConfigModel[];
  fieldDefinitions: ExternalAuthFieldDefinitionsResponse | null;
  loadError?: string;
  loadErrorIsUnauthorized?: boolean;
};

export function ExternalAuthModule({ initialProviders, fieldDefinitions, loadError, loadErrorIsUnauthorized }: Props) {
  const router = useRouter();
  const toast = useToast();
  const [isPending, startTransition] = useTransition();

  const [providers, setProviders] = useState(initialProviders);
  const [showForm, setShowForm] = useState(false);
  const [editingProvider, setEditingProvider] = useState<ExternalAuthProviderConfigModel | null>(
    null
  );

  // Form state
  const [formProviderName, setFormProviderName] = useState("");
  const [formIsEnabled, setFormIsEnabled] = useState(false);
  const [formAutoProvision, setFormAutoProvision] = useState(false);
  const [formAllowedDomains, setFormAllowedDomains] = useState("");
  const [formSettings, setFormSettings] = useState<Record<string, string>>({});
  const [confirmDeleteId, setConfirmDeleteId] = useState<string | null>(null);

  const availableProviders = fieldDefinitions
    ? Object.keys(fieldDefinitions.Providers)
    : ["Google"];
  const defaults = fieldDefinitions?.Defaults ?? {};

  if (loadError) {
    return (
      <section className="card">
        <div className="card-body" style={{ display: "grid", gap: "0.7rem" }}>
          <h2>External Auth Providers</h2>
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

  function openAddForm(providerName?: string) {
    const name = providerName ?? availableProviders[0] ?? "Google";
    setEditingProvider(null);
    setFormProviderName(name);
    setFormIsEnabled(false);
    setFormAutoProvision(false);
    setFormAllowedDomains("");
    setFormSettings(defaults[name] ? { ...defaults[name] } : {});
    setShowForm(true);
  }

  function openEditForm(provider: ExternalAuthProviderConfigModel) {
    setEditingProvider(provider);
    setFormProviderName(provider.ProviderName);
    setFormIsEnabled(provider.IsEnabled);
    setFormAutoProvision(provider.AutoProvisionEnabled);
    setFormAllowedDomains(provider.AllowedDomains ?? "");
    setFormSettings({ ...provider.Settings });
    setShowForm(true);
  }

  function cancelForm() {
    setShowForm(false);
    setEditingProvider(null);
    setFormSettings({});
  }

  function handleProviderNameChange(name: string) {
    setFormProviderName(name);
    if (!editingProvider) {
      setFormSettings(defaults[name] ? { ...defaults[name] } : {});
    }
  }

  function handleSave() {
    startTransition(async () => {
      const result = await saveExternalAuthProviderAction({
        id: editingProvider?.Id ?? null,
        providerName: formProviderName,
        providerType: 1,
        isEnabled: formIsEnabled,
        settings: formSettings,
        autoProvisionEnabled: formAutoProvision,
        allowedDomains: formAllowedDomains.trim() || null
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

  function handleDelete(id: string) {
    startTransition(async () => {
      const result = await deleteExternalAuthProviderAction({ id });
      if (result.ok) {
        toast.notify(result.message, "success");
        setConfirmDeleteId(null);
        router.refresh();
      } else {
        toast.notify(result.message, "error");
      }
    });
  }

  function handleTest(id: string) {
    startTransition(async () => {
      const result = await testExternalAuthProviderAction({ id });
      if (result.ok) {
        toast.notify(result.message, "success");
        router.refresh();
      } else {
        toast.notify(result.message, "error");
      }
    });
  }

  const currentFields: ProviderFieldDefinition[] =
    fieldDefinitions?.Providers?.[formProviderName] ?? [];

  return (
    <section className="card">
      <header className="card-head">
        <h2>External Auth Providers</h2>
        {!showForm && (
          <Button type="button" onClick={() => openAddForm()} disabled={isPending}>
            Add Provider
          </Button>
        )}
      </header>

      <div className="card-body" style={{ display: "grid", gap: "1rem" }}>
        {/* Add/Edit form */}
        {showForm && (
          <div
            style={{
              border: "1px solid var(--color-border)",
              borderRadius: 8,
              padding: "1.2rem",
              display: "grid",
              gap: "0.8rem"
            }}
          >
            <h3>{editingProvider ? "Edit Provider" : "Add Provider"}</h3>

            <label className="field-label">
              Provider
              {editingProvider ? (
                <Input value={formProviderName} disabled />
              ) : (
                <Select
                  value={formProviderName}
                  onChange={(e) => handleProviderNameChange(e.target.value)}
                >
                  {availableProviders.map((name) => (
                    <option key={name} value={name}>
                      {name}
                    </option>
                  ))}
                </Select>
              )}
            </label>

            <label className="field-label" style={{ display: "flex", alignItems: "center", gap: 8 }}>
              <input
                type="checkbox"
                checked={formIsEnabled}
                onChange={(e) => setFormIsEnabled(e.target.checked)}
              />
              Enabled
            </label>

            {/* Dynamic config fields */}
            {currentFields.map((field) => (
              <label key={field.Key} className="field-label">
                {field.Label}
                {field.Required && <span style={{ color: "var(--color-error)" }}> *</span>}
                {field.InputType === "textarea" ? (
                  <textarea
                    className="input"
                    rows={3}
                    value={formSettings[field.Key] ?? ""}
                    onChange={(e) =>
                      setFormSettings((prev) => ({ ...prev, [field.Key]: e.target.value }))
                    }
                    placeholder={defaults[formProviderName]?.[field.Key] ?? ""}
                  />
                ) : (
                  <Input
                    type={field.InputType === "password" ? "password" : "text"}
                    value={formSettings[field.Key] ?? ""}
                    onChange={(e) =>
                      setFormSettings((prev) => ({ ...prev, [field.Key]: e.target.value }))
                    }
                    placeholder={defaults[formProviderName]?.[field.Key] ?? ""}
                    required={field.Required}
                  />
                )}
              </label>
            ))}

            {/* Auto-provisioning section */}
            <div
              style={{
                borderTop: "1px solid var(--color-border)",
                paddingTop: "0.8rem",
                display: "grid",
                gap: "0.6rem"
              }}
            >
              <h4>Auto-Provisioning</h4>
              <label
                className="field-label"
                style={{ display: "flex", alignItems: "center", gap: 8 }}
              >
                <input
                  type="checkbox"
                  checked={formAutoProvision}
                  onChange={(e) => setFormAutoProvision(e.target.checked)}
                />
                Auto-create user accounts on first login
              </label>
              <label className="field-label">
                Allowed Email Domains (comma-separated)
                <Input
                  value={formAllowedDomains}
                  onChange={(e) => setFormAllowedDomains(e.target.value)}
                  placeholder="example.com, company.org"
                />
              </label>
            </div>

            <div style={{ display: "flex", gap: "0.5rem", justifyContent: "flex-end" }}>
              <Button type="button" onClick={cancelForm} disabled={isPending}>
                Cancel
              </Button>
              <Button type="button" onClick={handleSave} disabled={isPending}>
                {isPending ? "Saving..." : "Save"}
              </Button>
            </div>
          </div>
        )}

        {/* Provider list */}
        {providers.length === 0 && !showForm && (
          <p className="text-muted">
            No external auth providers configured. Click &quot;Add Provider&quot; to get started.
          </p>
        )}

        {providers.map((provider) => (
          <div
            key={provider.Id}
            style={{
              border: "1px solid var(--color-border)",
              borderRadius: 8,
              padding: "1rem",
              display: "grid",
              gap: "0.5rem"
            }}
          >
            <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
              <div style={{ display: "flex", alignItems: "center", gap: "0.6rem" }}>
                <strong>{provider.ProviderName}</strong>
                <span
                  className={`badge ${provider.IsEnabled ? "badge-success" : "badge-muted"}`}
                >
                  {provider.IsEnabled ? "Enabled" : "Disabled"}
                </span>
                {provider.LastTestSuccess === true && (
                  <span className="badge badge-success">Test Passed</span>
                )}
                {provider.LastTestSuccess === false && (
                  <span className="badge badge-error">Test Failed</span>
                )}
              </div>
              <div style={{ display: "flex", gap: "0.4rem" }}>
                <Button
                  type="button"
                  onClick={() => handleTest(provider.Id!)}
                  disabled={isPending}
                >
                  Test
                </Button>
                <Button
                  type="button"
                  onClick={() => openEditForm(provider)}
                  disabled={isPending}
                >
                  Edit
                </Button>
                {confirmDeleteId === provider.Id ? (
                  <>
                    <Button
                      type="button"
                      onClick={() => handleDelete(provider.Id!)}
                      disabled={isPending}
                    >
                      Confirm
                    </Button>
                    <Button
                      type="button"
                      onClick={() => setConfirmDeleteId(null)}
                      disabled={isPending}
                    >
                      Cancel
                    </Button>
                  </>
                ) : (
                  <Button
                    type="button"
                    onClick={() => setConfirmDeleteId(provider.Id!)}
                    disabled={isPending || provider.IsEnabled}
                  >
                    Delete
                  </Button>
                )}
              </div>
            </div>

            <div className="text-caption" style={{ display: "flex", gap: "1.5rem" }}>
              <span>Type: OIDC</span>
              {provider.AutoProvisionEnabled && <span>Auto-provision: On</span>}
              {provider.AllowedDomains && (
                <span>Domains: {provider.AllowedDomains}</span>
              )}
              {provider.LastTestedOn && (
                <span>Last tested: {formatUtcDateTime(provider.LastTestedOn)}</span>
              )}
            </div>

            {/* Show settings (masked) */}
            <div style={{ fontSize: "0.85rem", display: "flex", flexWrap: "wrap", gap: "0.8rem" }}>
              {Object.entries(provider.Settings).map(([key, value]) => (
                <span key={key} className="text-muted">
                  <strong>{key}:</strong> {value}
                </span>
              ))}
            </div>
          </div>
        ))}
      </div>
    </section>
  );
}
