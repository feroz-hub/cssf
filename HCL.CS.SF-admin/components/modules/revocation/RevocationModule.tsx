"use client";

import { useState, useTransition } from "react";

import {
  bulkRevokeByClientAction,
  bulkRevokeByUserAction,
  revokeTokenAction,
  searchRevocationAction
} from "@/app/admin/revocation/actions";
import { AlertDialog } from "@/components/ui/alert-dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { useToast } from "@/components/ui/toaster";
import { type TokenModel } from "@/lib/types/HCL.CS.SF";
import { formatUtcDateTime } from "@/lib/utils";

export function RevocationModule() {
  const { notify } = useToast();

  const [subject, setSubject] = useState("");
  const [clientId, setClientId] = useState("");
  const [tokenJti, setTokenJti] = useState("");
  const [results, setResults] = useState<TokenModel[]>([]);
  const [selected, setSelected] = useState<TokenModel | null>(null);
  const [bulkMode, setBulkMode] = useState<"subject" | "client" | null>(null);
  const [pending, startTransition] = useTransition();

  const search = () => {
    startTransition(async () => {
      const result = await searchRevocationAction({
        subject,
        clientId,
        tokenJti
      });

      if (!result.ok) {
        notify(result.message, "error");
        return;
      }

      setResults(result.data ?? []);
      notify(result.message, "success");
    });
  };

  const revokeOne = () => {
    if (!selected) {
      return;
    }

    startTransition(async () => {
      const result = await revokeTokenAction({
        token: selected.Token,
        tokenTypeHint: selected.TokenTypeHint
      });

      if (!result.ok) {
        notify(result.message, "error");
        return;
      }

      notify(result.message, "success");
      setSelected(null);
      setResults((items) => items.filter((item) => item.Token !== selected.Token));
    });
  };

  const revokeBulk = () => {
    startTransition(async () => {
      const result =
        bulkMode === "subject"
          ? await bulkRevokeByUserAction({ subject })
          : await bulkRevokeByClientAction({ clientId });

      if (!result.ok) {
        notify(result.message, "error");
        return;
      }

      notify(result.message, "success");
      setBulkMode(null);
      setResults([]);
    });
  };

  return (
    <>
      <section className="card">
        <header className="card-head">
          <div>
            <h2>Token & Session Revocation</h2>
            <p className="inline-message">Search active tokens and revoke individually or in bulk.</p>
          </div>
        </header>

        <div className="card-body" style={{ display: "grid", gap: "0.9rem" }}>
          <div className="form-grid" style={{ gridTemplateColumns: "repeat(auto-fit, minmax(220px, 1fr))" }}>
            <div className="form-row">
              <label>Subject (User ID)</label>
              <Input value={subject} onChange={(event) => setSubject(event.target.value)} />
            </div>
            <div className="form-row">
              <label>Client ID</label>
              <Input value={clientId} onChange={(event) => setClientId(event.target.value)} />
            </div>
            <div className="form-row">
              <label>Token JTI (contains)</label>
              <Input value={tokenJti} onChange={(event) => setTokenJti(event.target.value)} />
            </div>
          </div>

          <div className="toolbar">
            <Button type="button" variant="secondary" onClick={search} disabled={pending}>
              {pending ? "Searching..." : "Search"}
            </Button>
            <Button type="button" variant="danger" onClick={() => setBulkMode("subject")} disabled={!subject}>
              Revoke All by User
            </Button>
            <Button type="button" variant="danger" onClick={() => setBulkMode("client")} disabled={!clientId}>
              Revoke All by Client
            </Button>
          </div>

          <div className="table-wrap">
            <table className="table">
              <thead>
                <tr>
                  <th>Token Type</th>
                  <th>Issued At</th>
                  <th>Expires At</th>
                  <th>Client</th>
                  <th>Subject</th>
                  <th>Action</th>
                </tr>
              </thead>
              <tbody>
                {results.map((token) => (
                  <tr key={`${token.TokenTypeHint}-${token.Token.slice(0, 12)}`} className="table-row">
                    <td>{token.TokenTypeHint}</td>
                    <td>{formatUtcDateTime(token.LoginDateTime)}</td>
                    <td>-</td>
                    <td>{token.ClientId}</td>
                    <td>{token.UserName}</td>
                    <td>
                      <Button type="button" variant="danger" onClick={() => setSelected(token)}>
                        Revoke
                      </Button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      </section>

      <AlertDialog
        open={Boolean(selected)}
        title="Revoke token"
        description="Token will be invalidated immediately."
        confirmLabel={pending ? "Revoking..." : "Revoke"}
        onCancel={() => setSelected(null)}
        onConfirm={revokeOne}
      />

      <AlertDialog
        open={Boolean(bulkMode)}
        title="Bulk revoke"
        description={
          bulkMode === "subject"
            ? "Revoke every active token for this user."
            : "Revoke every active token for this client."
        }
        confirmLabel={pending ? "Revoking..." : "Revoke All"}
        onCancel={() => setBulkMode(null)}
        onConfirm={revokeBulk}
      />
    </>
  );
}
