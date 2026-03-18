export type UUID = string;

export type ResultStatus = 0 | 1 | "Succeeded" | "Failed";

export interface FrameworkError {
  Code: string;
  Description: string;
}

export interface FrameworkResult {
  Status: ResultStatus;
  Errors?: FrameworkError[];
}

export interface BaseTrailModel {
  CreatedBy?: string;
  ModifiedBy?: string;
  CreatedOn?: string;
  ModifiedOn?: string | null;
  IsDeleted?: boolean;
  RowVersion?: string;
}

export interface BaseModel extends BaseTrailModel {
  Id: UUID;
}

export interface ClientRedirectUrisModel extends BaseModel {
  ClientId: UUID;
  RedirectUri: string;
}

export interface ClientPostLogoutRedirectUrisModel extends BaseModel {
  ClientId: UUID;
  PostLogoutRedirectUri: string;
}

export type ApplicationType = 1 | 2 | 3 | 4;

export interface ClientsModel extends BaseModel {
  ClientId: string;
  ClientName: string;
  ClientUri: string;
  ClientIdIssuedAt: string;
  ClientSecretExpiresAt: string;
  ClientSecret: string;
  LogoUri: string;
  TermsOfServiceUri: string;
  PolicyUri: string;
  RefreshTokenExpiration: number;
  AccessTokenExpiration: number;
  IdentityTokenExpiration: number;
  LogoutTokenExpiration: number;
  AuthorizationCodeExpiration: number;
  AccessTokenType: number;
  RequirePkce: boolean;
  IsPkceTextPlain: boolean;
  RequireClientSecret: boolean;
  IsFirstPartyApp: boolean;
  AllowOfflineAccess: boolean;
  AllowAccessTokensViaBrowser: boolean;
  ApplicationType: ApplicationType;
  AllowedSigningAlgorithm: string;
  FrontChannelLogoutSessionRequired: boolean;
  FrontChannelLogoutUri: string;
  BackChannelLogoutSessionRequired: boolean;
  BackChannelLogoutUri: string;
  SupportedGrantTypes: string[];
  SupportedResponseTypes: string[];
  AllowedScopes: string[];
  RedirectUris: ClientRedirectUrisModel[];
  PostLogoutRedirectUris: ClientPostLogoutRedirectUrisModel[];
  /** Optional. When set, access tokens for this client use this value as the aud claim (e.g. rentflow.api, HCL.CS.SF.api). */
  PreferredAudience?: string;
}

export interface ApiResourceClaimsModel extends BaseModel {
  ApiResourceId: UUID;
  Type: string;
}

export interface ApiScopeClaimsModel extends BaseModel {
  ApiScopeId: UUID;
  Type: string;
}

export interface ApiScopesModel extends BaseModel {
  ApiResourceId: UUID;
  Name: string;
  DisplayName: string;
  Description: string;
  Required: boolean;
  Emphasize: boolean;
  ApiScopeClaims: ApiScopeClaimsModel[];
}

export interface ApiResourcesModel extends BaseModel {
  Name: string;
  DisplayName: string;
  Description: string;
  Enabled: boolean;
  ApiResourceClaims: ApiResourceClaimsModel[];
  ApiScopes: ApiScopesModel[];
}

export interface RoleClaimModel extends BaseTrailModel {
  Id: number;
  RoleId: UUID;
  ClaimType: string;
  ClaimValue: string;
}

export interface RoleModel extends BaseModel {
  Name: string;
  Description: string;
  RoleClaims: RoleClaimModel[];
}

export type TwoFactorType = 0 | 1 | 2 | 3;
export type IdentityProvider = 1 | 2 | 3;

export interface UserSecurityQuestionModel extends BaseModel {
  UserId: UUID;
  SecurityQuestionId: UUID;
  Answer: string;
}

export interface UserClaimModel extends BaseTrailModel {
  Id: number;
  UserId: UUID;
  ClaimType: string;
  ClaimValue: string;
  IsAdminClaim: boolean;
}

export interface UserModel extends BaseModel {
  UserName: string;
  Password: string;
  FirstName: string;
  LastName: string;
  DateOfBirth: string | null;
  Email: string;
  EmailConfirmed: boolean;
  PhoneNumber: string;
  PhoneNumberConfirmed: boolean;
  TwoFactorEnabled: boolean;
  TwoFactorType: TwoFactorType;
  LockoutEnd: string | null;
  LockoutEnabled: boolean;
  AccessFailedCount: number;
  LastPasswordChangedDate: string | null;
  RequiresDefaultPasswordChange: boolean | null;
  LastLoginDateTime: string | null;
  LastLogoutDateTime: string | null;
  IdentityProviderType: IdentityProvider;
  UserSecurityQuestion: UserSecurityQuestionModel[];
  UserClaims: UserClaimModel[];
}

