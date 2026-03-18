/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Security.Cryptography.X509Certificates;
using static HCL.CS.SF.Domain.Constants.Endpoint.OpenIdConstants;

namespace IntegrationTests.ApiDomainModel;

public abstract class BaseModel : BaseTrailModel
{
    public virtual Guid Id { get; set; }
}

public abstract class BaseTrailModel
{
    public string CreatedBy { get; set; }

    public string ModifiedBy { get; set; }

    public virtual DateTime CreatedOn { get; set; }

    public virtual DateTime? ModifiedOn { get; set; }

    public virtual bool IsDeleted { get; set; }


    public byte[] RowVersion { get; set; }
}

public abstract class BaseResponseModel : BaseModel
{
    public bool IsError { get; set; } = true;

    public string ErrorCode { get; set; }

    public string ErrorDescription { get; set; }
}

public class ApiResourcesModel : BaseModel
{
    public virtual string Name { get; set; }

    public virtual string DisplayName { get; set; }

    public virtual string Description { get; set; }

    public virtual bool Enabled { get; set; } = true;

    public virtual List<ApiResourceClaimsModel> ApiResourceClaims { get; set; }

    public virtual List<ApiScopesModel> ApiScopes { get; set; }
}

public class ApiScopeClaimsModel : BaseModel
{
    public virtual Guid ApiScopeId { get; set; }

    public virtual string Type { get; set; }
}

public class ApiResourceClaimsModel : BaseModel
{
    public virtual Guid ApiResourceId { get; set; }

    public virtual string Type { get; set; }
}

public class ApiScopesModel : BaseModel
{
    public virtual Guid ApiResourceId { get; set; }

    public virtual string Name { get; set; }

    public virtual string DisplayName { get; set; }

    public virtual string Description { get; set; }

    public virtual bool Required { get; set; } = false;

    public virtual bool Emphasize { get; set; } = false;

    public virtual List<ApiScopeClaimsModel> ApiScopeClaims { get; set; }
}

public class ClientRedirectUrisModel : BaseModel
{
    public Guid ClientId { get; set; }

    public string RedirectUri { get; set; }
}

public class ClientPostLogoutRedirectUrisModel : BaseModel
{
    public Guid ClientId { get; set; }

    public string PostLogoutRedirectUri { get; set; }
}

public class UserModel : BaseModel
{
    public virtual string UserName { get; set; }

    public virtual string Password { get; set; }

    public virtual string FirstName { get; set; }

    public virtual string LastName { get; set; }

    public virtual DateTime? DateOfBirth { get; set; }

    public virtual string Email { get; set; }

    public virtual bool EmailConfirmed { get; set; } = false;

    public virtual string PhoneNumber { get; set; }

    public virtual bool PhoneNumberConfirmed { get; set; } = false;

    public virtual bool TwoFactorEnabled { get; set; }

    public virtual TwoFactorType TwoFactorType { get; set; } = TwoFactorType.None;

    public virtual DateTimeOffset? LockoutEnd { get; set; }

    public virtual bool LockoutEnabled { get; set; }

    public virtual int AccessFailedCount { get; set; }

    public virtual DateTime? LastPasswordChangedDate { get; set; }

    public virtual bool? RequiresDefaultPasswordChange { get; set; }

    public virtual DateTime? LastLoginDateTime { get; set; }

    public virtual DateTime? LastLogoutDateTime { get; set; }

    public virtual IdentityProvider IdentityProviderType { get; set; } = IdentityProvider.Local;

    public virtual List<UserSecurityQuestionModel> UserSecurityQuestion { get; set; }

    public virtual List<UserClaimModel> UserClaims { get; set; }
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

public class UserClaimModel : BaseTrailModel
{
    public virtual int Id { get; set; }

    public virtual Guid UserId { get; set; }

    public virtual string ClaimType { get; set; }

    public virtual string ClaimValue { get; set; }

    public virtual bool IsAdminClaim { get; set; } = false;
}

public class ApiResourcesByScopesModel : BaseModel
{
    public virtual Guid ApiResourceId { get; set; }

    public virtual string ApiResourceName { get; set; }

    public virtual string ApiResourceClaimType { get; set; }

    public virtual string ApiScopeName { get; set; }

    public virtual string ApiScopeClaimType { get; set; }
}

public class IdentityResourcesByScopesModel
{
    public virtual Guid IdentityResourceId { get; set; }

    public virtual string IdentityResourceName { get; set; }

    public virtual string IdentityResourceClaimType { get; set; }

    public virtual string IdentityResourceClaimAliasType { get; set; }
}

public class UserSecurityQuestionModel : BaseModel
{
    public virtual Guid UserId { get; set; }

    public virtual Guid SecurityQuestionId { get; set; }

    public virtual string Answer { get; set; }
}

public class RoleModel : BaseModel
{
    public virtual string Name { get; set; }

    public virtual string Description { get; set; }

    public virtual List<RoleClaimModel> RoleClaims { get; set; }
}

public class RoleClaimModel : BaseTrailModel
{
    public virtual int Id { get; set; }

    public virtual Guid RoleId { get; set; }

    public virtual string ClaimType { get; set; }

    public virtual string ClaimValue { get; set; }
}

public class LogoutMessageModel
{
    public string ClientId { get; set; }

    public string PostLogoutRedirectUri { get; set; }

    public string SubjectId { get; set; }

    public string SessionId { get; set; }

    public IEnumerable<string> ClientIdCollection { get; set; }

    public Dictionary<string, string> Parameters { get; set; } = new();

    public bool HasClient => !string.IsNullOrWhiteSpace(ClientId) || ClientIdCollection?.Any() == true;

    public class UserRoleModel : BaseModel
    {
        public virtual Guid RoleId { get; set; }

        public virtual Guid UserId { get; set; }

        public virtual DateTime? ValidFrom { get; set; }

        public virtual DateTime? ValidTo { get; set; }
    }

    public class AsymmetricKeyInfoModel
    {
        public string KeyId { get; set; }

        public SigningAlgorithm Algorithm { get; set; }

        public X509Certificate2 Certificate { get; set; }
    }

    public class SignInResponseModel
    {
        public bool Succeeded { get; set; } = false;

        public bool IsLockedOut { get; set; } = false;

        public bool IsNotAllowed { get; set; } = false;

        public bool RequiresTwoFactor { get; set; } = false;

        public string Message { get; set; }

        public bool VerificationCodeSent { get; set; } = false;

        public NotificationTypes VerificationMode { get; set; }

        public string ErrorCode { get; set; }

        public string UserVerificationCode { get; set; }
    }
}

public class SecurityQuestionModel : BaseModel
{
    public virtual string Question { get; set; }
}

public class AuthenticatorAppSetupResponseModel
{
    public string SharedKey { get; set; }

    public string AuthenticatorUri { get; set; }

    public string VerificationCode { get; set; }
}

public class AuditSearchRequestModel : BaseModel
{
    public AuditType ActionType { get; set; } = AuditType.None;

    public string CreatedBy { get; set; }

    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    public PagingModel Page { get; set; }

    public DateTime? CreatedOn { get; set; }

    public string SearchValue { get; set; }
}

public class PagingModel
{
    public int TotalItems { get; set; }

    public int ItemsPerPage { get; set; }

    public int CurrentPage { get; set; }

    public int TotalPages => (int)Math.Ceiling((decimal)TotalItems / ItemsPerPage);

    public int TotalDisplayPages { get; set; }
}
