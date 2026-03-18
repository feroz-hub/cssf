/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Security.Claims;
using HCL.CS.SF.Domain;
using static HCL.CS.SF.Domain.Constants.Endpoint.OpenIdConstants;

namespace IntegrationTests.ApiDomainModel;

public class AuthorizationResponseModel : ErrorResponseModel
{
    public ValidatedAuthorizeRequestModel Request { get; set; }

    public string RedirectUri => Request?.RedirectUri;

    public string State => Request?.State;

    public string Scope { get; set; }

    public string IdentityToken { get; set; }

    public string AccessToken { get; set; }

    public string RefreshToken { get; set; }

    public int AccessTokenLifetime { get; set; }

    public string Code { get; set; }

    public string SessionState { get; set; }
}

public class ErrorResponseModel
{
    public bool IsError { get; set; } = true;

    public string ErrorCode { get; set; }

    public string ErrorDescription { get; set; }
}

public class ErrorResponseResultModel
{
    public string error { get; set; }

    public string error_description { get; set; }
}

public class ValidatedAuthorizeRequestModel : ValidatedBaseModel
{
    public ValidatedAuthorizeRequestModel()
    {
        RequestedScopes = new List<string>();
        AuthenticationContextReferenceClasses = new List<string>();
    }

    public string ResponseType { get; set; }

    public string ResponseMode { get; set; }

    public string GrantType { get; set; }

    public List<string> RequestedScopes { get; set; }

    public string State { get; set; }

    public bool IsOpenIdRequest { get; set; } = false;

    public bool IsApiResourceRequest { get; set; }

    public string Nonce { get; set; }

    public List<string> AuthenticationContextReferenceClasses { get; set; }

    public IEnumerable<string> PromptModes { get; set; } = Enumerable.Empty<string>();

    public int? MaxAge { get; set; }

    public string CodeChallenge { get; set; }

    public string CodeChallengeMethod { get; set; }

    public ClaimsPrincipal User { get; set; }
}

public class ValidatedBaseModel : ErrorResponseModel
{
    public Dictionary<string, string> RequestRawData { get; set; }

    public string ClientId { get; set; }

    public ClientsModel Client { get; set; }

    public ParsedSecretModel Secret { get; set; }

    public int AccessTokenExpiration { get; set; }

    public AccessTokenType AccessTokenType { get; set; }

    public ClaimsPrincipal Subject { get; set; }

    public string SessionId { get; set; }

    public TokenSettings TokenConfigOptions { get; set; }

    public AllowedScopesParserModel AllowedScopesParserModel { get; set; }

    public string RedirectUri { get; set; }

    public string EndpointBaseUrl { get; set; }

    public void SetClient(ClientsModel client, ParsedSecretModel secret = null)
    {
        Client = client ?? throw new ArgumentNullException(nameof(client));
        Secret = secret;
        ClientId = client.ClientId;

        AccessTokenExpiration = client.AccessTokenExpiration;
        AccessTokenType = client.AccessTokenType;
    }
}

public class ParsedSecretModel : ErrorResponseModel
{
    public string ClientId { get; set; }

    public object Credential { get; set; }

    public string Type { get; set; }

    public ParseMethods ParseMethod { get; set; }
}

public class AllowedScopesParserModel
{
    public List<string> ParsedIdentityResources { get; set; }

    public List<string> ParsedApiResources { get; set; }

    public List<string> ParsedApiScopes { get; set; }

    public List<string> ParsedTransactionScopes { get; set; }

    public bool AllowOfflineAccess { get; set; }

    public bool CreateIdentityToken { get; set; }

    public List<string> InvalidScopes { get; set; }

    public TokenDetailsModel TokenDetails { get; set; }

    public List<string> ParsedAllowedScopes
    {
        get
        {
            var parsedAllowedScopes = new List<string>();
            if (ParsedIdentityResources != null) parsedAllowedScopes.AddRange(ParsedIdentityResources);

            if (ParsedApiScopes != null) parsedAllowedScopes.AddRange(ParsedApiScopes);

            // TODO Refactor this property into a method.
            return parsedAllowedScopes.Distinct().ToList();
        }
    }

    public class TokenDetailsModel
    {
        public virtual UserModel User { get; set; }

        public virtual ClientsModel Client { get; set; }

        public virtual IList<IdentityResourcesModel> IdentityResources { get; set; } =
            new List<IdentityResourcesModel>();

        public virtual IList<ApiResourcesModel> ApiResources { get; set; } = new List<ApiResourcesModel>();

