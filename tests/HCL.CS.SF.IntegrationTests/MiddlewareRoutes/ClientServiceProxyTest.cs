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
using TestApp.Helper.Endpoint;
using Xunit;
using HCL.CS.SF.Domain.Constants;
using HCL.CS.SF.Domain.ErrorCodes;

namespace IntegrationTests.MiddlewareRoutes;

public class ClientServiceProxyTest : HCLCSSFFakeSetup
{
    private readonly Random random = new();
    public string clientId;
    public string clientName = "IntegrationTestClient";

    [Fact]
    public async Task RegisterClient_Success()
    {
        var clientsModel = ClientHelper.CreateClientsModel();
        clientsModel.ClientName = clientName + random.Next();
        clientsModel.ClientId = Guid.NewGuid().ToString();
        clientsModel.ClientSecret = Guid.NewGuid().ToString();

        var token = await GetAccessToken();
        // Call lock user middleware
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);
        await LoginAsync(User);
        var clientsModelurl = BaseUrl + ApiRoutePathConstants.RegisterClient;
        var clientsModelresponse = await FrontChannelClient.PostAsync(
            clientsModelurl,
            new StringContent(JsonConvert.SerializeObject(clientsModel), Encoding.UTF8, "application/json"));
        var clientResponse = await clientsModelresponse.Content.ReadAsStringAsync();
        var clientResponseResult = JsonConvert.DeserializeObject<ClientsModel>(clientResponse);
        clientId = clientResponseResult.ClientId;
    }

    [Fact]
    public async Task UpdateClient_Success()
    {
        await RegisterClient_Success();
        // Get
        var clientsModelurl = BaseUrl + ApiRoutePathConstants.GetClient;
        var clientsModelresponse = await FrontChannelClient.PostAsync(
            clientsModelurl,
            new StringContent(JsonConvert.SerializeObject(clientId), Encoding.UTF8, "application/json"));
        var clientResponse = await clientsModelresponse.Content.ReadAsStringAsync();
        var clientResponseResult = JsonConvert.DeserializeObject<ClientsModel>(clientResponse);

        // Update
        clientResponseResult.LogoUri = "https://localhost:5002/logo.pngproxytest";
        var updateClientsModelurl = BaseUrl + ApiRoutePathConstants.UpdateClient;
        var updateclientsModelResponse = await FrontChannelClient.PostAsync(
            updateClientsModelurl,
            new StringContent(JsonConvert.SerializeObject(clientResponseResult), Encoding.UTF8, "application/json"));
        var updateClientResponse = await updateclientsModelResponse.Content.ReadAsStringAsync();
        var updateclientResponseResult = JsonConvert.DeserializeObject<ClientsModel>(updateClientResponse);
        updateclientResponseResult.LogoUri.Contains(clientResponseResult.LogoUri);
    }

    [Fact]
    public async Task DeleteClient_Success()
    {
        await RegisterClient_Success();

        // Get
        var clientsModelurl = BaseUrl + ApiRoutePathConstants.GetClient;
        var clientsModelresponse = await FrontChannelClient.PostAsync(
            clientsModelurl,
            new StringContent(JsonConvert.SerializeObject(clientId), Encoding.UTF8, "application/json"));
        var clientResponse = await clientsModelresponse.Content.ReadAsStringAsync();
        var clientResponseResult = JsonConvert.DeserializeObject<ClientsModel>(clientResponse);

        // Delete
        var deleteClientsModelurl = BaseUrl + ApiRoutePathConstants.DeleteClient;
        var deleteclientsModelResponse = await FrontChannelClient.PostAsync(
            deleteClientsModelurl,
            new StringContent(JsonConvert.SerializeObject(clientResponseResult.ClientId), Encoding.UTF8,
                "application/json"));
        var updateClientResponse = await deleteclientsModelResponse.Content.ReadAsStringAsync();
        var deleteClientResponseResult = JsonConvert.DeserializeObject<FrameworkResult>(updateClientResponse);
        deleteClientResponseResult.Should().BeOfType<FrameworkResult>();
        deleteClientResponseResult.Status.Should().Be(ResultStatus.Success);
    }

    [Fact]
    public async Task GetAllCleints_Success()
    {
        await RegisterClient_Success();
        // Get
        var clientsModelurl = BaseUrl + ApiRoutePathConstants.GetAllClient;
        var clientsModelresponse = await FrontChannelClient.PostAsync(
            clientsModelurl,
            new StringContent(JsonConvert.SerializeObject(string.Empty), Encoding.UTF8, "application/json"));
        var clientResponse = await clientsModelresponse.Content.ReadAsStringAsync();
        var clientResponseResult = JsonConvert.DeserializeObject<ClientsModel>(clientResponse);
        clientResponseResult.Should().NotBeNull();
    }

    [Fact]
    public async Task GenerarateClientSecret_Success()
    {
        await RegisterClient_Success();
        // Get
        var clientsModelurl = BaseUrl + ApiRoutePathConstants.GenerateClientSecret;
        var clientsModelresponse = await FrontChannelClient.PostAsync(
            clientsModelurl,
            new StringContent(JsonConvert.SerializeObject(clientId), Encoding.UTF8, "application/json"));
        var clientResponse = await clientsModelresponse.Content.ReadAsStringAsync();
        var clientResponseResult = JsonConvert.DeserializeObject<ClientsModel>(clientResponse);
        clientResponseResult.Should().NotBeNull();
    }

    [Fact]
    public async Task RegisterClient_InvalidCleintName_ReturnFail()
    {
        var clientsModel = ClientHelper.CreateClientsModel();
        clientsModel.ClientName = null;
        clientsModel.ClientId = Guid.NewGuid().ToString();
        clientsModel.ClientSecret = Guid.NewGuid().ToString();

        var token = await GetAccessToken();
        // Call lock user middleware
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);
        var clientsModelurl = BaseUrl + ApiRoutePathConstants.RegisterClient;
        var clientsModelresponse = await FrontChannelClient.PostAsync(
            clientsModelurl,
            new StringContent(JsonConvert.SerializeObject(clientsModel), Encoding.UTF8, "application/json"));
        var clientResponse = await clientsModelresponse.Content.ReadAsStringAsync();
        var clientResponseResult = JsonConvert.DeserializeObject(clientResponse);
        clientResponseResult.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.ClientNameIsRequired));
    }

    [Fact]
    public async Task RegisterClient_CleintNameLenthExceed_ReturnFail()
    {
        var clientsModel = ClientHelper.CreateClientsModel();
        clientsModel.ClientName =
            "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        clientsModel.ClientId = Guid.NewGuid().ToString();
        clientsModel.ClientSecret = Guid.NewGuid().ToString();

        var token = await GetAccessToken();
        // Call lock user middleware
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);
        var clientsModelurl = BaseUrl + ApiRoutePathConstants.RegisterClient;
        var clientsModelresponse = await FrontChannelClient.PostAsync(
            clientsModelurl,
            new StringContent(JsonConvert.SerializeObject(clientsModel), Encoding.UTF8, "application/json"));
        var clientResponse = await clientsModelresponse.Content.ReadAsStringAsync();
        var clientResponseResult = JsonConvert.DeserializeObject(clientResponse);
        clientResponseResult.Should().Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.ClientNameTooLong));
    }

    // CheckValidLogoURI

    [Fact]
    public async Task RegisterClient_CheckNullLogoURI_ReturnFail()
    {
        var clientsModel = ClientHelper.CreateClientsModel();
        clientsModel.ClientName = "testCleint";
        clientsModel.ClientId = Guid.NewGuid().ToString();
        clientsModel.ClientSecret = Guid.NewGuid().ToString();
        clientsModel.LogoUri = null;

        var token = await GetAccessToken();
        // Call lock user middleware
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);
        var clientsModelurl = BaseUrl + ApiRoutePathConstants.RegisterClient;
        var clientsModelresponse = await FrontChannelClient.PostAsync(
            clientsModelurl,
            new StringContent(JsonConvert.SerializeObject(clientsModel), Encoding.UTF8, "application/json"));
        var clientResponse = await clientsModelresponse.Content.ReadAsStringAsync();
        var clientResponseResult = JsonConvert.DeserializeObject(clientResponse);
        clientResponseResult.Should().Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.InvalidLogoUri));
    }

    [Fact]
    public async Task RegisterClient_CheckInvalidLogoURI_ReturnFail()
    {
        var clientsModel = ClientHelper.CreateClientsModel();
        clientsModel.ClientName = "testCleint";
        clientsModel.ClientId = Guid.NewGuid().ToString();
        clientsModel.ClientSecret = Guid.NewGuid().ToString();
        clientsModel.LogoUri = "HellooUri";

        var token = await GetAccessToken();
        // Call lock user middleware
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);
        var clientsModelurl = BaseUrl + ApiRoutePathConstants.RegisterClient;
        var clientsModelresponse = await FrontChannelClient.PostAsync(
            clientsModelurl,
            new StringContent(JsonConvert.SerializeObject(clientsModel), Encoding.UTF8, "application/json"));
        var clientResponse = await clientsModelresponse.Content.ReadAsStringAsync();
        var clientResponseResult = JsonConvert.DeserializeObject(clientResponse);
        clientResponseResult.Should().Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.InvalidLogoUri));
    }

    [Fact]
    public async Task RegisterClient_NullValidPolicyURI_ReturnFail()
    {
        var clientsModel = ClientHelper.CreateClientsModel();
        clientsModel.ClientName = "testCleint";
        clientsModel.ClientId = Guid.NewGuid().ToString();
        clientsModel.ClientSecret = Guid.NewGuid().ToString();
        clientsModel.PolicyUri = null;

        var token = await GetAccessToken();
        // Call lock user middleware
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);
        var clientsModelurl = BaseUrl + ApiRoutePathConstants.RegisterClient;
        var clientsModelresponse = await FrontChannelClient.PostAsync(
            clientsModelurl,
            new StringContent(JsonConvert.SerializeObject(clientsModel), Encoding.UTF8, "application/json"));
        var clientResponse = await clientsModelresponse.Content.ReadAsStringAsync();
        var clientResponseResult = JsonConvert.DeserializeObject(clientResponse);
        clientResponseResult.Should().Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.InvalidPolicyUri));
    }

    [Fact]
    public async Task RegisterClient_NullAllowedScopes_ReturnFail()
    {
        var clientsModel = ClientHelper.CreateClientsModel();
        clientsModel.ClientName = "testCleint";
        clientsModel.ClientId = Guid.NewGuid().ToString();
        clientsModel.ClientSecret = Guid.NewGuid().ToString();
        clientsModel.AllowedScopes = null;

        var token = await GetAccessToken();
        // Call lock user middleware
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);
        var clientsModelurl = BaseUrl + ApiRoutePathConstants.RegisterClient;
        var clientsModelresponse = await FrontChannelClient.PostAsync(
            clientsModelurl,
            new StringContent(JsonConvert.SerializeObject(clientsModel), Encoding.UTF8, "application/json"));
        var clientResponse = await clientsModelresponse.Content.ReadAsStringAsync();
        var clientResponseResult = JsonConvert.DeserializeObject(clientResponse);
        clientResponseResult.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.AllowedScopesIsRequired));
    }

    [Fact]
    public async Task RegisterClient_InvalidResponseTypes_ReturnFail()
    {
        var clientsModel = ClientHelper.CreateClientsModel();
        clientsModel.ClientName = "testCleint";
        clientsModel.ClientId = Guid.NewGuid().ToString();
        clientsModel.ClientSecret = Guid.NewGuid().ToString();
        clientsModel.SupportedResponseTypes = new List<string> { "HI Hello code token id_token" };

        var token = await GetAccessToken();
        // Call lock user middleware
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);
        var clientsModelurl = BaseUrl + ApiRoutePathConstants.RegisterClient;
        var clientsModelresponse = await FrontChannelClient.PostAsync(
            clientsModelurl,
            new StringContent(JsonConvert.SerializeObject(clientsModel), Encoding.UTF8, "application/json"));
        var clientResponse = await clientsModelresponse.Content.ReadAsStringAsync();
        var clientResponseResult = JsonConvert.DeserializeObject<ClientsModel>(clientResponse);
        clientResponseResult.Should().NotBeNull();
    }

    [Fact]
    public async Task RegisterClient_InvalidValidAccessTokenExpirationRange_ReturnFail()
    {
        var clientsModel = ClientHelper.CreateClientsModel();
        clientsModel.ClientName = "testCleint";
        clientsModel.ClientId = Guid.NewGuid().ToString();
        clientsModel.ClientSecret = Guid.NewGuid().ToString();
        clientsModel.AccessTokenExpiration = 200;

        var token = await GetAccessToken();
        // Call lock user middleware
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);
        var clientsModelurl = BaseUrl + ApiRoutePathConstants.RegisterClient;
        var clientsModelresponse = await FrontChannelClient.PostAsync(
            clientsModelurl,
            new StringContent(JsonConvert.SerializeObject(clientsModel), Encoding.UTF8, "application/json"));
        var clientResponse = await clientsModelresponse.Content.ReadAsStringAsync();
        var clientResponseResult = JsonConvert.DeserializeObject(clientResponse);
        clientResponseResult.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.InvalidAccessTokenExpireRange));
    }

    [Fact]
    public async Task RegisterClient_InvalidValidRefershTokenExpirationRange_ReturnFail()
    {
        var clientsModel = ClientHelper.CreateClientsModel();
        clientsModel.ClientName = "testCleint";
        clientsModel.ClientId = Guid.NewGuid().ToString();
        clientsModel.ClientSecret = Guid.NewGuid().ToString();
        clientsModel.RefreshTokenExpiration = 200;

        var token = await GetAccessToken();
        // Call lock user middleware
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);
        var clientsModelurl = BaseUrl + ApiRoutePathConstants.RegisterClient;
        var clientsModelresponse = await FrontChannelClient.PostAsync(
            clientsModelurl,
            new StringContent(JsonConvert.SerializeObject(clientsModel), Encoding.UTF8, "application/json"));
        var clientResponse = await clientsModelresponse.Content.ReadAsStringAsync();
        var clientResponseResult = JsonConvert.DeserializeObject(clientResponse);
        clientResponseResult.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.InvalidRefreshTokenExpireRange));
    }

    [Fact]
    public async Task RegisterClient_InvalidValidAuthorizatoonTokenExpirationRange_ReturnFail()
    {
        var clientsModel = ClientHelper.CreateClientsModel();
        clientsModel.ClientName = "testCleint";
        clientsModel.ClientId = Guid.NewGuid().ToString();
        clientsModel.ClientSecret = Guid.NewGuid().ToString();
        clientsModel.AuthorizationCodeExpiration = 200;

        var token = await GetAccessToken();
        // Call lock user middleware
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);
        var clientsModelurl = BaseUrl + ApiRoutePathConstants.RegisterClient;
        var clientsModelresponse = await FrontChannelClient.PostAsync(
            clientsModelurl,
            new StringContent(JsonConvert.SerializeObject(clientsModel), Encoding.UTF8, "application/json"));
        var clientResponse = await clientsModelresponse.Content.ReadAsStringAsync();
        var clientResponseResult = JsonConvert.DeserializeObject(clientResponse);
        clientResponseResult.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.InvalidAuthorizationCodeExpireRange));
    }

    [Fact]
    public async Task UpdateClient_InvalidClientName()
    {
        await RegisterClient_Success();
        // Get
        var clientsModelurl = BaseUrl + ApiRoutePathConstants.GetClient;
        var clientsModelresponse = await FrontChannelClient.PostAsync(
            clientsModelurl,
            new StringContent(JsonConvert.SerializeObject(clientId), Encoding.UTF8, "application/json"));
        var clientResponse = await clientsModelresponse.Content.ReadAsStringAsync();
        var clientResponseResult = JsonConvert.DeserializeObject<ClientsModel>(clientResponse);

        clientResponseResult.ClientId = null;

        // Update
        clientResponseResult.LogoUri = "https://localhost:5002/logo.pngproxytest";
        var updateClientsModelurl = BaseUrl + ApiRoutePathConstants.UpdateClient;
        var updateclientsModelResponse = await FrontChannelClient.PostAsync(
            updateClientsModelurl,
            new StringContent(JsonConvert.SerializeObject(clientResponseResult), Encoding.UTF8, "application/json"));
        var updateClientResponse = await updateclientsModelResponse.Content.ReadAsStringAsync();
        var updateclientResponseResult = JsonConvert.DeserializeObject(updateClientResponse);
        updateclientResponseResult.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.ClientDoesNotExist));
    }


    [Fact]
    public async Task UpdateClient_ModifyByLengthExceeds()
    {
        await RegisterClient_Success();
        // Get
        var clientsModelurl = BaseUrl + ApiRoutePathConstants.GetClient;
        var clientsModelresponse = await FrontChannelClient.PostAsync(
            clientsModelurl,
            new StringContent(JsonConvert.SerializeObject(clientId), Encoding.UTF8, "application/json"));
        var clientResponse = await clientsModelresponse.Content.ReadAsStringAsync();
        var clientResponseResult = JsonConvert.DeserializeObject<ClientsModel>(clientResponse);

        clientResponseResult.ModifiedBy =
            "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        ;

        // Update
        clientResponseResult.LogoUri = "https://localhost:5002/logo.pngproxytest";
        var updateClientsModelurl = BaseUrl + ApiRoutePathConstants.UpdateClient;
        var updateclientsModelResponse = await FrontChannelClient.PostAsync(
            updateClientsModelurl,
            new StringContent(JsonConvert.SerializeObject(clientResponseResult), Encoding.UTF8, "application/json"));
        var updateClientResponse = await updateclientsModelResponse.Content.ReadAsStringAsync();
        var updateclientResponseResult = JsonConvert.DeserializeObject(updateClientResponse);
        updateclientResponseResult.Should()
            .Be(ResourceStringHandler.GetResourceString(EndpointErrorCodes.ModifiedByTooLong));
    }

    [Fact]
    public async Task DeleteClient_InvalidClientId()
    {
        await RegisterClient_Success();
        // Get
        var clientsModelurl = BaseUrl + ApiRoutePathConstants.GetClient;
        var clientsModelresponse = await FrontChannelClient.PostAsync(
            clientsModelurl,
            new StringContent(JsonConvert.SerializeObject(clientId), Encoding.UTF8, "application/json"));
        var clientResponse = await clientsModelresponse.Content.ReadAsStringAsync();
        var clientResponseResult = JsonConvert.DeserializeObject<ClientsModel>(clientResponse);
        clientResponseResult.ClientId = Guid.NewGuid().ToString();

        // Delete
        var deleteClientsModelurl = BaseUrl + ApiRoutePathConstants.DeleteClient;
        var deleteclientsModelResponse = await FrontChannelClient.PostAsync(
            deleteClientsModelurl,
            new StringContent(JsonConvert.SerializeObject(clientResponseResult.ClientId), Encoding.UTF8,
                "application/json"));
        var deleteClientResponse = await deleteclientsModelResponse.Content.ReadAsStringAsync();
        var deleteClientResponseResult = JsonConvert.DeserializeObject<FrameworkResult>(deleteClientResponse);
        deleteClientResponseResult.Should().BeOfType<FrameworkResult>();
        deleteClientResponseResult.Status.Should().Be(ResultStatus.Failed);
        deleteClientResponseResult.Errors.FirstOrDefault().Code.Should().Be(EndpointErrorCodes.ClientDoesNotExist);
    }
}
