/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain.Models.Api;

namespace TestApp.Helper.Api;

public static class ApiResourceHelper
{
    public static ApiResourcesModel CreateApiResourceModel()
    {
        var apiResourceModel = new ApiResourcesModel
        {
            Name = "ClientApi",
            Description = "To register and manage client applications",
            DisplayName = "Client Api",
            Enabled = false,
            CreatedBy = "Test",
            ApiResourceClaims = new List<ApiResourceClaimsModel>
            {
                new() { Type = "Type1", CreatedBy = "Test" },
                new() { Type = "Type2", CreatedBy = "Test" },
                new() { Type = "Type3", CreatedBy = "Test" }
            },
            ApiScopes = new List<ApiScopesModel>
            {
                new()
                {
                    Name = "ClientApi.Create", DisplayName = "DisplayName - ApiScope1",
                    Description = "ClientApi Description", Emphasize = false, Required = true,
                    CreatedBy = "Test",
                    ApiScopeClaims = new List<ApiScopeClaimsModel>
                    {
                        new() { Type = "ApiScopeClaim - 1", CreatedBy = "Test" },
                        new() { Type = "ApiScopeClaim - 2", CreatedBy = "Test" }
                    }
                },
                new()
                {
                    Name = "ApiScope2", DisplayName = "DisplayName - ApiScope2",
                    Description = "ClientApi Description", Emphasize = false, Required = true,
                    CreatedBy = "Test",
                    ApiScopeClaims = new List<ApiScopeClaimsModel>
                    {
                        new() { Type = "ApiScopeClaim - 3", CreatedBy = "Test" },
                        new() { Type = "ApiScopeClaim - 4", CreatedBy = "Test" }
                    }
                }
            }
        };
        return apiResourceModel;
    }

    public static List<ApiResourcesModel> GetApiResourceModelMasterData()
    {
        var apiResources = new List<ApiResourcesModel>();

        var apiResource1 = new ApiResourcesModel
        {
            Name = "ClientApi",
            Description = "To register and manage client applications",
            Enabled = true,
            ApiResourceClaims = new List<ApiResourceClaimsModel>
            {
                new() { Type = "Role" }
            },
            ApiScopes = new List<ApiScopesModel>
            {
                new()
                {
                    Name = "ClientApi.Create",
                    Description = "Create access for Client Api",
                    ApiScopeClaims = new List<ApiScopeClaimsModel>
                    {
                        new() { Type = "Role" }
                    }
                },
                new()
                {
                    Name = "ClientApi.ReadOnly",
                    Description = "ReadOnly access for Client Api"
                },
                new()
                {
                    Name = "ClientApi.ReadWrite",
                    Description = "ReadWrite access for Client Api"
                },
                new()
                {
                    Name = "ClientApi.Delete",
                    Description = "Delete access for Client Api",
                    ApiScopeClaims = new List<ApiScopeClaimsModel>
                    {
                        new() { Type = "Role" }
                    }
                }
            }
        };

        var apiResource2 = new ApiResourcesModel
        {
            Name = "UserApi",
            Description = "To register and manage all users supported in the system",
            Enabled = true,

            ApiScopes = new List<ApiScopesModel>
            {
                new() { Name = "UserApi.Create", Description = "Create access for User Api" },
                new() { Name = "UserAPI.ReadOnly", Description = "ReadOnly access for User Api" },
                new() { Name = "UserAPI.ReadWrite", Description = "ReadWrite access for User Api" },
                new() { Name = "UserAPI.Delete", Description = "Delete access for User Api" },
                new()
                {
                    Name = "UserAPI.FullAccess", Description = "Full access for User Api",
                    ApiScopeClaims = new List<ApiScopeClaimsModel>
                    {
                        new() { Type = "Role" }
                    }
                },
                new() { Name = "Shared.Readonly", Description = "Shared ReadOnly access for User Api" },
                new() { Name = "Shared.Write", Description = "Shared Write access for User Api" }
            }
        };

        var apiResource3 = new ApiResourcesModel
        {
            Name = "RoleApi",
            Description = "To support all role related workflows ",
            Enabled = true,
            ApiResourceClaims = new List<ApiResourceClaimsModel>
            {
                new() { Type = "Role" }
            },
            ApiScopes = new List<ApiScopesModel>
            {
                new() { Name = "RoleApi.Create", Description = "Create Access for Role Api" },
                new() { Name = "RoleApi.ReadOnly", Description = "ReadOnly Access for Role Api" },
                new() { Name = "RoleApi.ReadWrite", Description = "ReadWrite Access for Role Api" },
                new() { Name = "RoleApi.Delete", Description = "Delete Access for Role Api" },
                new()
                {
                    Name = "RoleApi.FullAccess", Description = "Full Access for Role Api",
                    ApiScopeClaims = new List<ApiScopeClaimsModel>
                    {
                        new() { Type = "Role" }
                    }
                },
                new() { Name = "Shared.Readonly", Description = "Shared ReadOnly access for User Api" },
                new() { Name = "Shared.Write", Description = "Shared Write access for User Api" }
            }
        };


        var apiResource4 = new ApiResourcesModel
        {
            Name = "NotificationApi",
            Description = "To manage all notification workflows supported in the system",
            Enabled = true,
            ApiScopes = new List<ApiScopesModel>
            {
                new() { Name = "NotificationApi.ReadOnly", Description = "ReadOnly Access for Notification Api" },
                new() { Name = "NotificationApi.FullAccess", Description = "Full Access for Notification Api" }
            }
        };


        apiResources.Add(apiResource1);
        apiResources.Add(apiResource2);
        apiResources.Add(apiResource3);
        apiResources.Add(apiResource4);

        return apiResources;
    }

