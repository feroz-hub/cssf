/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain.Entities.Api;
using HCL.CS.SF.Domain.Entities.Endpoint;
using HCL.CS.SF.Domain.Enums;
using static HCL.CS.SF.Domain.Constants.Endpoint.OpenIdConstants;

namespace HCLCSSFInstallerMVC.Infrastructure.Seeding;

/// <summary>
/// Provides factory methods that produce the master seed data for the security framework database.
/// Includes API resources with scopes, identity resources with claims, roles with claims,
/// security questions, and template entities for users and clients.
/// </summary>
public static class HCLCSSFMasterDataSeed
{
    /// <summary>
    /// Creates the default set of API resources and their associated scopes and claims.
    /// </summary>
    public static List<ApiResources> GetApiResourceEntityMaster()
    {
        return
        [
            new ApiResources
            {
                Name = "HCL.CS.SF.apiresource",
                DisplayName = "HCL.CS.SF.APIRESOURCE",
                Description = "To register and manage ApiResource",
                Enabled = true,
                IsDeleted = false,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "HCLCSSFUser",
                ApiScopes =
                [
                    new()
                    {
                        Name = "HCL.CS.SF.apiresource.manage", DisplayName = "HCL.CS.SF.APIRESOURCE.MANAGE",
                        Description = "Manage api resource", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow,
                        IsDeleted = false,
                        ApiScopeClaims = new List<ApiScopeClaims>
                        {
                            new()
                            {
                                Type = "permission", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow,
                                IsDeleted = false
                            },
                            new()
                            {
                                Type = "role", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow, IsDeleted = false
                            }
                        }
                    },

                    new()
                    {
                        Name = "HCL.CS.SF.apiresource.read", DisplayName = "HCL.CS.SF.APIRESOURCE.READ",
                        Description = "Read api resource", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow,
                        IsDeleted = false,
                        ApiScopeClaims = new List<ApiScopeClaims>
                        {
                            new()
                            {
                                Type = "permission", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow,
                                IsDeleted = false
                            },
                            new()
                            {
                                Type = "role", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow, IsDeleted = false
                            }
                        }
                    },

                    new()
                    {
                        Name = "HCL.CS.SF.apiresource.write", DisplayName = "HCL.CS.SF.APIRESOURCE.WRITE",
                        Description = "Write api resource", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow,
                        IsDeleted = false,
                        ApiScopeClaims = new List<ApiScopeClaims>
                        {
                            new()
                            {
                                Type = "permission", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow,
                                IsDeleted = false
                            },
                            new()
                            {
                                Type = "role", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow, IsDeleted = false
                            }
                        }
                    },

                    new()
                    {
                        Name = "HCL.CS.SF.apiresource.delete", DisplayName = "HCL.CS.SF.APIRESOURCE.DELETE",
                        Description = "Delete api resource", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow,
                        IsDeleted = false,
                        ApiScopeClaims = new List<ApiScopeClaims>
                        {
                            new()
                            {
                                Type = "permission", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow,
                                IsDeleted = false
                            },
                            new()
                            {
                                Type = "role", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow, IsDeleted = false
                            }
                        }
                    }
                ]
            },

            new()
            {
                Name = "HCL.CS.SF.identityresource",
                DisplayName = "HCL.CS.SF.IDENTITYRESOURCE",
                Description = "To register and manage IdentityResource",
                Enabled = true,
                IsDeleted = false,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "HCLCSSFUser",
                ApiScopes = new List<ApiScopes>
                {
                    new()
                    {
                        Name = "HCL.CS.SF.identityresource.manage", DisplayName = "HCL.CS.SF.IDENTITYRESOURCE.MANAGE",
                        Description = "Manage identityresource resource", CreatedBy = "HCLCSSFUser",
                        CreatedOn = DateTime.UtcNow, IsDeleted = false,
                        ApiScopeClaims = new List<ApiScopeClaims>
                        {
                            new()
                            {
                                Type = "permission", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow,
                                IsDeleted = false
                            },
                            new()
                            {
                                Type = "role", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow, IsDeleted = false
                            }
                        }
                    },
                    new()
                    {
                        Name = "HCL.CS.SF.identityresource.read", DisplayName = "HCL.CS.SF.IDENTITYRESOURCE.READ",
                        Description = "Read identityresource resource", CreatedBy = "HCLCSSFUser",
                        CreatedOn = DateTime.UtcNow, IsDeleted = false,
                        ApiScopeClaims = new List<ApiScopeClaims>
                        {
                            new()
                            {
                                Type = "permission", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow,
                                IsDeleted = false
                            },
                            new()
                            {
                                Type = "role", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow, IsDeleted = false
                            }
                        }
                    },
                    new()
                    {
                        Name = "HCL.CS.SF.identityresource.write", DisplayName = "HCL.CS.SF.IDENTITYRESOURCE.WRITE",
                        Description = "Write identityresource resource", CreatedBy = "HCLCSSFUser",
                        CreatedOn = DateTime.UtcNow, IsDeleted = false,
                        ApiScopeClaims = new List<ApiScopeClaims>
                        {
                            new()
                            {
                                Type = "permission", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow,
                                IsDeleted = false
                            },
                            new()
                            {
                                Type = "role", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow, IsDeleted = false
                            }
                        }
                    },
                    new()
                    {
                        Name = "HCL.CS.SF.identityresource.delete", DisplayName = "HCL.CS.SF.IDENTITYRESOURCE.DELETE",
                        Description = "Delete identityresource resource", CreatedBy = "HCLCSSFUser",
                        CreatedOn = DateTime.UtcNow, IsDeleted = false,
                        ApiScopeClaims = new List<ApiScopeClaims>
                        {
                            new()
                            {
                                Type = "permission", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow,
                                IsDeleted = false
                            },
                            new()
                            {
                                Type = "role", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow, IsDeleted = false
                            }
                        }
                    }
                }
            },

            new()
            {
                Name = "HCL.CS.SF.client",
                DisplayName = "HCL.CS.SF.CLIENT",
                Description = "To register and manage client",
                Enabled = true,
                IsDeleted = false,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "HCLCSSFUser",
                ApiScopes = new List<ApiScopes>
                {
                    new()
                    {
                        Name = "HCL.CS.SF.client.manage", DisplayName = "HCL.CS.SF.CLIENT.MANAGE",
                        Description = "Manage client resource", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow,
                        IsDeleted = false,
                        ApiScopeClaims = new List<ApiScopeClaims>
                        {
                            new()
                            {
                                Type = "permission", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow,
                                IsDeleted = false
                            },
                            new()
                            {
                                Type = "role", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow, IsDeleted = false
                            }
                        }
                    },
                    new()
                    {
                        Name = "HCL.CS.SF.client.read", DisplayName = "HCL.CS.SF.CLIENT.READ",
                        Description = "Read client resource", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow,
                        IsDeleted = false,
                        ApiScopeClaims = new List<ApiScopeClaims>
                        {
                            new()
                            {
                                Type = "permission", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow,
                                IsDeleted = false
                            },
                            new()
                            {
                                Type = "role", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow, IsDeleted = false
                            }
                        }
                    },
                    new()
                    {
                        Name = "HCL.CS.SF.client.write", DisplayName = "HCL.CS.SF.CLIENT.WRITE",
                        Description = "Write client resource", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow,
                        IsDeleted = false,
                        ApiScopeClaims = new List<ApiScopeClaims>
                        {
                            new()
                            {
                                Type = "permission", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow,
                                IsDeleted = false
                            },
                            new()
                            {
                                Type = "role", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow, IsDeleted = false
                            }
                        }
                    },
                    new()
                    {
                        Name = "HCL.CS.SF.client.delete", DisplayName = "HCL.CS.SF.CLIENT.DELETE",
                        Description = "Delete client resource", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow,
                        IsDeleted = false,
                        ApiScopeClaims = new List<ApiScopeClaims>
                        {
                            new()
                            {
                                Type = "permission", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow,
                                IsDeleted = false
                            },
                            new()
                            {
                                Type = "role", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow, IsDeleted = false
                            }
                        }
                    }
                }
            },

            new()
            {
                Name = "HCL.CS.SF.role",
                DisplayName = "HCL.CS.SF.ROLE",
                Description = "To register and manage role",
                Enabled = true,
                IsDeleted = false,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "HCLCSSFUser",
                ApiScopes = new List<ApiScopes>
                {
                    new()
                    {
                        Name = "HCL.CS.SF.role.manage", DisplayName = "HCL.CS.SF.ROLE.MANAGE",
                        Description = "Manage role resource", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow,
                        IsDeleted = false,
                        ApiScopeClaims = new List<ApiScopeClaims>
                        {
                            new()
                            {
                                Type = "permission", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow,
                                IsDeleted = false
                            },
                            new()
                            {
                                Type = "role", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow, IsDeleted = false
                            }
                        }
                    },
                    new()
                    {
                        Name = "HCL.CS.SF.role.read", DisplayName = "HCL.CS.SF.ROLE.READ", Description = "Read role resource",
                        CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow, IsDeleted = false,
                        ApiScopeClaims = new List<ApiScopeClaims>
                        {
                            new()
                            {
                                Type = "permission", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow,
                                IsDeleted = false
                            },
                            new()
                            {
                                Type = "role", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow, IsDeleted = false
                            }
                        }
                    },
                    new()
                    {
                        Name = "HCL.CS.SF.role.write", DisplayName = "HCL.CS.SF.ROLE.WRITE",
                        Description = "Write role resource", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow,
                        IsDeleted = false,
                        ApiScopeClaims = new List<ApiScopeClaims>
                        {
                            new()
                            {
                                Type = "permission", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow,
                                IsDeleted = false
                            },
                            new()
                            {
                                Type = "role", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow, IsDeleted = false
                            }
                        }
                    },
                    new()
                    {
                        Name = "HCL.CS.SF.role.delete", DisplayName = "HCL.CS.SF.ROLE.DELETE",
                        Description = "Delete role resource", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow,
                        IsDeleted = false,
                        ApiScopeClaims = new List<ApiScopeClaims>
                        {
                            new()
                            {
                                Type = "permission", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow,
                                IsDeleted = false
                            },
                            new()
                            {
                                Type = "role", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow, IsDeleted = false
                            }
                        }
                    }
                }
            },

            new()
            {
                Name = "HCL.CS.SF.adminuser",
                DisplayName = "HCL.CS.SF.ADMINUSER",
                Description = "To register and manage adminuser",
                Enabled = true,
                IsDeleted = false,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "HCLCSSFUser",
                ApiScopes = new List<ApiScopes>
                {
                    new()
                    {
                        Name = "HCL.CS.SF.adminuser.manage", DisplayName = "HCL.CS.SF.ADMINUSER.MANAGE",
                        Description = "Manage admin resource", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow,
                        IsDeleted = false,
                        ApiScopeClaims = new List<ApiScopeClaims>
                        {
                            new()
                            {
                                Type = "permission", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow,
                                IsDeleted = false
                            },
                            new()
                            {
                                Type = "role", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow, IsDeleted = false
                            }
                        }
                    },
                    new()
                    {
                        Name = "HCL.CS.SF.adminuser.read", DisplayName = "HCL.CS.SF.ADMINUSER.READ",
                        Description = "Read admin resource", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow,
                        IsDeleted = false,
                        ApiScopeClaims = new List<ApiScopeClaims>
                        {
                            new()
                            {
                                Type = "permission", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow,
                                IsDeleted = false
                            },
                            new()
                            {
                                Type = "role", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow, IsDeleted = false
                            }
                        }
                    },
                    new()
                    {
                        Name = "HCL.CS.SF.adminuser.write", DisplayName = "HCL.CS.SF.ADMINUSER.WRITE",
                        Description = "Write admin resource", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow,
                        IsDeleted = false,
                        ApiScopeClaims = new List<ApiScopeClaims>
                        {
                            new()
                            {
                                Type = "permission", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow,
                                IsDeleted = false
                            },
                            new()
                            {
                                Type = "role", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow, IsDeleted = false
                            }
                        }
                    },
                    new()
                    {
                        Name = "HCL.CS.SF.adminuser.delete", DisplayName = "HCL.CS.SF.ADMINUSER.DELETE",
                        Description = "Delete admin resource", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow,
                        IsDeleted = false,
                        ApiScopeClaims = new List<ApiScopeClaims>
                        {
                            new()
                            {
                                Type = "permission", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow,
                                IsDeleted = false
                            },
                            new()
                            {
                                Type = "role", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow, IsDeleted = false
                            }
                        }
                    }
                }
            },

            new()
            {
                Name = "HCL.CS.SF.user",
                DisplayName = "HCL.CS.SF.USER",
                Description = "To register and manage user",
                Enabled = true,
                IsDeleted = false,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "HCLCSSFUser",
                ApiScopes = new List<ApiScopes>
                {
                    new()
                    {
                        Name = "HCL.CS.SF.user.manage", DisplayName = "HCL.CS.SF.USER.MANAGE",
                        Description = "Manage admin resource", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow,
                        IsDeleted = false,
                        ApiScopeClaims = new List<ApiScopeClaims>
                        {
                            new()
                            {
                                Type = "permission", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow,
                                IsDeleted = false
                            },
                            new()
                            {
                                Type = "role", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow, IsDeleted = false
                            }
                        }
                    },
                    new()
                    {
                        Name = "HCL.CS.SF.user.read", DisplayName = "HCL.CS.SF.USER.READ",
                        Description = "Read admin resource", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow,
                        IsDeleted = false,
                        ApiScopeClaims = new List<ApiScopeClaims>
                        {
                            new()
                            {
                                Type = "permission", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow,
                                IsDeleted = false
                            },
                            new()
                            {
                                Type = "role", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow, IsDeleted = false
                            }
                        }
                    },
                    new()
                    {
                        Name = "HCL.CS.SF.user.write", DisplayName = "HCL.CS.SF.USER.WRITE",
                        Description = "Write admin resource", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow,
                        IsDeleted = false,
                        ApiScopeClaims = new List<ApiScopeClaims>
                        {
                            new()
                            {
                                Type = "permission", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow,
                                IsDeleted = false
                            },
                            new()
                            {
                                Type = "role", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow, IsDeleted = false
                            }
                        }
                    },
                    new()
                    {
                        Name = "HCL.CS.SF.user.delete", DisplayName = "HCL.CS.SF.USER.DELETE",
                        Description = "Delete admin resource", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow,
                        IsDeleted = false,
                        ApiScopeClaims = new List<ApiScopeClaims>
                        {
                            new()
                            {
                                Type = "permission", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow,
                                IsDeleted = false
                            },
                            new()
                            {
                                Type = "role", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow, IsDeleted = false
                            }
                        }
                    }
                }
            },

            new()
            {
                Name = "HCL.CS.SF.securitytoken",
                DisplayName = "HCL.CS.SF.SECURITYTOKEN",
                Description = "Manage HCL.CS.SF tokens",
                Enabled = true,
                IsDeleted = false,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "HCLCSSFUser",
                ApiScopes = new List<ApiScopes>
                {
                    new()
                    {
                        Name = "HCL.CS.SF.securitytoken.manage", DisplayName = "HCL.CS.SF.SECURITYTOKEN.MANAGE",
                        Description = "Manage security token", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow,
                        IsDeleted = false,
                        ApiScopeClaims = new List<ApiScopeClaims>
                        {
                            new()
                            {
                                Type = "permission", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow,
                                IsDeleted = false
                            },
                            new()
                            {
                                Type = "role", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow, IsDeleted = false
                            }
                        }
                    },
                    new()
                    {
                        Name = "HCL.CS.SF.securitytoken.read", DisplayName = "HCL.CS.SF.SECURITYTOKEN.READ",
                        Description = "Read security token", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow,
                        IsDeleted = false,
                        ApiScopeClaims = new List<ApiScopeClaims>
                        {
                            new()
                            {
                                Type = "permission", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow,
                                IsDeleted = false
                            },
                            new()
                            {
                                Type = "role", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow, IsDeleted = false
                            }
                        }
                    }
                }
            },

            // RentFlow app API resource – access tokens include "capabilities" claim from user role
            new()
            {
                Name = "rentflow-api",
                DisplayName = "RentFlow Api",
                Description = "RentFlow app – capabilities in access token by role",
                Enabled = true,
                IsDeleted = false,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "HCLCSSFUser",
                ApiResourceClaims = new List<ApiResourceClaims>
                {
                    new()
                    {
                        Type = "capabilities", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow, IsDeleted = false
                    },
                    new()
                    {
                        Type = "role", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow, IsDeleted = false
                    }
                },
                ApiScopes = new List<ApiScopes>
                {
                    new()
                    {
                        Name = "rentflow-api",
                        DisplayName = "RentFlow Api",
                        Description = "RentFlow API access with capabilities claim",
                        CreatedBy = "HCLCSSFUser",
                        CreatedOn = DateTime.UtcNow,
                        IsDeleted = false,
                        ApiScopeClaims = new List<ApiScopeClaims>
                        {
                            new()
                            {
                                Type = "capabilities", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow,
                                IsDeleted = false
                            },
                            new()
                            {
                                Type = "role", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow, IsDeleted = false
                            }
                        }
                    }
                }
            }
        ];
    }

