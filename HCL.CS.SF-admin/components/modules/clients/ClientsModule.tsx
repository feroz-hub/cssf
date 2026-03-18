"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useEffect, useMemo, useState, useTransition } from "react";

import {
  createClientAction,
  deleteClientAction,
  rotateClientSecretAction,
  updateClientAction
} from "@/app/admin/clients/actions";
import { SignInAgainButton } from "@/components/admin/SignInAgainButton";
import { AlertDialog } from "@/components/ui/alert-dialog";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Dialog } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Select } from "@/components/ui/select";
import { Textarea } from "@/components/ui/textarea";
import { useToast } from "@/components/ui/toaster";
import { notifyClientActionError } from "@/lib/clientActionErrors";
import { type ClientsModel } from "@/lib/types/HCL.CS.SF";
import { cn, formatUtcDateTime } from "@/lib/utils";

const grants = ["authorization_code", "refresh_token", "client_credentials", "password"];
const defaultGrantTypes = ["authorization_code", "refresh_token"];
const defaultScopeSelection = ["openid", "profile", "email", "offline_access"];
type ClientApplicationType = "1" | "2" | "3" | "4";

type ClientFormState = {
  clientId: string;
  name: string;
  type: ClientApplicationType;
  allowedGrantTypes: string[];
  redirectUrisText: string;
  postLogoutUrisText: string;
  allowedScopes: string[];
  accessTokenLifetime: number;
  refreshTokenLifetime: number;
  identityTokenLifetime: number;
  logoutTokenLifetime: number;
  authorizationCodeLifetime: number;
  clientUri: string;
  logoUri: string;
  termsOfServiceUri: string;
  policyUri: string;
  preferredAudience: string;
};

const baseForm: ClientFormState = {
  clientId: "",
  name: "",
  type: "1",
  allowedGrantTypes: defaultGrantTypes,
  redirectUrisText: "",
  postLogoutUrisText: "",
  allowedScopes: [],
  accessTokenLifetime: 900,
  refreshTokenLifetime: 86400,
  identityTokenLifetime: 3600,
  logoutTokenLifetime: 1800,
  authorizationCodeLifetime: 600,
  clientUri: "",
  logoUri: "",
  termsOfServiceUri: "",
  policyUri: "",
  preferredAudience: ""
};

function buildDefaultAllowedScopes(availableScopes: string[]): string[] {
  const availableScopeSet = new Set(availableScopes);
  const preferredScopes = defaultScopeSelection.filter((scope) => availableScopeSet.has(scope));

  if (preferredScopes.length > 0) {
    return preferredScopes;
  }

  return availableScopes.length > 0 ? [availableScopes[0]] : [];
}

function createDefaultForm(availableScopes: string[]): ClientFormState {
  return {
    ...baseForm,
    allowedGrantTypes: [...baseForm.allowedGrantTypes],
    allowedScopes: buildDefaultAllowedScopes(availableScopes)
  };
}

function toClientType(value: number): string {
  if (value === 2) {
    return "SPA";
  }

  if (value === 3) {
    return "Native";
  }

  if (value === 4) {
    return "Service";
  }

  return "Web";
}

function parseList(text: string): string[] {
  return [...new Set(text.split(/[\n,]+/).map((value) => value.trim()).filter(Boolean))];
}

function parseApplicationType(value: string): ClientApplicationType {
  return value === "1" || value === "2" || value === "3" || value === "4" ? value : "1";
}

function usesAuthorizationCodeGrant(grantTypes: string[]): boolean {
  return grantTypes.includes("authorization_code");
}