        public virtual IList<ApiScopesModel> ApiScopes { get; set; } = new List<ApiScopesModel>();

        public virtual IList<IdentityResourcesByScopesModel> IdentityResourcesByScopes { get; set; } =
            new List<IdentityResourcesByScopesModel>();

        public virtual IList<ApiResourcesByScopesModel> ApiResourcesByScopes { get; set; } =
            new List<ApiResourcesByScopesModel>();

        public virtual IList<UserRoleClaimTypesModel> UserRoleClaimTypes { get; set; } =
            new List<UserRoleClaimTypesModel>();

        public virtual IList<string> UserRoles { get; set; } = new List<string>();

        public virtual IList<UserRoleClaimsModel> RolePermissions { get; set; } = new List<UserRoleClaimsModel>();
    }

    public class UserRoleClaimsModel
    {
        public virtual Guid RoleId { get; set; }

        public virtual string RoleName { get; set; }

        public virtual IList<Claim> Claims { get; set; }
    }

    public class UserRoleClaimTypesModel
    {
        public virtual Guid UserId { get; set; }

        public virtual string UserName { get; set; }

        public virtual string RoleName { get; set; }

        public virtual string RoleClaimType { get; set; }

        public virtual string RoleClaimValue { get; set; }
    }

    public class IdentityResourcesModel : BaseModel
    {
        public virtual string Name { get; set; }

        public virtual string DisplayName { get; set; }

        public virtual string Description { get; set; }

        public virtual bool Enabled { get; set; } = true;

        public virtual bool Required { get; set; } = false;

        public virtual bool Emphasize { get; set; } = false;

        public virtual List<IdentityClaimsModel> IdentityClaims { get; set; }
    }

    public class IdentityClaimsModel : BaseModel
    {
        public virtual Guid IdentityResourceId { get; set; }

        public virtual string Type { get; set; }

        public virtual string AliasType { get; set; }
    }

    public class ClientsModel : BaseModel
    {
        public string ClientId { get; set; }

        public string ClientName { get; set; }

        public string ClientUri { get; set; }

        public DateTime ClientIdIssuedAt { get; set; }

        public DateTime ClientSecretExpiresAt { get; set; }

        public string ClientSecret { get; set; }

        public string LogoUri { get; set; }

        public string TermsOfServiceUri { get; set; }

        public string PolicyUri { get; set; }

        public int RefreshTokenExpiration { get; set; } = 86400;

        public int AccessTokenExpiration { get; set; } = 3600;

        public int IdentityTokenExpiration { get; set; } = 3600;

        public int LogoutTokenExpiration { get; set; } = 1800;

        public int AuthorizationCodeExpiration { get; set; } = 1800;

        public AccessTokenType AccessTokenType { get; set; } = AccessTokenType.JWT;

        public bool RequirePkce { get; set; }

        public bool IsPkceTextPlain { get; set; }

        public bool RequireClientSecret { get; set; } = true;

        public bool IsFirstPartyApp { get; set; } = true;

        public bool AllowOfflineAccess { get; set; }

        public bool AllowAccessTokensViaBrowser { get; set; }

        public ApplicationType ApplicationType { get; set; }

        public string AllowedSigningAlgorithm { get; set; } = Algorithms.HmacSha256;

        public bool FrontChannelLogoutSessionRequired { get; set; } = false;

        public string FrontChannelLogoutUri { get; set; }

        public bool BackChannelLogoutSessionRequired { get; set; }

        public string BackChannelLogoutUri { get; set; }

        public List<string> SupportedGrantTypes { get; set; }

        public List<string> SupportedResponseTypes { get; set; }

        public List<string> AllowedScopes { get; set; }

        public List<ClientRedirectUrisModel> RedirectUris { get; set; }

        public List<ClientPostLogoutRedirectUrisModel> PostLogoutRedirectUris { get; set; }
    }

    public class AuthorizeErrorResponseModel : ErrorResponseModel
    {
        public virtual string ErrorUri { get; set; }

        public virtual string State { get; set; }

        // TODO Update the below 4 values where it is applicable.

        public string TraceId { get; set; }

        public string ClientId { get; set; }

        public string RedirectUri { get; set; }

        public string ResponseMode { get; set; }
    }

    public class IntrospectionResponseModel
    {
        public bool Active { get; set; }

        public string ClientId { get; set; }

        public string UserName { get; set; }

        public string Scope { get; set; }

        public string SubjectId { get; set; }

        public string Audience { get; set; }

        public string Issuer { get; set; }

        public string ExpiresAt { get; set; }

        public string IssuedAt { get; set; }
    }
}
