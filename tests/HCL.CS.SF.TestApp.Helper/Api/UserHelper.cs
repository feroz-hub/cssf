/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Security.Claims;
using HCL.CS.SF.Domain.Entities.Api;
using HCL.CS.SF.Domain.Enums;
using HCL.CS.SF.Domain.Models.Api;

namespace TestApp.Helper.Api;

public static class UserHelper
{
    //Constants
    public const string LongText =
        "ASbPBMS1mOxFbyqcnMcxfEZmzi7NQBPKVQI9qRLlHCXJ1hEz6qXYO40huHIUjsdwsp7KJOMFejGSpvpnvc0AYgPTtypq5sBsHSTB1WCYhRYLcgGkELG7D7KvZ1sLscgGbor7TZC2vPfZALRdUclylqzFLGxNDkwJHbWzLBQNChtZvE265Mg3ZqNRb0y3STCpnrRPOyXV3d4T98DGnamjsDha48ce7e6xZvSUDFCgHt4iwypJQbKKdU8I7TLuASosJITzJso1NLq58nk2wza1zcrIeOJOieGFT0OUpYdfDKJe";

    public const UserSecurityQuestionModel nulluserSecurityQuestionModel = null;

    public static UserModel CreateModel()
    {
        var questionModel = new UserSecurityQuestionModel
        {
            Answer = "25"
        };

        var questionModelList = new List<UserSecurityQuestionModel>();
        questionModelList.Add(questionModel);

        var modelList = new List<UserClaimModel>();
        modelList.Add(new UserClaimModel { ClaimType = "Street", ClaimValue = "11 Test street", CreatedBy = "Kamal" });
        modelList.Add(new UserClaimModel { ClaimType = "City", ClaimValue = "Chennai", CreatedBy = "Kamal" });
        modelList.Add(new UserClaimModel { ClaimType = "Pincode", ClaimValue = "635002", CreatedBy = "Kamal" });

        var userModel = new UserModel
        {
            UserName = "BobAlice",
            Email = "jesuarockiana.aruln@HCL.CS.SF.com",
            PhoneNumber = "+919940554097",
            TwoFactorEnabled = false,
            Password = "Test@123",
            FirstName = "Bob Ken",
            LastName = "John Ken",
            DateOfBirth = new DateTime(1990, 1, 1),
            RequiresDefaultPasswordChange = false,
            CreatedBy = "Jan",
            ModifiedBy = "Jan",
            TwoFactorType = TwoFactorType.None,
            IdentityProviderType = IdentityProvider.Local,
            UserSecurityQuestion = questionModelList,
            UserClaims = modelList
        };
        return userModel;
    }

    public static List<UserModel> CreateUserModelMaster()
    {
        //var questionModel = new UserSecurityQuestionModel()
        //{
        //    Answer = "25"
        //};

        //var questionModelList = new List<UserSecurityQuestionModel>();
        //questionModelList.Add(questionModel);

        var modelList = new List<UserClaimModel>();
        modelList.Add(new UserClaimModel { ClaimType = "Street", ClaimValue = "11 Test street", CreatedBy = "Kamal" });
        modelList.Add(new UserClaimModel { ClaimType = "City", ClaimValue = "Chennai", CreatedBy = "Kamal" });
        modelList.Add(new UserClaimModel { ClaimType = "Pincode", ClaimValue = "635002", CreatedBy = "Kamal" });
        var userModelList = new List<UserModel>
        {
            new()
            {
                UserName = "JackRyan",
                Email = "JackRyan@gmail.com",
                EmailConfirmed = false,
                PhoneNumber = "+91234928347",
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = true,
                Password = "Test@123",
                FirstName = "Jack",
                LastName = "Ryan",
                DateOfBirth = new DateTime(1989, 5, 1),
                RequiresDefaultPasswordChange = false,
                CreatedBy = "JR",
                ModifiedBy = "JR",
                IdentityProviderType = IdentityProvider.Local,
                LockoutEnabled = false,
                UserSecurityQuestion = new List<UserSecurityQuestionModel>(),
                UserClaims = modelList
            },

            new()
            {
                UserName = "JacobIsmail",
                Email = "JacobIsmail@gmail.com",
                EmailConfirmed = false,
                PhoneNumber = "+91234928347",
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = true,
                Password = "Test@123",
                FirstName = "Jacob",
                LastName = "Ismail",
                DateOfBirth = new DateTime(1989, 5, 1),
                RequiresDefaultPasswordChange = false,
                CreatedBy = "JR",
                ModifiedBy = "JR",
                IdentityProviderType = IdentityProvider.Local,
                LockoutEnabled = false,
                UserSecurityQuestion = new List<UserSecurityQuestionModel>(),
                UserClaims = modelList
            },

            new()
            {
                UserName = "BobAlice",
                Email = "jesuarockiana.aruln@HCL.CS.SF.com",
                EmailConfirmed = false,
                PhoneNumber = "+919940554097",
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = true,
                Password = "Test@123",
                FirstName = "Bob Ken",
                LastName = "John Ken",
                DateOfBirth = new DateTime(1990, 1, 1),
                RequiresDefaultPasswordChange = false,
                CreatedBy = "Jan",
                ModifiedBy = "Jan",
                IdentityProviderType = IdentityProvider.Local,
                LockoutEnabled = false,
                UserSecurityQuestion = new List<UserSecurityQuestionModel>(),
                UserClaims = modelList
            }
        };
        return userModelList;
    }