    /// <summary>
    /// Creates the default OIDC identity resources (openid, email, profile, phone, address).
    /// </summary>
    public static List<IdentityResources> CreateIdentityResourceModelMaster()
    {
        return new List<IdentityResources>
        {
            new()
            {
                Name = "openid",
                DisplayName = "OPENID",
                Description = "openid",
                Enabled = true,
                Required = true,
                Emphasize = true,
                CreatedBy = "HCLCSSFUser",
                CreatedOn = DateTime.UtcNow
            },
            new()
            {
                Name = "profile",
                DisplayName = "PROFILE",
                Description = "profile",
                Enabled = true,
                Required = true,
                Emphasize = true,
                CreatedBy = "HCLCSSFUser",
                CreatedOn = DateTime.UtcNow,
                IdentityClaims = new List<IdentityClaims>
                {
                    new()
                    {
                        Type = "subject", CreatedBy = "HCLCSSFUser", AliasType = "sub", CreatedOn = DateTime.UtcNow
                    },
                    new()
                    {
                        Type = "name", CreatedBy = "HCLCSSFUser", AliasType = "username", CreatedOn = DateTime.UtcNow
                    },
                    new()
                    {
                        Type = "given_name", CreatedBy = "HCLCSSFUser", AliasType = "firstname",
                        CreatedOn = DateTime.UtcNow
                    },
                    new()
                    {
                        Type = "family_name", CreatedBy = "HCLCSSFUser", AliasType = "lastname",
                        CreatedOn = DateTime.UtcNow
                    },
                    new() { Type = "middle_name", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow },
                    new() { Type = "nickname", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow },
                    new() { Type = "preferred_username", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow },
                    new() { Type = "profile", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow },
                    new() { Type = "picture", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow },
                    new() { Type = "website", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow },
                    new() { Type = "Gender", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow },
                    new()
                    {
                        Type = "birthdate", CreatedBy = "HCLCSSFUser", AliasType = "dateofbirth",
                        CreatedOn = DateTime.UtcNow
                    },
                    new() { Type = "zoneinfo", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow },
                    new() { Type = "locale", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow },
                    new() { Type = "updated_at", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow },
                    new() { Type = "City", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow },
                    new() { Type = "PinCode", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow },
                    new() { Type = "Street", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow }
                }
            },
            new()
            {
                Name = "email",
                DisplayName = "EMAIL",
                Description = "email",
                Enabled = true,
                Required = true,
                Emphasize = true,
                CreatedBy = "HCLCSSFUser",
                CreatedOn = DateTime.UtcNow,
                IdentityClaims = new List<IdentityClaims>
                {
                    new() { Type = "email", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow },
                    new()
                    {
                        Type = "email_verified", AliasType = "emailconfirmed", CreatedBy = "HCLCSSFUser",
                        CreatedOn = DateTime.UtcNow
                    }
                }
            },
            new()
            {
                Name = "phone",
                DisplayName = "PHONE",
                Description = "phone",
                Enabled = true,
                Required = true,
                Emphasize = true,
                CreatedBy = "HCLCSSFUser",
                CreatedOn = DateTime.UtcNow,
                IdentityClaims = new List<IdentityClaims>
                {
                    new()
                    {
                        Type = "phone_number", CreatedBy = "HCLCSSFUser", AliasType = "phonenumber",
                        CreatedOn = DateTime.UtcNow
                    },
                    new()
                    {
                        Type = "phone_number_verified", AliasType = "phonenumberconfirmed", CreatedBy = "HCLCSSFUser",
                        CreatedOn = DateTime.UtcNow
                    }
                }
            },
            new()
            {
                Name = "address",
                DisplayName = "ADDRESS",
                Description = "address",
                Enabled = true,
                Required = true,
                Emphasize = true,
                CreatedBy = "HCLCSSFUser",
                CreatedOn = DateTime.UtcNow,
                IdentityClaims = new List<IdentityClaims>
                {
                    new() { Type = "address", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow }
                }
            }
        };
    }

