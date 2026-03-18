/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Microsoft.Extensions.DependencyInjection;
using HCL.CS.SF.Service.Interfaces.Interfaces.Api;

namespace HCL.CS.SF.ProxyService.Routes;

/// <summary>
/// Abstract base class that provides lazy-resolved access to all backend domain services
/// via the DI container. Route handler partial classes inherit from this base to access
/// services such as user accounts, roles, clients, API resources, identity resources,
/// audit trails, authentication, security tokens, notifications, and external auth management.
/// Services are resolved on first access and cached for the lifetime of the instance.
/// </summary>
internal abstract class BaseApiServiceInstance
{
    /// <summary>
    /// The DI service provider used to resolve backend service instances on demand.
    /// </summary>
    private readonly IServiceProvider serviceProvider;

    // Lazily-resolved service instance backing fields.
    private IApiResourceService apiResourceService;
    private IAuditTrailService auditTrailService;
    private IAuthenticationService authenticationService;
    private IClientServices clientServices;
    private IIdentityResourceService identityResourceService;
    private IRoleService roleService;
    private ISecurityTokenService securityTokenService;
    private IUserAccountService userAccountService;
    private INotificationManagementService notificationManagementService;
    private IExternalAuthManagementService externalAuthManagementService;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseApiServiceInstance"/> class.
    /// </summary>
    /// <param name="serviceProvider">The DI service provider.</param>
    protected BaseApiServiceInstance(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Gets the user account service for user registration, profile, claims, and role management.
    /// </summary>
    public IUserAccountService UserAccountService
    {
        get
        {
            if (userAccountService != null) return userAccountService;

            userAccountService = serviceProvider.GetService<IUserAccountService>();
            return userAccountService;
        }
    }

    /// <summary>
    /// Gets the API resource service for managing API resources, scopes, and their claims.
    /// </summary>
    public IApiResourceService ApiResourceService
    {
        get
        {
            if (apiResourceService != null) return apiResourceService;

            apiResourceService = serviceProvider.GetService<IApiResourceService>();
            return apiResourceService;
        }
    }

    /// <summary>
    /// Gets the identity resource service for managing OIDC identity resources and claims.
    /// </summary>
    public IIdentityResourceService IdentityResourceService
    {
        get
        {
            if (identityResourceService != null) return identityResourceService;

            identityResourceService = serviceProvider.GetService<IIdentityResourceService>();
            return identityResourceService;
        }
    }

    /// <summary>
    /// Gets the role service for managing roles and role claims.
    /// </summary>
    public IRoleService RoleService
    {
        get
        {
            if (roleService != null) return roleService;

            roleService = serviceProvider.GetService<IRoleService>();
            return roleService;
        }
    }

    /// <summary>
    /// Gets the audit trail service for recording and querying audit events.
    /// </summary>
    public IAuditTrailService AuditTrailService
    {
        get
        {
            if (auditTrailService != null) return auditTrailService;

            auditTrailService = serviceProvider.GetService<IAuditTrailService>();
            return auditTrailService;
        }
    }

    /// <summary>
    /// Gets the authentication service for sign-in, sign-out, and two-factor operations.
    /// </summary>
    public IAuthenticationService AuthenticationService
    {
        get
        {
            if (authenticationService != null) return authenticationService;

            authenticationService = serviceProvider.GetService<IAuthenticationService>();
            return authenticationService;
        }
    }

    /// <summary>
    /// Gets the client service for managing OAuth/OIDC client registrations.
    /// </summary>
    public IClientServices ClientServices
    {
        get
        {
            if (clientServices != null) return clientServices;

            clientServices = serviceProvider.GetService<IClientServices>();
            return clientServices;
        }
    }

    /// <summary>
    /// Gets the security token service for querying active and historical security tokens.
    /// </summary>
    public ISecurityTokenService SecurityTokenService
    {
        get
        {
            if (securityTokenService != null) return securityTokenService;

            securityTokenService = serviceProvider.GetService<ISecurityTokenService>();
            return securityTokenService;
        }
    }

    /// <summary>
    /// Gets the notification management service for managing notification providers, templates, and logs.
    /// </summary>
    public INotificationManagementService NotificationManagementService
    {
        get
        {
            if (notificationManagementService != null) return notificationManagementService;

            notificationManagementService = serviceProvider.GetService<INotificationManagementService>();
            return notificationManagementService;
        }
    }

    /// <summary>
    /// Gets the external auth management service for managing external identity provider configurations.
    /// </summary>
    public IExternalAuthManagementService ExternalAuthManagementService
    {
        get
        {
            if (externalAuthManagementService != null) return externalAuthManagementService;

            externalAuthManagementService = serviceProvider.GetService<IExternalAuthManagementService>();
            return externalAuthManagementService;
        }
    }
}
