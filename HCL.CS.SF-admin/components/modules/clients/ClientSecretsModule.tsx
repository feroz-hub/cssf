"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState, useTransition } from "react";

import { rotateClientSecretAction } from "@/app/admin/clients/actions";
import { AlertDialog } from "@/components/ui/alert-dialog";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { useToast } from "@/components/ui/toaster";
import { notifyClientActionError } from "@/lib/clientActionErrors";
import { formatUtcDateTime } from "@/lib/utils";

type Props = {
  clientId: string;
  clientName: string;
  secretExpiresAt: string;
};

export function ClientSecretsModule({ clientId, clientName, secretExpiresAt }: Props) {
  const router = useRouter();
  const { notify } = useToast();
  const [confirmRotate, setConfirmRotate] = useState(false);
  const [generatedSecret, setGeneratedSecret] = useState<string | null>(null);
  const [pending, startTransition] = useTransition();

  const rotate = () => {
    startTransition(async () => {
      try {
        const result = await rotateClientSecretAction({ clientId });
        if (!result.ok) {
          notify(result.message, "error");
          return;
        }

        setGeneratedSecret(result.data?.secret ?? null);
        notify(result.message, "success");
        setConfirmRotate(false);
        router.refresh();
      } catch (error) {
        notifyClientActionError(error, notify, "Client secret rotation failed.");
      }
    });
  };

  const copy = async () => {
    if (!generatedSecret) {
      return;
    }

    await navigator.clipboard.writeText(generatedSecret);
    notify("Secret copied to clipboard.", "success");
  };

  return (
    <section className="card">
      <header className="card-head">
        <div>
          <h2>Client Secrets</h2>
          <p className="inline-message">{clientName} ({clientId})</p>
        </div>
        <div className="toolbar">
          <Button type="button" variant="secondary" onClick={() => setConfirmRotate(true)}>
            Generate New Secret
          </Button>
          <Link href="/admin/clients">
            <Button type="button" variant="ghost">
              Back
            </Button>
          </Link>
        </div>
      </header>

      <div className="card-body" style={{ display: "grid", gap: "0.9rem" }}>
        <div className="table-wrap">
          <table className="table">
            <thead>
              <tr className="table-row">
                <th>Secret Type</th>
                <th>Status</th>
                <th>Expires At</th>
                <th>Action</th>
              </tr>
            </thead>
            <tbody>
              <tr>
                <td>Primary Client Secret</td>
                <td>
                  <Badge>Active</Badge>
                </td>
                <td>{formatUtcDateTime(secretExpiresAt)}</td>
                <td>
                  <span className="inline-message">
                    HCL.CS.SF API exposes one active secret. Rotation revokes the previous value automatically.
                  </span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        {generatedSecret ? (
          <div className="card">
            <div className="card-body" style={{ display: "grid", gap: "0.6rem" }}>
              <p className="inline-message success">Secret value shown once. Copy before closing.</p>
              <Input value={generatedSecret} readOnly />
              <div className="toolbar">
                <Button type="button" variant="secondary" onClick={copy}>
                  Copy Secret
                </Button>
                <Button type="button" variant="ghost" onClick={() => setGeneratedSecret(null)}>
                  Dismiss
                </Button>
              </div>
            </div>
          </div>
        ) : null}
      </div>

      <AlertDialog
        open={confirmRotate}
        title="Rotate client secret"
        description="Current secret will be revoked and replaced. Continue?"
        confirmLabel={pending ? "Rotating..." : "Rotate"}
        onCancel={() => setConfirmRotate(false)}
        onConfirm={rotate}
      />
    </section>
  );
}
