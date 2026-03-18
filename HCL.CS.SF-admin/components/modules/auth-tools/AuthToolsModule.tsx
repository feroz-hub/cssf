"use client";

import { useState, useTransition } from "react";

import {
  countRecoveryCodesAction,
  generateRecoveryCodesAction,
  passwordSignInAction,
  resetAuthenticatorAppAction,
  setupAuthenticatorAppAction,
  verifyAuthenticatorAppSetupAction
} from "@/app/admin/auth-tools/actions";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { useToast } from "@/components/ui/toaster";

function JsonBox({ value }: { value: unknown }) {
  return (
    <Textarea
      readOnly
      value={value ? JSON.stringify(value, null, 2) : ""}
      style={{ minHeight: 180, fontFamily: "ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace" }}
    />
  );
}

export function AuthToolsModule() {
  const { notify } = useToast();
  const [pending, startTransition] = useTransition();

  const [signInUserName, setSignInUserName] = useState("");
  const [signInPassword, setSignInPassword] = useState("");
  const [userId, setUserId] = useState("");
  const [applicationName, setApplicationName] = useState("HCL.CS.SF");
  const [userToken, setUserToken] = useState("");

  const [result, setResult] = useState<unknown>(null);

  const run = (fn: () => Promise<{ ok: boolean; message: string; data?: unknown } | { ok: boolean; message: string }>) => {
    startTransition(async () => {
      const response: any = await fn();
      if (!response.ok) {
        notify(response.message, "error");
        return;
      }
      notify(response.message, "success");
      setResult(response.data ?? null);
    });
  };

  return (
    <div style={{ display: "grid", gap: "1rem" }}>
      <section className="card">
        <header className="card-head">
          <div>
            <h2>Auth Tools</h2>
            <p className="inline-message">
              Support tooling for authentication endpoints. Use carefully in production environments.
            </p>
          </div>
        </header>

        <div className="card-body" style={{ display: "grid", gap: "0.9rem" }}>
          <div className="card" style={{ margin: 0 }}>
            <div className="card-head">
              <h3>Password sign-in</h3>
            </div>
            <div className="card-body" style={{ display: "grid", gap: "0.6rem" }}>
              <div className="form-row">
                <label className="form-label">Username</label>
                <Input value={signInUserName} onChange={(e) => setSignInUserName(e.target.value)} />
              </div>
              <div className="form-row">
                <label className="form-label">Password</label>
                <Input type="password" value={signInPassword} onChange={(e) => setSignInPassword(e.target.value)} />
              </div>
              <div className="form-row" style={{ display: "flex", justifyContent: "flex-end" }}>
                <Button
                  type="button"
                  onClick={() => run(() => passwordSignInAction({ user_name: signInUserName, password: signInPassword }))}
                  disabled={pending}
                >
                  Execute
                </Button>
              </div>
            </div>
          </div>

          <div className="card" style={{ margin: 0 }}>
            <div className="card-head">
              <h3>Authenticator app</h3>
            </div>
            <div className="card-body" style={{ display: "grid", gap: "0.6rem" }}>
              <div className="form-row">
                <label className="form-label">User Id</label>
                <Input value={userId} onChange={(e) => setUserId(e.target.value)} placeholder="GUID" />
              </div>
              <div className="form-row">
                <label className="form-label">Application name</label>
                <Input value={applicationName} onChange={(e) => setApplicationName(e.target.value)} />
              </div>
              <div className="form-row" style={{ display: "flex", justifyContent: "flex-end", gap: "0.6rem" }}>
                <Button
                  type="button"
                  variant="secondary"
                  onClick={() => run(() => setupAuthenticatorAppAction({ user_id: userId, application_name: applicationName }))}
                  disabled={pending}
                >
                  Setup
                </Button>
                <Button
                  type="button"
                  variant="danger"
                  onClick={() => run(() => resetAuthenticatorAppAction({ user_id: userId }))}
                  disabled={pending}
                >
                  Reset
                </Button>
              </div>

              <div className="form-row">
                <label className="form-label">Verification token</label>
                <Input value={userToken} onChange={(e) => setUserToken(e.target.value)} />
              </div>
              <div className="form-row" style={{ display: "flex", justifyContent: "flex-end" }}>
                <Button
                  type="button"
                  onClick={() => run(() => verifyAuthenticatorAppSetupAction({ user_id: userId, user_token: userToken }))}
                  disabled={pending}
                >
                  Verify setup
                </Button>
              </div>
            </div>
          </div>

          <div className="card" style={{ margin: 0 }}>
            <div className="card-head">
              <h3>Recovery codes</h3>
            </div>
            <div className="card-body" style={{ display: "grid", gap: "0.6rem" }}>
              <div className="form-row">
                <label className="form-label">User Id</label>
                <Input value={userId} onChange={(e) => setUserId(e.target.value)} placeholder="GUID" />
              </div>
              <div className="form-row" style={{ display: "flex", justifyContent: "flex-end", gap: "0.6rem" }}>
                <Button
                  type="button"
                  variant="secondary"
                  onClick={() => run(() => countRecoveryCodesAction({ user_id: userId }))}
                  disabled={pending}
                >
                  Count
                </Button>
                <Button
                  type="button"
                  onClick={() => run(() => generateRecoveryCodesAction({ user_id: userId }))}
                  disabled={pending}
                >
                  Generate
                </Button>
              </div>
            </div>
          </div>

          <div className="card" style={{ margin: 0 }}>
            <div className="card-head">
              <h3>Last result</h3>
            </div>
            <div className="card-body">
              <JsonBox value={result} />
            </div>
          </div>
        </div>
      </section>
    </div>
  );
}