function mapClientToForm(client: ClientsModel): ClientFormState {
  const type = parseApplicationType(String(client.ApplicationType));

  return {
    clientId: client.ClientId,
    name: client.ClientName,
    type,
    allowedGrantTypes: [...client.SupportedGrantTypes],
    redirectUrisText: client.RedirectUris.map((item) => item.RedirectUri).join("\n"),
    postLogoutUrisText: client.PostLogoutRedirectUris.map((item) => item.PostLogoutRedirectUri).join("\n"),
    allowedScopes: [...client.AllowedScopes],
    accessTokenLifetime: client.AccessTokenExpiration,
    refreshTokenLifetime: client.RefreshTokenExpiration,
    identityTokenLifetime: client.IdentityTokenExpiration,
    logoutTokenLifetime: client.LogoutTokenExpiration,
    authorizationCodeLifetime: client.AuthorizationCodeExpiration,
    clientUri: client.ClientUri,
    logoUri: client.LogoUri,
    termsOfServiceUri: client.TermsOfServiceUri,
    policyUri: client.PolicyUri,
    preferredAudience: client.PreferredAudience ?? ""
  };
}

function toPayload(form: ClientFormState) {
  return {
    clientId: form.clientId,
    name: form.name,
    type: form.type,
    allowedGrantTypes: form.allowedGrantTypes,
    redirectUris: parseList(form.redirectUrisText),
    postLogoutUris: parseList(form.postLogoutUrisText),
    allowedScopes: form.allowedScopes,
    accessTokenLifetime: Number(form.accessTokenLifetime),
    refreshTokenLifetime: Number(form.refreshTokenLifetime),
    identityTokenLifetime: Number(form.identityTokenLifetime),
    logoutTokenLifetime: Number(form.logoutTokenLifetime),
    authorizationCodeLifetime: Number(form.authorizationCodeLifetime),
    clientUri: form.clientUri,
    logoUri: form.logoUri,
    termsOfServiceUri: form.termsOfServiceUri,
    policyUri: form.policyUri,
    preferredAudience: form.preferredAudience
  };
}

type ScopeGroup = {
  id: string;
  label: string;
  allScope?: string;
  scopes: string[];
};

const standardScopes = new Set(["openid", "profile", "email", "phone", "offline_access"]);

const HCLCSSFGroupLabels: Record<string, string> = {
  client: "Clients",
  apiresource: "API Resources",
  identityresource: "Identity Resources",
  user: "Users",
  adminuser: "Admin Users",
  role: "Roles",
  securitytoken: "Security Tokens"
};

function buildScopeGroups(allScopes: string[]): ScopeGroup[] {
  const groups: Record<string, ScopeGroup> = {};

  const ensureGroup = (id: string, label: string): ScopeGroup => {
    if (!groups[id]) {
      groups[id] = { id, label, scopes: [] };
    }
    return groups[id];
  };

  for (const scope of allScopes) {
    if (standardScopes.has(scope)) {
      ensureGroup("standard", "Standard OpenID scopes").scopes.push(scope);
      continue;
    }

    if (scope.toLowerCase().startsWith("HCL.CS.SF.")) {
      const rest = scope.substring("HCL.CS.SF.".length);
      const [resourceKey, ...permissionParts] = rest.split(".");
      const key = resourceKey || "other";
      const groupId = `HCL.CS.SF:${key.toLowerCase()}`;
      const label = HCLCSSFGroupLabels[key.toLowerCase()] ?? `HCL.CS.SF: ${key}`;
      const group = ensureGroup(groupId, label);

      if (permissionParts.length === 0) {
        group.allScope = scope;
      } else {
        group.scopes.push(scope);
      }

      continue;
    }

    const head = scope.split(/[.:]/)[0] || "other";
    const id = `api:${head.toLowerCase()}`;
    const labelHead = head.charAt(0).toUpperCase() + head.slice(1);
    ensureGroup(id, `API: ${labelHead}`).scopes.push(scope);
  }

  return Object.values(groups).map((group) => ({
    ...group,
    scopes: Array.from(new Set(group.scopes)).sort()
  }));
}

const MAX_CLIENT_NAME_LENGTH = 255;
const MAX_URI_LENGTH = 2048;
const ACCESS_TOKEN_MIN = 60;
const ACCESS_TOKEN_MAX = 900;
const REFRESH_TOKEN_MIN = 300;
const REFRESH_TOKEN_MAX = 86400;
const IDENTITY_TOKEN_MIN = 60;
const IDENTITY_TOKEN_MAX = 3600;
const LOGOUT_TOKEN_MIN = 1800;
const LOGOUT_TOKEN_MAX = 86400;
const AUTH_CODE_MIN = 60;
const AUTH_CODE_MAX = 600;

