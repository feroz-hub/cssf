"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useMemo, useState, useTransition } from "react";

import {
  addIdentityClaimAction,
  createIdentityResourceAction,
  deleteIdentityClaimAction,
  deleteIdentityResourceAction,
  updateIdentityResourceAction
} from "@/app/admin/identity-resources/actions";
import { ConfirmDialog } from "@/components/admin/ConfirmDialog";
import { SignInAgainButton } from "@/components/admin/SignInAgainButton";
import { FormDialog } from "@/components/admin/FormDialog";
import { PageHeader } from "@/components/admin/PageHeader";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { useToast } from "@/components/ui/toaster";
import { type IdentityClaimsModel, type IdentityResourcesModel } from "@/lib/types/HCL.CS.SF";

type ResourceForm = {
  id: string;
  name: string;
  displayName: string;
  description: string;
  enabled: boolean;
  required: boolean;
  emphasize: boolean;
};

type ClaimForm = {
  id: string;
  identityResourceId: string;
  type: string;
  aliasType: string;
};

const ZERO_GUID = "00000000-0000-0000-0000-000000000000";

const defaultResourceForm: ResourceForm = {
  id: ZERO_GUID,
  name: "",
  displayName: "",
  description: "",
  enabled: true,
  required: false,
  emphasize: false
};

const defaultClaimForm: ClaimForm = {
  id: ZERO_GUID,
  identityResourceId: "",
  type: "",
  aliasType: ""
};

type Props = {
  resources: IdentityResourcesModel[];
  loadError?: string;
  loadErrorIsUnauthorized?: boolean;
};

