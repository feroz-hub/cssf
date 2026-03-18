/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Domain.Models.Endpoint;

/// <summary>
/// Contains the results of parsing and validating the requested scopes against
/// the client's allowed scopes. Categorizes scopes into identity resources, API resources,
/// API scopes, and transaction scopes, and flags invalid or unsupported scope values.
/// </summary>
public class AllowedScopesParserModel
{
    /// <summary>Identity resource scope names (e.g., "openid", "profile") that matched allowed scopes.</summary>
    public List<string> ParsedIdentityResources { get; set; }

    /// <summary>API resource names resolved from the requested scopes.</summary>
    public List<string> ParsedApiResources { get; set; }

    /// <summary>API scope names (e.g., "read", "write") that matched allowed scopes.</summary>
    public List<string> ParsedApiScopes { get; set; }

    /// <summary>Transaction-specific scopes parsed from the request.</summary>
    public List<string> ParsedTransactionScopes { get; set; }

    /// <summary>Indicates whether the "offline_access" scope was requested and the client is allowed to use it.</summary>
    public bool AllowOfflineAccess { get; set; }

    /// <summary>Indicates whether an identity token should be created (true when "openid" scope is present).</summary>
    public bool CreateIdentityToken { get; set; }

    /// <summary>Scope values from the request that did not match any registered or allowed scope.</summary>
    public List<string> InvalidScopes { get; set; }

    /// <summary>The resolved token details (user, client, resources) for token generation.</summary>
    public TokenDetailsModel TokenDetails { get; set; }

    /// <summary>
    /// Computes the combined list of all valid allowed scopes (identity resources + API scopes),
    /// with duplicates removed.
    /// </summary>
    public List<string> ParsedAllowedScopes
    {
        get
        {
            // Merge identity resource and API scope names into a single deduplicated list
            var parsedAllowedScopes = new List<string>();
            if (ParsedIdentityResources != null) parsedAllowedScopes.AddRange(ParsedIdentityResources);

            if (ParsedApiScopes != null) parsedAllowedScopes.AddRange(ParsedApiScopes);

            // TODO Refactor this property into a method.
            return parsedAllowedScopes.Distinct().ToList();
        }
    }
}
