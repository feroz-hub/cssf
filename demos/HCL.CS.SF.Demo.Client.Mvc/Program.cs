/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Globalization;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Lib.AspNetCore.Mvc.JqGrid.Core.Request;
using Lib.AspNetCore.Mvc.JqGrid.Infrastructure.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using HCL.CS.SF.DemoClientMvc.Interface;
using HCL.CS.SF.DemoClientMvc.Middlewares;
using HCL.CS.SF.DemoClientMvc.Options;
using HCL.CS.SF.DemoClientMvc.Service;
using HCL.CS.SF.DemoClientMvc.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOptions<OAuthClientOptions>()
    .Bind(builder.Configuration.GetSection(OAuthClientOptions.SectionName))
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

builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = "__Host.HCL.CS.SF.DemoClient.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

builder.Services.AddHttpClient();
builder.Services.AddHttpClient(ApiClientService.ResourceApiClientName, (sp, client) =>
{
    var oauth = sp.GetRequiredService<IOptions<OAuthClientOptions>>().Value;
    client.BaseAddress = new Uri(oauth.ResourceApiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(oauth.ApiTimeoutSeconds);
});

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.Cookie.Name = "__Host.HCL.CS.SF.DemoClient.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.IsEssential = true;
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);

        options.Events = new CookieAuthenticationEvents
        {
            OnValidatePrincipal = async context =>
            {
                var oauth = context.HttpContext.RequestServices.GetRequiredService<IOptions<OAuthClientOptions>>()
                    .Value;
                var tokenService = context.HttpContext.RequestServices.GetRequiredService<ITokenService>();

                var expiresAt = context.Properties.GetTokenValue("expires_at");
                if (!DateTimeOffset.TryParse(expiresAt, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind,
                        out var expires)) return;

                var remaining = expires - DateTimeOffset.UtcNow;
                var refreshThreshold = TimeSpan.FromSeconds(oauth.RefreshBeforeExpirySeconds);
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
        var oauth = builder.Configuration.GetSection(OAuthClientOptions.SectionName).Get<OAuthClientOptions>() ??
                    new OAuthClientOptions();

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

        options.Authority = oauth.Authority;
        options.MetadataAddress = string.IsNullOrWhiteSpace(oauth.MetadataAddress) ? null : oauth.MetadataAddress;
        options.ClientId = oauth.ClientId;
        options.ClientSecret = oauth.ClientSecret;
        options.RequireHttpsMetadata = true;
        options.ResponseType = OpenIdConnectResponseType.Code;
        options.ResponseMode = OpenIdConnectResponseMode.FormPost;
        options.UsePkce = true;
        options.GetClaimsFromUserInfoEndpoint = true;
        options.SaveTokens = true;
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.CallbackPath = oauth.CallbackPath;
        options.SignedOutCallbackPath = oauth.SignedOutCallbackPath;
        options.CorrelationCookie.Name = "__Host.HCL.CS.SF.DemoClient.Correlation";
        options.CorrelationCookie.HttpOnly = true;
        options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
        options.CorrelationCookie.SameSite = SameSiteMode.None;
        options.NonceCookie.Name = "__Host.HCL.CS.SF.DemoClient.Nonce";
        options.NonceCookie.HttpOnly = true;
        options.NonceCookie.SecurePolicy = CookieSecurePolicy.Always;
        options.NonceCookie.SameSite = SameSiteMode.None;

        options.Scope.Clear();
        foreach (var scope in oauth.Scopes) options.Scope.Add(scope);

        options.Events = new OpenIdConnectEvents
        {
            OnAuthorizationCodeReceived = async context =>
            {
                var tokenEndpoint = oauth.TokenEndpoint;
                if (string.IsNullOrWhiteSpace(tokenEndpoint))
                    tokenEndpoint = context.Options.Authority?.TrimEnd('/') + "/security/token";

                var tokenRequestParameters = new List<KeyValuePair<string, string>>();

                static void AddParameter(ICollection<KeyValuePair<string, string>> parameters, string key,
                    string? value)
                {
                    if (!string.IsNullOrWhiteSpace(value)) parameters.Add(new KeyValuePair<string, string>(key, value));
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
                    context.Fail("Token endpoint error.");
                    return;
                }

                using var tokenJson = JsonDocument.Parse(tokenResponseBody);
                var root = tokenJson.RootElement;
                var oidcTokenResponse = new OpenIdConnectMessage
                {
                    AccessToken = root.TryGetProperty("access_token", out var accessToken)
                        ? accessToken.GetString()
                        : null,
                    IdToken = root.TryGetProperty("id_token", out var idToken) ? idToken.GetString() : null,
                    RefreshToken = root.TryGetProperty("refresh_token", out var refreshToken)
                        ? refreshToken.GetString()
                        : null,
                    TokenType = root.TryGetProperty("token_type", out var tokenType) ? tokenType.GetString() : null,
                    ExpiresIn = root.TryGetProperty("expires_in", out var expiresIn) ? expiresIn.GetRawText() : null,
                    Scope = root.TryGetProperty("scope", out var scope) ? scope.GetString() : null
                };

                context.HandleCodeRedemption(oidcTokenResponse);
            },
            OnRemoteFailure = context =>
            {
                context.HandleResponse();

                var message = context.Failure?.Message ?? "Remote authentication failure.";
                if (message.Contains("Correlation failed", StringComparison.OrdinalIgnoreCase))
                    message = "OIDC correlation failed. Please clear your browser cookies and sign in again.";

                context.Response.Redirect(
                    $"/Error/Error?errorCode=OidcRemoteFailure&errorMessage={Uri.EscapeDataString(message)}");
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                context.HandleResponse();
                var message = "OpenID Connect authentication failed.";
                context.Response.Redirect(
                    $"/Error/Error?errorCode=OidcAuthenticationFailed&errorMessage={Uri.EscapeDataString(message)}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                if (context is not { SecurityToken: { } validatedJwt }) return Task.CompletedTask;

                var accessToken = context.TokenEndpointResponse?.AccessToken ?? context.ProtocolMessage?.AccessToken;
                if (string.IsNullOrWhiteSpace(accessToken)) return Task.CompletedTask;

                var atHashClaim = validatedJwt.Claims.FirstOrDefault(claim => claim.Type == "at_hash")?.Value;
                if (string.IsNullOrWhiteSpace(atHashClaim))
                {
                    context.Fail("ID token missing at_hash claim.");
                    return Task.CompletedTask;
                }

                try
                {
                    var computedAtHash = ComputeAtHash(accessToken, validatedJwt.Header.Alg);
                    if (!string.Equals(atHashClaim, computedAtHash, StringComparison.Ordinal))
                        context.Fail("Invalid at_hash value.");
                }
                catch (InvalidOperationException)
                {
                    context.Fail("Unsupported ID token signing algorithm.");
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddJqGrid();
builder.Services.AddTransient<IHttpService, HttpService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IApiClientService, ApiClientService>();
builder.Services.AddScoped<IEndpointFlowService, EndpointFlowService>();
builder.Services.AddAntiforgery(options => { options.HeaderName = "X-CSRF-TOKEN"; });
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    var oauthOptions = app.Services.GetRequiredService<IOptions<OAuthClientOptions>>().Value;
    if (string.IsNullOrWhiteSpace(oauthOptions.ClientSecret))
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(
            "OAuth:ClientSecret is not set. Sign-in will fail until you set it (e.g. in appsettings.Development.json or via OAuth__ClientSecret). " +
            "Use the client secret shown when you completed the HCL.CS.SF installer.");
    }
}

JqGridRequest.ParametersNames = new JqGridParametersNames { PagesCount = "npage" };

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();
else
    app.UseHsts();

app.UseHttpsRedirection();
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    "default",
    "{controller=Home}/{action=Index}/{id?}");

app.Run();
