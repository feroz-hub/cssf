# HCL.CS.SF Admin - Debugging & Development Guide

## Project Overview

| Property | Value |
|----------|-------|
| **Project Type** | Next.js 14 Application |
| **Project Path** | `HCL.CS.SF-admin/` |
| **Framework** | Next.js 14.2.5 |
| **React Version** | 18.3.1 |
| **Authentication** | NextAuth.js 4.24.7 |
| **Language** | TypeScript 5.5.4 |
| **Purpose** | Administrative UI for HCL.CS.SF Identity Server |

---

## Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Entry Points](#entry-points)
3. [Application Flow](#application-flow)
4. [Project Structure](#project-structure)
5. [Configuration & Environment](#configuration--environment)
6. [Authentication Flow](#authentication-flow)
7. [Debugging in Rider (WebStorm)](#debugging-in-rider-webstorm)
8. [VS Code Debugging](#vs-code-debugging)
9. [Key Components & Modules](#key-components--modules)
10. [Common Debugging Scenarios](#common-debugging-scenarios)
11. [API Routes Reference](#api-routes-reference)

---

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         HCL.CS.SF Admin (Next.js 14)                           │
│                        Administrative Dashboard                             │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │                         Next.js App Router                          │   │
│  ├─────────────────────────────────────────────────────────────────────┤   │
│  │  App Router (Server Components)    │   Client Components            │   │
│  │  ─────────────────────────────────────────────────────────────────  │   │
│  │  • layout.tsx (Server)             │   • Providers (Session)        │   │
│  │  • page.tsx (Server)               │   • Login Page                 │   │
│  │  • loading.tsx                     │   • Module Components          │   │
│  │  • error.tsx                       │   • UI Components              │   │
│  │  • actions.ts (Server Actions)     │                                │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                    │                                        │
│                                    ▼                                        │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │                      NextAuth.js Integration                        │   │
│  │  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  ┌────────────┐ │   │
│  │  │ Credentials │──│   JWT       │──│  Session    │──│  HCL.CS.SF    │ │   │
│  │  │  Provider   │  │  Strategy   │  │   Cookie    │  │   Server   │ │   │
│  │  └─────────────┘  └─────────────┘  └─────────────┘  └────────────┘ │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                    │                                        │
│                                    ▼                                        │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │                    HCL.CS.SF Identity Server API                       │   │
│  │         (OAuth2/OIDC via Password Grant + Refresh Token)            │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## Entry Points

### 1. Application Entry Point: `app/layout.tsx`

**File:** `HCL.CS.SF-admin/app/layout.tsx`

```typescript
// Root server component - loads session and wraps children
export default async function RootLayout({ children }: { children: React.ReactNode }) {
  const session = await auth();  // Server-side session fetch

  return (
    <html lang="en">
      <body>
        <Providers session={session}>{children}</Providers>
      </body>
    </html>
  );
}
```

### 2. Entry Point Flow

```
HTTP Request
    │
    ├─> middleware.ts                    # Runs first on every request
    │   ├─> Check JWT token validity
    │   ├─> Validate admin role
    │   └─> Redirect to login if needed
    │
    ├─> app/layout.tsx                   # Root layout (Server Component)
    │   ├─> auth()                       # Get session from cookie
    │   ├─> Pass session to Providers
    │   └─> Render layout structure
    │
    └─> Target Page (Server/Client Component)
        ├─> Server: Direct data fetching
        └─> Client: React component with hooks
```

### 3. Authentication Entry Point: `app/api/auth/[...nextauth]/route.ts`

**File:** `HCL.CS.SF-admin/app/api/auth/[...nextauth]/route.ts`

```typescript
// NextAuth.js API route handler
import NextAuth from "next-auth";
import { authOptions } from "@/lib/auth";

const handler = NextAuth(authOptions);
export { handler as GET, handler as POST };
```

**NextAuth Endpoints Created:**
- `/api/auth/signin` - Login page redirect
- `/api/auth/callback/HCL.CS.SF` - OAuth callback
- `/api/auth/session` - Get current session
- `/api/auth/csrf` - CSRF token
- `/api/auth/signout` - Logout

---

## Application Flow

### Complete Request Flow

```
┌────────────────────────────────────────────────────────────────────────────┐
│                         BROWSER REQUEST FLOW                               │
└────────────────────────────────────────────────────────────────────────────┘

1. INITIAL REQUEST (e.g., /admin/clients)
   │
   ▼
┌─────────────────┐
│   middleware.ts │  • Extract JWT from cookie
│   (Edge Runtime)│  • Verify token expiry
│                 │  • Check admin role claim
└────────┬────────┘
         │ No Token/Invalid          │ Valid Token
         ▼                           ▼
   ┌──────────┐                ┌─────────────────┐
   │ Redirect │                │  Continue to    │
   │  /login  │                │  Page Component │
   └──────────┘                └────────┬────────┘
                                        │
   ┌────────────────────────────────────┘
   ▼
┌────────────────────────────────────────────────────────────────────────────┐
│ 2. PAGE RENDERING                                                           │
├────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  Server Component (app/admin/layout.tsx)                                    │
│  ───────────────────────────────────────                                    │
│  • auth() → Get session server-side                                         │
│  • Check admin role                                                         │
│  • If no role → Show 403 Forbidden                                          │
│  • If admin → Render AdminLayout                                            │
│    ├─> Sidebar                                                              │
│    ├─> Header                                                               │
│    ├─> Breadcrumb                                                           │
│    └─> Children (page.tsx)                                                  │
│                                                                             │
│  Client Component (components/modules/*)                                    │
│  ───────────────────────────────────────                                    │
│  • useSession() → React hook for session                                    │
│  • API calls via lib/api/*                                                  │
│  • UI state management                                                      │
│                                                                             │
└────────────────────────────────────────────────────────────────────────────┘

3. API CALLS TO HCL.CS.SF SERVER
   │
   ├─> lib/api/clients.ts → GET/POST/PUT/DELETE
   ├─> lib/api/users.ts   → User management
   ├─> lib/api/roles.ts   → Role management
   ├─> lib/api/audit.ts   → Audit logs
   │
   ▼
┌────────────────────────────────────────────────────────────────────────────┐
│  Authorization: Bearer <accessToken>                                        │
│  X-Correlation-ID: <uuid>                                                   │
└────────────────────────────────────────────────────────────────────────────┘
```

### Authentication Flow (NextAuth)

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                        LOGIN FLOW (Password Grant)                          │
└─────────────────────────────────────────────────────────────────────────────┘

User submits credentials
         │
         ▼
┌─────────────────┐
│  app/(auth)/    │
│  login/page.tsx │  (Client Component)
│                 │
│ • useSession()  │
│ • signIn()      │──┐
│ • handleSubmit  │  │
└─────────────────┘  │
                     │
                     ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│  signIn("credentials", { username, password, callbackUrl, redirect: false })│
│                         ─────────────────────────────────────────────────   │
│  Calls: /api/auth/callback/credentials                                      │
└─────────────────────────────────────────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│  lib/auth.ts - authorize() function                                         │
│  ─────────────────────────────────────                                      │
│  • Validate username/password                                               │
│  • Call requestPasswordGrantTokens()                                        │
│    │                                                                        │
│    ├─> resolveTokenEndpoint()                                               │
│    ├─> Attempt 1: scope + client_secret_post                                │
│    ├─> Attempt 2: scope + basic auth                                        │
│    ├─> Attempt 3: no scope + client_secret_post                             │
│    └─> Attempt 4: no scope + basic auth                                     │
│  • Extract roles from access_token                                          │
│  • Create AuthenticatedUser                                                 │
└─────────────────────────────────────────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│  JWT Callback (jwt callback in authOptions)                                 │
│  ─────────────────────────────────────────                                  │
│  • Store accessToken, refreshToken, idToken in JWT                          │
│  • Store accessTokenExpires timestamp                                       │
│  • Store roles array                                                        │
│  • Check if token needs refresh (Date.now() < expires - 60s)                │
│  • If expired → refreshAccessToken()                                        │
└─────────────────────────────────────────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│  Session Callback (session callback in authOptions)                         │
│  ────────────────────────────────────────────────                           │
│  • Expose session.accessToken to client                                     │
│  • Expose session.refreshToken, session.idToken                             │
│  • Expose session.roles, session.isAdmin                                    │
└─────────────────────────────────────────────────────────────────────────────┘
                     │
                     ▼
              ┌────────────┐
              │   Client   │
              │  Redirect  │
              │  to Admin  │
              └────────────┘
```

### Token Refresh Flow

```
Token expires soon (< 60 seconds)
         │
         ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│  lib/auth.ts - refreshAccessToken()                                         │
│  ───────────────────────────────────                                        │
│  • POST to /security/token                                                  │
│  • grant_type=refresh_token                                                 │
│  • refresh_token from current JWT                                           │
│  • Basic auth with client_id:client_secret                                  │
│                                                                             │
│  On Success:                                                                │
│  • Update accessToken, refreshToken, idToken                                │
│  • Update accessTokenExpires timestamp                                      │
│  • Update roles from new token                                              │
│                                                                             │
│  On Failure:                                                                │
│  • Set token.error = "RefreshAccessTokenError"                              │
│  • User will be redirected to login on next request                         │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## Project Structure

```
HCL.CS.SF-admin/
│
├── app/                                    # Next.js App Router
│   │
│   ├── (auth)/                             # Auth route group
│   │   └── login/
│   │       └── page.tsx                    # Login page (Client)
│   │
│   ├── admin/                              # Protected admin routes
│   │   ├── layout.tsx                      # Admin layout (Server)
│   │   ├── page.tsx                        # Dashboard redirect
│   │   │
│   │   ├── clients/                        # Client management
│   │   │   ├── page.tsx                    # Clients list
│   │   │   ├── actions.ts                  # Server actions
│   │   │   ├── loading.tsx                 # Loading state
│   │   │   ├── error.tsx                   # Error boundary
│   │   │   └── [id]/                       # Client detail
│   │   │       └── page.tsx
│   │   │       └── secrets/
│   │   │
│   │   ├── users/                          # User management
│   │   │   ├── page.tsx
│   │   │   ├── actions.ts
│   │   │   ├── loading.tsx
│   │   │   ├── error.tsx
│   │   │   └── [id]/                       # User detail
│   │   │       └── page.tsx
│   │   │       └── sessions/
│   │   │
│   │   ├── roles/                          # Role management
│   │   ├── resources/                      # API resource management
│   │   ├── audit/                          # Audit logs
│   │   └── revocation/                     # Token revocation
│   │
│   ├── api/                                # API routes
│   │   ├── auth/
│   │   │   └── [...nextauth]/
│   │   │       └── route.ts                # NextAuth handler
│   │   ├── auth/federated-logout-url/      # Logout URL generation
│   │   └── health/
│   │       └── route.ts                    # Health check
│   │
│   ├── forbidden/
│   │   └── page.tsx                        # 403 page
│   │
│   ├── signed-out/
│   │   └── page.tsx                        # Post-logout page
│   │
│   ├── layout.tsx                          # Root layout (Server)
│   ├── page.tsx                            # Root redirect
│   └── globals.css                         # Global styles
│
├── components/                             # React components
│   ├── layout/                             # Layout components
│   │   ├── Header.tsx
│   │   ├── Sidebar.tsx
│   │   └── Breadcrumb.tsx
│   ├── modules/                            # Feature modules
│   │   ├── clients/
│   │   │   ├── ClientsModule.tsx
│   │   │   └── ClientSecretsModule.tsx
│   │   ├── users/
│   │   │   ├── UsersModule.tsx
│   │   │   └── UserDetailModule.tsx
│   │   └── ... (roles, audit, etc.)
│   ├── ui/                                 # UI primitives
│   │   ├── button.tsx
│   │   ├── input.tsx
│   │   ├── dialog.tsx
│   │   └── ...
│   └── providers.tsx                       # Session provider wrapper
│
├── lib/                                    # Library code
│   ├── api/                                # API client functions
│   │   ├── client.ts                       # HTTP client
│   │   ├── clients.ts                      # Clients API
│   │   ├── users.ts                        # Users API
│   │   ├── roles.ts                        # Roles API
│   │   ├── audit.ts                        # Audit API
│   │   ├── revocation.ts                   # Revocation API
│   │   ├── resources.ts                    # Resources API
│   │   └── routes.ts                       # Route utilities
│   ├── auth.ts                             # NextAuth configuration
│   ├── env.ts                              # Environment validation
│   ├── oidc.ts                             # OIDC discovery
│   ├── types/
│   │   └── HCL.CS.SF.ts                       # TypeScript types
│   └── utils.ts                            # Utilities
│
├── types/
│   └── next-auth.d.ts                      # NextAuth type extensions
│
├── middleware.ts                           # Next.js middleware
├── next.config.mjs                         # Next.js config
├── tsconfig.json                           # TypeScript config
├── package.json                            # Dependencies
└── .env.example                            # Environment template
```

---

## Configuration & Environment

### Environment Variables

| Variable | Required | Description | Example |
|----------|----------|-------------|---------|
| `NEXTAUTH_URL` | Yes | Public URL of this app | `https://localhost:3000` |
| `NEXTAUTH_SECRET` | Yes | JWT encryption secret | `random-secret-string` |
| `HCL.CS.SF_ISSUER` | Yes | HCL.CS.SF server base URL | `https://localhost:5001` |
| `HCL.CS.SF_CLIENT_ID` | Yes | OAuth client ID | `HCL.CS.SF-admin-client` |
| `HCL.CS.SF_CLIENT_SECRET` | Yes | OAuth client secret | `client-secret` |
| `HCL.CS.SF_API_BASE_URL` | No | HCL.CS.SF API base (defaults to issuer) | `https://localhost:5001` |
| `HCL.CS.SF_SCOPES` | No | Requested OAuth scopes | See default below |
| `HCL.CS.SF_ENABLE_FEDERATED_LOGOUT` | No | Use IdP logout | `false` |
| `HCL.CS.SF_ALLOW_INSECURE_TLS` | No | Dev only: skip TLS verify | `false` |

### Default Scopes

```
openid profile email offline_access phone 
HCL.CS.SF.apiresource HCL.CS.SF.client HCL.CS.SF.user HCL.CS.SF.role 
HCL.CS.SF.identityresource HCL.CS.SF.adminuser HCL.CS.SF.securitytoken
```

### Environment File Setup

```bash
# Copy example
cp .env.example .env.local

# Edit .env.local
NEXTAUTH_URL=https://localhost:3000
NEXTAUTH_SECRET=your-random-secret-here
HCL.CS.SF_ISSUER=https://localhost:5001
HCL.CS.SF_CLIENT_ID=your-client-id
HCL.CS.SF_CLIENT_SECRET=your-client-secret
```

### Next.js Configuration

**File:** `next.config.mjs`

```javascript
/** @type {import("next").NextConfig} */
const nextConfig = {
  output: "standalone"  // For Docker deployment
};

export default nextConfig;
```

---

## Authentication Flow

### NextAuth Configuration (`lib/auth.ts`)

```typescript
export const authOptions: NextAuthOptions = {
  session: {
    strategy: "jwt"  // JWT-based sessions
  },
  
  providers: [
    CredentialsProvider({
      name: "HCL.CS.SF Credentials",
      credentials: {
        username: { label: "Username", type: "text" },
        password: { label: "Password", type: "password" }
      },
      authorize: async (credentials) => {
        // Password grant to HCL.CS.SF server
        const tokenResponse = await requestPasswordGrantTokens(userName, password);
        return createAuthenticatedUser(userName, tokenResponse);
      }
    })
  ],
  
  callbacks: {
    redirect: async ({ url, baseUrl }) => {
      // URL validation and redirect handling
    },
    
    jwt: async ({ token, account, user }) => {
      // Initial sign in - store tokens
      if (account?.provider === "credentials" && user) {
        return {
          ...token,
          accessToken: user.accessToken,
          refreshToken: user.refreshToken,
          accessTokenExpires: user.accessTokenExpires,
          roles: user.roles,
          isAdmin: user.isAdmin
        };
      }
      
      // Token still valid - return as-is
      if (Date.now() < token.accessTokenExpires - 60_000) {
        return token;
      }
      
      // Token expired - refresh
      return refreshAccessToken(token);
    },
    
    session: async ({ session, token }) => {
      // Expose to client
      session.accessToken = token.accessToken;
      session.roles = token.roles;
      session.isAdmin = token.isAdmin;
      return session;
    }
  },
  
  pages: {
    signIn: "/login"  // Custom login page
  }
};
```

### JWT Token Structure

```typescript
// Server-side JWT (encrypted in cookie)
{
  "name": "John Doe",
  "email": "john@example.com",
  "accessToken": "eyJhbGciOiJSUzI1NiIs...",
  "refreshToken": "def50200...",
  "idToken": "eyJhbGciOiJSUzI1NiIs...",
  "accessTokenExpires": 1705312800000,
  "scopes": "openid profile email...",
  "roles": ["Admin", "User"],
  "isAdmin": true,
  "sub": "user-id"
}

// Client-side Session (from useSession())
{
  "user": { "name": "John Doe", "email": "john@example.com" },
  "accessToken": "eyJhbGciOiJSUzI1NiIs...",
  "roles": ["Admin", "User"],
  "isAdmin": true,
  "expires": "2024-01-15T12:00:00.000Z"
}
```

### Middleware Authorization (`middleware.ts`)

```typescript
export async function middleware(request: NextRequest) {
  // Extract JWT from request cookie
  const token = await getToken({
    req: request,
    secret: process.env.NEXTAUTH_SECRET
  });
  
  // No token → redirect to login
  if (!token) {
    const loginUrl = new URL("/login", request.url);
    loginUrl.searchParams.set("callbackUrl", request.nextUrl.pathname);
    return NextResponse.redirect(loginUrl);
  }
  
  // Check admin role
  const roles = readRolesFromToken(token);
  if (!isAdminRole(roles)) {
    return NextResponse.rewrite(
      new URL("/forbidden", request.url),
      { status: 403 }
    );
  }
  
  return NextResponse.next();
}

export const config = {
  matcher: ["/admin/:path*"]  // Run on all /admin/* routes
};
```

---

## Debugging in Rider (WebStorm)

### 1. Prerequisites

- JetBrains Rider 2023.3+ or WebStorm
- Node.js 18+ installed
- npm or pnpm installed

### 2. Run Configuration Setup

```
Run → Edit Configurations → + → npm

Name: HCL.CS.SF Admin - Dev
Command: run
Scripts: dev:https
Environment Variables:
    NEXTAUTH_URL=https://localhost:3000
    NEXTAUTH_SECRET=debug-secret
    HCL.CS.SF_ISSUER=https://localhost:5001
    HCL.CS.SF_CLIENT_ID=your-client-id
    HCL.CS.SF_CLIENT_SECRET=your-client-secret
```

### 3. JavaScript Debug Configuration

```
Run → Edit Configurations → + → JavaScript Debug

Name: HCL.CS.SF Admin - Debug
URL: https://localhost:3000
Remote URLs:
    http://localhost:3000 → file:///path/to/HCL.CS.SF-admin
```

### 4. Rider Debug Keyboard Shortcuts

#### Windows/Linux

| Shortcut | Action | Description |
|----------|--------|-------------|
| `F5` | Start Debugging | Run with debugger |
| `Shift+F5` | Stop Debugging | Stop debug session |
| `Ctrl+F5` | Run Without Debug | Start dev server |
| `Ctrl+F2` | Stop | Stop running process |
| `F9` | Toggle Breakpoint | Set/remove breakpoint |
| `Ctrl+F8` | Toggle Breakpoint | Alternative |
| `F10` | Step Over | Execute line, don't enter functions |
| `F11` | Step Into | Enter function call |
| `Shift+F11` | Step Out | Exit current function |
| `Alt+F9` | Run to Cursor | Execute until cursor |
| `Alt+F8` | Evaluate Expression | Evaluate expression at breakpoint |
| `Ctrl+Shift+F8` | View Breakpoints | Manage all breakpoints |
| `Ctrl+Shift+F7` | Highlight Usages | Show variable usage |

#### macOS

| Shortcut | Action | Description |
|----------|--------|-------------|
| `⌃D` (Control+D) | Start Debugging | Run with debugger |
| `⌘F2` (Cmd+F2) | Stop Debugging | Stop debug session |
| `⌃⌥R` (Ctrl+Opt+R) | Run Without Debug | Start dev server |
| `⌘F8` (Cmd+F8) | Toggle Breakpoint | Set/remove breakpoint |
| `F8` | Step Over | Execute line, don't enter functions |
| `F7` | Step Into | Enter function call |
| `⇧F8` (Shift+F8) | Step Out | Exit current function |
| `⌥F9` (Opt+F9) | Run to Cursor | Execute until cursor |
| `⌥F8` (Opt+F8) | Evaluate Expression | Evaluate expression at breakpoint |
| `⇧⌘F8` (Shift+Cmd+F8) | View Breakpoints | Manage all breakpoints |
| `⇧⌘F7` (Shift+Cmd+F7) | Highlight Usages | Show variable usage |

### 5. Debugging Client Components

```typescript
// app/(auth)/login/page.tsx
"use client";

export default function LoginPage() {
  const { status } = useSession();
  
  // Set breakpoint here
  console.log("Session status:", status);  // <-- Breakpoint
  
  const startLogin = async () => {
    // Set breakpoint here to debug login flow
    const result = await signIn("credentials", {  // <-- Breakpoint
      username: userName,
      password,
      callbackUrl,
      redirect: false
    });
    
    // Inspect result
    console.log("Login result:", result);  // <-- Breakpoint
  };
  
  return (...);
}
```

### 6. Debugging Server Components

```typescript
// app/admin/layout.tsx
export default async function AdminLayout({ children }) {
  // Set breakpoint here - runs on server
  const session = await auth();  // <-- Breakpoint
  
  if (!session) {
    // Breakpoint to debug redirect
    redirect("/login");  // <-- Breakpoint
  }
  
  const roles = session.roles ?? [];
  if (!hasAdminRole(roles)) {
    // Breakpoint for forbidden case
    return <ForbiddenPage />;  // <-- Breakpoint
  }
  
  return (...);
}
```

### 7. Debugging NextAuth

```typescript
// lib/auth.ts - authorize callback
async authorize(credentials) {
  // Breakpoint to inspect credentials
  const userName = credentials?.username;  // <-- Breakpoint
  
  // Breakpoint to debug token request
  const tokenResponse = await requestPasswordGrantTokens(
    userName, 
    password
  );  // <-- Breakpoint
  
  return createAuthenticatedUser(userName, tokenResponse);
}
```

---

## VS Code Debugging

### 1. Launch Configuration

**Create `.vscode/launch.json`:**

```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": "Next.js: Debug Server",
      "type": "node",
      "request": "launch",
      "runtimeExecutable": "npm",
      "runtimeArgs": ["run", "dev:https"],
      "env": {
        "NEXTAUTH_URL": "https://localhost:3000",
        "NEXTAUTH_SECRET": "debug-secret"
      },
      "console": "integratedTerminal",
      "skipFiles": ["<node_internals>/**"]
    },
    {
      "name": "Next.js: Debug Client",
      "type": "chrome",
      "request": "launch",
      "url": "https://localhost:3000",
      "webRoot": "${workspaceFolder}/HCL.CS.SF-admin"
    },
    {
      "name": "Next.js: Full Stack",
      "type": "node",
      "request": "launch",
      "runtimeExecutable": "npm",
      "runtimeArgs": ["run", "dev:https"],
      "console": "integratedTerminal",
      "serverReadyAction": {
        "pattern": "started server on .+, url: (https?://.+)",
        "uriFormat": "%s",
        "action": "debugWithChrome"
      }
    }
  ]
}
```

### 2. VS Code Debug Shortcuts

#### Windows/Linux

| Shortcut | Action |
|----------|--------|
| `F5` | Start Debugging |
| `Shift+F5` | Stop |
| `Ctrl+F5` | Run without Debugging |
| `F9` | Toggle Breakpoint |
| `F10` | Step Over |
| `F11` | Step Into |
| `Shift+F11` | Step Out |
| `Ctrl+Shift+F5` | Restart |

#### macOS

| Shortcut | Action |
|----------|--------|
| `F5` | Start Debugging |
| `⇧F5` (Shift+F5) | Stop |
| `⌘F5` (Cmd+F5) | Run without Debugging |
| `F9` | Toggle Breakpoint |
| `F10` | Step Over |
| `F11` | Step Into |
| `⇧F11` (Shift+F11) | Step Out |
| `⇧⌘F5` (Shift+Cmd+F5) | Restart |

---

## Key Components & Modules

### Layout Components

| Component | File | Type | Purpose |
|-----------|------|------|---------|
| `AdminLayout` | `app/admin/layout.tsx` | Server | Main admin shell |
| `Header` | `components/layout/Header.tsx` | Client | Top navigation |
| `Sidebar` | `components/layout/Sidebar.tsx` | Client | Side navigation |
| `Breadcrumb` | `components/layout/Breadcrumb.tsx` | Client | Page breadcrumbs |

### Module Components

| Component | File | Purpose |
|-----------|------|---------|
| `ClientsModule` | `components/modules/clients/ClientsModule.tsx` | Client list management |
| `UsersModule` | `components/modules/users/UsersModule.tsx` | User list management |
| `UserDetailModule` | `components/modules/users/UserDetailModule.tsx` | User detail/edit |
| `RolesModule` | `components/modules/roles/RolesModule.tsx` | Role management |
| `ResourcesModule` | `components/modules/resources/ResourcesModule.tsx` | API resources |
| `AuditModule` | `components/modules/audit/AuditModule.tsx` | Audit log viewer |

### API Client Functions

**File:** `lib/api/client.ts`

```typescript
export async function apiRequest<T>(
  endpoint: string,
  options: RequestInit = {}
): Promise<T> {
  const session = await getSession();
  
  const response = await fetch(`${API_BASE}${endpoint}`, {
    ...options,
    headers: {
      "Content-Type": "application/json",
      "Authorization": `Bearer ${session?.accessToken}`,
      ...options.headers
    }
  });
  
  if (!response.ok) {
    throw new ApiError(response.status, await response.text());
  }
  
  return response.json();
}
```

### API Functions by Module

```typescript
// lib/api/clients.ts
export async function getClients(): Promise<Client[]>;
export async function createClient(data: CreateClientRequest): Promise<Client>;
export async function updateClient(id: string, data: UpdateClientRequest): Promise<Client>;
export async function deleteClient(id: string): Promise<void>;

// lib/api/users.ts
export async function getUsers(): Promise<User[]>;
export async function getUser(id: string): Promise<User>;
export async function createUser(data: CreateUserRequest): Promise<User>;
export async function updateUser(id: string, data: UpdateUserRequest): Promise<User>;
export async function deleteUser(id: string): Promise<void>;

// lib/api/roles.ts
export async function getRoles(): Promise<Role[]>;
export async function createRole(data: CreateRoleRequest): Promise<Role>;

// lib/api/audit.ts
export async function getAuditLogs(params?: AuditParams): Promise<AuditLog[]>;
```

---

## Common Debugging Scenarios

### Scenario 1: Login Not Working

```typescript
// Check in lib/auth.ts - authorize()

// Breakpoint 1: Check credentials received
const userName = typeof credentials?.username === "string" 
  ? credentials.username.trim() 
  : "";

// Breakpoint 2: Check token endpoint response
const tokenResponse = await requestPasswordGrantTokens(userName, password);
// Inspect: tokenResponse.access_token, tokenResponse.error

// Breakpoint 3: Check user creation
const user = createAuthenticatedUser(userName, tokenResponse);
// Inspect: user.roles, user.isAdmin, user.accessToken
```

**Common Issues:**
- Wrong HCL.CS.SF_ISSUER URL
- Client not registered in HCL.CS.SF
- Client doesn't support password grant
- User has 2FA enabled (not supported in admin UI)

### Scenario 2: Session Not Persisting

```typescript
// Check in lib/auth.ts - jwt callback

// Breakpoint: Initial sign in
if (account?.provider === "credentials" && user) {
  console.log("Storing tokens:", {
    accessToken: user.accessToken?.substring(0, 20),
    expires: user.accessTokenExpires
  });
  return { ... };
}

// Breakpoint: Token refresh check
if (Date.now() < token.accessTokenExpires - 60_000) {
  console.log("Token still valid");
  return token;
}
console.log("Token needs refresh");
return refreshAccessToken(token);
```

### Scenario 3: API Calls Failing

```typescript
// Check in lib/api/client.ts

export async function apiRequest<T>(endpoint: string, options: RequestInit) {
  const session = await getSession();
  
  // Breakpoint 1: Check session
  console.log("Session:", session?.accessToken ? "Present" : "Missing");
  
  // Breakpoint 2: Check request
  console.log("Request:", {
    url: `${API_BASE}${endpoint}`,
    method: options.method,
    token: session?.accessToken?.substring(0, 20)
  });
  
  const response = await fetch(...);
  
  // Breakpoint 3: Check response
  console.log("Response:", {
    status: response.status,
    ok: response.ok
  });
}
```

### Scenario 4: Middleware Redirect Loop

```typescript
// Check in middleware.ts

export async function middleware(request: NextRequest) {
  const token = await getToken({ req: request, secret: process.env.NEXTAUTH_SECRET });
  
  // Breakpoint 1: Check token extraction
  console.log("Token present:", !!token);
  console.log("Token roles:", token?.roles);
  
  if (!token) {
    // Breakpoint 2: Check redirect URL
    const loginUrl = new URL("/login", request.url);
    console.log("Redirecting to:", loginUrl.toString());
    return NextResponse.redirect(loginUrl);
  }
  
  // ... rest of middleware
}
```

### Scenario 5: Token Refresh Failing

```typescript
// Check in lib/auth.ts - refreshAccessToken()

async function refreshAccessToken(token: JWT): Promise<JWT> {
  // Breakpoint 1: Check refresh token present
  if (!token.refreshToken) {
    return { ...token, error: "MissingRefreshToken" };
  }
  
  try {
    // Breakpoint 2: Check request
    const response = await fetch(tokenEndpoint, {
      method: "POST",
      body: new URLSearchParams({
        grant_type: "refresh_token",
        refresh_token: token.refreshToken
      })
    });
    
    // Breakpoint 3: Check response
    const refreshed = await response.json();
    console.log("Refresh response:", {
      status: response.status,
      hasAccessToken: !!refreshed.access_token,
      error: refreshed.error
    });
    
    if (!response.ok) {
      return { ...token, error: refreshed.error || "RefreshAccessTokenError" };
    }
    
    // Return updated token
    return { ... };
  } catch (e) {
    // Breakpoint 4: Check error
    console.error("Refresh error:", e);
    return { ...token, error: "RefreshAccessTokenError" };
  }
}
```

---

## API Routes Reference

### NextAuth Routes

| Route | Method | Purpose |
|-------|--------|---------|
| `/api/auth/signin` | GET/POST | Sign in page |
| `/api/auth/callback/HCL.CS.SF` | GET/POST | OAuth callback |
| `/api/auth/callback/credentials` | POST | Credentials callback |
| `/api/auth/session` | GET | Get session |
| `/api/auth/csrf` | GET | Get CSRF token |
| `/api/auth/signout` | GET/POST | Sign out |

### Custom API Routes

| Route | Method | Purpose |
|-------|--------|---------|
| `/api/auth/federated-logout-url` | GET | Generate IdP logout URL |
| `/api/health` | GET | Health check |

### HCL.CS.SF API Endpoints (called from lib/api/*)

| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/Security/Api/clients` | GET/POST | Client management |
| `/Security/Api/clients/{id}` | GET/PUT/DELETE | Single client |
| `/Security/Api/users` | GET/POST | User management |
| `/Security/Api/users/{id}` | GET/PUT/DELETE | Single user |
| `/Security/Api/roles` | GET/POST | Role management |
| `/Security/Api/apiresources` | GET/POST | API resources |
| `/Security/Api/identityresources` | GET/POST | Identity resources |
| `/Security/Api/audit` | GET | Audit logs |
| `/Security/Api/revocation` | POST | Revoke tokens |
| `/Security/Api/usersessions/{id}` | GET/DELETE | User sessions |

---

## Quick Start Commands

```bash
# Navigate to project
cd HCL.CS.SF-admin

# Install dependencies
npm install
# or
pnpm install

# Set up environment
cp .env.example .env.local
# Edit .env.local with your values

# Run development server with HTTPS
npm run dev:https

# Run without HTTPS (not recommended for HCL.CS.SF)
npm run dev

# Build for production
npm run build

# Start production server
npm start

# Run linter
npm run lint

# Check formatting
npm run format:check
```

---

## Troubleshooting Guide

### Issue: Self-Signed Certificate Error

```bash
# Option 1: Use system CA
NODE_OPTIONS=--use-system-ca npm run dev:https

# Option 2: Disable TLS check (DEV ONLY)
# Add to .env.local:
HCL.CS.SF_ALLOW_INSECURE_TLS=true
```

### Issue: "Missing NEXTAUTH_SECRET" Error

```bash
# Generate a secret
openssl rand -base64 32

# Add to .env.local:
NEXTAUTH_SECRET=your-generated-secret
```

### Issue: Session Not Available in Client Components

```typescript
// Make sure to wrap with SessionProvider
// components/providers.tsx
"use client";
import { SessionProvider } from "next-auth/react";

export function Providers({ children, session }) {
  return (
    <SessionProvider session={session}>
      {children}
    </SessionProvider>
  );
}
```

### Issue: API Calls Return 401

```bash
# Check:
1. Session is valid (not expired)
2. Access token is in session
3. Token is being sent in Authorization header
4. HCL.CS.SF server accepts the token issuer
```

### Issue: "403 Forbidden" on Admin Pages

```bash
# Check:
1. User has admin role in HCL.CS.SF
2. Roles are being extracted from token correctly
3. Role claim format matches expected format
```

---

## Browser DevTools Debugging

### Network Tab

Monitor these requests:
1. `POST /api/auth/callback/credentials` - Login
2. `GET /api/auth/session` - Session check
3. `GET /Security/Api/*` - API calls to HCL.CS.SF

### Application Tab → Cookies

Check for:
- `next-auth.session-token` - JWT session cookie
- `next-auth.callback-url` - Callback URL
- `next-auth.csrf-token` - CSRF protection

### Console Tab

Enable verbose logging:
```javascript
// In browser console
localStorage.setItem('debug', 'next-auth:*');
```

---

## Session Data Flow

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         SESSION DATA FLOW                                   │
└─────────────────────────────────────────────────────────────────────────────┘

1. LOGIN
   User credentials
        │
        ▼
   ┌──────────────────────────────────────┐
   │ authorize() → createAuthenticatedUser │
   │ Returns: { accessToken, roles, ... }  │
   └──────────────────────────────────────┘
        │
        ▼
   ┌──────────────────────────────────────┐
   │ jwt callback (initial)               │
   │ Encrypts and stores in cookie        │
   └──────────────────────────────────────┘
        │
        ▼
   ┌──────────────────────────────────────┐
   │ session callback                     │
   │ Exposes to client                    │
   └──────────────────────────────────────┘

2. PAGE LOAD (Server Component)
   Request
     │
     ▼
   ┌──────────────────────────────────────┐
   │ auth() function                      │
   │ Decrypts JWT cookie                  │
   │ Returns session object               │
   └──────────────────────────────────────┘

3. PAGE LOAD (Client Component)
   React render
     │
     ▼
   ┌──────────────────────────────────────┐
   │ useSession() hook                    │
   │ Fetches from /api/auth/session       │
   │ Returns session object               │
   └──────────────────────────────────────┘

4. API CALL
   Component
     │
     ▼
   ┌──────────────────────────────────────┐
   │ getSession()                         │
   │ Gets accessToken from session        │
   │ Adds Authorization header            │
   └──────────────────────────────────────┘
     │
     ▼
   HCL.CS.SF API Server

5. TOKEN REFRESH (automatic)
   JWT callback detects expiry
     │
     ▼
   ┌──────────────────────────────────────┐
   │ refreshAccessToken()                 │
   │ POST to /security/token              │
   │ Updates tokens in JWT                │
   └──────────────────────────────────────┘
```
