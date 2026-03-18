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
using static IntegrationTests.ApiDomainModel.AllowedScopesParserModel;
using ApiDomainModel_FrameworkResult = IntegrationTests.ApiDomainModel.FrameworkResult;
using FrameworkResult = IntegrationTests.ApiDomainModel.FrameworkResult;

namespace IntegrationTests.MiddlewareRoutes;

public class IdentityResourceProxyServiceTest : HCLCSSFFakeSetup
{
    public string identityResourceName;

    [Fact]
    public async Task Add_AddIdentityResource_ReturnSuccess()
    {
        var random = new Random();
        var identityResourcesModelInput = IdentityResourceHelper.CreateIdentityResourceModel();
        identityResourcesModelInput.Name = string.Concat("IntegrationTestIdentity", "_", random.Next().ToString());

        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);

        var addIdentityResourceurl = BaseUrl + ApiRoutePathConstants.AddIdentityResource;
        var response = await FrontChannelClient.PostAsync(
            addIdentityResourceurl,
            new StringContent(JsonConvert.SerializeObject(identityResourcesModelInput), Encoding.UTF8,
                "application/json"));
        var lockUserResopnse = await response.Content.ReadAsStringAsync();
        var frameworkResult = JsonConvert.DeserializeObject<FrameworkResult>(lockUserResopnse);
        frameworkResult.Should().BeOfType<ApiDomainModel_FrameworkResult>();
        frameworkResult.Status.Should().Be(ResultStatus.Success);
        identityResourceName = identityResourcesModelInput.Name;
    }

    [Fact]
    public async Task Update_IdentityResource_ReturnSuccess()
    {
        // Add.
        await Add_AddIdentityResource_ReturnSuccess();

        // Get.
        var getIdentityResourceByNameurl = BaseUrl + ApiRoutePathConstants.GetIdentityResourceByName;
        var response = await FrontChannelClient.PostAsync(
            getIdentityResourceByNameurl,
            new StringContent(JsonConvert.SerializeObject(identityResourceName), Encoding.UTF8, "application/json"));
        var identityResourceResponse = await response.Content.ReadAsStringAsync();
        var resultIdentityResourceModel =
            JsonConvert.DeserializeObject<IdentityResourcesModel>(identityResourceResponse);

        // Update.
        resultIdentityResourceModel.DisplayName = "TestProxy";

        var updateIdentityResourceurl = BaseUrl + ApiRoutePathConstants.UpdateIdentityResource;
        var updateIdentityresponse = await FrontChannelClient.PostAsync(
            updateIdentityResourceurl,
            new StringContent(JsonConvert.SerializeObject(resultIdentityResourceModel), Encoding.UTF8,
                "application/json"));
        var updateIdentityResponse = await updateIdentityresponse.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<ApiDomainModel_FrameworkResult>(updateIdentityResponse);
        result.Should().BeOfType<ApiDomainModel_FrameworkResult>();
        result.Status.Should().Be(ResultStatus.Success);
    }

    [Fact]
    public async Task Delete_IdentityResource_ReturnSuccess()
    {
        // Add.
        await Add_AddIdentityResource_ReturnSuccess();

        // Get.
        var getIdentityResourceByNameurl = BaseUrl + ApiRoutePathConstants.GetIdentityResourceByName;
        var response = await FrontChannelClient.PostAsync(
            getIdentityResourceByNameurl,
            new StringContent(JsonConvert.SerializeObject(identityResourceName), Encoding.UTF8, "application/json"));
        var identityResourceResponse = await response.Content.ReadAsStringAsync();
        var resultIdentityResourceModel =
            JsonConvert.DeserializeObject<IdentityResourcesModel>(identityResourceResponse);

        // Delete.
        var deleteIdentityResourceurl = BaseUrl + ApiRoutePathConstants.DeleteIdentityResourceById;
        var deleteIdentityresponse = await FrontChannelClient.PostAsync(
            deleteIdentityResourceurl,
            new StringContent(JsonConvert.SerializeObject(resultIdentityResourceModel.Id), Encoding.UTF8,
                "application/json"));
        var deleteIdentityResponse = await deleteIdentityresponse.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<ApiDomainModel_FrameworkResult>(deleteIdentityResponse);
        result.Should().BeOfType<ApiDomainModel_FrameworkResult>();
        result.Status.Should().Be(ResultStatus.Success);
    }

    [Fact]
    public async Task Delete_IdentityResource_ByName_ReturnSuccess()
    {
        // Add.
        await Add_AddIdentityResource_ReturnSuccess();

        // Get.
        var getIdentityResourceByNameurl = BaseUrl + ApiRoutePathConstants.GetIdentityResourceByName;
        var response = await FrontChannelClient.PostAsync(
            getIdentityResourceByNameurl,
            new StringContent(JsonConvert.SerializeObject(identityResourceName), Encoding.UTF8, "application/json"));
        var identityResourceResponse = await response.Content.ReadAsStringAsync();
        var resultIdentityResourceModel =
            JsonConvert.DeserializeObject<IdentityResourcesModel>(identityResourceResponse);

        // Delete.
        var deleteIdentityResourceurl = BaseUrl + ApiRoutePathConstants.DeleteIdentityResourceByName;
        var deleteIdentityresponse = await FrontChannelClient.PostAsync(
            deleteIdentityResourceurl,
            new StringContent(JsonConvert.SerializeObject(resultIdentityResourceModel.Name), Encoding.UTF8,
                "application/json"));
        var deleteIdentityResponse = await deleteIdentityresponse.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<ApiDomainModel_FrameworkResult>(deleteIdentityResponse);
        result.Should().BeOfType<ApiDomainModel_FrameworkResult>();
        result.Status.Should().Be(ResultStatus.Success);
    }

    [Fact]
    public async Task GetAll_IdentityResource_ReturnSuccess()
    {
        // Add.
        await Add_AddIdentityResource_ReturnSuccess();

        // Get.
        var getIdentityResourceByNameurl = BaseUrl + ApiRoutePathConstants.GetAllIdentityResources;
        var response = await FrontChannelClient.PostAsync(
            getIdentityResourceByNameurl,
            new StringContent(JsonConvert.SerializeObject(identityResourceName), Encoding.UTF8, "application/json"));
        var identityResourceResponse = await response.Content.ReadAsStringAsync();
        var resultIdentityResourceModel =
            JsonConvert.DeserializeObject<List<IdentityResourcesModel>>(identityResourceResponse);
        resultIdentityResourceModel.Should().NotBeNull();
    }

    [Fact]
    public async Task Add_IdentityResourceClaim_ReturnSuccess()
    {
        // Add.
        await Add_AddIdentityResource_ReturnSuccess();

        // Get.
        var getIdentityResourceByNameurl = BaseUrl + ApiRoutePathConstants.GetIdentityResourceByName;
        var response = await FrontChannelClient.PostAsync(
            getIdentityResourceByNameurl,
            new StringContent(JsonConvert.SerializeObject(identityResourceName), Encoding.UTF8, "application/json"));
        var identityResourceResponse = await response.Content.ReadAsStringAsync();
        var resultIdentityResourceModel =
            JsonConvert.DeserializeObject<IdentityResourcesModel>(identityResourceResponse);

        var identityResourceClaimInputModel = IdentityResourceHelper.CreateIdentityResourceClaimModel();
        identityResourceClaimInputModel.IdentityResourceId = resultIdentityResourceModel.Id;

        // Add Claim
        var claimIdentityResourceurl = BaseUrl + ApiRoutePathConstants.AddIdentityResourceClaim;
        var claimIdentityresponse = await FrontChannelClient.PostAsync(
            claimIdentityResourceurl,
            new StringContent(JsonConvert.SerializeObject(identityResourceClaimInputModel), Encoding.UTF8,
                "application/json"));

        var claimIdentityResponse = await claimIdentityresponse.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<ApiDomainModel_FrameworkResult>(claimIdentityResponse);
        result.Should().BeOfType<ApiDomainModel_FrameworkResult>();
        result.Status.Should().Be(ResultStatus.Success);
    }

    [Fact]
    public async Task Delete_IdentityResourceClaimById_ReturnSuccess()
    {
        // Add.
        await Add_AddIdentityResource_ReturnSuccess();

        // Get.
        var getIdentityResourceByNameurl = BaseUrl + ApiRoutePathConstants.GetIdentityResourceByName;
        var response = await FrontChannelClient.PostAsync(
            getIdentityResourceByNameurl,
            new StringContent(JsonConvert.SerializeObject(identityResourceName), Encoding.UTF8, "application/json"));
        var identityResourceResponse = await response.Content.ReadAsStringAsync();
        var resultIdentityResourceModel =
            JsonConvert.DeserializeObject<IdentityResourcesModel>(identityResourceResponse);

        // Delete.
        var deleteIdentityResourceurl = BaseUrl + ApiRoutePathConstants.DeleteIdentityResourceClaimModel;
        var deleteIdentityresponse = await FrontChannelClient.PostAsync(
            deleteIdentityResourceurl,
            new StringContent(JsonConvert.SerializeObject(resultIdentityResourceModel.IdentityClaims[0]), Encoding.UTF8,
                "application/json"));
        var deleteIdentityResponse = await deleteIdentityresponse.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<ApiDomainModel_FrameworkResult>(deleteIdentityResponse);
        result.Should().BeOfType<ApiDomainModel_FrameworkResult>();
        result.Status.Should().Be(ResultStatus.Success);
    }

    [Fact]
    public async Task Delete_IdentityResourceClaimModel_ReturnSuccess()
    {
        // Add.
        await Add_AddIdentityResource_ReturnSuccess();

        // Get.
        var getIdentityResourceByNameurl = BaseUrl + ApiRoutePathConstants.GetIdentityResourceByName;
        var response = await FrontChannelClient.PostAsync(
            getIdentityResourceByNameurl,
            new StringContent(JsonConvert.SerializeObject(identityResourceName), Encoding.UTF8, "application/json"));
        var identityResourceResponse = await response.Content.ReadAsStringAsync();
        var resultIdentityResourceModel =
            JsonConvert.DeserializeObject<IdentityResourcesModel>(identityResourceResponse);

        // Get Claim.
        var getClaimIdentityResourceurl = BaseUrl + ApiRoutePathConstants.GetIdentityResourceClaims;
        var getClaimIdentityresponse = await FrontChannelClient.PostAsync(
            getClaimIdentityResourceurl,
            new StringContent(JsonConvert.SerializeObject(resultIdentityResourceModel.Id), Encoding.UTF8,
                "application/json"));
        var getClaimIdentityResponse = await getClaimIdentityresponse.Content.ReadAsStringAsync();
        var getClaimIdentityResourceModel =
            JsonConvert.DeserializeObject<List<IdentityClaimsModel>>(getClaimIdentityResponse);
        // Delete Claim
        var deleteIdentityResourceurl = BaseUrl + ApiRoutePathConstants.DeleteIdentityResourceClaimModel;
        var deleteIdentityresponse = await FrontChannelClient.PostAsync(
            deleteIdentityResourceurl,
            new StringContent(JsonConvert.SerializeObject(getClaimIdentityResourceModel[0]), Encoding.UTF8,
                "application/json"));
        var deleteIdentityResponse = await deleteIdentityresponse.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<ApiDomainModel_FrameworkResult>(deleteIdentityResponse);
        result.Should().BeOfType<ApiDomainModel_FrameworkResult>();
        result.Status.Should().Be(ResultStatus.Success);
    }

    [Fact]
    public async Task AddIdentityResourceAsync_Nullcheck()
    {
        var identityResourcesModelInput = IdentityResourceHelper.CreateIdentityResourceModel();
        identityResourcesModelInput.Name = null;

        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);

        var addIdentityResourceurl = BaseUrl + ApiRoutePathConstants.AddIdentityResource;
        var response = await FrontChannelClient.PostAsync(
            addIdentityResourceurl,
            new StringContent(JsonConvert.SerializeObject(identityResourcesModelInput), Encoding.UTF8,
                "application/json"));
        var lockUserResopnse = await response.Content.ReadAsStringAsync();
        var frameworkResult = JsonConvert.DeserializeObject<FrameworkResult>(lockUserResopnse);
        frameworkResult.Should().BeOfType<ApiDomainModel_FrameworkResult>();
        frameworkResult.Status.Should().Be(ResultStatus.Failed);
        frameworkResult.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.IdentityResourceNameRequired);
    }

    [Fact]
    public async Task AddIdentityResourceAsync_Emptycheck()
    {
        var identityResourcesModelInput = IdentityResourceHelper.CreateIdentityResourceModel();
        identityResourcesModelInput.Name = string.Empty;

        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);

        var addIdentityResourceurl = BaseUrl + ApiRoutePathConstants.AddIdentityResource;
        var response = await FrontChannelClient.PostAsync(
            addIdentityResourceurl,
            new StringContent(JsonConvert.SerializeObject(identityResourcesModelInput), Encoding.UTF8,
                "application/json"));
        var lockUserResopnse = await response.Content.ReadAsStringAsync();
        var frameworkResult = JsonConvert.DeserializeObject<FrameworkResult>(lockUserResopnse);
        frameworkResult.Should().BeOfType<ApiDomainModel_FrameworkResult>();
        frameworkResult.Status.Should().Be(ResultStatus.Failed);
        frameworkResult.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.IdentityResourceNameRequired);
    }

    [Fact]
    public async Task AddIdentityResourceAsync_Childmodel_Empty()
    {
        var random = new Random();
        var identityResourcesModelInput = IdentityResourceHelper.CreateIdentityResourceModel();
        identityResourcesModelInput.Name = string.Concat(identityResourceName, "_", random.Next().ToString());
        identityResourcesModelInput.IdentityClaims.Clear();

        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);

        var addIdentityResourceurl = BaseUrl + ApiRoutePathConstants.AddIdentityResource;
        var response = await FrontChannelClient.PostAsync(
            addIdentityResourceurl,
            new StringContent(JsonConvert.SerializeObject(identityResourcesModelInput), Encoding.UTF8,
                "application/json"));
        var lockUserResopnse = await response.Content.ReadAsStringAsync();
        var frameworkResult = JsonConvert.DeserializeObject<FrameworkResult>(lockUserResopnse);
        frameworkResult.Should().BeOfType<ApiDomainModel_FrameworkResult>();
        frameworkResult.Status.Should().Be(ResultStatus.Success);
    }

    [Fact]
    public async Task AddIdentityResourceAsync_ChildModelNull()
    {
        var random = new Random();
        var identityResourcesModelInput = IdentityResourceHelper.CreateIdentityResourceModel();
        identityResourcesModelInput.Name = string.Concat(identityResourceName, "_", random.Next().ToString());
        identityResourcesModelInput.IdentityClaims = null;

        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);

        var addIdentityResourceurl = BaseUrl + ApiRoutePathConstants.AddIdentityResource;
        var response = await FrontChannelClient.PostAsync(
            addIdentityResourceurl,
            new StringContent(JsonConvert.SerializeObject(identityResourcesModelInput), Encoding.UTF8,
                "application/json"));
        var lockUserResopnse = await response.Content.ReadAsStringAsync();
        var frameworkResult = JsonConvert.DeserializeObject<FrameworkResult>(lockUserResopnse);
        frameworkResult.Should().BeOfType<ApiDomainModel_FrameworkResult>();
        frameworkResult.Status.Should().Be(ResultStatus.Success);
    }

    [Fact]
    [Trait("Category", "ErrorCase")]
    public async Task AddIdentityResourceAsync_ChildtableTypeNULL()
    {
        var model = IdentityResourceHelper.CreateIdentityResourceModel();
        var random = new Random();
        model.Name = string.Concat(identityResourceName, "_", random.Next().ToString());
        model.IdentityClaims[0].Type = null;

        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);

        var addIdentityResourceurl = BaseUrl + ApiRoutePathConstants.AddIdentityResource;
        var response = await FrontChannelClient.PostAsync(
            addIdentityResourceurl,
            new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json"));
        var lockUserResopnse = await response.Content.ReadAsStringAsync();
        var frameworkResult = JsonConvert.DeserializeObject<FrameworkResult>(lockUserResopnse);
        frameworkResult.Should().BeOfType<ApiDomainModel_FrameworkResult>();
        frameworkResult.Status.Should().Be(ResultStatus.Failed);
        frameworkResult.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.IdentityResourceClaimTypeRequired);
    }

    [Fact]
    public async Task AddIdentityResourceAsync_Duplicate_Error()
    {
        var random = new Random();
        var identityResourcesModelInput = IdentityResourceHelper.CreateIdentityResourceModel();
        identityResourcesModelInput.Name = string.Concat("IntegrationTest", "_", random.Next().ToString());

        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);

        var addIdentityResourceurl = BaseUrl + ApiRoutePathConstants.AddIdentityResource;
        var response = await FrontChannelClient.PostAsync(
            addIdentityResourceurl,
            new StringContent(JsonConvert.SerializeObject(identityResourcesModelInput), Encoding.UTF8,
                "application/json"));
        addIdentityResourceurl = BaseUrl + ApiRoutePathConstants.AddIdentityResource;
        response = await FrontChannelClient.PostAsync(
            addIdentityResourceurl,
            new StringContent(JsonConvert.SerializeObject(identityResourcesModelInput), Encoding.UTF8,
                "application/json"));

        var lockUserResopnse = await response.Content.ReadAsStringAsync();
        var frameworkResult = JsonConvert.DeserializeObject<FrameworkResult>(lockUserResopnse);
        frameworkResult.Should().BeOfType<ApiDomainModel_FrameworkResult>();
        frameworkResult.Status.Should().Be(ResultStatus.Failed);
        frameworkResult.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.IdentityResourceAlreadyExists);
    }

    [Fact]
    public async Task AddIdentityResourceAsync_NameLenthExceed_Failure()
    {
        var identityResourcesModelInput = IdentityResourceHelper.CreateIdentityResourceModel();
        identityResourcesModelInput.Name =
            "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";

        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);

        var addIdentityResourceurl = BaseUrl + ApiRoutePathConstants.AddIdentityResource;
        var response = await FrontChannelClient.PostAsync(
            addIdentityResourceurl,
            new StringContent(JsonConvert.SerializeObject(identityResourcesModelInput), Encoding.UTF8,
                "application/json"));

        var lockUserResopnse = await response.Content.ReadAsStringAsync();
        var frameworkResult = JsonConvert.DeserializeObject<FrameworkResult>(lockUserResopnse);
        frameworkResult.Should().BeOfType<ApiDomainModel_FrameworkResult>();
        frameworkResult.Status.Should().Be(ResultStatus.Failed);
        frameworkResult.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.IdentityResourceNameTooLong);
    }

    [Fact]
    public async Task AddIdentityResourceAsync_NameNull()
    {
        var identityResourcesModelInput = IdentityResourceHelper.CreateIdentityResourceModel();
        identityResourcesModelInput.Name = null;

        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);

        var addIdentityResourceurl = BaseUrl + ApiRoutePathConstants.AddIdentityResource;
        var response = await FrontChannelClient.PostAsync(
            addIdentityResourceurl,
            new StringContent(JsonConvert.SerializeObject(identityResourcesModelInput), Encoding.UTF8,
                "application/json"));

        var lockUserResopnse = await response.Content.ReadAsStringAsync();
        var frameworkResult = JsonConvert.DeserializeObject<FrameworkResult>(lockUserResopnse);
        frameworkResult.Should().BeOfType<ApiDomainModel_FrameworkResult>();
        frameworkResult.Status.Should().Be(ResultStatus.Failed);
        frameworkResult.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.IdentityResourceNameRequired);
    }

    [Fact]
    public async Task AddIdentityResourceAsync_InvalidName_ReturnError()
    {
        var identityResourcesModelInput = IdentityResourceHelper.CreateIdentityResourceModel();
        identityResourcesModelInput.Name = string.Empty;

        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);

        var addIdentityResourceurl = BaseUrl + ApiRoutePathConstants.AddIdentityResource;
        var response = await FrontChannelClient.PostAsync(
            addIdentityResourceurl,
            new StringContent(JsonConvert.SerializeObject(identityResourcesModelInput), Encoding.UTF8,
                "application/json"));

        var lockUserResopnse = await response.Content.ReadAsStringAsync();
        var frameworkResult = JsonConvert.DeserializeObject<FrameworkResult>(lockUserResopnse);
        frameworkResult.Should().BeOfType<ApiDomainModel_FrameworkResult>();
        frameworkResult.Status.Should().Be(ResultStatus.Failed);
        frameworkResult.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.IdentityResourceNameRequired);
    }

    [Fact]
    public async Task UpdateIdentityResourceAsync_ChildModelNull()
    {
        // Add.
        await Add_AddIdentityResource_ReturnSuccess();

        // Get.
        var getIdentityResourceByNameurl = BaseUrl + ApiRoutePathConstants.GetIdentityResourceByName;
        var response = await FrontChannelClient.PostAsync(
            getIdentityResourceByNameurl,
            new StringContent(JsonConvert.SerializeObject(identityResourceName), Encoding.UTF8, "application/json"));
        var identityResourceResponse = await response.Content.ReadAsStringAsync();
        var resultIdentityResourceModel =
            JsonConvert.DeserializeObject<IdentityResourcesModel>(identityResourceResponse);

        resultIdentityResourceModel.IdentityClaims = null;

        // Update.
        resultIdentityResourceModel.DisplayName = "TestProxy";

        var updateIdentityResourceurl = BaseUrl + ApiRoutePathConstants.UpdateIdentityResource;
        var updateIdentityresponse = await FrontChannelClient.PostAsync(
            updateIdentityResourceurl,
            new StringContent(JsonConvert.SerializeObject(resultIdentityResourceModel), Encoding.UTF8,
                "application/json"));
        var updateIdentityResponse = await updateIdentityresponse.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<ApiDomainModel_FrameworkResult>(updateIdentityResponse);
        result.Should().BeOfType<ApiDomainModel_FrameworkResult>();
        result.Status.Should().Be(ResultStatus.Success);
    }

    [Fact]
    public async Task UpdateIdentityResourceAsync_ChildModelEmpty()
    {
        // Add.
        await Add_AddIdentityResource_ReturnSuccess();

        // Get.
        var getIdentityResourceByNameurl = BaseUrl + ApiRoutePathConstants.GetIdentityResourceByName;
        var response = await FrontChannelClient.PostAsync(
            getIdentityResourceByNameurl,
            new StringContent(JsonConvert.SerializeObject(identityResourceName), Encoding.UTF8, "application/json"));
        var identityResourceResponse = await response.Content.ReadAsStringAsync();
        var resultIdentityResourceModel =
            JsonConvert.DeserializeObject<IdentityResourcesModel>(identityResourceResponse);

        resultIdentityResourceModel.IdentityClaims.Clear();

        // Update.
        resultIdentityResourceModel.DisplayName = "TestProxy";

        var updateIdentityResourceurl = BaseUrl + ApiRoutePathConstants.UpdateIdentityResource;
        var updateIdentityresponse = await FrontChannelClient.PostAsync(
            updateIdentityResourceurl,
            new StringContent(JsonConvert.SerializeObject(resultIdentityResourceModel), Encoding.UTF8,
                "application/json"));
        var updateIdentityResponse = await updateIdentityresponse.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<ApiDomainModel_FrameworkResult>(updateIdentityResponse);
        result.Should().BeOfType<ApiDomainModel_FrameworkResult>();
        result.Status.Should().Be(ResultStatus.Success);
    }

    [Fact]
    public async Task Update_IdentityResource_NameLengthExceed_Error()
    {
        // Add.
        await Add_AddIdentityResource_ReturnSuccess();

        // Get.
        var getIdentityResourceByNameurl = BaseUrl + ApiRoutePathConstants.GetIdentityResourceByName;
        var response = await FrontChannelClient.PostAsync(
            getIdentityResourceByNameurl,
            new StringContent(JsonConvert.SerializeObject(identityResourceName), Encoding.UTF8, "application/json"));
        var identityResourceResponse = await response.Content.ReadAsStringAsync();
        var resultIdentityResourceModel =
            JsonConvert.DeserializeObject<IdentityResourcesModel>(identityResourceResponse);

        resultIdentityResourceModel.DisplayName =
            "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";

        // Update.
        var updateIdentityResourceurl = BaseUrl + ApiRoutePathConstants.UpdateIdentityResource;
        var updateIdentityresponse = await FrontChannelClient.PostAsync(
            updateIdentityResourceurl,
            new StringContent(JsonConvert.SerializeObject(resultIdentityResourceModel), Encoding.UTF8,
                "application/json"));
        var updateIdentityResponse = await updateIdentityresponse.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<ApiDomainModel_FrameworkResult>(updateIdentityResponse);
        result.Should().BeOfType<ApiDomainModel_FrameworkResult>();
        result.Status.Should().Be(ResultStatus.Failed);
        result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.IdentityDisplayNameTooLong);
    }

    [Fact]
    public async Task Update_IdentityResource_NullCheck()
    {
        // Add.
        await Add_AddIdentityResource_ReturnSuccess();

        // Get.
        var getIdentityResourceByNameurl = BaseUrl + ApiRoutePathConstants.GetIdentityResourceByName;
        var response = await FrontChannelClient.PostAsync(
            getIdentityResourceByNameurl,
            new StringContent(JsonConvert.SerializeObject(identityResourceName), Encoding.UTF8, "application/json"));
        var identityResourceResponse = await response.Content.ReadAsStringAsync();
        var resultIdentityResourceModel =
            JsonConvert.DeserializeObject<IdentityResourcesModel>(identityResourceResponse);

        resultIdentityResourceModel.Name = null;

        // Update.
        var updateIdentityResourceurl = BaseUrl + ApiRoutePathConstants.UpdateIdentityResource;
        var updateIdentityresponse = await FrontChannelClient.PostAsync(
            updateIdentityResourceurl,
            new StringContent(JsonConvert.SerializeObject(resultIdentityResourceModel), Encoding.UTF8,
                "application/json"));
        var updateIdentityResponse = await updateIdentityresponse.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<ApiDomainModel_FrameworkResult>(updateIdentityResponse);
        result.Should().BeOfType<ApiDomainModel_FrameworkResult>();
        result.Status.Should().Be(ResultStatus.Failed);
        result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.IdentityResourceNameRequired);
    }

    [Fact]
    public async Task Update_IdentityResource_EmptyCheck()
    {
        // Add.
        await Add_AddIdentityResource_ReturnSuccess();

        // Get.
        var getIdentityResourceByNameurl = BaseUrl + ApiRoutePathConstants.GetIdentityResourceByName;
        var response = await FrontChannelClient.PostAsync(
            getIdentityResourceByNameurl,
            new StringContent(JsonConvert.SerializeObject(identityResourceName), Encoding.UTF8, "application/json"));
        var identityResourceResponse = await response.Content.ReadAsStringAsync();
        var resultIdentityResourceModel =
            JsonConvert.DeserializeObject<IdentityResourcesModel>(identityResourceResponse);

        resultIdentityResourceModel.Name = null;

        // Update.
        var updateIdentityResourceurl = BaseUrl + ApiRoutePathConstants.UpdateIdentityResource;
        var updateIdentityresponse = await FrontChannelClient.PostAsync(
            updateIdentityResourceurl,
            new StringContent(JsonConvert.SerializeObject(resultIdentityResourceModel), Encoding.UTF8,
                "application/json"));
        var updateIdentityResponse = await updateIdentityresponse.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<ApiDomainModel_FrameworkResult>(updateIdentityResponse);
        result.Should().BeOfType<ApiDomainModel_FrameworkResult>();
        result.Status.Should().Be(ResultStatus.Failed);
        result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.IdentityResourceNameRequired);
    }

    [Fact]
    public async Task AddIdentityResourceClaimAsync_TypeLenthExceedd_Error()
    {
        // Add.
        await Add_AddIdentityResource_ReturnSuccess();

        // Get.
        var getIdentityResourceByNameurl = BaseUrl + ApiRoutePathConstants.GetIdentityResourceByName;
        var response = await FrontChannelClient.PostAsync(
            getIdentityResourceByNameurl,
            new StringContent(JsonConvert.SerializeObject(identityResourceName), Encoding.UTF8, "application/json"));
        var identityResourceResponse = await response.Content.ReadAsStringAsync();
        var resultIdentityResourceModel =
            JsonConvert.DeserializeObject<IdentityResourcesModel>(identityResourceResponse);

        var identityResourceClaimInputModel = IdentityResourceHelper.CreateIdentityResourceClaimModel();
        identityResourceClaimInputModel.IdentityResourceId = resultIdentityResourceModel.Id;
        identityResourceClaimInputModel.Type =
            "hhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllll";

        // Add Claim
        var claimIdentityResourceurl = BaseUrl + ApiRoutePathConstants.AddIdentityResourceClaim;
        var claimIdentityresponse = await FrontChannelClient.PostAsync(
            claimIdentityResourceurl,
            new StringContent(JsonConvert.SerializeObject(identityResourceClaimInputModel), Encoding.UTF8,
                "application/json"));

        var claimIdentityResponse = await claimIdentityresponse.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<ApiDomainModel_FrameworkResult>(claimIdentityResponse);
        result.Should().BeOfType<ApiDomainModel_FrameworkResult>();
        result.Status.Should().Be(ResultStatus.Failed);
        result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.IdentityResourceClaimTypeTooLong);
    }

    [Fact]
    public async Task AddIdentityResourceClaimAsync_TypeNull_Error()
    {
        // Add.
        await Add_AddIdentityResource_ReturnSuccess();

        // Get.
        var getIdentityResourceByNameurl = BaseUrl + ApiRoutePathConstants.GetIdentityResourceByName;
        var response = await FrontChannelClient.PostAsync(
            getIdentityResourceByNameurl,
            new StringContent(JsonConvert.SerializeObject(identityResourceName), Encoding.UTF8, "application/json"));
        var identityResourceResponse = await response.Content.ReadAsStringAsync();
        var resultIdentityResourceModel =
            JsonConvert.DeserializeObject<IdentityResourcesModel>(identityResourceResponse);

        var identityResourceClaimInputModel = IdentityResourceHelper.CreateIdentityResourceClaimModel();
        identityResourceClaimInputModel.IdentityResourceId = resultIdentityResourceModel.Id;
        identityResourceClaimInputModel.Type = null;

        // Add Claim
        var claimIdentityResourceurl = BaseUrl + ApiRoutePathConstants.AddIdentityResourceClaim;
        var claimIdentityresponse = await FrontChannelClient.PostAsync(
            claimIdentityResourceurl,
            new StringContent(JsonConvert.SerializeObject(identityResourceClaimInputModel), Encoding.UTF8,
                "application/json"));

        var claimIdentityResponse = await claimIdentityresponse.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<ApiDomainModel_FrameworkResult>(claimIdentityResponse);
        result.Should().BeOfType<ApiDomainModel_FrameworkResult>();
        result.Status.Should().Be(ResultStatus.Failed);
        result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.IdentityResourceClaimTypeRequired);
    }

    [Fact]
    public async Task Delete_IdentityResource_InvalidName_ReturError()
    {
        // Add.
        await Add_AddIdentityResource_ReturnSuccess();

        // Get.
        var getIdentityResourceByNameurl = BaseUrl + ApiRoutePathConstants.GetIdentityResourceByName;
        var response = await FrontChannelClient.PostAsync(
            getIdentityResourceByNameurl,
            new StringContent(JsonConvert.SerializeObject(identityResourceName), Encoding.UTF8, "application/json"));
        var identityResourceResponse = await response.Content.ReadAsStringAsync();
        var resultIdentityResourceModel =
            JsonConvert.DeserializeObject<IdentityResourcesModel>(identityResourceResponse);
        resultIdentityResourceModel.Name = "testproxy";

        // Delete.
        var deleteIdentityResourceurl = BaseUrl + ApiRoutePathConstants.DeleteIdentityResourceByName;
        var deleteIdentityresponse = await FrontChannelClient.PostAsync(
            deleteIdentityResourceurl,
            new StringContent(JsonConvert.SerializeObject(resultIdentityResourceModel.Name), Encoding.UTF8,
                "application/json"));
        var deleteIdentityResponse = await deleteIdentityresponse.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<ApiDomainModel_FrameworkResult>(deleteIdentityResponse);
        result.Should().BeOfType<ApiDomainModel_FrameworkResult>();
        result.Status.Should().Be(ResultStatus.Failed);
        result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.IdentityResourceNameRequired);
    }
}
