/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain.Enums;
using HCL.CS.SF.Domain.Models.Api;
using HCL.CS.SF.Domain.Models.Endpoint;
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;
using static HCL.CS.SF.Domain.Constants.Endpoint.OpenIdConstants;

namespace IntegrationTests.DataSeed;

public static class IntegrationDataSeed
{
    public static List<SecurityQuestionModel> CreateSecurityQuestionModelMaster()
    {
        var questionsModelList = new List<SecurityQuestionModel>
        {
            new() { Question = "What is your age?", CreatedBy = "Test", CreatedOn = DateTime.UtcNow },
            new() { Question = "What is your first phonenumber ?", CreatedBy = "Test", CreatedOn = DateTime.UtcNow },
            new() { Question = "What is your last phonenumber?", CreatedBy = "Test", CreatedOn = DateTime.UtcNow },
            new() { Question = "What is your office Name?", CreatedBy = "Test", CreatedOn = DateTime.UtcNow }
        };
        return questionsModelList;
    }

    public static List<UserModel> CreateUserModelMaster()
    {
        var modelList = new List<UserClaimModel>();
        modelList.Add(new UserClaimModel { ClaimType = "Street", ClaimValue = "11 Test street", CreatedBy = "Kamal" });
        modelList.Add(new UserClaimModel { ClaimType = "City", ClaimValue = "Chennai", CreatedBy = "Kamal" });
        modelList.Add(new UserClaimModel { ClaimType = "Pincode", ClaimValue = "635002", CreatedBy = "Kamal" });
        var userModelList = new List<UserModel>
        {
            new()
            {
                UserName = "BobUser",
                Email = "BobUser@gmail.com",
                PhoneNumber = "+91234928347",
                TwoFactorEnabled = false,
                Password = "Test@123456789",
                FirstName = "Jack",
                LastName = "Ryan",
                DateOfBirth = new DateTime(1989, 5, 1),
                RequiresDefaultPasswordChange = false,
                CreatedBy = "JR",
                ModifiedBy = "JR",
                IdentityProviderType = IdentityProvider.Local,
                UserSecurityQuestion = new List<UserSecurityQuestionModel>(),
                UserClaims = modelList,
                PhoneNumberConfirmed = true,
                EmailConfirmed = true
            },

            new()
            {
                UserName = "JacobIsmail",
                Email = "JacobIsmail@gmail.com",
                PhoneNumber = "+91234928347",
                TwoFactorEnabled = false,
                Password = "Test@123456789",
                FirstName = "Jacob",
                LastName = "Ismail",
                DateOfBirth = new DateTime(1989, 5, 1),
                RequiresDefaultPasswordChange = false,
                CreatedBy = "JR",
                ModifiedBy = "JR",
                IdentityProviderType = IdentityProvider.Local,
                UserSecurityQuestion = new List<UserSecurityQuestionModel>(),
                UserClaims = modelList,
                PhoneNumberConfirmed = true,
                EmailConfirmed = true
            },

            new()
            {
                UserName = "BobAlice",
                Email = "BobAlice@gmail.com",
                PhoneNumber = "+919912312345",
                TwoFactorEnabled = false,
                Password = "Test@123",
                FirstName = "Bob",
                LastName = "Alice",
                DateOfBirth = new DateTime(1990, 1, 1),
                RequiresDefaultPasswordChange = false,
                CreatedBy = "BobUser",
                ModifiedBy = "BobUser",
                IdentityProviderType = IdentityProvider.Local,
                UserSecurityQuestion = new List<UserSecurityQuestionModel>(),
                UserClaims = modelList,
                PhoneNumberConfirmed = true,
                EmailConfirmed = true
            }
        };
        return userModelList;
    }

    public static UserSecurityQuestionModel CreateUserSecurityQuestionModelMaster()
    {
        var questionModel = new UserSecurityQuestionModel
        {
            Answer = "TestAnswer",
            CreatedBy = "Test",
            CreatedOn = DateTime.UtcNow
        };
        return questionModel;
    }

    public static List<RoleModel> CreateRoleModelMaster()
    {
        var roleModelList = new List<RoleModel>
        {
            new()
            {
                Description = "To Perform general view and read of role and ",
                Name = "GeneralUser",
                CreatedBy = "Suresh"
            },

            new()
            {
                Description = "To perform system related settings and configuration",
                Name = "SystemAdmin",
                CreatedBy = "Suresh"
            },

            new()
            {
                Description = "To perform client management",
                Name = "ClientAdmin",
                CreatedBy = "Suresh"
            },

            new()
            {
                Description = "To perform Role management",
                Name = "RoleAdmin",
                CreatedBy = "Suresh"
            },

            new()
            {
                Description = "To Perform general view and read of role and ",
                Name = "ResourceAdmin",
                CreatedBy = "Suresh"
            },

            new()
            {
                Description = "To Perform Guest Operations ",
                Name = "Guest",
                CreatedBy = "Suresh"
            }
        };
        return roleModelList;
    }

