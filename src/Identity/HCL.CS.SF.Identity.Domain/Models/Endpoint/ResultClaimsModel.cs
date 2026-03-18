/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Security.Claims;

namespace HCL.CS.SF.Domain.Models.Endpoint;

/// <summary>
/// Holds the categorized claims that will be embedded into identity tokens and access tokens.
/// Claims are separated by category (role, audience, scope, identity, transaction, permission)
/// to allow fine-grained control over which claims appear in which token type.
/// </summary>
public class ResultClaimsModel
{
    /// <summary>Claims derived from the user's assigned roles (e.g., "role" claim).</summary>
    public virtual List<Claim> RoleClaims { get; set; }

    /// <summary>Audience claims (aud) identifying the intended recipients of the token.</summary>
    public virtual List<Claim> AudienceClaims { get; set; }

    /// <summary>Scope-derived claims to be included in identity tokens.</summary>
    public virtual List<Claim> IdentityTokenScopeClaims { get; set; }

    /// <summary>Scope-derived claims to be included in access tokens.</summary>
    public virtual List<Claim> AccessTokenScopeClaims { get; set; }

    /// <summary>User identity claims (e.g., name, email) resolved from identity resources.</summary>
    public virtual List<Claim> IdentityClaims { get; set; }

    /// <summary>Transaction-specific claims for custom transaction scopes.</summary>
    public virtual List<Claim> TransactionClaims { get; set; }

    /// <summary>Permission claims derived from role-based access control (RBAC) policies.</summary>
    public virtual List<Claim> PermissionClaims { get; set; }

    /// <summary>Custom access-token claims that keep their claim type (e.g. capabilities). Not merged into permission claims.</summary>
    public virtual List<Claim> CustomAccessTokenClaims { get; set; }

    /// <summary>
    /// Computes the aggregate list of all claims to be embedded in the access token
    /// by combining scope, role, transaction, permission, and custom claims.
    /// </summary>
    public List<Claim> AccessTokenClaims
    {
        get
        {
            // Merge all claim categories that belong in the access token
            var accessTokenClaims = new List<Claim>();
            accessTokenClaims.AddRange(IdentityTokenScopeClaims);
            accessTokenClaims.AddRange(RoleClaims);
            accessTokenClaims.AddRange(TransactionClaims);
            accessTokenClaims.AddRange(PermissionClaims);
            if (CustomAccessTokenClaims != null)
                accessTokenClaims.AddRange(CustomAccessTokenClaims);
            return accessTokenClaims;
        }
    }
}
