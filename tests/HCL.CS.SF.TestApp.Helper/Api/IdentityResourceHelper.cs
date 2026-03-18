/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain.Entities.Api;
using HCL.CS.SF.Domain.Models.Api;

namespace TestApp.Helper.Api;

public static class IdentityResourceHelper
{
    public static List<IdentityResourcesModel> GetIdentityResourceModelMasterData()
    {
        var identityResources = new List<IdentityResourcesModel>();

        var identityResource1 = new IdentityResourcesModel
        {
            Name = "OpenId",
            DisplayName = "Your user identifier",
            Description = "Your user identifier",
            Enabled = true,
            Required = true,
            IdentityClaims = new List<IdentityClaimsModel>
            {
                new() { Type = "subject" }
            }
        };

        var identityResource2 = new IdentityResourcesModel
        {
            Name = "Profile",
            DisplayName = "User profile",
            Description = "Your user profile information (first name, last name, etc.)",
            Enabled = true,
            Emphasize = true,
            IdentityClaims = new List<IdentityClaimsModel>
            {
                new() { Type = "name" },
                new() { Type = "family_name" },
                new() { Type = "given_name" },
                new() { Type = "middle_name" },
                new() { Type = "nickname" },
                new() { Type = "preferred_username" },
                new() { Type = "profile" },
                new() { Type = "picture" },
                new() { Type = "website" },
                new() { Type = "Gender" },
                new() { Type = "birthdate" },
                new() { Type = "zoneinfo" },
                new() { Type = "locale" },
                new() { Type = "updated_at" },
                new() { Type = "City" },
                new() { Type = "PinCode" },
                new() { Type = "Street" }
            }
        };

        var identityResource3 = new IdentityResourcesModel
        {
            Name = "Email",
            DisplayName = "Your email address",
            Description = "Your email address",
            Enabled = true,
            Emphasize = true,
            IdentityClaims = new List<IdentityClaimsModel>
            {
                new() { Type = "email" },
                new() { Type = "email_verified" }
            }
        };

        var identityResource4 = new IdentityResourcesModel
        {
            Name = "Phone",
            DisplayName = "Your phone number",
            Description = "Your phone number",
            Enabled = true,
            Emphasize = true,
            IdentityClaims = new List<IdentityClaimsModel>
            {
                new() { Type = "phone_number" },
                new() { Type = "phone_number_verified" }
            }
        };

        var identityResource5 = new IdentityResourcesModel
        {
            Name = "Address",
            DisplayName = "Your postal address",
            Description = "Your postal address",
            Enabled = true,
            Emphasize = true,
            IdentityClaims = new List<IdentityClaimsModel>
            {
                new() { Type = "address" }
            }
        };

        identityResources.Add(identityResource1);
        identityResources.Add(identityResource2);
        identityResources.Add(identityResource3);
        identityResources.Add(identityResource4);
        identityResources.Add(identityResource5);
        return identityResources;
    }

    public static IdentityResourcesModel CreateIdentityResourceModel()
    {
        var identityResourceModel = new IdentityResourcesModel
        {
            Name = "IDResource",
            DisplayName = "TestDisplayName1 - IDResource1",
            Description = "This is 1 test Description -IDResource1",
            Enabled = false,
            CreatedBy = "Test",
            IdentityClaims = new List<IdentityClaimsModel>
            {
                new() { Type = "Type1", CreatedBy = "Test" },
                new() { Type = "Type2", CreatedBy = "Test" },
                new() { Type = "Type3", CreatedBy = "Test" }
            }
        };
        return identityResourceModel;
    }

    public static IdentityResourcesModel UpdateIdentityResourceModel()
    {
        var identityResourceModel = new IdentityResourcesModel
        {
            Name = "IDResource",
            DisplayName = "TestDisplayName2 - IDResource2",
            Description = "This is 1 test Description -IDResource2",
            Enabled = false,
            IdentityClaims = new List<IdentityClaimsModel>
            {
                new() { Type = "Type4" },
                new() { Type = "Type5" },
                new() { Type = "Type6" }
            }
        };
        return identityResourceModel;
    }

    public static IdentityClaimsModel CreateIdentityResourceClaimModel()
    {
        var identityResourceClaimsModel = new IdentityClaimsModel
        {
            IdentityResourceId = Guid.NewGuid(),
            CreatedBy = "Test",
            Type = "Test Claim"
        };
        return identityResourceClaimsModel;
    }

    public static List<IdentityClaimsModel> CreateIdentityResourceClaimModelList()
    {
        var identityResourceClaimsModel = new List<IdentityClaimsModel>
        {
            new()
            {
                IdentityResourceId = Guid.NewGuid(),
                CreatedBy = "Test",
                Type = "email"
            },
            new()
            {
                IdentityResourceId = Guid.NewGuid(),
                CreatedBy = "Test",
                Type = "email_Verify"
            }
        };
        return identityResourceClaimsModel;
    }

    public static IdentityClaims GetIdentityClaims()
    {
        var identityClaims = new IdentityClaims
        {
            Id = Guid.NewGuid(),
            IdentityResourceId = Guid.NewGuid(),
            Type = "email",
            CreatedBy = "RB"
        };
        return identityClaims;
    }

    public static List<IdentityClaims> GetIdentityClaimsList()
    {
        var identityClaimsList = new List<IdentityClaims>
        {
            new()
            {
                Id = Guid.NewGuid(),
                IdentityResourceId = Guid.NewGuid(),
                Type = "email",
                CreatedBy = "RB"
            },
            new()
            {
                Id = Guid.NewGuid(),
                IdentityResourceId = Guid.NewGuid(),
                Type = "email_verify",
                CreatedBy = "RB"
            }
        };
        return identityClaimsList;
    }

    public static List<IdentityResources> GetIdentityResourcesList()
    {
        var identityResourcesList = new List<IdentityResources>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Required = true,
                Enabled = true,
                DisplayName = "TestDemo",
                CreatedBy = "RB",
                CreatedOn = DateTime.UtcNow,
                IdentityClaims = new List<IdentityClaims>
                {
                    new() { Type = "Type1", CreatedBy = "RB" },
                    new() { Type = "Type2", CreatedBy = "RB" }
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                Required = true,
                Enabled = true,
                DisplayName = "TestDemo2",
                CreatedBy = "RB",
                CreatedOn = DateTime.UtcNow,
                IdentityClaims = new List<IdentityClaims>
                {
                    new() { Type = "Type3", CreatedBy = "RB" },
                    new() { Type = "Type4", CreatedBy = "RB" }
                }
            }
        };
        return identityResourcesList;
    }
}
