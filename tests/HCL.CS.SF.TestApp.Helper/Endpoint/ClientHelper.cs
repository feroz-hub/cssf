/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain.Entities.Endpoint;
using HCL.CS.SF.Domain.Enums;
using HCL.CS.SF.Domain.Models.Api;
using HCL.CS.SF.Domain.Models.Endpoint;
using HCL.CS.SF.Domain.Models.Endpoint.Validation;
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;
using static HCL.CS.SF.Domain.Constants.Endpoint.OpenIdConstants;

namespace TestApp.Helper.Endpoint;

public static class ClientHelper
{
    public static ClientsModel CreateClientsModel()
    {
        var clientModel = new ClientsModel
        {
            ClientId = "e2AK48mjbwaXpXjeL0QPJ44Dm0QOrr8ubs5piIH2ckY=",
            ClientName = "HCL.CS.SF Client 001",
            ClientUri = "http://identity.io",
            ClientIdIssuedAt = DateTime.Now,
            ClientSecret = "fZJnyNuhdcsw94ULApd7mHW8VimdTVMeY36nYrWCg3U=",
            ClientSecretExpiresAt = DateTime.Now.AddDays(10),
            LogoUri = "https://localhost:44300/logo.png",
            TermsOfServiceUri = "https://localhost:44300/Tos.cshtml",
            PolicyUri = "https://localhost:44300/Policy.cshtml",

            RefreshTokenExpiration = 1800,
            AccessTokenExpiration = 1800,
            IdentityTokenExpiration = 1800,
            AuthorizationCodeExpiration = 1800,

            AccessTokenType = 0,
            RequirePkce = true,
            IsPkceTextPlain = true,
            RequireClientSecret = true,
            IsFirstPartyApp = false,

            RedirectUris = new List<ClientRedirectUrisModel>
            {
                new() { RedirectUri = "https://localhost:44300/index.html" },
                new() { RedirectUri = "https://localhost:44300/callback.html" }
            },

            PostLogoutRedirectUris = new List<ClientPostLogoutRedirectUrisModel>
            {
                new() { PostLogoutRedirectUri = "https://localhost:44300/index.html" },
                new() { PostLogoutRedirectUri = "https://localhost:44300/callback.html" }
            },

            SupportedGrantTypes = new List<string>
                { "authorization_code", "client_credentials", "password", "refresh_token", "hybrid" },

            // AllowedScopes = "openid clientapi offline_access",

            AllowedSigningAlgorithm = Algorithms.HmacSha512,

            AllowOfflineAccess = true,

            ApplicationType = ApplicationType.SinglePageApp,
            AllowedScopes = new List<string>
            {
                "HCL.CS.SF.client"
            },
            SupportedResponseTypes = new List<string> { "code token id_token" }
        };

        return clientModel;
    }