    public static SecurityQuestionModel CreateSecurityQuestionModel()
    {
        var questionModel = new SecurityQuestionModel
        {
            Id = Guid.NewGuid(),
            Question = "What is your age ?",
            CreatedBy = "Kamal"
        };
        return questionModel;
    }

    public static List<SecurityQuestionModel> CreateSecurityQuestionModelList()
    {
        var securityQuestionModels = new List<SecurityQuestionModel>
        {
            new()
            {
                Id = new Guid("91DC9EA6-D72D-4878-D58D-08D9B0A30AC8"),
                Question = "What is your first job?",
                CreatedBy = "Test"
            },
            new()
            {
                Id = new Guid("91BC9EA6-D69D-4878-D58D-08D9B0A101C8"),
                Question = "What is your favourite hobby?",
                CreatedBy = "Test"
            },
            new()
            {
                Id = new Guid("91DC9FA5-D72D-4878-D58D-08D9B0A30AC8"),
                Question = "What is your first phonenumber ?",
                CreatedBy = "Test"
            }
        };
        return securityQuestionModels;
    }

    public static UserSecurityQuestionModel CreateUserSecurityQuestionModel()
    {
        var questionModel = new UserSecurityQuestionModel
        {
            UserId = Guid.NewGuid(),
            SecurityQuestionId = Guid.NewGuid(),
            Answer = "YES"
        };
        return questionModel;
    }

    public static List<UserSecurityQuestionModel> CreateUserSecurityQuestionModel_List()
    {
        var userSecurityQuestionModels = new List<UserSecurityQuestionModel>();
        userSecurityQuestionModels.Add(new UserSecurityQuestionModel
        {
            UserId = Guid.NewGuid(),
            SecurityQuestionId = Guid.NewGuid(),
            Answer = "Blacky"
        });
        userSecurityQuestionModels.Add(new UserSecurityQuestionModel
        {
            UserId = Guid.NewGuid(),
            SecurityQuestionId = Guid.NewGuid(),
            Answer = "YES"
        });
        userSecurityQuestionModels.Add(new UserSecurityQuestionModel
        {
            UserId = Guid.NewGuid(),
            SecurityQuestionId = Guid.NewGuid(),
            Answer = "New Zealand"
        });
        return userSecurityQuestionModels;
    }

    public static List<UserSecurityQuestionModel> userSecurityQuestionModelswithLessThanMinQuestions()
    {
        var userSecurityQuestionModels = new List<UserSecurityQuestionModel>
        {
            new() { Id = Guid.NewGuid(), SecurityQuestionId = Guid.NewGuid(), Answer = "25" },
            new() { Id = Guid.NewGuid(), SecurityQuestionId = Guid.NewGuid(), Answer = "Blacky" }
        };
        return userSecurityQuestionModels;
    }

