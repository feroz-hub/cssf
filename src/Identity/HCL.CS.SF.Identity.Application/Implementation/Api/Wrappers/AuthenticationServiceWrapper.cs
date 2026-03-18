/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using HCL.CS.SF.Domain.Constants.Endpoint;
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint;

namespace HCL.CS.SF.Service.Implementation.Api.Wrappers;

/// <summary>
/// Custom authentication service wrapper that intercepts ASP.NET Core sign-in and sign-out events
/// to integrate with the identity server's session management layer. On sign-in, it enriches
/// the claims principal with OIDC authentication metadata (idp, amr, auth_time) and binds
/// a server-side session cookie. On sign-out, it triggers back-channel logout notifications
/// to relying parties before completing the sign-out flow.
/// </summary>

public class AuthenticationServiceWrapper(
    IAuthenticationSchemeProvider schemes,
    ISessionManagementService session,
    IBackChannelLogoutService backChannelLogoutService,
    IAuthenticationHandlerProvider handlers,
    IClaimsTransformation transform,
    IOptions<AuthenticationOptions> options)
    : AuthenticationService(schemes, handlers, transform, options)
{
    private readonly IAuthenticationSchemeProvider schemes = schemes;

    /// <summary>
    /// Intercepts the sign-in flow to enrich the principal with OIDC claims and create
    /// a server-side session cookie before delegating to the base authentication service.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <param name="scheme">The authentication scheme name, or null for the default scheme.</param>
    /// <param name="principal">The claims principal representing the authenticated user.</param>
    /// <param name="properties">Optional authentication properties (e.g., persistence, redirect).</param>

    public override async Task SignInAsync(HttpContext context, string scheme, ClaimsPrincipal principal,
        AuthenticationProperties properties)
    {
        var defaultScheme = await schemes.GetDefaultSignInSchemeAsync();
        var cookieScheme = await session.GetAuthenticationSchemeAsync();

        if ((scheme == null && defaultScheme?.Name == cookieScheme) || scheme == cookieScheme)
        {
            AddClaimsToPrincipal(principal);

            properties ??= new AuthenticationProperties();
            await session.CreateAndBindSessionCookieAsync(principal, properties);
        }

        await base.SignInAsync(context, scheme, principal, properties);
    }

    /// <summary>
    /// Intercepts the sign-out flow to perform back-channel logout notifications to
    /// downstream relying parties before delegating to the base authentication service.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <param name="scheme">The authentication scheme name, or null for the default scheme.</param>
    /// <param name="properties">Optional authentication properties for the sign-out.</param>

    public override async Task SignOutAsync(HttpContext context, string scheme, AuthenticationProperties properties)
    {
        var defaultScheme = await schemes.GetDefaultSignOutSchemeAsync();
        var cookieScheme = await session.GetAuthenticationSchemeAsync();

        if ((scheme == null && defaultScheme?.Name == cookieScheme) || scheme == cookieScheme)
        {
            await backChannelLogoutService.ProcessLogoutAsync();
            SetSignOutCalled(context);
        }

        await base.SignOutAsync(context, scheme, properties);
    }

    /// <summary>
    /// Marks the HTTP context to indicate that sign-out has been initiated,
    /// allowing downstream middleware to detect the sign-out state.
    /// </summary>

    private void SetSignOutCalled(HttpContext context)
    {
        context.Items[AuthenticationConstants.EnvironmentPaths.SignOutCalled] = "true";
    }

    /// <summary>
    /// Validates and enriches the claims principal during sign-in. Ensures the principal
    /// has exactly one identity with a subject claim, then adds OIDC authentication metadata.
    /// </summary>

    private void AddClaimsToPrincipal(ClaimsPrincipal principal)
    {
        if (principal.Identities.Count() != 1)
            throw new InvalidOperationException("User should allow to login with only one credentails");

        if (principal.FindFirst(OpenIdConstants.ClaimTypes.Sub) == null)
            throw new InvalidOperationException("No subject claim found");

        AddAdditionalAuthenticationClaims(principal, DateTime.UtcNow);
    }

    /// <summary>
    /// Adds OIDC-required authentication claims (idp, amr, auth_time) to the principal's
    /// identity. These claims are needed by token generation to populate ID tokens correctly.
    /// For local users, sets idp to "local" and amr to "pwd"; for external providers,
    /// the identity provider claim is derived from the authentication method.
    /// </summary>

    private void AddAdditionalAuthenticationClaims(ClaimsPrincipal principal, DateTime authTime)
    {
        var identity = principal.Identities.First();

        var amr = identity.FindFirst(OpenIdConstants.ClaimTypes.AuthenticationMethod);
        if (amr != null &&
            identity.FindFirst(OpenIdConstants.ClaimTypes.IdentityProvider) == null &&
            identity.FindFirst(OpenIdConstants.ClaimTypes.AuthenticationMethod) == null)
        {
            identity.RemoveClaim(amr);
            identity.AddClaim(new Claim(OpenIdConstants.ClaimTypes.IdentityProvider, amr.Value));
        }

        if (identity.FindFirst(OpenIdConstants.ClaimTypes.IdentityProvider) == null)
            // TODO: Add IdentityProvider value as external if user is from ldap
            identity.AddClaim(new Claim(OpenIdConstants.ClaimTypes.IdentityProvider,
                AuthenticationConstants.LocalIdentityProvider));

        if (identity.FindFirst(OpenIdConstants.ClaimTypes.AuthenticationMethod) == null &&
            identity.FindFirst(OpenIdConstants.ClaimTypes.IdentityProvider).Value ==
            AuthenticationConstants.LocalIdentityProvider)
            identity.AddClaim(new Claim(OpenIdConstants.ClaimTypes.AuthenticationMethod,
                OpenIdConstants.UserAuthenticationMethods.Password));

        if (identity.FindFirst(OpenIdConstants.ClaimTypes.AuthenticationTime) == null)
        {
            var time = authTime.ToUnixTime().ToString();
            identity.AddClaim(new Claim(OpenIdConstants.ClaimTypes.AuthenticationTime, time,
                ClaimValueTypes.Integer64));
        }
    }
}