function validateForm(
  form: ClientFormState,
  availableScopes: string[],
  notify: (message: string, kind?: "success" | "error" | "info") => void
) {
  const errors: string[] = [];
  const redirectUris = parseList(form.redirectUrisText);
  const requiresRedirectUris = usesAuthorizationCodeGrant(form.allowedGrantTypes);
  const availableScopeSet = new Set(availableScopes);

  if (!form.name.trim()) {
    errors.push("Client name is required.");
  } else if (form.name.length > MAX_CLIENT_NAME_LENGTH) {
    errors.push(`Client name must be ${MAX_CLIENT_NAME_LENGTH} characters or fewer.`);
  }

  if (requiresRedirectUris && redirectUris.length === 0) {
    errors.push("At least one redirect URI is required when authorization_code is selected.");
  }

  if (redirectUris.some((u) => u.length > MAX_URI_LENGTH)) {
    errors.push(`Each redirect URI must be ${MAX_URI_LENGTH} characters or fewer.`);
  }

  const postLogoutUris = parseList(form.postLogoutUrisText);
  if (postLogoutUris.some((u) => u.length > MAX_URI_LENGTH)) {
    errors.push(`Each post-logout URI must be ${MAX_URI_LENGTH} characters or fewer.`);
  }

  if (form.allowedGrantTypes.length === 0) {
    errors.push("Select at least one allowed grant type.");
  }

  if (form.allowedScopes.length === 0) {
    errors.push("Select at least one allowed scope.");
  } else if (availableScopeSet.size === 0) {
    errors.push("Available scopes failed to load. Reload the page before saving the client.");
  } else {
    const invalidScopes = Array.from(new Set(form.allowedScopes.filter((scope) => !availableScopeSet.has(scope))));
    if (invalidScopes.length > 0) {
      errors.push(`Unknown or unavailable scopes selected: ${invalidScopes.join(", ")}.`);
    }
  }

  const access = Number(form.accessTokenLifetime);
  if (access < ACCESS_TOKEN_MIN || access > ACCESS_TOKEN_MAX) {
    errors.push(`Access token lifetime must be between ${ACCESS_TOKEN_MIN} and ${ACCESS_TOKEN_MAX} seconds.`);
  }

  const refresh = Number(form.refreshTokenLifetime);
  if (refresh < REFRESH_TOKEN_MIN || refresh > REFRESH_TOKEN_MAX) {
    errors.push(`Refresh token lifetime must be between ${REFRESH_TOKEN_MIN} and ${REFRESH_TOKEN_MAX} seconds.`);
  }

  const identity = Number(form.identityTokenLifetime);
  if (identity < IDENTITY_TOKEN_MIN || identity > IDENTITY_TOKEN_MAX) {
    errors.push(`Identity token lifetime must be between ${IDENTITY_TOKEN_MIN} and ${IDENTITY_TOKEN_MAX} seconds.`);
  }

  const logout = Number(form.logoutTokenLifetime);
  if (logout < LOGOUT_TOKEN_MIN || logout > LOGOUT_TOKEN_MAX) {
    errors.push(`Logout token lifetime must be between ${LOGOUT_TOKEN_MIN} and ${LOGOUT_TOKEN_MAX} seconds.`);
  }

  const authCode = Number(form.authorizationCodeLifetime);
  if (authCode < AUTH_CODE_MIN || authCode > AUTH_CODE_MAX) {
    errors.push(`Authorization code lifetime must be between ${AUTH_CODE_MIN} and ${AUTH_CODE_MAX} seconds.`);
  }

  if (errors.length > 0) {
    notify(errors.join(" "), "error");
    return false;
  }

  return true;
}

type Props = {
  clients: ClientsModel[];
  availableScopes: string[];
  loadError?: string;
  loadErrorIsUnauthorized?: boolean;
};

