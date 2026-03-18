/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Security.Claims;
using HCL.CS.SF.Domain.Entities.Api;
using HCL.CS.SF.Domain.Models.Api;
using HCL.CS.SF.Domain.Models.Api.Response;

namespace TestApp.Helper.Api;

public static class RoleHelper
{
    public static RoleModel CreateRoleModel()
    {
        var roleModel = new RoleModel
        {
            Description = "To perform security administration for CS Framework",
            Name = "SecurityAdmin",
            CreatedBy = "Test",
            IsDeleted = false,
            RoleClaims = new List<RoleClaimModel>
            {
                new()
                {
                    ClaimType = "LockUser", ClaimValue = "HCL.CS.SF.role.read", CreatedBy = "Test",
                    CreatedOn = DateTime.UtcNow
                }
            }
        };
        return roleModel;
    }

    public static List<RoleModel> CreateRoleModelMaster()
    {
        var roleModelList = new List<RoleModel>
        {
            new()
            {
                Description = "To perform security administration for CS Framework",
                Name = "SecurityAdmin",
                CreatedBy = "Test"
            },

            new()
            {
                Description = "To perform system related settings and configuration",
                Name = "SystemAdmin",
                CreatedBy = "Test"
            },

            new()
            {
                Description = "Only create and update lab test",
                Name = "LabTechnician-1",
                CreatedBy = "Test",
                RoleClaims = new List<RoleClaimModel>
                {
                    new()
                    {
                        ClaimType = "LockUser", ClaimValue = "LockUser", CreatedBy = "Test", CreatedOn = DateTime.UtcNow
                    },
                    new()
                    {
                        ClaimType = "LockUser", ClaimValue = "LockUser", CreatedBy = "Test", CreatedOn = DateTime.UtcNow
                    },
                    new()
                    {
                        ClaimType = "AddEditRole", ClaimValue = "AddEditRole", CreatedBy = "Test",
                        CreatedOn = DateTime.UtcNow
                    },
                    new()
                    {
                        ClaimType = "DeleteRole", ClaimValue = "DeleteRole", CreatedBy = "Test",
                        CreatedOn = DateTime.UtcNow
                    }
                }
            },

            new()
            {
                Description = "Can create and delete lab test",
                Name = "LabTechnician-2",
                CreatedBy = "Test",
                RoleClaims = new List<RoleClaimModel>
                {
                    new()
                    {
                        ClaimType = "LockUser", ClaimValue = "LockUser", CreatedBy = "Test", CreatedOn = DateTime.UtcNow
                    },
                    new()
                    {
                        ClaimType = "DeleteUserRole", ClaimValue = "DeleteUserRole", CreatedBy = "Test",
                        CreatedOn = DateTime.UtcNow
                    },
                    new()
                    {
                        ClaimType = "AddRoleClaim", ClaimValue = "AddRoleClaim", CreatedBy = "Test",
                        CreatedOn = DateTime.UtcNow
                    },
                    new()
                    {
                        ClaimType = "DeleteRoleClaim", ClaimValue = "DeleteRoleClaim", CreatedBy = "Test",
                        CreatedOn = DateTime.UtcNow
                    }
                }
            }
        };
        return roleModelList;
    }

    public static RoleClaimModel CreateRoleClaimModel()
    {
        var roleClaimModel = new RoleClaimModel();
        roleClaimModel.ClaimType = "Permission";
        roleClaimModel.ClaimValue = "HCL.CS.SF.role.read";
        roleClaimModel.RoleId = Guid.NewGuid();
        return roleClaimModel;
    }

    public static RoleClaimModel CreateRoleClaimModelWithNullClaimValue()
    {
        var roleClaimModel = new RoleClaimModel();
        roleClaimModel.ClaimType = "Permission";
        roleClaimModel.ClaimValue = null;
        roleClaimModel.RoleId = Guid.NewGuid();
        return roleClaimModel;
    }

    public static UserRoleModel CreateUserRoleModel()
    {
        var roleModel = new UserRoleModel();

        return roleModel;
    }

    public static UserPermissionsResponseModel CreateUserPermissionsResponseModel()
    {
        var userPermissionsResponseModel = new UserPermissionsResponseModel
        {
            UserId = Guid.NewGuid(),
            RolePermissions = new List<UserRoleClaimsModel>()
        };

        return userPermissionsResponseModel;
    }

