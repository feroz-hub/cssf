/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Microsoft.Extensions.DependencyInjection;
using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Enums;
using HCL.CS.SF.Domain.Models.Api;
using HCL.CS.SF.Domain.Models.Endpoint;
using HCL.CS.SF.DomainServices.Seed;
using HCL.CS.SF.Service.Interfaces.Interfaces.Api;

namespace IntegrationTests.DataSeed;

public class SeedCreator : HCLCSSFFakeSetup, ISeedCreator
{
    private readonly List<SeedModel> seedModels;
    //private IServiceProvider serviceProvider;

    public SeedCreator()
    {
        seedModels = new List<SeedModel>();
        CreateSeedModels();
        //this.serviceProvider = serviceProvider;
    }

    public async Task<bool> AddSeedModelsAsync(int orderNumber, BaseModel model)
    {
        return await Task.Run(() =>
        {
            var returnValue = true;
            try
            {
                seedModels.Add(new SeedModel { OrderNumber = 1, Model = model });
            }
            catch (Exception)
            {
                returnValue = false;
            }

            return returnValue;
        });
    }

    public async Task<FrameworkResult> CreateMasterDataAsync()
    {
        FrameworkResult result = null;
        foreach (var modelClass in seedModels.OrderBy(x => x.OrderNumber))
        {
            Type type = null;
            if (modelClass.Model != null)
                type = modelClass.Model.GetType();
            else if (modelClass.TrailModel != null) type = modelClass.TrailModel.GetType();

            if (type == typeof(SecurityQuestionModel))
                result = await SeedSecurityQuestions();
            else if (type == typeof(UserModel))
                result = await SeedUsers();
            else if (type == typeof(RoleModel))
                result = await SeedRoles();
            else if (type == typeof(RoleClaimModel))
                result = await SeedRoleClaims();
            else if (type == typeof(IdentityResourcesModel))
                result = await SeedIdentityResources();
            else if (type == typeof(ApiResourcesModel))
                result = await SeedApiResources();
            else if (type == typeof(ClientsModel))
                result = await SeedClients();
            else if (type == typeof(UserRoleModel)) result = await SeedUserRole();

            if (result != null && result.Status == ResultStatus.Failed) break;
        }

        return result;
    }

    private void CreateSeedModels()
    {
        seedModels.Add(new SeedModel { OrderNumber = 1, Model = new SecurityQuestionModel() });
        seedModels.Add(new SeedModel { OrderNumber = 2, Model = new UserModel() });
        seedModels.Add(new SeedModel { OrderNumber = 3, Model = new RoleModel() });
        seedModels.Add(new SeedModel { OrderNumber = 4, TrailModel = new RoleClaimModel() });
        seedModels.Add(new SeedModel { OrderNumber = 5, Model = new UserRoleModel() });
        seedModels.Add(new SeedModel { OrderNumber = 6, Model = new IdentityResourcesModel() });
        seedModels.Add(new SeedModel { OrderNumber = 7, Model = new ApiResourcesModel() });
        seedModels.Add(new SeedModel { OrderNumber = 8, Model = new ClientsModel() });
    }

    public async Task<FrameworkResult> SeedUsers()
    {
        FrameworkResult result = null;
        var userServices = ServiceProvider.GetService<IUserAccountService>();
        var userModelList = IntegrationDataSeed.CreateUserModelMaster();
        foreach (var userModel in userModelList)
        {
            userModel.TwoFactorType = TwoFactorType.Email;
            var userquestionList = await userServices.GetAllSecurityQuestionsAsync();
            var questionModelList = new List<UserSecurityQuestionModel>();
            foreach (var question in userquestionList)
            {
                var userSecurityQuestion = IntegrationDataSeed.CreateUserSecurityQuestionModelMaster();
                userSecurityQuestion.SecurityQuestionId = question.Id;
                questionModelList.Add(userSecurityQuestion);
            }

            userModel.UserSecurityQuestion = questionModelList;
            result = await userServices.RegisterUserAsync(userModel);
        }

        return result;
    }

    public async Task<FrameworkResult> SeedSecurityQuestions()
    {
        FrameworkResult result = null;
        var userServices = ServiceProvider.GetService<IUserAccountService>();
        var questionsList = IntegrationDataSeed.CreateSecurityQuestionModelMaster();
        foreach (var question in questionsList) result = await userServices.AddSecurityQuestionAsync(question);

        return result;
    }