    /// <summary>
    /// Creates the default roles (HCLCSSFAdmin, HCLCSSFUser, rentflow_owner, rentflow_manager, rentflow_resident).
    /// </summary>
    public static List<Roles> CreateRolesMaster()
    {
        return new List<Roles>
        {
            new()
            {
                Description = "HCLCSSFAdmin",
                Name = "HCLCSSFAdmin",
                NormalizedName = "HCLCSSFADMIN",
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                CreatedBy = "HCLCSSFUser",
                CreatedOn = DateTime.UtcNow
            },

            new()
            {
                Description = "HCLCSSFUser",
                Name = "HCLCSSFUser",
                NormalizedName = "HCLCSSFUSER",
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                CreatedBy = "HCLCSSFUser",
                CreatedOn = DateTime.UtcNow
            },

            new()
            {
                Description = "RentFlow owner – full tenant and property management",
                Name = "rentflow_owner",
                NormalizedName = "RENTFLOW_OWNER",
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                CreatedBy = "HCLCSSFUser",
                CreatedOn = DateTime.UtcNow
            },
            new()
            {
                Description = "RentFlow manager – property and occupancy management",
                Name = "rentflow_manager",
                NormalizedName = "RENTFLOW_MANAGER",
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                CreatedBy = "HCLCSSFUser",
                CreatedOn = DateTime.UtcNow
            },
            new()
            {
                Description = "RentFlow resident – limited tenant actions",
                Name = "rentflow_resident",
                NormalizedName = "RENTFLOW_RESIDENT",
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                CreatedBy = "HCLCSSFUser",
                CreatedOn = DateTime.UtcNow
            }
        };
    }