    public static Users GetUser()
    {
        var users = new Users
        {
            FirstName = "Test",
            LastName = "Test2",
            IsDeleted = false
        };

        return users;
    }

    public static Users GetUserByName(string userName)
    {
        var users = new Users
        {
            FirstName = "Test",
            LastName = "Test2",
            IsDeleted = false,
            UserName = userName,
            Id = Guid.NewGuid()
        };

        return users;
    }

    public static Users GetUserById(Guid userId)
    {
        var users = new Users
        {
            FirstName = "Test",
            LastName = "Test2",
            IsDeleted = false,
            UserName = "PeterParker",
            Id = userId
        };

        return users;
    }

    public static List<Users> GetUsers()
    {
        var userList = new List<Users>
        {
            new() { FirstName = "Test", LastName = "Test2", IsDeleted = false },
            new() { FirstName = "Test2", LastName = "Test2", IsDeleted = false },
            new() { FirstName = "Test3", LastName = "Test23", IsDeleted = true },
            new() { FirstName = "Test4", LastName = "Test25", IsDeleted = false },
            new() { FirstName = "Test5", LastName = "Test29", IsDeleted = false }
        };
        return userList;
    }

    public static List<RoleClaimModel> CreateRoleClaimModel_SecurityAdmin()
    {
        var roleClaimModelList = new List<RoleClaimModel>
        {
            new() { ClaimType = "LockUser", ClaimValue = "LockUser", CreatedBy = "Test", CreatedOn = DateTime.UtcNow },
            new()
            {
                ClaimType = "AddEditRole", ClaimValue = "AddEditRole", CreatedBy = "Test", CreatedOn = DateTime.UtcNow
            },
            new()
            {
                ClaimType = "DeleteRole", ClaimValue = "DeleteRole", CreatedBy = "Test", CreatedOn = DateTime.UtcNow
            },
            new()
            {
                ClaimType = "AddEditUserRole", ClaimValue = "AddEditUserRole", CreatedBy = "Test",
                CreatedOn = DateTime.UtcNow
            },
            new()
            {
                ClaimType = "DeleteUserRole", ClaimValue = "DeleteUserRole", CreatedBy = "Test",
                CreatedOn = DateTime.UtcNow
            },
            new()
            {
                ClaimType = "AddRoleClaim", ClaimValue = "AddRoleClaim", CreatedBy = "Test", CreatedOn = DateTime.UtcNow
            },
            new()
            {
                ClaimType = "DeleteRoleClaim", ClaimValue = "DeleteRoleClaim", CreatedBy = "Test",
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
                ClaimType = "AddUpdateClient", ClaimValue = "AddUpdateClient", CreatedBy = "Test",
                CreatedOn = DateTime.UtcNow
            },
            new()
            {
                ClaimType = "RevokeToken", ClaimValue = "RevokeToken", CreatedBy = "Test", CreatedOn = DateTime.UtcNow
            },
            new()
            {
                ClaimType = "AddEditNetworkParameters", ClaimValue = "AddEditNetworkParameters", CreatedBy = "Test",
                CreatedOn = DateTime.UtcNow
            }
        };
        return roleClaimModelList;
    }

    public static List<RoleClaimModel> CreateRoleClaimModel_LabTechnician_1()
    {
        var roleClaimModelList = new List<RoleClaimModel>
        {
            new()
            {
                ClaimType = "CreateUpdateLabTest", ClaimValue = "CreateUpdateLabTest", CreatedBy = "Test",
                CreatedOn = DateTime.UtcNow
            }
        };
        return roleClaimModelList;
    }

    public static List<RoleClaimModel> CreateRoleClaimModel_LabTechnician_2()
    {
        var roleClaimModelList = new List<RoleClaimModel>
        {
            new()
            {
                ClaimType = "CreateUpdateLabTest", ClaimValue = "HCL.CS.SF.role.write", CreatedBy = "Test",
                CreatedOn = DateTime.UtcNow
            },
            new()
            {
                ClaimType = "DeleteLabTest", ClaimValue = "HCL.CS.SF.role.delete", CreatedBy = "Test2",
                CreatedOn = DateTime.UtcNow
            },
            new()
            {
                ClaimType = "UpdateLabTest", ClaimValue = "HCL.CS.SF.client.write", CreatedBy = "Test3",
                CreatedOn = DateTime.UtcNow
            }
        };
        return roleClaimModelList;
    }

