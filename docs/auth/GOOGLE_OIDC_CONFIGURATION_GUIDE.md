# Google OIDC Configuration Guide — HCL.CS.SF Platform

## Part 1: Google Cloud Console Setup

### 1. Create a Google Cloud Project

> Skip if you already have one.

- Go to [console.cloud.google.com](https://console.cloud.google.com)
- Click **Select a project** → **New Project**
- Name it (e.g., "HCL.CS.SF Auth") and click **Create**

### 2. Enable the Google Identity API

- Navigate to **APIs & Services → Library**
- Search for **"Google Identity"** or **"Google+ API"**
- Click **Enable**

### 3. Configure the OAuth Consent Screen

- Go to **APIs & Services → OAuth consent screen**
- Choose **External** (or **Internal** if using Google Workspace)
- Fill in:
  - **App name**: your application name
  - **User support email**: your support email
  - **Authorized domains**: your production domain (e.g., `yourdomain.com`)
  - **Developer contact email**
- Under **Scopes**, add: `openid`, `email`, `profile`
- Save

### 4. Create OAuth 2.0 Credentials

- Go to **APIs & Services → Credentials**
- Click **Create Credentials → OAuth client ID**
- Application type: **Web application**
- Name: e.g., "HCL.CS.SF OIDC"
- **Authorized redirect URIs** — add all environments:

  ```
  https://your-app-domain.com/auth/external/google/signin-callback
  https://localhost:5001/auth/external/google/signin-callback
  ```

- Click **Create**
- **Copy the Client ID and Client Secret** — you'll need these in the next step

---

## Part 2: Configure in HCL.CS.SF Admin UI

### Prerequisites

- Your admin account must have the `HCL.CS.SF.externalauth.manage` scope
- The database migration must have run (table `HCL.CS.SF_ExternalAuthProviderConfig` exists)

### Steps

1. **Navigate** to `/admin/external-auth` in the HCL.CS.SF Admin panel (under **Security** in the sidebar)

2. **Click "Add Provider"**

3. **Select "Google"** from the Provider dropdown

4. **Fill in the configuration fields:**

   | Field | Value | Required |
   |-------|-------|----------|
   | **Client ID** | The OAuth Client ID from Google Cloud Console | Yes |
   | **Client Secret** | The OAuth Client Secret from Google Cloud Console | Yes |
   | **Authority** | `https://accounts.google.com` (pre-filled default) | No |
   | **Metadata Address** | `https://accounts.google.com/.well-known/openid-configuration` (pre-filled default) | No |
   | **Callback Path** | `/auth/external/google/signin-callback` (pre-filled default) | No |
   | **Allowed Redirect Hosts** | Comma-separated hostnames your app runs on, e.g. `localhost:3000, yourdomain.com` | No |

5. **Check "Enabled"** to activate Google login

6. **Configure Auto-Provisioning** (optional):
   - Check **"Auto-create user accounts on first login"** if you want users to be created automatically when they sign in with Google for the first time
   - In **Allowed Email Domains**, enter comma-separated domains to restrict which Google accounts can auto-provision, e.g.: `company.com, partner.org`
   - Leave domains empty to allow any Google account

7. **Click "Save"**

---

## Part 3: Test the Configuration

1. On the provider card, click **"Test"**
   - This sends an HTTP request to Google's OpenID Connect metadata endpoint
   - A **"Test Passed"** badge confirms the Authority and MetadataAddress are reachable
   - A **"Test Failed"** badge means the metadata endpoint is unreachable — check Authority and MetadataAddress values

2. **Test actual login** by navigating to your Demo Server login page and clicking the Google sign-in option

---

## Part 4: Configuration Priority (DB vs appsettings.json)

The system uses a **DB-first fallback** strategy:

1. **DB config** (via Admin UI) is checked first — if a Google provider exists and `IsEnabled = true`, its settings are used
2. **appsettings.json** (`Authentication:Google` section) is used as fallback only if no DB config is found

This means:

- Existing `appsettings.json` configurations continue working without changes
- Once you save a config via Admin UI, it takes precedence over appsettings.json
- To revert to appsettings.json config, disable or delete the DB provider from Admin UI

### appsettings.json Fallback Reference

If you prefer file-based config (e.g., for local development), the Demo Server reads from:

```json
{
  "Authentication": {
    "Google": {
      "Enabled": true,
      "ClientId": "your-client-id.apps.googleusercontent.com",
      "ClientSecret": "your-client-secret",
      "Authority": "https://accounts.google.com",
      "MetadataAddress": "https://accounts.google.com/.well-known/openid-configuration",
      "CallbackPath": "/auth/external/google/signin-callback",
      "AllowedRedirectHosts": ["localhost:3000"]
    },
    "ExternalAccount": {
      "AutoProvisionEnabled": false,
      "AllowedDomains": [],
      "AllowedDomainsByTenant": {}
    }
  }
}
```

---

## Part 5: Troubleshooting

| Issue | Cause | Fix |
|-------|-------|-----|
| "Test Failed" after saving | Incorrect Authority or MetadataAddress | Verify the URLs are correct and reachable from your server |
| Google login redirects to error page | Callback Path mismatch | Ensure the Callback Path in Admin UI matches the Authorized redirect URI in Google Cloud Console |
| `redirect_uri_mismatch` error from Google | URI not registered in Google Console | Add the exact redirect URI (including protocol and port) to Google Cloud Console → Credentials → Authorized redirect URIs |
| Users can't auto-provision | Domain not in Allowed Domains list | Add their email domain to the Allowed Email Domains field, or leave it empty to allow all |
| 401 on Admin UI page | Missing scopes | Ensure your admin token includes `HCL.CS.SF.externalauth.read` and `HCL.CS.SF.externalauth.manage` |
| Provider card shows "Disabled" | Enabled checkbox not checked | Edit the provider and check the "Enabled" checkbox |
| Client Secret shows as masked (`Gocl****`) | Expected behavior | Secrets are masked after save for security — re-enter the full secret only if you need to change it |

---

## Part 6: Security Notes

- **Client Secret** is stored encrypted in the `ConfigJson` column and masked in API responses (first 4 chars + `****`)
- The Admin UI never displays the full Client Secret after initial save
- Only users with the `HCL.CS.SF.externalauth.manage` scope can create, edit, or delete providers
- Read-only access requires the `HCL.CS.SF.externalauth.read` scope
- The **Test** operation only verifies metadata endpoint reachability — it does not perform a full OAuth flow

---

## Quick Checklist

- [ ] Google Cloud Project created
- [ ] OAuth consent screen configured with `openid`, `email`, `profile` scopes
- [ ] OAuth 2.0 Client ID created with correct redirect URIs
- [ ] Provider added in HCL.CS.SF Admin UI with Client ID and Client Secret
- [ ] Provider marked as **Enabled**
- [ ] "Test" button shows **Test Passed**
- [ ] Auto-provisioning configured (if needed)
- [ ] End-to-end Google login tested