export function ClientsModule({ clients, availableScopes, loadError, loadErrorIsUnauthorized }: Props) {
  const router = useRouter();
  const { notify } = useToast();
  const defaultCreateForm = useMemo(() => createDefaultForm(availableScopes), [availableScopes]);

  const [form, setForm] = useState<ClientFormState>(() => createDefaultForm(availableScopes));
  const [selectedClient, setSelectedClient] = useState<ClientsModel | null>(null);
  const [createOpen, setCreateOpen] = useState(false);
  const [editOpen, setEditOpen] = useState(false);
  const [deleteClientId, setDeleteClientId] = useState<string | null>(null);
  const [generatedSecret, setGeneratedSecret] = useState<string | null>(null);
  const [generatedSecretClientId, setGeneratedSecretClientId] = useState<string | null>(null);
  const [page, setPage] = useState(1);
  const pageSize = 10;
  const [pending, startTransition] = useTransition();

  const pagedClients = useMemo(() => {
    const start = (page - 1) * pageSize;
    return clients.slice(start, start + pageSize);
  }, [clients, page]);

  const totalPages = Math.max(1, Math.ceil(clients.length / pageSize));

  const onCreate = () => {
    setForm(defaultCreateForm);
    setCreateOpen(true);
  };

  // When Create dialog opens, ensure form has default URIs so fields show actual values.
  useEffect(() => {
    if (createOpen) {
      setForm(defaultCreateForm);
    }
  }, [createOpen, defaultCreateForm]);

  const onEdit = (client: ClientsModel) => {
    setSelectedClient(client);
    setForm(mapClientToForm(client));
    setEditOpen(true);
  };

  const onSubmitCreate = () => {
    startTransition(async () => {
      try {
        if (!validateForm(form, availableScopes, notify)) {
          return;
        }

        const result = await createClientAction(toPayload(form));
        if (!result.ok) {
          notify(result.message, "error");
          return;
        }

        notify(result.message, "success");
        if (result.data?.secret) {
          setGeneratedSecret(result.data.secret);
          setGeneratedSecretClientId(result.data.clientId);
        }

        setCreateOpen(false);
        router.refresh();
      } catch (error) {
        notifyClientActionError(error, notify, "Client creation failed.");
      }
    });
  };

  const onSubmitUpdate = () => {
    startTransition(async () => {
      try {
        if (!selectedClient) {
          return;
        }

        if (!validateForm(form, availableScopes, notify)) {
          return;
        }

        const result = await updateClientAction({
          ...toPayload(form),
          clientId: selectedClient.ClientId
        });

        if (!result.ok) {
          notify(result.message, "error");
          return;
        }

        notify(result.message, "success");
        setEditOpen(false);
        router.refresh();
      } catch (error) {
        notifyClientActionError(error, notify, "Client update failed.");
      }
    });
  };

  const onConfirmDelete = () => {
    if (!deleteClientId) {
      return;
    }

    startTransition(async () => {
      try {
        const result = await deleteClientAction({ clientId: deleteClientId });
        if (!result.ok) {
          notify(result.message, "error");
          return;
        }

        notify(result.message, "success");
        setDeleteClientId(null);
        router.refresh();
      } catch (error) {
        notifyClientActionError(error, notify, "Client deletion failed.");
      }
    });
  };

  const onRotateSecret = (clientId: string) => {
    startTransition(async () => {
      try {
        const result = await rotateClientSecretAction({ clientId });
        if (!result.ok) {
          notify(result.message, "error");
          return;
        }

        notify(result.message, "success");
        setGeneratedSecret(result.data?.secret ?? null);
        setGeneratedSecretClientId(clientId);
      } catch (error) {
        notifyClientActionError(error, notify, "Client secret rotation failed.");
      }
    });
  };

  const copySecret = async () => {
    if (!generatedSecret) {
      return;
    }

    await navigator.clipboard.writeText(generatedSecret);
    notify("Secret copied to clipboard.", "success");
  };

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
            <h2>Client Management</h2>
            <p className="inline-message">Registered OAuth/OIDC clients.</p>
          </div>
          <div className="toolbar">
            <Button type="button" variant="secondary" onClick={onCreate}>
              Create Client
            </Button>
          </div>
        </header>

        <div className="card-body">
          {generatedSecret ? (
            <div className="card" style={{ marginBottom: "0.9rem" }}>
              <div className="card-body" style={{ display: "grid", gap: "0.6rem" }}>
                <p className="inline-message success">
                  New secret for client <strong>{generatedSecretClientId}</strong>. Copy now; it will not be shown again.
                </p>
                <Input readOnly value={generatedSecret} />
                <div className="toolbar">
                  <Button type="button" variant="secondary" onClick={copySecret}>
                    Copy secret
                  </Button>
                  <Button type="button" variant="ghost" onClick={() => setGeneratedSecret(null)}>
                    Dismiss
                  </Button>
                </div>
              </div>
            </div>
          ) : null}

          <div className="table-wrap">
            <table className="table">
              <thead>
                <tr>
                  <th>Client ID</th>
                  <th>Name</th>
                  <th>Type</th>
                  <th>Enabled</th>
                  <th>Created At</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {pagedClients.map((client) => (
                  <tr key={client.ClientId} className="table-row">
                    <td>{client.ClientId}</td>
                    <td>{client.ClientName}</td>
                    <td>{toClientType(client.ApplicationType)}</td>
                    <td>
                      <Badge className={client.IsDeleted ? "badge-danger" : ""}>
                        {client.IsDeleted ? "No" : "Yes"}
                      </Badge>
                    </td>
                    <td>{formatUtcDateTime(client.CreatedOn)}</td>
                    <td>
                      <div className="table-actions">
                        <Button type="button" variant="ghost" onClick={() => onEdit(client)}>
                          Edit
                        </Button>
                        <Link href={`/admin/clients/${encodeURIComponent(client.ClientId)}/secrets`}>
                          <Button type="button" variant="ghost">
                            Secrets
                          </Button>
                        </Link>
                        <Button type="button" variant="danger" onClick={() => setDeleteClientId(client.ClientId)}>
                          Delete
                        </Button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          <div className="toolbar" style={{ marginTop: "0.8rem", justifyContent: "space-between" }}>
            <span className="inline-message">Page {page} of {totalPages}</span>
            <div className="toolbar">
              <Button type="button" variant="ghost" onClick={() => setPage((value) => Math.max(1, value - 1))}>
                Previous
              </Button>
              <Button type="button" variant="ghost" onClick={() => setPage((value) => Math.min(totalPages, value + 1))}>
                Next
              </Button>
            </div>
          </div>
        </div>
      </section>

      <Dialog
        open={createOpen}
        title="Create Client"
        description="Register a new client application."
        onClose={() => setCreateOpen(false)}
      >
        <ClientForm
          form={form}
          setForm={setForm}
          availableScopes={availableScopes}
          submitLabel={pending ? "Saving..." : "Create"}
          onSubmit={onSubmitCreate}
          onRotateSecret={null}
          pending={pending}
        />
      </Dialog>

      <Dialog
        open={editOpen}
        title="Edit Client"
        description="Update client settings and URI lists."
        onClose={() => setEditOpen(false)}
      >
        <ClientForm
          form={form}
          setForm={setForm}
          availableScopes={availableScopes}
          submitLabel={pending ? "Saving..." : "Update"}
          onSubmit={onSubmitUpdate}
          onRotateSecret={selectedClient ? () => onRotateSecret(selectedClient.ClientId) : null}
          pending={pending}
        />
      </Dialog>

      <AlertDialog
        open={Boolean(deleteClientId)}
        title="Delete client"
        description="This action permanently removes the client registration."
        confirmLabel={pending ? "Deleting..." : "Delete"}
        onCancel={() => setDeleteClientId(null)}
        onConfirm={onConfirmDelete}
      />
    </>
  );
}

type ClientFormProps = {
  form: ClientFormState;
  setForm: React.Dispatch<React.SetStateAction<ClientFormState>>;
  availableScopes: string[];
  submitLabel: string;
  onSubmit: () => void;
  onRotateSecret: (() => void) | null;
  pending: boolean;
};

function ClientForm({
  form,
  setForm,
  availableScopes,
  submitLabel,
  onSubmit,
  onRotateSecret,
  pending
}: ClientFormProps) {
  const scopeCatalog = useMemo(
    () => Array.from(new Set([...availableScopes, ...form.allowedScopes])),
    [availableScopes, form.allowedScopes]
  );
  const scopeGroups = useMemo(() => buildScopeGroups(scopeCatalog), [scopeCatalog]);
  const unavailableSelectedScopes = useMemo(() => {
    const availableScopeSet = new Set(availableScopes);
    return Array.from(new Set(form.allowedScopes.filter((scope) => !availableScopeSet.has(scope)))).sort();
  }, [availableScopes, form.allowedScopes]);
  const requiresRedirectUris = usesAuthorizationCodeGrant(form.allowedGrantTypes);

  return (
    <div className="form-grid">
      <div className="form-grid" style={{ gap: "0.6rem" }}>
        <h3 className="text-heading">Basics</h3>
        <div className="form-row">
          <label>Client Name</label>
          <Input
            value={form.name}
            placeholder="Admin Console"
            maxLength={MAX_CLIENT_NAME_LENGTH}
            onChange={(event) => setForm((state) => ({ ...state, name: event.target.value }))}
          />
          <p className="inline-message">Human-friendly name shown in HCL.CS.SF and consent screens (max {MAX_CLIENT_NAME_LENGTH} characters).</p>
        </div>

        <div className="form-row">
          <label>Client ID</label>
          <Input
            value={form.clientId}
            placeholder="Leave empty to auto-generate"
            onChange={(event) => setForm((state) => ({ ...state, clientId: event.target.value }))}
          />
        </div>

        <div className="form-row">
          <label>Application Type</label>
          <Select
            value={form.type}
            onChange={(event) =>
              setForm((state) => ({
                ...state,
                type: parseApplicationType(event.target.value)
              }))
            }
          >
            <option value="1">Web (server-rendered)</option>
            <option value="2">SPA (browser-only)</option>
            <option value="3">Native (desktop / mobile)</option>
            <option value="4">Service (machine-to-machine)</option>
          </Select>
        </div>
      </div>

      <div className="form-grid" style={{ gap: "0.6rem" }}>
        <h3 className="text-heading">Tokens & Permissions</h3>
        <div className="form-row">
          <label>Allowed Grant Types</label>
          <div className="checkbox-group checkbox-group-inline">
            {grants.map((grant) => {
              const checked = form.allowedGrantTypes.includes(grant);
              return (
                <label
                  key={grant}
                  className={cn("checkbox-pill", checked && "checkbox-pill-checked")}
                >
                  <input
                    type="checkbox"
                    checked={checked}
                    onChange={() =>
                      setForm((state) => {
                        const has = state.allowedGrantTypes.includes(grant);
                        return {
                          ...state,
                          allowedGrantTypes: has
                            ? state.allowedGrantTypes.filter((value) => value !== grant)
                            : [...state.allowedGrantTypes, grant]
                        };
                      })
                    }
                  />
                  <span className="text-mono">{grant}</span>
                </label>
              );
            })}
          </div>
          <p className="inline-message">
            Common: <span className="text-mono">authorization_code</span> +{" "}
            <span className="text-mono">refresh_token</span> for web and SPA clients.
          </p>
        </div>

        <div className="form-row">
          <label>Allowed Scopes</label>
          {availableScopes.length === 0 ? (
            <p className="inline-message error">Scopes are unavailable right now. Reload the page before saving this client.</p>
          ) : null}
          {unavailableSelectedScopes.length > 0 ? (
            <p className="inline-message error">
              Selected scopes missing from the current server catalog: {unavailableSelectedScopes.join(", ")}.
            </p>
          ) : null}
          <div className="form-grid" style={{ gap: "0.45rem" }}>
            {scopeGroups.map((group) => (
              <div key={group.id} className="form-grid" style={{ gap: "0.3rem" }}>
                <span className="text-caption">{group.label}</span>
                {group.allScope ? (
                  <div className="checkbox-group-inline">
                    {(() => {
                      const allChecked =
                        form.allowedScopes.includes(group.allScope) &&
                        group.scopes.every((scope) => form.allowedScopes.includes(scope));
                      const anySelected =
                        form.allowedScopes.includes(group.allScope) ||
                        group.scopes.some((scope) => form.allowedScopes.includes(scope));
                      const partial = anySelected && !allChecked;

                      return (
                        <label
                          className={cn(
                            "checkbox-pill",
                            allChecked && "checkbox-pill-checked",
                            partial && "checkbox-pill-partial"
                          )}
                        >
                          <input
                            type="checkbox"
                            checked={allChecked}
                            onChange={() =>
                              setForm((state) => {
                                const current = new Set(state.allowedScopes);
                                const allValues = [group.allScope!, ...group.scopes];

                                if (allChecked) {
                                  allValues.forEach((scope) => current.delete(scope));
                                } else {
                                  allValues.forEach((scope) => current.add(scope));
                                }

                                return {
                                  ...state,
                                  allowedScopes: Array.from(current)
                                };
                              })
                            }
                          />
                          <span className="text-body">
                            All {group.label.toLowerCase().replace(/s$/, "")} permissions
                          </span>
                        </label>
                      );
                    })()}
                  </div>
                ) : null}
                <div className="checkbox-group checkbox-group-inline">
                  {group.scopes.map((scope) => {
                    const checked = form.allowedScopes.includes(scope);
                    return (
                      <label
                        key={scope}
                        className={cn("checkbox-pill", checked && "checkbox-pill-checked")}
                      >
                        <input
                          type="checkbox"
                          checked={checked}
                          onChange={() =>
                            setForm((state) => {
                              const has = state.allowedScopes.includes(scope);
                              return {
                                ...state,
                                allowedScopes: has
                                  ? state.allowedScopes.filter((value) => value !== scope)
                                  : [...state.allowedScopes, scope]
                              };
                            })
                          }
                        />
                        <span className="text-mono">{scope}</span>
                      </label>
                    );
                  })}
                </div>
              </div>
            ))}
          </div>
          <p className="inline-message">Select the APIs and identity resources this client can access.</p>
        </div>

        <div className="form-row">
          <label>Access Token Lifetime (seconds)</label>
          <Input
            type="number"
            min={ACCESS_TOKEN_MIN}
            max={ACCESS_TOKEN_MAX}
            value={form.accessTokenLifetime}
            onChange={(event) =>
              setForm((state) => ({ ...state, accessTokenLifetime: Number(event.target.value || "0") }))
            }
          />
          <p className="inline-message">Between {ACCESS_TOKEN_MIN} and {ACCESS_TOKEN_MAX} seconds (server default max 900).</p>
        </div>

        <div className="form-row">
          <label>Refresh Token Lifetime (seconds)</label>
          <Input
            type="number"
            min={REFRESH_TOKEN_MIN}
            max={REFRESH_TOKEN_MAX}
            value={form.refreshTokenLifetime}
            onChange={(event) =>
              setForm((state) => ({ ...state, refreshTokenLifetime: Number(event.target.value || "0") }))
            }
          />
          <p className="inline-message">Between {REFRESH_TOKEN_MIN} and {REFRESH_TOKEN_MAX} seconds (24 hours = 86400).</p>
        </div>

        <div className="form-row">
          <label>Identity Token Lifetime (seconds)</label>
          <Input
            type="number"
            min={IDENTITY_TOKEN_MIN}
            max={IDENTITY_TOKEN_MAX}
            value={form.identityTokenLifetime}
            onChange={(event) =>
              setForm((state) => ({ ...state, identityTokenLifetime: Number(event.target.value || "0") }))
            }
          />
          <p className="inline-message">Between {IDENTITY_TOKEN_MIN} and {IDENTITY_TOKEN_MAX} seconds (ID token expiry).</p>
        </div>

        <div className="form-row">
          <label>Logout Token Lifetime (seconds)</label>
          <Input
            type="number"
            min={LOGOUT_TOKEN_MIN}
            max={LOGOUT_TOKEN_MAX}
            value={form.logoutTokenLifetime}
            onChange={(event) =>
              setForm((state) => ({ ...state, logoutTokenLifetime: Number(event.target.value || "0") }))
            }
          />
          <p className="inline-message">Between {LOGOUT_TOKEN_MIN} and {LOGOUT_TOKEN_MAX} seconds.</p>
        </div>

        <div className="form-row">
          <label>Authorization Code Lifetime (seconds)</label>
          <Input
            type="number"
            min={AUTH_CODE_MIN}
            max={AUTH_CODE_MAX}
            value={form.authorizationCodeLifetime}
            onChange={(event) =>
              setForm((state) => ({ ...state, authorizationCodeLifetime: Number(event.target.value || "0") }))
            }
          />
          <p className="inline-message">Between {AUTH_CODE_MIN} and {AUTH_CODE_MAX} seconds.</p>
        </div>
      </div>

      <div className="form-grid" style={{ gap: "0.6rem" }}>
        <h3 className="text-heading">Redirects & Branding</h3>
        <div className="form-row">
          <label>Redirect URIs</label>
          <Textarea
            value={form.redirectUrisText}
            placeholder="https://localhost:3000/api/auth/callback/HCL.CS.SF"
            onChange={(event) => setForm((state) => ({ ...state, redirectUrisText: event.target.value }))}
          />
          <p className="inline-message">
            {requiresRedirectUris
              ? `Required when authorization_code is selected. One URL per line or comma-separated. Must match app configuration exactly (max ${MAX_URI_LENGTH} characters per URI).`
              : `Optional for non-browser clients. Leave blank for client_credentials-only clients, or enter one URL per line / comma-separated when needed (max ${MAX_URI_LENGTH} characters per URI).`}
          </p>
        </div>

        <div className="form-row">
          <label>Post Logout Redirect URIs</label>
          <Textarea
            value={form.postLogoutUrisText}
            placeholder="https://localhost:3000/login"
            onChange={(event) => setForm((state) => ({ ...state, postLogoutUrisText: event.target.value }))}
          />
          <p className="inline-message">Optional. Max {MAX_URI_LENGTH} characters per URI.</p>
        </div>

        <div className="form-row">
          <label>Client URI</label>
          <Input
            value={form.clientUri}
            placeholder="https://localhost:3000"
            onChange={(event) => setForm((state) => ({ ...state, clientUri: event.target.value }))}
          />
        </div>

        <div className="form-row">
          <label>Logo URI</label>
          <Input
            value={form.logoUri}
            placeholder="https://localhost:3000/logo"
            onChange={(event) => setForm((state) => ({ ...state, logoUri: event.target.value }))}
          />
        </div>

        <div className="form-row">
          <label>Terms Of Service URI</label>
          <Input
            value={form.termsOfServiceUri}
            placeholder="https://localhost:3000/terms"
            onChange={(event) => setForm((state) => ({ ...state, termsOfServiceUri: event.target.value }))}
          />
        </div>

        <div className="form-row">
          <label>Policy URI</label>
          <Input
            value={form.policyUri}
            placeholder="https://localhost:3000/policy"
            onChange={(event) => setForm((state) => ({ ...state, policyUri: event.target.value }))}
          />
        </div>
        <div className="form-row">
          <label>Default / preferred audience (optional)</label>
          <Input
            value={form.preferredAudience}
            placeholder="e.g. rentflow.api or HCL.CS.SF.api"
            onChange={(event) => setForm((state) => ({ ...state, preferredAudience: event.target.value }))}
          />
          <p className="inline-message" style={{ marginTop: "0.25rem", fontSize: "0.8rem" }}>
            When set, access tokens for this client will use this value as the <code>aud</code> claim.
          </p>
        </div>
      </div>

      <div className="dialog-actions">
        {onRotateSecret ? (
          <Button type="button" variant="secondary" onClick={onRotateSecret} disabled={pending}>
            Rotate Secret
          </Button>
        ) : null}
        <Button type="button" variant="primary" onClick={onSubmit} disabled={pending}>
          {submitLabel}
        </Button>
      </div>
    </div>
  );
}