    /// <summary>Creates the full set of role claims for the HCLCSSFAdmin role (full administrative access).</summary>
    public static List<RoleClaims> CreateRoleClaims_HCLCSSFAdmin()
    {
        var RoleClaimsList = new List<RoleClaims>
        {
            new()
            {
                ClaimType = "permission", ClaimValue = "HCL.CS.SF.apiResource.manage", CreatedBy = "HCLCSSFUser",
                CreatedOn = DateTime.UtcNow
            },
            new()
            {
                ClaimType = "permission", ClaimValue = "HCL.CS.SF.identityresource.manage", CreatedBy = "HCLCSSFUser",
                CreatedOn = DateTime.UtcNow
            },
            new()
            {
                ClaimType = "permission", ClaimValue = "HCL.CS.SF.client.manage", CreatedBy = "HCLCSSFUser",
                CreatedOn = DateTime.UtcNow
            },
            new()
            {
                ClaimType = "permission", ClaimValue = "HCL.CS.SF.audittrail.manage", CreatedBy = "HCLCSSFUser",
                CreatedOn = DateTime.UtcNow
            },
            new()
            {
                ClaimType = "permission", ClaimValue = "HCL.CS.SF.role.manage", CreatedBy = "HCLCSSFUser",
                CreatedOn = DateTime.UtcNow
            },
            new()
            {
                ClaimType = "permission", ClaimValue = "HCL.CS.SF.adminuser.manage", CreatedBy = "HCLCSSFUser",
                CreatedOn = DateTime.UtcNow
            },
            new()
            {
                ClaimType = "permission", ClaimValue = "HCL.CS.SF.user.write", CreatedBy = "HCLCSSFUser",
                CreatedOn = DateTime.UtcNow
            },
            new()
            {
                ClaimType = "permission", ClaimValue = "HCL.CS.SF.user.read", CreatedBy = "HCLCSSFUser",
                CreatedOn = DateTime.UtcNow
            },
            new()
            {
                ClaimType = "permission", ClaimValue = "HCL.CS.SF.securitytoken.manage", CreatedBy = "HCLCSSFUser",
                CreatedOn = DateTime.UtcNow
            }
        };
        return RoleClaimsList;
    }

