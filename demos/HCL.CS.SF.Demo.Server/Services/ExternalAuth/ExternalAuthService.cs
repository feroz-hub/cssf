/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Text.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using HCL.CS.SF.DemoServerApp.Constants;
using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Constants;
using HCL.CS.SF.Domain.Constants.Endpoint;
using HCL.CS.SF.Domain.Entities.Api;
using HCL.CS.SF.Domain.Enums;
using HCL.CS.SF.DomainServices;
using HCL.CS.SF.DomainServices.Infra;
using HCL.CS.SF.DomainServices.UnitOfWork.Api;
using HCL.CS.SF.DomainServices.Wrappers;
using HCL.CS.SF.DemoServerApp.Options;
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint;

namespace HCL.CS.SF.DemoServerApp.Services.ExternalAuth;

internal sealed class ExternalAuthService(
    IEnumerable<IExternalAuthProvider> providers,
    IRepository<ExternalIdentities> externalIdentitiesRepository,
    IRepository<ExternalAuthProviderConfig> externalAuthConfigRepository,
    UserManagerWrapper<Users> userManager,
    SignInManagerWrapper<Users> signInManager,
    IUserManagementUnitOfWork userManagementUnitOfWork,
    ITenantContext tenantContext,
    IAuthorizationService authorizationService,
    IInteractionService interactionService,
    ILoggerInstance loggerInstance,
    IOptions<GoogleOidcOptions> googleOptions,
    IOptions<ExternalAccountOptions> externalAccountOptions,
    HCLCSSFConfig HCLCSSFConfig)
    : IExternalAuthService
{
    private const string ReturnUrlItem = "return_url";
    private const string TenantIdItem = "tenant_id";
    private const string LinkRequestItem = "is_link_request";
    private const string ProviderItem = "provider";
    private const string CorrelationIdItem = "correlation_id";
    private const string GoogleIdentityProviderClaimValue = "google";

    private readonly Dictionary<string, IExternalAuthProvider> providerMap = providers
        .ToDictionary(provider => provider.Provider, provider => provider, StringComparer.OrdinalIgnoreCase);

    private readonly GoogleOidcOptions google = googleOptions.Value;
    private readonly ExternalAccountOptions externalAccount = externalAccountOptions.Value;
    private readonly SystemSettings systemSettings = HCLCSSFConfig.SystemSettings;
    private readonly ILoggerService logger = loggerInstance.GetLoggerInstance(LogKeyConstants.Authentication);

    private GoogleOidcOptions? _resolvedGoogleOptions;
    private ExternalAccountOptions? _resolvedAccountOptions;

    private async Task<GoogleOidcOptions> ResolveGoogleOptionsAsync()
    {
        if (_resolvedGoogleOptions != null) return _resolvedGoogleOptions;

        try
        {
            var dbConfigs = await externalAuthConfigRepository
                .GetAsync(c => c.ProviderName == "Google" && c.IsEnabled);

            var dbConfig = dbConfigs.FirstOrDefault();
            if (dbConfig != null && !string.IsNullOrWhiteSpace(dbConfig.ConfigJson))
            {
                var settings = JsonSerializer.Deserialize<Dictionary<string, string>>(dbConfig.ConfigJson);
                if (settings != null
                    && settings.TryGetValue("ClientId", out var clientId)
                    && !string.IsNullOrWhiteSpace(clientId))
                {
                    _resolvedGoogleOptions = new GoogleOidcOptions
                    {
                        Enabled = true,
                        ClientId = clientId,
                        ClientSecret = settings.GetValueOrDefault("ClientSecret", ""),
                        Authority = settings.GetValueOrDefault("Authority", "https://accounts.google.com"),
                        MetadataAddress = settings.GetValueOrDefault("MetadataAddress",
                            "https://accounts.google.com/.well-known/openid-configuration"),
                        CallbackPath = settings.GetValueOrDefault("CallbackPath",
                            "/auth/external/google/signin-callback"),
                        AllowedRedirectHosts = settings.TryGetValue("AllowedRedirectHosts", out var hosts)
                            && !string.IsNullOrWhiteSpace(hosts)
                            ? hosts.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                            : google.AllowedRedirectHosts
                    };

                    logger.WriteTo(Log.Debug, "Google OIDC config loaded from database.");
                    return _resolvedGoogleOptions;
                }
            }
        }
        catch (Exception ex)
        {
            logger.WriteToWithCaller(Log.Warning, ex,
                "Failed to load Google OIDC config from database, falling back to appsettings.");
        }

        _resolvedGoogleOptions = google;
        return _resolvedGoogleOptions;
    }

    private async Task<ExternalAccountOptions> ResolveAccountOptionsAsync()
    {
        if (_resolvedAccountOptions != null) return _resolvedAccountOptions;

        try
        {
            var dbConfigs = await externalAuthConfigRepository
                .GetAsync(c => c.ProviderName == "Google" && c.IsEnabled);

            var dbConfig = dbConfigs.FirstOrDefault();
            if (dbConfig != null)
            {
                _resolvedAccountOptions = new ExternalAccountOptions
                {
                    AutoProvisionEnabled = dbConfig.AutoProvisionEnabled,
                    AllowedDomains = !string.IsNullOrWhiteSpace(dbConfig.AllowedDomains)
                        ? dbConfig.AllowedDomains.Split(',',
                            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        : externalAccount.AllowedDomains,
                    AllowedDomainsByTenant = externalAccount.AllowedDomainsByTenant
                };

                return _resolvedAccountOptions;
            }
        }
        catch (Exception ex)
        {
            logger.WriteToWithCaller(Log.Warning, ex,
                "Failed to load external account options from database, falling back to appsettings.");
        }

        _resolvedAccountOptions = externalAccount;
        return _resolvedAccountOptions;
    }

    public AuthenticationProperties BuildChallengeProperties(
        string provider,
        string? returnUrl,
        string? tenantId,
        bool isLinkRequest)
    {
        if (!providerMap.ContainsKey(provider))
            throw new InvalidOperationException($"External provider '{provider}' is not supported.");

        var resolvedGoogle = ResolveGoogleOptionsAsync().GetAwaiter().GetResult();
        if (!resolvedGoogle.Enabled)
            throw new InvalidOperationException("Google sign-in is disabled.");

        var normalizedReturnUrl = NormalizeReturnUrl(returnUrl);
        var normalizedTenantId = NormalizeTenantId(tenantId);

        var userId = isLinkRequest && !string.IsNullOrWhiteSpace(userManager.GetUserId(signInManager.Context.User))
            ? userManager.GetUserId(signInManager.Context.User)
            : null;

        var properties = signInManager.ConfigureExternalAuthenticationProperties(
            GoogleExternalAuthProvider.Scheme,
            "/auth/external/google/callback",
            userId);

        properties.Items[ReturnUrlItem] = normalizedReturnUrl;
        properties.Items[TenantIdItem] = normalizedTenantId;
        properties.Items[LinkRequestItem] = isLinkRequest ? "true" : "false";
        properties.Items[ProviderItem] = provider;
        properties.Items[CorrelationIdItem] = signInManager.Context.TraceIdentifier;

        return properties;
    }

    public async Task<ExternalAuthResult> CompleteGoogleCallbackAsync(HttpContext httpContext)
    {
        logger.WriteTo(Log.Debug, "External Google callback processing started.");

        var externalAuthentication = await httpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);
        if (!externalAuthentication.Succeeded || externalAuthentication.Principal == null)
        {
            logger.WriteTo(Log.Warning, "External Google callback failed: authentication failed or no principal.");
            return Failed("Google authentication failed. Please try again.", "/");
        }

        try
        {
            var items = externalAuthentication.Properties?.Items ?? new Dictionary<string, string>();
            var provider = GetItem(items, ProviderItem) ?? GoogleExternalAuthProvider.ProviderName;
            var returnUrl = NormalizeReturnUrl(GetItem(items, ReturnUrlItem));
            var tenantId = NormalizeTenantId(GetItem(items, TenantIdItem));
            var isLinkRequest =
                string.Equals(GetItem(items, LinkRequestItem), "true", StringComparison.OrdinalIgnoreCase);

            if (!providerMap.TryGetValue(provider, out var providerHandler))
            {
                logger.WriteTo(Log.Warning, "External Google callback failed: unsupported provider " + provider + ".");
                return Failed("Unsupported external provider.", returnUrl);
            }

            var payload = providerHandler.ParseIdentity(externalAuthentication.Principal);
            if (string.IsNullOrWhiteSpace(payload.Subject) || string.IsNullOrWhiteSpace(payload.Issuer))
            {
                logger.WriteTo(Log.Warning, "External Google callback failed: invalid identity payload (missing sub/iss).");
                return Failed("Google identity payload is invalid.", returnUrl);
            }

            var providerKey = BuildProviderKey(payload.Issuer, payload.Subject);
            var existingLink = await GetLinkedIdentityAsync(provider, payload.Issuer, payload.Subject);

            if (isLinkRequest)
            {
                var currentUser = await userManager.GetUserAsync(httpContext.User);
                if (currentUser == null)
                {
                    logger.WriteTo(Log.Warning, "External Google link failed: user session expired.");
                    return Failed("User session expired. Please sign in again.", returnUrl);
                }

                var linkResult = await EnsureLinkedAsync(currentUser, provider, providerKey, payload, tenantId);
                if (!linkResult.Succeeded)
                {
                    logger.WriteTo(Log.Warning, "External Google link failed for user " + currentUser.UserName + ": " + linkResult.Message);
                    return Failed(linkResult.Message, "/profile");
                }

                logger.WriteTo(Log.Debug, "Google account linked successfully for user: " + currentUser.UserName);
                return Succeeded("Google account linked successfully.", "/profile");
            }

            var user = await ResolveOrCreateUserAsync(existingLink, provider, providerKey, payload, tenantId);
            if (user == null)
            {
                logger.WriteTo(Log.Warning, "External Google sign-in failed: no account found for this Google identity (email may be unverified or not provisioned).");
                return Failed("Account not found for this Google identity.", returnUrl);
            }

            if (user.LockoutEnabled && user.LockoutEnd.HasValue && user.LockoutEnd.Value.UtcDateTime > DateTime.UtcNow)
            {
                logger.WriteTo(Log.Warning, "External Google sign-in failed: user account locked for " + user.UserName);
                return Failed("User account is locked.", returnUrl);
            }

            var signInResult = await signInManager.ExternalLoginSignInAsync(
                provider,
                providerKey,
                false,
                false);

            if (signInResult.RequiresTwoFactor)
            {
                logger.WriteTo(Log.Debug, "External Google sign-in requires two factor for user: " + user.UserName);
                httpContext.Session.SetString("UserName", user.UserName);
                return RequiresTwoFactor(returnUrl);
            }

            if (signInResult.IsLockedOut)
            {
                logger.WriteTo(Log.Warning, "External Google sign-in failed: user locked out for " + user.UserName);
                return Failed("User account is locked.", returnUrl);
            }

            if (signInResult.IsNotAllowed)
            {
                logger.WriteTo(Log.Warning, "External Google sign-in failed: login not allowed for " + user.UserName);
                return Failed("Login is not allowed for this account.", returnUrl);
            }

            if (!signInResult.Succeeded)
            {
                // Fallback for stores where external login row is not yet available in the current request scope.
                await signInManager.SignInAsync(user, false);
            }

            user.LastLoginDateTime = DateTime.UtcNow;
            await userManagementUnitOfWork.UserRepository.UpdateAsync(user, new[] { nameof(Users.LastLoginDateTime) });
            await userManagementUnitOfWork.UserRepository.SaveChangesAsync();

            if (existingLink != null)
            {
                existingLink.LastSignInAt = DateTime.UtcNow;
                await externalIdentitiesRepository.UpdateAsync(existingLink, new[] { nameof(ExternalIdentities.LastSignInAt) });
                await externalIdentitiesRepository.SaveChangesAsync();
            }

            logger.WriteTo(Log.Debug, "External Google sign-in successful for user: " + user.UserName);

            var verificationCode = await authorizationService.SaveVerificationCodeAsync(user.UserName);
            var redirectUrl = await interactionService.ConstructUserVerificationCode(returnUrl, verificationCode);
            return Succeeded(string.Empty, redirectUrl);
        }
        catch (Exception ex)
        {
            logger.WriteToWithCaller(Log.Error, ex, "External authentication callback processing failed.");
            return Failed("Google sign-in failed.", "/");
        }
        finally
        {
            await httpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        }
    }

    public async Task<ExternalAuthResult> UnlinkGoogleAsync(HttpContext httpContext)
    {
        var user = await userManager.GetUserAsync(httpContext.User);
        if (user == null)
        {
            logger.WriteTo(Log.Warning, "Unlink Google failed: user session expired.");
            return Failed("User session expired.", "/account/login");
        }

        var links = await externalIdentitiesRepository.GetAsync(entity =>
            entity.UserId == user.Id
            && entity.Provider == GoogleExternalAuthProvider.ProviderName);

        if (links.Count > 0)
        {
            await externalIdentitiesRepository.DeleteAsync(links);
            await externalIdentitiesRepository.SaveChangesWithHardDeleteAsync();
        }

        var logins = await userManager.GetLoginsAsync(user);
        foreach (var login in logins.Where(login =>
                     login.LoginProvider.Equals(GoogleExternalAuthProvider.ProviderName,
                         StringComparison.OrdinalIgnoreCase)))
            await userManager.RemoveLoginAsync(user, login.LoginProvider, login.ProviderKey);

        logger.WriteTo(Log.Debug, "Google account unlinked successfully for user: " + user.UserName);
        return Succeeded("Google account unlinked successfully.", "/profile");
    }

    private async Task<Users?> ResolveOrCreateUserAsync(
        ExternalIdentities? existingLink,
        string provider,
        string providerKey,
        ExternalIdentityPayload payload,
        string tenantId)
    {
        if (existingLink != null)
        {
            var linkedUser = await userManager.FindByIdAsync(existingLink.UserId.ToString());
            if (linkedUser != null) return linkedUser;
        }

        if (string.IsNullOrWhiteSpace(payload.Email) || !payload.EmailVerified)
            return null;

        var existingUser = await FindUserByEmailAndTenantAsync(payload.Email, tenantId);
        if (existingUser != null)
        {
            var linkResult = await EnsureLinkedAsync(existingUser, provider, providerKey, payload, tenantId);
            return linkResult.Succeeded ? existingUser : null;
        }

        if (!CanAutoProvision(payload.Email, tenantId)) return null;

        var provisionedUser = await AutoProvisionUserAsync(payload, tenantId);
        if (provisionedUser == null) return null;

        var provisionLinkResult = await EnsureLinkedAsync(provisionedUser, provider, providerKey, payload, tenantId);
        return provisionLinkResult.Succeeded ? provisionedUser : null;
    }

    private async Task<ExternalAuthResult> EnsureLinkedAsync(
        Users user,
        string provider,
        string providerKey,
        ExternalIdentityPayload payload,
        string tenantId)
    {
        var link = await GetLinkedIdentityAsync(provider, payload.Issuer, payload.Subject);
        if (link != null && link.UserId != user.Id)
            return Failed("Google identity is already linked to a different account.", "/account/login");

        if (link == null)
        {
            var newLink = new ExternalIdentities
            {
                UserId = user.Id,
                TenantId = tenantId,
                Provider = provider,
                Issuer = payload.Issuer,
                Subject = payload.Subject,
                Email = payload.Email,
                EmailVerified = payload.EmailVerified,
                LinkedAt = DateTime.UtcNow,
                LastSignInAt = DateTime.UtcNow,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "ExternalAuth",
                ModifiedBy = "ExternalAuth"
            };

            await externalIdentitiesRepository.InsertAsync(newLink);
            await externalIdentitiesRepository.SaveChangesAsync();
        }
        else
        {
            var updatedProperties = new List<string>();
            if (!string.Equals(link.Email, payload.Email, StringComparison.OrdinalIgnoreCase))
            {
                link.Email = payload.Email;
                updatedProperties.Add(nameof(ExternalIdentities.Email));
            }

            if (link.EmailVerified != payload.EmailVerified)
            {
                link.EmailVerified = payload.EmailVerified;
                updatedProperties.Add(nameof(ExternalIdentities.EmailVerified));
            }

            if (link.LastSignInAt == null || link.LastSignInAt.Value < DateTime.UtcNow.AddMinutes(-1))
            {
                link.LastSignInAt = DateTime.UtcNow;
                updatedProperties.Add(nameof(ExternalIdentities.LastSignInAt));
            }

            if (updatedProperties.Count > 0)
            {
                await externalIdentitiesRepository.UpdateAsync(link, updatedProperties.ToArray());
                await externalIdentitiesRepository.SaveChangesAsync();
            }
        }

        var loginOwner = await userManager.FindByLoginAsync(provider, providerKey);
        if (loginOwner != null && loginOwner.Id != user.Id)
            return Failed("Google identity is already linked to a different account.", "/account/login");

        if (loginOwner == null)
        {
            var addLogin = await userManager.AddLoginAsync(user, new UserLoginInfo(provider, providerKey, provider));
            if (!addLogin.Succeeded)
                return Failed("Unable to link Google login to the current account.", "/account/login");
        }

        await EnsureIdentityClaimsAsync(user, tenantId);
        return Succeeded(string.Empty, string.Empty);
    }

    private async Task EnsureIdentityClaimsAsync(Users user, string tenantId)
    {
        var currentClaims = await userManager.GetClaimsAsync(user);
        if (!currentClaims.Any(claim =>
                claim.Type == OpenIdConstants.ClaimTypes.IdentityProvider
                && claim.Value.Equals(GoogleIdentityProviderClaimValue, StringComparison.OrdinalIgnoreCase)))
            await userManager.AddClaimAsync(user,
                new Claim(OpenIdConstants.ClaimTypes.IdentityProvider, GoogleIdentityProviderClaimValue));

        if (!string.IsNullOrWhiteSpace(tenantId) &&
            !currentClaims.Any(claim =>
                IsTenantClaimType(claim.Type)
                && claim.Value.Equals(tenantId, StringComparison.OrdinalIgnoreCase)))
            await userManager.AddClaimAsync(user, new Claim("tenant_id", tenantId));
    }

    private async Task<Users?> AutoProvisionUserAsync(ExternalIdentityPayload payload, string tenantId)
    {
        var username = await BuildUniqueUserNameAsync(payload.Email);
        var firstName = ExtractFirstName(payload);
        var lastName = ExtractLastName(payload);

        var user = new Users
        {
            Id = Guid.NewGuid(),
            UserName = username,
            Email = payload.Email,
            EmailConfirmed = payload.EmailVerified,
            FirstName = firstName,
            LastName = lastName,
            IdentityProviderType = IdentityProvider.Google,
            TwoFactorType = TwoFactorType.None,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "ExternalAuth",
            ModifiedBy = "ExternalAuth",
            RequiresDefaultPasswordChange = false
        };

        var createResult = await userManager.CreateAsync(user, CreateGeneratedPassword());
        if (!createResult.Succeeded) return null;

        if (!string.IsNullOrWhiteSpace(systemSettings.UserConfig.DefaultUserRole))
            await userManager.AddToRoleAsync(user, systemSettings.UserConfig.DefaultUserRole);

        if (!string.IsNullOrWhiteSpace(tenantId))
            await userManager.AddClaimAsync(user, new Claim("tenant_id", tenantId));

        await userManager.AddClaimAsync(user, new Claim(OpenIdConstants.ClaimTypes.IdentityProvider,
            GoogleIdentityProviderClaimValue));

        return await userManager.FindByIdAsync(user.Id.ToString());
    }

    private async Task<Users?> FindUserByEmailAndTenantAsync(string email, string tenantId)
    {
        var normalizedEmail = userManager.NormalizeEmail(email);
        var users = await userManager.Users
            .Where(user => user.NormalizedEmail == normalizedEmail)
            .ToListAsync();

        if (users.Count == 0) return null;
        if (users.Count == 1 && string.IsNullOrWhiteSpace(tenantId)) return users[0];

        if (string.IsNullOrWhiteSpace(tenantId))
            return null;

        var matches = new List<Users>();
        foreach (var user in users)
        {
            var claims = await userManager.GetClaimsAsync(user);
            if (claims.Any(claim => IsTenantClaimType(claim.Type)
                                    && claim.Value.Equals(tenantId, StringComparison.OrdinalIgnoreCase)))
                matches.Add(user);
        }

        if (matches.Count == 1) return matches[0];

        // Backward-compatibility path for environments that have not yet stamped tenant claims.
        if (matches.Count == 0 && users.Count == 1) return users[0];

        return null;
    }

    private async Task<ExternalIdentities?> GetLinkedIdentityAsync(string provider, string issuer, string subject)
    {
        var links = await externalIdentitiesRepository.GetAsync(entity =>
            entity.Provider == provider
            && entity.Issuer == issuer
            && entity.Subject == subject);

        return links.FirstOrDefault();
    }

    private string NormalizeReturnUrl(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl)) return "/";

        if (Uri.TryCreate(returnUrl, UriKind.Relative, out _)
            && returnUrl.StartsWith("/", StringComparison.Ordinal))
            return returnUrl;

        if (!Uri.TryCreate(returnUrl, UriKind.Absolute, out var absoluteUri)) return "/";

        var resolvedGoogle = ResolveGoogleOptionsAsync().GetAwaiter().GetResult();
        if (resolvedGoogle.AllowedRedirectHosts.Any(host =>
                host.Equals(absoluteUri.Host, StringComparison.OrdinalIgnoreCase)))
            return returnUrl;

        return "/";
    }

    private string NormalizeTenantId(string? tenantId)
    {
        var selected = string.IsNullOrWhiteSpace(tenantId) ? tenantContext.TenantId : tenantId;
        return selected?.Trim() ?? string.Empty;
    }

    private bool CanAutoProvision(string email, string tenantId)
    {
        var resolvedAccount = ResolveAccountOptionsAsync().GetAwaiter().GetResult();
        if (!resolvedAccount.AutoProvisionEnabled) return false;

        var atIndex = email.LastIndexOf('@');
        if (atIndex <= 0 || atIndex == email.Length - 1) return false;

        var emailDomain = email[(atIndex + 1)..];
        if (string.IsNullOrWhiteSpace(emailDomain)) return false;

        if (!string.IsNullOrWhiteSpace(tenantId)
            && resolvedAccount.AllowedDomainsByTenant.TryGetValue(tenantId, out var tenantDomains)
            && tenantDomains.Length > 0)
            return tenantDomains.Any(domain => DomainMatches(emailDomain, domain));

        if (resolvedAccount.AllowedDomains.Length == 0) return true;
        return resolvedAccount.AllowedDomains.Any(domain => DomainMatches(emailDomain, domain));
    }

    private static bool DomainMatches(string emailDomain, string configuredDomain)
    {
        return emailDomain.Equals(configuredDomain, StringComparison.OrdinalIgnoreCase)
               || emailDomain.EndsWith("." + configuredDomain, StringComparison.OrdinalIgnoreCase);
    }

    private async Task<string> BuildUniqueUserNameAsync(string email)
    {
        var candidateBase = email.Split('@')[0];
        candidateBase = new string(candidateBase
            .Where(ch => char.IsLetterOrDigit(ch) || ch == '.' || ch == '_' || ch == '-')
            .ToArray());

        if (string.IsNullOrWhiteSpace(candidateBase)) candidateBase = "googleuser";

        var candidate = candidateBase;
        var suffix = 0;
        while (await userManager.FindByNameAsync(candidate) != null)
        {
            suffix++;
            candidate = $"{candidateBase}{suffix}";
        }

        return candidate;
    }

    private static string CreateGeneratedPassword()
    {
        // Generated only for compatibility with existing identity store requirements.
        return $"G!{Convert.ToBase64String(Guid.NewGuid().ToByteArray())}a1";
    }

    private static string ExtractFirstName(ExternalIdentityPayload payload)
    {
        if (string.IsNullOrWhiteSpace(payload.DisplayName)) return "Google";
        var first = payload.DisplayName.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        return string.IsNullOrWhiteSpace(first) ? "Google" : first;
    }

    private static string ExtractLastName(ExternalIdentityPayload payload)
    {
        if (string.IsNullOrWhiteSpace(payload.DisplayName)) return "User";
        var parts = payload.DisplayName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2) return "User";
        return string.Join(' ', parts.Skip(1));
    }

    private static bool IsTenantClaimType(string claimType)
    {
        return claimType.Equals("tenant_id", StringComparison.OrdinalIgnoreCase)
               || claimType.Equals("tenant", StringComparison.OrdinalIgnoreCase)
               || claimType.Equals("tid", StringComparison.OrdinalIgnoreCase);
    }

    private static string BuildProviderKey(string issuer, string subject)
    {
        return $"{issuer.Trim().TrimEnd('/')}|{subject.Trim()}";
    }

    private static string? GetItem(IDictionary<string, string?> items, string key)
    {
        return items.TryGetValue(key, out var value) ? value : null;
    }

    private static ExternalAuthResult Failed(string message, string redirectUrl)
    {
        return new ExternalAuthResult
        {
            Succeeded = false,
            Message = message,
            RedirectUrl = string.IsNullOrWhiteSpace(redirectUrl) ? "/" : redirectUrl
        };
    }

    private static ExternalAuthResult Succeeded(string message, string redirectUrl)
    {
        return new ExternalAuthResult
        {
            Succeeded = true,
            Message = message,
            RedirectUrl = string.IsNullOrWhiteSpace(redirectUrl) ? "/" : redirectUrl
        };
    }

    private static ExternalAuthResult RequiresTwoFactor(string returnUrl)
    {
        return new ExternalAuthResult
        {
            RequiresTwoFactor = true,
            RedirectUrl = string.IsNullOrWhiteSpace(returnUrl)
                ? "/account/authenticationmethod"
                : $"/account/authenticationmethod?returnUrl={Uri.EscapeDataString(returnUrl)}"
        };
    }
}
