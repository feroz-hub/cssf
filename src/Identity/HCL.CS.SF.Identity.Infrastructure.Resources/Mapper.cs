/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using AutoMapper;
using AutoMapper.EquivalencyExpression;
using AutoMapper.Internal;
using Microsoft.Extensions.Logging.Abstractions;
using HCL.CS.SF.Domain.Entities.Api;
using HCL.CS.SF.Domain.Entities.Endpoint;
using HCL.CS.SF.Domain.Models.Api;
using HCL.CS.SF.Domain.Models.Endpoint;
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;

namespace HCL.CS.SF.Infrastructure.Resources;

/// <summary>
/// Configures AutoMapper profiles for the Security Framework infrastructure layer.
/// Centralises all entity-to-model and model-to-entity mapping definitions across
/// audit, notification, authentication, user management, and authorization domains.
/// </summary>
public class Mapper
{
    /// <summary>
    /// Creates and returns a fully configured <see cref="MapperConfiguration"/> containing all
    /// mapping profiles required by the Security Framework.
    /// </summary>
    /// <returns>A <see cref="MapperConfiguration"/> instance ready for creating <see cref="IMapper"/> instances.</returns>
    public MapperConfiguration InitializeMapper()
    {
        // Optionally apply a commercial AutoMapper license key from the environment
        var licenseKey = Environment.GetEnvironmentVariable("HCL.CS.SF_AUTOMAPPER_LICENSE_KEY");
        var configuration = new MapperConfiguration(config =>
        {
            if (!string.IsNullOrWhiteSpace(licenseKey))
            {
                config.LicenseKey = licenseKey;
            }

            // .NET 8 introduces LINQ generic math helpers that can trigger
            // AutoMapper method-discovery reflection issues on startup.
            config.Internal().MethodMappingEnabled = false;
            config.ShouldMapMethod = _ => false;

            config.AddCollectionMappers();
            AuditMapper(config);
            NotificationMapper(config);
            AuthenticationMapper(config);
            UserManagementMapper(config);
            AuthorizationMapper(config);
        }, NullLoggerFactory.Instance);
        return configuration;
    }

    /// <summary>
    /// Registers mapping profiles for user management entities including users, security questions, and user claims.
    /// Uses equality comparison by <c>Id</c> for collection-aware update scenarios.
    /// </summary>
    /// <param name="config">The mapper configuration expression to register profiles against.</param>
    private static void UserManagementMapper(IMapperConfigurationExpression config)
    {
        config.CreateMap<Users, UserDisplayModel>();
        config.CreateMap<UserModel, Users>().EqualityComparison((src, dest) => src.Id == dest.Id);
        config.CreateMap<Users, UserModel>();
        config.CreateMap<SecurityQuestionModel, SecurityQuestions>()
            .EqualityComparison((src, dest) => src.Id == dest.Id);
        config.CreateMap<SecurityQuestions, SecurityQuestionModel>();
        config.CreateMap<UserSecurityQuestionModel, UserSecurityQuestions>()
            .EqualityComparison((src, dest) => src.Id == dest.Id);
        config.CreateMap<UserSecurityQuestions, UserSecurityQuestionModel>();
        config.CreateMap<UserClaimModel, UserClaims>().EqualityComparison((src, dest) => src.Id == dest.Id);
        config.CreateMap<UserClaims, UserClaimModel>();
    }