    public static UserClaimModel CreateUserClaimModel()
    {
        var model = new UserClaimModel
        {
            ClaimType = "Gender",
            ClaimValue = "M",
            CreatedBy = "Rosh",
            UserId = Guid.NewGuid()
        };
        return model;
    }

    public static List<UserClaimModel> CreateUserClaimModel_List(Guid userId)
    {
        var modelList = new List<UserClaimModel>();
        //var model = new UserClaimModel { ClaimType = "Gender", ClaimValue = "M" };
        modelList.Add(new UserClaimModel
            { UserId = userId, ClaimType = "Street", ClaimValue = "11 Test street", CreatedBy = "Kamal" });
        modelList.Add(new UserClaimModel
            { UserId = userId, ClaimType = "City", ClaimValue = "Chennai", CreatedBy = "Kamal" });
        modelList.Add(new UserClaimModel
            { UserId = userId, ClaimType = "Pincode", ClaimValue = "635002", CreatedBy = "Kamal" });
        return modelList;
    }

    public static UserModel CreateUser_Authentication()
    {
        var userModel = new UserModel
        {
            UserName = "Authentication",
            Email = "jesuarockiana.aruln@HCL.CS.SF.com",
            EmailConfirmed = false,
            PhoneNumber = "+8680959119",
            PhoneNumberConfirmed = false,
            TwoFactorEnabled = true,
            Password = "Test@123",
            FirstName = "Bob Ken",
            LastName = "John Ken",
            DateOfBirth = new DateTime(1990, 1, 1),
            RequiresDefaultPasswordChange = false,
            CreatedBy = "Jan",
            ModifiedBy = "Jan",
            IdentityProviderType = IdentityProvider.Local,
            LockoutEnabled = false
        };
        return userModel;
    }

    public static Users GetUser()
    {
        var users = new Users
        {
            FirstName = "Peter",
            LastName = "Parker",
            DateOfBirth = new DateTime(2001, 12, 01),
            TwoFactorType = TwoFactorType.Email,
            LastPasswordChangedDate = null,
            RequiresDefaultPasswordChange = false,
            LastLoginDateTime = null,
            LastLogoutDateTime = null,
            IdentityProviderType = IdentityProvider.Local,
            IsDeleted = false,
            CreatedOn = DateTime.UtcNow,
            ModifiedOn = null,
            CreatedBy = "Rosh",
            ModifiedBy = "Steve Ditko",
            Id = new Guid("d060686f-f66b-42b9-5952-08d9e5807a3e"),
            UserName = "PeterParker_698370305",
            NormalizedUserName = "PETERPARKER_698370305",
            Email = "PeterParker_698370305@HCL.CS.SF.com",
            NormalizedEmail = "PETERPARKER_698370305@HCL.CS.SF.COM",
            EmailConfirmed = false,
            PasswordHash = "$argon2i$v=19$m=32768,t=10,p=5$pu4J1dCu7oXljn/LtPvOnA$OzC2FMGNRQJYvs/iV1Eig28Jwvw",
            SecurityStamp = "OMYUZMB4W2YQAPXAHMAIRLADC3HMLTTA",
            ConcurrencyStamp = "fbd46f9a-9d52-416e-a42e-cb56b046ba3a",
            PhoneNumber = "+8680959119",
            PhoneNumberConfirmed = false,
            TwoFactorEnabled = true,
            LockoutEnd = null,
            LockoutEnabled = true,
            AccessFailedCount = 0
        };

        return users;
    }

    public static List<Users> GetUsersByUserName(string userName)
    {
        var userList = new List<Users>
        {
            new() { FirstName = "Test", LastName = "Test2", IsDeleted = true, UserName = userName },
            new() { FirstName = "Test2", LastName = "Test2", IsDeleted = false, UserName = userName }
        };
        return userList;
    }