    public static List<RoleClaimModel> CreateRoleClaimModel_RoleAdmin()
    {
        var roleClaimModelList = new List<RoleClaimModel>
        {
            new()
            {
                ClaimType = "permission", ClaimValue = "HCL.CS.SF.Role.Read", CreatedBy = "Test",
                CreatedOn = DateTime.UtcNow
            },
            new()
            {
                ClaimType = "permission", ClaimValue = "HCL.CS.SF.Role.Write", CreatedBy = "Test",
                CreatedOn = DateTime.UtcNow
            },
            new()
            {
                ClaimType = "permission", ClaimValue = "HCL.CS.SF.Role.Delete", CreatedBy = "Test",
                CreatedOn = DateTime.UtcNow
            },
            new()
            {
                ClaimType = "permission", ClaimValue = "HCL.CS.SF.Role.Manage", CreatedBy = "Test",
                CreatedOn = DateTime.UtcNow
            }
        };
        return roleClaimModelList;
    }

    public static List<RoleClaimModel> CreateRoleClaimModel_SystemAdmin()
    {
        var roleClaimModelList = new List<RoleClaimModel>
        {
            new()
            {
                ClaimType = "permission", ClaimValue = "HCL.CS.SF.Admin.Manage", CreatedBy = "Test",
                CreatedOn = DateTime.UtcNow
            }
        };
        return roleClaimModelList;
    }

    public static List<RoleClaimModel> CreateRoleClaimModel_ClientAdmin()
    {
        var roleClaimModelList = new List<RoleClaimModel>
        {
            new()
            {
                ClaimType = "permission", ClaimValue = "HCL.CS.SF.Cleint.Read", CreatedBy = "Test",
                CreatedOn = DateTime.UtcNow
            },
            new()
            {
                ClaimType = "permission", ClaimValue = "HCL.CS.SF.Cleint.Write", CreatedBy = "Test",
                CreatedOn = DateTime.UtcNow
            },
            new()
            {
                ClaimType = "permission", ClaimValue = "HCL.CS.SF.Cleint.Delete", CreatedBy = "Test",
                CreatedOn = DateTime.UtcNow
            },
            new()
            {
                ClaimType = "permission", ClaimValue = "HCL.CS.SF.Cleint.Manage", CreatedBy = "Test",
                CreatedOn = DateTime.UtcNow
            }
        };
        return roleClaimModelList;
    }

    public static List<RoleClaimModel> CreateRoleClaimModel_GeneralUser()
    {
        var roleClaimModelList = new List<RoleClaimModel>
        {
            new()
            {
                ClaimType = "permission", ClaimValue = "HCL.CS.SF.User.Read", CreatedBy = "Test",
                CreatedOn = DateTime.UtcNow
            },
            new()
            {
                ClaimType = "permission", ClaimValue = "HCL.CS.SF.User.Write", CreatedBy = "Test",
                CreatedOn = DateTime.UtcNow
            },
            new()
            {
                ClaimType = "permission", ClaimValue = "HCL.CS.SF.User.Delete", CreatedBy = "Test",
                CreatedOn = DateTime.UtcNow
            },
            new()
            {
                ClaimType = "permission", ClaimValue = "HCL.CS.SF.User.Manage", CreatedBy = "Test",
                CreatedOn = DateTime.UtcNow
            }
        };
        return roleClaimModelList;
    }

    public static List<RoleClaimModel> CreateRoleClaimModel_ResourceAdmin()
    {
        var roleClaimModelList = new List<RoleClaimModel>
        {
            new()
            {
                ClaimType = "permission", ClaimValue = "HCL.CS.SF.ApiResource.Read", CreatedBy = "Test",
                CreatedOn = DateTime.UtcNow
            },
            new()
            {
                ClaimType = "permission", ClaimValue = "HCL.CS.SF.ApiResource.Write", CreatedBy = "Test",
                CreatedOn = DateTime.UtcNow
            },
            new()
            {
                ClaimType = "permission", ClaimValue = "HCL.CS.SF.ApiResource.Delete", CreatedBy = "Test",
                CreatedOn = DateTime.UtcNow
            },
            new()
            {
                ClaimType = "permission", ClaimValue = "HCL.CS.SF.ApiResource.Manage", CreatedBy = "Test",
                CreatedOn = DateTime.UtcNow
            },

            new()
            {
                ClaimType = "permission", ClaimValue = "HCL.CS.SF.IdentityResource.Read", CreatedBy = "Test",
                CreatedOn = DateTime.UtcNow
            },
            new()
            {
                ClaimType = "permission", ClaimValue = "HCL.CS.SF.IdentityResource.Write", CreatedBy = "Test",
                CreatedOn = DateTime.UtcNow
            },
            new()
            {
                ClaimType = "permission", ClaimValue = "HCL.CS.SF.IdentityResource.Delete", CreatedBy = "Test",
                CreatedOn = DateTime.UtcNow
            },
            new()
            {
                ClaimType = "permission", ClaimValue = "HCL.CS.SF.IdentityResource.Manage", CreatedBy = "Test",
                CreatedOn = DateTime.UtcNow
            }
        };
        return roleClaimModelList;
    }

