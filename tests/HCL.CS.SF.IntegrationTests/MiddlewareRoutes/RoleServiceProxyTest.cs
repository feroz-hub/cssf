/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Net.Http.Headers;
using System.Text;
using FluentAssertions;
using IntegrationTests.ApiDomainModel;
using Newtonsoft.Json;
using TestApp.Helper.Api;
using Xunit;
using HCL.CS.SF.Domain.Constants;

namespace IntegrationTests.MiddlewareRoutes;

public class RoleServiceProxyTest : HCLCSSFFakeSetup
{
    private readonly Random random = new();
    public string roleName = "IntegrationTestRole";

    [Fact]
    public async Task CreateRole_Success()
    {
        // Add.
        var roleModelInput = RoleHelper.CreateRoleModel();
        roleModelInput.Name = roleName + random.Next();

        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);

        var addroleurl = BaseUrl + ApiRoutePathConstants.CreateRole;
        var response = await FrontChannelClient.PostAsync(
            addroleurl,
            new StringContent(JsonConvert.SerializeObject(roleModelInput), Encoding.UTF8, "application/json"));
        var lockUserResopnse = await response.Content.ReadAsStringAsync();
        var frameworkResult = JsonConvert.DeserializeObject<FrameworkResult>(lockUserResopnse);
        frameworkResult.Should().BeOfType<FrameworkResult>();
        frameworkResult.Status.Should().Be(ResultStatus.Success);
        roleName = roleModelInput.Name;
    }

    [Fact]
    public async Task CreateRoleAsync_DuplicateRoleExists_ReturnsError_RoleAlreadyExists()
    {
        // Add.
        var roleModelInput = RoleHelper.CreateRoleModel();
        roleModelInput.Name = roleName + random.Next();

        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);

        var addroleurl = BaseUrl + ApiRoutePathConstants.CreateRole;
        var response = await FrontChannelClient.PostAsync(
            addroleurl,
            new StringContent(JsonConvert.SerializeObject(roleModelInput), Encoding.UTF8, "application/json"));

        response = await FrontChannelClient.PostAsync(
            addroleurl,
            new StringContent(JsonConvert.SerializeObject(roleModelInput), Encoding.UTF8, "application/json"));

        var lockUserResopnse = await response.Content.ReadAsStringAsync();
        var frameworkResult = JsonConvert.DeserializeObject<FrameworkResult>(lockUserResopnse);
        frameworkResult.Should().BeOfType<FrameworkResult>();
        frameworkResult.Should().BeOfType<FrameworkResult>();
        frameworkResult.Status.Should().Be(ResultStatus.Failed);
        frameworkResult.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.RoleAlreadyExists);
    }

    [Fact]
    public async Task CreateRoleAsync_NameLengthExceedsLimit_ReturnsError_ThrowsException()
    {
        // Add.
        var roleModelInput = RoleHelper.CreateRoleModel();
        roleModelInput.Name =
            "hhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllll";

        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);

        var addroleurl = BaseUrl + ApiRoutePathConstants.CreateRole;
        var response = await FrontChannelClient.PostAsync(
            addroleurl,
            new StringContent(JsonConvert.SerializeObject(roleModelInput), Encoding.UTF8, "application/json"));

        var lockUserResopnse = await response.Content.ReadAsStringAsync();
        var frameworkResult = JsonConvert.DeserializeObject<FrameworkResult>(lockUserResopnse);
        frameworkResult.Should().BeOfType<FrameworkResult>();
        frameworkResult.Should().BeOfType<FrameworkResult>();
        frameworkResult.Status.Should().Be(ResultStatus.Failed);
        frameworkResult.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.RoleNameTooLong);
    }

    [Fact]
    public async Task CreateRoleAsync_NameNull_ReturnsError_ThrowsException()
    {
        // Add.
        var roleModelInput = RoleHelper.CreateRoleModel();
        roleModelInput.Name = null;

        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);

        var addroleurl = BaseUrl + ApiRoutePathConstants.CreateRole;
        var response = await FrontChannelClient.PostAsync(
            addroleurl,
            new StringContent(JsonConvert.SerializeObject(roleModelInput), Encoding.UTF8, "application/json"));

        var lockUserResopnse = await response.Content.ReadAsStringAsync();
        var frameworkResult = JsonConvert.DeserializeObject<FrameworkResult>(lockUserResopnse);
        frameworkResult.Should().BeOfType<FrameworkResult>();
        frameworkResult.Should().BeOfType<FrameworkResult>();
        frameworkResult.Status.Should().Be(ResultStatus.Failed);
        frameworkResult.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.InvalidRoleName);
    }

    [Fact]
    public async Task CreateRoleAsync_InvalidRoleName_ReturnsError_ThrowsException()
    {
        // Add.
        var roleModelInput = RoleHelper.CreateRoleModel();
        roleModelInput.Name = string.Empty;

        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);

        var addroleurl = BaseUrl + ApiRoutePathConstants.CreateRole;
        var response = await FrontChannelClient.PostAsync(
            addroleurl,
            new StringContent(JsonConvert.SerializeObject(roleModelInput), Encoding.UTF8, "application/json"));

        var lockUserResopnse = await response.Content.ReadAsStringAsync();
        var frameworkResult = JsonConvert.DeserializeObject<FrameworkResult>(lockUserResopnse);
        frameworkResult.Should().BeOfType<FrameworkResult>();
        frameworkResult.Should().BeOfType<FrameworkResult>();
        frameworkResult.Status.Should().Be(ResultStatus.Failed);
        frameworkResult.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.InvalidRoleName);
    }

    [Fact]
    public async Task Update_Role_Success()
    {
        // Add.
        await CreateRole_Success();

        // Get Role
        var getRoleurl = BaseUrl + ApiRoutePathConstants.GetRoleByName;
        var response = await FrontChannelClient.PostAsync(
            getRoleurl,
            new StringContent(JsonConvert.SerializeObject(roleName), Encoding.UTF8, "application/json"));
        var roleResponse = await response.Content.ReadAsStringAsync();
        var roleModel = JsonConvert.DeserializeObject<RoleModel>(roleResponse);

        roleModel.Description = "TestProxy";

        // Update Role
        var updateRoleurl = BaseUrl + ApiRoutePathConstants.UpdateRole;
        var updateIdentityresponse = await FrontChannelClient.PostAsync(
            updateRoleurl,
            new StringContent(JsonConvert.SerializeObject(roleModel), Encoding.UTF8, "application/json"));
        var updateIdentityResponse = await updateIdentityresponse.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<FrameworkResult>(updateIdentityResponse);
        result.Should().BeOfType<FrameworkResult>();
        result.Status.Should().Be(ResultStatus.Success);
    }

    [Fact]
    public async Task Update_InvalidNameLength_ReturnFail()
    {
        // Add.
        await CreateRole_Success();

        // Get Role
        var getRoleurl = BaseUrl + ApiRoutePathConstants.GetRoleByName;
        var response = await FrontChannelClient.PostAsync(
            getRoleurl,
            new StringContent(JsonConvert.SerializeObject(roleName), Encoding.UTF8, "application/json"));
        var roleResponse = await response.Content.ReadAsStringAsync();
        var roleModel = JsonConvert.DeserializeObject<RoleModel>(roleResponse);

        roleModel.Name =
            "hhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllll";

        // Update Role
        var updateRoleurl = BaseUrl + ApiRoutePathConstants.UpdateRole;
        var updateIdentityresponse = await FrontChannelClient.PostAsync(
            updateRoleurl,
            new StringContent(JsonConvert.SerializeObject(roleModel), Encoding.UTF8, "application/json"));
        var updateIdentityResponse = await updateIdentityresponse.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<FrameworkResult>(updateIdentityResponse);
        result.Status.Should().Be(ResultStatus.Failed);
        result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.RoleNameTooLong);
    }

    [Fact]
    public async Task Update_InvalidRole_ReturnFail()
    {
        // Add.
        await CreateRole_Success();

        // Get Role
        var getRoleurl = BaseUrl + ApiRoutePathConstants.GetRoleByName;
        var response = await FrontChannelClient.PostAsync(
            getRoleurl,
            new StringContent(JsonConvert.SerializeObject(roleName), Encoding.UTF8, "application/json"));
        var roleResponse = await response.Content.ReadAsStringAsync();
        var roleModel = JsonConvert.DeserializeObject<RoleModel>(roleResponse);

        roleModel.Id = Guid.NewGuid();

        // Update Role
        var updateRoleurl = BaseUrl + ApiRoutePathConstants.UpdateRole;
        var updateIdentityresponse = await FrontChannelClient.PostAsync(
            updateRoleurl,
            new StringContent(JsonConvert.SerializeObject(roleModel), Encoding.UTF8, "application/json"));
        var updateIdentityResponse = await updateIdentityresponse.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<FrameworkResult>(updateIdentityResponse);
        result.Should().BeOfType<FrameworkResult>();
        result.Status.Should().Be(ResultStatus.Failed);
    }

    [Fact]
    public async Task Delete_RoleById_Success()
    {
        // Add.
        await CreateRole_Success();

        // Get Role
        var getRoleurl = BaseUrl + ApiRoutePathConstants.GetRoleByName;
        var response = await FrontChannelClient.PostAsync(
            getRoleurl,
            new StringContent(JsonConvert.SerializeObject(roleName), Encoding.UTF8, "application/json"));
        var roleResponse = await response.Content.ReadAsStringAsync();
        var roleModel = JsonConvert.DeserializeObject<RoleModel>(roleResponse);

        // Delete Role
        var deleteRoleurl = BaseUrl + ApiRoutePathConstants.DeleteRoleById;
        var deleteresponse = await FrontChannelClient.PostAsync(
            deleteRoleurl,
            new StringContent(JsonConvert.SerializeObject(roleModel.Id), Encoding.UTF8, "application/json"));
        var deleteRoleResponse = await deleteresponse.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<FrameworkResult>(deleteRoleResponse);
        result.Should().BeOfType<FrameworkResult>();
        result.Status.Should().Be(ResultStatus.Success);
    }

    [Fact]
    public async Task Delete_RoleByName_Success()
    {
        // Add.
        await CreateRole_Success();

        // Delete Role
        var deleteRoleurl = BaseUrl + ApiRoutePathConstants.DeleteRoleByName;
        var deleteresponse = await FrontChannelClient.PostAsync(
            deleteRoleurl,
            new StringContent(JsonConvert.SerializeObject(roleName), Encoding.UTF8, "application/json"));
        var deleteRoleResponse = await deleteresponse.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<FrameworkResult>(deleteRoleResponse);
        result.Should().BeOfType<FrameworkResult>();
        result.Status.Should().Be(ResultStatus.Success);
    }

    [Fact]
    public async Task Add_RoleClaim_Success()
    {
        // Add.
        await CreateRole_Success();

        // Get Role
        var getRoleurl = BaseUrl + ApiRoutePathConstants.GetRoleByName;
        var response = await FrontChannelClient.PostAsync(
            getRoleurl,
            new StringContent(JsonConvert.SerializeObject(roleName), Encoding.UTF8, "application/json"));
        var roleResponse = await response.Content.ReadAsStringAsync();
        var roleModel = JsonConvert.DeserializeObject<RoleModel>(roleResponse);

        var addRoleClaimModelInput = RoleHelper.CreateRoleClaimModel();
        addRoleClaimModelInput.RoleId = roleModel.Id;
        //string randomString = random.Next().ToString();
        //addRoleClaimModelInput.ClaimType = string.Concat("Access", "_", randomString);
        //addRoleClaimModelInput.ClaimValue = string.Concat("Access.Role", "_", randomString);

        // add Role Claim
        var addRoleClaimurl = BaseUrl + ApiRoutePathConstants.AddRoleClaim;
        var addRoleClaimresponse = await FrontChannelClient.PostAsync(
            addRoleClaimurl,
            new StringContent(JsonConvert.SerializeObject(addRoleClaimModelInput), Encoding.UTF8, "application/json"));
        var addRoleClaimResponse = await addRoleClaimresponse.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<FrameworkResult>(addRoleClaimResponse);
        result.Should().BeOfType<FrameworkResult>();
        result.Status.Should().Be(ResultStatus.Success);
    }

    [Fact]
    public async Task Add_RoleClaimListModel_Success()
    {
        // Add.
        await CreateRole_Success();

        // Get Role
        var getRoleurl = BaseUrl + ApiRoutePathConstants.GetRoleByName;
        var response = await FrontChannelClient.PostAsync(
            getRoleurl,
            new StringContent(JsonConvert.SerializeObject(roleName), Encoding.UTF8, "application/json"));
        var roleResponse = await response.Content.ReadAsStringAsync();
        var roleModel = JsonConvert.DeserializeObject<RoleModel>(roleResponse);

        var roleClaimModelInputList = RoleHelper.CreateRoleClaimModel_LabTechnician_2();
        var roleClaimCreatedBy = "Test";
        var randomString = random.Next().ToString();
        foreach (var roleClaim in roleClaimModelInputList)
        {
            roleClaim.RoleId = roleModel.Id;
            roleClaim.CreatedBy = roleClaimCreatedBy;
            //roleClaim.ClaimType = string.Concat(roleClaim.ClaimType, "_", randomString);
            //roleClaim.ClaimValue = string.Concat(roleClaim.ClaimValue, "_", randomString);
        }

        // add Role Claim
        var addRoleClaimurl = BaseUrl + ApiRoutePathConstants.AddRoleClaimList;
        var addRoleClaimresponse = await FrontChannelClient.PostAsync(
            addRoleClaimurl,
            new StringContent(JsonConvert.SerializeObject(roleClaimModelInputList), Encoding.UTF8, "application/json"));
        var addRoleClaimResponse = await addRoleClaimresponse.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<FrameworkResult>(addRoleClaimResponse);
        result.Should().BeOfType<FrameworkResult>();
        result.Status.Should().Be(ResultStatus.Success);
    }

    [Fact]
    public async Task Remove_RoleClaim_Success()
    {
        await CreateRole_Success();

        // Get Role
        var getRoleurl = BaseUrl + ApiRoutePathConstants.GetRoleByName;
        var response = await FrontChannelClient.PostAsync(
            getRoleurl,
            new StringContent(JsonConvert.SerializeObject(roleName), Encoding.UTF8, "application/json"));
        var roleResponse = await response.Content.ReadAsStringAsync();
        var roleModel = JsonConvert.DeserializeObject<RoleModel>(roleResponse);

        var addRoleClaimModelInput = RoleHelper.CreateRoleClaimModel();
        addRoleClaimModelInput.RoleId = roleModel.Id;
        var randomString = random.Next().ToString();
        //addRoleClaimModelInput.ClaimType = string.Concat("Access", "_", randomString);
        //addRoleClaimModelInput.ClaimValue = string.Concat("Access.Role", "_", randomString);

        var addRoleClaimurl = BaseUrl + ApiRoutePathConstants.AddRoleClaim;
        var addRoleClaimresponse = await FrontChannelClient.PostAsync(
            addRoleClaimurl,
            new StringContent(JsonConvert.SerializeObject(addRoleClaimModelInput), Encoding.UTF8, "application/json"));
        var addRoleClaimResponse = await addRoleClaimresponse.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<FrameworkResult>(addRoleClaimResponse);
        result.Should().BeOfType<FrameworkResult>();
        result.Status.Should().Be(ResultStatus.Success);

        // Remove Claim
        var removeRoleClaimurl = BaseUrl + ApiRoutePathConstants.RemoveRoleClaim;
        var removeRoleClaimresponse = await FrontChannelClient.PostAsync(
            removeRoleClaimurl,
            new StringContent(JsonConvert.SerializeObject(addRoleClaimModelInput), Encoding.UTF8, "application/json"));
        var removeRoleClaimResponse = await removeRoleClaimresponse.Content.ReadAsStringAsync();
        var removeresult = JsonConvert.DeserializeObject<FrameworkResult>(removeRoleClaimResponse);
        removeresult.Should().BeOfType<FrameworkResult>();
        removeresult.Status.Should().Be(ResultStatus.Success);
    }

    [Fact]
    public async Task Remove_InvalidRoleClaimModel_ReturnError()
    {
        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);

        var addRoleClaimModelInput = RoleHelper.CreateRoleClaimModel();
        addRoleClaimModelInput.RoleId = Guid.NewGuid();
        var randomString = random.Next().ToString();
        //addRoleClaimModelInput.ClaimType = string.Concat("Access", "_", randomString);
        //addRoleClaimModelInput.ClaimValue = string.Concat("Access.Role", "_", randomString);

        // Remove Claim
        var removeRoleClaimurl = BaseUrl + ApiRoutePathConstants.RemoveRoleClaim;
        var removeRoleClaimresponse = await FrontChannelClient.PostAsync(
            removeRoleClaimurl,
            new StringContent(JsonConvert.SerializeObject(addRoleClaimModelInput), Encoding.UTF8, "application/json"));
        var removeRoleClaimResponse = await removeRoleClaimresponse.Content.ReadAsStringAsync();
        var removeresult = JsonConvert.DeserializeObject<FrameworkResult>(removeRoleClaimResponse);
        removeresult.Should().BeOfType<FrameworkResult>();
        removeresult.Status.Should().Be(ResultStatus.Failed);
        removeresult.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.RoleClaimNotExists);
    }

    [Fact]
    public async Task Remove_RoleClaimList_Success()
    {
        await CreateRole_Success();

        // Get Role
        var getRoleurl = BaseUrl + ApiRoutePathConstants.GetRoleByName;
        var response = await FrontChannelClient.PostAsync(
            getRoleurl,
            new StringContent(JsonConvert.SerializeObject(roleName), Encoding.UTF8, "application/json"));
        var roleResponse = await response.Content.ReadAsStringAsync();
        var roleModel = JsonConvert.DeserializeObject<RoleModel>(roleResponse);

        var roleClaimModelInputList = RoleHelper.CreateRoleClaimModel_LabTechnician_2();
        var roleClaimCreatedBy = "Test";
        var randomString = random.Next().ToString();
        foreach (var roleClaim in roleClaimModelInputList)
        {
            roleClaim.RoleId = roleModel.Id;
            roleClaim.CreatedBy = roleClaimCreatedBy;
            //roleClaim.ClaimType = string.Concat(roleClaim.ClaimType, "_", randomString);
            //roleClaim.ClaimValue = string.Concat(roleClaim.ClaimValue, "_", randomString);
        }

        // add Role Claim
        var addRoleClaimurl = BaseUrl + ApiRoutePathConstants.AddRoleClaimList;
        var addRoleClaimresponse = await FrontChannelClient.PostAsync(
            addRoleClaimurl,
            new StringContent(JsonConvert.SerializeObject(roleClaimModelInputList), Encoding.UTF8, "application/json"));
        var addRoleClaimResponse = await addRoleClaimresponse.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<FrameworkResult>(addRoleClaimResponse);
        result.Should().BeOfType<FrameworkResult>();
        result.Status.Should().Be(ResultStatus.Success);

        // Remove Claim
        var removeRoleClaimurl = BaseUrl + ApiRoutePathConstants.RemoveRoleClaimsList;
        var removeRoleClaimresponse = await FrontChannelClient.PostAsync(
            removeRoleClaimurl,
            new StringContent(JsonConvert.SerializeObject(roleClaimModelInputList), Encoding.UTF8, "application/json"));
        var removeRoleClaimResponse = await removeRoleClaimresponse.Content.ReadAsStringAsync();
        var removeresult = JsonConvert.DeserializeObject<FrameworkResult>(removeRoleClaimResponse);
        removeresult.Should().BeOfType<FrameworkResult>();
        removeresult.Status.Should().Be(ResultStatus.Success);
    }

    [Fact]
    public async Task Add_RoleClaimListModel_RoleClaimExists()
    {
        // Add.
        await CreateRole_Success();

        // Get Role
        var getRoleurl = BaseUrl + ApiRoutePathConstants.GetRoleByName;
        var response = await FrontChannelClient.PostAsync(
            getRoleurl,
            new StringContent(JsonConvert.SerializeObject(roleName), Encoding.UTF8, "application/json"));
        var roleResponse = await response.Content.ReadAsStringAsync();
        var roleModel = JsonConvert.DeserializeObject<RoleModel>(roleResponse);

        var roleClaimModelInputList = RoleHelper.CreateRoleClaimModel_LabTechnician_2();
        var roleClaimCreatedBy = "Test";
        var randomString = random.Next().ToString();
        foreach (var roleClaim in roleClaimModelInputList)
        {
            roleClaim.RoleId = roleModel.Id;
            roleClaim.CreatedBy = roleClaimCreatedBy;
            //roleClaim.ClaimType = string.Concat(roleClaim.ClaimType, "_", randomString);
            //roleClaim.ClaimValue = string.Concat(roleClaim.ClaimValue, "_", randomString);
        }

        // add Role Claim
        var addRoleClaimurl = BaseUrl + ApiRoutePathConstants.AddRoleClaimList;
        var addRoleClaimresponse = await FrontChannelClient.PostAsync(
            addRoleClaimurl,
            new StringContent(JsonConvert.SerializeObject(roleClaimModelInputList), Encoding.UTF8, "application/json"));

        addRoleClaimurl = BaseUrl + ApiRoutePathConstants.AddRoleClaimList;
        addRoleClaimresponse = await FrontChannelClient.PostAsync(
            addRoleClaimurl,
            new StringContent(JsonConvert.SerializeObject(roleClaimModelInputList), Encoding.UTF8, "application/json"));

        var addRoleClaimResponse = await addRoleClaimresponse.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<FrameworkResult>(addRoleClaimResponse);
        result.Should().BeOfType<FrameworkResult>();
        result.Status.Should().Be(ResultStatus.Failed);
        result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.RoleClaimExists);
    }

    [Fact]
    public async Task DeleteRole_InvalidRoleId()
    {
        var role_id = Guid.NewGuid();
        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);
        // Delete Role
        var deleteRoleurl = BaseUrl + ApiRoutePathConstants.DeleteRoleById;
        var deleteresponse = await FrontChannelClient.PostAsync(
            deleteRoleurl,
            new StringContent(JsonConvert.SerializeObject(role_id), Encoding.UTF8, "application/json"));
        var deleteRoleResponse = await deleteresponse.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<FrameworkResult>(deleteRoleResponse);
        result.Status.Should().Be(ResultStatus.Failed);
        result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.InvalidRoleId);
    }

    [Fact]
    public async Task DeleteRole_InvalidRoleName()
    {
        var role_name = "TestRole";
        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);
        // Delete Role
        var deleteRoleurl = BaseUrl + ApiRoutePathConstants.DeleteRoleByName;
        var deleteresponse = await FrontChannelClient.PostAsync(
            deleteRoleurl,
            new StringContent(JsonConvert.SerializeObject(role_name), Encoding.UTF8, "application/json"));
        var deleteRoleResponse = await deleteresponse.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<FrameworkResult>(deleteRoleResponse);
        result.Status.Should().Be(ResultStatus.Failed);
        result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.InvalidRoleName);
    }
}
