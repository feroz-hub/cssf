# GitHub Configuration Guide — HCL.CS.SF Platform

This document explains every `.github` configuration file in this repository, what it does, how GitHub uses it, and how the team should interact with it.

---

## Repository Info

| Field | Value |
|---|---|
| **Repo** | `your-org/hcl-cs-sf-platform` |
| **Stack** | .NET 8, IdentityServer / OpenIddict, Entity Framework Core |
| **Components** | Identity Server, Admin Portal (`HCL.CS.SF-admin/`), Docker, Kubernetes |
| **Test Command** | `dotnet test` |

---

## .github Structure

```
.github/
├── CODEOWNERS
├── pull_request_template.md
├── dependabot.yml
├── ISSUE_TEMPLATE/
│   ├── bug_report.md
│   └── feature_request.md
└── workflows/
    ├── ci.yml
    ├── docker-build.yml
    └── security-scan.yml
```

---

## 1. CODEOWNERS

**File:** `.github/CODEOWNERS`

### What It Does

Tells GitHub **who must review and approve pull requests** before they can be merged. GitHub automatically adds the matched owners as required reviewers.

### Our Configuration

```
* @your-org
```

Every file in the repo requires approval from `@your-org`.

### How to Enable Enforcement

1. Go to **Settings → Branches → Add rule** for `main`
2. Check **"Require a pull request before merging"**
3. Check **"Require review from Code Owners"**
4. Set **"Required number of approvals"** to 1
5. Save

### When to Update

When you want per-folder ownership, e.g.:

```
/src/                      @your-org/identity
/HCL.CS.SF-admin/          @your-org/identity
/docker/                   @your-org/devops
/k8s/                      @your-org/devops
/installer/                @your-org/devops
/.github/                  @your-org/devops
```

---

## 2. Pull Request Template

**File:** `.github/pull_request_template.md`

### What It Does

When anyone opens a PR, GitHub **auto-fills the description** with this template.

### Sections

| Section | What to Fill |
|---|---|
| **Summary** | What changed and why |
| **Type of Change** | Check one: bug fix, feature, breaking change, refactor, docs, CI/CD |
| **Related Issues** | Link issues: `Fixes #42`, `Closes #99` |
| **Changes Made** | Bullet list of key changes |
| **Screenshots** | Admin portal UI changes, API response examples |
| **Testing** | Check: unit, integration, manual |
| **Test Plan** | How you tested (e.g., "ran `dotnet test`, tested OAuth flow in Postman") |
| **Checklist** | Self-review, tests pass (`dotnet test`), migrations verified |

### Tips

- Use `Fixes #123` to **auto-close** the linked issue when the PR merges
- **Always verify database migrations** if you changed entity models or DbContext
- Security-sensitive changes (auth flows, token handling, claims) need extra scrutiny
- Don't delete sections — write "N/A" if not applicable

---

## 3. Issue Templates

**Files:**
- `.github/ISSUE_TEMPLATE/bug_report.md`
- `.github/ISSUE_TEMPLATE/feature_request.md`

### What They Do

When creating a new issue, GitHub shows a **template chooser** instead of a blank form. Templates auto-apply labels (`bug` or `enhancement`).

### Bug Report Fields

| Field | What to Provide |
|---|---|
| **Description** | What went wrong |
| **Steps to Reproduce** | Numbered steps to recreate |
| **Expected vs Actual** | What should vs did happen |
| **Environment** | Service/component, API version, commit SHA, environment (dev/staging/prod) |
| **Logs / Stack Trace** | Paste error output (redact any secrets/tokens) |

### Feature Request Fields

| Field | What to Provide |
|---|---|
| **Problem Statement** | What problem this solves |
| **Proposed Solution** | How it should work |
| **Alternatives** | Other approaches considered |
| **Acceptance Criteria** | Checkboxes defining "done" |

### Security Note

For **security vulnerabilities**, do NOT create a public issue. Instead, use GitHub's **private security advisory** feature or report directly to the team.

---

## 4. Dependabot

**File:** `.github/dependabot.yml`

### What It Does

Every Monday, Dependabot scans dependencies and **auto-opens PRs** to update outdated or insecure packages.

### Configured Ecosystems