    public static List<IdentityResourcesModel> CreateIdentityResourceModelMaster()
    {
        var identityResources = new List<IdentityResourcesModel>();
        var identityResource1 = new IdentityResourcesModel
        {
            Name = "OpenId",
            DisplayName = "Your user identifier",
            Description = "Your user identifier",
            Enabled = true,
            Required = true,
            CreatedBy = "Test",
            IdentityClaims = new List<IdentityClaimsModel>
            {
                new() { Type = "subject", AliasType = "sub", CreatedBy = "Test" }
            }
        };
        var identityResource2 = new IdentityResourcesModel
        {
            Name = "Profile",
            DisplayName = "User profile",
            Description = "Your user profile information (first name, last name, etc.)",
            Enabled = true,
            Emphasize = true,
            CreatedBy = "Test",
            IdentityClaims = new List<IdentityClaimsModel>
            {
                new() { Type = "name", CreatedBy = "Test", AliasType = "username" },
                new() { Type = "family_name", CreatedBy = "Test", AliasType = "lastname" },
                new() { Type = "given_name", CreatedBy = "Test", AliasType = "firstname" },
                new() { Type = "middle_name", CreatedBy = "Test" },
                new() { Type = "nickname", CreatedBy = "Test" },
                new() { Type = "preferred_username", CreatedBy = "Test" },
                new() { Type = "profile", CreatedBy = "Test" },
                new() { Type = "picture", CreatedBy = "Test" },
                new() { Type = "website", CreatedBy = "Test" },
                new() { Type = "Gender", CreatedBy = "Test" },
                new() { Type = "birthdate", CreatedBy = "Test", AliasType = "dateofbirth" },
                new() { Type = "zoneinfo", CreatedBy = "Test" },
                new() { Type = "locale", CreatedBy = "Test" },
                new() { Type = "updated_at", CreatedBy = "Test" },
                new() { Type = "City", CreatedBy = "Test" },
                new() { Type = "PinCode", CreatedBy = "Test" },
                new() { Type = "Street", CreatedBy = "Test" }
            }
        };
        var identityResource3 = new IdentityResourcesModel
        {
            Name = "Email",
            DisplayName = "Your email address",
            Description = "Your email address",
            Enabled = true,
            Emphasize = true,
            CreatedBy = "Test",
            IdentityClaims = new List<IdentityClaimsModel>
            {
                new() { Type = "email", CreatedBy = "Test" },
                new() { Type = "email_verified", AliasType = "emailconfirmed", CreatedBy = "Test" }
            }
        };
        var identityResource4 = new IdentityResourcesModel
        {
            Name = "Phone",
            DisplayName = "Your phone number",
            Description = "Your phone number",
            Enabled = true,
            Emphasize = true,
            CreatedBy = "Test",
            IdentityClaims = new List<IdentityClaimsModel>
            {
                new() { Type = "phone_number", CreatedBy = "Test", AliasType = "phonenumber" },
                new() { Type = "phone_number_verified", AliasType = "phonenumberconfirmed", CreatedBy = "Test" }
            }
        };
        var identityResource5 = new IdentityResourcesModel
        {
            Name = "Address",
            DisplayName = "Your postal address",
            Description = "Your postal address",
            Enabled = true,
            Emphasize = true,
            CreatedBy = "Test",
            IdentityClaims = new List<IdentityClaimsModel>
            {
                new() { Type = "address", CreatedBy = "Test" }
            }
        };

        identityResources.Add(identityResource1);
        identityResources.Add(identityResource2);
        identityResources.Add(identityResource3);
        identityResources.Add(identityResource4);
        identityResources.Add(identityResource5);
        return identityResources;
    }