    public static List<ClientsModel> CreateClientsModelMasterList()
    {
        var clientsModelsList = new List<ClientsModel>
        {
            new()
            {
                ClientId = "e903d2df91f2a1d1ee0e691e5dcbc454=",
                ClientName = "HCL.CS.SF Client 001",
                ClientUri = "http:/identity.io",
                ClientIdIssuedAt = DateTime.Now,
                // ClientSecret = "abcdefghijklmnopqrstuvwxyz.,",
                ClientSecretExpiresAt = DateTime.Now.AddDays(10),
                LogoUri = "https://localhost:44300/logo.png",
                TermsOfServiceUri = "https://localhost:44300/Tos.cshtml",
                PolicyUri = "https://localhost:44300/Policy.cshtml",

                RefreshTokenExpiration = 1800,
                AccessTokenExpiration = 1800,
                IdentityTokenExpiration = 1800,
                AuthorizationCodeExpiration = 1800,

                AccessTokenType = 0,
                RequirePkce = true,
                IsPkceTextPlain = true,
                RequireClientSecret = true,
                IsFirstPartyApp = false,

                RedirectUris = new List<ClientRedirectUrisModel>
                {
                    new() { RedirectUri = "https://localhost:44300/index.html" },
                    new() { RedirectUri = "https://localhost:44300/callback.html" }
                },

                PostLogoutRedirectUris = new List<ClientPostLogoutRedirectUrisModel>
                {
                    new() { PostLogoutRedirectUri = "https://localhost:44300/index.html" },
                    new() { PostLogoutRedirectUri = "https://localhost:44300/callback.html" }
                },

                SupportedGrantTypes = new List<string>
                    { "authorization_code", "client_credentials", "password", "refresh_token", "hybrid" },

                // AllowedScopes = "openid clientapi offline_access",

                AllowedSigningAlgorithm = Algorithms.HmacSha512,

                AllowOfflineAccess = true,

                ApplicationType = ApplicationType.SinglePageApp,
                AllowedScopes = new List<string>
                {
                    "openid email profile clientapi offline_access"
                },
                SupportedResponseTypes = new List<string> { "code token id_token" }
            },
            new()
            {
                ClientId = "3b9605ee7b4d66f3759a36fd9b3ae126=",
                ClientName = "HCL.CS.SF Client 002",
                ClientUri = "http:/identity.io",
                ClientIdIssuedAt = DateTime.Now,
                // ClientSecret = "abcdefghijklmnopqrstuvwxyz.,",
                ClientSecretExpiresAt = DateTime.Now.AddDays(10),
                LogoUri = "https://localhost:5000/logo.png",
                TermsOfServiceUri = "https://localhost:5000/Tos.cshtml",
                PolicyUri = "https://localhost:5000/Policy.cshtml",

                RefreshTokenExpiration = 1800,
                AccessTokenExpiration = 1800,
                IdentityTokenExpiration = 1800,
                AuthorizationCodeExpiration = 1800,

                AccessTokenType = 0,
                RequirePkce = true,
                IsPkceTextPlain = true,
                RequireClientSecret = true,
                IsFirstPartyApp = false,

                RedirectUris = new List<ClientRedirectUrisModel>
                {
                    new() { RedirectUri = "https://localhost:44300/index.html" },
                    new() { RedirectUri = "https://localhost:44300/callback.html" }
                },

                PostLogoutRedirectUris = new List<ClientPostLogoutRedirectUrisModel>
                {
                    new() { PostLogoutRedirectUri = "https://localhost:44300/index.html" },
                    new() { PostLogoutRedirectUri = "https://localhost:44300/callback.html" }
                },

                SupportedGrantTypes = new List<string>
                    { "authorization_code", "client_credentials", "password", "refresh_token", "hybrid" },

                // AllowedScopes = "openid clientapi offline_access",

                AllowedSigningAlgorithm = Algorithms.HmacSha512,

                AllowOfflineAccess = true,

                ApplicationType = ApplicationType.RegularWeb,
                AllowedScopes = new List<string>
                {
                    "openid email profile clientapi offline_access"
                },
                SupportedResponseTypes = new List<string> { "code token id_token" }
            },
            new()
            {
                ClientId = "e2AK48mjbwaXpXjeL0QPJ44Dm0QOrr8ubs5piIH2ckY=",
                ClientName = "HCL.CS.SF Client 003",
                ClientUri = "http:/identity.io",
                ClientIdIssuedAt = DateTime.Now,
                // ClientSecret = "abcdefghijklmnopqrstuvwxyz.,",
                ClientSecretExpiresAt = DateTime.Now.AddDays(10),
                LogoUri = "https://localhost:50000/logo.png",
                TermsOfServiceUri = "https://localhost:50000/Tos.cshtml",
                PolicyUri = "https://localhost:50000/Policy.cshtml",

                RefreshTokenExpiration = 1800,
                AccessTokenExpiration = 1800,
                IdentityTokenExpiration = 1800,
                AuthorizationCodeExpiration = 1800,

                AccessTokenType = 0,
                RequirePkce = true,
                IsPkceTextPlain = true,
                RequireClientSecret = true,
                IsFirstPartyApp = false,

                RedirectUris = new List<ClientRedirectUrisModel>
                {
                    new() { RedirectUri = "https://localhost:50000/index.html" },
                    new() { RedirectUri = "https://localhost:50000/callback.html" }
                },

                PostLogoutRedirectUris = new List<ClientPostLogoutRedirectUrisModel>
                {
                    new() { PostLogoutRedirectUri = "https://localhost:50000/index.html" },
                    new() { PostLogoutRedirectUri = "https://localhost:50000/callback.html" }
                },

                SupportedGrantTypes = new List<string> { "authorization_code", "hybrid" },

                // AllowedScopes = "openid clientapi offline_access",

                AllowedSigningAlgorithm = Algorithms.HmacSha512,

                AllowOfflineAccess = true,

                ApplicationType = ApplicationType.Native,
                AllowedScopes = new List<string>
                {
                    "openid offline_access"
                },
                SupportedResponseTypes = new List<string> { "code token id_token" }
            }
        };
        return clientsModelsList;
    }