    public static List<ApiResourcesModel> UpdateApiResourceModel()
    {
        var apiResources = new List<ApiResourcesModel>();

        var apiResource1 = new ApiResourcesModel
        {
            Name = "ClientApi",
            Description = "To register and manage client applications",
            Enabled = true,
            ApiResourceClaims = new List<ApiResourceClaimsModel>
            {
                new() { Type = "Role" }
            },
            ApiScopes = new List<ApiScopesModel>
            {
                new()
                {
                    Name = "ClientApi.Create",
                    Description = "Create access for Client Api",
                    ApiScopeClaims = new List<ApiScopeClaimsModel>
                    {
                        new() { Type = "Role" }
                    }
                }
            }
        };
        apiResources.Add(apiResource1);
        return apiResources;
    }

    public static ApiResourcesModel UpdateApiResourceModelUnUsed()
    {
        var apiResourceModel = new ApiResourcesModel
        {
            Name = "TestApiResource1",
            DisplayName = "TestDisplayName2",
            Description = "This is 2 test Description - Updated ",
            Enabled = false,
            CreatedBy = "Test",
            ApiResourceClaims = new List<ApiResourceClaimsModel>
            {
                new() { Type = "Type4", CreatedBy = "Test" },
                new() { Type = "Type5", CreatedBy = "Test" },
                new() { Type = "Type6", CreatedBy = "Test" }
            },
            ApiScopes = new List<ApiScopesModel>
            {
                new()
                {
                    Name = "ApiScope3", DisplayName = "DisplayName - ApiScope3",
                    Description = "TestApiScope3 Description", Emphasize = false, Required = true, CreatedBy = "Test",
                    ApiScopeClaims = new List<ApiScopeClaimsModel>
                    {
                        new() { Type = "ApiScopeClaim - 5", CreatedBy = "Test" },
                        new() { Type = "ApiScopeClaim - 6", CreatedBy = "Test" }
                    }
                },
                new()
                {
                    Name = "ApiScope4", DisplayName = "DisplayName - ApiScope4",
                    Description = "TestApiScope4 Description", Emphasize = true, Required = false, CreatedBy = "Test",
                    ApiScopeClaims = new List<ApiScopeClaimsModel>
                    {
                        new() { Type = "ApiScopeClaim - 7", CreatedBy = "Test" },
                        new() { Type = "ApiScopeClaim - 8", CreatedBy = "Test" }
                    }
                }
            }
        };
        return apiResourceModel;
    }

    public static ApiResourceClaimsModel CreateApiResourceClaimModel()
    {
        var apiResourceClaimsModel = new ApiResourceClaimsModel
        {
            ApiResourceId = Guid.NewGuid(),
            CreatedBy = "Test",
            Type = "Test Claim"
        };
        return apiResourceClaimsModel;
    }

    public static ApiResourceClaimsModel CreateApiResourceClaimSuccess()
    {
        var guid = Guid.NewGuid();
        var apiResourceClaimsModel = new ApiResourceClaimsModel
        {
            ApiResourceId = guid,
            CreatedBy = "Test",
            Type = "Test"
        };
        return apiResourceClaimsModel;
    }

    public static ApiScopesModel CreateApiScopeModel()
    {
        var apiScopeModel = new ApiScopesModel
        {
            Name = "ApiScopeOne",
            DisplayName = "DisplayName - ApiScope1",
            Description = "ClientApi Description",
            Emphasize = false,
            Required = true,
            CreatedBy = "Test",
            ApiScopeClaims = new List<ApiScopeClaimsModel>
            {
                new() { Type = "ApiScopeClaim - 1", CreatedBy = "Test" },
                new() { Type = "ApiScopeClaim - 2", CreatedBy = "Test" }
            }
        };
        return apiScopeModel;
    }

    public static ApiScopesModel CreateApiScopeModelSuccess()
    {
        var gud = Guid.NewGuid();
        var apiScopeModel = new ApiScopesModel
        {
            ApiResourceId = gud,
            Name = "ApiScopeOne",
            DisplayName = "DisplayName - ApiScope1",
            Description = "ClientApi Description",
            Emphasize = false,
            Required = true,
            CreatedBy = "Test",
            ApiScopeClaims = new List<ApiScopeClaimsModel>
            {
                new() { ApiScopeId = gud, Type = "ApiScopeClaim - 1", CreatedBy = "Test" },
                new() { ApiScopeId = gud, Type = "ApiScopeClaim - 2", CreatedBy = "Test" }
            }
        };
        return apiScopeModel;
    }

    public static ApiScopeClaimsModel CreateApiScopeClaimModel_Success()
    {
        var guid = Guid.NewGuid();
        var apiScopeClaimsModel = new ApiScopeClaimsModel
        {
            ApiScopeId = guid,
            CreatedBy = "Test",
            Type = "Test"
        };
        return apiScopeClaimsModel;
    }

    public static ApiScopeClaimsModel CreateApiScopeClaimModel()
    {
        var guid = Guid.NewGuid();
        var apiScopeClaimsModel = new ApiScopeClaimsModel
        {
            ApiScopeId = guid,
            CreatedBy = "Test",
            Type = "Test"
        };
        return apiScopeClaimsModel;
    }
}
