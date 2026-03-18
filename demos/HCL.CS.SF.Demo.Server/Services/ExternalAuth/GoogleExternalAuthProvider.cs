/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Security.Claims;

namespace HCL.CS.SF.DemoServerApp.Services.ExternalAuth;

public sealed class GoogleExternalAuthProvider : IExternalAuthProvider
{
    public const string Scheme = "GoogleOidc";

    public const string ProviderName = "Google";

    public string Provider => ProviderName;

    public bool CanHandle(string provider)
    {
        return string.Equals(provider, ProviderName, StringComparison.OrdinalIgnoreCase);
    }

    public ExternalIdentityPayload ParseIdentity(ClaimsPrincipal principal)
    {
        var subject = principal.FindFirstValue("sub")
                      ?? principal.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? string.Empty;

        var issuer = principal.FindFirstValue("iss") ?? string.Empty;
        if (string.IsNullOrWhiteSpace(issuer)) issuer = "https://accounts.google.com";

        var email = principal.FindFirstValue(ClaimTypes.Email)
                    ?? principal.FindFirstValue("email")
                    ?? string.Empty;

        var displayName = principal.FindFirstValue(ClaimTypes.Name)
                          ?? principal.FindFirstValue("name")
                          ?? email;

        var emailVerifiedRaw = principal.FindFirstValue("email_verified");
        var emailVerified = string.Equals(emailVerifiedRaw, "true", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(emailVerifiedRaw, "1", StringComparison.OrdinalIgnoreCase);

        return new ExternalIdentityPayload
        {
            Issuer = issuer.Trim(),
            Subject = subject.Trim(),
            Email = email.Trim(),
            EmailVerified = emailVerified,
            DisplayName = displayName.Trim()
        };
    }
}