export function IdentityResourcesModule({ resources, loadError, loadErrorIsUnauthorized }: Props) {
  const router = useRouter();
  const { notify } = useToast();
  const [pending, startTransition] = useTransition();

  const [expandedResourceId, setExpandedResourceId] = useState<string | null>(null);
  const [resourceForm, setResourceForm] = useState<ResourceForm>(defaultResourceForm);
  const [claimForm, setClaimForm] = useState<ClaimForm>(defaultClaimForm);
  const [resourceModalOpen, setResourceModalOpen] = useState(false);
  const [claimModalOpen, setClaimModalOpen] = useState(false);
  const [deleteResourceId, setDeleteResourceId] = useState<string | null>(null);
  const [deleteClaimId, setDeleteClaimId] = useState<string | null>(null);

  const sortedResources = useMemo(
    () => [...resources].sort((a, b) => a.Name.localeCompare(b.Name)),
    [resources]
  );

  const openCreate = () => {
    setResourceForm(defaultResourceForm);
    setResourceModalOpen(true);
  };

  const openEdit = (resource: IdentityResourcesModel) => {
    setResourceForm({
      id: resource.Id,
      name: resource.Name,
      displayName: resource.DisplayName,
      description: resource.Description,
      enabled: resource.Enabled,
      required: resource.Required,
      emphasize: resource.Emphasize
    });
    setResourceModalOpen(true);
  };

  const openClaimCreate = (resourceId: string) => {
    setClaimForm({ ...defaultClaimForm, identityResourceId: resourceId });
    setClaimModalOpen(true);
  };

  const saveResource = () => {
    startTransition(async () => {
      const action = resourceForm.id === ZERO_GUID ? createIdentityResourceAction : updateIdentityResourceAction;
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

  const saveClaim = () => {
    startTransition(async () => {
      const result = await addIdentityClaimAction(claimForm);
      if (!result.ok) {
        notify(result.message, "error");
        return;
      }
      notify(result.message, "success");
      setClaimModalOpen(false);
      router.refresh();
    });
  };

  const confirmDeleteResource = () => {
    if (!deleteResourceId) {
      return;
    }
    startTransition(async () => {
      const result = await deleteIdentityResourceAction({ id: deleteResourceId });
      if (!result.ok) {
        notify(result.message, "error");
        return;
      }
      notify(result.message, "success");
      setDeleteResourceId(null);
      router.refresh();
    });
  };

  const confirmDeleteClaim = () => {
    if (!deleteClaimId) {
      return;
    }
    startTransition(async () => {
      const result = await deleteIdentityClaimAction({ id: deleteClaimId });
      if (!result.ok) {
        notify(result.message, "error");
        return;
      }
      notify(result.message, "success");
      setDeleteClaimId(null);
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
        <PageHeader
          title="Identity Resources"
          subtitle="Manage identity resources and their claim types."
          actions={
            <Button type="button" variant="secondary" onClick={openCreate}>
              Create Identity Resource
            </Button>
          }
        />

        <div className="card-body">
          <div className="table-wrap">
            <table className="table">
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Display Name</th>
                  <th>Claims Count</th>
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
                      <td>{resource.IdentityClaims?.length ?? 0}</td>
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
                            onClick={() => setExpandedResourceId(expanded ? null : resource.Id)}
                          >
                            {expanded ? "Hide Claims" : "Claims"}
                          </Button>
                          <Button type="button" variant="ghost" onClick={() => openEdit(resource)}>
                            Edit
                          </Button>
                          <Button type="button" variant="danger" onClick={() => setDeleteResourceId(resource.Id)}>
                            Delete
                          </Button>
                        </div>

                        {expanded ? (
                          <div className="card" style={{ marginTop: "0.8rem" }}>
                            <div className="card-head">
                              <h3>Claims</h3>
                              <Button type="button" variant="secondary" onClick={() => openClaimCreate(resource.Id)}>
                                Add Claim
                              </Button>
                            </div>
                            <div className="card-body">
                              <div className="table-wrap">
                                <table className="table">
                                  <thead>
                                    <tr>
                                      <th>Type</th>
                                      <th>Alias</th>
                                      <th>Actions</th>
                                    </tr>
                                  </thead>
                                  <tbody>
                                    {(resource.IdentityClaims ?? []).map((claim: IdentityClaimsModel) => (
                                      <tr key={claim.Id} className="table-row">
                                        <td>{claim.Type}</td>
                                        <td>{claim.AliasType}</td>
                                        <td>
                                          <div className="table-actions">
                                            <Button
                                              type="button"
                                              variant="danger"
                                              onClick={() => setDeleteClaimId(claim.Id)}
                                            >
                                              Delete
                                            </Button>
                                          </div>
                                        </td>
                                      </tr>
                                    ))}

                                    {(resource.IdentityClaims ?? []).length === 0 ? (
                                      <tr>
                                        <td colSpan={3}>
                                          <p className="inline-message">No claims defined.</p>
                                        </td>
                                      </tr>
                                    ) : null}
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

                {sortedResources.length === 0 ? (
                  <tr>
                    <td colSpan={5}>
                      <p className="inline-message">No identity resources yet.</p>
                    </td>
                  </tr>
                ) : null}
              </tbody>
            </table>
          </div>
        </div>
      </section>

      <FormDialog
        open={resourceModalOpen}
        title="Identity Resource"
        pending={pending}
        onClose={() => setResourceModalOpen(false)}
        onSubmit={saveResource}
      >
        <div className="form-row">
          <label className="form-label">Name</label>
          <Input value={resourceForm.name} onChange={(e) => setResourceForm((v) => ({ ...v, name: e.target.value }))} />
        </div>
        <div className="form-row">
          <label className="form-label">Display Name</label>
          <Input
            value={resourceForm.displayName}
            onChange={(e) => setResourceForm((v) => ({ ...v, displayName: e.target.value }))}
          />
        </div>
        <div className="form-row">
          <label className="form-label">Description</label>
          <Textarea
            value={resourceForm.description}
            onChange={(e) => setResourceForm((v) => ({ ...v, description: e.target.value }))}
          />
        </div>
        <div className="form-row" style={{ display: "flex", gap: "1rem", alignItems: "center" }}>
          <label className="form-label" style={{ minWidth: 110 }}>
            Enabled
          </label>
          <input
            type="checkbox"
            checked={resourceForm.enabled}
            onChange={(e) => setResourceForm((v) => ({ ...v, enabled: e.target.checked }))}
          />
          <label className="form-label" style={{ minWidth: 110 }}>
            Required
          </label>
          <input
            type="checkbox"
            checked={resourceForm.required}
            onChange={(e) => setResourceForm((v) => ({ ...v, required: e.target.checked }))}
          />
          <label className="form-label" style={{ minWidth: 110 }}>
            Emphasize
          </label>
          <input
            type="checkbox"
            checked={resourceForm.emphasize}
            onChange={(e) => setResourceForm((v) => ({ ...v, emphasize: e.target.checked }))}
          />
        </div>
      </FormDialog>

      <FormDialog
        open={claimModalOpen}
        title="Add Identity Claim"
        submitLabel="Add"
        pending={pending}
        onClose={() => setClaimModalOpen(false)}
        onSubmit={saveClaim}
      >
        <div className="form-row">
          <label className="form-label">Type</label>
          <Input value={claimForm.type} onChange={(e) => setClaimForm((v) => ({ ...v, type: e.target.value }))} />
        </div>
        <div className="form-row">
          <label className="form-label">Alias</label>
          <Input
            value={claimForm.aliasType}
            onChange={(e) => setClaimForm((v) => ({ ...v, aliasType: e.target.value }))}
          />
        </div>
      </FormDialog>

      <ConfirmDialog
        open={deleteResourceId !== null}
        title="Delete identity resource?"
        description="This will permanently remove the identity resource and its claim definitions."
        confirmLabel={pending ? "Deleting..." : "Delete"}
        pending={pending}
        onCancel={() => setDeleteResourceId(null)}
        onConfirm={confirmDeleteResource}
      />

      <ConfirmDialog
        open={deleteClaimId !== null}
        title="Delete identity claim?"
        description="This will permanently remove the claim from the identity resource."
        confirmLabel={pending ? "Deleting..." : "Delete"}
        pending={pending}
        onCancel={() => setDeleteClaimId(null)}
        onConfirm={confirmDeleteClaim}
      />
    </>
  );
}

