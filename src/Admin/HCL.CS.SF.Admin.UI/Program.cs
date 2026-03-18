/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using HCL.CS.SF.Admin.UI.Interfaces;
using HCL.CS.SF.Admin.UI.Options;
using HCL.CS.SF.Admin.UI.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Bind & validate OAuth admin options
// ---------------------------------------------------------------------------
builder.Services
    .AddOptions<OAuthAdminOptions>()
    .Bind(builder.Configuration.GetSection(OAuthAdminOptions.SectionName))
    .ValidateDataAnnotations()
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.ClientId),
        "OAuth:ClientId must be provided via configuration or environment.")
    .Validate<IWebHostEnvironment>(
        (options, env) => env.IsDevelopment() || !string.IsNullOrWhiteSpace(options.ClientSecret),
        "OAuth:ClientSecret must be provided via configuration or environment (e.g. OAuth__ClientSecret). In Development you can set it in appsettings.Development.json or user-secrets.")
    .Validate(
        options => options.Scopes.Any(scope => string.Equals(scope, "openid", StringComparison.Ordinal)),
        "OAuth:Scopes must include openid.")
    .Validate(
        options => Uri.TryCreate(options.Authority, UriKind.Absolute, out var authorityUri)
                   && authorityUri.Scheme == Uri.UriSchemeHttps,
        "OAuth:Authority must be an absolute HTTPS URI.")
    .Validate(
        options => string.IsNullOrWhiteSpace(options.MetadataAddress)
                   || (Uri.TryCreate(options.MetadataAddress, UriKind.Absolute, out var metadataUri)
                       && metadataUri.Scheme == Uri.UriSchemeHttps),
        "OAuth:MetadataAddress must be an absolute HTTPS URI.")
    .Validate(
        options => Uri.TryCreate(options.TokenEndpoint, UriKind.Absolute, out var tokenEndpointUri)
                   && tokenEndpointUri.Scheme == Uri.UriSchemeHttps,
        "OAuth:TokenEndpoint must be an absolute HTTPS URI.")
    .Validate(
        options => Uri.TryCreate(options.RevocationEndpoint, UriKind.Absolute, out var revocationEndpointUri)
                   && revocationEndpointUri.Scheme == Uri.UriSchemeHttps,
        "OAuth:RevocationEndpoint must be an absolute HTTPS URI.")
    .Validate(
        options => Uri.TryCreate(options.ResourceApiBaseUrl, UriKind.Absolute, out var resourceApiUri)
                   && resourceApiUri.Scheme == Uri.UriSchemeHttps,
        "OAuth:ResourceApiBaseUrl must be an absolute HTTPS URI.")
    .ValidateOnStart();

// ---------------------------------------------------------------------------
// Read OAuth options early for HttpClient / OIDC setup
// ---------------------------------------------------------------------------
var oauth = builder.Configuration.GetSection(OAuthAdminOptions.SectionName).Get<OAuthAdminOptions>()
            ?? new OAuthAdminOptions();

// ---------------------------------------------------------------------------
// TLS bypass handler for development with self-signed certs
// ---------------------------------------------------------------------------
HttpClientHandler? insecureTlsHandler = null;
if (oauth.AllowInsecureTls && builder.Environment.IsDevelopment())
{
    insecureTlsHandler = new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    };
}

// ---------------------------------------------------------------------------
// Core services
// ---------------------------------------------------------------------------
builder.Services.AddHttpContextAccessor();

