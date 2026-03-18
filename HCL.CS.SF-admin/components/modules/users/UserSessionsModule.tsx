"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState, useTransition } from "react";

import { revokeAllUserSessionsAction, revokeSessionAction } from "@/app/admin/users/actions";
import { AlertDialog } from "@/components/ui/alert-dialog";
import { Button } from "@/components/ui/button";
import { useToast } from "@/components/ui/toaster";
import { type TokenModel } from "@/lib/types/HCL.CS.SF";
import { formatUtcDateTime } from "@/lib/utils";

type Props = {
  userId: string;
  userName: string;
  sessions: TokenModel[];
};

export function UserSessionsModule({ userId, userName, sessions }: Props) {
  const router = useRouter();
  const { notify } = useToast();

  const [targetToken, setTargetToken] = useState<TokenModel | null>(null);
  const [bulkConfirmOpen, setBulkConfirmOpen] = useState(false);
  const [pending, startTransition] = useTransition();

  const revokeOne = () => {
    if (!targetToken) {
      return;
    }

    startTransition(async () => {
      const result = await revokeSessionAction({
        token: targetToken.Token,
        tokenTypeHint: targetToken.TokenTypeHint
      });

      if (!result.ok) {
        notify(result.message, "error");
        return;
      }

      notify(result.message, "success");
      setTargetToken(null);
      router.refresh();
    });
  };

  const revokeAll = () => {
    startTransition(async () => {
      const result = await revokeAllUserSessionsAction({ userId });
      if (!result.ok) {
        notify(result.message, "error");
        return;
      }

      notify(result.message, "success");
      setBulkConfirmOpen(false);
      router.refresh();
    });
  };

  return (
    <>
      <section className="card">
        <header className="card-head">
          <div>
            <h2>Active Sessions</h2>
            <p className="inline-message">{userName} ({userId})</p>
          </div>
          <div className="toolbar">
            <Button type="button" variant="danger" onClick={() => setBulkConfirmOpen(true)}>
              Revoke All Sessions
            </Button>
            <Link href={`/admin/users/${encodeURIComponent(userId)}`}>
              <Button type="button" variant="ghost">
                Back to User
              </Button>
            </Link>
          </div>
        </header>

        <div className="card-body">
          <div className="table-wrap">
            <table className="table">
              <thead>
                <tr>
                  <th>Token Type</th>
                  <th>Client</th>
                  <th>Last Activity</th>
                  <th>IP Address</th>
                  <th>User Agent</th>
                  <th>Action</th>
                </tr>
              </thead>
              <tbody>
                {sessions.map((session) => (
                  <tr key={`${session.TokenTypeHint}-${session.Token.slice(0, 12)}`} className="table-row">
                    <td>{session.TokenTypeHint}</td>
                    <td>{session.ClientName}</td>
                    <td>{formatUtcDateTime(session.LoginDateTime)}</td>
                    <td>-</td>
                    <td>-</td>
                    <td>
                      <Button type="button" variant="danger" onClick={() => setTargetToken(session)}>
                        Revoke
                      </Button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          <p className="inline-message">
            IP address and user-agent are not exposed by current HCL.CS.SF token APIs; fields are shown as unavailable.
          </p>
        </div>
      </section>

      <AlertDialog
        open={Boolean(targetToken)}
        title="Revoke session"
        description="This token will be invalidated immediately."
        confirmLabel={pending ? "Revoking..." : "Revoke"}
        onCancel={() => setTargetToken(null)}
        onConfirm={revokeOne}
      />

      <AlertDialog
        open={bulkConfirmOpen}
        title="Revoke all sessions"
        description="All active tokens for this user will be revoked."
        confirmLabel={pending ? "Revoking..." : "Revoke All"}
        onCancel={() => setBulkConfirmOpen(false)}
        onConfirm={revokeAll}
      />
    </>
  );
}