| Ecosystem | Directory | What It Updates | PR Limit |
|---|---|---|---|
| **nuget** | `/` | NuGet packages (IdentityServer, EF Core, security libs, etc.) | 10 |
| **github-actions** | `/` | Workflow action versions (e.g., `actions/checkout@v4`) | 5 |
| **docker** | `/docker` | Docker base images (e.g., `mcr.microsoft.com/dotnet/aspnet`) | 5 |

### PR Labels

Dependabot PRs are auto-labeled for easy filtering:
- `dependencies` — all dependency PRs
- `nuget` / `ci` / `docker` — ecosystem-specific

### How to Handle Dependabot PRs

1. **Patch/Minor updates** → review changelog, merge if CI passes
2. **Major updates** → read breaking changes, test auth flows manually, then merge
3. **Security alerts** → prioritize immediately (critical for an identity platform)
4. **Identity/Auth package updates** → extra caution, test OAuth/OIDC flows end-to-end
5. **To ignore a package:**
   ```yaml
   ignore:
     - dependency-name: "some-package"
       versions: [">=3.0.0"]
   ```

### Enable Dependabot Alerts

Go to **Settings → Code security and analysis** → Enable:
- Dependabot alerts
- Dependabot security updates

---

## 5. Workflows (CI/CD)

**Directory:** `.github/workflows/`

| Workflow | Trigger | What It Does |
|---|---|---|
| `ci.yml` | Push / PR | Build, test, lint the .NET solution |
| `docker-build.yml` | Push / PR | Build and validate Docker images |
| `security-scan.yml` | Schedule / PR | Scan for security vulnerabilities |

### How to Use

- All CI checks must pass before a PR can be merged
- View logs: **PR → Checks tab → click the failed check**
- Re-run a failed job: **Actions tab → select run → Re-run jobs**
- Run locally before pushing: `dotnet build && dotnet test`
- Build Docker locally: `docker build -f docker/Dockerfile .`

---

## 6. Complete Developer Workflow

```
1. CREATE ISSUE
   → GitHub → Issues → New Issue
   → Select "Bug Report" or "Feature Request"     ← ISSUE_TEMPLATE
   → Fill in details (include service/component)
   → Issue #42 created (label: bug)
   → (Security issues: use private advisory instead)

2. BRANCH & FIX
   → git checkout -b fix/token-expiry
   → Make changes, commit, push

3. OPEN PR
   → GitHub → New Pull Request
   → Description auto-filled                       ← pull_request_template.md
   → Write "Fixes #42" in summary
   → Verify migrations if entities changed
   → Fill checklist, submit

4. AUTOMATED CHECKS
   → CI runs build + tests                         ← workflows/ci.yml
   → Docker build validates                        ← workflows/docker-build.yml
   → Security scan runs                            ← workflows/security-scan.yml

5. CODE REVIEW
   → @your-org auto-requested                      ← CODEOWNERS
   → Reviewer approves (extra scrutiny for auth changes)

6. MERGE & DEPLOY
   → PR merged → Issue #42 auto-closed
   → Deploy via your deployment pipeline (Docker/K8s)

7. DEPENDENCIES (weekly)
   → Dependabot opens update PRs                   ← dependabot.yml
   → Team reviews and merges
   → Auth package updates tested end-to-end
```

---

## 7. Setup Checklist (Admin)

- [ ] Enable branch protection on `main` and `staging`
  - Require PR before merging
  - Require Code Owner review
  - Require CI status checks to pass
- [ ] Enable Dependabot alerts and security updates
- [ ] Create labels: `bug`, `enhancement`, `dependencies`, `nuget`, `ci`, `docker`
- [ ] Add `@your-org` to repo with **Write** access
- [ ] Enable **private vulnerability reporting** (Settings → Code security → Private vulnerability reporting)

---

## Quick Reference (CLI)

```bash
# Create PR (template auto-applied)
gh pr create --title "Fix token expiry" --body "Fixes #42"

# Create bug issue
gh issue create --template bug_report.md --title "[BUG] Token refresh fails"

# List Dependabot PRs
gh pr list --label dependencies

# Check CI status
gh pr checks

# List open bugs
gh issue list --label bug

# Run full checks locally before pushing
dotnet build && dotnet test
```