builder.Services.AddSession(options =>
{
    options.Cookie.Name = "__Host.HCL.CS.SF.Admin.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

builder.Services.AddHttpClient();
builder.Services.AddHttpClient("ResourceApiClient", (sp, client) =>
{
    var opts = sp.GetRequiredService<IOptions<OAuthAdminOptions>>().Value;
    client.BaseAddress = new Uri(opts.ResourceApiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(opts.ApiTimeoutSeconds);
}).ConfigurePrimaryHttpMessageHandler(() =>
{
    if (insecureTlsHandler != null)
    {
        return new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
    }

    return new HttpClientHandler();
});

// ---------------------------------------------------------------------------
// Authentication — Cookie + OpenID Connect
// ---------------------------------------------------------------------------
builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.Cookie.Name = "__Host.HCL.CS.SF.Admin.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.IsEssential = true;
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";

        options.Events = new CookieAuthenticationEvents
        {
            OnValidatePrincipal = async context =>
            {
                var oauthOpts = context.HttpContext.RequestServices.GetRequiredService<IOptions<OAuthAdminOptions>>()
                    .Value;
                var tokenService = context.HttpContext.RequestServices.GetRequiredService<ITokenService>();

                var expiresAt = context.Properties.GetTokenValue("expires_at");
                if (!DateTimeOffset.TryParse(expiresAt, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind,
                        out var expires)) return;

                var remaining = expires - DateTimeOffset.UtcNow;
                var refreshThreshold = TimeSpan.FromSeconds(oauthOpts.RefreshBeforeExpirySeconds);
                if (remaining > refreshThreshold) return;

                var refreshed = await tokenService.RefreshTokenAsync(
                    context.Properties,
                    context.Principal,
                    persistCookie: false,
                    context.HttpContext.RequestAborted);
                if (refreshed)
                {
                    context.ShouldRenew = true;
                    return;
                }

                context.RejectPrincipal();
                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
        };
    })
    .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
    {
        static string ComputeAtHash(string accessToken, string signingAlgorithm)
        {
            using HashAlgorithm hashAlgorithm = signingAlgorithm switch
            {
                "RS256" or "ES256" => SHA256.Create(),
                _ => throw new InvalidOperationException(
                    "Unsupported ID token signing algorithm for at_hash validation.")
            };

            var tokenBytes = Encoding.ASCII.GetBytes(accessToken);
            var hash = hashAlgorithm.ComputeHash(tokenBytes);
            var halfHash = new byte[hash.Length / 2];
            Array.Copy(hash, halfHash, halfHash.Length);
            return Base64UrlEncoder.Encode(halfHash);
        }

        static void AddClaimIfMissing(ClaimsIdentity identity, string claimType, string? claimValue)
        {
            if (string.IsNullOrWhiteSpace(claimValue)) return;

            var trimmedValue = claimValue.Trim();
            if (identity.HasClaim(claimType, trimmedValue)) return;

            identity.AddClaim(new Claim(claimType, trimmedValue));
        }

        static void AddNormalizedRoleClaim(ClaimsIdentity identity, string? role)
        {
            if (string.IsNullOrWhiteSpace(role)) return;

            AddClaimIfMissing(identity, ClaimTypes.Role, role);
            AddClaimIfMissing(identity, "role", role);
        }

        static void AddPermissionClaim(ClaimsIdentity identity, string? permission)
        {
            AddClaimIfMissing(identity, "permission", permission);
        }

        static void AddJsonClaims(JsonElement element, string propertyName, Action<string> addClaim)
        {
            if (!element.TryGetProperty(propertyName, out var propertyValue)) return;

            switch (propertyValue.ValueKind)
            {
                case JsonValueKind.Array:
                    foreach (var item in propertyValue.EnumerateArray())
                        if (item.ValueKind == JsonValueKind.String)
                            addClaim(item.GetString() ?? string.Empty);
                    break;
                case JsonValueKind.String:
                    addClaim(propertyValue.GetString() ?? string.Empty);
                    break;
            }
        }

        static void NormalizeClaimsFromAccessToken(ClaimsPrincipal? principal, string? accessToken)
        {
            if (principal?.Identity is not ClaimsIdentity identity || string.IsNullOrWhiteSpace(accessToken)) return;

            JwtSecurityToken jwt;
            try
            {
                jwt = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
            }
            catch
            {
                return;
            }

            foreach (var claim in jwt.Claims)
            {
                if (string.Equals(claim.Type, "role", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(claim.Type, ClaimTypes.Role, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(claim.Type, "roles", StringComparison.OrdinalIgnoreCase))
                {
                    AddNormalizedRoleClaim(identity, claim.Value);
                    continue;
                }

                if (string.Equals(claim.Type, "permission", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(claim.Type, "permissions", StringComparison.OrdinalIgnoreCase))
                    AddPermissionClaim(identity, claim.Value);
            }
        }

        static void NormalizeClaimsFromUserInfo(ClaimsPrincipal? principal, JsonElement userInfo)
        {
            if (principal?.Identity is not ClaimsIdentity identity) return;

            AddJsonClaims(userInfo, ClaimTypes.Role, role => AddNormalizedRoleClaim(identity, role));
            AddJsonClaims(userInfo, "role", role => AddNormalizedRoleClaim(identity, role));
            AddJsonClaims(userInfo, "roles", role => AddNormalizedRoleClaim(identity, role));
            AddJsonClaims(userInfo, "permission", permission => AddPermissionClaim(identity, permission));
            AddJsonClaims(userInfo, "permissions", permission => AddPermissionClaim(identity, permission));
        }

        options.Authority = oauth.Authority;
        options.MetadataAddress = string.IsNullOrWhiteSpace(oauth.MetadataAddress) ? null : oauth.MetadataAddress;
        options.ClientId = oauth.ClientId;
        options.ClientSecret = oauth.ClientSecret;
        options.RequireHttpsMetadata = !oauth.AllowInsecureTls;
        options.ResponseType = OpenIdConnectResponseType.Code;
        options.ResponseMode = OpenIdConnectResponseMode.FormPost;
        options.UsePkce = true;
        options.GetClaimsFromUserInfoEndpoint = true;
        options.SaveTokens = true;
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.CallbackPath = oauth.CallbackPath;
        options.SignedOutCallbackPath = oauth.SignedOutCallbackPath;
        options.CorrelationCookie.Name = "__Host.HCL.CS.SF.Admin.Correlation";
        options.CorrelationCookie.HttpOnly = true;
        options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
        options.CorrelationCookie.SameSite = SameSiteMode.None;
        options.NonceCookie.Name = "__Host.HCL.CS.SF.Admin.Nonce";
        options.NonceCookie.HttpOnly = true;
        options.NonceCookie.SecurePolicy = CookieSecurePolicy.Always;
        options.NonceCookie.SameSite = SameSiteMode.None;

        // Backchannel TLS bypass for dev self-signed certs
        if (insecureTlsHandler != null)
        {
            options.BackchannelHttpHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };
        }

        // Token validation
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = oauth.Authority,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromMinutes(5),
            NameClaimType = "name",
            RoleClaimType = "role",
            // Resolve signing keys dynamically from the JWKS endpoint
            IssuerSigningKeyResolver = (token, securityToken, kid, parameters) =>
            {
                var jwksUrl = oauth.Authority.TrimEnd('/') + "/.well-known/openid-configuration/jwks";
                using var handler = new HttpClientHandler();
                if (oauth.AllowInsecureTls)
                    handler.ServerCertificateCustomValidationCallback =
                        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                using var httpClient = new HttpClient(handler);
                var jwksJson = httpClient.GetStringAsync(jwksUrl).GetAwaiter().GetResult();
                var jwks = new JsonWebKeySet(jwksJson);
                return jwks.GetSigningKeys();
            }
        };

        options.Scope.Clear();
        foreach (var scope in oauth.Scopes) options.Scope.Add(scope);

        options.Events = new OpenIdConnectEvents
        {
            OnUserInformationReceived = context =>
            {
                NormalizeClaimsFromUserInfo(context.Principal, context.User.RootElement);

                var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                    .CreateLogger("OIDC.UserInfo");
                if (context.Principal != null)
                {
                    var roles = context.Principal.FindAll(ClaimTypes.Role)
                        .Concat(context.Principal.FindAll("role"))
                        .Select(claim => claim.Value)
                        .Distinct(StringComparer.OrdinalIgnoreCase);
                    logger.LogInformation("Normalized userinfo roles: {Roles}", string.Join(", ", roles));
                }

                return Task.CompletedTask;
            },
            OnAuthorizationCodeReceived = async context =>
            {
                var tokenEndpoint = oauth.TokenEndpoint;
                if (string.IsNullOrWhiteSpace(tokenEndpoint))
                    tokenEndpoint = context.Options.Authority?.TrimEnd('/') + "/security/token";

                var tokenRequestParameters = new List<KeyValuePair<string, string>>();

                static void AddParameter(ICollection<KeyValuePair<string, string>> parameters, string key,
                    string? value)
                {
                    if (!string.IsNullOrWhiteSpace(value))
                        parameters.Add(new KeyValuePair<string, string>(key, value));
                }

                if (context.TokenEndpointRequest != null)
                {
                    var redirectUri = context.TokenEndpointRequest.RedirectUri;
                    if (string.IsNullOrWhiteSpace(redirectUri))
                        redirectUri =
                            $"{context.Request.Scheme}://{context.Request.Host}{context.Options.CallbackPath}";

                    AddParameter(tokenRequestParameters, "grant_type",
                        context.TokenEndpointRequest.GrantType ?? "authorization_code");
                    AddParameter(tokenRequestParameters, "code",
                        context.TokenEndpointRequest.Code ?? context.ProtocolMessage.Code);
                    AddParameter(tokenRequestParameters, "redirect_uri", redirectUri);
                }

                if (context.TokenEndpointRequest != null)
                {
                    AddParameter(tokenRequestParameters, "code_verifier",
                        context.TokenEndpointRequest.GetParameter("code_verifier"));
                    AddParameter(tokenRequestParameters, "client_id", context.Options.ClientId);

                    foreach (var parameter in context.TokenEndpointRequest.Parameters)
                    {
                        if (string.Equals(parameter.Key, "client_secret", StringComparison.Ordinal)) continue;
                        if (tokenRequestParameters.Any(existing =>
                                string.Equals(existing.Key, parameter.Key, StringComparison.Ordinal)))
                            continue;
                        AddParameter(tokenRequestParameters, parameter.Key, parameter.Value);
                    }
                }

                using var tokenRequest = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint);
                tokenRequest.Content = new FormUrlEncodedContent(tokenRequestParameters);

                var basicCredential =
                    Convert.ToBase64String(
                        Encoding.UTF8.GetBytes($"{context.Options.ClientId}:{context.Options.ClientSecret}"));
                tokenRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", basicCredential);

                using var tokenResponse =
                    await context.Backchannel.SendAsync(tokenRequest, context.HttpContext.RequestAborted);
                var tokenResponseBody =
                    await tokenResponse.Content.ReadAsStringAsync(context.HttpContext.RequestAborted);
                if (!tokenResponse.IsSuccessStatusCode)
                {
                    context.HandleCodeRedemption();
                    context.Fail($"Token endpoint error: {tokenResponseBody}");
                    return;
                }

                using var tokenJson = JsonDocument.Parse(tokenResponseBody);
                var root = tokenJson.RootElement;
                var oidcTokenResponse = new OpenIdConnectMessage
                {
                    AccessToken = root.TryGetProperty("access_token", out var at) ? at.GetString() : null,
                    IdToken = root.TryGetProperty("id_token", out var idt) ? idt.GetString() : null,
                    RefreshToken = root.TryGetProperty("refresh_token", out var rt) ? rt.GetString() : null,
                    TokenType = root.TryGetProperty("token_type", out var tt) ? tt.GetString() : null,
                    ExpiresIn = root.TryGetProperty("expires_in", out var ei) ? ei.GetRawText() : null,
                    Scope = root.TryGetProperty("scope", out var sc) ? sc.GetString() : null
                };

                context.HandleCodeRedemption(oidcTokenResponse);
            },
            OnRemoteFailure = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                    .CreateLogger("OIDC");
                context.HandleResponse();

                var message = context.Failure?.Message ?? "Remote authentication failure.";
                logger.LogError(context.Failure, "OIDC remote failure: {Message}", message);

                if (message.Contains("Correlation failed", StringComparison.OrdinalIgnoreCase))
                    message = "OIDC correlation failed. Please clear your browser cookies and sign in again.";

                context.Response.Redirect(
                    $"/Account/AuthError?errorCode=OidcRemoteFailure&errorMessage={Uri.EscapeDataString(message)}");
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                    .CreateLogger("OIDC");
                var message = context.Exception?.Message ?? "OpenID Connect authentication failed.";
                logger.LogError(context.Exception, "OIDC authentication failed: {Message}", message);

                context.HandleResponse();
                context.Response.Redirect(
                    $"/Account/AuthError?errorCode=OidcAuthenticationFailed&errorMessage={Uri.EscapeDataString(message)}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                // Debug: log all claims from the principal
                var debugLogger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                    .CreateLogger("OIDC.Claims");
                if (context.Principal != null)
                {
                    debugLogger.LogInformation("=== Token validated. User claims ===");
                    foreach (var claim in context.Principal.Claims)
                        debugLogger.LogInformation("  Claim: {Type} = {Value}", claim.Type, claim.Value);
                }

                if (context is not { SecurityToken: { } validatedJwt }) return Task.CompletedTask;

                var accessToken = context.TokenEndpointResponse?.AccessToken ?? context.ProtocolMessage?.AccessToken;
                NormalizeClaimsFromAccessToken(context.Principal, accessToken);
                if (string.IsNullOrWhiteSpace(accessToken)) return Task.CompletedTask;

                // at_hash validation — skip if the identity server doesn't emit it
                var atHashClaim = validatedJwt.Claims.FirstOrDefault(claim => claim.Type == "at_hash")?.Value;
                if (string.IsNullOrWhiteSpace(atHashClaim))
                    return Task.CompletedTask; // Not all identity servers include at_hash

                try
                {
                    var computedAtHash = ComputeAtHash(accessToken, validatedJwt.Header.Alg);
                    if (!string.Equals(atHashClaim, computedAtHash, StringComparison.Ordinal))
                        context.Fail("Invalid at_hash value.");
                }
                catch (InvalidOperationException)
                {
                    // Unknown signing algorithm — skip at_hash validation
                    var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                        .CreateLogger("OIDC");
                    logger.LogWarning("Skipping at_hash validation: unsupported signing algorithm {Alg}.",
                        validatedJwt.Header.Alg);
                }

                return Task.CompletedTask;
            }
        };
    });

// ---------------------------------------------------------------------------
// Authorization — AdminOnly policy
// ---------------------------------------------------------------------------
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireAssertion(context =>
        {
            var roleClaims = context.User.FindAll(ClaimTypes.Role)
                .Concat(context.User.FindAll("role"))
                .Select(c => c.Value)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            return oauth.AdminRoles.Any(role => roleClaims.Contains(role));
        });
    });
});

// ---------------------------------------------------------------------------
// Application services
// ---------------------------------------------------------------------------
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IApiClientService, ApiClientService>();
builder.Services.AddScoped<IAdminApiService, AdminApiService>();
builder.Services.AddAntiforgery(options => { options.HeaderName = "X-CSRF-TOKEN"; });
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// ---------------------------------------------------------------------------
// Build & configure pipeline
// ---------------------------------------------------------------------------
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    var oauthOptions = app.Services.GetRequiredService<IOptions<OAuthAdminOptions>>().Value;
    if (string.IsNullOrWhiteSpace(oauthOptions.ClientSecret))
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(
            "OAuth:ClientSecret is not set. Sign-in will fail until you set it (e.g. in appsettings.Development.json or via OAuth__ClientSecret). " +
            "Use the client secret shown when you completed the HCL.CS.SF installer.");
    }
}

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();
else
    app.UseHsts();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    "account",
    "Account/{action=Login}/{id?}",
    new { controller = "Account" });
app.MapControllers();

// Redirect root to admin dashboard
app.MapGet("/", context =>
{
    context.Response.Redirect("/Admin/Dashboard");
    return Task.CompletedTask;
});

app.Run();
