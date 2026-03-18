import Link from "next/link";
import { notFound, redirect } from "next/navigation";

import { Button } from "@/components/ui/button";
import { HCLCSSFApiError } from "@/lib/api/client";
import { getClient } from "@/lib/api/clients";

export default async function ClientDetailPage({ params }: { params: { id: string } }) {
  const clientId = decodeURIComponent(params.id);

  try {
    const client = await getClient(clientId);

    return (
      <section className="card">
        <header className="card-head">
          <div>
            <h2>{client.ClientName}</h2>
            <p className="inline-message">Client ID: {client.ClientId}</p>
          </div>
          <div className="toolbar">
            <Link href={`/admin/clients/${encodeURIComponent(client.ClientId)}/secrets`}>
              <Button type="button" variant="secondary">
                Manage Secrets
              </Button>
            </Link>
            <Link href="/admin/clients">
              <Button type="button" variant="ghost">
                Back
              </Button>
            </Link>
          </div>
        </header>
        <div className="card-body">
          <div className="table-wrap">
            <table className="table">
              <tbody>
                <tr>
                  <th>Name</th>
                  <td>{client.ClientName}</td>
                </tr>
                <tr>
                  <th>Type</th>
                  <td>{client.ApplicationType}</td>
                </tr>
                <tr>
                  <th>Scopes</th>
                  <td>{client.AllowedScopes.join(", ")}</td>
                </tr>
                <tr>
                  <th>Grant Types</th>
                  <td>{client.SupportedGrantTypes.join(", ")}</td>
                </tr>
                <tr>
                  <th>Redirect URIs</th>
                  <td>{client.RedirectUris.map((uri) => uri.RedirectUri).join(", ")}</td>
                </tr>
                <tr>
                  <th>Post Logout URIs</th>
                  <td>{client.PostLogoutRedirectUris.map((uri) => uri.PostLogoutRedirectUri).join(", ")}</td>
                </tr>
                <tr>
                  <th>Access Token Lifetime</th>
                  <td>{client.AccessTokenExpiration}</td>
                </tr>
                <tr>
                  <th>Refresh Token Lifetime</th>
                  <td>{client.RefreshTokenExpiration}</td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </section>
    );
  } catch (error) {
    if (error instanceof HCLCSSFApiError && error.statusCode === 401) {
      redirect("/login");
    }
    notFound();
  }
}