    /// <summary>
    /// Registers mapping profiles for authentication-related entities including OAuth clients,
    /// identity resources, API resources, API scopes, and security tokens.
    /// Handles space-delimited string-to-list conversions for scopes, grant types, and response types,
    /// as well as Unix timestamp-to-DateTime conversions for client timestamps.
    /// </summary>
    /// <param name="config">The mapper configuration expression to register profiles against.</param>
    private static void AuthenticationMapper(IMapperConfigurationExpression config)
    {
        config.CreateMap<ClientRedirectUrisModel, ClientRedirectUris>().ReverseMap();
        config.CreateMap<ClientPostLogoutRedirectUrisModel, ClientPostLogoutRedirectUris>().ReverseMap();

        // Map ClientsModel -> Clients: convert list fields to space-delimited strings
        // and DateTime fields to Unix timestamps for persistent storage
        config.CreateMap<ClientsModel, Clients>()
            .ForMember(dest => dest.ClientIdIssuedAt, opt => opt.MapFrom(src => src.ClientIdIssuedAt.ToUnixTime()))
            .ForMember(desc => desc.AllowedScopes,
                opt => opt.MapFrom(src => string.Join(" ", src.AllowedScopes.ToArray())))
            .ForMember(desc => desc.SupportedGrantTypes,
                opt => opt.MapFrom(src => string.Join(" ", src.SupportedGrantTypes.ToArray())))
            .ForMember(desc => desc.SupportedResponseTypes,
                opt => opt.MapFrom(src => string.Join(" ", src.SupportedResponseTypes.ToArray())))
            .ForMember(
                dest => dest.ClientSecretExpiresAt,
                opt => opt.MapFrom(src => src.ClientSecretExpiresAt.ToUnixTime()));

        // Map Clients -> ClientsModel: convert space-delimited strings back to lists
        // and Unix timestamps back to DateTime for domain model consumption
        config.CreateMap<Clients, ClientsModel>()
            .ForMember(dest => dest.ClientIdIssuedAt, opt => opt.MapFrom(src => src.ClientIdIssuedAt.ToDateTime()))
            .ForMember(desc => desc.AllowedScopes,
                opt => opt.MapFrom(src => src.AllowedScopes.Split(' ', StringSplitOptions.None).ToList()))
            .ForMember(desc => desc.SupportedGrantTypes,
                opt => opt.MapFrom(src => src.SupportedGrantTypes.Split(' ', StringSplitOptions.None).ToList()))
            .ForMember(desc => desc.SupportedResponseTypes,
                opt => opt.MapFrom(src => src.SupportedResponseTypes.Split(' ', StringSplitOptions.None).ToList()))
            .ForMember(
                dest => dest.ClientSecretExpiresAt,
                opt => opt.MapFrom(src => src.ClientSecretExpiresAt.ToDateTime()));

        // Identity resources and their associated claims
        config.CreateMap<IdentityResourcesModel, IdentityResources>()
            .EqualityComparison((src, dest) => src.Id == dest.Id);
        config.CreateMap<IdentityClaimsModel, IdentityClaims>().EqualityComparison((src, dest) => src.Id == dest.Id);
        config.CreateMap<IdentityResources, IdentityResourcesModel>();
        config.CreateMap<IdentityClaims, IdentityClaimsModel>();

        // API resources, resource claims, scopes, and scope claims
        // UseDestinationValue for RowVersion prevents overwriting the concurrency token during updates
        config.CreateMap<ApiResourcesModel, ApiResources>()
            .ForMember(dest => dest.RowVersion, opt => opt.UseDestinationValue())
            .EqualityComparison((src, dest) => src.Id == dest.Id);
        config.CreateMap<ApiResourceClaimsModel, ApiResourceClaims>()
            .ForMember(dest => dest.RowVersion, opt => opt.UseDestinationValue())
            .EqualityComparison((src, dest) => src.Id == dest.Id);
        config.CreateMap<ApiScopesModel, ApiScopes>()
            .ForMember(dest => dest.RowVersion, opt => opt.UseDestinationValue())
            .EqualityComparison((src, dest) => src.Id == dest.Id);
        config.CreateMap<ApiScopeClaimsModel, ApiScopeClaims>()
            .ForMember(dest => dest.RowVersion, opt => opt.UseDestinationValue())
            .EqualityComparison((src, dest) => src.Id == dest.Id);

        config.CreateMap<ApiResources, ApiResourcesModel>();
        config.CreateMap<ApiResourceClaims, ApiResourceClaimsModel>();
        config.CreateMap<ApiScopes, ApiScopesModel>();
        config.CreateMap<ApiScopeClaims, ApiScopeClaimsModel>();
        config.CreateMap<SecurityTokens, SecurityTokensModel>().ReverseMap();
    }

    /// <summary>
    /// Registers mapping profiles for authorization entities including roles, role claims, and user-role associations.
    /// </summary>
    /// <param name="config">The mapper configuration expression to register profiles against.</param>
    private static void AuthorizationMapper(IMapperConfigurationExpression config)
    {
        config.CreateMap<RoleModel, Roles>().EqualityComparison((src, dest) => src.Id == dest.Id);
        config.CreateMap<RoleClaimModel, RoleClaims>().EqualityComparison((src, dest) => src.Id == dest.Id);
        config.CreateMap<Roles, RoleModel>();
        config.CreateMap<RoleClaims, RoleClaimModel>();
        config.CreateMap<UserRoleModel, UserRoles>().ReverseMap();
    }

    /// <summary>
    /// Registers mapping profiles for audit trail entities, converting the <c>ActionType</c> enum to its string representation.
    /// </summary>
    /// <param name="config">The mapper configuration expression to register profiles against.</param>
    private static void AuditMapper(IMapperConfigurationExpression config)
    {
        config.CreateMap<AuditTrailModel, AuditTrail>()
            .ForMember(dest => dest.ActionType, opt => opt.MapFrom(src => src.ActionType.ToString())).ReverseMap();
    }

    /// <summary>
    /// Registers mapping profiles for notification entities including email and SMS messages.
    /// Maps notification info to channel-specific message models and then to persistent <see cref="Notification"/> entities.
    /// </summary>
    /// <param name="config">The mapper configuration expression to register profiles against.</param>
    private static void NotificationMapper(IMapperConfigurationExpression config)
    {
        config.CreateMap<NotificationInfoModel, EmailMessageModel>();
        config.CreateMap<NotificationInfoModel, SMSMessage>()
            .ForMember(dest => dest.To, opt => opt.MapFrom(src => src.ToAddress));
        config.CreateMap<EmailMessageModel, Notification>()
            .ForMember(dest => dest.Recipient, opt => opt.MapFrom(src => src.ToAddress))
            .ForMember(dest => dest.Sender, opt => opt.MapFrom(src => src.FromAddress))
            .ForMember(dest => dest.Activity, opt => opt.MapFrom(src => src.Activity));

        config.CreateMap<SMSMessage, Notification>()
            .ForMember(dest => dest.Recipient, opt => opt.MapFrom(src => src.To))
            .ForMember(dest => dest.Activity, opt => opt.MapFrom(src => src.Activity));
    }
}
