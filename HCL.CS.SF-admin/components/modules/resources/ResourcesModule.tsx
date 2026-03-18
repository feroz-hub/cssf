"use client";

import Link from "next/link";
import { useCallback } from "react";
import { useRouter } from "next/navigation";
import { useMemo, useState, useTransition } from "react";

import {
  addApiResourceClaimAction,
  addApiScopeClaimAction,
  createResourceAction,
  createScopeAction,
  deleteApiResourceClaimAction,
  deleteApiScopeClaimAction,
  deleteResourceAction,
  deleteScopeAction,
  getApiResourceClaimsAction,
  getApiScopeClaimsAction,
  updateResourceAction,
  updateScopeAction
} from "@/app/admin/resources/actions";
import { SignInAgainButton } from "@/components/admin/SignInAgainButton";
import { AlertDialog } from "@/components/ui/alert-dialog";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Dialog } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { useToast } from "@/components/ui/toaster";
import {
  type ApiResourceClaimsModel,
  type ApiResourcesModel,
  type ApiScopeClaimsModel,
  type ApiScopesModel
} from "@/lib/types/HCL.CS.SF";

type ResourceForm = {
  id: string;
  name: string;
  displayName: string;
  description: string;
  enabled: boolean;
};

type ScopeForm = {
  id: string;
  resourceId: string;
  name: string;
  displayName: string;
  description: string;
  required: boolean;
  emphasize: boolean;
};

const defaultResourceForm: ResourceForm = {
  id: "00000000-0000-0000-0000-000000000000",
  name: "",
  displayName: "",
  description: "",
  enabled: true
};

const defaultScopeForm: ScopeForm = {
  id: "00000000-0000-0000-0000-000000000000",
  resourceId: "",
  name: "",
  displayName: "",
  description: "",
  required: false,
  emphasize: false
};

type Props = {
  resources: ApiResourcesModel[];
  loadError?: string;
  loadErrorIsUnauthorized?: boolean;
};