    public static ClientsModel UpdateClientsModel()
    {
        var clientModel = new ClientsModel
        {
            ClientId = "cc4d195a99ffa97f9a6027892cb9f198",
            ClientName = "HCL.CS.SF Client Update",
            ClientUri = "http://dentity.io",
            ClientSecretExpiresAt = DateTime.UtcNow.AddDays(15),
            ClientSecret = "KRIJqfnv18xfqcBs1cuNj+zfs2RW5YWeJ42mxKy7fj4=",
            LogoUri = "https://localhost:44300/logo.png",
            TermsOfServiceUri = "https://localhost:44300/Tos.cshtml",
            PolicyUri = "https://localhost:44300/Policy.cshtml",

            RefreshTokenExpiration = 1800,
            AccessTokenExpiration = 1800,
            IdentityTokenExpiration = 1800,
            AuthorizationCodeExpiration = 1800,

            AccessTokenType = 0,
            RequirePkce = false,
            IsPkceTextPlain = false,
            RequireClientSecret = true,
            IsFirstPartyApp = false,

            RedirectUris = new List<ClientRedirectUrisModel>
            {
                new() { ClientId = Guid.NewGuid(), RedirectUri = "https://localhost:44400/index.html" },
                new() { ClientId = Guid.NewGuid(), RedirectUri = "https://localhost:44400/callback.html" }
            },

            PostLogoutRedirectUris = new List<ClientPostLogoutRedirectUrisModel>
            {
                new() { ClientId = Guid.NewGuid(), PostLogoutRedirectUri = "https://localhost:44300/index.html" },
                new() { ClientId = Guid.NewGuid(), PostLogoutRedirectUri = "https://localhost:44300/callback.html" }
            },

            // SupportedGrantTypes = "authorization_code client_credentials password",
            // AllowedScopes = "openid email profile clientapi offline_access",
            // SupportedResponseTypes = "code token id_token",
            AllowedSigningAlgorithm = Algorithms.HmacSha384,

            ApplicationType = ApplicationType.Native,
            AllowedScopes = new List<string>
            {
                "openid email profile clientapi offline_access"
            },
            SupportedResponseTypes = new List<string> { "code token id_token" },
            SupportedGrantTypes = new List<string>
                { "authorization_code", "client_credentials", "password", "refresh_token", "hybrid" }
        };

        return clientModel;
    }

    public static ClientSecretValidationModel GetClientSecretValidationModel()
    {
        var secretValidationModel = new ClientSecretValidationModel();
        return secretValidationModel;
    }

    public static RoleClaimModel CreateRoleClaimModel()
    {
        var roleClaimModel = new RoleClaimModel();
        roleClaimModel.ClaimType = "Permission";
        roleClaimModel.ClaimValue = "ClientRegistration.Add";
        return roleClaimModel;
    }