    /// <summary>Creates the role claims for the HCLCSSFUser role (standard user access).</summary>
    public static List<RoleClaims> CreateRoleClaims_HCLCSSFUser()
    {
        var RoleClaimsList = new List<RoleClaims>
        {
            new()
            {
                ClaimType = "permission", ClaimValue = "HCL.CS.SF.role.read", CreatedBy = "HCLCSSFUser",
                CreatedOn = DateTime.UtcNow
            },
            new()
            {
                ClaimType = "permission", ClaimValue = "HCL.CS.SF.user.write", CreatedBy = "HCLCSSFUser",
                CreatedOn = DateTime.UtcNow
            },
            new()
            {
                ClaimType = "permission", ClaimValue = "HCL.CS.SF.user.read", CreatedBy = "HCLCSSFUser",
                CreatedOn = DateTime.UtcNow
            },
            new()
            {
                ClaimType = "permission", ClaimValue = "HCL.CS.SF.user.delete", CreatedBy = "HCLCSSFUser",
                CreatedOn = DateTime.UtcNow
            }
        };
        return RoleClaimsList;
    }

    /// <summary>Capabilities for RentFlow owner role – included in access token when scope includes rentflow.</summary>
    /// <summary>Creates the role claims for the rentflow_owner role.</summary>
    public static List<RoleClaims> CreateRoleClaims_RentFlowOwner()
    {
        var capabilities = new[]
        {
            "health:read", "tenant:read", "tenant:write", "tenant:users:read", "tenant:users:invite",
            "tenant:users:accept", "tenant:users:manage", "property:create", "property:read", "property:floor:add",
            "property:room:add", "property:bed:add", "property:spaces:read", "property:spaces:manage",
            "occupancy:read", "occupancy:bed:read", "occupancy:bed:assign", "occupancy:bed:unassign",
            "resident:create", "resident:read", "meal:read"
        };
        return capabilities.Select(c => new RoleClaims
        {
            ClaimType = "capabilities",
            ClaimValue = c,
            CreatedBy = "HCLCSSFUser",
            CreatedOn = DateTime.UtcNow
        }).ToList();
    }

