/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Security.Claims;
using HCL.CS.SF.Domain.Constants.Endpoint;
using HCL.CS.SF.Domain.Models.Endpoint.Request;
using HCL.CS.SF.Domain.Models.Endpoint.Response;

namespace HCL.CS.SF.Domain.Models.Endpoint.Validation;

/// <summary>
/// Validation result for the OAuth 2.0 Resource Owner Password Credentials (ROPC) grant.
/// Validates the resource owner's credentials and constructs a claims principal with the
/// required OIDC claims (sub, amr, idp, auth_time) upon successful authentication.
/// </summary>
public class RopValidationModel : ErrorResponseModel
{
    /// <summary>Default constructor. IsError defaults to true until validation succeeds.</summary>
    public RopValidationModel()
    {
    }

    /// <summary>
    /// Constructs a validation result from an existing claims principal.
    /// Validates that the principal contains exactly one identity with the required OIDC claims.
    /// </summary>
    /// <param name="principal">The pre-authenticated claims principal to validate.</param>
    public RopValidationModel(ClaimsPrincipal principal)
    {
        IsError = false;
        // Require exactly one identity in the principal
        if (principal.Identities.Count() != 1) return;

        // Validate required OIDC claims are present
        if (principal.FindFirst(OpenIdConstants.ClaimTypes.Sub) == null) return;

        if (principal.FindFirst(OpenIdConstants.ClaimTypes.IdentityProvider) == null) return;

        if (principal.FindFirst(OpenIdConstants.ClaimTypes.AuthenticationMethod) == null) return;

        if (principal.FindFirst(OpenIdConstants.ClaimTypes.AuthenticationTime) == null) return;

        Subject = principal;
    }

    /// <summary>
    /// Constructs a successful validation result by building a claims principal from individual values.
    /// Creates the required OIDC claims (sub, amr, idp, auth_time) and merges any additional claims.
    /// </summary>
    /// <param name="subject">The subject identifier (sub) of the authenticated user.</param>
    /// <param name="authenticationMethod">The authentication method reference (amr) value.</param>
    /// <param name="authTime">The UTC time when the user was authenticated.</param>
    /// <param name="claims">Optional additional claims to include in the principal.</param>
    /// <param name="identityProvider">The identity provider identifier. Defaults to the local provider.</param>
    public RopValidationModel(
        string subject,
        string authenticationMethod,
        DateTime authTime,
        IEnumerable<Claim> claims = null,
        string identityProvider = AuthenticationConstants.LocalIdentityProvider)
    {
        IsError = false;

        // Build the core OIDC claims required for token generation
        var resultClaims = new List<Claim>
        {
            new(OpenIdConstants.ClaimTypes.Sub, subject),
            new(OpenIdConstants.ClaimTypes.AuthenticationMethod, authenticationMethod),
            new(OpenIdConstants.ClaimTypes.IdentityProvider, identityProvider),
            new(OpenIdConstants.ClaimTypes.AuthenticationTime,
                new DateTimeOffset(authTime).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        if (claims != null && claims.Any()) resultClaims.AddRange(claims);

        // Create a ClaimsIdentity with the authentication method as the authentication type
        var id = new ClaimsIdentity(authenticationMethod);
        id.AddClaims(resultClaims.Distinct());

        Subject = new ClaimsPrincipal(id);
    }

    /// <summary>The resource owner's username submitted in the token request.</summary>
    public string UserName { get; set; }

    /// <summary>The resource owner's password submitted in the token request.</summary>
    public string Password { get; set; }

    /// <summary>The validated token request that triggered this ROPC validation.</summary>
    public ValidatedTokenRequestModel Request { get; set; }

    /// <summary>The authenticated resource owner's claims principal, set upon successful validation.</summary>
    public ClaimsPrincipal Subject { get; set; }
}
