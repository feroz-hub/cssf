"use client";

import Link from "next/link";
import { useState } from "react";

import { requestForgotPasswordAction } from "@/app/(auth)/login/actions";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";

export default function ForgotPasswordPage() {
  const [username, setUsername] = useState("");
  const [message, setMessage] = useState<{ type: "success" | "error"; text: string } | null>(null);
  const [pending, setPending] = useState(false);

  const submit = async () => {
    setMessage(null);
    if (!username.trim()) {
      setMessage({ type: "error", text: "Username is required." });
      return;
    }
    setPending(true);
    const result = await requestForgotPasswordAction(username);
    setPending(false);
    setMessage({ type: result.ok ? "success" : "error", text: result.message });
  };

  return (
    <main className="login-shell">
      <section className="login-card">
        <p className="kicker">HCL.CS.SF Administration</p>
        <h1>Forgot password</h1>
        <p>Enter your username. If an account exists, we will send reset instructions to your registered email.</p>

        {message ? (
          <p className={message.type === "success" ? "inline-success" : "inline-error"}>{message.text}</p>
        ) : null}

        <div className="form-grid">
          <div className="form-row">
            <label htmlFor="forgot-username">Username</label>
            <Input
              id="forgot-username"
              type="text"
              autoComplete="username"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              onKeyDown={(e) => e.key === "Enter" && void submit()}
              disabled={pending}
              placeholder="Your username"
            />
          </div>
        </div>

        <Button type="button" onClick={submit} disabled={pending}>
          {pending ? "Sending..." : "Send reset instructions"}
        </Button>

        <p style={{ marginTop: "1rem" }}>
          <Link href="/login" className="link">
            Back to sign in
          </Link>
        </p>
      </section>
    </main>
  );
}