    /// <summary>Capabilities for RentFlow manager role – included in access token when scope includes rentflow.</summary>
    /// <summary>Creates the role claims for the rentflow_manager role.</summary>
    public static List<RoleClaims> CreateRoleClaims_RentFlowManager()
    {
        var capabilities = new[]
        {
            "health:read", "tenant:read", "tenant:users:read", "tenant:users:invite", "property:read",
            "property:floor:add", "property:room:add", "property:bed:add", "property:spaces:read",
            "property:spaces:manage", "occupancy:read", "occupancy:bed:read", "occupancy:bed:assign",
            "occupancy:bed:unassign", "resident:create", "resident:read", "meal:read"
        };
        return capabilities.Select(c => new RoleClaims
        {
            ClaimType = "capabilities",
            ClaimValue = c,
            CreatedBy = "HCLCSSFUser",
            CreatedOn = DateTime.UtcNow
        }).ToList();
    }

    /// <summary>Capabilities for RentFlow resident role – included in access token when scope includes rentflow.</summary>
    /// <summary>Creates the role claims for the rentflow_resident role.</summary>
    public static List<RoleClaims> CreateRoleClaims_RentFlowResident()
    {
        var capabilities = new[] { "health:read", "tenant:users:accept", "meal:skip", "roomchat:read", "roomchat:send" };
        return capabilities.Select(c => new RoleClaims
        {
            ClaimType = "capabilities",
            ClaimValue = c,
            CreatedBy = "HCLCSSFUser",
            CreatedOn = DateTime.UtcNow
        }).ToList();
    }