export interface UserDisplayModel extends BaseModel {
  UserName: string;
  Password: string;
  FirstName: string;
  LastName: string;
  Email: string;
  PhoneNumber: string;
  LockoutEnd: string | null;
  LockoutEnabled: boolean;
}

export interface UserRoleModel extends BaseModel {
  RoleId: UUID;
  UserId: UUID;
  ValidFrom: string | null;
  ValidTo: string | null;
}

export interface UserRoleClaimsModel {
  RoleId: UUID;
  RoleName: string;
  Claims: RoleClaimModel[];
}

export interface UserPermissionsResponseModel {
  UserId: UUID;
  RolePermissions: UserRoleClaimsModel[];
}

export interface PagingModel {
  TotalItems: number;
  ItemsPerPage: number;
  CurrentPage: number;
  TotalPages: number;
  TotalDisplayPages: number;
}

export type AuditType = 0 | 1 | 2 | 3;

export interface AuditTrailModel {
  Id: UUID;
  ActionType: AuditType;
  TableName: string;
  OldValue: string | null;
  NewValue: string | null;
  AffectedColumn: string | null;
  ActionName: string;
  CreatedBy: string | null;
  CreatedOn: string;
}

export interface AuditSearchRequestModel {
  ActionType: AuditType;
  CreatedBy: string;
  FromDate: string | null;
  ToDate: string | null;
  Page: PagingModel;
  CreatedOn: string | null;
  SearchValue: string;
}

export interface AuditResponseModel {
  AuditList: AuditTrailModel[];
  PageInfo: PagingModel;
}

export interface TokenModel {
  ClientId: string;
  ClientName: string;
  UserId?: UUID;
  UserName: string;
  LoginDateTime: string | null;
  Token: string;
  TokenTypeHint: "access_token" | "refresh_token";
}

export interface SecurityQuestionModel extends BaseModel {
  Question: string;
}

export interface IdentityClaimsModel extends BaseModel {
  IdentityResourceId: UUID;
  Type: string;
  AliasType: string;
}

export interface IdentityResourcesModel extends BaseModel {
  Name: string;
  DisplayName: string;
  Description: string;
  Enabled: boolean;
  Required: boolean;
  Emphasize: boolean;
  IdentityClaims: IdentityClaimsModel[];
}

export interface IdentityResourcesByScopesModel {
  IdentityResourceId: UUID;
  IdentityResourceName: string;
  IdentityResourceClaimType: string;
  IdentityResourceClaimAliasType: string;
}

export interface SignInResponseModel {
  Succeeded: boolean;
  IsLockedOut: boolean;
  IsNotAllowed: boolean;
  RequiresTwoFactor: boolean;
  TwoFactorVerificationMode: TwoFactorType;
  TwoFactorVerificationCodeSent: boolean;
  Message: string;
  ErrorCode: string;
  UserVerificationCode: string;
}

export interface AuthenticatorAppSetupResponseModel {
  SharedKey: string;
  AuthenticatorUri: string;
  VerificationCode: string;
}

export interface AuthenticatorAppResponseModel {
  Succeeded: boolean;
  Message: string;
  RecoveryCodes: string[];
}

// Snake_case request payloads expected by gateway routes
export type PasswordSignInRequest = {
  user_name: string;
  password: string;
};

export type PasswordSignInByTwoFactorAuthenticatorTokenRequest = {
  user_name: string;
  password: string;
  two_factor_authenticator_token: string;
};

export type SetupAuthenticatorAppRequest = {
  user_id: UUID;
  application_name: string;
};

export type VerifyAuthenticatorAppSetupRequest = {
  user_id: UUID;
  user_token: string;
};

export type LockUserWithEndDateRequest = {
  user_id: UUID;
  end_date: string;
};

export type ChangePasswordRequest = {
  user_id: UUID;
  current_password: string;
  new_password: string;
};

export type ResetPasswordRequest = {
  user_name: string;
  password_reset_token: string;
  new_password: string;
};

export type SetTwoFactorEnabledRequest = {
  user_id: UUID;
  enabled: boolean;
};

export type UpdateUserTwoFactorTypeRequest = {
  user_id: UUID;
  two_factor_type: TwoFactorType;
};

export type UnLockUserByTokenRequest = {
  user_name: string;
  user_token: string;
  token_purpose: string;
};

