"use client";

import { useState, useTransition } from "react";

import {
  authorizationCodeExchangeAction,
  clientCredentialsFlowAction,
  getAuthorizeUrlAction,
  introspectTokenAction,
  resourceOwnerPasswordFlowAction,
  revokeTokenAction
} from "@/app/admin/operations/endpoints/actions";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { useToast } from "@/components/ui/toaster";
import { generatePkce } from "@/lib/pkce";

type FlowKey =
  | "clientCredentials"
  | "authorizationCode"
  | "resourceOwnerPassword"
  | "introspect"
  | "revocation";

type ResultState = {
  message: string;
  status?: string;
  body?: string;
};

function formatBody(value: unknown): string {
  if (value == null) return "";
  if (typeof value === "string") return value;
  try {
    return JSON.stringify(value, null, 2);
  } catch {
    return String(value);
  }
}

/** Generate a random nonce for OIDC (required when scope includes openid). Max 300 chars. */
function randomNonce(): string {
  if (typeof crypto !== "undefined" && crypto.randomUUID) {
    return crypto.randomUUID();
  }
  return Math.random().toString(36).slice(2) + Date.now().toString(36);
}

export function EndpointsModule() {
  const { notify } = useToast();
  const [flow, setFlow] = useState<FlowKey>("clientCredentials");
  const [pending, startTransition] = useTransition();
  const [result, setResult] = useState<ResultState | null>(null);

  const [clientId, setClientId] = useState("");
  const [clientSecret, setClientSecret] = useState("");
  const [scope, setScope] = useState("openid profile email offline_access");

  const [userName, setUserName] = useState("");
  const [password, setPassword] = useState("");

  const [token, setToken] = useState("");
  const [tokenTypeHint, setTokenTypeHint] = useState("access_token");

  const [redirectUri, setRedirectUri] = useState("https://localhost:3000/callback");
  const [state, setState] = useState("");
  const [nonce, setNonce] = useState("");
  const [codeChallenge, setCodeChallenge] = useState("");
  const [codeChallengeMethod, setCodeChallengeMethod] = useState("S256");
  const [authCode, setAuthCode] = useState("");
  const [codeVerifier, setCodeVerifier] = useState("");
  const [authorizeUrl, setAuthorizeUrl] = useState("");

  const runClientCredentials = () => {
    startTransition(async () => {
      const response = await clientCredentialsFlowAction({ clientId, clientSecret, scope });
      if (!response.ok) {
        notify(response.message, "error");
      } else {
        notify(response.message, "success");
      }
      setResult({
        message: response.message,
        body: formatBody(response.data)
      });
    });
  };

  const runResourceOwnerPassword = () => {
    startTransition(async () => {
      const response = await resourceOwnerPasswordFlowAction({
        userName,
        password,
        scope,
        clientId: clientId || undefined,
        clientSecret: clientSecret || undefined
      });
      if (!response.ok) {
        notify(response.message, "error");
      } else {
        notify(response.message, "success");
      }
      setResult({
        message: response.message,
        body: formatBody(response.data)
      });
    });
  };

  const runIntrospect = () => {
    startTransition(async () => {
      const response = await introspectTokenAction({
        token,
        tokenTypeHint: tokenTypeHint || undefined,
        clientId: clientId || undefined,
        clientSecret: clientSecret || undefined
      });
      if (!response.ok) {
        notify(response.message, "error");
      } else {
        notify(response.message, "success");
      }
      setResult({
        message: response.message,
        body: formatBody(response.data)
      });
    });
  };

  const runRevocation = () => {
    startTransition(async () => {
      const response = await revokeTokenAction({
        token,
        tokenTypeHint: tokenTypeHint || undefined,
        clientId: clientId || undefined,
        clientSecret: clientSecret || undefined
      });
      if (!response.ok) {
        notify(response.message, "error");
      } else {
        notify(response.message, "success");
      }
      setResult({
        message: response.message,
        body: formatBody(response.data)
      });
    });
  };
  const generatePkcePair = () => {
    startTransition(async () => {
      try {
        const { codeVerifier: v, codeChallenge: c } = await generatePkce();
        setCodeVerifier(v);
        setCodeChallenge(c);
        setCodeChallengeMethod("S256");
        notify("PKCE pair generated. Use Build Authorize URL, then exchange the code with the same verifier.", "success");
        setResult({ message: "PKCE generated. Code verifier and challenge are filled. Build the authorize URL, then after login paste the code and click Exchange." });
      } catch (e) {
        notify(e instanceof Error ? e.message : "PKCE generation failed.", "error");
        setResult({ message: "PKCE generation failed." });
      }
    });
  };

  const buildAuthorizeUrl = () => {
    startTransition(async () => {
      const scopeIncludesOpenId = scope.split(/\s+/).some((s) => s.toLowerCase() === "openid");
      const effectiveNonce = nonce.trim() || (scopeIncludesOpenId ? randomNonce() : undefined);
      if (scopeIncludesOpenId && effectiveNonce) setNonce(effectiveNonce);

      const response = await getAuthorizeUrlAction({
        clientId,
        redirectUri,
        scope,
        state: state || undefined,
        nonce: effectiveNonce || undefined,
        codeChallenge: codeChallenge || undefined,
        codeChallengeMethod: codeChallengeMethod || undefined
      });
      if (!response.ok) {
        notify(response.message, "error");
        setResult({ message: response.message });
        return;
      }
      const url = response.data?.url ?? "";
      setAuthorizeUrl(url);
      setResult({ message: "Authorize URL ready. Open it in a browser, sign in, then paste the code from the callback.", body: url });
      notify("Authorize URL built.", "success");
    });
  };

  const runAuthorizationCodeExchange = () => {
    startTransition(async () => {
      const response = await authorizationCodeExchangeAction({
        code: authCode,
        redirectUri,
        clientId,
        clientSecret,
        codeVerifier: codeVerifier || undefined
      });
      if (!response.ok) {
        notify(response.message, "error");
      } else {
        notify(response.message, "success");
      }
      setResult({
        message: response.message,
        body: formatBody(response.data)
      });
    });
  };

  const renderForm = () => {
    if (flow === "clientCredentials") {
      return (
        <>
          <p className="inline-message">
            Request an access token using the Client Credentials grant against <code>/security/token</code>.
          </p>
          <div className="form-grid">
            <div className="form-row">
              <label>Client ID</label>
              <Input value={clientId} onChange={(event) => setClientId(event.target.value)} />
            </div>
            <div className="form-row">
              <label>Client Secret</label>
              <Input
                type="password"
                value={clientSecret}
                onChange={(event) => setClientSecret(event.target.value)}
              />
            </div>
            <div className="form-row">
              <label>Scope</label>
              <Input value={scope} onChange={(event) => setScope(event.target.value)} />
            </div>
            <div>
              <Button type="button" onClick={runClientCredentials} disabled={pending}>
                {pending ? "Requesting..." : "Request Token"}
              </Button>
            </div>
          </div>
        </>
      );
    }

    if (flow === "authorizationCode") {
      return (
        <>
          <p className="inline-message">
            Two steps: (1) Build the authorize URL and open it to get an authorization code. (2) Exchange the code for
            tokens. Use the same <code>redirect_uri</code> in both steps. Optional: PKCE with <code>code_challenge</code>{" "}
            / <code>code_verifier</code>.
          </p>
          <div className="form-grid">
            <div className="form-row">
              <label>Client ID</label>
              <Input value={clientId} onChange={(e) => setClientId(e.target.value)} />
            </div>
            <div className="form-row">
              <label>Client Secret</label>
              <Input
                type="password"
                value={clientSecret}
                onChange={(e) => setClientSecret(e.target.value)}
              />
            </div>
            <div className="form-row">
              <label>Redirect URI</label>
              <Input value={redirectUri} onChange={(e) => setRedirectUri(e.target.value)} />
            </div>
            <div className="form-row">
              <label>Scope</label>
              <Input value={scope} onChange={(e) => setScope(e.target.value)} />
            </div>
            <div className="form-row">
              <label>State (optional)</label>
              <Input value={state} onChange={(e) => setState(e.target.value)} placeholder="e.g. random-state" />
            </div>
            <div className="form-row">
              <label>Nonce (optional, for id_token)</label>
              <Input value={nonce} onChange={(e) => setNonce(e.target.value)} placeholder="e.g. random-nonce" />
            </div>
            <div className="form-row">
              <label>Code challenge (optional, PKCE)</label>
              <Input value={codeChallenge} onChange={(e) => setCodeChallenge(e.target.value)} placeholder="From Generate PKCE or paste" />
            </div>
            <div>
              <Button type="button" variant="secondary" onClick={generatePkcePair} disabled={pending}>
                {pending ? "Generating..." : "Generate PKCE pair"}
              </Button>
            </div>
            <div className="form-row">
              <label>Code challenge method (optional)</label>
              <Input
                value={codeChallengeMethod}
                onChange={(e) => setCodeChallengeMethod(e.target.value)}
                placeholder="S256 or plain"
              />
            </div>
            <div>
              <Button type="button" onClick={buildAuthorizeUrl} disabled={pending}>
                {pending ? "Building..." : "Build Authorize URL"}
              </Button>
              {authorizeUrl ? (
                <Button
                  type="button"
                  variant="secondary"
                  style={{ marginLeft: "0.5rem" }}
                  onClick={() => window.open(authorizeUrl, "_blank")}
                >
                  Open in new tab
                </Button>
              ) : null}
            </div>
            <div className="form-row" style={{ gridColumn: "1 / -1", marginTop: "0.5rem", paddingTop: "0.75rem", borderTop: "1px solid var(--border-subtle)" }}>
              <label>Authorization code (from callback <code>?code=...</code>)</label>
              <Input value={authCode} onChange={(e) => setAuthCode(e.target.value)} placeholder="Paste the code from redirect" />
            </div>
            <div className="form-row">
              <label>Code verifier (required if PKCE was used)</label>
              <Input value={codeVerifier} onChange={(e) => setCodeVerifier(e.target.value)} />
            </div>
            <div>
              <Button type="button" onClick={runAuthorizationCodeExchange} disabled={pending}>
                {pending ? "Exchanging..." : "Exchange code for tokens"}
              </Button>
            </div>
          </div>
        </>
      );
    }

    if (flow === "resourceOwnerPassword") {
      return (
        <>
          <p className="inline-message">
            Resource Owner Password grant for demo/testing. Requires a client allowed for <code>password</code> grant.
          </p>
          <div className="form-grid">
            <div className="form-row">
              <label>Username</label>
              <Input value={userName} onChange={(event) => setUserName(event.target.value)} />
            </div>
            <div className="form-row">
              <label>Password</label>
              <Input type="password" value={password} onChange={(event) => setPassword(event.target.value)} />
            </div>
            <div className="form-row">
              <label>Client ID (optional)</label>
              <Input value={clientId} onChange={(event) => setClientId(event.target.value)} />
            </div>
            <div className="form-row">
              <label>Client Secret (optional)</label>
              <Input
                type="password"
                value={clientSecret}
                onChange={(event) => setClientSecret(event.target.value)}
              />
            </div>
            <div>
              <Button type="button" onClick={runResourceOwnerPassword} disabled={pending}>
                {pending ? "Requesting..." : "Generate Token"}
              </Button>
            </div>
          </div>
        </>
      );
    }

    if (flow === "introspect") {
      return (
        <>
          <p className="inline-message">
            Introspect an access or refresh token via <code>/security/introspect</code>.
          </p>
          <div className="form-grid">
            <div className="form-row">
              <label>Token</label>
              <Textarea value={token} onChange={(event) => setToken(event.target.value)} rows={4} />
            </div>
            <div className="form-row">
              <label>Token Type Hint</label>
              <Input
                value={tokenTypeHint}
                onChange={(event) => setTokenTypeHint(event.target.value)}
                placeholder="access_token or refresh_token"
              />
            </div>
            <div className="form-row">
              <label>Client ID (optional)</label>
              <Input value={clientId} onChange={(event) => setClientId(event.target.value)} />
            </div>
            <div className="form-row">
              <label>Client Secret (optional)</label>
              <Input
                type="password"
                value={clientSecret}
                onChange={(event) => setClientSecret(event.target.value)}
              />
            </div>
            <div>
              <Button type="button" onClick={runIntrospect} disabled={pending}>
                {pending ? "Requesting..." : "Introspect Token"}
              </Button>
            </div>
          </div>
        </>
      );
    }

    return (
      <>
        <p className="inline-message">
          Revoke an access or refresh token via <code>/security/revocation</code>.
        </p>
        <div className="form-grid">
          <div className="form-row">
            <label>Token</label>
            <Textarea value={token} onChange={(event) => setToken(event.target.value)} rows={4} />
          </div>
          <div className="form-row">
            <label>Token Type Hint</label>
            <Input
              value={tokenTypeHint}
              onChange={(event) => setTokenTypeHint(event.target.value)}
              placeholder="access_token or refresh_token"
            />
          </div>
          <div className="form-row">
            <label>Client ID (optional)</label>
            <Input value={clientId} onChange={(event) => setClientId(event.target.value)} />
          </div>
          <div className="form-row">
            <label>Client Secret (optional)</label>
            <Input
              type="password"
              value={clientSecret}
              onChange={(event) => setClientSecret(event.target.value)}
            />
          </div>
          <div>
            <Button type="button" onClick={runRevocation} disabled={pending}>
              {pending ? "Requesting..." : "Revoke Token"}
            </Button>
          </div>
        </div>
      </>
    );
  };

  return (
    <section className="card">
      <header className="card-head">
        <div>
          <h2 className="text-heading">Endpoints</h2>
          <p className="inline-message">
            Guided actions for HCL.CS.SF OAuth/OIDC endpoints, inspired by the MVC demo client.
          </p>
        </div>
      </header>
      <div className="card-body" style={{ display: "grid", gridTemplateColumns: "minmax(0, 220px) minmax(0, 1fr)", gap: "1rem" }}>
        <aside style={{ borderRight: "1px solid var(--border-subtle)", paddingRight: "1rem" }}>
          <div className="toolbar" style={{ flexDirection: "column", alignItems: "stretch", gap: "0.4rem" }}>
            <Button
              type="button"
              variant={flow === "clientCredentials" ? "secondary" : "ghost"}
              onClick={() => setFlow("clientCredentials")}
            >
              Client Credentials
            </Button>
            <Button
              type="button"
              variant={flow === "authorizationCode" ? "secondary" : "ghost"}
              onClick={() => setFlow("authorizationCode")}
            >
              Authorization Code
            </Button>
            <Button
              type="button"
              variant={flow === "resourceOwnerPassword" ? "secondary" : "ghost"}
              onClick={() => setFlow("resourceOwnerPassword")}
            >
              Resource Owner Password
            </Button>
            <Button
              type="button"
              variant={flow === "introspect" ? "secondary" : "ghost"}
              onClick={() => setFlow("introspect")}
            >
              Introspect Token
            </Button>
            <Button
              type="button"
              variant={flow === "revocation" ? "secondary" : "ghost"}
              onClick={() => setFlow("revocation")}
            >
              Revoke Token
            </Button>
          </div>
        </aside>
        <div style={{ display: "grid", gap: "1rem" }}>
          <div className="card">
            <div className="card-body" style={{ display: "grid", gap: "0.75rem" }}>{renderForm()}</div>
          </div>
          <div className="card">
            <div className="card-head">
              <h3>Result</h3>
            </div>
            <div className="card-body">
              {result ? (
                <>
                  <p className="inline-message">{result.message}</p>
                  {result.body ? (
                    <pre
                      style={{
                        marginTop: "0.75rem",
                        maxHeight: 320,
                        overflow: "auto",
                        fontSize: "0.8rem"
                      }}
                    >
                      {result.body}
                    </pre>
                  ) : null}
                </>
              ) : (
                <p className="inline-message">No requests executed yet.</p>
              )}
            </div>
          </div>
        </div>
      </div>
    </section>
  );
}