    //Method to get the userroles for testing
    public static UserRoles GetUserRole()
    {
        var userRoles = new UserRoles
        {
            IsDeleted = false,
            ValidFrom = DateTime.UtcNow,
            ValidTo = DateTime.UtcNow.AddMonths(3),
            CreatedBy = "test"
        };
        return userRoles;
    }

    public static List<UserRoles> GetUserRoles()
    {
        var userModelList = new List<UserRoles>
        {
            new()
            {
                IsDeleted = false,
                ValidFrom = DateTime.UtcNow,
                ValidTo = DateTime.UtcNow.AddMonths(3),
                CreatedBy = "test"
            },
            new()
            {
                IsDeleted = false,
                ValidFrom = DateTime.UtcNow,
                ValidTo = DateTime.UtcNow.AddMonths(3),
                CreatedBy = "test"
            }
        };
        return userModelList;
    }

    public static Roles GetRole()
    {
        return new Roles
        {
            Description = "To perform security administration for CS Framework",
            Name = "SecurityAdmin",
            CreatedBy = "Test",
            IsDeleted = false,
            CreatedOn = DateTime.UtcNow
        };
    }

    public static RoleClaims GetRoleClaim()
    {
        return new RoleClaims
        {
            ClaimType = "CreateUpdateLabTest",
            ClaimValue = "HCL.CS.SF.role.read",
            CreatedBy = "Test",
            CreatedOn = DateTime.UtcNow
        };
    }

    public static List<string> GetRolesForUser()
    {
        var Roles = new List<string>
        {
            "SecurityAdmin",
            "LabTechnician-1",
            "LabTechnician-2"
        };
        return Roles;
    }

    public static List<Users> GetUsersForRole(string roleName)
    {
        return new List<Users>
        {
            new() { FirstName = "Test", LastName = "Test2", IsDeleted = false },
            new() { FirstName = "Test2", LastName = "Test2", IsDeleted = false },
            new() { FirstName = "Test3", LastName = "Test23", IsDeleted = true },
            new() { FirstName = "Test4", LastName = "Test25", IsDeleted = false },
            new() { FirstName = "Test5", LastName = "Test29", IsDeleted = false }
        };
    }

    public static UserModel CreateUserModel()
    {
        var userModel = new UserModel { FirstName = "Test", LastName = "Test2", IsDeleted = false };

        return userModel;
    }

    public static List<UserModel> CreateUserModelList()
    {
        var userModelList = new List<UserModel>
        {
            new() { FirstName = "Test", LastName = "Test2", IsDeleted = false },
            new() { FirstName = "Test2", LastName = "Test2", IsDeleted = false },
            new() { FirstName = "Test3", LastName = "Test23", IsDeleted = true },
            new() { FirstName = "Test4", LastName = "Test25", IsDeleted = false },
            new() { FirstName = "Test5", LastName = "Test29", IsDeleted = false }
        };
        return userModelList;
    }

    public static List<UserRoleModel> CreateUserRoleModelList()
    {
        var userRoleModeList = new List<UserRoleModel>
        {
            new() { UserId = Guid.NewGuid(), RoleId = Guid.NewGuid(), IsDeleted = false },
            new() { UserId = Guid.NewGuid(), RoleId = Guid.NewGuid(), IsDeleted = false },
            new() { UserId = Guid.NewGuid(), RoleId = Guid.NewGuid(), IsDeleted = false }
        };
        return userRoleModeList;
    }

    public static List<UserRoles> CreateUserRoleList()
    {
        var userRoleModeList = new List<UserRoles>
        {
            new() { UserId = Guid.NewGuid(), RoleId = Guid.NewGuid(), IsDeleted = false }
        };
        return userRoleModeList;
    }

    public static List<Claim> GetClaims()
    {
        var claimList = new List<Claim>
        {
            new("LockUser", "LockUser"),
            new("AddEditRole", "AddEditRole"),
            new("DeleteUserRole", "DeleteUserRole")
        };
        return claimList;
    }

    public static List<RoleClaims> GetRoleClaims()
    {
        return new List<RoleClaims>
        {
            new()
            {
                ClaimType = "CreateUpdateLabTest",
                ClaimValue = "CreateUpdateLabTest",
                CreatedBy = "Test",
                CreatedOn = DateTime.UtcNow
            },
            new()
            {
                ClaimType = "DeleteUpdateSystem",
                ClaimValue = "DeleteUpdateSystem",
                CreatedBy = "Test",
                CreatedOn = DateTime.UtcNow
            }
        };
    }
}