    public static Clients GetClients()
    {
        return new Clients
        {
            ClientId = "test.Client.1",
            ClientName = "HCL.CS.SF Client 001",
            ClientUri = "http:/identity.io",
            ClientSecret = "abcdefghijklmnopqrstuvwxyz.,",
            ClientIdIssuedAt = DateTime.UtcNow.ToUnixTime(),
            ClientSecretExpiresAt = DateTime.Now.AddDays(180).ToUnixTime(),
            LogoUri = "https://localhost:5002/logo.png",
            TermsOfServiceUri = "https://localhost:5002/Tos.cshtml",
            PolicyUri = "https://localhost:5002/Policy.cshtml",

            RefreshTokenExpiration = 2592000,
            AccessTokenExpiration = 3600,
            IdentityTokenExpiration = 300,
            AuthorizationCodeExpiration = 300,

            AccessTokenType = 0,
            RequirePkce = true,
            //AllowPlainTextPkce = true,
            RequireClientSecret = true,
            IsFirstPartyApp = false,
            AllowAccessTokensViaBrowser = true,
            CreatedBy = "Test",
            RedirectUris = new List<ClientRedirectUris>
            {
                new() { RedirectUri = "https://localhost:5002/index.html", CreatedBy = "Test" },
                new() { RedirectUri = "https://localhost:5002/callback.html", CreatedBy = "Test" },
                new() { RedirectUri = "https://localhost:5002/signin-oidc", CreatedBy = "Test" },
                new() { RedirectUri = "https://localhost:5003/signin-oidc", CreatedBy = "Test" }
            },

            PostLogoutRedirectUris = new List<ClientPostLogoutRedirectUris>
            {
                new() { PostLogoutRedirectUri = "https://localhost:5002/callback.html", CreatedBy = "Test" },
                new() { PostLogoutRedirectUri = "https://localhost:5003/callback.html", CreatedBy = "Test" },
                new() { PostLogoutRedirectUri = "https://localhost:5002/signout-oidc", CreatedBy = "Test" },
                new() { PostLogoutRedirectUri = "https://localhost:5003/signout-oidc", CreatedBy = "Test" },
                new() { PostLogoutRedirectUri = "https://localhost:5002/signout-callback-oidc", CreatedBy = "Test" },
                new() { PostLogoutRedirectUri = "https://localhost:5003/signout-callback-oidc", CreatedBy = "Test" }
            },
            SupportedGrantTypes = "authorization_code client_credentials password refresh_token",

            AllowedScopes = "openid email profile clientapi offline_access",

            SupportedResponseTypes = "code id_token token",

            AllowedSigningAlgorithm = Algorithms.RsaSha256,

            AllowOfflineAccess = true,

            ApplicationType = ApplicationType.RegularWeb,

            FrontChannelLogoutUri = "https://localhost:5002/signout-oidc"
        };
    }

