"use client";

import Link from "next/link";
import { useState } from "react";

import { resetPasswordAction } from "@/app/(auth)/login/actions";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";

export default function ResetPasswordPage() {
  const [username, setUsername] = useState("");
  const [token, setToken] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [message, setMessage] = useState<{ type: "success" | "error"; text: string } | null>(null);
  const [pending, setPending] = useState(false);

  const submit = async () => {
    setMessage(null);
    setPending(true);
    const result = await resetPasswordAction(username, token, newPassword, confirmPassword);
    setPending(false);
    setMessage({ type: result.ok ? "success" : "error", text: result.message });
    if (result.ok) {
      setUsername("");
      setToken("");
      setNewPassword("");
      setConfirmPassword("");
    }
  };

  return (
    <main className="login-shell">
      <section className="login-card">
        <p className="kicker">HCL.CS.SF Administration</p>
        <h1>Reset password</h1>
        <p>Enter your username, the reset token from your email, and a new password.</p>

        {message ? (
          <p className={message.type === "success" ? "inline-success" : "inline-error"}>{message.text}</p>
        ) : null}

        <div className="form-grid">
          <div className="form-row">
            <label htmlFor="reset-username">Username</label>
            <Input
              id="reset-username"
              type="text"
              autoComplete="username"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              disabled={pending}
              placeholder="Your username"
            />
          </div>
          <div className="form-row">
            <label htmlFor="reset-token">Reset token</label>
            <Input
              id="reset-token"
              type="text"
              autoComplete="one-time-code"
              value={token}
              onChange={(e) => setToken(e.target.value)}
              disabled={pending}
              placeholder="Paste the token from your email"
            />
          </div>
          <div className="form-row">
            <label htmlFor="reset-new-password">New password</label>
            <Input
              id="reset-new-password"
              type="password"
              autoComplete="new-password"
              value={newPassword}
              onChange={(e) => setNewPassword(e.target.value)}
              disabled={pending}
              placeholder="At least 8 characters, no spaces"
            />
          </div>
          <div className="form-row">
            <label htmlFor="reset-confirm">Confirm new password</label>
            <Input
              id="reset-confirm"
              type="password"
              autoComplete="new-password"
              value={confirmPassword}
              onChange={(e) => setConfirmPassword(e.target.value)}
              onKeyDown={(e) => e.key === "Enter" && void submit()}
              disabled={pending}
              placeholder="Same as above"
            />
          </div>
        </div>

        <Button type="button" onClick={submit} disabled={pending}>
          {pending ? "Resetting…" : "Reset password"}
        </Button>

        <p style={{ marginTop: "1rem" }}>
          <Link href="/login" className="link">
            Back to sign in
          </Link>
          {" · "}
          <Link href="/login/forgot-password" className="link">
            Forgot password?
          </Link>
        </p>
      </section>
    </main>
  );
}