    public static List<ApiResourcesModel> CreateApiResourceModelMaster()
    {
        var apiResources = new List<ApiResourcesModel>();
        var apiResource1 = new ApiResourcesModel
        {
            Name = "HCL.CS.SF.Role",
            Description = "To register and manage Roles",
            Enabled = true,
            CreatedBy = "JohnDoe",
            ApiResourceClaims = new List<ApiResourceClaimsModel>
            {
                new() { Type = "Role", CreatedBy = "JohnDoe" }
            },
            ApiScopes = new List<ApiScopesModel>
            {
                new()
                {
                    Name = "HCL.CS.SF.Role.Read", Description = "ReadOnly access for Role Api", CreatedBy = "JohnDoe"
                },
                new()
                {
                    Name = "HCL.CS.SF.Role.Write", Description = "Create/Update access for Role Api", CreatedBy = "JohnDoe"
                },
                new()
                {
                    Name = "HCL.CS.SF.Role.Delete", Description = "Delete access for Role Api", CreatedBy = "JohnDoe"
                },
                new()
                {
                    Name = "HCL.CS.SF.Role.Manage", Description = "Read/Write/Delete access for Role Api",
                    CreatedBy = "JohnDoe",
                    ApiScopeClaims = new List<ApiScopeClaimsModel>
                    {
                        new() { Type = "Role", CreatedBy = "JohnDoe" }
                    }
                }
            }
        };

        var apiResource2 = new ApiResourcesModel
        {
            Name = "HCL.CS.SF.Client",
            Description = "To register and manage all users supported in the system",
            Enabled = true,
            CreatedBy = "Test",
            ApiScopes = new List<ApiScopesModel>
            {
                new()
                {
                    Name = "HCL.CS.SF.Client.Read", Description = "ReadOnly access for Client Api", CreatedBy = "Test"
                },
                new() { Name = "HCL.CS.SF.Client.Write", Description = "Write access for Client Api", CreatedBy = "Test" },
                new()
                {
                    Name = "HCL.CS.SF.Client.Delete", Description = "Delete access for Client Api", CreatedBy = "Test"
                },
                new()
                {
                    Name = "HCL.CS.SF.Client.Manage", Description = "Read/Write/Delete access for Client Api",
                    CreatedBy = "Test"
                }
            }
        };
        var apiResource3 = new ApiResourcesModel
        {
            Name = "HCL.CS.SF.User",
            Description = "To register and manage all users supported in the system",
            Enabled = true,
            CreatedBy = "Test",
            ApiScopes = new List<ApiScopesModel>
            {
                new() { Name = "HCL.CS.SF.User.Read", Description = "ReadOnly access for Client Api", CreatedBy = "Test" },
                new() { Name = "HCL.CS.SF.User.Write", Description = "Write access for Client Api", CreatedBy = "Test" },
                new() { Name = "HCL.CS.SF.User.Delete", Description = "Delete access for Client Api", CreatedBy = "Test" },
                new()
                {
                    Name = "HCL.CS.SF.User.Manage", Description = "Read/Write/Delete access for Client Api",
                    CreatedBy = "Test"
                }
            }
        };

        var apiResource4 = new ApiResourcesModel
        {
            Name = "HCL.CS.SF.ApiResource",
            Description = "To register and manage all users supported in the system",
            Enabled = true,
            CreatedBy = "Test",
            ApiScopes = new List<ApiScopesModel>
            {
                new()
                {
                    Name = "HCL.CS.SF.ApiResource.Read", Description = "ReadOnly access for Client Api", CreatedBy = "Test"
                },
                new()
                {
                    Name = "HCL.CS.SF.ApiResource.Write", Description = "Write access for Client Api", CreatedBy = "Test"
                },
                new()
                {
                    Name = "HCL.CS.SF.ApiResource.Delete", Description = "Delete access for Client Api", CreatedBy = "Test"
                },
                new()
                {
                    Name = "HCL.CS.SF.ApiResource.Manage", Description = "Read/Write/Delete access for Client Api",
                    CreatedBy = "Test"
                }
            }
        };

        var apiResource5 = new ApiResourcesModel
        {
            Name = "HCL.CS.SF.IdentityResource",
            Description = "To register and manage all users supported in the system",
            Enabled = true,
            CreatedBy = "Test",
            ApiScopes = new List<ApiScopesModel>
            {
                new()
                {
                    Name = "HCL.CS.SF.IdentityResource.Read", Description = "ReadOnly access for Client Api",
                    CreatedBy = "Test"
                },
                new()
                {
                    Name = "HCL.CS.SF.IdentityResource.Write", Description = "Write access for Client Api",
                    CreatedBy = "Test"
                },
                new()
                {
                    Name = "HCL.CS.SF.IdentityResource.Delete", Description = "Delete access for Client Api",
                    CreatedBy = "Test"
                },
                new()
                {
                    Name = "HCL.CS.SF.IdentityResource.Manage", Description = "Read/Write/Delete access for Client Api",
                    CreatedBy = "Test"
                }
            }
        };
        var apiResource6 = new ApiResourcesModel
        {
            Name = "HCL.CS.SF.Admin",
            Description = "To register and manage all users supported in the system",
            Enabled = true,
            CreatedBy = "Test",
            ApiScopes = new List<ApiScopesModel>
            {
                new()
                {
                    Name = "HCL.CS.SF.Admin.Manage", Description = "Read/Write/Delete access for Client Api",
                    CreatedBy = "Test"
                }
            }
        };

        apiResources.Add(apiResource1);
        apiResources.Add(apiResource2);
        apiResources.Add(apiResource3);
        apiResources.Add(apiResource4);
        apiResources.Add(apiResource5);
        apiResources.Add(apiResource6);
        return apiResources;
    }