    public static List<Users> GetUsers()
    {
        var userList = new List<Users>
        {
            new() { FirstName = "Test", LastName = "Test2", IsDeleted = false, UserName = "test" },
            new() { FirstName = "Test2", LastName = "Test2", IsDeleted = false, UserName = "test" }
        };
        return userList;
    }

    public static UserModel CreateUserRequestModel()
    {
        var modelList = new List<UserClaimModel>();
        modelList.Add(new UserClaimModel { ClaimType = "Forward", ClaimValue = "Ronaldo", CreatedBy = "Rosh" });

        var questionList = new List<UserSecurityQuestionModel> { new() };
        var userRequestModel = new UserModel
        {
            UserName = "PeterParker",
            Email = "roshan.bashyam@HCL.CS.SF.com",
            PhoneNumber = "+919820958196",
            TwoFactorEnabled = true,
            TwoFactorType = TwoFactorType.Email,
            Password = "TestUser@2021",
            FirstName = "Peter",
            LastName = "Parker",
            DateOfBirth = new DateTime(2001, 8, 10),
            RequiresDefaultPasswordChange = false,
            CreatedBy = "Stan Lee",
            ModifiedBy = "Steve Ditko",
            IdentityProviderType = IdentityProvider.Local,
            UserSecurityQuestion = questionList,
            UserClaims = modelList
        };

        return userRequestModel;
    }

    public static List<UserModel> CreateUserRequestModelMaster()
    {
        //var questionModel = new UserSecurityQuestionModel()
        //{
        //    Answer = "25"
        //};

        //var questionModelList = new List<UserSecurityQuestionModel>();
        //questionModelList.Add(questionModel);

        var modelList = new List<UserClaimModel>();
        modelList.Add(new UserClaimModel { ClaimType = "Street", ClaimValue = "11 Test street", CreatedBy = "Kamal" });
        modelList.Add(new UserClaimModel { ClaimType = "City", ClaimValue = "Chennai", CreatedBy = "Kamal" });
        modelList.Add(new UserClaimModel { ClaimType = "Pincode", ClaimValue = "635002", CreatedBy = "Kamal" });
        var userModelList = new List<UserModel>
        {
            new()
            {
                UserName = "JackRyan",
                Email = "JackRyan@gmail.com",
                PhoneNumber = "+91234928347",
                TwoFactorEnabled = true,
                Password = "Test@123",
                FirstName = "Jack",
                LastName = "Ryan",
                DateOfBirth = new DateTime(1989, 5, 1),
                RequiresDefaultPasswordChange = false,
                CreatedBy = "RB",
                ModifiedBy = "RB",
                CreatedOn = DateTime.UtcNow,
                IdentityProviderType = IdentityProvider.Local,
                UserSecurityQuestion = new List<UserSecurityQuestionModel>(),
                UserClaims = modelList
            },

            new()
            {
                UserName = "JacobIsmail",
                Email = "JacobIsmail@gmail.com",
                PhoneNumber = "+91234928347",
                TwoFactorEnabled = true,
                Password = "Test@123",
                FirstName = "Jacob",
                LastName = "Ismail",
                DateOfBirth = new DateTime(1989, 5, 1),
                RequiresDefaultPasswordChange = false,
                CreatedBy = "RB",
                ModifiedBy = "RB",
                CreatedOn = DateTime.UtcNow,
                IdentityProviderType = IdentityProvider.Local,
                UserSecurityQuestion = new List<UserSecurityQuestionModel>(),
                UserClaims = modelList
            },

            new()
            {
                UserName = "BobAlice",
                Email = "jesuarockiana.aruln@HCL.CS.SF.com",
                PhoneNumber = "+919940554097",
                TwoFactorEnabled = true,
                Password = "Test@123",
                FirstName = "Bob Ken",
                LastName = "John Ken",
                DateOfBirth = new DateTime(1990, 1, 1),
                RequiresDefaultPasswordChange = false,
                CreatedBy = "RB",
                ModifiedBy = "RB",
                CreatedOn = DateTime.UtcNow,
                IdentityProviderType = IdentityProvider.Local,
                UserSecurityQuestion = new List<UserSecurityQuestionModel>(),
                UserClaims = modelList
            },
            new()
            {
                UserName = "BruceWayne",
                Email = "dark.knight@JL.com",
                PhoneNumber = "+919940554097",
                TwoFactorEnabled = true,
                Password = "Batman@123",
                FirstName = "Bruce",
                LastName = "Wayne",
                DateOfBirth = new DateTime(1938, 4, 17),
                RequiresDefaultPasswordChange = false,
                CreatedBy = "RB",
                ModifiedBy = "RB",
                CreatedOn = DateTime.UtcNow,
                IdentityProviderType = IdentityProvider.Local,
                UserSecurityQuestion = new List<UserSecurityQuestionModel>(),
                UserClaims = modelList
            }
        };
        return userModelList;
    }

