/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Security.Claims;
using HCL.CS.SF.Domain.Enums;
using HCL.CS.SF.Domain.Models.Endpoint.Response;

namespace HCL.CS.SF.Domain.Models.Endpoint.Validation;

/// <summary>
/// Base class for all validated request models in the OAuth 2.0 / OIDC pipeline.
/// Contains the common state accumulated during request validation, including the
/// authenticated client, parsed secret, subject, scopes, and token configuration.
/// </summary>
public class ValidatedBaseModel : ErrorResponseModel
{
    /// <summary>The raw key-value pairs from the incoming request for reference during validation.</summary>
    public Dictionary<string, string> RequestRawData { get; set; }

    /// <summary>The OAuth 2.0 client identifier extracted from the request.</summary>
    public string ClientId { get; set; }

    /// <summary>The fully resolved client configuration model after lookup.</summary>
    public ClientsModel Client { get; set; }

    /// <summary>The parsed client secret used for client authentication.</summary>
    public ParsedSecretModel Secret { get; set; }

    /// <summary>The access token lifetime in seconds, derived from the client configuration.</summary>
    public int AccessTokenExpiration { get; set; }

    /// <summary>The access token format (JWT or Reference), derived from the client configuration.</summary>
    public AccessTokenType AccessTokenType { get; set; }

    /// <summary>The authenticated user's claims principal, if a user is involved in the request.</summary>
    public ClaimsPrincipal Subject { get; set; }

    /// <summary>The authentication session identifier associated with this request.</summary>
    public string SessionId { get; set; }

    /// <summary>Token configuration options (e.g., issuer, signing key) from server settings.</summary>
    public TokenSettings TokenConfigOptions { get; set; }

    /// <summary>The parsed and validated scope breakdown for this request.</summary>
    public AllowedScopesParserModel AllowedScopesParserModel { get; set; }

    /// <summary>The validated redirect URI for this request.</summary>
    public string RedirectUri { get; set; }

    /// <summary>The base URL of the identity server endpoints (used for issuer construction).</summary>
    public string EndpointBaseUrl { get; set; }

    /// <summary>
    /// Sets the client and optional secret on this validated request, copying token
    /// configuration values (expiration, token type) from the client model.
    /// </summary>
    /// <param name="client">The resolved client model. Must not be null.</param>
    /// <param name="secret">The parsed client secret, if available.</param>
    public void SetClient(ClientsModel client, ParsedSecretModel secret = null)
    {
        Client = client ?? throw new ArgumentNullException(nameof(client));
        Secret = secret;
        ClientId = client.ClientId;

        // Copy token settings from the client configuration
        AccessTokenExpiration = client.AccessTokenExpiration;
        AccessTokenType = client.AccessTokenType;
    }
}