    public static List<ClientsModel> CreateClientMaster()
    {
        ClientsModel clientModelName;

        var clients = new List<ClientsModel>();
        var algorithm = "HS256,HS512,HS384,RS256,RS512,RS384,PS256,PS512,PS384,ES256,ES512,ES384".Split(",");
        foreach (var item in algorithm)
        {
            var algorithmName = item.ToUpper();
            var clientName = "HCL.CS.SF" + " " + item;
            clientModelName = new ClientsModel
            {
                ClientId = 32.RandomString(),
                ClientName = clientName,
                ClientUri = "https://identity:5002",
                ClientSecret = 32.RandomString(),
                ClientIdIssuedAt = DateTime.Now,
                ClientSecretExpiresAt = DateTime.Now.AddDays(180),
                LogoUri = "https://localhost:5002/logo.png",
                TermsOfServiceUri = "https://localhost:5002/Tos.cshtml",
                PolicyUri = "https://localhost:5002/Policy.cshtml",

                RefreshTokenExpiration = 3600 * 2, // Two hours
                AccessTokenExpiration = 3600,
                IdentityTokenExpiration = 3600,
                AuthorizationCodeExpiration = 1900,
                AccessTokenType = 0,
                RequirePkce = true,
                IsPkceTextPlain = true,
                RequireClientSecret = true,
                IsFirstPartyApp = false,
                AllowAccessTokensViaBrowser = true,
                CreatedBy = "Test",
                RedirectUris = new List<ClientRedirectUrisModel>
                {
                    new() { RedirectUri = "https://localhost:44300/index.html", CreatedBy = "Test" },
                    new() { RedirectUri = "https://localhost:5002/callback.html", CreatedBy = "Test" },
                    new() { RedirectUri = "https://localhost:5002/signin-oidc", CreatedBy = "Test" }
                },

                PostLogoutRedirectUris = new List<ClientPostLogoutRedirectUrisModel>
                {
                    new() { PostLogoutRedirectUri = "https://localhost:5002/callback.html", CreatedBy = "Test" },
                    new() { PostLogoutRedirectUri = "https://localhost:5002/signout-oidc", CreatedBy = "Test" },
                    new() { PostLogoutRedirectUri = "https://localhost:5002/signout-callback-oidc", CreatedBy = "Test" }
                },
                SupportedGrantTypes = new List<string>
                    { "authorization_code", "client_credentials", "password", "refresh_token", "hybrid" },
                AllowedScopes = new List<string>
                    { "openid", "email", "profile", "clientapi", "offline_access", "phone", "userapi", "clientapi" },
                SupportedResponseTypes = new List<string> { "code", "id_token", "token" },
                AllowedSigningAlgorithm = algorithmName,
                AllowOfflineAccess = true,
                ApplicationType = ApplicationType.RegularWeb,
                FrontChannelLogoutUri = "https://localhost:5002/signout-oidc"
            };

            clients.Add(clientModelName);
        }

        // Plain PKCE Client
        var plainPKCEClient = new ClientsModel
        {
            ClientId = 32.RandomString(),
            ClientName = "HCL.CS.SF Plain PKCE Client",
            ClientUri = "https://identity:5002",
            ClientSecret = 32.RandomString(),
            ClientIdIssuedAt = DateTime.Now,
            ClientSecretExpiresAt = DateTime.Now.AddDays(180),
            LogoUri = "https://localhost:5002/logo.png",
            TermsOfServiceUri = "https://localhost:5002/Tos.cshtml",
            PolicyUri = "https://localhost:5002/Policy.cshtml",

            RefreshTokenExpiration = 3600 * 2, // Two hours
            AccessTokenExpiration = 3600,
            IdentityTokenExpiration = 3600,
            AuthorizationCodeExpiration = 1900,
            LogoutTokenExpiration = 3600,
            AccessTokenType = 0,
            RequirePkce = true,
            IsPkceTextPlain = true,
            RequireClientSecret = true,
            IsFirstPartyApp = false,
            AllowAccessTokensViaBrowser = true,
            CreatedBy = "Test",
            RedirectUris = new List<ClientRedirectUrisModel>
            {
                new() { RedirectUri = "https://localhost:44300/index.html", CreatedBy = "Test" },
                new() { RedirectUri = "https://localhost:5002/callback.html", CreatedBy = "Test" },
                new() { RedirectUri = "https://localhost:5002/signin-oidc", CreatedBy = "Test" }
            },

            PostLogoutRedirectUris = new List<ClientPostLogoutRedirectUrisModel>
            {
                new() { PostLogoutRedirectUri = "https://localhost:5002/callback.html", CreatedBy = "Test" },
                new() { PostLogoutRedirectUri = "https://localhost:5002/signout-oidc", CreatedBy = "Test" },
                new() { PostLogoutRedirectUri = "https://localhost:5002/signout-callback-oidc", CreatedBy = "Test" }
            },
            SupportedGrantTypes = new List<string>
                { "authorization_code", "client_credentials", "password", "refresh_token", "hybrid" },
            AllowedScopes = new List<string>
                { "openid", "email", "profile", "clientapi", "offline_access", "phone", "userapi", "clientapi" },
            SupportedResponseTypes = new List<string> { "code", "id_token", "token" },
            AllowedSigningAlgorithm = Algorithms.RsaSha256,
            AllowOfflineAccess = true,
            ApplicationType = ApplicationType.RegularWeb,
            FrontChannelLogoutUri = "https://localhost:5002/signout-oidc"
        };

        clients.Add(plainPKCEClient);

        var s256Client = new ClientsModel
        {
            ClientId = 32.RandomString(),
            ClientName = "HCL.CS.SF S256 Client",
            ClientUri = "https://identity:5002",
            ClientSecret = 32.RandomString(),
            ClientIdIssuedAt = DateTime.Now,
            ClientSecretExpiresAt = DateTime.Now.AddDays(180),
            LogoUri = "https://localhost:5002/logo.png",
            TermsOfServiceUri = "https://localhost:5002/Tos.cshtml",
            PolicyUri = "https://localhost:5002/Policy.cshtml",

            RefreshTokenExpiration = 3600 * 2, // Two hours
            AccessTokenExpiration = 3600,
            IdentityTokenExpiration = 3600,
            AuthorizationCodeExpiration = 1800,
            LogoutTokenExpiration = 3600,
            AccessTokenType = 0,
            RequirePkce = true,
            IsPkceTextPlain = false,
            RequireClientSecret = true,
            IsFirstPartyApp = false,
            AllowAccessTokensViaBrowser = true,
            CreatedBy = "Test",
            RedirectUris = new List<ClientRedirectUrisModel>
            {
                new() { RedirectUri = "https://localhost:44300/index.html", CreatedBy = "Test" },
                new() { RedirectUri = "https://localhost:5002/callback.html", CreatedBy = "Test" },
                new() { RedirectUri = "https://localhost:5002/signin-oidc", CreatedBy = "Test" }
            },

            PostLogoutRedirectUris = new List<ClientPostLogoutRedirectUrisModel>
            {
                new() { PostLogoutRedirectUri = "https://localhost:5002/callback.html", CreatedBy = "Test" },
                new() { PostLogoutRedirectUri = "https://localhost:5002/signout-oidc", CreatedBy = "Test" },
                new() { PostLogoutRedirectUri = "https://localhost:5002/signout-callback-oidc", CreatedBy = "Test" }
            },
            SupportedGrantTypes = new List<string>
                { "authorization_code", "client_credentials", "password", "refresh_token", "hybrid" },
            AllowedScopes = new List<string> { "openid", "email", "profile", "clientapi", "offline_access", "phone" },
            SupportedResponseTypes = new List<string> { "code", "id_token", "token" },
            AllowedSigningAlgorithm = Algorithms.RsaSha256,
            AllowOfflineAccess = true,
            ApplicationType = ApplicationType.RegularWeb,
            FrontChannelLogoutUri = "https://localhost:5002/signout-oidc"
        };

        clients.Add(s256Client);

        var earlyTOkenExpireClient = new ClientsModel
        {
            ClientId = 32.RandomString(),
            ClientName = "HCL.CS.SF Early Token Expire Client",
            ClientUri = "https://identity:5002",
            ClientSecret = 32.RandomString(),
            ClientIdIssuedAt = DateTime.Now,
            ClientSecretExpiresAt = DateTime.Now.AddDays(180),
            LogoUri = "https://localhost:5002/logo.png",
            TermsOfServiceUri = "https://localhost:5002/Tos.cshtml",
            PolicyUri = "https://localhost:5002/Policy.cshtml",

            RefreshTokenExpiration = 1800, // Two hours
            AccessTokenExpiration = 1800,
            IdentityTokenExpiration = 1800,
            AuthorizationCodeExpiration = 1800,
            LogoutTokenExpiration = 3600,
            AccessTokenType = 0,
            RequirePkce = true,
            IsPkceTextPlain = true,
            RequireClientSecret = true,
            IsFirstPartyApp = false,
            AllowAccessTokensViaBrowser = true,
            CreatedBy = "Test",
            RedirectUris = new List<ClientRedirectUrisModel>
            {
                new() { RedirectUri = "https://localhost:44300/index.html", CreatedBy = "Test" },
                new() { RedirectUri = "https://localhost:5002/callback.html", CreatedBy = "Test" },
                new() { RedirectUri = "https://localhost:5002/signin-oidc", CreatedBy = "Test" }
            },

            PostLogoutRedirectUris = new List<ClientPostLogoutRedirectUrisModel>
            {
                new() { PostLogoutRedirectUri = "https://localhost:5002/callback.html", CreatedBy = "Test" },
                new() { PostLogoutRedirectUri = "https://localhost:5002/signout-oidc", CreatedBy = "Test" },
                new() { PostLogoutRedirectUri = "https://localhost:5002/signout-callback-oidc", CreatedBy = "Test" }
            },
            SupportedGrantTypes = new List<string>
                { "authorization_code", "client_credentials", "password", "refresh_token", "hybrid" },
            AllowedScopes = new List<string> { "openid", "email", "profile", "clientapi", "offline_access", "phone" },
            SupportedResponseTypes = new List<string> { "code", "id_token", "token" },
            AllowedSigningAlgorithm = Algorithms.RsaSha256,
            AllowOfflineAccess = true,
            ApplicationType = ApplicationType.RegularWeb,
            FrontChannelLogoutUri = "https://localhost:5002/signout-oidc"
        };

        clients.Add(earlyTOkenExpireClient);

        var clientSecretExpireClient = new ClientsModel
        {
            ClientId = 32.RandomString(),
            ClientName = "Client Secret Expire Client",
            ClientUri = "https://identity:5002",
            ClientSecret = 32.RandomString(),
            ClientIdIssuedAt = DateTime.Now,
            ClientSecretExpiresAt = DateTime.Now.AddDays(-1),
            LogoUri = "https://localhost:5002/logo.png",
            TermsOfServiceUri = "https://localhost:5002/Tos.cshtml",
            PolicyUri = "https://localhost:5002/Policy.cshtml",

            RefreshTokenExpiration = 3600 * 2, // Two hours
            AccessTokenExpiration = 3600,
            IdentityTokenExpiration = 3600,
            AuthorizationCodeExpiration = 1800,
            LogoutTokenExpiration = 3600,
            AccessTokenType = 0,
            RequirePkce = true,
            IsPkceTextPlain = true,
            RequireClientSecret = true,
            IsFirstPartyApp = false,
            AllowAccessTokensViaBrowser = true,
            CreatedBy = "Test",
            RedirectUris = new List<ClientRedirectUrisModel>
            {
                new() { RedirectUri = "https://localhost:44300/index.html", CreatedBy = "Test" },
                new() { RedirectUri = "https://localhost:5002/callback.html", CreatedBy = "Test" },
                new() { RedirectUri = "https://localhost:5002/signin-oidc", CreatedBy = "Test" }
            },

            PostLogoutRedirectUris = new List<ClientPostLogoutRedirectUrisModel>
            {
                new() { PostLogoutRedirectUri = "https://localhost:5002/callback.html", CreatedBy = "Test" },
                new() { PostLogoutRedirectUri = "https://localhost:5002/signout-oidc", CreatedBy = "Test" },
                new() { PostLogoutRedirectUri = "https://localhost:5002/signout-callback-oidc", CreatedBy = "Test" }
            },
            SupportedGrantTypes = new List<string>
                { "authorization_code", "client_credentials", "password", "refresh_token", "hybrid" },
            AllowedScopes = new List<string> { "openid", "email", "profile", "clientapi", "offline_access", "phone" },
            SupportedResponseTypes = new List<string> { "code", "id_token", "token" },
            AllowedSigningAlgorithm = Algorithms.RsaSha256,
            AllowOfflineAccess = true,
            ApplicationType = ApplicationType.RegularWeb,
            FrontChannelLogoutUri = "https://localhost:5002/signout-oidc"
        };

        clients.Add(clientSecretExpireClient);

        var disablePKCEClient = new ClientsModel
        {
            ClientId = 32.RandomString(),
            ClientName = "HCL.CS.SF ES256 Algorithm Client",
            ClientUri = "https://identity:5002",
            ClientSecret = 32.RandomString(),
            ClientIdIssuedAt = DateTime.Now,
            ClientSecretExpiresAt = DateTime.Now.AddDays(180),
            LogoUri = "https://localhost:5002/logo.png",
            TermsOfServiceUri = "https://localhost:5002/Tos.cshtml",
            PolicyUri = "https://localhost:5002/Policy.cshtml",

            RefreshTokenExpiration = 3600 * 2, // Two hours
            AccessTokenExpiration = 3600,
            IdentityTokenExpiration = 3600,
            AuthorizationCodeExpiration = 1800,
            LogoutTokenExpiration = 3600,
            AccessTokenType = 0,
            RequirePkce = true,
            IsPkceTextPlain = true,
            RequireClientSecret = true,
            IsFirstPartyApp = false,
            AllowAccessTokensViaBrowser = true,
            CreatedBy = "Test",
            RedirectUris = new List<ClientRedirectUrisModel>
            {
                new() { RedirectUri = "https://localhost:44300/index.html", CreatedBy = "Test" },
                new() { RedirectUri = "https://localhost:5002/callback.html", CreatedBy = "Test" },
                new() { RedirectUri = "https://localhost:5002/signin-oidc", CreatedBy = "Test" }
            },

            PostLogoutRedirectUris = new List<ClientPostLogoutRedirectUrisModel>
            {
                new() { PostLogoutRedirectUri = "https://localhost:5002/callback.html", CreatedBy = "Test" },
                new() { PostLogoutRedirectUri = "https://localhost:5002/signout-oidc", CreatedBy = "Test" },
                new() { PostLogoutRedirectUri = "https://localhost:5002/signout-callback-oidc", CreatedBy = "Test" }
            },
            SupportedGrantTypes = new List<string>
                { "authorization_code", "client_credentials", "password", "refresh_token", "hybrid" },
            AllowedScopes = new List<string>
                { "openid", "email", "profile", "clientapi", "offline_access", "phone", "userapi" },
            SupportedResponseTypes = new List<string> { "code", "id_token", "token" },
            AllowedSigningAlgorithm = Algorithms.RsaSha256,
            AllowOfflineAccess = true,
            ApplicationType = ApplicationType.RegularWeb,
            FrontChannelLogoutUri = "https://localhost:5002/signout-oidc"
        };

        clients.Add(disablePKCEClient);

        var disableClientScretClient = new ClientsModel
        {
            ClientId = 32.RandomString(),
            ClientName = "HCL.CS.SF DisableClientSecret Client",
            ClientUri = "https://identity:5002",
            ClientSecret = 32.RandomString(),
            ClientIdIssuedAt = DateTime.Now,
            ClientSecretExpiresAt = DateTime.Now.AddDays(180),
            LogoUri = "https://localhost:5002/logo.png",
            TermsOfServiceUri = "https://localhost:5002/Tos.cshtml",
            PolicyUri = "https://localhost:5002/Policy.cshtml",

            RefreshTokenExpiration = 3600 * 2, // Two hours
            AccessTokenExpiration = 3600,
            IdentityTokenExpiration = 3600,
            AuthorizationCodeExpiration = 1800,
            LogoutTokenExpiration = 3600,
            AccessTokenType = 0,
            RequirePkce = true,
            IsPkceTextPlain = true,
            RequireClientSecret = false,
            IsFirstPartyApp = false,
            AllowAccessTokensViaBrowser = true,
            CreatedBy = "Test",
            RedirectUris = new List<ClientRedirectUrisModel>
            {
                new() { RedirectUri = "https://localhost:44300/index.html", CreatedBy = "Test" },
                new() { RedirectUri = "https://localhost:5002/callback.html", CreatedBy = "Test" },
                new() { RedirectUri = "https://localhost:5002/signin-oidc", CreatedBy = "Test" }
            },

            PostLogoutRedirectUris = new List<ClientPostLogoutRedirectUrisModel>
            {
                new() { PostLogoutRedirectUri = "https://localhost:5002/callback.html", CreatedBy = "Test" },
                new() { PostLogoutRedirectUri = "https://localhost:5002/signout-oidc", CreatedBy = "Test" },
                new() { PostLogoutRedirectUri = "https://localhost:5002/signout-callback-oidc", CreatedBy = "Test" }
            },
            SupportedGrantTypes = new List<string>
                { "authorization_code", "client_credentials", "password", "refresh_token", "hybrid" },
            AllowedScopes = new List<string> { "openid", "email", "profile", "clientapi", "offline_access", "phone" },
            SupportedResponseTypes = new List<string> { "code", "id_token", "token" },
            AllowedSigningAlgorithm = Algorithms.RsaSha256,
            AllowOfflineAccess = true,
            ApplicationType = ApplicationType.RegularWeb,
            FrontChannelLogoutUri = "https://localhost:5002/signout-oidc"
        };
        clients.Add(disableClientScretClient);


        return clients;
    }
}
