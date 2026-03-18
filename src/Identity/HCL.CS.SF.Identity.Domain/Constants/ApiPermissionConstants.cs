/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.Domain.Constants;

/// <summary>
/// Defines the fine-grained permission claim values used to authorize access to Admin API endpoints.
/// Each constant represents a specific permission that can be assigned to roles and included in access tokens.
/// The API gateway checks these permission claims against the route's required permissions
/// before forwarding requests to the identity service.
/// </summary>
public static class ApiPermissionConstants
{
    /// <summary>
    /// The base namespace prefix for all permission claim values (e.g., "HCL.CS.SF.").
    /// </summary>
    public const string BaseName = "HCL.CS.SF.";

    /// <summary>
    /// Permission for anonymous (unauthenticated) access. Assigned to public API operations
    /// such as user registration, sign-in, and token verification.
    /// </summary>
    public const string Anonymous = BaseName + "anonymous";

    // ──── API Resource Permissions ────

    /// <summary>
    /// Permission to read API resource definitions and their associated scopes and claims.
    /// </summary>
    public const string ApiResourceRead = BaseName + "apiresource.read";

    /// <summary>
    /// Permission to create and update API resource definitions.
    /// </summary>
    public const string ApiResourceWrite = BaseName + "apiresource.write";

    /// <summary>
    /// Permission to delete API resource definitions.
    /// </summary>
    public const string ApiResourceDelete = BaseName + "apiresource.delete";

    /// <summary>
    /// Full management permission for API resources (includes read, write, and delete).
    /// </summary>
    public const string ApiResourceManage = BaseName + "apiresource.manage";

    // ──── User Permissions ────

    /// <summary>
    /// Permission to read user profile information and claims.
    /// </summary>
    public const string UserRead = BaseName + "user.read";

    /// <summary>
    /// Permission to create and update user accounts and their claims.
    /// </summary>
    public const string UserWrite = BaseName + "user.write";

    /// <summary>
    /// Permission to delete user accounts and their associated data.
    /// </summary>
    public const string UserDelete = BaseName + "user.delete";

    /// <summary>
    /// Full management permission for user accounts (includes read, write, and delete).
    /// </summary>
    public const string UserManage = BaseName + "user.manage";

    // ──── Identity Resource Permissions ────

    /// <summary>
    /// Permission to read identity resource definitions (e.g., openid, profile, email scopes).
    /// </summary>
    public const string IdentityResourceRead = BaseName + "identityresource.read";

    /// <summary>
    /// Permission to create and update identity resource definitions.
    /// </summary>
    public const string IdentityResourceWrite = BaseName + "identityresource.write";

    /// <summary>
    /// Permission to delete identity resource definitions.
    /// </summary>
    public const string IdentityResourceDelete = BaseName + "identityresource.delete";

    /// <summary>
    /// Full management permission for identity resources (includes read, write, and delete).
    /// </summary>
    public const string IdentityResourceManage = BaseName + "identityresource.manage";

    // ──── Role Permissions ────

    /// <summary>
    /// Permission to read role definitions and their associated claims.
    /// </summary>
    public const string RoleRead = BaseName + "role.read";

    /// <summary>
    /// Permission to create and update roles and assign role claims.
    /// </summary>
    public const string RoleWrite = BaseName + "role.write";

    /// <summary>
    /// Permission to delete roles.
    /// </summary>
    public const string RoleDelete = BaseName + "role.delete";

    /// <summary>
    /// Full management permission for roles (includes read, write, and delete).
    /// </summary>
    public const string RoleManage = BaseName + "role.manage";

    // ──── Admin User Permissions ────

    /// <summary>
    /// Permission to read administrative user accounts and their claims.
    /// </summary>
    public const string AdminRead = BaseName + "adminuser.read";

    /// <summary>
    /// Permission to create and update administrative user accounts, lock/unlock users, and manage admin claims.
    /// </summary>
    public const string AdminWrite = BaseName + "adminuser.write";

    /// <summary>
    /// Permission to delete administrative user accounts and remove admin claims.
    /// </summary>
    public const string AdminDelete = BaseName + "adminuser.delete";

    /// <summary>
    /// Full management permission for administrative users (includes read, write, and delete).
    /// </summary>
    public const string AdminManage = BaseName + "adminuser.manage";

    // ──── Audit Trail Permissions ────

    /// <summary>
    /// Permission to read audit trail records (change history for tracked entities).
    /// </summary>
    public const string AuditRead = BaseName + "audittrail.read";

    /// <summary>
    /// Full management permission for the audit trail system (includes read and administrative operations).
    /// </summary>
    public const string AuditManage = BaseName + "audittrail.manage";

    // ──── Client Permissions ────

    /// <summary>
    /// Permission to read OAuth client registrations and their configuration.
    /// </summary>
    public const string ClientRead = BaseName + "client.read";

    /// <summary>
    /// Permission to register new OAuth clients and update existing client configurations.
    /// </summary>
    public const string ClientWrite = BaseName + "client.write";

    /// <summary>
    /// Permission to delete OAuth client registrations.
    /// </summary>
    public const string ClientDelete = BaseName + "client.delete";

    /// <summary>
    /// Full management permission for OAuth clients (includes read, write, and delete).
    /// </summary>
    public const string ClientManage = BaseName + "client.manage";

    // ──── Security Token Permissions ────

    /// <summary>
    /// Full management permission for security tokens (view and revoke active access/refresh tokens).
    /// </summary>
    public const string SecurityTokenManage = BaseName + "securitytoken.manage";

    /// <summary>
    /// Permission to read/query active security tokens issued by the authorization server.
    /// </summary>
    public const string SecurityTokenRead = BaseName + "securitytoken.read";

    // ──── Notification Permissions ────

    /// <summary>
    /// Permission to read notification logs, templates, and provider configurations.
    /// </summary>
    public const string NotificationRead = BaseName + "notification.read";

    /// <summary>
    /// Full management permission for notifications (configure providers, manage templates, send test messages).
    /// </summary>
    public const string NotificationManage = BaseName + "notification.manage";

    // ──── External Auth Permissions ────

    /// <summary>
    /// Permission to read external authentication provider configurations (e.g., Google OIDC settings).
    /// </summary>
    public const string ExternalAuthRead = BaseName + "externalauth.read";

    /// <summary>
    /// Full management permission for external auth providers (configure, test, and delete provider integrations).
    /// </summary>
    public const string ExternalAuthManage = BaseName + "externalauth.manage";
}