    public static UserSecurityQuestions GetUserSecurityQuestions()
    {
        var userSecurityQuestions = new UserSecurityQuestions
        {
            Id = Guid.NewGuid(),
            SecurityQuestion = new SecurityQuestions
            {
                Id = Guid.NewGuid(),
                Question = "Martial Status?",
                CreatedBy = "Test",
                CreatedOn = DateTime.UtcNow
            },
            Answer = "Single",
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "Test"
        };
        return userSecurityQuestions;
    }

    public static UserSecurityQuestions GetUserSecurityQuestionsWithEmptyAnswer()
    {
        var userSecurityQuestions = new UserSecurityQuestions
        {
            Id = Guid.NewGuid(),
            SecurityQuestion = new SecurityQuestions
            {
                Id = Guid.NewGuid(),
                Question = "What is Your Pets Name?",
                CreatedBy = "Test",
                CreatedOn = DateTime.UtcNow
            },
            Answer = string.Empty,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "Test"
        };
        return userSecurityQuestions;
    }

    public static List<UserSecurityQuestions> GetUserSecurityQuestionsList()
    {
        var questionsList = new List<UserSecurityQuestions>
        {
            new()
            {
                Id = Guid.NewGuid(),
                SecurityQuestion = new SecurityQuestions
                {
                    Id = Guid.NewGuid(),
                    Question = "What is Your Pets Name?",
                    CreatedBy = "Test",
                    CreatedOn = DateTime.UtcNow
                },
                Answer = "Scooby-Doo",
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "Test"
            },
            new()
            {
                Id = Guid.NewGuid(),
                SecurityQuestion = new SecurityQuestions
                {
                    Id = Guid.NewGuid(),
                    Question = "What is Your Favourite Location?",
                    CreatedBy = "Test",
                    CreatedOn = DateTime.UtcNow
                },
                Answer = "Nirvana Land",
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "Test"
            },
            new()
            {
                Id = Guid.NewGuid(),
                SecurityQuestion = new SecurityQuestions
                {
                    Id = Guid.NewGuid(),
                    Question = "What is your favourite drink?",
                    CreatedBy = "Test",
                    CreatedOn = DateTime.UtcNow
                },
                Answer = "w@Ter$",
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "Test"
            },
            new()
            {
                Id = Guid.NewGuid(),
                SecurityQuestion = new SecurityQuestions
                {
                    Id = Guid.NewGuid(),
                    Question = "Where is your location",
                    CreatedBy = "Test",
                    CreatedOn = DateTime.UtcNow
                },
                Answer = "Wonder Land",
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "Test"
            }
        };
        return questionsList;
    }

    //UserClaims
    public static UserClaims GetUserClaims()
    {
        var userClaims = new UserClaims
        {
            ClaimType = "forward",
            ClaimValue = "Ronaldo",
            CreatedBy = "Rosh",
            Id = new Random().Next()
        };
        return userClaims;
    }

    public static List<Claim> GetClaimsList()
    {
        var claimList = new List<Claim>
        {
            new("LockUser", "LockUser"),
            new("AddEditRole", "AddEditRole"),
            new("DeleteUserRole", "DeleteUserRole")
        };
        return claimList;
    }