    private async Task<FrameworkResult> SeedUserRole()
    {
        var userModelList = IntegrationDataSeed.CreateUserModelMaster();
        var roleModelList = IntegrationDataSeed.CreateRoleModelMaster();
        var userServices = ServiceProvider.GetService<IUserAccountService>();
        var roleService = ServiceProvider.GetService<IRoleService>();

        // Get user by username
        var user1 = await userServices.GetUserByNameAsync(userModelList[0].UserName);
        var user2 = await userServices.GetUserByNameAsync(userModelList[1].UserName);

        var role1 = await roleService.GetRoleAsync(roleModelList[0].Name);
        var role2 = await roleService.GetRoleAsync(roleModelList[1].Name);
        var role3 = await roleService.GetRoleAsync(roleModelList[2].Name);
        var role4 = await roleService.GetRoleAsync(roleModelList[3].Name);

        var userRoleMapping1 = new UserRoleModel();
        userRoleMapping1.UserId = user1.Id;
        userRoleMapping1.RoleId = role1.Id;
        userRoleMapping1.CreatedBy = "Suresh";
        var addUserRoleResult1 = await userServices.AddUserRoleAsync(userRoleMapping1);

        var userRoleMapping2 = new UserRoleModel();
        userRoleMapping2.UserId = user1.Id;
        userRoleMapping2.RoleId = role2.Id;
        userRoleMapping2.CreatedBy = "Suresh";
        var addUserRoleResult2 = await userServices.AddUserRoleAsync(userRoleMapping2);

        var userRoleMapping3 = new UserRoleModel();
        userRoleMapping3.UserId = user2.Id;
        userRoleMapping3.RoleId = role3.Id;
        userRoleMapping3.CreatedBy = "Suresh";
        var addUserRoleResult3 = await userServices.AddUserRoleAsync(userRoleMapping3);

        var userRoleMapping4 = new UserRoleModel();
        userRoleMapping4.UserId = user2.Id;
        userRoleMapping4.RoleId = role4.Id;
        userRoleMapping4.CreatedBy = "Suresh";
        var addUserRoleResult4 = await userServices.AddUserRoleAsync(userRoleMapping4);

        return null;
    }

    public async Task<FrameworkResult> SeedRoles()
    {
        FrameworkResult result = null;
        var roleServices = ServiceProvider.GetService<IRoleService>();
        var roleList = IntegrationDataSeed.CreateRoleModelMaster();
        foreach (var role in roleList) result = await roleServices.CreateRoleAsync(role);

        return result;
    }

    public async Task<FrameworkResult> SeedRoleClaims()
    {
        FrameworkResult result = null;
        var roleServices = ServiceProvider.GetService<IRoleService>();
        var roleClaimListRoleAdmin = IntegrationDataSeed.CreateRoleClaimModel_RoleAdmin();
        var roleClaimListClientAdmin = IntegrationDataSeed.CreateRoleClaimModel_ClientAdmin();
        var roleClaimListGeneralUser = IntegrationDataSeed.CreateRoleClaimModel_GeneralUser();
        var roleClaimListResourceAdmin = IntegrationDataSeed.CreateRoleClaimModel_ResourceAdmin();
        var roleClaimListSystemAdmin = IntegrationDataSeed.CreateRoleClaimModel_SystemAdmin();

        var sysAdminRole = await roleServices.GetRoleAsync("SystemAdmin");
        foreach (var roleClaimModel in roleClaimListSystemAdmin)
        {
            roleClaimModel.RoleId = sysAdminRole.Id;
            result = await roleServices.AddRoleClaimAsync(roleClaimModel);
        }

        var securityAdminRole = await roleServices.GetRoleAsync("RoleAdmin");
        foreach (var roleClaimModel in roleClaimListRoleAdmin)
        {
            roleClaimModel.RoleId = securityAdminRole.Id;
            result = await roleServices.AddRoleClaimAsync(roleClaimModel);
        }

        var labtech1Role = await roleServices.GetRoleAsync("ClientAdmin");
        foreach (var roleClaimModel in roleClaimListClientAdmin)
        {
            roleClaimModel.RoleId = labtech1Role.Id;
            result = await roleServices.AddRoleClaimAsync(roleClaimModel);
        }

        var labtech2Role = await roleServices.GetRoleAsync("GeneralUser");
        foreach (var roleClaimModel in roleClaimListGeneralUser)
        {
            roleClaimModel.RoleId = labtech2Role.Id;
            result = await roleServices.AddRoleClaimAsync(roleClaimModel);
        }

        var labtech3Role = await roleServices.GetRoleAsync("ResourceAdmin");
        foreach (var roleClaimModel in roleClaimListResourceAdmin)
        {
            roleClaimModel.RoleId = labtech3Role.Id;
            result = await roleServices.AddRoleClaimAsync(roleClaimModel);
        }

        return result;
    }

    private async Task<FrameworkResult> SeedIdentityResources()
    {
        FrameworkResult result = null;

        var identityResourceService = ServiceProvider.GetService<IIdentityResourceService>();

        var identityResourceModelList = IntegrationDataSeed.CreateIdentityResourceModelMaster();
        foreach (var identityResource in identityResourceModelList)
            result = await identityResourceService.AddIdentityResourceAsync(identityResource);
        return result;
    }

    private async Task<FrameworkResult> SeedApiResources()
    {
        FrameworkResult result = null;

        var apiResourceService = ServiceProvider.GetService<IApiResourceService>();
        var apiResourceModelList = IntegrationDataSeed.CreateApiResourceModelMaster();
        foreach (var apiResource in apiResourceModelList)
            result = await apiResourceService.AddApiResourceAsync(apiResource);

        return result;
    }

    private async Task<FrameworkResult> SeedClients()
    {
        FrameworkResult result = null;
        var clientService = ServiceProvider.GetService<IClientServices>();
        var clientModelList = IntegrationDataSeed.CreateClientMaster();
        foreach (var client in clientModelList)
        {
            var clienmodel = await clientService.RegisterClientAsync(client);
        }

        return result;
    }
}

public class SeedModel
{
    public virtual int OrderNumber { get; set; }
    public virtual BaseModel Model { get; set; }
    public virtual BaseTrailModel TrailModel { get; set; }
}