    /// <summary>Creates the default set of security questions for account recovery.</summary>
    public static List<SecurityQuestions> CreateSecurityQuestionsModelMaster()
    {
        return new List<SecurityQuestions>
        {
            new()
            {
                Question = "What primary school did you attend?", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow,
                IsDeleted = false
            },
            new()
            {
                Question = "In what town or city was your first full time job?", CreatedBy = "HCLCSSFUser",
                CreatedOn = DateTime.UtcNow, IsDeleted = false
            },
            new()
            {
                Question = "In what town or city did you meet your spouse or partner?", CreatedBy = "HCLCSSFUser",
                CreatedOn = DateTime.UtcNow, IsDeleted = false
            },
            new()
            {
                Question = "What are the last five digits of your driver's license number?", CreatedBy = "HCLCSSFUser",
                CreatedOn = DateTime.UtcNow, IsDeleted = false
            },
            new()
            {
                Question = "What time of the day were you born? (hh:mm)", CreatedBy = "HCLCSSFUser",
                CreatedOn = DateTime.UtcNow, IsDeleted = false
            },
            new()
            {
                Question = "What is the name of the place your wedding reception was held?", CreatedBy = "HCLCSSFUser",
                CreatedOn = DateTime.UtcNow, IsDeleted = false
            },
            new()
            {
                Question = "What is the name of your favorite childhood friend?", CreatedBy = "HCLCSSFUser",
                CreatedOn = DateTime.UtcNow, IsDeleted = false
            },
            new()
            {
                Question = "What was your childhood nickname?", CreatedBy = "HCLCSSFUser", CreatedOn = DateTime.UtcNow,
                IsDeleted = false
            },
            new()
            {
                Question = "What was the last name of your teacher?", CreatedBy = "HCLCSSFUser",
                CreatedOn = DateTime.UtcNow, IsDeleted = false
            }
        };
    }