    public static List<UserClaims> GetUserClaimsList()
    {
        var claimList = new List<UserClaims>
        {
            new()
            {
                ClaimType = "UserClaimType.Permission", ClaimValue = "user.CreateUser", CreatedBy = "Rosh",
                CreatedOn = DateTime.UtcNow, UserId = Guid.NewGuid()
            },
            new()
            {
                ClaimType = "UserClaimType.Permission", ClaimValue = "user.UpdateUser", CreatedBy = "Rosh",
                CreatedOn = DateTime.UtcNow, UserId = Guid.NewGuid()
            },
            new()
            {
                ClaimType = "UserClaimType.Permission", ClaimValue = "user.DeleteUser", CreatedBy = "Rosh",
                CreatedOn = DateTime.UtcNow, UserId = Guid.NewGuid()
            }
        };
        return claimList;
    }

    public static UserClaimModel GetExistingUserClaimModel()
    {
        var model = new UserClaimModel
        {
            ClaimType = "Gender",
            ClaimValue = "M",
            CreatedBy = "Rosh",
            Id = new Random().Next(),
            UserId = Guid.NewGuid()
        };
        return model;
    }

    public static UserClaimModel GetNewUserClaimModel()
    {
        var model = new UserClaimModel
        {
            ClaimType = "Marital Status",
            ClaimValue = "Single",
            CreatedBy = "Rosh",
            UserId = Guid.NewGuid(),
            Id = new Random().Next()
        };
        return model;
    }

    public static SecurityQuestions GetSecurityQuestions()
    {
        var securityQuestions = new SecurityQuestions
        {
            Id = new Guid("91DC9EA6-D72D-4878-D58D-08D9B0A30AC8"),
            Question = "What is your first phonenumber ?",
            CreatedBy = "Test"
        };
        return securityQuestions;
    }

    public static List<SecurityQuestions> GetSecurityQuestionsList()
    {
        var securityQuestions = new List<SecurityQuestions>
        {
            new()
            {
                Id = new Guid("91DC9EA6-D72D-4878-D58D-08D9B0A30AC8"),
                Question = "What is your first job?",
                CreatedBy = "Test"
            },
            new()
            {
                Id = new Guid("91BC9EA6-D69D-4878-D58D-08D9B0A101C8"),
                Question = "What is your favourite hobby?",
                CreatedBy = "Test"
            },
            new()
            {
                Id = new Guid("91DC9FA5-D72D-4878-D58D-08D9B0A30AC8"),
                Question = "What is your first phonenumber ?",
                CreatedBy = "Test"
            }
        };
        return securityQuestions;
    }

    public static UserModel CreateUserRequestModelForUserRole()
    {
        var modelList = new List<UserClaimModel>();
        modelList.Add(new UserClaimModel { ClaimType = "Read", ClaimValue = "Read.Email", CreatedBy = "Rosh" });
        modelList.Add(new UserClaimModel { ClaimType = "Write", ClaimValue = "Write.Email", CreatedBy = "Rosh" });
        modelList.Add(new UserClaimModel { ClaimType = "Access", ClaimValue = "Access.All", CreatedBy = "Rosh" });

        var questionList = new List<UserSecurityQuestionModel>
        {
            new()
            {
                CreatedBy = "Rosh"
            }
        };
        var userRequestModel = new UserModel
        {
            UserName = "PeterParker",
            Email = "roshan.bashyam@HCL.CS.SF.com",
            PhoneNumber = "+919820958196",
            TwoFactorEnabled = true,
            TwoFactorType = TwoFactorType.Email,
            Password = "TestUser@2021",
            FirstName = "Peter",
            LastName = "Parker",
            DateOfBirth = new DateTime(2001, 8, 10),
            RequiresDefaultPasswordChange = false,
            CreatedBy = "Stan Lee",
            ModifiedBy = "Steve Ditko",
            IdentityProviderType = IdentityProvider.Local,
            UserSecurityQuestion = questionList,
            UserClaims = modelList,
            EmailConfirmed = true,
            PhoneNumberConfirmed = true,
            LockoutEnabled = true,
            LockoutEnd = DateTime.UtcNow,
            AccessFailedCount = 3
        };

        return userRequestModel;
    }
}