export type GeneratePasswordResetTokenRequest = {
  user_name: string;
  notification_type: number;
};

export type GenerateUserTokenRequest = {
  user_name: string;
  token_purpose: string;
  notification_template: string;
  notification_type: number;
};

export type VerifyUserTokenRequest = {
  user_name: string;
  token_purpose: string;
  user_token: string;
};

export type VerifyEmailTokenRequest = {
  user_name: string;
  email_token: string;
};

export type VerifySmsTokenRequest = {
  user_name: string;
  sms_token: string;
};

export type GetUsersForClaimRequest = {
  claim_type: string;
  claim_value: string;
};

export type UnLockUserBySecurityQuestionsRequest = {
  user_name: string;
  user_security_questions_list: UserSecurityQuestionModel[];
};

export interface SecurityTokensModel extends BaseModel {
  Key: string;
  TokenType: string;
  TokenValue: string;
  ConsumedTime: string | null;
  ConsumedAt: string | null;
  TokenReuseDetected: boolean;
  ClientId: string;
  SessionId: string;
  UserId: string;
  SubjectId: string;
  CreationTime: string;
  ExpiresAt: number;
}

export interface ClaimValueModel {
  Type: string;
  Value: string;
}

export interface ActionResult<TData = void> {
  ok: boolean;
  message: string;
  errors?: Record<string, string[]>;
  data?: TData;
}

export interface ApiListItem<TData> {
  items: TData[];
  total: number;
  page: number;
  pageSize: number;
}

// Notification Management Types

export interface NotificationSearchRequestModel {
  Type?: number | null;
  Status?: number | null;
  FromDate?: string | null;
  ToDate?: string | null;
  SearchValue?: string;
  Page: PagingModel;
}

export interface NotificationLogModel {
  Id: UUID;
  UserId: UUID;
  MessageId: string;
  Type: number;
  Activity: string;
  Status: number;
  Sender: string;
  Recipient: string;
  CreatedOn: string;
}

export interface NotificationLogResponseModel {
  Notifications: NotificationLogModel[];
  PageInfo: PagingModel;
}

export interface NotificationTemplateResponseModel {
  EmailTemplates: EmailTemplateModel[];
  SmsTemplates: SmsTemplateModel[];
}

export interface EmailTemplateModel {
  Name: string;
  Subject: string;
  FromAddress: string;
  FromName: string;
  CC: string;
  TemplateFormat: string;
}

export interface SmsTemplateModel {
  Name: string;
  TemplateFormat: string;
}

export interface ProviderConfigModel {
  Id?: UUID;
  ProviderName: string;
  ChannelType: number;
  IsActive: boolean;
  Settings: Record<string, string>;
  LastTestedOn?: string | null;
  LastTestSuccess?: boolean | null;
}

export interface SaveProviderConfigRequest {
  Id?: UUID | null;
  ProviderName: string;
  ChannelType: number;
  IsActive: boolean;
  Settings: Record<string, string>;
}

export interface SetActiveProviderRequest {
  Id: UUID;
}

export interface DeleteProviderConfigRequest {
  Id: UUID;
}

export interface ProviderFieldDefinition {
  Key: string;
  Label: string;
  InputType: string;
  Required: boolean;
}

export interface ProviderFieldDefinitionsResponse {
  EmailProviders: Record<string, ProviderFieldDefinition[]>;
  SmsProviders: Record<string, ProviderFieldDefinition[]>;
}

export interface SendTestNotificationRequest {
  Type: number;
  Recipient: string;
  ProviderConfigId?: UUID | null;
}

// External Auth Provider Config
export interface ExternalAuthProviderConfigModel {
  Id?: UUID;
  ProviderName: string;
  ProviderType: number;
  IsEnabled: boolean;
  Settings: Record<string, string>;
  AutoProvisionEnabled: boolean;
  AllowedDomains?: string | null;
  LastTestedOn?: string | null;
  LastTestSuccess?: boolean | null;
}

export interface SaveExternalAuthProviderRequest {
  Id?: UUID | null;
  ProviderName: string;
  ProviderType: number;
  IsEnabled: boolean;
  Settings: Record<string, string>;
  AutoProvisionEnabled: boolean;
  AllowedDomains?: string | null;
}

export interface DeleteExternalAuthProviderRequest {
  Id: UUID;
}

export interface TestExternalAuthProviderRequest {
  Id: UUID;
}

export interface ExternalAuthFieldDefinitionsResponse {
  Providers: Record<string, ProviderFieldDefinition[]>;
  Defaults: Record<string, Record<string, string>>;
}