export function ResourcesModule({ resources, loadError, loadErrorIsUnauthorized }: Props) {
  const router = useRouter();
  const { notify } = useToast();
  const [expandedResourceId, setExpandedResourceId] = useState<string | null>(null);
  const [resourceForm, setResourceForm] = useState<ResourceForm>(defaultResourceForm);
  const [scopeForm, setScopeForm] = useState<ScopeForm>(defaultScopeForm);
  const [resourceModalOpen, setResourceModalOpen] = useState(false);
  const [scopeModalOpen, setScopeModalOpen] = useState(false);
  const [deleteResourceId, setDeleteResourceId] = useState<string | null>(null);
  const [deleteScopeId, setDeleteScopeId] = useState<string | null>(null);
  const [resourceClaimsMap, setResourceClaimsMap] = useState<Record<string, ApiResourceClaimsModel[]>>({});
  const [scopeClaimsMap, setScopeClaimsMap] = useState<Record<string, ApiScopeClaimsModel[]>>({});
  const [resourceClaimTypeInput, setResourceClaimTypeInput] = useState<Record<string, string>>({});
  const [scopeClaimTypeInput, setScopeClaimTypeInput] = useState<Record<string, string>>({});
  const [pending, startTransition] = useTransition();

  const loadClaimsForResource = useCallback((resourceId: string, scopeIds: string[]) => {
    startTransition(async () => {
      const [resourceClaims, ...scopeClaimsList] = await Promise.all([
        getApiResourceClaimsAction(resourceId),
        ...scopeIds.map((id) => getApiScopeClaimsAction(id))
      ]);
      setResourceClaimsMap((prev) => ({ ...prev, [resourceId]: resourceClaims }));
      setScopeClaimsMap((prev) => {
        const next = { ...prev };
        scopeIds.forEach((id, i) => {
          next[id] = scopeClaimsList[i] as ApiScopeClaimsModel[];
        });
        return next;
      });
    });
  }, []);

  const toggleExpand = (resourceId: string, scopeIds: string[]) => {
    if (expandedResourceId === resourceId) {
      setExpandedResourceId(null);
    } else {
      setExpandedResourceId(resourceId);
      if (!resourceClaimsMap[resourceId]) loadClaimsForResource(resourceId, scopeIds);
    }
  };

  const addResourceClaim = (resourceId: string) => {
    const type = (resourceClaimTypeInput[resourceId] ?? "").trim();
    if (!type) {
      notify("Enter a claim type (e.g. capabilities, role, permission).", "error");
      return;
    }
    startTransition(async () => {
      const result = await addApiResourceClaimAction({ resourceId, type });
      if (!result.ok) {
        notify(result.message, "error");
        return;
      }
      notify(result.message, "success");
      setResourceClaimTypeInput((s) => ({ ...s, [resourceId]: "" }));
      const claims = await getApiResourceClaimsAction(resourceId);
      setResourceClaimsMap((prev) => ({ ...prev, [resourceId]: claims }));
      router.refresh();
    });
  };

  const removeResourceClaim = (resourceId: string, claimId: string) => {
    startTransition(async () => {
      const result = await deleteApiResourceClaimAction({ claimId });
      if (!result.ok) {
        notify(result.message, "error");
        return;
      }
      notify(result.message, "success");
      const claims = await getApiResourceClaimsAction(resourceId);
      setResourceClaimsMap((prev) => ({ ...prev, [resourceId]: claims }));
      router.refresh();
    });
  };

  const addScopeClaim = (scopeId: string, resourceId: string) => {
    const type = (scopeClaimTypeInput[scopeId] ?? "").trim();
    if (!type) {
      notify("Enter a claim type (e.g. capabilities, role, permission).", "error");
      return;
    }
    startTransition(async () => {
      const result = await addApiScopeClaimAction({ scopeId, type });
      if (!result.ok) {
        notify(result.message, "error");
        return;
      }
      notify(result.message, "success");
      setScopeClaimTypeInput((s) => ({ ...s, [scopeId]: "" }));
      const claims = await getApiScopeClaimsAction(scopeId);
      setScopeClaimsMap((prev) => ({ ...prev, [scopeId]: claims }));
      router.refresh();
    });
  };

  const removeScopeClaim = (scopeId: string, claimId: string) => {
    startTransition(async () => {
      const result = await deleteApiScopeClaimAction({ claimId });
      if (!result.ok) {
        notify(result.message, "error");
        return;
      }
      notify(result.message, "success");
      const claims = await getApiScopeClaimsAction(scopeId);
      setScopeClaimsMap((prev) => ({ ...prev, [scopeId]: claims }));
      router.refresh();
    });
  };

  const sortedResources = useMemo(
    () => [...resources].sort((left, right) => left.Name.localeCompare(right.Name)),
    [resources]
  );

  const openResourceCreate = () => {
    setResourceForm(defaultResourceForm);
    setResourceModalOpen(true);
  };

  const openResourceEdit = (resource: ApiResourcesModel) => {
    setResourceForm({
      id: resource.Id,
      name: resource.Name,
      displayName: resource.DisplayName,
      description: resource.Description,
      enabled: resource.Enabled
    });
    setResourceModalOpen(true);
  };

  const openScopeCreate = (resourceId: string) => {
    setScopeForm({ ...defaultScopeForm, resourceId });
    setScopeModalOpen(true);
  };

  const openScopeEdit = (scope: ApiScopesModel) => {
    setScopeForm({
      id: scope.Id,
      resourceId: scope.ApiResourceId,
      name: scope.Name,
      displayName: scope.DisplayName,
      description: scope.Description,
      required: scope.Required,
      emphasize: scope.Emphasize
    });
    setScopeModalOpen(true);
  };

  const saveResource = () => {
    startTransition(async () => {
      const action =
        resourceForm.id === "00000000-0000-0000-0000-000000000000"
          ? createResourceAction
          : updateResourceAction;

      const result = await action(resourceForm);
      if (!result.ok) {
        notify(result.message, "error");
        return;
      }

      notify(result.message, "success");
      setResourceModalOpen(false);
      router.refresh();
    });
  };

  const saveScope = () => {
    startTransition(async () => {
      const action =
        scopeForm.id === "00000000-0000-0000-0000-000000000000" ? createScopeAction : updateScopeAction;

      const result = await action(scopeForm);
      if (!result.ok) {
        notify(result.message, "error");
        return;
      }

      notify(result.message, "success");
      setScopeModalOpen(false);
      router.refresh();
    });
  };

  const confirmDeleteResource = () => {
    if (!deleteResourceId) {
      return;
    }

    startTransition(async () => {
      const result = await deleteResourceAction({ id: deleteResourceId });
      if (!result.ok) {
        notify(result.message, "error");
        return;
      }

      notify(result.message, "success");
      setDeleteResourceId(null);
      router.refresh();
    });
  };

  const confirmDeleteScope = () => {
    if (!deleteScopeId) {
      return;
    }

    startTransition(async () => {
      const result = await deleteScopeAction({ id: deleteScopeId });
      if (!result.ok) {
        notify(result.message, "error");
        return;
      }

      notify(result.message, "success");
      setDeleteScopeId(null);
      router.refresh();
    });
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
            <h2>API Resources & Scopes</h2>
            <p className="inline-message">Manage API resource metadata and nested scope definitions.</p>
          </div>
          <Button type="button" variant="secondary" onClick={openResourceCreate}>
            Create Resource
          </Button>
        </header>

        <div className="card-body">
          <div className="table-wrap">
            <table className="table">
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Display Name</th>
                  <th>Scopes Count</th>
                  <th>Enabled</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {sortedResources.map((resource) => {
                  const expanded = expandedResourceId === resource.Id;
                  return (
                    <tr key={resource.Id} className="table-row">
                      <td>{resource.Name}</td>
                      <td>{resource.DisplayName}</td>
                      <td>{resource.ApiScopes.length}</td>
                      <td>
                        <Badge className={!resource.Enabled ? "badge-danger" : ""}>
                          {resource.Enabled ? "Yes" : "No"}
                        </Badge>
                      </td>
                      <td>
                        <div className="table-actions">
                          <Button
                            type="button"
                            variant="ghost"
                            onClick={() => toggleExpand(resource.Id, resource.ApiScopes?.map((s) => s.Id) ?? [])}
                          >
                            {expanded ? "Hide Scopes" : "Scopes"}
                          </Button>
                          <Button type="button" variant="ghost" onClick={() => openResourceEdit(resource)}>
                            Edit
                          </Button>
                          <Button type="button" variant="danger" onClick={() => setDeleteResourceId(resource.Id)}>
                            Delete
                          </Button>
                        </div>

                        {expanded ? (
                          <div className="card" style={{ marginTop: "0.8rem" }}>
                            <div className="card-head">
                              <h3>Resource claim types</h3>
                            </div>
                            <div className="card-body">
                              <p className="inline-message" style={{ marginBottom: "0.5rem" }}>
                                Claim types (e.g. <code>role</code>, <code>permission</code>, <code>capabilities</code>) to include in access tokens when this resource is in scope.
                              </p>
                              <div className="table-wrap" style={{ marginBottom: "0.5rem" }}>
                                <table className="table">
                                  <tbody>
                                    {(resourceClaimsMap[resource.Id] ?? []).map((c) => (
                                      <tr key={c.Id}>
                                        <td><code>{c.Type}</code></td>
                                        <td>
                                          <Button
                                            type="button"
                                            variant="ghost"
                                            onClick={() => removeResourceClaim(resource.Id, c.Id)}
                                            disabled={pending}
                                          >
                                            Remove
                                          </Button>
                                        </td>
                                      </tr>
                                    ))}
                                  </tbody>
                                </table>
                              </div>
                              <div style={{ display: "flex", gap: "0.5rem", alignItems: "center", flexWrap: "wrap" }}>
                                <Input
                                  placeholder="e.g. capabilities"
                                  value={resourceClaimTypeInput[resource.Id] ?? ""}
                                  onChange={(e) =>
                                    setResourceClaimTypeInput((s) => ({ ...s, [resource.Id]: e.target.value }))
                                  }
                                  onKeyDown={(e) => e.key === "Enter" && addResourceClaim(resource.Id)}
                                  style={{ maxWidth: "14rem" }}
                                />
                                <Button
                                  type="button"
                                  variant="secondary"
                                  onClick={() => addResourceClaim(resource.Id)}
                                  disabled={pending}
                                >
                                  Add claim type
                                </Button>
                              </div>
                            </div>
                            <div className="card-head">
                              <h3>Scopes</h3>
                              <Button type="button" variant="secondary" onClick={() => openScopeCreate(resource.Id)}>
                                Create Scope
                              </Button>
                            </div>
                            <div className="card-body">
                              <div className="table-wrap">
                                <table className="table">
                                  <thead>
                                    <tr>
                                      <th>Name</th>
                                      <th>Display Name</th>
                                      <th>Required</th>
                                      <th>Emphasize</th>
                                      <th>Claim types</th>
                                      <th>Actions</th>
                                    </tr>
                                  </thead>
                                  <tbody>
                                    {resource.ApiScopes?.map((scope) => (
                                      <tr key={scope.Id} className="table-row">
                                        <td>{scope.Name}</td>
                                        <td>{scope.DisplayName}</td>
                                        <td>
                                          <Badge className={scope.Required ? "" : "badge-muted"}>
                                            {scope.Required ? "Yes" : "No"}
                                          </Badge>
                                        </td>
                                        <td>
                                          <Badge className={scope.Emphasize ? "" : "badge-muted"}>
                                            {scope.Emphasize ? "Yes" : "No"}
                                          </Badge>
                                        </td>
                                        <td>
                                          <div style={{ display: "flex", flexDirection: "column", gap: "0.25rem" }}>
                                            {(scopeClaimsMap[scope.Id] ?? []).map((c) => (
                                              <span key={c.Id} style={{ display: "flex", alignItems: "center", gap: "0.25rem" }}>
                                                <code>{c.Type}</code>
                                                <Button
                                                  type="button"
                                                  variant="ghost"
                                                  onClick={() => removeScopeClaim(scope.Id, c.Id)}
                                                  disabled={pending}
                                                >
                                                  Remove
                                                </Button>
                                              </span>
                                            ))}
                                            <div style={{ display: "flex", gap: "0.25rem", alignItems: "center", flexWrap: "wrap" }}>
                                              <Input
                                                                placeholder="e.g. capabilities"
                                                                value={scopeClaimTypeInput[scope.Id] ?? ""}
                                                                onChange={(e) =>
                                                                  setScopeClaimTypeInput((s) => ({ ...s, [scope.Id]: e.target.value }))
                                                                }
                                                                onKeyDown={(e) => e.key === "Enter" && addScopeClaim(scope.Id, resource.Id)}
                                                                style={{ maxWidth: "10rem" }}
                                                              />
                                              <Button
                                                type="button"
                                                variant="ghost"
                                                onClick={() => addScopeClaim(scope.Id, resource.Id)}
                                                disabled={pending}
                                              >
                                                Add
                                              </Button>
                                            </div>
                                          </div>
                                        </td>
                                        <td>
                                          <div className="table-actions">
                                            <Button
                                              type="button"
                                              variant="ghost"
                                              onClick={() => openScopeEdit(scope)}
                                            >
                                              Edit
                                            </Button>
                                            <Button
                                              type="button"
                                              variant="danger"
                                              onClick={() => setDeleteScopeId(scope.Id)}
                                            >
                                              Delete
                                            </Button>
                                          </div>
                                        </td>
                                      </tr>
                                    ))}
                                  </tbody>
                                </table>
                              </div>
                            </div>
                          </div>
                        ) : null}
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        </div>
      </section>

      <Dialog
        open={resourceModalOpen}
        title={resourceForm.id === "00000000-0000-0000-0000-000000000000" ? "Create Resource" : "Edit Resource"}
        onClose={() => setResourceModalOpen(false)}
      >
        <div className="form-grid">
          <div className="form-row">
            <label>Name</label>
            <Input
              value={resourceForm.name}
              onChange={(event) => setResourceForm((state) => ({ ...state, name: event.target.value }))}
            />
          </div>
          <div className="form-row">
            <label>Display Name</label>
            <Input
              value={resourceForm.displayName}
              onChange={(event) => setResourceForm((state) => ({ ...state, displayName: event.target.value }))}
            />
          </div>
          <div className="form-row">
            <label>Description</label>
            <Textarea
              value={resourceForm.description}
              onChange={(event) => setResourceForm((state) => ({ ...state, description: event.target.value }))}
            />
          </div>
          <label>
            <input
              type="checkbox"
              checked={resourceForm.enabled}
              onChange={(event) => setResourceForm((state) => ({ ...state, enabled: event.target.checked }))}
            />{" "}
            Enabled
          </label>
          <div className="dialog-actions">
            <Button type="button" onClick={saveResource} disabled={pending}>
              {pending ? "Saving..." : "Save"}
            </Button>
          </div>
        </div>
      </Dialog>

      <Dialog
        open={scopeModalOpen}
        title={scopeForm.id === "00000000-0000-0000-0000-000000000000" ? "Create Scope" : "Edit Scope"}
        onClose={() => setScopeModalOpen(false)}
      >
        <div className="form-grid">
          <div className="form-row">
            <label>Name</label>
            <Input
              value={scopeForm.name}
              onChange={(event) => setScopeForm((state) => ({ ...state, name: event.target.value }))}
            />
          </div>
          <div className="form-row">
            <label>Display Name</label>
            <Input
              value={scopeForm.displayName}
              onChange={(event) => setScopeForm((state) => ({ ...state, displayName: event.target.value }))}
            />
          </div>
          <div className="form-row">
            <label>Description</label>
            <Textarea
              value={scopeForm.description}
              onChange={(event) => setScopeForm((state) => ({ ...state, description: event.target.value }))}
            />
          </div>
          <label>
            <input
              type="checkbox"
              checked={scopeForm.required}
              onChange={(event) => setScopeForm((state) => ({ ...state, required: event.target.checked }))}
            />{" "}
            Required
          </label>
          <label>
            <input
              type="checkbox"
              checked={scopeForm.emphasize}
              onChange={(event) => setScopeForm((state) => ({ ...state, emphasize: event.target.checked }))}
            />{" "}
            Emphasize
          </label>
          <div className="dialog-actions">
            <Button type="button" onClick={saveScope} disabled={pending}>
              {pending ? "Saving..." : "Save"}
            </Button>
          </div>
        </div>
      </Dialog>

      <AlertDialog
        open={Boolean(deleteResourceId)}
        title="Delete resource"
        description="This will remove the API resource and all nested scopes if no client depends on them."
        confirmLabel={pending ? "Deleting..." : "Delete"}
        onCancel={() => setDeleteResourceId(null)}
        onConfirm={confirmDeleteResource}
      />

      <AlertDialog
        open={Boolean(deleteScopeId)}
        title="Delete scope"
        description="Delete this scope only if no client has it assigned."
        confirmLabel={pending ? "Deleting..." : "Delete"}
        onCancel={() => setDeleteScopeId(null)}
        onConfirm={confirmDeleteScope}
      />
    </>
  );
}