    /// <summary>Creates a template UserRoles entity whose RoleId and UserId must be set by the caller.</summary>
    public static UserRoles CreateUserRoleModelMaster()
    {
        return new UserRoles
        {
            ValidFrom = DateTime.UtcNow,
            ValidTo = DateTime.UtcNow.AddDays(90),
            CreatedBy = "HCLCSSFUser",
            CreatedOn = DateTime.UtcNow
        };
    }

    /// <summary>Creates a template Users entity with default field values; caller must set username, email, and password hash.</summary>
    public static Users CreateUserModelMaster()
    {
        return new Users
        {
            TwoFactorEnabled = false,
            EmailConfirmed = true,
            PhoneNumberConfirmed = true,
            RequiresDefaultPasswordChange = false,
            CreatedBy = "HCLCSSFUser",
            CreatedOn = DateTime.UtcNow,
            TwoFactorType = TwoFactorType.None,
            SecurityStamp = "6XPYYMZYMSG73X6CMLERAU4PMQ4W4V6"
        };
    }

    /// <summary>Creates a template Clients entity with default OAuth settings; caller must set ClientId, ClientSecret, URIs, and scopes.</summary>
    public static Clients CreateClientMaster()
    {
        return new Clients
        {
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "HCLCSSFUser",
            LogoUri = string.Empty,
            TermsOfServiceUri = string.Empty,
            PolicyUri = string.Empty,

            RefreshTokenExpiration = 86400,
            AccessTokenExpiration = 3600,
            IdentityTokenExpiration = 3600,
            AuthorizationCodeExpiration = 1800,
            LogoutTokenExpiration = 1800,

            AccessTokenType = AccessTokenType.JWT,
            RequirePkce = true,
            IsPkceTextPlain = false,
            RequireClientSecret = true,
            IsFirstPartyApp = true,
            AllowAccessTokensViaBrowser = false,

            AllowedSigningAlgorithm = Algorithms.RsaSha256,
            AllowOfflineAccess = true,
            ApplicationType = ApplicationType.RegularWeb,

            FrontChannelLogoutSessionRequired = true,
            BackChannelLogoutSessionRequired = true
        };
    }
}