    public static List<Clients> GetClientsList()
    {
        var clientsList = new List<Clients>
        {
            new()
            {
                ClientId = "51478b9cb68b36657f3075f7f34cf491",
                ClientName = "HCL.CS.SF Client 001",
                ClientUri = "http:/identity.io",
                ClientSecret = "KRIJqfnv18xfqcBs1cuNj+zfs2RW5YWeJ42mxKy7fj4=",
                ClientIdIssuedAt = DateTime.UtcNow.ToUnixTime(),
                ClientSecretExpiresAt = DateTime.Now.AddDays(180).ToUnixTime(),
                LogoUri = "https://localhost:5002/logo.png",
                TermsOfServiceUri = "https://localhost:5002/Tos.cshtml",
                PolicyUri = "https://localhost:5002/Policy.cshtml",

                RefreshTokenExpiration = 2592000,
                AccessTokenExpiration = 3600,
                IdentityTokenExpiration = 300,
                AuthorizationCodeExpiration = 300,
                AccessTokenType = 0,
                RequirePkce = true,
                //AllowPlainTextPkce = true,
                RequireClientSecret = true,
                IsFirstPartyApp = false,
                AllowAccessTokensViaBrowser = true,
                CreatedBy = "Test",
                RedirectUris = new List<ClientRedirectUris>
                {
                    new() { RedirectUri = "https://localhost:5002/index.html", CreatedBy = "Test" },
                    new() { RedirectUri = "https://localhost:5002/callback.html", CreatedBy = "Test" },
                    new() { RedirectUri = "https://localhost:5002/signin-oidc", CreatedBy = "Test" },
                    new() { RedirectUri = "https://localhost:5003/signin-oidc", CreatedBy = "Test" }
                },

                PostLogoutRedirectUris = new List<ClientPostLogoutRedirectUris>
                {
                    new() { PostLogoutRedirectUri = "https://localhost:5002/callback.html", CreatedBy = "Test" },
                    new() { PostLogoutRedirectUri = "https://localhost:5003/callback.html", CreatedBy = "Test" },
                    new() { PostLogoutRedirectUri = "https://localhost:5002/signout-oidc", CreatedBy = "Test" },
                    new() { PostLogoutRedirectUri = "https://localhost:5003/signout-oidc", CreatedBy = "Test" },
                    new()
                    {
                        PostLogoutRedirectUri = "https://localhost:5002/signout-callback-oidc", CreatedBy = "Test"
                    },
                    new() { PostLogoutRedirectUri = "https://localhost:5003/signout-callback-oidc", CreatedBy = "Test" }
                },
                SupportedGrantTypes = "authorization_code client_credentials password refresh_token",

                AllowedScopes = "openid email profile clientapi offline_access",

                SupportedResponseTypes = "code id_token token",

                AllowedSigningAlgorithm = Algorithms.RsaSha256,

                AllowOfflineAccess = true,

                ApplicationType = ApplicationType.RegularWeb,

                FrontChannelLogoutUri = "https://localhost:5002/signout-oidc"
            },
            new()
            {
                ClientId = "585837bdfadbe0f455b79ff30f2570f4",
                ClientName = "HCL.CS.SF Client 001",
                ClientUri = "http:/identity.io",
                ClientSecret = "/zZem5fv7lLgu3oBs61fGH4f/krz9kZFEL5iTmmwGzs=",
                ClientIdIssuedAt = DateTime.UtcNow.ToUnixTime(),
                ClientSecretExpiresAt = DateTime.Now.AddDays(180).ToUnixTime(),
                LogoUri = "https://localhost:4000/logo.png",
                TermsOfServiceUri = "https://localhost:4000/Tos.cshtml",
                PolicyUri = "https://localhost:4000/Policy.cshtml",

                RefreshTokenExpiration = 2592000,
                AccessTokenExpiration = 3600,
                IdentityTokenExpiration = 300,
                AuthorizationCodeExpiration = 300,

                AccessTokenType = 0,
                RequirePkce = true,
                //AllowPlainTextPkce = true,
                RequireClientSecret = true,
                IsFirstPartyApp = false,
                AllowAccessTokensViaBrowser = true,
                CreatedBy = "Test",
                RedirectUris = new List<ClientRedirectUris>
                {
                    new() { RedirectUri = "https://localhost:4000/index.html", CreatedBy = "Test" },
                    new() { RedirectUri = "https://localhost:4000/callback.html", CreatedBy = "Test" }
                },

                PostLogoutRedirectUris = new List<ClientPostLogoutRedirectUris>
                {
                    new() { PostLogoutRedirectUri = "https://localhost:4000/callback.html", CreatedBy = "Test" },
                    new() { PostLogoutRedirectUri = "https://localhost:5003/callback.html", CreatedBy = "Test" },
                    new() { PostLogoutRedirectUri = "https://localhost:5003/signout-oidc", CreatedBy = "Test" },
                    new() { PostLogoutRedirectUri = "https://localhost:4000/signout-callback-oidc", CreatedBy = "Test" }
                },
                SupportedGrantTypes = "authorization_code client_credentials",

                AllowedScopes = "openid clientapi offline_access",

                SupportedResponseTypes = "code id_token token",

                AllowedSigningAlgorithm = Algorithms.RsaSha256,

                AllowOfflineAccess = true,

                ApplicationType = ApplicationType.Native,

                FrontChannelLogoutUri = "https://localhost:5002/signout-oidc"
            }
        };
        return clientsList;
    }

    public static List<ClientRedirectUris> GetClientRedirectUris()
    {
        return new List<ClientRedirectUris>
        {
            new() { RedirectUri = "https://localhost:5002/index.html", CreatedBy = "Test" },
            new() { RedirectUri = "https://localhost:5002/callback.html", CreatedBy = "Test" }
        };
    }

    public static List<ClientPostLogoutRedirectUris> GetClientPostLogoutRedirectUris()
    {
        return new List<ClientPostLogoutRedirectUris>
        {
            new() { PostLogoutRedirectUri = "https://localhost:5002/callback.html", CreatedBy = "Test" },
            new() { PostLogoutRedirectUri = "https://localhost:5003/callback.html", CreatedBy = "Test" }
        };
    }

    public static IList<SecurityTokens> GetSecurityTokenList()
    {
        var clientsList = new List<SecurityTokens>
        {
            new()
            {
                ClientId = "Test",
                Key = "Test"
            }
        };
        return clientsList;
    }
}
