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

public class ApiResourceProxyServiceTests : HCLCSSFFakeSetup
{
    private const string ScopeName = "IntegrationTestApiResource";
    private readonly Random random = new();
    public string apiResourceName;

    [Fact]
    public async Task Add_ApiResource_ReturnSuccess()
    {
        var apiResourcesModelInput = ApiResourceHelper.CreateApiResourceModel();
        apiResourcesModelInput.Name = string.Concat("IntegrationTest", "_", random.Next().ToString());
        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);

        var addApiResourceurl = BaseUrl + ApiRoutePathConstants.AddApiResource;
        var addApiResourceresponse = await FrontChannelClient.PostAsync(
            addApiResourceurl,
            new StringContent(JsonConvert.SerializeObject(apiResourcesModelInput), Encoding.UTF8, "application/json"));
        var apiResourceResponse = await addApiResourceresponse.Content.ReadAsStringAsync();
        var frameworkResult = JsonConvert.DeserializeObject<FrameworkResult>(apiResourceResponse);
        frameworkResult.Should().BeOfType<FrameworkResult>();
        frameworkResult.Status.Should().Be(ResultStatus.Success);
        apiResourceName = apiResourcesModelInput.Name;
    }

    [Fact]
    public async Task UpdateApiResource_ReturnSuccess()
    {
        await Add_ApiResource_ReturnSuccess();

        var getApiResourceurl = BaseUrl + ApiRoutePathConstants.GetApiResourceByName;
        var getApiResourceresponse = await FrontChannelClient.PostAsync(
            getApiResourceurl,
            new StringContent(JsonConvert.SerializeObject(apiResourceName), Encoding.UTF8, "application/json"));
        var apiResourceResponse = await getApiResourceresponse.Content.ReadAsStringAsync();
        var resultApiresourceModel = JsonConvert.DeserializeObject<ApiResourcesModel>(apiResourceResponse);
        resultApiresourceModel.DisplayName = "TestProxy ";
        // Update ApiResource

        // Call lock user middleware
        var updateApiResourceurl = BaseUrl + ApiRoutePathConstants.UpdateApiResource;
        var updateApiResourceresponse = await FrontChannelClient.PostAsync(
            updateApiResourceurl,
            new StringContent(JsonConvert.SerializeObject(resultApiresourceModel), Encoding.UTF8, "application/json"));
        apiResourceResponse = await updateApiResourceresponse.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<FrameworkResult>(apiResourceResponse);
        result.Should().BeOfType<FrameworkResult>();
        result.Status.Should().Be(ResultStatus.Success);
    }

    [Fact]
    public async Task DeleteApiResourceById_ReturnSuccess()
    {
        await Add_ApiResource_ReturnSuccess();
        // Get ApiResource
        var getApiResourceurl = BaseUrl + ApiRoutePathConstants.GetApiResourceByName;
        var getApiResourceresponse = await FrontChannelClient.PostAsync(
            getApiResourceurl,
            new StringContent(JsonConvert.SerializeObject(apiResourceName), Encoding.UTF8, "application/json"));
        var apiResourceResponse = await getApiResourceresponse.Content.ReadAsStringAsync();
        var resultApiresourceModel = JsonConvert.DeserializeObject<ApiResourcesModel>(apiResourceResponse);

        // Delete ApiResource By Id
        var deleteApiResourceurl = BaseUrl + ApiRoutePathConstants.DeleteApiResourceById;
        var deleteApiResourceresponse = await FrontChannelClient.PostAsync(
            deleteApiResourceurl,
            new StringContent(JsonConvert.SerializeObject(resultApiresourceModel.Id), Encoding.UTF8,
                "application/json"));
        var deleteApiResource = await deleteApiResourceresponse.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<FrameworkResult>(deleteApiResource);
        result.Should().BeOfType<FrameworkResult>();
        result.Status.Should().Be(ResultStatus.Success);
    }

    [Fact]
    public async Task DeleteApiResourceByName_ReturnSuccess()
    {
        await Add_ApiResource_ReturnSuccess();

        // Get ApiResource
        var getApiResourceurl = BaseUrl + ApiRoutePathConstants.GetApiResourceByName;
        var getApiResourceresponse = await FrontChannelClient.PostAsync(
            getApiResourceurl,
            new StringContent(JsonConvert.SerializeObject(apiResourceName), Encoding.UTF8, "application/json"));
        var apiResourceResponse = await getApiResourceresponse.Content.ReadAsStringAsync();
        var resultApiresourceModel = JsonConvert.DeserializeObject<ApiResourcesModel>(apiResourceResponse);

        // Delete ApiResource By Id
        var deleteapiresourceUrl = BaseUrl + ApiRoutePathConstants.DeleteApiResourceByName;
        var deleteapiresourceresponse = await FrontChannelClient.PostAsync(
            deleteapiresourceUrl,
            new StringContent(JsonConvert.SerializeObject(resultApiresourceModel.Name), Encoding.UTF8,
                "application/json"));
        var deleteapiResourceResponse = await deleteapiresourceresponse.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<FrameworkResult>(deleteapiResourceResponse);
        result.Should().BeOfType<FrameworkResult>();
        result.Status.Should().Be(ResultStatus.Success);
    }

    // Get Api Resource

    [Fact]
    public async Task GetApiResource_Success()
    {
        await Add_ApiResource_ReturnSuccess();

        var getApiResourceurl = BaseUrl + ApiRoutePathConstants.GetApiResourceByName;
        var getApiResourceResponse = await FrontChannelClient.PostAsync(
            getApiResourceurl,
            new StringContent(JsonConvert.SerializeObject(apiResourceName), Encoding.UTF8, "application/json"));
        var apiResourceResponse = await getApiResourceResponse.Content.ReadAsStringAsync();
        var resultApiresourceModel = JsonConvert.DeserializeObject<ApiResourcesModel>(apiResourceResponse);
        resultApiresourceModel.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAllApiResource_Success()
    {
        // Get ApiResource
        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);
        var apiResourcesurl = BaseUrl + ApiRoutePathConstants.GetAllApiResources;
        var apiResourcesresponse = await FrontChannelClient.PostAsync(
            apiResourcesurl,
            new StringContent(JsonConvert.SerializeObject(string.Empty), Encoding.UTF8, "application/json"));
        var apiResourceResponse = await apiResourcesresponse.Content.ReadAsStringAsync();
        var resultApiresourceModel = JsonConvert.DeserializeObject<List<ApiResourcesModel>>(apiResourceResponse);
        resultApiresourceModel.Should().NotBeNull();
    }

    [Fact]
    public async Task AddApiResourceClaim_ReturnSuccess()
    {
        var random = new Random();
        await Add_ApiResource_ReturnSuccess();

        // Get ApiResource
        var getApiResourceurl = BaseUrl + ApiRoutePathConstants.GetApiResourceByName;
        var getApiResourceresponse = await FrontChannelClient.PostAsync(
            getApiResourceurl,
            new StringContent(JsonConvert.SerializeObject(apiResourceName), Encoding.UTF8, "application/json"));
        var apiResourceResponse = await getApiResourceresponse.Content.ReadAsStringAsync();
        var resultApiresourceModel = JsonConvert.DeserializeObject<ApiResourcesModel>(apiResourceResponse);

        // Add ApiResourceClaim using the ResourceId from the GetApiResource
        var apiResourcesClaimModelInput = ApiResourceHelper.CreateApiResourceClaimModel();
        apiResourcesClaimModelInput.ApiResourceId = resultApiresourceModel.Id;
        apiResourcesClaimModelInput.Type =
            string.Concat(apiResourcesClaimModelInput.Type, "_", random.Next().ToString());

        var addApiResourceClaimUrl = BaseUrl + ApiRoutePathConstants.AddApiResourceClaim;
        var addApiResourceClaimresponse = await FrontChannelClient.PostAsync(
            addApiResourceClaimUrl,
            new StringContent(JsonConvert.SerializeObject(apiResourcesClaimModelInput), Encoding.UTF8,
                "application/json"));
        var response3 = await addApiResourceClaimresponse.Content.ReadAsStringAsync();
        var frameworkResult2 = JsonConvert.DeserializeObject<FrameworkResult>(response3);
        frameworkResult2.Should().BeOfType<FrameworkResult>();
        frameworkResult2.Status.Should().Be(ResultStatus.Success);
    }

    [Fact]
    public async Task DeleteApiResourceClaim_ReturnSuccess()
    {
        var random = new Random();
        await Add_ApiResource_ReturnSuccess();

        // Get ApiResource
        var getApiResourceUrl = BaseUrl + ApiRoutePathConstants.GetApiResourceByName;
        var response = await FrontChannelClient.PostAsync(
            getApiResourceUrl,
            new StringContent(JsonConvert.SerializeObject(apiResourceName), Encoding.UTF8, "application/json"));
        var apiResourceResponse = await response.Content.ReadAsStringAsync();
        var resultApiresourceModel = JsonConvert.DeserializeObject<ApiResourcesModel>(apiResourceResponse);

        // Add ApiResourceClaim using the ResourceId from the GetApiResource
        var apiResourcesClaimModelInput = ApiResourceHelper.CreateApiResourceClaimModel();
        apiResourcesClaimModelInput.ApiResourceId = resultApiresourceModel.Id;
        apiResourcesClaimModelInput.Type =
            string.Concat(apiResourcesClaimModelInput.Type, "_", random.Next().ToString());

        var addApiResourceClaimurl = BaseUrl + ApiRoutePathConstants.AddApiResourceClaim;
        var addApiResourceClaimresponse = await FrontChannelClient.PostAsync(
            addApiResourceClaimurl,
            new StringContent(JsonConvert.SerializeObject(apiResourcesClaimModelInput), Encoding.UTF8,
                "application/json"));
        var addApiResourceClaimResponse = await addApiResourceClaimresponse.Content.ReadAsStringAsync();
        var addApiResourceClaimFrameworkResult =
            JsonConvert.DeserializeObject<FrameworkResult>(addApiResourceClaimResponse);
        addApiResourceClaimFrameworkResult.Should().BeOfType<FrameworkResult>();
        addApiResourceClaimFrameworkResult.Status.Should().Be(ResultStatus.Success);

        // Delete Api Resource Claim
        var deleteApiResourceClaimUrl = BaseUrl + ApiRoutePathConstants.DeleteApiResourceClaimModel;
        var deleteApiResourceClaimResponse = await FrontChannelClient.PostAsync(
            deleteApiResourceClaimUrl,
            new StringContent(JsonConvert.SerializeObject(apiResourcesClaimModelInput), Encoding.UTF8,
                "application/json"));
        var deleteApiResourceClaimResponseModel = await deleteApiResourceClaimResponse.Content.ReadAsStringAsync();
        var deleteApiresourceModelFrameworkResult =
            JsonConvert.DeserializeObject<FrameworkResult>(deleteApiResourceClaimResponseModel);
        deleteApiresourceModelFrameworkResult.Should().BeOfType<FrameworkResult>();
        deleteApiresourceModelFrameworkResult.Status.Should().Be(ResultStatus.Success);
    }

    [Fact]
    public async Task DeleteApiResourceClaimById_ReturnSuccess()
    {
        var random = new Random();
        await Add_ApiResource_ReturnSuccess();

        // Get ApiResource
        var getApiResourceurl = BaseUrl + ApiRoutePathConstants.GetApiResourceByName;
        var getApiResourceurlresponse = await FrontChannelClient.PostAsync(
            getApiResourceurl,
            new StringContent(JsonConvert.SerializeObject(apiResourceName), Encoding.UTF8, "application/json"));
        var apiResourceResponse = await getApiResourceurlresponse.Content.ReadAsStringAsync();
        var resultApiresourceModel = JsonConvert.DeserializeObject<ApiResourcesModel>(apiResourceResponse);

        // Add ApiResourceClaim using the ResourceId from the GetApiResource
        var apiResourcesClaimModelInput = ApiResourceHelper.CreateApiResourceClaimModel();
        apiResourcesClaimModelInput.ApiResourceId = resultApiresourceModel.Id;
        apiResourcesClaimModelInput.Type =
            string.Concat(apiResourcesClaimModelInput.Type, "_", random.Next().ToString());

        var addApiResourceClaimurl = BaseUrl + ApiRoutePathConstants.AddApiResourceClaim;
        var addApiResourceClaimurlResponse = await FrontChannelClient.PostAsync(
            addApiResourceClaimurl,
            new StringContent(JsonConvert.SerializeObject(apiResourcesClaimModelInput), Encoding.UTF8,
                "application/json"));
        var addApiResourceClaimResponse = await addApiResourceClaimurlResponse.Content.ReadAsStringAsync();
        var addApiResourceClaimFrameworkResult =
            JsonConvert.DeserializeObject<FrameworkResult>(addApiResourceClaimResponse);
        addApiResourceClaimFrameworkResult.Should().BeOfType<FrameworkResult>();
        addApiResourceClaimFrameworkResult.Status.Should().Be(ResultStatus.Success);

        // Delete Api Resource Claim
        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);

        var deleteApiResourceClaimurl = BaseUrl + ApiRoutePathConstants.DeleteApiResourceClaimById;
        var deleteApiResourceClaimurlResponse = await FrontChannelClient.PostAsync(
            deleteApiResourceClaimurl,
            new StringContent(JsonConvert.SerializeObject(resultApiresourceModel.ApiResourceClaims[0].Id),
                Encoding.UTF8, "application/json"));
        var deleteApiResourceClaimResponse = await deleteApiResourceClaimurlResponse.Content.ReadAsStringAsync();
        var deleteApiResourceClaimFrameworkResult =
            JsonConvert.DeserializeObject<FrameworkResult>(deleteApiResourceClaimResponse);
        deleteApiResourceClaimFrameworkResult.Should().BeOfType<FrameworkResult>();
        deleteApiResourceClaimFrameworkResult.Status.Should().Be(ResultStatus.Success);
    }

    [Fact]
    public async Task AddApiScope_ReturnSuccess()
    {
        var random = new Random();
        await Add_ApiResource_ReturnSuccess();

        // Get ApiResource
        var getApiResourceurl = BaseUrl + ApiRoutePathConstants.GetApiResourceByName;
        var getApiResourceUrlResponse = await FrontChannelClient.PostAsync(
            getApiResourceurl,
            new StringContent(JsonConvert.SerializeObject(apiResourceName), Encoding.UTF8, "application/json"));
        var apiResourceResponse = await getApiResourceUrlResponse.Content.ReadAsStringAsync();
        var resultApiresourceModel = JsonConvert.DeserializeObject<ApiResourcesModel>(apiResourceResponse);

        var apiScopeModelInput = ApiResourceHelper.CreateApiScopeModel();
        apiScopeModelInput.Name = string.Concat(ScopeName, "_", random.Next().ToString());
        apiScopeModelInput.ApiResourceId = resultApiresourceModel.Id;

        var addApiScopeUrl = BaseUrl + ApiRoutePathConstants.AddApiScope;
        var addApiScopeUrlresponse = await FrontChannelClient.PostAsync(
            addApiScopeUrl,
            new StringContent(JsonConvert.SerializeObject(apiScopeModelInput), Encoding.UTF8, "application/json"));
        var addApiScopeframeworkResultResponse = await addApiScopeUrlresponse.Content.ReadAsStringAsync();
        var addApiScopeframeworkResult =
            JsonConvert.DeserializeObject<FrameworkResult>(addApiScopeframeworkResultResponse);
        addApiScopeframeworkResult.Should().BeOfType<FrameworkResult>();
        addApiScopeframeworkResult.Status.Should().Be(ResultStatus.Success);
    }

    [Fact]
    public async Task UpdateApiScope_ReturnSuccess()
    {
        var random = new Random();
        await Add_ApiResource_ReturnSuccess();

        // Get ApiResource
        var getApiResourceurl = BaseUrl + ApiRoutePathConstants.GetApiResourceByName;
        var getApiResourceurlresponse = await FrontChannelClient.PostAsync(
            getApiResourceurl,
            new StringContent(JsonConvert.SerializeObject(apiResourceName), Encoding.UTF8, "application/json"));
        var apiResourceResponse = await getApiResourceurlresponse.Content.ReadAsStringAsync();
        var resultApiresourceModel = JsonConvert.DeserializeObject<ApiResourcesModel>(apiResourceResponse);

        var apiScopeModelInput = ApiResourceHelper.CreateApiScopeModel();
        apiScopeModelInput.Name = string.Concat(ScopeName, "_", random.Next().ToString());
        apiScopeModelInput.ApiResourceId = resultApiresourceModel.Id;

        var addApiScopeUrl = BaseUrl + ApiRoutePathConstants.AddApiScope;
        var addApiScopeUrlResponse = await FrontChannelClient.PostAsync(
            addApiScopeUrl,
            new StringContent(JsonConvert.SerializeObject(apiScopeModelInput), Encoding.UTF8, "application/json"));
        var addApiScopeResponse = await addApiScopeUrlResponse.Content.ReadAsStringAsync();
        var addApiScopeResponseFrameworkResult = JsonConvert.DeserializeObject<FrameworkResult>(addApiScopeResponse);
        addApiScopeResponseFrameworkResult.Should().BeOfType<FrameworkResult>();
        addApiScopeResponseFrameworkResult.Status.Should().Be(ResultStatus.Success);

        // Get Api Scope
        var getApiScopeByNameUrl = BaseUrl + ApiRoutePathConstants.GetApiScopeByName;
        var getApiScopeByNameUrlresponse = await FrontChannelClient.PostAsync(
            getApiScopeByNameUrl,
            new StringContent(JsonConvert.SerializeObject(apiScopeModelInput.Name), Encoding.UTF8, "application/json"));
        var apiScopeResponse = await getApiScopeByNameUrlresponse.Content.ReadAsStringAsync();
        var resultApisScopeModel = JsonConvert.DeserializeObject<ApiScopesModel>(apiScopeResponse);

        resultApiresourceModel.Description = "Test scope";

        // Update Api Scope
        var updateApiScopeUrl = BaseUrl + ApiRoutePathConstants.UpdateApiScope;
        var updateApiScopeUrlresponse = await FrontChannelClient.PostAsync(
            updateApiScopeUrl,
            new StringContent(JsonConvert.SerializeObject(resultApisScopeModel), Encoding.UTF8, "application/json"));
        apiResourceResponse = await updateApiScopeUrlresponse.Content.ReadAsStringAsync();
        var apiResourceResponseResult = JsonConvert.DeserializeObject<FrameworkResult>(apiResourceResponse);
        apiResourceResponseResult.Should().BeOfType<FrameworkResult>();
        apiResourceResponseResult.Status.Should().Be(ResultStatus.Success);
    }

    [Fact]
    public async Task DeleteApiScopeByName_ReturnSuccess()
    {
        var random = new Random();
        await Add_ApiResource_ReturnSuccess();

        // Get ApiResource
        var getApiResourceurl = BaseUrl + ApiRoutePathConstants.GetApiResourceByName;
        var getApiResourceUrlResponse = await FrontChannelClient.PostAsync(
            getApiResourceurl,
            new StringContent(JsonConvert.SerializeObject(apiResourceName), Encoding.UTF8, "application/json"));
        var apiResourceResponse = await getApiResourceUrlResponse.Content.ReadAsStringAsync();
        var resultApiresourceModel = JsonConvert.DeserializeObject<ApiResourcesModel>(apiResourceResponse);

        var apiScopeModelInput = ApiResourceHelper.CreateApiScopeModel();
        apiScopeModelInput.Name = string.Concat(ScopeName, "_", random.Next().ToString());
        apiScopeModelInput.ApiResourceId = resultApiresourceModel.Id;

        var addApiScopeUrl = BaseUrl + ApiRoutePathConstants.AddApiScope;
        var addApiScopeUrlResponse = await FrontChannelClient.PostAsync(
            addApiScopeUrl,
            new StringContent(JsonConvert.SerializeObject(apiScopeModelInput), Encoding.UTF8, "application/json"));
        var addApiScopeUrlResponseresponse = await addApiScopeUrlResponse.Content.ReadAsStringAsync();
        var addApiScopeframeworkResult = JsonConvert.DeserializeObject<FrameworkResult>(addApiScopeUrlResponseresponse);
        addApiScopeframeworkResult.Should().BeOfType<FrameworkResult>();
        addApiScopeframeworkResult.Status.Should().Be(ResultStatus.Success);

        // Delete Api Scope
        var deleteApiScopeUrl = BaseUrl + ApiRoutePathConstants.DeleteApiScopeByName;
        var deleteApiScopeUrlResponse = await FrontChannelClient.PostAsync(
            deleteApiScopeUrl,
            new StringContent(JsonConvert.SerializeObject(apiScopeModelInput.Name), Encoding.UTF8, "application/json"));
        var deleteApiScopeResponse = await deleteApiScopeUrlResponse.Content.ReadAsStringAsync();
        var frameworkResult = JsonConvert.DeserializeObject<FrameworkResult>(deleteApiScopeResponse);
        frameworkResult.Should().BeOfType<FrameworkResult>();
        frameworkResult.Status.Should().Be(ResultStatus.Success);
    }

    [Fact]
    public async Task DeleteApiScopeById_ReturnSuccess()
    {
        var random = new Random();
        await Add_ApiResource_ReturnSuccess();

        // Get ApiResource
        var getApiResourceUrl = BaseUrl + ApiRoutePathConstants.GetApiResourceByName;
        var getApiResourceUrlresponse = await FrontChannelClient.PostAsync(
            getApiResourceUrl,
            new StringContent(JsonConvert.SerializeObject(apiResourceName), Encoding.UTF8, "application/json"));
        var apiResourceResponse = await getApiResourceUrlresponse.Content.ReadAsStringAsync();
        var resultApiresourceModel = JsonConvert.DeserializeObject<ApiResourcesModel>(apiResourceResponse);

        var apiScopeModelInput = ApiResourceHelper.CreateApiScopeModel();
        apiScopeModelInput.Name = string.Concat(ScopeName, "_", random.Next().ToString());
        apiScopeModelInput.ApiResourceId = resultApiresourceModel.Id;

        var addApiScopeUrl = BaseUrl + ApiRoutePathConstants.AddApiScope;
        var addApiScopeResponse = await FrontChannelClient.PostAsync(
            addApiScopeUrl,
            new StringContent(JsonConvert.SerializeObject(apiScopeModelInput), Encoding.UTF8, "application/json"));
        var addApiScoperesponse = await addApiScopeResponse.Content.ReadAsStringAsync();
        var addApiScopeframeworkResult = JsonConvert.DeserializeObject<FrameworkResult>(addApiScoperesponse);
        addApiScopeframeworkResult.Should().BeOfType<FrameworkResult>();
        addApiScopeframeworkResult.Status.Should().Be(ResultStatus.Success);

        // Get Api Scope
        var getApiScopeByNameUrl = BaseUrl + ApiRoutePathConstants.GetApiScopeByName;
        var getApiScopeResponse = await FrontChannelClient.PostAsync(
            getApiScopeByNameUrl,
            new StringContent(JsonConvert.SerializeObject(apiScopeModelInput.Name), Encoding.UTF8, "application/json"));
        var apiScopeResponse = await getApiScopeResponse.Content.ReadAsStringAsync();
        var resultApiScopeModel = JsonConvert.DeserializeObject<ApiScopesModel>(apiScopeResponse);

        // Delete Api Scope
        var deleteApiScopeUrl = BaseUrl + ApiRoutePathConstants.DeleteApiScopeById;
        var deleteApiScopeUrlresponse = await FrontChannelClient.PostAsync(
            deleteApiScopeUrl,
            new StringContent(JsonConvert.SerializeObject(resultApiScopeModel.Id), Encoding.UTF8, "application/json"));
        var deleteApiScopeResponse = await deleteApiScopeUrlresponse.Content.ReadAsStringAsync();
        var deleteApiScopeFrameworkResult = JsonConvert.DeserializeObject<FrameworkResult>(deleteApiScopeResponse);
        deleteApiScopeFrameworkResult.Should().BeOfType<FrameworkResult>();
        deleteApiScopeFrameworkResult.Status.Should().Be(ResultStatus.Success);
    }

    [Fact]
    [Trait("Category", "AddSuccessCase")]
    public async Task AddApiScopeClaimAsync_Success()
    {
        await Add_ApiResource_ReturnSuccess();

        // Get ApiResource
        var getApiResourceurl = BaseUrl + ApiRoutePathConstants.GetApiResourceByName;
        var getApiResourceResponse = await FrontChannelClient.PostAsync(
            getApiResourceurl,
            new StringContent(JsonConvert.SerializeObject(apiResourceName), Encoding.UTF8, "application/json"));
        var apiResourceResponse = await getApiResourceResponse.Content.ReadAsStringAsync();
        var resultApiresourceModel = JsonConvert.DeserializeObject<ApiResourcesModel>(apiResourceResponse);

        var apiScopeModelInput = ApiResourceHelper.CreateApiScopeModel();
        apiScopeModelInput.Name = string.Concat(ScopeName, "_", random.Next().ToString());
        apiScopeModelInput.ApiResourceId = resultApiresourceModel.Id;

        var addApiScopeurl = BaseUrl + ApiRoutePathConstants.AddApiScope;
        var addApiScopeUrlResponse = await FrontChannelClient.PostAsync(
            addApiScopeurl,
            new StringContent(JsonConvert.SerializeObject(apiScopeModelInput), Encoding.UTF8, "application/json"));
        var addApiScopeResponse = await addApiScopeUrlResponse.Content.ReadAsStringAsync();
        var addApiScopeFrameworkResult = JsonConvert.DeserializeObject<FrameworkResult>(addApiScopeResponse);
        addApiScopeFrameworkResult.Should().BeOfType<FrameworkResult>();
        addApiScopeFrameworkResult.Status.Should().Be(ResultStatus.Success);

        // Get Api Scope
        var getApiScopeUrl = BaseUrl + ApiRoutePathConstants.GetApiScopeByName;
        var getApiScopeUrlresponse = await FrontChannelClient.PostAsync(
            getApiScopeUrl,
            new StringContent(JsonConvert.SerializeObject(apiScopeModelInput.Name), Encoding.UTF8, "application/json"));
        var apiResourceResponse2 = await getApiScopeUrlresponse.Content.ReadAsStringAsync();
        var resultApiresourceModel2 = JsonConvert.DeserializeObject<ApiScopesModel>(apiResourceResponse2);

        var apiScopeClaimModelInput = ApiResourceHelper.CreateApiScopeClaimModel();
        apiScopeClaimModelInput.ApiScopeId = resultApiresourceModel2.Id;
        apiScopeClaimModelInput.Type = string.Concat("CR7", "_", random.Next().ToString());

        // Add Scope Claim
        var addApiScopeClaimUrl = BaseUrl + ApiRoutePathConstants.AddApiScopeClaim;
        var addApiScopeClaimUrlResponse = await FrontChannelClient.PostAsync(
            addApiScopeClaimUrl,
            new StringContent(JsonConvert.SerializeObject(apiScopeClaimModelInput), Encoding.UTF8, "application/json"));
        var addApiScopeClaimResponse = await addApiScopeClaimUrlResponse.Content.ReadAsStringAsync();
        var addApiScopeClaimResponseResult = JsonConvert.DeserializeObject<FrameworkResult>(addApiScopeClaimResponse);
        addApiScopeClaimResponseResult.Should().BeOfType<FrameworkResult>();
        addApiScopeClaimResponseResult.Status.Should().Be(ResultStatus.Success);
    }

    [Fact]
    [Trait("Category", "AddSuccessCase")]
    public async Task DeleteApiScopeClaimAsyncByScopeId_Success()
    {
        var random = new Random();
        await Add_ApiResource_ReturnSuccess();

        // Get ApiResource
        var getApiResourceurl = BaseUrl + ApiRoutePathConstants.GetApiResourceByName;
        var getApiResourceurlResponse = await FrontChannelClient.PostAsync(
            getApiResourceurl,
            new StringContent(JsonConvert.SerializeObject(apiResourceName), Encoding.UTF8, "application/json"));
        var apiResourceResponse = await getApiResourceurlResponse.Content.ReadAsStringAsync();
        var resultApiresourceModel = JsonConvert.DeserializeObject<ApiResourcesModel>(apiResourceResponse);

        var apiScopeModelInput = ApiResourceHelper.CreateApiScopeModel();
        apiScopeModelInput.Name = string.Concat(ScopeName, "_", random.Next().ToString());
        apiScopeModelInput.ApiResourceId = resultApiresourceModel.Id;

        var addApiScopeUrl = BaseUrl + ApiRoutePathConstants.AddApiScope;
        var addApiScopeUrlresponse = await FrontChannelClient.PostAsync(
            addApiScopeUrl,
            new StringContent(JsonConvert.SerializeObject(apiScopeModelInput), Encoding.UTF8, "application/json"));
        var addApiScopeResponse = await addApiScopeUrlresponse.Content.ReadAsStringAsync();
        var addApiScopeResponseResult = JsonConvert.DeserializeObject<FrameworkResult>(addApiScopeResponse);
        addApiScopeResponseResult.Should().BeOfType<FrameworkResult>();
        addApiScopeResponseResult.Status.Should().Be(ResultStatus.Success);

        // Get Api Scope
        var getApiScopeurl = BaseUrl + ApiRoutePathConstants.GetApiScopeByName;
        var getApiScopeurlResponse = await FrontChannelClient.PostAsync(
            getApiScopeurl,
            new StringContent(JsonConvert.SerializeObject(apiScopeModelInput.Name), Encoding.UTF8, "application/json"));
        var apiResourceResponse2 = await getApiScopeurlResponse.Content.ReadAsStringAsync();
        var resultApiresourceModel2 = JsonConvert.DeserializeObject<ApiScopesModel>(apiResourceResponse2);

        // Deelete Scope Claim
        var deleteApiScopeClaimUrl = BaseUrl + ApiRoutePathConstants.DeleteApiScopeClaimModel;
        var deleteApiScopeClaimUrlResponse = await FrontChannelClient.PostAsync(
            deleteApiScopeClaimUrl,
            new StringContent(JsonConvert.SerializeObject(resultApiresourceModel2.ApiScopeClaims[0]), Encoding.UTF8,
                "application/json"));
        var deleteApiScopeClaimResponse = await deleteApiScopeClaimUrlResponse.Content.ReadAsStringAsync();
        var deleteApiScopeClaimResponseResult =
            JsonConvert.DeserializeObject<FrameworkResult>(deleteApiScopeClaimResponse);
        deleteApiScopeClaimResponseResult.Should().BeOfType<FrameworkResult>();
        deleteApiScopeClaimResponseResult.Status.Should().Be(ResultStatus.Success);
    }

    [Fact]
    [Trait("Category", "AddSuccessCase")]
    public async Task DeleteApiScopeClaimAsync_Success()
    {
        await Add_ApiResource_ReturnSuccess();

        // Get ApiResource
        var getApiResourceurl = BaseUrl + ApiRoutePathConstants.GetApiResourceByName;
        var getApiResourceurlresponse = await FrontChannelClient.PostAsync(
            getApiResourceurl,
            new StringContent(JsonConvert.SerializeObject(apiResourceName), Encoding.UTF8, "application/json"));
        var apiResourceResponse = await getApiResourceurlresponse.Content.ReadAsStringAsync();
        var resultApiresourceModel = JsonConvert.DeserializeObject<ApiResourcesModel>(apiResourceResponse);

        var apiScopeModelInput = ApiResourceHelper.CreateApiScopeModel();
        apiScopeModelInput.Name = string.Concat(ScopeName, "_", random.Next().ToString());
        apiScopeModelInput.ApiResourceId = resultApiresourceModel.Id;

        var addApiScopeUrl = BaseUrl + ApiRoutePathConstants.AddApiScope;
        var addApiScopeUrlResponse = await FrontChannelClient.PostAsync(
            addApiScopeUrl,
            new StringContent(JsonConvert.SerializeObject(apiScopeModelInput), Encoding.UTF8, "application/json"));
        var addApiScopeResponse = await addApiScopeUrlResponse.Content.ReadAsStringAsync();
        var addApiScopeResponseResult = JsonConvert.DeserializeObject<FrameworkResult>(addApiScopeResponse);
        addApiScopeResponseResult.Should().BeOfType<FrameworkResult>();
        addApiScopeResponseResult.Status.Should().Be(ResultStatus.Success);

        // Get Api Scope
        var getApiScopeUrl = BaseUrl + ApiRoutePathConstants.GetApiScopeByName;
        var getApiScopeUrlResponse = await FrontChannelClient.PostAsync(
            getApiScopeUrl,
            new StringContent(JsonConvert.SerializeObject(apiScopeModelInput.Name), Encoding.UTF8, "application/json"));
        var getApiScopeResponse = await getApiScopeUrlResponse.Content.ReadAsStringAsync();
        var resultGetApiresourceModel = JsonConvert.DeserializeObject<ApiScopesModel>(getApiScopeResponse);

        var apiScopeClaimModelInput = ApiResourceHelper.CreateApiScopeClaimModel();
        apiScopeClaimModelInput.ApiScopeId = resultGetApiresourceModel.Id;
        apiScopeClaimModelInput.Type = string.Concat("CR7", "_", random.Next().ToString());
        // Get Api Scope Calim
        var getApiScopeClaimsUrl = BaseUrl + ApiRoutePathConstants.GetApiScopeClaims;
        var getApiScopeClaimsUrlResponse = await FrontChannelClient.PostAsync(
            getApiScopeClaimsUrl,
            new StringContent(JsonConvert.SerializeObject(resultGetApiresourceModel.Id), Encoding.UTF8,
                "application/json"));
        var getApiScopeClaimsResponse = await getApiScopeClaimsUrlResponse.Content.ReadAsStringAsync();
        var resultApiresourceModel3 =
            JsonConvert.DeserializeObject<List<ApiScopeClaimsModel>>(getApiScopeClaimsResponse);

        // Delete ApiScope Claim
        var deleteApiScopeClaimModelurl = BaseUrl + ApiRoutePathConstants.DeleteApiScopeClaimModel;
        var deleteApiScopeClaimModelurlresponse = await FrontChannelClient.PostAsync(
            deleteApiScopeClaimModelurl,
            new StringContent(JsonConvert.SerializeObject(resultApiresourceModel3[0]), Encoding.UTF8,
                "application/json"));
        var deleteApiScopeClaimModelResponse = await deleteApiScopeClaimModelurlresponse.Content.ReadAsStringAsync();
        var deleteApiScopeResult = JsonConvert.DeserializeObject<FrameworkResult>(deleteApiScopeClaimModelResponse);
        deleteApiScopeResult.Should().BeOfType<FrameworkResult>();
        deleteApiScopeResult.Status.Should().Be(ResultStatus.Success);
    }

    [Fact]
    [Trait("Category", "AddErrorCase")]
    public async Task AddApiResourceAsync_NameIsNull_ReturnsError()
    {
        try
        {
            var apiResourcesModelInput = ApiResourceHelper.CreateApiResourceModel();
            apiResourcesModelInput.Name = null;

            var token = await GetAccessToken();
            FrontChannelClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token.access_token);

            var addApiResourceurl = BaseUrl + ApiRoutePathConstants.AddApiResource;
            var addApiResourceresponse = await FrontChannelClient.PostAsync(
                addApiResourceurl,
                new StringContent(JsonConvert.SerializeObject(apiResourcesModelInput), Encoding.UTF8,
                    "application/json"));

            var apiResourceResponse = await addApiResourceresponse.Content.ReadAsStringAsync();
            var frameworkResult = JsonConvert.DeserializeObject<FrameworkResult>(apiResourceResponse);

            frameworkResult.Should().BeOfType<FrameworkResult>();
            frameworkResult.Status.Should().Be(ResultStatus.Failed);
            frameworkResult.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.ApiResourceNameRequired);
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    [Fact]
    [Trait("Category", "AddErrorCase")]
    public async Task AddApiResourceAsync_Name_LengthExceededLimit_DBExceptionThrown()
    {
        var apiResourcesModelInput = ApiResourceHelper.CreateApiResourceModel();
        apiResourcesModelInput.Name =
            "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);

        var addApiResourceurl = BaseUrl + ApiRoutePathConstants.AddApiResource;
        var addApiResourceresponse = await FrontChannelClient.PostAsync(
            addApiResourceurl,
            new StringContent(JsonConvert.SerializeObject(apiResourcesModelInput), Encoding.UTF8, "application/json"));

        var apiResourceResponse = await addApiResourceresponse.Content.ReadAsStringAsync();
        var frameworkResult = JsonConvert.DeserializeObject<FrameworkResult>(apiResourceResponse);

        frameworkResult.Should().BeOfType<FrameworkResult>();
        frameworkResult.Status.Should().Be(ResultStatus.Failed);
        frameworkResult.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.ApiResourceNameTooLong);
    }

    [Fact]
    [Trait("Category", "AddErrorCase")]
    public async Task AddApiResourceAsync_DisplayName_LengthExceededLimit_DBExceptionThrown()
    {
        var apiResourcesModelInput = ApiResourceHelper.CreateApiResourceModel();
        apiResourcesModelInput.Name = string.Concat("IntegrationTest", "_", random.Next().ToString());
        apiResourcesModelInput.DisplayName =
            "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);

        var addApiResourceurl = BaseUrl + ApiRoutePathConstants.AddApiResource;
        var addApiResourceresponse = await FrontChannelClient.PostAsync(
            addApiResourceurl,
            new StringContent(JsonConvert.SerializeObject(apiResourcesModelInput), Encoding.UTF8, "application/json"));

        var apiResourceResponse = await addApiResourceresponse.Content.ReadAsStringAsync();
        var frameworkResult = JsonConvert.DeserializeObject<FrameworkResult>(apiResourceResponse);

        frameworkResult.Should().BeOfType<FrameworkResult>();
        frameworkResult.Status.Should().Be(ResultStatus.Failed);
        frameworkResult.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.ApiResourceDisplayNameTooLong);
    }

    [Fact]
    [Trait("Category", "AddErrorCase")]
    public async Task AddApiResourceAsync_DuplicateExists_ForGivenResourceName_ReturnsError_AddFailure()
    {
        await Add_ApiResource_ReturnSuccess();
        var apiResourcesModelInput = ApiResourceHelper.CreateApiResourceModel();
        apiResourcesModelInput.Name = apiResourceName;

        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);

        var addApiResourceurl = BaseUrl + ApiRoutePathConstants.AddApiResource;
        var addApiResourceresponse = await FrontChannelClient.PostAsync(
            addApiResourceurl,
            new StringContent(JsonConvert.SerializeObject(apiResourcesModelInput), Encoding.UTF8, "application/json"));

        var apiResourceResponse = await addApiResourceresponse.Content.ReadAsStringAsync();
        var frameworkResult = JsonConvert.DeserializeObject<FrameworkResult>(apiResourceResponse);

        frameworkResult.Should().BeOfType<FrameworkResult>();
        frameworkResult.Status.Should().Be(ResultStatus.Failed);
        frameworkResult.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.ApiResourceAlreadyExists);
    }

    [Fact]
    [Trait("Category", "AddErrorCase")]
    public async Task AddApiScope_Name_IsEmpty_ErrorReturned_AddFailure()
    {
        await Add_ApiResource_ReturnSuccess();

        // Get ApiResource
        var getApiResourceurl = BaseUrl + ApiRoutePathConstants.GetApiResourceByName;
        var getApiResourceUrlResponse = await FrontChannelClient.PostAsync(
            getApiResourceurl,
            new StringContent(JsonConvert.SerializeObject(apiResourceName), Encoding.UTF8, "application/json"));
        var apiResourceResponse = await getApiResourceUrlResponse.Content.ReadAsStringAsync();
        var resultApiresourceModel = JsonConvert.DeserializeObject<ApiResourcesModel>(apiResourceResponse);

        var apiScopeModelInput = ApiResourceHelper.CreateApiScopeModel();
        // Arrange
        apiScopeModelInput.Name = string.Empty;
        apiScopeModelInput.ApiResourceId = resultApiresourceModel.Id;

        // Act
        var addApiScopeUrl = BaseUrl + ApiRoutePathConstants.AddApiScope;
        var addApiScopeUrlresponse = await FrontChannelClient.PostAsync(
            addApiScopeUrl,
            new StringContent(JsonConvert.SerializeObject(apiScopeModelInput), Encoding.UTF8, "application/json"));
        var addApiScopeframeworkResultResponse = await addApiScopeUrlresponse.Content.ReadAsStringAsync();
        var addApiScopeframeworkResult =
            JsonConvert.DeserializeObject<FrameworkResult>(addApiScopeframeworkResultResponse);

        // Assert
        addApiScopeframeworkResult.Should().BeOfType<FrameworkResult>();
        addApiScopeframeworkResult.Status.Should().Be(ResultStatus.Failed);
        addApiScopeframeworkResult.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.ApiScopeNameRequired);
    }

    [Fact]
    [Trait("Category", "AddErrorCase")]
    public async Task AddApiScope_ApiScopeName_LengthExceeded_ThrowDbException()
    {
        await Add_ApiResource_ReturnSuccess();

        // Get ApiResource
        var getApiResourceurl = BaseUrl + ApiRoutePathConstants.GetApiResourceByName;
        var getApiResourceUrlResponse = await FrontChannelClient.PostAsync(
            getApiResourceurl,
            new StringContent(JsonConvert.SerializeObject(apiResourceName), Encoding.UTF8, "application/json"));
        var apiResourceResponse = await getApiResourceUrlResponse.Content.ReadAsStringAsync();
        var resultApiresourceModel = JsonConvert.DeserializeObject<ApiResourcesModel>(apiResourceResponse);

        var apiScopeModelInput = ApiResourceHelper.CreateApiScopeModel();
        // Arrange
        apiScopeModelInput.Name =
            "hhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllll";
        apiScopeModelInput.ApiResourceId = resultApiresourceModel.Id;

        // Act
        var addApiScopeUrl = BaseUrl + ApiRoutePathConstants.AddApiScope;
        var addApiScopeUrlresponse = await FrontChannelClient.PostAsync(
            addApiScopeUrl,
            new StringContent(JsonConvert.SerializeObject(apiScopeModelInput), Encoding.UTF8, "application/json"));
        var addApiScopeframeworkResultResponse = await addApiScopeUrlresponse.Content.ReadAsStringAsync();
        var addApiScopeframeworkResult =
            JsonConvert.DeserializeObject<FrameworkResult>(addApiScopeframeworkResultResponse);

        // Assert
        addApiScopeframeworkResult.Should().BeOfType<FrameworkResult>();
        addApiScopeframeworkResult.Status.Should().Be(ResultStatus.Failed);
        addApiScopeframeworkResult.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.ApiScopeNameTooLong);
    }

    [Fact]
    public async Task AddApiScope_DuplicateExists_ReturnsError_AddFailure()
    {
        await Add_ApiResource_ReturnSuccess();

        // Get ApiResource
        var getApiResourceurl = BaseUrl + ApiRoutePathConstants.GetApiResourceByName;
        var getApiResourceUrlResponse = await FrontChannelClient.PostAsync(
            getApiResourceurl,
            new StringContent(JsonConvert.SerializeObject(apiResourceName), Encoding.UTF8, "application/json"));
        var apiResourceResponse = await getApiResourceUrlResponse.Content.ReadAsStringAsync();
        var resultApiresourceModel = JsonConvert.DeserializeObject<ApiResourcesModel>(apiResourceResponse);

        var apiScopeModelInput = ApiResourceHelper.CreateApiScopeModel();
        apiScopeModelInput.Name = string.Concat(ScopeName, "_", random.Next().ToString());
        apiScopeModelInput.ApiResourceId = resultApiresourceModel.Id;

        var addApiScopeUrl = BaseUrl + ApiRoutePathConstants.AddApiScope;
        var addApiScopeUrlresponse = await FrontChannelClient.PostAsync(
            addApiScopeUrl,
            new StringContent(JsonConvert.SerializeObject(apiScopeModelInput), Encoding.UTF8, "application/json"));

        addApiScopeUrl = BaseUrl + ApiRoutePathConstants.AddApiScope;
        addApiScopeUrlresponse = await FrontChannelClient.PostAsync(
            addApiScopeUrl,
            new StringContent(JsonConvert.SerializeObject(apiScopeModelInput), Encoding.UTF8, "application/json"));

        var addApiScopeframeworkResultResponse = await addApiScopeUrlresponse.Content.ReadAsStringAsync();
        var addApiScopeframeworkResult =
            JsonConvert.DeserializeObject<FrameworkResult>(addApiScopeframeworkResultResponse);
        addApiScopeframeworkResult.Should().BeOfType<FrameworkResult>();
        addApiScopeframeworkResult.Status.Should().Be(ResultStatus.Failed);
        addApiScopeframeworkResult.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.ApiScopeAlreadyExists);
    }

    [Fact]
    public async Task AddApiResourceClaim_DuplicateExists_ForResourceIdAndType_ReturnsError_AddFailure()
    {
        await Add_ApiResource_ReturnSuccess();

        // Get ApiResource
        var getApiResourceurl = BaseUrl + ApiRoutePathConstants.GetApiResourceByName;
        var getApiResourceresponse = await FrontChannelClient.PostAsync(
            getApiResourceurl,
            new StringContent(JsonConvert.SerializeObject(apiResourceName), Encoding.UTF8, "application/json"));
        var apiResourceResponse = await getApiResourceresponse.Content.ReadAsStringAsync();
        var resultApiresourceModel = JsonConvert.DeserializeObject<ApiResourcesModel>(apiResourceResponse);

        // Add ApiResourceClaim using the ResourceId from the GetApiResource
        var apiResourcesClaimModelInput = ApiResourceHelper.CreateApiResourceClaimModel();
        apiResourcesClaimModelInput.ApiResourceId = resultApiresourceModel.Id;
        apiResourcesClaimModelInput.Type =
            string.Concat(apiResourcesClaimModelInput.Type, "_", random.Next().ToString());

        var addApiResourceClaimUrl = BaseUrl + ApiRoutePathConstants.AddApiResourceClaim;
        var addApiResourceClaimresponse = await FrontChannelClient.PostAsync(
            addApiResourceClaimUrl,
            new StringContent(JsonConvert.SerializeObject(apiResourcesClaimModelInput), Encoding.UTF8,
                "application/json"));

        addApiResourceClaimUrl = BaseUrl + ApiRoutePathConstants.AddApiResourceClaim;
        addApiResourceClaimresponse = await FrontChannelClient.PostAsync(
            addApiResourceClaimUrl,
            new StringContent(JsonConvert.SerializeObject(apiResourcesClaimModelInput), Encoding.UTF8,
                "application/json"));

        var resourceClaimResponse = await addApiResourceClaimresponse.Content.ReadAsStringAsync();
        var frameworkResult2 = JsonConvert.DeserializeObject<FrameworkResult>(resourceClaimResponse);
        frameworkResult2.Should().BeOfType<FrameworkResult>();
        frameworkResult2.Status.Should().Be(ResultStatus.Failed);
        frameworkResult2.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.ApiResourceClaimAlreadyExists);
    }

    [Fact]
    public async Task AddApiResourceClaim_Type_LengthExceedsLimit_ThrowException()
    {
        await Add_ApiResource_ReturnSuccess();

        // Get ApiResource
        var getApiResourceurl = BaseUrl + ApiRoutePathConstants.GetApiResourceByName;
        var getApiResourceresponse = await FrontChannelClient.PostAsync(
            getApiResourceurl,
            new StringContent(JsonConvert.SerializeObject(apiResourceName), Encoding.UTF8, "application/json"));
        var apiResourceResponse = await getApiResourceresponse.Content.ReadAsStringAsync();
        var resultApiresourceModel = JsonConvert.DeserializeObject<ApiResourcesModel>(apiResourceResponse);

        // Add ApiResourceClaim using the ResourceId from the GetApiResource
        var apiResourcesClaimModelInput = ApiResourceHelper.CreateApiResourceClaimModel();
        apiResourcesClaimModelInput.ApiResourceId = resultApiresourceModel.Id;
        apiResourcesClaimModelInput.Type =
            "hhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllll";

        var addApiResourceClaimUrl = BaseUrl + ApiRoutePathConstants.AddApiResourceClaim;
        var addApiResourceClaimresponse = await FrontChannelClient.PostAsync(
            addApiResourceClaimUrl,
            new StringContent(JsonConvert.SerializeObject(apiResourcesClaimModelInput), Encoding.UTF8,
                "application/json"));

        var resourceClaimResponse = await addApiResourceClaimresponse.Content.ReadAsStringAsync();
        var frameworkResult2 = JsonConvert.DeserializeObject<FrameworkResult>(resourceClaimResponse);
        frameworkResult2.Should().BeOfType<FrameworkResult>();
        frameworkResult2.Status.Should().Be(ResultStatus.Failed);
        frameworkResult2.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.ApiResourceClaimTypeTooLong);
    }

    [Fact]
    public async Task UpdateApiResource_ConcurrencyError()
    {
        await Add_ApiResource_ReturnSuccess();
        // Get ApiResource
        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);

        // Get Model 1
        var getApiResourceurl = BaseUrl + ApiRoutePathConstants.GetApiResourceByName;
        var getApiResourceresponse = await FrontChannelClient.PostAsync(
            getApiResourceurl,
            new StringContent(JsonConvert.SerializeObject(apiResourceName), Encoding.UTF8, "application/json"));
        var apiResourceResponse = await getApiResourceresponse.Content.ReadAsStringAsync();
        var resultApiresourceModel = JsonConvert.DeserializeObject<ApiResourcesModel>(apiResourceResponse);

        // Get Model 2
        getApiResourceurl = BaseUrl + ApiRoutePathConstants.GetApiResourceByName;
        getApiResourceresponse = await FrontChannelClient.PostAsync(
            getApiResourceurl,
            new StringContent(JsonConvert.SerializeObject(apiResourceName), Encoding.UTF8, "application/json"));
        apiResourceResponse = await getApiResourceresponse.Content.ReadAsStringAsync();
        var resultApiresourceModel2 = JsonConvert.DeserializeObject<ApiResourcesModel>(apiResourceResponse);

        // Update ApiResource
        resultApiresourceModel2.DisplayName = "Test1";
        resultApiresourceModel2.ModifiedBy = "Roshan";
        resultApiresourceModel.Description = "Jesu";
        resultApiresourceModel.DisplayName = "Suresh";
        resultApiresourceModel.ModifiedBy = "Jesu";

        // Call lock user middleware
        var updateApiResourceurl = BaseUrl + ApiRoutePathConstants.UpdateApiResource;
        var updateApiResourceresponse = await FrontChannelClient.PostAsync(
            updateApiResourceurl,
            new StringContent(JsonConvert.SerializeObject(resultApiresourceModel2), Encoding.UTF8, "application/json"));

        updateApiResourceurl = BaseUrl + ApiRoutePathConstants.UpdateApiResource;
        updateApiResourceresponse = await FrontChannelClient.PostAsync(
            updateApiResourceurl,
            new StringContent(JsonConvert.SerializeObject(resultApiresourceModel), Encoding.UTF8, "application/json"));
        apiResourceResponse = await updateApiResourceresponse.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<FrameworkResult>(apiResourceResponse);
        result.Should().BeOfType<FrameworkResult>();
        result.Status.Should().Be(ResultStatus.Failed);
        result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.ConcurrencyFailure);
    }

    [Fact]
    public async Task UpdateApiResource_Name_LengthExceedsLimit_ThrowsException()
    {
        await Add_ApiResource_ReturnSuccess();
        // Get ApiResource
        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);

        var getApiResourceurl = BaseUrl + ApiRoutePathConstants.GetApiResourceByName;
        var getApiResourceresponse = await FrontChannelClient.PostAsync(
            getApiResourceurl,
            new StringContent(JsonConvert.SerializeObject(apiResourceName), Encoding.UTF8, "application/json"));
        var apiResourceResponse = await getApiResourceresponse.Content.ReadAsStringAsync();
        var resultApiresourceModel = JsonConvert.DeserializeObject<ApiResourcesModel>(apiResourceResponse);
        resultApiresourceModel.Name =
            "hhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllll";
        // Update ApiResource

        // Call lock user middleware
        var updateApiResourceurl = BaseUrl + ApiRoutePathConstants.UpdateApiResource;
        var updateApiResourceresponse = await FrontChannelClient.PostAsync(
            updateApiResourceurl,
            new StringContent(JsonConvert.SerializeObject(resultApiresourceModel), Encoding.UTF8, "application/json"));
        apiResourceResponse = await updateApiResourceresponse.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<FrameworkResult>(apiResourceResponse);
        result.Should().BeOfType<FrameworkResult>();
        result.Status.Should().Be(ResultStatus.Failed);
        result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.ApiResourceNameTooLong);
    }

    [Fact]
    public async Task UpdateApiResource_NoActiveRecordsFound_ForGivenResourceID_ReturnsError()
    {
        await Add_ApiResource_ReturnSuccess();
        // Get ApiResource
        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);

        var getApiResourceurl = BaseUrl + ApiRoutePathConstants.GetApiResourceByName;
        var getApiResourceresponse = await FrontChannelClient.PostAsync(
            getApiResourceurl,
            new StringContent(JsonConvert.SerializeObject(apiResourceName), Encoding.UTF8, "application/json"));
        var apiResourceResponse = await getApiResourceresponse.Content.ReadAsStringAsync();
        var resultApiresourceModel = JsonConvert.DeserializeObject<ApiResourcesModel>(apiResourceResponse);

        // Delete Api
        var deleteApiResourceurl = BaseUrl + ApiRoutePathConstants.DeleteApiResourceById;
        var deleteApiResourceresponse = await FrontChannelClient.PostAsync(
            deleteApiResourceurl,
            new StringContent(JsonConvert.SerializeObject(resultApiresourceModel.Id), Encoding.UTF8,
                "application/json"));
        var deleteapiResourceResponse = await deleteApiResourceresponse.Content.ReadAsStringAsync();
        var deleteResult = JsonConvert.DeserializeObject<FrameworkResult>(deleteapiResourceResponse);
        deleteResult.Should().BeOfType<FrameworkResult>();
        deleteResult.Status.Should().Be(ResultStatus.Success);
        resultApiresourceModel.Name = "TestProxy";
        // Update ApiResource

        // Call lock user middleware
        var updateApiResourceurl = BaseUrl + ApiRoutePathConstants.UpdateApiResource;
        var updateApiResourceresponse = await FrontChannelClient.PostAsync(
            updateApiResourceurl,
            new StringContent(JsonConvert.SerializeObject(resultApiresourceModel), Encoding.UTF8, "application/json"));
        apiResourceResponse = await updateApiResourceresponse.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<FrameworkResult>(apiResourceResponse);
        result.Should().BeOfType<FrameworkResult>();
        result.Status.Should().Be(ResultStatus.Failed);
        result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.ApiResourceIdInvalid);
    }

    [Fact]
    public async Task DeleteApiResource_ByName_NoRecordsFound_ReturnsError_InvalidResourceName()
    {
        // Delete API Resource by invalid resourceName
        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);
        var invalidResourceName = string.Concat(apiResourceName, "_", random.Next().ToString());
        var deleteApiResourceurl = BaseUrl + ApiRoutePathConstants.DeleteApiResourceByName;
        var deleteApiResourceresponse = await FrontChannelClient.PostAsync(
            deleteApiResourceurl,
            new StringContent(JsonConvert.SerializeObject(invalidResourceName), Encoding.UTF8, "application/json"));
        var deleteApiResource = await deleteApiResourceresponse.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<FrameworkResult>(deleteApiResource);
        result.Should().BeOfType<FrameworkResult>();
        result.Status.Should().Be(ResultStatus.Failed);
        result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.ApiResourceNameInvalid);
    }

    [Fact]
    public async Task DeleteApiResourceClaim_ById_NoRecordsFound_ReturnsError_InvalidResourceId()
    {
        // Arrange
        var invalidResourceIdInput = Guid.NewGuid();
        // Delete API Resource by invalid resourceName
        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);
        var deleteApiResourceurl = BaseUrl + ApiRoutePathConstants.DeleteApiResourceById;
        var deleteApiResourceresponse = await FrontChannelClient.PostAsync(
            deleteApiResourceurl,
            new StringContent(JsonConvert.SerializeObject(invalidResourceIdInput), Encoding.UTF8, "application/json"));
        var deleteApiResource = await deleteApiResourceresponse.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<FrameworkResult>(deleteApiResource);
        result.Should().BeOfType<FrameworkResult>();
        result.Status.Should().Be(ResultStatus.Failed);
        result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.ApiResourceIdInvalid);
    }

    [Fact]
    public async Task DeleteApiScope_ById_NoRecordsForScopeId_ReturnError()
    {
        try
        {
            // Arrange
            var invalidApiScopeId = Guid.NewGuid();
            // Act
            var token = await GetAccessToken();
            FrontChannelClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token.access_token);

            var deleteApiScopeurl = BaseUrl + ApiRoutePathConstants.DeleteApiScopeById;
            var deleteApiScopesponse = await FrontChannelClient.PostAsync(
                deleteApiScopeurl,
                new StringContent(JsonConvert.SerializeObject(invalidApiScopeId), Encoding.UTF8, "application/json"));
            var deleteApiScope = await deleteApiScopesponse.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<FrameworkResult>(deleteApiScope);
            result.Should().BeOfType<FrameworkResult>();
            result.Status.Should().Be(ResultStatus.Failed);
            result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.ApiScopeIdInvalid);
        }

        catch (Exception ex)
        {
            throw ex;
        }
    }

    [Fact]
    public async Task DeleteApiScope_ByName_NoRecordsForScopeName_ReturnError()
    {
        try
        {
            // Arrange
            var invalidApiScopeName = string.Concat("CR7", "_", random.Next().ToString());
            // Act
            var token = await GetAccessToken();
            FrontChannelClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token.access_token);

            var deleteApiScopeurl = BaseUrl + ApiRoutePathConstants.DeleteApiScopeByName;
            var deleteApiScopesponse = await FrontChannelClient.PostAsync(
                deleteApiScopeurl,
                new StringContent(JsonConvert.SerializeObject(invalidApiScopeName), Encoding.UTF8, "application/json"));
            var deleteApiScope = await deleteApiScopesponse.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<FrameworkResult>(deleteApiScope);
            result.Should().BeOfType<FrameworkResult>();
            result.Status.Should().Be(ResultStatus.Failed);
            result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.ApiScopeNameInvalid);
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    [Fact]
    [Trait("Category", "DeleteErrorCase")]
    public async Task DeleteApiScopeClaim_ByScopeId_NoRecordsFound_Returns_InvalidScopeId_Error()
    {
        // Arrange
        var invalidApiScopeId = Guid.NewGuid();
        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);

        var deleteApiScopeurl = BaseUrl + ApiRoutePathConstants.DeleteApiScopeClaimById;
        var deleteApiScopesponse = await FrontChannelClient.PostAsync(
            deleteApiScopeurl,
            new StringContent(JsonConvert.SerializeObject(invalidApiScopeId), Encoding.UTF8, "application/json"));
        var deleteApiScope = await deleteApiScopesponse.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<FrameworkResult>(deleteApiScope);
        result.Should().BeOfType<FrameworkResult>();
        result.Status.Should().Be(ResultStatus.Failed);
        result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.ApiScopeClaimIdInvalid);
    }
}
