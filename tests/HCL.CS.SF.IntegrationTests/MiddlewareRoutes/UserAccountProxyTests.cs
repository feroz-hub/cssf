/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Net;
using System.Net.Http.Headers;
using System.Text;
using FluentAssertions;
using IntegrationTests.ApiDomainModel;
using IntegrationTests.Endpoint.Setup;
using Newtonsoft.Json;
using TestApp.Helper.Api;
using Xunit;
using HCL.CS.SF.Domain.Constants;
using HCL.CS.SF.Domain.Constants.Endpoint;
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;

namespace IntegrationTests.MiddlewareRoutes;

public class UserAccountProxyTests : HCLCSSFFakeSetup
{
    private const string UserNameForUpdateAndGet = "IntegrationTestUser";

    private readonly string hCLCSS256ClientName = "HCL.CS.SF S256 Client";

    private readonly Random random;

    private readonly string userName = "BobUser";
    private ClientsModel clientModel;

    private string positiveCaseClientName = "HCL.CS.SF Plain PKCE Client";

    public string roleName = "IntegrationTest";
    public string userRoleRoleName;
    public string userRoleUserId;

    public UserAccountProxyTests()
    {
        random = new Random();
    }

    [Fact]
    [Trait("Category", "SuccessCase")]
    public async Task<Users_Info> RegisterUserAsync_ReturnSuccess()
    {
        var userInfo = new Users_Info();
        // Getting create User Model.
        var userModelInput = CreateUserRequestModel();
        var randomString = random.Next().ToString();
        userModelInput.UserName = string.Concat(UserNameForUpdateAndGet, "_", randomString);
        userModelInput.FirstName = userModelInput.UserName.ToUpper();
        userModelInput.LastName = userModelInput.UserName.ToLower();
        userModelInput.Email = string.Concat(userModelInput.UserName, "@", "HCL.CS.SF.com");
        userModelInput.CreatedBy = "Rosh";

        // Getting Security Questions
        var securityQuestionResult_url = BaseUrl + ApiRoutePathConstants.GetAllSecurityQuestions;
        var securityQuestionResul_response = await FrontChannelClient
            .PostAsync(
                securityQuestionResult_url,
                null);
        var securityQuestionsDetails = await securityQuestionResul_response.Content.ReadAsStringAsync();

        var securityQuestionsResult =
            JsonConvert.DeserializeObject<List<SecurityQuestionModel>>(securityQuestionsDetails).ToList();

        // Adding questions to user
        if (securityQuestionsResult != null && securityQuestionsResult.Count > 0)
        {
            userModelInput.UserSecurityQuestion[0].SecurityQuestionId = securityQuestionsResult[0].Id;
            userModelInput.UserSecurityQuestion[0].Answer = "Test1";
            userModelInput.UserSecurityQuestion[1].SecurityQuestionId = securityQuestionsResult[1].Id;
            userModelInput.UserSecurityQuestion[1].Answer = "Test2";
            userModelInput.UserSecurityQuestion[2].SecurityQuestionId = securityQuestionsResult[2].Id;
            userModelInput.UserSecurityQuestion[2].Answer = "Test3";
        }

        // Adding User
        var registerUser_Url = BaseUrl + ApiRoutePathConstants.RegisterUser;
        var registerUser_response = await FrontChannelClient
            .PostAsync(
                registerUser_Url,
                new StringContent(JsonConvert.SerializeObject(userModelInput), Encoding.UTF8, "application/json"));
        var userDetails = await registerUser_response.Content.ReadAsStringAsync();
        var registerUser_result = JsonConvert.DeserializeObject<FrameworkResult>(userDetails);
        registerUser_result.Should().BeOfType<FrameworkResult>();
        registerUser_result.Status.Should().Be(ResultStatus.Success);

        userInfo.UserName = userModelInput.UserName;
        userInfo.Password = userModelInput.Password;
        userInfo.userSecurityQuestionModels = userModelInput.UserSecurityQuestion;
        return userInfo;
    }

    [Fact]
    [Trait("Category", "ErrorCase")]
    public async Task RegisterUserAsync_InvalidUserName_ReturnError()
    {
        // Getting create User Model.
        var userModelInput = CreateUserRequestModel();
        var randomString = random.Next().ToString();
        userModelInput.UserName = null;

        // Getting Security Questions
        var securityQuestionResult_url = BaseUrl + ApiRoutePathConstants.GetAllSecurityQuestions;
        var securityQuestionResul_response = await FrontChannelClient
            .PostAsync(
                securityQuestionResult_url,
                null);
        var securityQuestionsDetails = await securityQuestionResul_response.Content.ReadAsStringAsync();

        var securityQuestionsResult =
            JsonConvert.DeserializeObject<List<SecurityQuestionModel>>(securityQuestionsDetails).ToList();

        // Adding questions to user
        if (securityQuestionsResult != null && securityQuestionsResult.Count > 0)
        {
            userModelInput.UserSecurityQuestion[0].SecurityQuestionId = securityQuestionsResult[0].Id;
            userModelInput.UserSecurityQuestion[0].Answer = "Test1";
            userModelInput.UserSecurityQuestion[1].SecurityQuestionId = securityQuestionsResult[1].Id;
            userModelInput.UserSecurityQuestion[1].Answer = "Test2";
            userModelInput.UserSecurityQuestion[2].SecurityQuestionId = securityQuestionsResult[2].Id;
            userModelInput.UserSecurityQuestion[2].Answer = "Test3";
        }

        // Adding User
        var registerUser_Url = BaseUrl + ApiRoutePathConstants.RegisterUser;
        var registerUser_response = await FrontChannelClient
            .PostAsync(
                registerUser_Url,
                new StringContent(JsonConvert.SerializeObject(userModelInput), Encoding.UTF8, "application/json"));
        var userDetails = await registerUser_response.Content.ReadAsStringAsync();
        var registerUser_result = JsonConvert.DeserializeObject<FrameworkResult>(userDetails);
        registerUser_result.Should().BeOfType<FrameworkResult>();
        registerUser_result.Status.Should().Be(ResultStatus.Failed);
        registerUser_result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.UsernameRequired);
    }

    [Fact]
    [Trait("Category", "ErrorCase")]
    public async Task RegisterUserAsync_InvalidEmail_ReturnError()
    {
        // Getting create User Model.
        var userModelInput = CreateUserRequestModel();
        var randomString = random.Next().ToString();
        userModelInput.UserName = random.Next().ToString();
        userModelInput.Email = "Hello.Suresh";

        // Getting Security Questions
        var securityQuestionResult_url = BaseUrl + ApiRoutePathConstants.GetAllSecurityQuestions;
        var securityQuestionResul_response = await FrontChannelClient
            .PostAsync(
                securityQuestionResult_url,
                null);
        var securityQuestionsDetails = await securityQuestionResul_response.Content.ReadAsStringAsync();

        var securityQuestionsResult =
            JsonConvert.DeserializeObject<List<SecurityQuestionModel>>(securityQuestionsDetails).ToList();

        // Adding questions to user
        if (securityQuestionsResult != null && securityQuestionsResult.Count > 0)
        {
            userModelInput.UserSecurityQuestion[0].SecurityQuestionId = securityQuestionsResult[0].Id;
            userModelInput.UserSecurityQuestion[0].Answer = "Test1";
            userModelInput.UserSecurityQuestion[1].SecurityQuestionId = securityQuestionsResult[1].Id;
            userModelInput.UserSecurityQuestion[1].Answer = "Test2";
            userModelInput.UserSecurityQuestion[2].SecurityQuestionId = securityQuestionsResult[2].Id;
            userModelInput.UserSecurityQuestion[2].Answer = "Test3";
        }

        // Adding User
        var registerUser_Url = BaseUrl + ApiRoutePathConstants.RegisterUser;
        var registerUser_response = await FrontChannelClient
            .PostAsync(
                registerUser_Url,
                new StringContent(JsonConvert.SerializeObject(userModelInput), Encoding.UTF8, "application/json"));
        var userDetails = await registerUser_response.Content.ReadAsStringAsync();
        var registerUser_result = JsonConvert.DeserializeObject<FrameworkResult>(userDetails);
        registerUser_result.Should().BeOfType<FrameworkResult>();
        registerUser_result.Status.Should().Be(ResultStatus.Failed);
        registerUser_result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.InvalidEmailFormat);
    }

    [Fact]
    [Trait("Category", "ErrorCase")]
    public async Task RegisterUserAsync_InvalidPhone_ReturnError()
    {
        // Getting create User Model.
        var userModelInput = CreateUserRequestModel();
        var randomString = random.Next().ToString();
        userModelInput.UserName = random.Next().ToString();
        userModelInput.PhoneNumber = "+9186OPds9119";
        userModelInput.Email = string.Concat(userModelInput.UserName, "@", "HCL.CS.SF.com");

        // Getting Security Questions
        var securityQuestionResult_url = BaseUrl + ApiRoutePathConstants.GetAllSecurityQuestions;
        var securityQuestionResul_response = await FrontChannelClient
            .PostAsync(
                securityQuestionResult_url,
                null);
        var securityQuestionsDetails = await securityQuestionResul_response.Content.ReadAsStringAsync();

        var securityQuestionsResult =
            JsonConvert.DeserializeObject<List<SecurityQuestionModel>>(securityQuestionsDetails).ToList();

        // Adding questions to user
        if (securityQuestionsResult != null && securityQuestionsResult.Count > 0)
        {
            userModelInput.UserSecurityQuestion[0].SecurityQuestionId = securityQuestionsResult[0].Id;
            userModelInput.UserSecurityQuestion[0].Answer = "Test1";
            userModelInput.UserSecurityQuestion[1].SecurityQuestionId = securityQuestionsResult[1].Id;
            userModelInput.UserSecurityQuestion[1].Answer = "Test2";
            userModelInput.UserSecurityQuestion[2].SecurityQuestionId = securityQuestionsResult[2].Id;
            userModelInput.UserSecurityQuestion[2].Answer = "Test3";
        }

        // Adding User
        var registerUser_Url = BaseUrl + ApiRoutePathConstants.RegisterUser;
        var registerUser_response = await FrontChannelClient
            .PostAsync(
                registerUser_Url,
                new StringContent(JsonConvert.SerializeObject(userModelInput), Encoding.UTF8, "application/json"));
        var userDetails = await registerUser_response.Content.ReadAsStringAsync();
        var registerUser_result = JsonConvert.DeserializeObject<FrameworkResult>(userDetails);
        registerUser_result.Should().BeOfType<FrameworkResult>();
        registerUser_result.Status.Should().Be(ResultStatus.Failed);
        registerUser_result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.InvalidPhonenumber);
    }

    [Fact]
    [Trait("Category", "ErrorCase")]
    public async Task RegisterUserAsync_UserNameLenthExceeded_ReturnError()
    {
        // Getting create User Model.
        var userModelInput = CreateUserRequestModel();
        var randomString = random.Next().ToString();
        userModelInput.UserName =
            "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        userModelInput.Email = string.Concat(userModelInput.UserName, "@", "HCL.CS.SF.com");

        // Getting Security Questions
        var securityQuestionResult_url = BaseUrl + ApiRoutePathConstants.GetAllSecurityQuestions;
        var securityQuestionResul_response = await FrontChannelClient
            .PostAsync(
                securityQuestionResult_url,
                null);
        var securityQuestionsDetails = await securityQuestionResul_response.Content.ReadAsStringAsync();

        var securityQuestionsResult =
            JsonConvert.DeserializeObject<List<SecurityQuestionModel>>(securityQuestionsDetails).ToList();

        // Adding questions to user
        if (securityQuestionsResult != null && securityQuestionsResult.Count > 0)
        {
            userModelInput.UserSecurityQuestion[0].SecurityQuestionId = securityQuestionsResult[0].Id;
            userModelInput.UserSecurityQuestion[0].Answer = "Test1";
            userModelInput.UserSecurityQuestion[1].SecurityQuestionId = securityQuestionsResult[1].Id;
            userModelInput.UserSecurityQuestion[1].Answer = "Test2";
            userModelInput.UserSecurityQuestion[2].SecurityQuestionId = securityQuestionsResult[2].Id;
            userModelInput.UserSecurityQuestion[2].Answer = "Test3";
        }

        // Adding User
        var registerUser_Url = BaseUrl + ApiRoutePathConstants.RegisterUser;
        var registerUser_response = await FrontChannelClient
            .PostAsync(
                registerUser_Url,
                new StringContent(JsonConvert.SerializeObject(userModelInput), Encoding.UTF8, "application/json"));
        var userDetails = await registerUser_response.Content.ReadAsStringAsync();
        var registerUser_result = JsonConvert.DeserializeObject<FrameworkResult>(userDetails);
        registerUser_result.Should().BeOfType<FrameworkResult>();
        registerUser_result.Status.Should().Be(ResultStatus.Failed);
        registerUser_result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.InvalidLengthForUsername);
    }

    [Fact]
    [Trait("Category", "ErrorCase")]
    public async Task RegisterUserAsync_InvalidFirstName_ReturnError()
    {
        // Getting create User Model.
        var userModelInput = CreateUserRequestModel();
        var randomString = random.Next().ToString();
        userModelInput.UserName = random.Next().ToString();
        userModelInput.FirstName = null;
        userModelInput.Email = string.Concat(userModelInput.UserName, "@", "HCL.CS.SF.com");

        // Getting Security Questions
        var securityQuestionResult_url = BaseUrl + ApiRoutePathConstants.GetAllSecurityQuestions;
        var securityQuestionResul_response = await FrontChannelClient
            .PostAsync(
                securityQuestionResult_url,
                null);
        var securityQuestionsDetails = await securityQuestionResul_response.Content.ReadAsStringAsync();

        var securityQuestionsResult =
            JsonConvert.DeserializeObject<List<SecurityQuestionModel>>(securityQuestionsDetails).ToList();

        // Adding questions to user
        if (securityQuestionsResult != null && securityQuestionsResult.Count > 0)
        {
            userModelInput.UserSecurityQuestion[0].SecurityQuestionId = securityQuestionsResult[0].Id;
            userModelInput.UserSecurityQuestion[0].Answer = "Test1";
            userModelInput.UserSecurityQuestion[1].SecurityQuestionId = securityQuestionsResult[1].Id;
            userModelInput.UserSecurityQuestion[1].Answer = "Test2";
            userModelInput.UserSecurityQuestion[2].SecurityQuestionId = securityQuestionsResult[2].Id;
            userModelInput.UserSecurityQuestion[2].Answer = "Test3";
        }

        // Adding User
        var registerUser_Url = BaseUrl + ApiRoutePathConstants.RegisterUser;
        var registerUser_response = await FrontChannelClient
            .PostAsync(
                registerUser_Url,
                new StringContent(JsonConvert.SerializeObject(userModelInput), Encoding.UTF8, "application/json"));
        var userDetails = await registerUser_response.Content.ReadAsStringAsync();
        var registerUser_result = JsonConvert.DeserializeObject<FrameworkResult>(userDetails);
        registerUser_result.Should().BeOfType<FrameworkResult>();
        registerUser_result.Status.Should().Be(ResultStatus.Failed);
        registerUser_result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.FirstnameRequired);
    }

    [Fact]
    [Trait("Category", "ErrorCase")]
    public async Task RegisterUserAsync_FirstnameLengthExceeded_ReturnError()
    {
        // Getting create User Model.
        var userModelInput = CreateUserRequestModel();
        var randomString = random.Next().ToString();
        userModelInput.UserName = random.Next().ToString();
        userModelInput.FirstName =
            "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        userModelInput.Email = string.Concat(userModelInput.UserName, "@", "HCL.CS.SF.com");

        // Getting Security Questions
        var securityQuestionResult_url = BaseUrl + ApiRoutePathConstants.GetAllSecurityQuestions;
        var securityQuestionResul_response = await FrontChannelClient
            .PostAsync(
                securityQuestionResult_url,
                null);
        var securityQuestionsDetails = await securityQuestionResul_response.Content.ReadAsStringAsync();

        var securityQuestionsResult =
            JsonConvert.DeserializeObject<List<SecurityQuestionModel>>(securityQuestionsDetails).ToList();

        // Adding questions to user
        if (securityQuestionsResult != null && securityQuestionsResult.Count > 0)
        {
            userModelInput.UserSecurityQuestion[0].SecurityQuestionId = securityQuestionsResult[0].Id;
            userModelInput.UserSecurityQuestion[0].Answer = "Test1";
            userModelInput.UserSecurityQuestion[1].SecurityQuestionId = securityQuestionsResult[1].Id;
            userModelInput.UserSecurityQuestion[1].Answer = "Test2";
            userModelInput.UserSecurityQuestion[2].SecurityQuestionId = securityQuestionsResult[2].Id;
            userModelInput.UserSecurityQuestion[2].Answer = "Test3";
        }

        // Adding User
        var registerUser_Url = BaseUrl + ApiRoutePathConstants.RegisterUser;
        var registerUser_response = await FrontChannelClient
            .PostAsync(
                registerUser_Url,
                new StringContent(JsonConvert.SerializeObject(userModelInput), Encoding.UTF8, "application/json"));
        var userDetails = await registerUser_response.Content.ReadAsStringAsync();
        var registerUser_result = JsonConvert.DeserializeObject<FrameworkResult>(userDetails);
        registerUser_result.Should().BeOfType<FrameworkResult>();
        registerUser_result.Status.Should().Be(ResultStatus.Failed);
        registerUser_result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.InvalidLengthForFirstName);
    }

    [Fact]
    [Trait("Category", "ErrorCase")]
    public async Task RegisterUserAsync_CreatedByLengthExceeded_ReturnError()
    {
        // Getting create User Model.
        var userModelInput = CreateUserRequestModel();
        var randomString = random.Next().ToString();
        userModelInput.UserName = random.Next().ToString();
        userModelInput.CreatedBy =
            "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        userModelInput.Email = string.Concat(userModelInput.UserName, "@", "HCL.CS.SF.com");

        // Getting Security Questions
        var securityQuestionResult_url = BaseUrl + ApiRoutePathConstants.GetAllSecurityQuestions;
        var securityQuestionResul_response = await FrontChannelClient
            .PostAsync(
                securityQuestionResult_url,
                null);
        var securityQuestionsDetails = await securityQuestionResul_response.Content.ReadAsStringAsync();

        var securityQuestionsResult =
            JsonConvert.DeserializeObject<List<SecurityQuestionModel>>(securityQuestionsDetails).ToList();

        // Adding questions to user
        if (securityQuestionsResult != null && securityQuestionsResult.Count > 0)
        {
            userModelInput.UserSecurityQuestion[0].SecurityQuestionId = securityQuestionsResult[0].Id;
            userModelInput.UserSecurityQuestion[0].Answer = "Test1";
            userModelInput.UserSecurityQuestion[1].SecurityQuestionId = securityQuestionsResult[1].Id;
            userModelInput.UserSecurityQuestion[1].Answer = "Test2";
            userModelInput.UserSecurityQuestion[2].SecurityQuestionId = securityQuestionsResult[2].Id;
            userModelInput.UserSecurityQuestion[2].Answer = "Test3";
        }

        // Adding User
        var registerUser_Url = BaseUrl + ApiRoutePathConstants.RegisterUser;
        var registerUser_response = await FrontChannelClient
            .PostAsync(
                registerUser_Url,
                new StringContent(JsonConvert.SerializeObject(userModelInput), Encoding.UTF8, "application/json"));
        var userDetails = await registerUser_response.Content.ReadAsStringAsync();
        var registerUser_result = JsonConvert.DeserializeObject<FrameworkResult>(userDetails);
        registerUser_result.Should().BeOfType<FrameworkResult>();
        registerUser_result.Status.Should().Be(ResultStatus.Failed);
        registerUser_result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.CreatedByTooLong);
    }

    [Fact]
    [Trait("Category", "SuccessCase")]
    public async Task RegisterUserAsync_CreatedByNULL_Success()
    {
        // Getting create User Model.
        var userModelInput = CreateUserRequestModel();
        var randomString = random.Next().ToString();
        userModelInput.UserName = random.Next().ToString();
        userModelInput.CreatedBy = null;
        userModelInput.Email = string.Concat(userModelInput.UserName, "@", "HCL.CS.SF.com");

        // Getting Security Questions
        var securityQuestionResult_url = BaseUrl + ApiRoutePathConstants.GetAllSecurityQuestions;
        var securityQuestionResul_response = await FrontChannelClient
            .PostAsync(
                securityQuestionResult_url,
                null);
        var securityQuestionsDetails = await securityQuestionResul_response.Content.ReadAsStringAsync();

        var securityQuestionsResult =
            JsonConvert.DeserializeObject<List<SecurityQuestionModel>>(securityQuestionsDetails).ToList();

        // Adding questions to user
        if (securityQuestionsResult != null && securityQuestionsResult.Count > 0)
        {
            userModelInput.UserSecurityQuestion[0].SecurityQuestionId = securityQuestionsResult[0].Id;
            userModelInput.UserSecurityQuestion[0].Answer = "Test1";
            userModelInput.UserSecurityQuestion[1].SecurityQuestionId = securityQuestionsResult[1].Id;
            userModelInput.UserSecurityQuestion[1].Answer = "Test2";
            userModelInput.UserSecurityQuestion[2].SecurityQuestionId = securityQuestionsResult[2].Id;
            userModelInput.UserSecurityQuestion[2].Answer = "Test3";
        }

        // Adding User
        var registerUser_Url = BaseUrl + ApiRoutePathConstants.RegisterUser;
        var registerUser_response = await FrontChannelClient
            .PostAsync(
                registerUser_Url,
                new StringContent(JsonConvert.SerializeObject(userModelInput), Encoding.UTF8, "application/json"));
        var userDetails = await registerUser_response.Content.ReadAsStringAsync();
        var registerUser_result = JsonConvert.DeserializeObject<FrameworkResult>(userDetails);
        registerUser_result.Should().BeOfType<FrameworkResult>();
        registerUser_result.Status.Should().Be(ResultStatus.Success);
    }

    [Fact]
    [Trait("Category", "ErrorCase")]
    public async Task RegisterUserAsync_ClaimTypeNULL_RerturnFail()
    {
        // Getting create User Model.
        var userModelInput = CreateUserRequestModel();
        var randomString = random.Next().ToString();
        userModelInput.UserName = random.Next().ToString();
        userModelInput.UserClaims[0].ClaimType = null;
        userModelInput.Email = string.Concat(userModelInput.UserName, "@", "HCL.CS.SF.com");

        // Getting Security Questions
        var securityQuestionResult_url = BaseUrl + ApiRoutePathConstants.GetAllSecurityQuestions;
        var securityQuestionResul_response = await FrontChannelClient
            .PostAsync(
                securityQuestionResult_url,
                null);
        var securityQuestionsDetails = await securityQuestionResul_response.Content.ReadAsStringAsync();

        var securityQuestionsResult =
            JsonConvert.DeserializeObject<List<SecurityQuestionModel>>(securityQuestionsDetails).ToList();

        // Adding questions to user
        if (securityQuestionsResult != null && securityQuestionsResult.Count > 0)
        {
            userModelInput.UserSecurityQuestion[0].SecurityQuestionId = securityQuestionsResult[0].Id;
            userModelInput.UserSecurityQuestion[0].Answer = "Test1";
            userModelInput.UserSecurityQuestion[1].SecurityQuestionId = securityQuestionsResult[1].Id;
            userModelInput.UserSecurityQuestion[1].Answer = "Test2";
            userModelInput.UserSecurityQuestion[2].SecurityQuestionId = securityQuestionsResult[2].Id;
            userModelInput.UserSecurityQuestion[2].Answer = "Test3";
        }

        // Adding User
        var registerUser_Url = BaseUrl + ApiRoutePathConstants.RegisterUser;
        var registerUser_response = await FrontChannelClient
            .PostAsync(
                registerUser_Url,
                new StringContent(JsonConvert.SerializeObject(userModelInput), Encoding.UTF8, "application/json"));
        var userDetails = await registerUser_response.Content.ReadAsStringAsync();
        var registerUser_result = JsonConvert.DeserializeObject<FrameworkResult>(userDetails);
        registerUser_result.Should().BeOfType<FrameworkResult>();
        registerUser_result.Status.Should().Be(ResultStatus.Failed);
        registerUser_result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.UserClaimTypeRequired);
    }

    [Fact]
    [Trait("Category", "ErrorCase")]
    public async Task RegisterUserAsync_ClaimValueNULL_ReturnError()
    {
        // Getting create User Model.
        var userModelInput = CreateUserRequestModel();
        var randomString = random.Next().ToString();
        userModelInput.UserName = random.Next().ToString();
        userModelInput.UserClaims[0].ClaimValue = null;
        userModelInput.Email = string.Concat(userModelInput.UserName, "@", "HCL.CS.SF.com");

        // Getting Security Questions
        var securityQuestionResult_url = BaseUrl + ApiRoutePathConstants.GetAllSecurityQuestions;
        var securityQuestionResul_response = await FrontChannelClient
            .PostAsync(
                securityQuestionResult_url,
                null);
        var securityQuestionsDetails = await securityQuestionResul_response.Content.ReadAsStringAsync();

        var securityQuestionsResult =
            JsonConvert.DeserializeObject<List<SecurityQuestionModel>>(securityQuestionsDetails).ToList();

        // Adding questions to user
        if (securityQuestionsResult != null && securityQuestionsResult.Count > 0)
        {
            userModelInput.UserSecurityQuestion[0].SecurityQuestionId = securityQuestionsResult[0].Id;
            userModelInput.UserSecurityQuestion[0].Answer = "Test1";
            userModelInput.UserSecurityQuestion[1].SecurityQuestionId = securityQuestionsResult[1].Id;
            userModelInput.UserSecurityQuestion[1].Answer = "Test2";
            userModelInput.UserSecurityQuestion[2].SecurityQuestionId = securityQuestionsResult[2].Id;
            userModelInput.UserSecurityQuestion[2].Answer = "Test3";
        }

        // Adding User
        var registerUser_Url = BaseUrl + ApiRoutePathConstants.RegisterUser;
        var registerUser_response = await FrontChannelClient
            .PostAsync(
                registerUser_Url,
                new StringContent(JsonConvert.SerializeObject(userModelInput), Encoding.UTF8, "application/json"));
        var userDetails = await registerUser_response.Content.ReadAsStringAsync();
        var registerUser_result = JsonConvert.DeserializeObject<FrameworkResult>(userDetails);
        registerUser_result.Should().BeOfType<FrameworkResult>();
        registerUser_result.Status.Should().Be(ResultStatus.Failed);
        registerUser_result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.UserClaimValueRequired);
    }

    [Fact]
    [Trait("Category", "SuccessCase")]
    public async Task UpdateUserAsync_Success()
    {
        // Register User
        var userInfo = await RegisterUserAsync_ReturnSuccess();
        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);
        // Get User by Name
        var username = userInfo.UserName;
        var getUserByUserName_url = BaseUrl + ApiRoutePathConstants.GetUserByName;
        var getUserByUserName_response = await FrontChannelClient
            .PostAsync(
                getUserByUserName_url,
                new StringContent(JsonConvert.SerializeObject(username), Encoding.UTF8, "application/json"));
        var userDetails = await getUserByUserName_response.Content.ReadAsStringAsync();
        var getUserDetailsByUserNameResult = JsonConvert.DeserializeObject<UserModel>(userDetails);
        getUserDetailsByUserNameResult.Should().NotBeNull();
        getUserDetailsByUserNameResult.UserName.Should().BeEquivalentTo(username);

        // Update User.
        // Login user due to Secure Api
        // Update Details.
        getUserDetailsByUserNameResult.LastName = "Ryan";
        getUserDetailsByUserNameResult.Email = "pr@avengers.com" + random.Next();
        var updateUser_Url = BaseUrl + ApiRoutePathConstants.UpdateUser;
        var updateUser_response = await FrontChannelClient
            .PostAsync(
                updateUser_Url,
                new StringContent(JsonConvert.SerializeObject(getUserDetailsByUserNameResult), Encoding.UTF8,
                    "application/json"));
        var updated_userDetails = await updateUser_response.Content.ReadAsStringAsync();
        var updateUser_result = JsonConvert.DeserializeObject<FrameworkResult>(updated_userDetails);
        updateUser_result.Should().BeOfType<FrameworkResult>();
        updateUser_result.Status.Should().Be(ResultStatus.Success);
    }

    [Fact]
    [Trait("Category", "SuccessCase")]
    public async Task IsUserExistById_ReturnSuccess()
    {
        // Register User
        var userInfo = await RegisterUserAsync_ReturnSuccess();
        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);
        // Get User by Name
        var username = userInfo.UserName;
        var getUserByUserName_url = BaseUrl + ApiRoutePathConstants.GetUserByName;
        var getUserByUserName_response = await FrontChannelClient
            .PostAsync(
                getUserByUserName_url,
                new StringContent(JsonConvert.SerializeObject(username), Encoding.UTF8, "application/json"));
        var userDetails = await getUserByUserName_response.Content.ReadAsStringAsync();
        var getUserDetailsByUserNameResult = JsonConvert.DeserializeObject<UserModel>(userDetails);

        var userId = getUserDetailsByUserNameResult.Id;

        var url = BaseUrl + ApiRoutePathConstants.IsUserExistsById;
        var response = await FrontChannelClient
            .PostAsync(
                url,
                new StringContent(JsonConvert.SerializeObject(userId), Encoding.UTF8, "application/json"));
        var isUserExistResponse = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<bool>(isUserExistResponse);
        result.Should().Be(true);
    }

    [Fact]
    [Trait("Category", "SuccessCase")]
    public async Task IsUserExistByName_ReturnSuccess()
    {
        var username = "BobUser";
        var url = BaseUrl + ApiRoutePathConstants.IsUserExistsByName;
        var response = await FrontChannelClient
            .PostAsync(
                url,
                new StringContent(JsonConvert.SerializeObject(username), Encoding.UTF8, "application/json"));
        var isUserExistResponse = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<bool>(isUserExistResponse);
        result.Should().Be(true);
    }

    [Fact]
    [Trait("Category", "SuccessCase")]
    public async Task GetUserByNameAsync_ReturnsSuccess()
    {
        // With Seed Data
        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);

        var url = BaseUrl + ApiRoutePathConstants.GetUserByName;
        var response = await FrontChannelClient
            .PostAsync(
                url,
                new StringContent(JsonConvert.SerializeObject(userName), Encoding.UTF8, "application/json"));
        var userDetails = await response.Content.ReadAsStringAsync();
        var getUserDetailsByUserNameResult = JsonConvert.DeserializeObject<UserModel>(userDetails);
        getUserDetailsByUserNameResult.Should().NotBeNull();
        getUserDetailsByUserNameResult.UserName.Should().BeEquivalentTo(userName);
    }

    [Fact]
    [Trait("Category", "SuccessCase")]
    public async Task GetUserByEmailAsync_ReturnsSuccess()
    {
        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);

        // With Seed Data
        var userEmailId = "bob.test@HCL.CS.SF.com";
        var url = BaseUrl + ApiRoutePathConstants.GetUserByEmail;
        var response = await FrontChannelClient
            .PostAsync(url,
                new StringContent(JsonConvert.SerializeObject(userEmailId), Encoding.UTF8, "application/json"));
        var userDetails = await response.Content.ReadAsStringAsync();
        var getUserDetailsByEmailResult = JsonConvert.DeserializeObject<UserModel>(userDetails);
        getUserDetailsByEmailResult.Should().NotBeNull();
        getUserDetailsByEmailResult.Email.Should().BeEquivalentTo(userEmailId);
    }

    [Fact]
    [Trait("Category", "SuccessCase")]
    public async Task GetUserByIdAsync_ReturnsSuccess()
    {
        // Register User
        var userInfo = await RegisterUserAsync_ReturnSuccess();
        // Get User by Name
        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);

        var username = userInfo.UserName;
        var getUserByUserName_url = BaseUrl + ApiRoutePathConstants.GetUserByName;
        var getUserByUserName_response = await FrontChannelClient
            .PostAsync(getUserByUserName_url,
                new StringContent(JsonConvert.SerializeObject(username), Encoding.UTF8, "application/json"));
        var getUserDetails = await getUserByUserName_response.Content.ReadAsStringAsync();
        var getUserDetailsByUserNameResult = JsonConvert.DeserializeObject<UserModel>(getUserDetails);

        var userId = getUserDetailsByUserNameResult.Id;

        var url = BaseUrl + ApiRoutePathConstants.GetUserById;
        var response = await FrontChannelClient
            .PostAsync(
                url,
                new StringContent(JsonConvert.SerializeObject(userId), Encoding.UTF8, "application/json"));
        var userDetails = await response.Content.ReadAsStringAsync();
        var getUserDetailsByUserIdResult = JsonConvert.DeserializeObject<UserModel>(userDetails);
        getUserDetailsByUserIdResult.Should().NotBeNull();
        getUserDetailsByUserIdResult.Id.Should().Be(userId);
    }

    [Fact]
    [Trait("Category", "SuccessCase")]
    public async Task DeleteUser_By_UserName_Success()
    {
        // Add User.
        // Register User
        var userInfo = await RegisterUserAsync_ReturnSuccess();

        // Generate Token
        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);
        var deleteUser_url = BaseUrl + ApiRoutePathConstants.DeleteUserByName;
        var response = await FrontChannelClient.PostAsync(
            deleteUser_url,
            new StringContent(JsonConvert.SerializeObject(userInfo.UserName), Encoding.UTF8, "application/json"));
        var deleteUser_Resopnse = await response.Content.ReadAsStringAsync();
        var deleteUser_Result = JsonConvert.DeserializeObject<FrameworkResult>(deleteUser_Resopnse);
        deleteUser_Result.Should().BeOfType<FrameworkResult>();
        deleteUser_Result.Status.Should().Be(ResultStatus.Success);
    }

    [Fact]
    [Trait("Category", "SuccessCase")]
    public async Task DeleteUser_By_Id_Success()
    {
        // Add User.
        // Register User
        var userInfo = await RegisterUserAsync_ReturnSuccess();

        // Generate Token
        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);
        var getUserByUserName_url = BaseUrl + ApiRoutePathConstants.GetUserByName;
        var getUserByUserName_response = await FrontChannelClient
            .PostAsync(
                getUserByUserName_url,
                new StringContent(JsonConvert.SerializeObject(userInfo.UserName), Encoding.UTF8, "application/json"));
        var userDetails = await getUserByUserName_response.Content.ReadAsStringAsync();
        var getUserDetailsByUserNameResult = JsonConvert.DeserializeObject<UserModel>(userDetails);
        getUserDetailsByUserNameResult.Should().NotBeNull();
        getUserDetailsByUserNameResult.UserName.Should().BeEquivalentTo(userInfo.UserName);

        // Delete User By Id
        var userId_ToDelete = getUserDetailsByUserNameResult.Id;
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);
        var deleteUser_url = BaseUrl + ApiRoutePathConstants.DeleteUserById;
        var response = await FrontChannelClient.PostAsync(
            deleteUser_url,
            new StringContent(JsonConvert.SerializeObject(userId_ToDelete), Encoding.UTF8, "application/json"));
        var deleteUser_Resopnse = await response.Content.ReadAsStringAsync();
        var deleteUser_Result = JsonConvert.DeserializeObject<FrameworkResult>(deleteUser_Resopnse);
        deleteUser_Result.Should().BeOfType<FrameworkResult>();
        deleteUser_Result.Status.Should().Be(ResultStatus.Success);
    }

    [Fact]
    [Trait("Category", "SuccessCase")]
    public async Task ChangePassword_Success()
    {
        // Add User.
        // Register User
        var userInfo = await RegisterUserAsync_ReturnSuccess();

        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);


        var getUserByUserName_url = BaseUrl + ApiRoutePathConstants.GetUserByName;
        var getUserByUserName_response = await FrontChannelClient
            .PostAsync(
                getUserByUserName_url,
                new StringContent(JsonConvert.SerializeObject(userInfo.UserName), Encoding.UTF8, "application/json"));
        var userDetails = await getUserByUserName_response.Content.ReadAsStringAsync();
        var getUserDetailsByUserNameResult = JsonConvert.DeserializeObject<UserModel>(userDetails);
        getUserDetailsByUserNameResult.Should().NotBeNull();
        getUserDetailsByUserNameResult.UserName.Should().BeEquivalentTo(userInfo.UserName);

        // Change Password
        var userId = getUserDetailsByUserNameResult.Id;
        var currentPassword = userInfo.Password;
        var newPassword = "Test@1989";
        var data = new
        {
            user_id = userId,
            current_password = currentPassword,
            new_password = newPassword
        };
        var changePassword_url = BaseUrl + ApiRoutePathConstants.ChangePassword;
        var response = await FrontChannelClient.PostAsync(
            changePassword_url,
            new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json"));
        var changePassword_Resopnse = await response.Content.ReadAsStringAsync();
        var changePassword_Result = JsonConvert.DeserializeObject<FrameworkResult>(changePassword_Resopnse);
        changePassword_Result.Should().BeOfType<FrameworkResult>();
        changePassword_Result.Status.Should().Be(ResultStatus.Success);
    }

    //[Fact]
    //[Trait("Category", "SuccessCase")]
    //public async Task ResetPassword_Success()
    //{
    //    // Add User.
    //    // Register User
    //    Users_Info userInfo = await RegisterUserAsync_ReturnSuccess();
    //    TokenResponseResultModel token = await GetAccessToken();
    //    FrontChannelClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.access_token);
    //    //// Get User By Name
    //    var getUserByUserName_url = BaseUrl + ApiRoutePathConstants.GetUserByName;
    //    var getUserByUserName_response = await FrontChannelClient
    //                                          .PostAsync(
    //                                                      getUserByUserName_url,
    //                                                      new StringContent(JsonConvert.SerializeObject(userInfo.UserName), Encoding.UTF8, "application/json"));
    //    var userDetails = await getUserByUserName_response.Content.ReadAsStringAsync();
    //    UserModel getUserDetailsByUserNameResult = JsonConvert.DeserializeObject<UserModel>(userDetails);
    //    getUserDetailsByUserNameResult.Should().NotBeNull();
    //    getUserDetailsByUserNameResult.UserName.Should().BeEquivalentTo(userInfo.UserName);

    //    var datauserinfo = new
    //    {
    //        user_name = userInfo.UserName,
    //        notification_type = NotificationTypes.SMS,
    //    };
    //    // Generate reset password token
    //    Guid userId = getUserDetailsByUserNameResult.Id;
    //    var generateResetPasswordToken_url = BaseUrl + ApiRoutePathConstants.GeneratePasswordResetToken;
    //    var generateResetPasswordToken_response = await FrontChannelClient
    //                                          .PostAsync(
    //                                                      generateResetPasswordToken_url,
    //                                                      new StringContent(JsonConvert.SerializeObject(datauserinfo), Encoding.UTF8, "application/json"));
    //    string resetPasswordToken = await generateResetPasswordToken_response.Content.ReadAsStringAsync();
    //    string resetPasswordTokenResult = JsonConvert.DeserializeObject<string>(resetPasswordToken);
    //    resetPasswordTokenResult.Should().NotBeNullOrWhiteSpace();

    //    // Reset Password
    //    string newPassword = "Test@1989";
    //    var data = new
    //    {
    //        user_id = userId,
    //        password_reset_token = resetPasswordTokenResult,
    //        new_password = newPassword,
    //    };
    //    var resetPassword_url = HCLCSSFFakeSetup.BaseUrl + ApiRoutePathConstants.ResetPassword;
    //    var resetPassword_response = await FrontChannelClient.PostAsync(
    //                                                        resetPassword_url,
    //                                                        new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json"));
    //    var resetPassword_Resopnse = await resetPassword_response.Content.ReadAsStringAsync();
    //    var resetPassword_Result = JsonConvert.DeserializeObject<ApiDomainModel.FrameworkResult>(resetPassword_Resopnse);
    //    resetPassword_Result.Should().BeOfType<ApiDomainModel.FrameworkResult>();
    //    resetPassword_Result.Status.Should().Be(ApiDomainModel.ResultStatus.Success);
    //}

    [Fact]
    [Trait("Category", "SuccessCase")]
    public async Task LockUserAsync_ById_Success()
    {
        var userInfo = await RegisterUserAsync_ReturnSuccess();

        // Generate Access token
        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);

        // Get USer by name
        var getUserByUserName_url = BaseUrl + ApiRoutePathConstants.GetUserByName;
        var getUserByUserName_response = await FrontChannelClient
            .PostAsync(
                getUserByUserName_url,
                new StringContent(JsonConvert.SerializeObject(userInfo.UserName), Encoding.UTF8, "application/json"));
        var userDetails = await getUserByUserName_response.Content.ReadAsStringAsync();
        var getUserDetailsByUserNameResult = JsonConvert.DeserializeObject<UserModel>(userDetails);
        getUserDetailsByUserNameResult.Should().NotBeNull();
        getUserDetailsByUserNameResult.UserName.Should().BeEquivalentTo(userInfo.UserName);

        // Lock User
        var userId = getUserDetailsByUserNameResult.Id;
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);
        var url = BaseUrl + ApiRoutePathConstants.LockUser;
        var response = await FrontChannelClient.PostAsync(
            url,
            new StringContent(JsonConvert.SerializeObject(userId), Encoding.UTF8, "application/json"));
        var lockUserResopnse = await response.Content.ReadAsStringAsync();
        var lockUser_Result = JsonConvert.DeserializeObject<FrameworkResult>(lockUserResopnse);
        lockUser_Result.Should().BeOfType<FrameworkResult>();
        lockUser_Result.Status.Should().Be(ResultStatus.Success);
    }

    [Fact]
    [Trait("Category", "ErrorCase")]
    public async Task LockUserAsync_InvalidUserId_ReturnFail()
    {
        var userInfo = await RegisterUserAsync_ReturnSuccess();

        // Generate Access token
        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);
        // Lock User
        var userId = Guid.NewGuid();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);
        var url = BaseUrl + ApiRoutePathConstants.LockUser;
        var response = await FrontChannelClient.PostAsync(
            url,
            new StringContent(JsonConvert.SerializeObject(userId), Encoding.UTF8, "application/json"));
        var lockUserResopnse = await response.Content.ReadAsStringAsync();
        var lockUser_Result = JsonConvert.DeserializeObject<FrameworkResult>(lockUserResopnse);
        lockUser_Result.Should().BeOfType<FrameworkResult>();
        lockUser_Result.Should().BeOfType<FrameworkResult>();
        lockUser_Result.Status.Should().Be(ResultStatus.Failed);
        lockUser_Result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.InvalidUserId);
    }

    [Fact]
    [Trait("Category", "SuccessCase")]
    public async Task LockUser_ById_LockEndDateGiven_Success()
    {
        var userInfo = await RegisterUserAsync_ReturnSuccess();

        // Generate Access token
        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);

        // Get User by name
        var getUserByUserName_url = BaseUrl + ApiRoutePathConstants.GetUserByName;
        var getUserByUserName_response = await FrontChannelClient
            .PostAsync(
                getUserByUserName_url,
                new StringContent(JsonConvert.SerializeObject(userInfo.UserName), Encoding.UTF8, "application/json"));
        var userDetails = await getUserByUserName_response.Content.ReadAsStringAsync();
        var getUserDetailsByUserNameResult = JsonConvert.DeserializeObject<UserModel>(userDetails);
        getUserDetailsByUserNameResult.Should().NotBeNull();
        getUserDetailsByUserNameResult.UserName.Should().BeEquivalentTo(userInfo.UserName);

        // Lock User with EndDate
        var userId = getUserDetailsByUserNameResult.Id;
        DateTime? lockOutEndDate = DateTime.UtcNow.AddDays(1); // User to be locked for 1 Day.
        var lock_data = new
        {
            user_id = userId,
            end_date = lockOutEndDate
        };

        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);
        var url = BaseUrl + ApiRoutePathConstants.LockUserWithEndDatePath;
        var response = await FrontChannelClient
            .PostAsync(
                url,
                new StringContent(JsonConvert.SerializeObject(lock_data), Encoding.UTF8, "application/json"));
        var lockUserResopnse = await response.Content.ReadAsStringAsync();
        var lockUser_Result = JsonConvert.DeserializeObject<FrameworkResult>(lockUserResopnse);
        lockUser_Result.Should().BeOfType<FrameworkResult>();
        lockUser_Result.Status.Should().Be(ResultStatus.Success);
    }

    [Fact]
    [Trait("Category", "SuccessCase")]
    public async Task UnlockUserAsync_ReturnsSuccess()
    {
        // Register User
        var userInfo = await RegisterUserAsync_ReturnSuccess();

        // Generate Access Token
        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);
        // Get User by name
        var getUserByUserName_url = BaseUrl + ApiRoutePathConstants.GetUserByName;
        var getUserByUserName_response = await FrontChannelClient
            .PostAsync(
                getUserByUserName_url,
                new StringContent(JsonConvert.SerializeObject(userInfo.UserName), Encoding.UTF8, "application/json"));
        var userDetails = await getUserByUserName_response.Content.ReadAsStringAsync();
        var getUserDetailsByUserNameResult = JsonConvert.DeserializeObject<UserModel>(userDetails);
        getUserDetailsByUserNameResult.Should().NotBeNull();
        getUserDetailsByUserNameResult.UserName.Should().BeEquivalentTo(userInfo.UserName);

        // UnLock User
        var url = BaseUrl + ApiRoutePathConstants.LockUser;
        var response = await FrontChannelClient.PostAsync(
            url,
            new StringContent(JsonConvert.SerializeObject(getUserDetailsByUserNameResult.Id), Encoding.UTF8,
                "application/json"));
        var lockUserResopnse = await response.Content.ReadAsStringAsync();
        var lockUser_Result = JsonConvert.DeserializeObject<FrameworkResult>(lockUserResopnse);
        lockUser_Result.Should().BeOfType<FrameworkResult>();
        lockUser_Result.Status.Should().Be(ResultStatus.Success);

        // Unlock User
        var url_annonymoous = BaseUrl + ApiRoutePathConstants.UnLockUser;
        var unLockResponse = await FrontChannelClient
            .PostAsync(url_annonymoous,
                new StringContent(JsonConvert.SerializeObject(userInfo.UserName), Encoding.UTF8, "application/json"));
        var unLockUserResponse = await unLockResponse.Content.ReadAsStringAsync();
        var unLockUserResult = JsonConvert.DeserializeObject<FrameworkResult>(unLockUserResponse);
        unLockUserResult.Should().BeOfType<FrameworkResult>();
        unLockUserResult.Status.Should().Be(ResultStatus.Success);
    }

    [Fact]
    [Trait("Category", "ErrorCase")]
    public async Task UnlockUserAsync_InvalidUsderId_ReturnsFail()
    {
        // Unlock User
        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);

        var userIdForUnLocking = Guid.NewGuid();
        var url_annonymoous = BaseUrl + ApiRoutePathConstants.UnLockUser;
        var unLockResponse = await FrontChannelClient
            .PostAsync(url_annonymoous,
                new StringContent(JsonConvert.SerializeObject(userIdForUnLocking), Encoding.UTF8, "application/json"));
        var unLockUserResponse = await unLockResponse.Content.ReadAsStringAsync();
        var unLockUserResult = JsonConvert.DeserializeObject<FrameworkResult>(unLockUserResponse);
        unLockUserResult.Should().BeOfType<FrameworkResult>();
        unLockUserResult.Status.Should().Be(ResultStatus.Failed);
        unLockUserResult.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.InvalidUserId);
    }

    //[Fact]
    //[Trait("Category", "SuccessCase")]
    //public async Task UnlockUserAsync_ByToken_ReturnsSuccess()
    //{
    //    // Register User
    //    Users_Info userInfo = await RegisterUserAsync_ReturnSuccess();

    //    // Generate Access Token
    //    TokenResponseResultModel token = await GetAccessToken();
    //    FrontChannelClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.access_token);

    //    // Get User by name
    //    var getUserByUserName_url = BaseUrl + ApiRoutePathConstants.GetUserByName;
    //    var getUserByUserName_response = await FrontChannelClient
    //                                          .PostAsync(
    //                                                      getUserByUserName_url,
    //                                                      new StringContent(JsonConvert.SerializeObject(userInfo.UserName), Encoding.UTF8, "application/json"));
    //    var userDetails = await getUserByUserName_response.Content.ReadAsStringAsync();
    //    UserModel getUserDetailsByUserNameResult = JsonConvert.DeserializeObject<UserModel>(userDetails);
    //    getUserDetailsByUserNameResult.Should().NotBeNull();
    //    getUserDetailsByUserNameResult.UserName.Should().BeEquivalentTo(userInfo.UserName);

    //    // Get UserToken
    //    Guid userId = getUserDetailsByUserNameResult.Id;
    //    string purpose = "Unlock Token";
    //    var generateUserToken_data = new
    //    {
    //        user_id = userId,
    //        token_purpose = purpose,
    //    };
    //    var generateUserToken_url = BaseUrl + ApiRoutePathConstants.GenerateUserTokenAsync;
    //    var generateUserToken_response = await FrontChannelClient
    //                                          .PostAsync(
    //                                                      generateUserToken_url,
    //                                                      new StringContent(JsonConvert.SerializeObject(generateUserToken_data), Encoding.UTF8, "application/json"));
    //    var userToken = await generateUserToken_response.Content.ReadAsStringAsync();
    //    var userTokenResult = JsonConvert.DeserializeObject<string>(userToken);
    //    userTokenResult.Should().NotBeNullOrWhiteSpace();

    //    // Lock User
    //    FrontChannelClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.access_token);
    //    var url = HCLCSSFFakeSetup.BaseUrl + ApiRoutePathConstants.LockUser;
    //    var response = await FrontChannelClient.PostAsync(
    //        url,
    //        new StringContent(JsonConvert.SerializeObject(userId), Encoding.UTF8, "application/json"));
    //    var lockUserResopnse = await response.Content.ReadAsStringAsync();
    //    var lockUser_Result = JsonConvert.DeserializeObject<ApiDomainModel.FrameworkResult>(lockUserResopnse);
    //    lockUser_Result.Should().BeOfType<ApiDomainModel.FrameworkResult>();
    //    lockUser_Result.Status.Should().Be(ApiDomainModel.ResultStatus.Success);

    //    // Unlock User
    //    var unlockUserData = new
    //    {
    //        user_id = userId,
    //        user_token = userTokenResult,
    //        token_purpose = purpose,
    //    };
    //    var url_annonymoous = BaseUrl + ApiRoutePathConstants.UnLockUserByToken;
    //    var unLockResponse = await FrontChannelClient
    //                                        .PostAsync(
    //                                                            url_annonymoous,
    //                                                            new StringContent(JsonConvert.SerializeObject(unlockUserData), Encoding.UTF8, "application/json"));
    //    var unLockUserResponse = await unLockResponse.Content.ReadAsStringAsync();
    //    var unLockUserResult = JsonConvert.DeserializeObject<ApiDomainModel.FrameworkResult>(unLockUserResponse);
    //    unLockUserResult.Should().BeOfType<ApiDomainModel.FrameworkResult>();
    //    unLockUserResult.Status.Should().Be(ApiDomainModel.ResultStatus.Success);
    //}

    [Fact]
    [Trait("Category", "ErrorCase")]
    public async Task UnlockUserAsync_InvalidToken_ReturnsFail()
    {
        // Register User
        var userInfo = await RegisterUserAsync_ReturnSuccess();
        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);
        // Get UserToken
        var purpose = "Unlock Token";

        var getUserByUserName_url = BaseUrl + ApiRoutePathConstants.GetUserByName;
        var getUserByUserName_response = await FrontChannelClient
            .PostAsync(
                getUserByUserName_url,
                new StringContent(JsonConvert.SerializeObject(userInfo.UserName), Encoding.UTF8, "application/json"));
        var userDetails = await getUserByUserName_response.Content.ReadAsStringAsync();
        var getUserDetailsByUserNameResult = JsonConvert.DeserializeObject<UserModel>(userDetails);
        getUserDetailsByUserNameResult.Should().NotBeNull();
        getUserDetailsByUserNameResult.UserName.Should().BeEquivalentTo(userInfo.UserName);

        // Lock User
        var url = BaseUrl + ApiRoutePathConstants.LockUser;
        var response = await FrontChannelClient.PostAsync(
            url,
            new StringContent(JsonConvert.SerializeObject(getUserDetailsByUserNameResult.Id), Encoding.UTF8,
                "application/json"));
        var lockUserResopnse = await response.Content.ReadAsStringAsync();
        var lockUser_Result = JsonConvert.DeserializeObject<FrameworkResult>(lockUserResopnse);
        lockUser_Result.Should().BeOfType<FrameworkResult>();
        lockUser_Result.Status.Should().Be(ResultStatus.Success);

        // Unlock User
        var unlockUserData = new
        {
            user_name = userInfo.UserName,
            user_token = "12454t5",
            token_purpose = purpose
        };
        var url_annonymoous = BaseUrl + ApiRoutePathConstants.UnLockUserByToken;
        var unLockResponse = await FrontChannelClient
            .PostAsync(url_annonymoous,
                new StringContent(JsonConvert.SerializeObject(unlockUserData), Encoding.UTF8, "application/json"));
        var unLockUserResponse = await unLockResponse.Content.ReadAsStringAsync();
        var unLockUserResult = JsonConvert.DeserializeObject<FrameworkResult>(unLockUserResponse);
        unLockUserResult.Should().BeOfType<FrameworkResult>();
        unLockUserResult.Status.Should().Be(ResultStatus.Failed);
        unLockUserResult.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.InvalidUserToken);
    }

    [Fact]
    [Trait("Category", "SuccessCase")]
    public async Task UnlockUserAsync_ByUserSecurityQuestions_ReturnsSuccess()
    {
        // Register User
        var userInfo = await RegisterUserAsync_ReturnSuccess();

        // Generate Access Token
        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);

        // Get User by name
        var getUserByUserName_url = BaseUrl + ApiRoutePathConstants.GetUserByName;
        var getUserByUserName_response = await FrontChannelClient
            .PostAsync(
                getUserByUserName_url,
                new StringContent(JsonConvert.SerializeObject(userInfo.UserName), Encoding.UTF8, "application/json"));
        var userDetails = await getUserByUserName_response.Content.ReadAsStringAsync();
        var getUserDetailsByUserNameResult = JsonConvert.DeserializeObject<UserModel>(userDetails);
        getUserDetailsByUserNameResult.Should().NotBeNull();
        getUserDetailsByUserNameResult.UserName.Should().BeEquivalentTo(userInfo.UserName);

        // Lock User
        var userId = getUserDetailsByUserNameResult.Id;
        var url = BaseUrl + ApiRoutePathConstants.LockUser;
        var response = await FrontChannelClient.PostAsync(
            url,
            new StringContent(JsonConvert.SerializeObject(userId), Encoding.UTF8, "application/json"));
        var lockUserResopnse = await response.Content.ReadAsStringAsync();
        var lockUser_Result = JsonConvert.DeserializeObject<FrameworkResult>(lockUserResopnse);
        lockUser_Result.Should().BeOfType<FrameworkResult>();
        lockUser_Result.Status.Should().Be(ResultStatus.Success);

        // Unlock User
        var userSecurityQuestions = userInfo.userSecurityQuestionModels;
        foreach (var item in userSecurityQuestions) item.UserId = userId;

        var unlockUserData = new
        {
            user_name = userInfo.UserName,
            user_security_questions_list = userSecurityQuestions
        };
        var url_annonymoous = BaseUrl + ApiRoutePathConstants.UnLockUserByuserSecurityQuestions;
        var unLockResponse = await FrontChannelClient
            .PostAsync(
                url_annonymoous,
                new StringContent(JsonConvert.SerializeObject(unlockUserData), Encoding.UTF8, "application/json"));
        var unLockUserResponse = await unLockResponse.Content.ReadAsStringAsync();
        var unLockUserResult = JsonConvert.DeserializeObject<FrameworkResult>(unLockUserResponse);
        unLockUserResult.Should().BeOfType<FrameworkResult>();
        unLockUserResult.Status.Should().Be(ResultStatus.Success);
    }

    [Fact]
    [Trait("Category", "SuccessCase")]
    public async Task AddClaimAsync_Success()
    {
        // Register User
        var userInfo = await RegisterUserAsync_ReturnSuccess();

        // Generate Access Token
        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);
        // Get User by name
        var getUserByUserName_url = BaseUrl + ApiRoutePathConstants.GetUserByName;
        var getUserByUserName_response = await FrontChannelClient
            .PostAsync(
                getUserByUserName_url,
                new StringContent(JsonConvert.SerializeObject(userInfo.UserName), Encoding.UTF8, "application/json"));
        var userDetails = await getUserByUserName_response.Content.ReadAsStringAsync();
        var getUserDetailsByUserNameResult = JsonConvert.DeserializeObject<UserModel>(userDetails);
        getUserDetailsByUserNameResult.Should().NotBeNull();

        var userClaimModel = UserHelper.CreateUserClaimModel();
        var userid = getUserDetailsByUserNameResult.Id;
        userClaimModel.ClaimType = userClaimModel.ClaimType + string.Empty + random.Next();
        userClaimModel.UserId = userid;

        var addClaimUrl = BaseUrl + ApiRoutePathConstants.AddClaim;
        var addClaimResponse = await FrontChannelClient.PostAsync(
            addClaimUrl,
            new StringContent(JsonConvert.SerializeObject(userClaimModel), Encoding.UTF8, "application/json"));
        var claimResopnse = await addClaimResponse.Content.ReadAsStringAsync();
        var claim_Result = JsonConvert.DeserializeObject<FrameworkResult>(claimResopnse);

        claim_Result.Should().BeOfType<FrameworkResult>();
        claim_Result.Status.Should().Be(ResultStatus.Success);
    }

    [Fact]
    [Trait("Category", "ErrorCase")]
    public async Task AddClaimAsync_InvalidUserId_ReturnFail()
    {
        // Register User
        var userInfo = await RegisterUserAsync_ReturnSuccess();

        // Generate Access Token
        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);
        // Get User by name
        var getUserByUserName_url = BaseUrl + ApiRoutePathConstants.GetUserByName;
        var getUserByUserName_response = await FrontChannelClient
            .PostAsync(
                getUserByUserName_url,
                new StringContent(JsonConvert.SerializeObject(userInfo.UserName), Encoding.UTF8, "application/json"));
        var userDetails = await getUserByUserName_response.Content.ReadAsStringAsync();
        var getUserDetailsByUserNameResult = JsonConvert.DeserializeObject<UserModel>(userDetails);
        getUserDetailsByUserNameResult.Should().NotBeNull();

        var userClaimModel = UserHelper.CreateUserClaimModel();
        var userid = Guid.NewGuid();
        userClaimModel.ClaimType = userClaimModel.ClaimType + string.Empty + random.Next();
        userClaimModel.UserId = userid;

        var addClaimUrl = BaseUrl + ApiRoutePathConstants.AddClaim;
        var addClaimResponse = await FrontChannelClient.PostAsync(
            addClaimUrl,
            new StringContent(JsonConvert.SerializeObject(userClaimModel), Encoding.UTF8, "application/json"));
        var claimResopnse = await addClaimResponse.Content.ReadAsStringAsync();
        var claim_Result = JsonConvert.DeserializeObject<FrameworkResult>(claimResopnse);

        claim_Result.Should().BeOfType<FrameworkResult>();
        claim_Result.Status.Should().Be(ResultStatus.Failed);
        claim_Result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.InvalidUserId);
    }

    [Fact]
    [Trait("Category", "ErrorCase")]
    public async Task AddClaimAsync_InvalidClaimType_ReturnFail()
    {
        // Register User
        var userInfo = await RegisterUserAsync_ReturnSuccess();

        // Generate Access Token
        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);
        // Get User by name
        var getUserByUserName_url = BaseUrl + ApiRoutePathConstants.GetUserByName;
        var getUserByUserName_response = await FrontChannelClient
            .PostAsync(
                getUserByUserName_url,
                new StringContent(JsonConvert.SerializeObject(userInfo.UserName), Encoding.UTF8, "application/json"));
        var userDetails = await getUserByUserName_response.Content.ReadAsStringAsync();
        var getUserDetailsByUserNameResult = JsonConvert.DeserializeObject<UserModel>(userDetails);
        getUserDetailsByUserNameResult.Should().NotBeNull();

        var userClaimModel = UserHelper.CreateUserClaimModel();
        var userid = getUserDetailsByUserNameResult.Id;
        userClaimModel.ClaimType = null;
        userClaimModel.UserId = userid;

        var addClaimUrl = BaseUrl + ApiRoutePathConstants.AddClaim;
        var addClaimResponse = await FrontChannelClient.PostAsync(
            addClaimUrl,
            new StringContent(JsonConvert.SerializeObject(userClaimModel), Encoding.UTF8, "application/json"));
        var claimResopnse = await addClaimResponse.Content.ReadAsStringAsync();
        var claim_Result = JsonConvert.DeserializeObject<FrameworkResult>(claimResopnse);

        claim_Result.Should().BeOfType<FrameworkResult>();
        claim_Result.Status.Should().Be(ResultStatus.Failed);
        claim_Result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.UserClaimTypeRequired);
    }

    [Fact]
    [Trait("Category", "ErrorCase")]
    public async Task AddClaimAsync_ClaimTypeLenthExceded_ReturnFail()
    {
        // Register User
        var userInfo = await RegisterUserAsync_ReturnSuccess();

        // Generate Access Token
        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);
        // Get User by name
        var getUserByUserName_url = BaseUrl + ApiRoutePathConstants.GetUserByName;
        var getUserByUserName_response = await FrontChannelClient
            .PostAsync(
                getUserByUserName_url,
                new StringContent(JsonConvert.SerializeObject(userInfo.UserName), Encoding.UTF8, "application/json"));
        var userDetails = await getUserByUserName_response.Content.ReadAsStringAsync();
        var getUserDetailsByUserNameResult = JsonConvert.DeserializeObject<UserModel>(userDetails);
        getUserDetailsByUserNameResult.Should().NotBeNull();

        var userClaimModel = UserHelper.CreateUserClaimModel();
        var userid = getUserDetailsByUserNameResult.Id;
        userClaimModel.ClaimType =
            "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        userClaimModel.UserId = userid;

        var addClaimUrl = BaseUrl + ApiRoutePathConstants.AddClaim;
        var addClaimResponse = await FrontChannelClient.PostAsync(
            addClaimUrl,
            new StringContent(JsonConvert.SerializeObject(userClaimModel), Encoding.UTF8, "application/json"));
        var claimResopnse = await addClaimResponse.Content.ReadAsStringAsync();
        var claim_Result = JsonConvert.DeserializeObject<FrameworkResult>(claimResopnse);

        claim_Result.Should().BeOfType<FrameworkResult>();
        claim_Result.Status.Should().Be(ResultStatus.Success);
    }

    [Fact]
    [Trait("Category", "SuccessCase")]
    public async Task AddClaimListAsync_Success()
    {
        // Register User
        var userInfo = await RegisterUserAsync_ReturnSuccess();

        // Generate Access Token
        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);
        // Get User by name
        var getUserByUserName_url = BaseUrl + ApiRoutePathConstants.GetUserByName;
        var getUserByUserName_response = await FrontChannelClient
            .PostAsync(
                getUserByUserName_url,
                new StringContent(JsonConvert.SerializeObject(userInfo.UserName), Encoding.UTF8, "application/json"));
        var userDetails = await getUserByUserName_response.Content.ReadAsStringAsync();
        var getUserDetailsByUserNameResult = JsonConvert.DeserializeObject<UserModel>(userDetails);
        getUserDetailsByUserNameResult.Should().NotBeNull();

        var userClaimModel = UserHelper.CreateUserClaimModel_List(getUserDetailsByUserNameResult.Id);

        userClaimModel[0].ClaimType = userClaimModel[0].ClaimType + string.Empty + random.Next();
        userClaimModel[1].ClaimType = userClaimModel[1].ClaimType + string.Empty + random.Next();
        userClaimModel[2].ClaimType = userClaimModel[2].ClaimType + string.Empty + random.Next();

        userClaimModel[0].ClaimValue = userClaimModel[0].ClaimValue + string.Empty + random.Next();
        userClaimModel[1].ClaimValue = userClaimModel[1].ClaimValue + string.Empty + random.Next();
        userClaimModel[2].ClaimValue = userClaimModel[2].ClaimValue + string.Empty + random.Next();

        var addClaimUrl = BaseUrl + ApiRoutePathConstants.AddClaimList;
        var addClaimResponse = await FrontChannelClient.PostAsync(
            addClaimUrl,
            new StringContent(JsonConvert.SerializeObject(userClaimModel), Encoding.UTF8, "application/json"));
        var claimResopnse = await addClaimResponse.Content.ReadAsStringAsync();
        var claim_Result = JsonConvert.DeserializeObject<FrameworkResult>(claimResopnse);

        claim_Result.Should().BeOfType<FrameworkResult>();
        claim_Result.Status.Should().Be(ResultStatus.Success);
    }

    [Fact]
    [Trait("Category", "SuccessCase")]
    public async Task RemoveClaimAsync_Success()
    {
        // Register User
        var userInfo = await RegisterUserAsync_ReturnSuccess();

        // Generate Access Token
        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);
        // Get User by name
        var getUserByUserName_url = BaseUrl + ApiRoutePathConstants.GetUserByName;
        var getUserByUserName_response = await FrontChannelClient
            .PostAsync(
                getUserByUserName_url,
                new StringContent(JsonConvert.SerializeObject(userInfo.UserName), Encoding.UTF8, "application/json"));
        var userDetails = await getUserByUserName_response.Content.ReadAsStringAsync();
        var getUserDetailsByUserNameResult = JsonConvert.DeserializeObject<UserModel>(userDetails);
        getUserDetailsByUserNameResult.Should().NotBeNull();

        var userClaimModel = UserHelper.CreateUserClaimModel();
        var userid = getUserDetailsByUserNameResult.Id;
        userClaimModel.ClaimType = userClaimModel.ClaimType + string.Empty + random.Next();
        userClaimModel.UserId = userid;

        var addClaimUrl = BaseUrl + ApiRoutePathConstants.AddClaim;
        var addClaimResponse = await FrontChannelClient.PostAsync(
            addClaimUrl,
            new StringContent(JsonConvert.SerializeObject(userClaimModel), Encoding.UTF8, "application/json"));
        var claimResopnse = await addClaimResponse.Content.ReadAsStringAsync();
        var claim_Result = JsonConvert.DeserializeObject<FrameworkResult>(claimResopnse);

        claim_Result.Should().BeOfType<FrameworkResult>();
        claim_Result.Status.Should().Be(ResultStatus.Success);

        // Remove Claim
        var removeClaimUrl = BaseUrl + ApiRoutePathConstants.RemoveClaim;
        var removeClaimResponse = await FrontChannelClient.PostAsync(
            removeClaimUrl,
            new StringContent(JsonConvert.SerializeObject(userClaimModel), Encoding.UTF8, "application/json"));
        var removeClaimResopnse = await removeClaimResponse.Content.ReadAsStringAsync();
        var removeClaim_Result = JsonConvert.DeserializeObject<FrameworkResult>(removeClaimResopnse);
        removeClaim_Result.Should().BeOfType<FrameworkResult>();
        removeClaim_Result.Status.Should().Be(ResultStatus.Success);
    }

    [Fact]
    [Trait("Category", "ErrorCase")]
    public async Task RemoveClaimAsync_InvalidUserId_ReturnFail()
    {
        // Register User
        var userInfo = await RegisterUserAsync_ReturnSuccess();

        // Generate Access Token
        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);
        // Get User by name
        var getUserByUserName_url = BaseUrl + ApiRoutePathConstants.GetUserByName;
        var getUserByUserName_response = await FrontChannelClient
            .PostAsync(
                getUserByUserName_url,
                new StringContent(JsonConvert.SerializeObject(userInfo.UserName), Encoding.UTF8, "application/json"));
        var userDetails = await getUserByUserName_response.Content.ReadAsStringAsync();
        var getUserDetailsByUserNameResult = JsonConvert.DeserializeObject<UserModel>(userDetails);
        getUserDetailsByUserNameResult.Should().NotBeNull();

        var userClaimModel = UserHelper.CreateUserClaimModel();
        var userid = Guid.NewGuid();
        userClaimModel.ClaimType = userClaimModel.ClaimType + string.Empty + random.Next();
        userClaimModel.UserId = userid;

        // Remove Claim
        var removeClaimUrl = BaseUrl + ApiRoutePathConstants.RemoveClaim;
        var removeClaimResponse = await FrontChannelClient.PostAsync(
            removeClaimUrl,
            new StringContent(JsonConvert.SerializeObject(userClaimModel), Encoding.UTF8, "application/json"));
        var removeClaimResopnse = await removeClaimResponse.Content.ReadAsStringAsync();
        var removeClaim_Result = JsonConvert.DeserializeObject<FrameworkResult>(removeClaimResopnse);
        removeClaim_Result.Should().BeOfType<FrameworkResult>();
        removeClaim_Result.Status.Should().Be(ResultStatus.Failed);
        removeClaim_Result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.InvalidUserClaims);
    }

    [Fact]
    [Trait("Category", "SuccessCase")]
    public async Task RemoveCLaimListAsync_Success()
    {
        // Register User
        var userInfo = await RegisterUserAsync_ReturnSuccess();

        // Generate Access Token
        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);
        // Get User by name
        var getUserByUserName_url = BaseUrl + ApiRoutePathConstants.GetUserByName;
        var getUserByUserName_response = await FrontChannelClient
            .PostAsync(
                getUserByUserName_url,
                new StringContent(JsonConvert.SerializeObject(userInfo.UserName), Encoding.UTF8, "application/json"));
        var userDetails = await getUserByUserName_response.Content.ReadAsStringAsync();
        var getUserDetailsByUserNameResult = JsonConvert.DeserializeObject<UserModel>(userDetails);
        getUserDetailsByUserNameResult.Should().NotBeNull();

        var userClaimModel = UserHelper.CreateUserClaimModel_List(getUserDetailsByUserNameResult.Id);

        userClaimModel[0].ClaimType = userClaimModel[0].ClaimType + string.Empty + random.Next();
        userClaimModel[1].ClaimType = userClaimModel[1].ClaimType + string.Empty + random.Next();
        userClaimModel[2].ClaimType = userClaimModel[2].ClaimType + string.Empty + random.Next();

        userClaimModel[0].ClaimValue = userClaimModel[0].ClaimValue + string.Empty + random.Next();
        userClaimModel[1].ClaimValue = userClaimModel[1].ClaimValue + string.Empty + random.Next();
        userClaimModel[2].ClaimValue = userClaimModel[2].ClaimValue + string.Empty + random.Next();

        var addClaimUrl = BaseUrl + ApiRoutePathConstants.AddClaimList;
        var addClaimResponse = await FrontChannelClient.PostAsync(
            addClaimUrl,
            new StringContent(JsonConvert.SerializeObject(userClaimModel), Encoding.UTF8, "application/json"));
        var claimResopnse = await addClaimResponse.Content.ReadAsStringAsync();
        var claim_Result = JsonConvert.DeserializeObject<FrameworkResult>(claimResopnse);

        claim_Result.Should().BeOfType<FrameworkResult>();
        claim_Result.Status.Should().Be(ResultStatus.Success);

        // Remove Claim
        var removeClaimUrl = BaseUrl + ApiRoutePathConstants.RemoveClaimList;
        var removeClaimResponse = await FrontChannelClient.PostAsync(
            removeClaimUrl,
            new StringContent(JsonConvert.SerializeObject(userClaimModel), Encoding.UTF8, "application/json"));
        var removeClaimResopnse = await removeClaimResponse.Content.ReadAsStringAsync();
        var removeClaim_Result = JsonConvert.DeserializeObject<FrameworkResult>(removeClaimResopnse);
        removeClaim_Result.Should().BeOfType<FrameworkResult>();
        removeClaim_Result.Status.Should().Be(ResultStatus.Success);
    }

    [Fact]
    [Trait("Category", "SuccessCase")]
    public async Task ReplaceClaimAsync_Success()
    {
        // Register User
        var userInfo = await RegisterUserAsync_ReturnSuccess();

        // Generate Access Token
        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);
        // Get User by name
        var getUserByUserName_url = BaseUrl + ApiRoutePathConstants.GetUserByName;
        var getUserByUserName_response = await FrontChannelClient
            .PostAsync(
                getUserByUserName_url,
                new StringContent(JsonConvert.SerializeObject(userInfo.UserName), Encoding.UTF8, "application/json"));
        var userDetails = await getUserByUserName_response.Content.ReadAsStringAsync();
        var getUserDetailsByUserNameResult = JsonConvert.DeserializeObject<UserModel>(userDetails);
        getUserDetailsByUserNameResult.Should().NotBeNull();

        var userClaimModel = UserHelper.CreateUserClaimModel();
        var userid = getUserDetailsByUserNameResult.Id;
        userClaimModel.ClaimType = userClaimModel.ClaimType + string.Empty + random.Next();
        userClaimModel.UserId = userid;

        var addClaimUrl = BaseUrl + ApiRoutePathConstants.AddClaim;
        var addClaimResponse = await FrontChannelClient.PostAsync(
            addClaimUrl,
            new StringContent(JsonConvert.SerializeObject(userClaimModel), Encoding.UTF8, "application/json"));
        var claimResopnse = await addClaimResponse.Content.ReadAsStringAsync();
        var claim_Result = JsonConvert.DeserializeObject<FrameworkResult>(claimResopnse);

        claim_Result.Should().BeOfType<FrameworkResult>();
        claim_Result.Status.Should().Be(ResultStatus.Success);

        var userClaimModel2 = UserHelper.CreateUserClaimModel();
        userClaimModel2.ClaimType = userClaimModel.ClaimType + string.Empty + random.Next();
        userClaimModel2.UserId = userid;

        var claim_data = new
        {
            existingUserClaimModel = userClaimModel,
            newUserClaimModel = userClaimModel2
        };

        // Replace Claim
        var removeClaimUrl = BaseUrl + ApiRoutePathConstants.ReplaceClaim;
        var removeClaimResponse = await FrontChannelClient.PostAsync(
            removeClaimUrl,
            new StringContent(JsonConvert.SerializeObject(claim_data), Encoding.UTF8, "application/json"));
        var removeClaimResopnse = await removeClaimResponse.Content.ReadAsStringAsync();
        var removeClaim_Result = JsonConvert.DeserializeObject<FrameworkResult>(removeClaimResopnse);
        removeClaim_Result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "SuccessCase")]
    public async Task GetClaimAsync_Success()
    {
        // Register User
        var userInfo = await RegisterUserAsync_ReturnSuccess();

        // Generate Access Token
        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);
        // Get User by name
        var getUserByUserName_url = BaseUrl + ApiRoutePathConstants.GetUserByName;
        var getUserByUserName_response = await FrontChannelClient
            .PostAsync(
                getUserByUserName_url,
                new StringContent(JsonConvert.SerializeObject(userInfo.UserName), Encoding.UTF8, "application/json"));
        var userDetails = await getUserByUserName_response.Content.ReadAsStringAsync();
        var getUserDetailsByUserNameResult = JsonConvert.DeserializeObject<UserModel>(userDetails);
        getUserDetailsByUserNameResult.Should().NotBeNull();

        var userClaimModel = UserHelper.CreateUserClaimModel();
        var userid = getUserDetailsByUserNameResult.Id;
        userClaimModel.ClaimType = userClaimModel.ClaimType + string.Empty + random.Next();
        userClaimModel.UserId = userid;

        var addClaimUrl = BaseUrl + ApiRoutePathConstants.AddClaim;
        var addClaimResponse = await FrontChannelClient.PostAsync(
            addClaimUrl,
            new StringContent(JsonConvert.SerializeObject(userClaimModel), Encoding.UTF8, "application/json"));
        var claimResopnse = await addClaimResponse.Content.ReadAsStringAsync();
        var claim_Result = JsonConvert.DeserializeObject<FrameworkResult>(claimResopnse);

        claim_Result.Should().BeOfType<FrameworkResult>();
        claim_Result.Status.Should().Be(ResultStatus.Success);

        // Get Claim
        var getClaimUrl = BaseUrl + ApiRoutePathConstants.GetClaims;
        var getClaimResponse = await FrontChannelClient.PostAsync(
            getClaimUrl,
            new StringContent(JsonConvert.SerializeObject(userid), Encoding.UTF8, "application/json"));
        var getClaimResopnse = await getClaimResponse.Content.ReadAsStringAsync();
        var getClaim_Result = JsonConvert.DeserializeObject(getClaimResopnse);
        getClaim_Result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "ErrorCase")]
    public async Task GetClaimAsync_InvalidUserId_ReturnFail()
    {
        var userId = Guid.NewGuid();
        // Generate Access Token
        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);

        // Get Claim
        var getClaimUrl = BaseUrl + ApiRoutePathConstants.GetClaims;
        var getClaimResponse = await FrontChannelClient.PostAsync(
            getClaimUrl,
            new StringContent(JsonConvert.SerializeObject(userId), Encoding.UTF8, "application/json"));
        var getClaimResopnse = await getClaimResponse.Content.ReadAsStringAsync();
        var getClaim_Result = JsonConvert.DeserializeObject(getClaimResopnse);
        getClaim_Result.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "SuccessCase")]
    public async Task GetUserClaimsClaimAsync_Success()
    {
        // Register User
        var userInfo = await RegisterUserAsync_ReturnSuccess();

        // Generate Access Token
        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);
        // Get User by name
        var getUserByUserName_url = BaseUrl + ApiRoutePathConstants.GetUserByName;
        var getUserByUserName_response = await FrontChannelClient
            .PostAsync(
                getUserByUserName_url,
                new StringContent(JsonConvert.SerializeObject(userInfo.UserName), Encoding.UTF8, "application/json"));
        var userDetails = await getUserByUserName_response.Content.ReadAsStringAsync();
        var getUserDetailsByUserNameResult = JsonConvert.DeserializeObject<UserModel>(userDetails);
        getUserDetailsByUserNameResult.Should().NotBeNull();

        var userClaimModel = UserHelper.CreateUserClaimModel();
        var userid = getUserDetailsByUserNameResult.Id;
        userClaimModel.ClaimType = userClaimModel.ClaimType + string.Empty + random.Next();
        userClaimModel.UserId = userid;

        var addClaimUrl = BaseUrl + ApiRoutePathConstants.AddClaim;
        var addClaimResponse = await FrontChannelClient.PostAsync(
            addClaimUrl,
            new StringContent(JsonConvert.SerializeObject(userClaimModel), Encoding.UTF8, "application/json"));
        var claimResopnse = await addClaimResponse.Content.ReadAsStringAsync();
        var claim_Result = JsonConvert.DeserializeObject<FrameworkResult>(claimResopnse);

        claim_Result.Should().BeOfType<FrameworkResult>();
        claim_Result.Status.Should().Be(ResultStatus.Success);

        // Get Claim
        var getClaimUrl = BaseUrl + ApiRoutePathConstants.GetUserClaims;
        var getClaimResponse = await FrontChannelClient.PostAsync(
            getClaimUrl,
            new StringContent(JsonConvert.SerializeObject(userid), Encoding.UTF8, "application/json"));
        var getClaimResopnse = await getClaimResponse.Content.ReadAsStringAsync();
        var getClaim_Result = JsonConvert.DeserializeObject<List<UserClaimModel>>(getClaimResopnse);
        getClaim_Result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "SuccessCase")]
    public async Task AddAdminClaimListAsync_Success()
    {
        // Register User
        var userInfo = await RegisterUserAsync_ReturnSuccess();

        // Generate Access Token
        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);
        // Get User by name
        var getUserByUserName_url = BaseUrl + ApiRoutePathConstants.GetUserByName;
        var getUserByUserName_response = await FrontChannelClient
            .PostAsync(
                getUserByUserName_url,
                new StringContent(JsonConvert.SerializeObject(userInfo.UserName), Encoding.UTF8, "application/json"));
        var userDetails = await getUserByUserName_response.Content.ReadAsStringAsync();
        var getUserDetailsByUserNameResult = JsonConvert.DeserializeObject<UserModel>(userDetails);
        getUserDetailsByUserNameResult.Should().NotBeNull();

        var userClaimModel = UserHelper.CreateUserClaimModel_List(getUserDetailsByUserNameResult.Id);

        userClaimModel[0].ClaimType = userClaimModel[0].ClaimType + string.Empty + random.Next();
        userClaimModel[1].ClaimType = userClaimModel[1].ClaimType + string.Empty + random.Next();
        userClaimModel[2].ClaimType = userClaimModel[2].ClaimType + string.Empty + random.Next();

        userClaimModel[0].ClaimValue = userClaimModel[0].ClaimValue + string.Empty + random.Next();
        userClaimModel[1].ClaimValue = userClaimModel[1].ClaimValue + string.Empty + random.Next();
        userClaimModel[2].ClaimValue = userClaimModel[2].ClaimValue + string.Empty + random.Next();

        var addClaimUrl = BaseUrl + ApiRoutePathConstants.AddAdminClaimList;
        var addClaimResponse = await FrontChannelClient.PostAsync(
            addClaimUrl,
            new StringContent(JsonConvert.SerializeObject(userClaimModel), Encoding.UTF8, "application/json"));
        var claimResopnse = await addClaimResponse.Content.ReadAsStringAsync();
        var claim_Result = JsonConvert.DeserializeObject<FrameworkResult>(claimResopnse);

        claim_Result.Should().BeOfType<FrameworkResult>();
        claim_Result.Status.Should().Be(ResultStatus.Success);
    }

    [Fact]
    [Trait("Category", "SuccessCase")]
    public async Task AddAdminClaimAsync_Success()
    {
        // Register User
        var userInfo = await RegisterUserAsync_ReturnSuccess();

        // Generate Access Token
        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);
        // Get User by name
        var getUserByUserName_url = BaseUrl + ApiRoutePathConstants.GetUserByName;
        var getUserByUserName_response = await FrontChannelClient
            .PostAsync(
                getUserByUserName_url,
                new StringContent(JsonConvert.SerializeObject(userInfo.UserName), Encoding.UTF8, "application/json"));
        var userDetails = await getUserByUserName_response.Content.ReadAsStringAsync();
        var getUserDetailsByUserNameResult = JsonConvert.DeserializeObject<UserModel>(userDetails);
        getUserDetailsByUserNameResult.Should().NotBeNull();

        var userClaimModel = UserHelper.CreateUserClaimModel();
        var userid = getUserDetailsByUserNameResult.Id;
        userClaimModel.ClaimType = userClaimModel.ClaimType + string.Empty + random.Next();
        userClaimModel.UserId = userid;

        var addClaimUrl = BaseUrl + ApiRoutePathConstants.AddAdminClaim;
        var addClaimResponse = await FrontChannelClient.PostAsync(
            addClaimUrl,
            new StringContent(JsonConvert.SerializeObject(userClaimModel), Encoding.UTF8, "application/json"));
        var claimResopnse = await addClaimResponse.Content.ReadAsStringAsync();
        var claim_Result = JsonConvert.DeserializeObject<FrameworkResult>(claimResopnse);

        claim_Result.Should().BeOfType<FrameworkResult>();
        claim_Result.Status.Should().Be(ResultStatus.Success);
    }

    [Fact]
    [Trait("Category", "SuccessCase")]
    public async Task RemoveAdminClaimAsync_Success()
    {
        // Register User
        var userInfo = await RegisterUserAsync_ReturnSuccess();

        // Generate Access Token
        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);
        // Get User by name
        var getUserByUserName_url = BaseUrl + ApiRoutePathConstants.GetUserByName;
        var getUserByUserName_response = await FrontChannelClient
            .PostAsync(
                getUserByUserName_url,
                new StringContent(JsonConvert.SerializeObject(userInfo.UserName), Encoding.UTF8, "application/json"));
        var userDetails = await getUserByUserName_response.Content.ReadAsStringAsync();
        var getUserDetailsByUserNameResult = JsonConvert.DeserializeObject<UserModel>(userDetails);
        getUserDetailsByUserNameResult.Should().NotBeNull();

        var userClaimModel = UserHelper.CreateUserClaimModel();
        var userid = getUserDetailsByUserNameResult.Id;
        userClaimModel.ClaimType = userClaimModel.ClaimType + string.Empty + random.Next();
        userClaimModel.IsAdminClaim = true;
        userClaimModel.UserId = userid;

        var addClaimUrl = BaseUrl + ApiRoutePathConstants.AddAdminClaim;
        var addClaimResponse = await FrontChannelClient.PostAsync(
            addClaimUrl,
            new StringContent(JsonConvert.SerializeObject(userClaimModel), Encoding.UTF8, "application/json"));
        var claimResopnse = await addClaimResponse.Content.ReadAsStringAsync();
        var claim_Result = JsonConvert.DeserializeObject<FrameworkResult>(claimResopnse);

        claim_Result.Should().BeOfType<FrameworkResult>();
        claim_Result.Status.Should().Be(ResultStatus.Success);

        // Remove Claim
        var removeClaimUrl = BaseUrl + ApiRoutePathConstants.RemoveAdminClaim;
        var removeClaimResponse = await FrontChannelClient.PostAsync(
            removeClaimUrl,
            new StringContent(JsonConvert.SerializeObject(userClaimModel), Encoding.UTF8, "application/json"));
        var removeClaimResopnse = await removeClaimResponse.Content.ReadAsStringAsync();
        var removeClaim_Result = JsonConvert.DeserializeObject<FrameworkResult>(removeClaimResopnse);
        removeClaim_Result.Should().BeOfType<FrameworkResult>();
        removeClaim_Result.Status.Should().Be(ResultStatus.Success);
    }

    [Fact]
    [Trait("Category", "ErrorCase")]
    public async Task RemoveAdminClaimAsync_InvalidUserId_ReteunFail()
    {
        // Generate Access Token
        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);

        var userClaimModel = UserHelper.CreateUserClaimModel();
        var userid = Guid.NewGuid();
        userClaimModel.ClaimType = userClaimModel.ClaimType + string.Empty + random.Next();
        userClaimModel.IsAdminClaim = true;
        userClaimModel.UserId = userid;

        // Remove Claim
        var removeClaimUrl = BaseUrl + ApiRoutePathConstants.RemoveAdminClaim;
        var removeClaimResponse = await FrontChannelClient.PostAsync(
            removeClaimUrl,
            new StringContent(JsonConvert.SerializeObject(userClaimModel), Encoding.UTF8, "application/json"));
        var removeClaimResopnse = await removeClaimResponse.Content.ReadAsStringAsync();
        var removeClaim_Result = JsonConvert.DeserializeObject<FrameworkResult>(removeClaimResopnse);
        removeClaim_Result.Should().BeOfType<FrameworkResult>();
        removeClaim_Result.Status.Should().Be(ResultStatus.Failed);
        removeClaim_Result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.InvalidUserClaims);
    }

    [Fact]
    [Trait("Category", "SuccessCase")]
    public async Task RemoveAdminCLaimListAsync_Success()
    {
        // Register User
        var userInfo = await RegisterUserAsync_ReturnSuccess();

        // Generate Access Token
        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);
        // Get User by name
        var getUserByUserName_url = BaseUrl + ApiRoutePathConstants.GetUserByName;
        var getUserByUserName_response = await FrontChannelClient
            .PostAsync(
                getUserByUserName_url,
                new StringContent(JsonConvert.SerializeObject(userInfo.UserName), Encoding.UTF8, "application/json"));
        var userDetails = await getUserByUserName_response.Content.ReadAsStringAsync();
        var getUserDetailsByUserNameResult = JsonConvert.DeserializeObject<UserModel>(userDetails);
        getUserDetailsByUserNameResult.Should().NotBeNull();

        var userClaimModel = UserHelper.CreateUserClaimModel_List(getUserDetailsByUserNameResult.Id);

        userClaimModel[0].ClaimType = userClaimModel[0].ClaimType + string.Empty + random.Next();
        userClaimModel[1].ClaimType = userClaimModel[1].ClaimType + string.Empty + random.Next();
        userClaimModel[2].ClaimType = userClaimModel[2].ClaimType + string.Empty + random.Next();

        userClaimModel[0].ClaimValue = userClaimModel[0].ClaimValue + string.Empty + random.Next();
        userClaimModel[1].ClaimValue = userClaimModel[1].ClaimValue + string.Empty + random.Next();
        userClaimModel[2].ClaimValue = userClaimModel[2].ClaimValue + string.Empty + random.Next();

        userClaimModel[0].IsAdminClaim = true;
        userClaimModel[1].IsAdminClaim = true;
        userClaimModel[2].IsAdminClaim = true;
        var addClaimUrl = BaseUrl + ApiRoutePathConstants.AddAdminClaimList;
        var addClaimResponse = await FrontChannelClient.PostAsync(
            addClaimUrl,
            new StringContent(JsonConvert.SerializeObject(userClaimModel), Encoding.UTF8, "application/json"));
        var claimResopnse = await addClaimResponse.Content.ReadAsStringAsync();
        var claim_Result = JsonConvert.DeserializeObject<FrameworkResult>(claimResopnse);

        claim_Result.Should().BeOfType<FrameworkResult>();
        claim_Result.Status.Should().Be(ResultStatus.Success);

        // Get User by name
        var getadminclaim_url = BaseUrl + ApiRoutePathConstants.GetAdminUserClaims;
        var getadminclaim_response = await FrontChannelClient
            .PostAsync(
                getadminclaim_url,
                new StringContent(JsonConvert.SerializeObject(getUserDetailsByUserNameResult.Id), Encoding.UTF8,
                    "application/json"));
        var getclaimDetails = await getadminclaim_response.Content.ReadAsStringAsync();
        var geclaimResult = JsonConvert.DeserializeObject<List<UserClaimModel>>(getclaimDetails);
        geclaimResult.Should().NotBeNull();
        // Remove Claim
        var removeClaimUrl = BaseUrl + ApiRoutePathConstants.RemoveAdminClaim;
        var removeClaimResponse = await FrontChannelClient.PostAsync(
            removeClaimUrl,
            new StringContent(JsonConvert.SerializeObject(geclaimResult[1]), Encoding.UTF8, "application/json"));
        var removeClaimResopnse = await removeClaimResponse.Content.ReadAsStringAsync();
        var removeClaim_Result = JsonConvert.DeserializeObject<FrameworkResult>(removeClaimResopnse);
        removeClaim_Result.Should().BeOfType<FrameworkResult>();
        removeClaim_Result.Status.Should().Be(ResultStatus.Success);
    }

    [Fact]
    [Trait("Category", "SuccessCase")]
    public async Task AddUserRoleAsync_ModelInput_Success()
    {
        // Register User
        var userInfo = await RegisterUserAsync_ReturnSuccess();
        // Get User by Name
        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);

        var username = userInfo.UserName;
        var getUserByUserName_url = BaseUrl + ApiRoutePathConstants.GetUserByName;
        var getUserByUserName_response = await FrontChannelClient
            .PostAsync(
                getUserByUserName_url,
                new StringContent(JsonConvert.SerializeObject(username), Encoding.UTF8, "application/json"));
        var userDetails = await getUserByUserName_response.Content.ReadAsStringAsync();
        var getUserDetailsByUserNameResult = JsonConvert.DeserializeObject<UserModel>(userDetails);
        getUserDetailsByUserNameResult.Should().NotBeNull();

        // Add Role
        await CreateRole_Success();

        // Get Role by name
        var getRoleUrl = BaseUrl + ApiRoutePathConstants.GetRoleByName;
        var getRoleResponse = await FrontChannelClient
            .PostAsync(
                getRoleUrl,
                new StringContent(JsonConvert.SerializeObject(roleName), Encoding.UTF8, "application/json"));
        var roleDetails = await getRoleResponse.Content.ReadAsStringAsync();
        var getRoleResult = JsonConvert.DeserializeObject<RoleModel>(roleDetails);
        getRoleResult.Should().NotBeNull();

        // Arrange for UserRoleModel Input
        var userRoleModelInput = RoleHelper.CreateUserRoleModel();
        userRoleModelInput.UserId = getUserDetailsByUserNameResult.Id;
        userRoleModelInput.RoleId = getRoleResult.Id;
        userRoleModelInput.CreatedBy = "Rosh";

        // Act : AddUserRoleAsync
        var registerUser_Url = BaseUrl + ApiRoutePathConstants.AddUserRole;
        var registerUser_response = await FrontChannelClient
            .PostAsync(
                registerUser_Url,
                new StringContent(JsonConvert.SerializeObject(userRoleModelInput), Encoding.UTF8, "application/json"));
        var userRoleDetails = await registerUser_response.Content.ReadAsStringAsync();
        var registerUser_result = JsonConvert.DeserializeObject<FrameworkResult>(userRoleDetails);
        registerUser_result.Should().BeOfType<FrameworkResult>();
        registerUser_result.Status.Should().Be(ResultStatus.Success);

        userRoleUserId = getUserDetailsByUserNameResult.Id.ToString();
        userRoleRoleName = roleName;
    }

    [Fact]
    [Trait("Category", "ErrorCase")]
    public async Task AddUserRoleAsync_InvalidUserId_ReturnFail()
    {
        // Add Role
        await CreateRole_Success();

        // Get Role by name
        var getRoleUrl = BaseUrl + ApiRoutePathConstants.GetRoleByName;
        var getRoleResponse = await FrontChannelClient
            .PostAsync(
                getRoleUrl,
                new StringContent(JsonConvert.SerializeObject(roleName), Encoding.UTF8, "application/json"));
        var roleDetails = await getRoleResponse.Content.ReadAsStringAsync();
        var getRoleResult = JsonConvert.DeserializeObject<RoleModel>(roleDetails);
        getRoleResult.Should().NotBeNull();

        // Arrange for UserRoleModel Input
        var userRoleModelInput = RoleHelper.CreateUserRoleModel();
        userRoleModelInput.UserId = Guid.NewGuid();
        userRoleModelInput.RoleId = getRoleResult.Id;
        userRoleModelInput.CreatedBy = "Rosh";

        // Act : AddUserRoleAsync
        var registerUser_Url = BaseUrl + ApiRoutePathConstants.AddUserRole;
        var registerUser_response = await FrontChannelClient
            .PostAsync(
                registerUser_Url,
                new StringContent(JsonConvert.SerializeObject(userRoleModelInput), Encoding.UTF8, "application/json"));
        var userRoleDetails = await registerUser_response.Content.ReadAsStringAsync();
        var registerUser_result = JsonConvert.DeserializeObject<FrameworkResult>(userRoleDetails);
        registerUser_result.Should().BeOfType<FrameworkResult>();
        registerUser_result.Status.Should().Be(ResultStatus.Failed);
        registerUser_result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.InvalidUserId);
    }

    [Fact]
    [Trait("Category", "ErrorCase")]
    public async Task AddUserRoleAsync_InvalidRoleId_ReturnFail()
    {
        // Register User
        var userInfo = await RegisterUserAsync_ReturnSuccess();
        // Get User by Name

        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);


        var username = userInfo.UserName;
        var getUserByUserName_url = BaseUrl + ApiRoutePathConstants.GetUserByName;
        var getUserByUserName_response = await FrontChannelClient
            .PostAsync(
                getUserByUserName_url,
                new StringContent(JsonConvert.SerializeObject(username), Encoding.UTF8, "application/json"));
        var userDetails = await getUserByUserName_response.Content.ReadAsStringAsync();
        var getUserDetailsByUserNameResult = JsonConvert.DeserializeObject<UserModel>(userDetails);
        getUserDetailsByUserNameResult.Should().NotBeNull();

        // Add Role
        await CreateRole_Success();

        // Get Role by name
        var getRoleUrl = BaseUrl + ApiRoutePathConstants.GetRoleByName;
        var getRoleResponse = await FrontChannelClient
            .PostAsync(
                getRoleUrl,
                new StringContent(JsonConvert.SerializeObject(roleName), Encoding.UTF8, "application/json"));
        var roleDetails = await getRoleResponse.Content.ReadAsStringAsync();
        var getRoleResult = JsonConvert.DeserializeObject<RoleModel>(roleDetails);
        getRoleResult.Should().NotBeNull();

        // Arrange for UserRoleModel Input
        var userRoleModelInput = RoleHelper.CreateUserRoleModel();
        userRoleModelInput.UserId = getUserDetailsByUserNameResult.Id;
        userRoleModelInput.RoleId = Guid.NewGuid();
        userRoleModelInput.CreatedBy = "Rosh";

        // Act : AddUserRoleAsync
        var registerUser_Url = BaseUrl + ApiRoutePathConstants.AddUserRole;
        var registerUser_response = await FrontChannelClient
            .PostAsync(
                registerUser_Url,
                new StringContent(JsonConvert.SerializeObject(userRoleModelInput), Encoding.UTF8, "application/json"));
        var userRoleDetails = await registerUser_response.Content.ReadAsStringAsync();
        var registerUser_result = JsonConvert.DeserializeObject<FrameworkResult>(userRoleDetails);
        registerUser_result.Should().BeOfType<FrameworkResult>();
        registerUser_result.Status.Should().Be(ResultStatus.Failed);
        registerUser_result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.InvalidRoleId);
    }

    [Fact]
    public async Task GetUserRolesByUser()
    {
        await AddUserRoleAsync_ModelInput_Success();

        // Get Role by name
        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);

        var getRoleUrl = BaseUrl + ApiRoutePathConstants.GetUserRoles;
        var getRoleResponse = await FrontChannelClient
            .PostAsync(
                getRoleUrl,
                new StringContent(JsonConvert.SerializeObject(userRoleUserId), Encoding.UTF8, "application/json"));
        var roleDetails = await getRoleResponse.Content.ReadAsStringAsync();
        var getRoleResult = JsonConvert.DeserializeObject(roleDetails);
        getRoleResult.Should().NotBeNull();
    }

    [Fact]
    public async Task GetUserRolesByInvalidUser_ReturnNullResult()
    {
        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);

        var userId = Guid.NewGuid();
        // Get Role by name
        var getRoleUrl = BaseUrl + ApiRoutePathConstants.GetUserRoles;
        var getRoleResponse = await FrontChannelClient
            .PostAsync(
                getRoleUrl,
                new StringContent(JsonConvert.SerializeObject(userId), Encoding.UTF8, "application/json"));
        var roleDetails = await getRoleResponse.Content.ReadAsStringAsync();
        var getRoleResult = JsonConvert.DeserializeObject(roleDetails);
        getRoleResult.Should().Be("User Identifier is invalid.");
    }

    [Fact]
    public async Task GetUsersInvalidROleName_ReturnNull()
    {
        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);

        // Get Role by name
        var getRoleUrl = BaseUrl + ApiRoutePathConstants.GetUsersInRole;
        var getRoleResponse = await FrontChannelClient
            .PostAsync(
                getRoleUrl,
                new StringContent(JsonConvert.SerializeObject(userRoleRoleName + "12"), Encoding.UTF8,
                    "application/json"));
        var roleDetails = await getRoleResponse.Content.ReadAsStringAsync();
        var getRoleResult = JsonConvert.DeserializeObject(roleDetails);
        getRoleResult.Should().BeNull();
    }

    [Fact]
    public async Task GetUsersInRole_InvalidRoleName()
    {
        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);

        var roleName = "HCLCSSFUserS";
        // Get Role by name
        var getRoleUrl = BaseUrl + ApiRoutePathConstants.GetUsersInRole;
        var getRoleResponse = await FrontChannelClient
            .PostAsync(
                getRoleUrl,
                new StringContent(JsonConvert.SerializeObject(roleName), Encoding.UTF8, "application/json"));
        var roleDetails = await getRoleResponse.Content.ReadAsStringAsync();
        var getRoleResult = JsonConvert.DeserializeObject<UserModel>(roleDetails);
        getRoleResult.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "SuccessCase")]
    public async Task RemoveUserRoleAsync_ModelInput_Success()
    {
        // Register User
        var userInfo = await RegisterUserAsync_ReturnSuccess();
        // Get User by Name

        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);

        var username = userInfo.UserName;
        var getUserByUserName_url = BaseUrl + ApiRoutePathConstants.GetUserByName;
        var getUserByUserName_response = await FrontChannelClient
            .PostAsync(
                getUserByUserName_url,
                new StringContent(JsonConvert.SerializeObject(username), Encoding.UTF8, "application/json"));
        var userDetails = await getUserByUserName_response.Content.ReadAsStringAsync();
        var getUserDetailsByUserNameResult = JsonConvert.DeserializeObject<UserModel>(userDetails);
        getUserDetailsByUserNameResult.Should().NotBeNull();

        // Add Role
        await CreateRole_Success();

        // Get Role by name
        var getRoleUrl = BaseUrl + ApiRoutePathConstants.GetRoleByName;
        var getRoleResponse = await FrontChannelClient
            .PostAsync(
                getRoleUrl,
                new StringContent(JsonConvert.SerializeObject(roleName), Encoding.UTF8, "application/json"));
        var roleDetails = await getRoleResponse.Content.ReadAsStringAsync();
        var getRoleResult = JsonConvert.DeserializeObject<RoleModel>(roleDetails);
        getRoleResult.Should().NotBeNull();

        // Arrange for UserRoleModel Input
        var userRoleModelInput = RoleHelper.CreateUserRoleModel();
        userRoleModelInput.UserId = getUserDetailsByUserNameResult.Id;
        userRoleModelInput.RoleId = getRoleResult.Id;
        userRoleModelInput.CreatedBy = "Rosh";

        // Act : AddUserRoleAsync
        var registerUser_Url = BaseUrl + ApiRoutePathConstants.AddUserRole;
        var registerUser_response = await FrontChannelClient
            .PostAsync(
                registerUser_Url,
                new StringContent(JsonConvert.SerializeObject(userRoleModelInput), Encoding.UTF8, "application/json"));
        var userRoleDetails = await registerUser_response.Content.ReadAsStringAsync();
        var registerUser_result = JsonConvert.DeserializeObject<FrameworkResult>(userRoleDetails);
        registerUser_result.Should().BeOfType<FrameworkResult>();
        registerUser_result.Status.Should().Be(ResultStatus.Success);

        var removeUserRole_Url = BaseUrl + ApiRoutePathConstants.RemoveUserRole;
        var removeUserRole_response = await FrontChannelClient
            .PostAsync(
                removeUserRole_Url,
                new StringContent(JsonConvert.SerializeObject(userRoleModelInput), Encoding.UTF8, "application/json"));
        var removeUserRoleDetails = await removeUserRole_response.Content.ReadAsStringAsync();
        var removeUserRole_Result = JsonConvert.DeserializeObject<FrameworkResult>(removeUserRoleDetails);
        removeUserRole_Result.Should().BeOfType<FrameworkResult>();
        removeUserRole_Result.Status.Should().Be(ResultStatus.Success);
    }

    [Fact]
    [Trait("Category", "ErrorCase")]
    public async Task RemoveUserRoleAsync_InvalidUserId_ReturnFail()
    {
        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);

        // Arrange for UserRoleModel Input
        var userRoleModelInput = RoleHelper.CreateUserRoleModel();
        userRoleModelInput.UserId = Guid.NewGuid();
        userRoleModelInput.RoleId = Guid.NewGuid();
        userRoleModelInput.CreatedBy = "Rosh";

        var removeUserRole_Url = BaseUrl + ApiRoutePathConstants.RemoveUserRole;
        var removeUserRole_response = await FrontChannelClient
            .PostAsync(
                removeUserRole_Url,
                new StringContent(JsonConvert.SerializeObject(userRoleModelInput), Encoding.UTF8, "application/json"));
        var removeUserRoleDetails = await removeUserRole_response.Content.ReadAsStringAsync();
        var removeUserRole_Result = JsonConvert.DeserializeObject<FrameworkResult>(removeUserRoleDetails);
        removeUserRole_Result.Should().BeOfType<FrameworkResult>();
        removeUserRole_Result.Status.Should().Be(ResultStatus.Failed);
        removeUserRole_Result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.UserRoleNotExists);
    }

    [Fact]
    [Trait("Category", "ErrorCase")]
    public async Task RemoveUserRoleAsync_InvalidRoleId_ReturnFail()
    {
        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);

        // Arrange for UserRoleModel Input
        var userRoleModelInput = RoleHelper.CreateUserRoleModel();
        userRoleModelInput.UserId = Guid.NewGuid();
        userRoleModelInput.RoleId = Guid.NewGuid();
        userRoleModelInput.CreatedBy = "Rosh";

        var removeUserRole_Url = BaseUrl + ApiRoutePathConstants.RemoveUserRole;
        var removeUserRole_response = await FrontChannelClient
            .PostAsync(
                removeUserRole_Url,
                new StringContent(JsonConvert.SerializeObject(userRoleModelInput), Encoding.UTF8, "application/json"));
        var removeUserRoleDetails = await removeUserRole_response.Content.ReadAsStringAsync();
        var removeUserRole_Result = JsonConvert.DeserializeObject<FrameworkResult>(removeUserRoleDetails);
        removeUserRole_Result.Should().BeOfType<FrameworkResult>();
        removeUserRole_Result.Status.Should().Be(ResultStatus.Failed);
        removeUserRole_Result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.UserRoleNotExists);
    }

    [Fact]
    [Trait("Category", "SuccessCase")]
    public async Task AddSecurityQuestionAsync_Success()
    {
        // Arrange
        var securityQuestion = UserHelper.CreateSecurityQuestionModel();
        securityQuestion.CreatedOn = DateTime.UtcNow;
        securityQuestion.CreatedBy = "Suresh";
        securityQuestion.Question = "What is your age ?" + string.Empty + random.Next() + string.Empty;

        // Generate Access Token
        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);

        var addsecurityQuestion_Url = BaseUrl + ApiRoutePathConstants.AddSecurityQuestion;
        var addsecurityQuestion_response = await FrontChannelClient
            .PostAsync(
                addsecurityQuestion_Url,
                new StringContent(JsonConvert.SerializeObject(securityQuestion), Encoding.UTF8, "application/json"));
        var addSecurityQuestionResult = await addsecurityQuestion_response.Content.ReadAsStringAsync();
        var frameworkResult = JsonConvert.DeserializeObject<FrameworkResult>(addSecurityQuestionResult);
        frameworkResult.Should().BeOfType<FrameworkResult>();
        frameworkResult.Status.Should().Be(ResultStatus.Success);
    }

    [Fact]
    [Trait("Category", "ErrorCase")]
    public async Task AddSecurityQuestionAsync_PassingNullQuestion_ReturnFail()
    {
        // Arrange
        var securityQuestion = UserHelper.CreateSecurityQuestionModel();
        securityQuestion.CreatedOn = DateTime.UtcNow;
        securityQuestion.CreatedBy = "Suresh";
        securityQuestion.Question = null;

        // Generate Access Token
        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);

        var addsecurityQuestion_Url = BaseUrl + ApiRoutePathConstants.AddSecurityQuestion;
        var addsecurityQuestion_response = await FrontChannelClient
            .PostAsync(
                addsecurityQuestion_Url,
                new StringContent(JsonConvert.SerializeObject(securityQuestion), Encoding.UTF8, "application/json"));
        var addSecurityQuestionResult = await addsecurityQuestion_response.Content.ReadAsStringAsync();
        var frameworkResult = JsonConvert.DeserializeObject<FrameworkResult>(addSecurityQuestionResult);
        frameworkResult.Should().BeOfType<FrameworkResult>();
        frameworkResult.Status.Should().Be(ResultStatus.Failed);
        frameworkResult.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.InvalidSecurityQuestion);
    }

    [Fact]
    [Trait("Category", "ErrorCase")]
    public async Task AddSecurityQuestionAsync_QuestiomnLengthExceded_ReturnFail()
    {
        // Arrange
        var securityQuestion = UserHelper.CreateSecurityQuestionModel();
        securityQuestion.CreatedOn = DateTime.UtcNow;
        securityQuestion.CreatedBy = "Suresh";
        securityQuestion.Question =
            "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";

        // Generate Access Token
        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);

        var addsecurityQuestion_Url = BaseUrl + ApiRoutePathConstants.AddSecurityQuestion;
        var addsecurityQuestion_response = await FrontChannelClient
            .PostAsync(
                addsecurityQuestion_Url,
                new StringContent(JsonConvert.SerializeObject(securityQuestion), Encoding.UTF8, "application/json"));
        var addSecurityQuestionResult = await addsecurityQuestion_response.Content.ReadAsStringAsync();
        var frameworkResult = JsonConvert.DeserializeObject<FrameworkResult>(addSecurityQuestionResult);
        frameworkResult.Should().BeOfType<FrameworkResult>();
        frameworkResult.Status.Should().Be(ResultStatus.Failed);
        frameworkResult.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.SecurityQuestionTooLong);
    }

    [Fact]
    [Trait("Category", "ErrorCase")]
    public async Task AddSecurityQuestionAsync_DuplicateQuestion_ReturnFail()
    {
        // Arrange
        var securityQuestion = UserHelper.CreateSecurityQuestionModel();
        securityQuestion.CreatedOn = DateTime.UtcNow;
        securityQuestion.CreatedBy = "Suresh";
        securityQuestion.Question = "What is your age ?" + string.Empty + random.Next() + string.Empty;

        // Generate Access Token
        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);

        var addsecurityQuestion_Url = BaseUrl + ApiRoutePathConstants.AddSecurityQuestion;
        var addsecurityQuestion_response = await FrontChannelClient
            .PostAsync(
                addsecurityQuestion_Url,
                new StringContent(JsonConvert.SerializeObject(securityQuestion), Encoding.UTF8, "application/json"));

        addsecurityQuestion_response = await FrontChannelClient
            .PostAsync(
                addsecurityQuestion_Url,
                new StringContent(JsonConvert.SerializeObject(securityQuestion), Encoding.UTF8, "application/json"));

        var addSecurityQuestionResult = await addsecurityQuestion_response.Content.ReadAsStringAsync();
        var frameworkResult = JsonConvert.DeserializeObject<FrameworkResult>(addSecurityQuestionResult);
        frameworkResult.Should().BeOfType<FrameworkResult>();
        frameworkResult.Status.Should().Be(ResultStatus.Failed);
        frameworkResult.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.SecurityQuestionAlreadyExists);
    }

    [Fact]
    [Trait("Category", "SuccessCase")]
    public async Task UpdateSecurityQuestionAsync_Success()
    {
        // Arrange
        await AddSecurityQuestionAsync_Success();

        // Generate Access Token
        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);

        var getSecurityQuestion_Url = BaseUrl + ApiRoutePathConstants.GetAllSecurityQuestions;
        var getSecurityQuestion_response = await FrontChannelClient
            .PostAsync(
                getSecurityQuestion_Url,
                new StringContent(JsonConvert.SerializeObject(string.Empty), Encoding.UTF8, "application/json"));
        var addSecurityQuestionResult = await getSecurityQuestion_response.Content.ReadAsStringAsync();
        var questionResult = JsonConvert.DeserializeObject<IList<SecurityQuestionModel>>(addSecurityQuestionResult);
        questionResult.Should().NotBeNull();

        questionResult[0].Question = "What is your age ?" + string.Empty + random.Next() + string.Empty;

        // Update Security Question
        var updateSecurityQuestion_Url = BaseUrl + ApiRoutePathConstants.UpdateSecurityQuestion;
        var updateSecurityQuestion_response = await FrontChannelClient
            .PostAsync(
                updateSecurityQuestion_Url,
                new StringContent(JsonConvert.SerializeObject(questionResult[0]), Encoding.UTF8, "application/json"));
        var updateSecurityQuestionResult = await updateSecurityQuestion_response.Content.ReadAsStringAsync();
        var frameworkResult = JsonConvert.DeserializeObject<FrameworkResult>(updateSecurityQuestionResult);
        frameworkResult.Should().BeOfType<FrameworkResult>();
        frameworkResult.Status.Should().Be(ResultStatus.Success);
    }

    [Fact]
    [Trait("Category", "ErrorCase")]
    public async Task UpdateSecurityQuestionAsync_QuestionLengthExceeded_ReturnError()
    {
        // Arrange
        await AddSecurityQuestionAsync_Success();

        // Generate Access Token
        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);

        var getSecurityQuestion_Url = BaseUrl + ApiRoutePathConstants.GetAllSecurityQuestions;
        var getSecurityQuestion_response = await FrontChannelClient
            .PostAsync(
                getSecurityQuestion_Url,
                new StringContent(JsonConvert.SerializeObject(string.Empty), Encoding.UTF8, "application/json"));
        var addSecurityQuestionResult = await getSecurityQuestion_response.Content.ReadAsStringAsync();
        var questionResult = JsonConvert.DeserializeObject<IList<SecurityQuestionModel>>(addSecurityQuestionResult);
        questionResult.Should().NotBeNull();

        questionResult[0].Question =
            "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";

        // Update Security Question
        var updateSecurityQuestion_Url = BaseUrl + ApiRoutePathConstants.UpdateSecurityQuestion;
        var updateSecurityQuestion_response = await FrontChannelClient
            .PostAsync(
                updateSecurityQuestion_Url,
                new StringContent(JsonConvert.SerializeObject(questionResult[0]), Encoding.UTF8, "application/json"));
        var updateSecurityQuestionResult = await updateSecurityQuestion_response.Content.ReadAsStringAsync();
        var frameworkResult = JsonConvert.DeserializeObject<FrameworkResult>(updateSecurityQuestionResult);
        frameworkResult.Should().BeOfType<FrameworkResult>();
        frameworkResult.Status.Should().Be(ResultStatus.Failed);
        frameworkResult.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.SecurityQuestionTooLong);
    }

    [Fact]
    [Trait("Category", "ErrorCase")]
    public async Task UpdateSecurityQuestionAsync_CreatedByLengthExceeded_ReturnError()
    {
        // Arrange
        await AddSecurityQuestionAsync_Success();

        // Generate Access Token
        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);

        var getSecurityQuestion_Url = BaseUrl + ApiRoutePathConstants.GetAllSecurityQuestions;
        var getSecurityQuestion_response = await FrontChannelClient
            .PostAsync(
                getSecurityQuestion_Url,
                new StringContent(JsonConvert.SerializeObject(string.Empty), Encoding.UTF8, "application/json"));
        var addSecurityQuestionResult = await getSecurityQuestion_response.Content.ReadAsStringAsync();
        var questionResult = JsonConvert.DeserializeObject<IList<SecurityQuestionModel>>(addSecurityQuestionResult);
        questionResult.Should().NotBeNull();

        questionResult[0].Question = "What is your age ?" + string.Empty + random.Next() + string.Empty;
        questionResult[0].CreatedBy =
            "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";

        // Update Security Question
        var updateSecurityQuestion_Url = BaseUrl + ApiRoutePathConstants.UpdateSecurityQuestion;
        var updateSecurityQuestion_response = await FrontChannelClient
            .PostAsync(
                updateSecurityQuestion_Url,
                new StringContent(JsonConvert.SerializeObject(questionResult[0]), Encoding.UTF8, "application/json"));
        var updateSecurityQuestionResult = await updateSecurityQuestion_response.Content.ReadAsStringAsync();
        var frameworkResult = JsonConvert.DeserializeObject<FrameworkResult>(updateSecurityQuestionResult);
        frameworkResult.Should().BeOfType<FrameworkResult>();
        frameworkResult.Status.Should().Be(ResultStatus.Failed);
        frameworkResult.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.CreatedByTooLong);
    }

    [Fact]
    [Trait("Category", "SuccessCase")]
    public async Task GetAllSecurityQuestionAsync_Success()
    {
        // Arrange
        await AddSecurityQuestionAsync_Success();

        // Generate Access Token
        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);

        var getSecurityQuestion_Url = BaseUrl + ApiRoutePathConstants.GetAllSecurityQuestions;
        var getSecurityQuestion_response = await FrontChannelClient
            .PostAsync(
                getSecurityQuestion_Url,
                new StringContent(JsonConvert.SerializeObject(string.Empty), Encoding.UTF8, "application/json"));
        var addSecurityQuestionResult = await getSecurityQuestion_response.Content.ReadAsStringAsync();
        var questionResult = JsonConvert.DeserializeObject<IList<SecurityQuestionModel>>(addSecurityQuestionResult);
        questionResult.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "SuccessCase")]
    public async Task DeleteSecurityQuestionAsync_Success()
    {
        // Arrange
        var securityQuestion = UserHelper.CreateSecurityQuestionModel();
        securityQuestion.CreatedOn = DateTime.UtcNow;
        securityQuestion.CreatedBy = "Suresh";
        securityQuestion.Question = "What is your age ?" + string.Empty + random.Next() + string.Empty;

        // Generate Access Token
        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);

        var addsecurityQuestion_Url = BaseUrl + ApiRoutePathConstants.AddSecurityQuestion;
        var addsecurityQuestion_response = await FrontChannelClient
            .PostAsync(
                addsecurityQuestion_Url,
                new StringContent(JsonConvert.SerializeObject(securityQuestion), Encoding.UTF8, "application/json"));
        var addSecurityQuestionResult = await addsecurityQuestion_response.Content.ReadAsStringAsync();
        var frameworkResult = JsonConvert.DeserializeObject<FrameworkResult>(addSecurityQuestionResult);
        frameworkResult.Should().BeOfType<FrameworkResult>();
        frameworkResult.Status.Should().Be(ResultStatus.Success);

        // Delete Security Question
        var deleteSecurityQuestion_Url = BaseUrl + ApiRoutePathConstants.DeleteSecurityQuestion;
        var deleteSecurityQuestion_response = await FrontChannelClient
            .PostAsync(
                deleteSecurityQuestion_Url,
                new StringContent(JsonConvert.SerializeObject(securityQuestion.Id), Encoding.UTF8, "application/json"));
        var deleteSecurityQuestionResult = await deleteSecurityQuestion_response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<FrameworkResult>(deleteSecurityQuestionResult);
        result.Should().BeOfType<FrameworkResult>();
        result.Status.Should().Be(ResultStatus.Success);
    }

    [Fact]
    [Trait("Category", "ErrorCase")]
    public async Task DeleteSecurityQuestionAsyncInvalidQuestionId_ReturnFail()
    {
        // Arrange
        var securityQuestion = UserHelper.CreateSecurityQuestionModel();
        // Generate Access Token
        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);

        // Delete Security Question
        var deleteSecurityQuestion_Url = BaseUrl + ApiRoutePathConstants.DeleteSecurityQuestion;
        var deleteSecurityQuestion_response = await FrontChannelClient
            .PostAsync(
                deleteSecurityQuestion_Url,
                new StringContent(JsonConvert.SerializeObject(securityQuestion.Id), Encoding.UTF8, "application/json"));
        var deleteSecurityQuestionResult = await deleteSecurityQuestion_response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<FrameworkResult>(deleteSecurityQuestionResult);
        result.Should().BeOfType<FrameworkResult>();
        result.Status.Should().Be(ResultStatus.Failed);
        result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.SecurityQuestionNotExists);
    }

    [Fact]
    [Trait("Category", "SuccessCase")]
    public async Task Add_User_SecurityQuestion_Success()
    {
        var securityQuestion = UserHelper.CreateSecurityQuestionModel();
        var random = new Random();
        securityQuestion.CreatedOn = DateTime.UtcNow;
        securityQuestion.CreatedBy = "Suresh";
        securityQuestion.Question = "What is your age ?" + string.Empty + random.Next() + string.Empty;
        // Generate Access Token
        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);

        var addsecurityQuestion_Url = BaseUrl + ApiRoutePathConstants.AddSecurityQuestion;
        var addsecurityQuestion_response = await FrontChannelClient
            .PostAsync(
                addsecurityQuestion_Url,
                new StringContent(JsonConvert.SerializeObject(securityQuestion), Encoding.UTF8, "application/json"));
        var addSecurityQuestionResult = await addsecurityQuestion_response.Content.ReadAsStringAsync();
        var frameworkResult = JsonConvert.DeserializeObject<FrameworkResult>(addSecurityQuestionResult);
        frameworkResult.Should().BeOfType<FrameworkResult>();
        frameworkResult.Status.Should().Be(ResultStatus.Success);

        // With Seed Data
        var url = BaseUrl + ApiRoutePathConstants.GetUserByName;
        var response = await FrontChannelClient
            .PostAsync(url,
                new StringContent(JsonConvert.SerializeObject(userName), Encoding.UTF8, "application/json"));
        var userDetails = await response.Content.ReadAsStringAsync();
        var getUserDetailsByUserNameResult = JsonConvert.DeserializeObject<UserModel>(userDetails);
        getUserDetailsByUserNameResult.Should().NotBeNull();
        getUserDetailsByUserNameResult.UserName.Should().BeEquivalentTo(userName);

        var securityQuestion2 = UserHelper.CreateUserSecurityQuestionModel();
        securityQuestion2.SecurityQuestionId = securityQuestion.Id;
        securityQuestion2.UserId = getUserDetailsByUserNameResult.Id;
        securityQuestion2.CreatedBy = "Test";

        var addUserSecurityQuestion_Url = BaseUrl + ApiRoutePathConstants.AddUserSecurityQuestion;
        var addUserSecurityQuestion_response = await FrontChannelClient
            .PostAsync(
                addUserSecurityQuestion_Url,
                new StringContent(JsonConvert.SerializeObject(securityQuestion2), Encoding.UTF8, "application/json"));
        var addUserSecurityQuestionResult = await addUserSecurityQuestion_response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<FrameworkResult>(addUserSecurityQuestionResult);
        result.Should().BeOfType<FrameworkResult>();
        result.Status.Should().Be(ResultStatus.Success);
    }

    [Fact]
    [Trait("Category", "ErrorCase")]
    public async Task Add_User_SecurityQuestion_InvalidUserId_ReturnFail()
    {
        var securityQuestion = UserHelper.CreateSecurityQuestionModel();
        var random = new Random();
        securityQuestion.CreatedOn = DateTime.UtcNow;
        securityQuestion.CreatedBy = "Suresh";
        securityQuestion.Question = "What is your age ?" + string.Empty + random.Next() + string.Empty;
        // Generate Access Token
        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);

        var addsecurityQuestion_Url = BaseUrl + ApiRoutePathConstants.AddSecurityQuestion;
        var addsecurityQuestion_response = await FrontChannelClient
            .PostAsync(
                addsecurityQuestion_Url,
                new StringContent(JsonConvert.SerializeObject(securityQuestion), Encoding.UTF8, "application/json"));
        var addSecurityQuestionResult = await addsecurityQuestion_response.Content.ReadAsStringAsync();
        var frameworkResult = JsonConvert.DeserializeObject<FrameworkResult>(addSecurityQuestionResult);
        frameworkResult.Should().BeOfType<FrameworkResult>();
        frameworkResult.Status.Should().Be(ResultStatus.Success);

        // With Seed Data
        var url = BaseUrl + ApiRoutePathConstants.GetUserByName;
        var response = await FrontChannelClient
            .PostAsync(
                url,
                new StringContent(JsonConvert.SerializeObject(userName), Encoding.UTF8, "application/json"));
        var userDetails = await response.Content.ReadAsStringAsync();
        var getUserDetailsByUserNameResult = JsonConvert.DeserializeObject<UserModel>(userDetails);
        getUserDetailsByUserNameResult.Should().NotBeNull();
        getUserDetailsByUserNameResult.UserName.Should().BeEquivalentTo(userName);

        var securityQuestion2 = UserHelper.CreateUserSecurityQuestionModel();
        securityQuestion2.SecurityQuestionId = securityQuestion.Id;
        securityQuestion2.UserId = Guid.NewGuid();
        securityQuestion2.CreatedBy = "Test";

        var addUserSecurityQuestion_Url = BaseUrl + ApiRoutePathConstants.AddUserSecurityQuestion;
        var addUserSecurityQuestion_response = await FrontChannelClient
            .PostAsync(
                addUserSecurityQuestion_Url,
                new StringContent(JsonConvert.SerializeObject(securityQuestion2), Encoding.UTF8, "application/json"));
        var addUserSecurityQuestionResult = await addUserSecurityQuestion_response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<FrameworkResult>(addUserSecurityQuestionResult);
        result.Should().BeOfType<FrameworkResult>();
        result.Status.Should().Be(ResultStatus.Failed);
        result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.InvalidUserId);
    }

    [Fact]
    [Trait("Category", "ErrorCase")]
    public async Task Add_User_SecurityQuestion_InvalidSecurityQuestion_ReturnFail()
    {
        var securityQuestion = UserHelper.CreateSecurityQuestionModel();
        var random = new Random();
        securityQuestion.CreatedOn = DateTime.UtcNow;
        securityQuestion.CreatedBy = "Suresh";
        securityQuestion.Question = "What is your age ?" + string.Empty + random.Next() + string.Empty;
        // Generate Access Token
        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);

        var addsecurityQuestion_Url = BaseUrl + ApiRoutePathConstants.AddSecurityQuestion;
        var addsecurityQuestion_response = await FrontChannelClient
            .PostAsync(
                addsecurityQuestion_Url,
                new StringContent(JsonConvert.SerializeObject(securityQuestion), Encoding.UTF8, "application/json"));
        var addSecurityQuestionResult = await addsecurityQuestion_response.Content.ReadAsStringAsync();
        var frameworkResult = JsonConvert.DeserializeObject<FrameworkResult>(addSecurityQuestionResult);
        frameworkResult.Should().BeOfType<FrameworkResult>();
        frameworkResult.Status.Should().Be(ResultStatus.Success);

        // With Seed Data
        var url = BaseUrl + ApiRoutePathConstants.GetUserByName;
        var response = await FrontChannelClient
            .PostAsync(
                url,
                new StringContent(JsonConvert.SerializeObject(userName), Encoding.UTF8, "application/json"));
        var userDetails = await response.Content.ReadAsStringAsync();
        var getUserDetailsByUserNameResult = JsonConvert.DeserializeObject<UserModel>(userDetails);
        getUserDetailsByUserNameResult.Should().NotBeNull();
        getUserDetailsByUserNameResult.UserName.Should().BeEquivalentTo(userName);

        var securityQuestion2 = UserHelper.CreateUserSecurityQuestionModel();
        securityQuestion2.SecurityQuestionId = Guid.NewGuid();
        securityQuestion2.UserId = getUserDetailsByUserNameResult.Id;
        securityQuestion2.CreatedBy = "Test";

        var addUserSecurityQuestion_Url = BaseUrl + ApiRoutePathConstants.AddUserSecurityQuestion;
        var addUserSecurityQuestion_response = await FrontChannelClient
            .PostAsync(
                addUserSecurityQuestion_Url,
                new StringContent(JsonConvert.SerializeObject(securityQuestion2), Encoding.UTF8, "application/json"));
        var addUserSecurityQuestionResult = await addUserSecurityQuestion_response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<FrameworkResult>(addUserSecurityQuestionResult);
        result.Should().BeOfType<FrameworkResult>();
        result.Status.Should().Be(ResultStatus.Failed);
        result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.InvalidSecurityQuestionId);
    }

    [Fact]
    [Trait("Category", "SuccessCase")]
    public async Task UpdateUserSecurityQuestionAsync_Success()
    {
        await Add_User_SecurityQuestion_Success();

        // Get User
        var url = BaseUrl + ApiRoutePathConstants.GetUserByName;
        var response = await FrontChannelClient
            .PostAsync(
                url,
                new StringContent(JsonConvert.SerializeObject(userName), Encoding.UTF8, "application/json"));
        var userDetails = await response.Content.ReadAsStringAsync();
        var getUserDetailsByUserNameResult = JsonConvert.DeserializeObject<UserModel>(userDetails);
        getUserDetailsByUserNameResult.Should().NotBeNull();
        getUserDetailsByUserNameResult.UserName.Should().BeEquivalentTo(userName);

        var getSecurityQuestion_Url = BaseUrl + ApiRoutePathConstants.GetUserSecurityQuestions;
        var getSecurityQuestion_response = await FrontChannelClient
            .PostAsync(
                getSecurityQuestion_Url,
                new StringContent(JsonConvert.SerializeObject(getUserDetailsByUserNameResult.Id), Encoding.UTF8,
                    "application/json"));
        var addSecurityQuestionResult = await getSecurityQuestion_response.Content.ReadAsStringAsync();
        var questionResult = JsonConvert.DeserializeObject<IList<UserSecurityQuestionModel>>(addSecurityQuestionResult);
        questionResult.Should().NotBeNull();

        // Update
        questionResult[0].Answer = "TestProxy";

        // Update Security Question
        var updateSecurityQuestion_Url = BaseUrl + ApiRoutePathConstants.UpdateUserSecurityQuestion;
        var updateSecurityQuestion_response = await FrontChannelClient
            .PostAsync(
                updateSecurityQuestion_Url,
                new StringContent(JsonConvert.SerializeObject(questionResult[0]), Encoding.UTF8, "application/json"));
        var updateSecurityQuestionResult = await updateSecurityQuestion_response.Content.ReadAsStringAsync();
        var frameworkResult = JsonConvert.DeserializeObject<FrameworkResult>(updateSecurityQuestionResult);
        frameworkResult.Should().BeOfType<FrameworkResult>();
        frameworkResult.Status.Should().Be(ResultStatus.Success);
    }

    [Fact]
    [Trait("Category", "ErrorCase")]
    public async Task UpdateUserSecurityQuestionAsync_InvalidUserId_ReturnFail()
    {
        await Add_User_SecurityQuestion_Success();

        // Get User
        var url = BaseUrl + ApiRoutePathConstants.GetUserByName;
        var response = await FrontChannelClient
            .PostAsync(
                url,
                new StringContent(JsonConvert.SerializeObject(userName), Encoding.UTF8, "application/json"));
        var userDetails = await response.Content.ReadAsStringAsync();
        var getUserDetailsByUserNameResult = JsonConvert.DeserializeObject<UserModel>(userDetails);
        getUserDetailsByUserNameResult.Should().NotBeNull();
        getUserDetailsByUserNameResult.UserName.Should().BeEquivalentTo(userName);

        var getSecurityQuestion_Url = BaseUrl + ApiRoutePathConstants.GetUserSecurityQuestions;
        var getSecurityQuestion_response = await FrontChannelClient
            .PostAsync(
                getSecurityQuestion_Url,
                new StringContent(JsonConvert.SerializeObject(getUserDetailsByUserNameResult.Id), Encoding.UTF8,
                    "application/json"));
        var addSecurityQuestionResult = await getSecurityQuestion_response.Content.ReadAsStringAsync();
        var questionResult = JsonConvert.DeserializeObject<IList<UserSecurityQuestionModel>>(addSecurityQuestionResult);
        questionResult.Should().NotBeNull();

        // Update
        questionResult[0].Answer = "TestProxy";
        questionResult[0].UserId = Guid.NewGuid();

        // Update Security Question
        var updateSecurityQuestion_Url = BaseUrl + ApiRoutePathConstants.UpdateUserSecurityQuestion;
        var updateSecurityQuestion_response = await FrontChannelClient
            .PostAsync(
                updateSecurityQuestion_Url,
                new StringContent(JsonConvert.SerializeObject(questionResult[0]), Encoding.UTF8, "application/json"));
        var updateSecurityQuestionResult = await updateSecurityQuestion_response.Content.ReadAsStringAsync();
        var frameworkResult = JsonConvert.DeserializeObject<FrameworkResult>(updateSecurityQuestionResult);
        frameworkResult.Should().BeOfType<FrameworkResult>();
        frameworkResult.Status.Should().Be(ResultStatus.Failed);
        frameworkResult.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.InvalidUserId);
    }

    [Fact]
    [Trait("Category", "ErrorCase")]
    public async Task UpdateUserSecurityQuestionAsync_InvalidSecurityQuestionId_ReturnFail()
    {
        await Add_User_SecurityQuestion_Success();

        // Get User
        var url = BaseUrl + ApiRoutePathConstants.GetUserByName;
        var response = await FrontChannelClient
            .PostAsync(
                url,
                new StringContent(JsonConvert.SerializeObject(userName), Encoding.UTF8, "application/json"));
        var userDetails = await response.Content.ReadAsStringAsync();
        var getUserDetailsByUserNameResult = JsonConvert.DeserializeObject<UserModel>(userDetails);
        getUserDetailsByUserNameResult.Should().NotBeNull();
        getUserDetailsByUserNameResult.UserName.Should().BeEquivalentTo(userName);

        var getSecurityQuestion_Url = BaseUrl + ApiRoutePathConstants.GetUserSecurityQuestions;
        var getSecurityQuestion_response = await FrontChannelClient
            .PostAsync(
                getSecurityQuestion_Url,
                new StringContent(JsonConvert.SerializeObject(getUserDetailsByUserNameResult.Id), Encoding.UTF8,
                    "application/json"));
        var addSecurityQuestionResult = await getSecurityQuestion_response.Content.ReadAsStringAsync();
        var questionResult = JsonConvert.DeserializeObject<IList<UserSecurityQuestionModel>>(addSecurityQuestionResult);
        questionResult.Should().NotBeNull();

        // Update
        questionResult[0].Answer = "TestProxy";
        questionResult[0].SecurityQuestionId = Guid.NewGuid();

        // Update Security Question
        var updateSecurityQuestion_Url = BaseUrl + ApiRoutePathConstants.UpdateUserSecurityQuestion;
        var updateSecurityQuestion_response = await FrontChannelClient
            .PostAsync(
                updateSecurityQuestion_Url,
                new StringContent(JsonConvert.SerializeObject(questionResult[0]), Encoding.UTF8, "application/json"));
        var updateSecurityQuestionResult = await updateSecurityQuestion_response.Content.ReadAsStringAsync();
        var frameworkResult = JsonConvert.DeserializeObject<FrameworkResult>(updateSecurityQuestionResult);
        frameworkResult.Should().BeOfType<FrameworkResult>();
        frameworkResult.Status.Should().Be(ResultStatus.Failed);
        frameworkResult.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.InvalidSecurityQuestionId);
    }

    [Fact]
    [Trait("Category", "SuccessCase")]
    public async Task GetUserSecurityQuestionAsync_Success()
    {
        await Add_User_SecurityQuestion_Success();

        // Get User
        var url = BaseUrl + ApiRoutePathConstants.GetUserByName;
        var response = await FrontChannelClient
            .PostAsync(url,
                new StringContent(JsonConvert.SerializeObject(userName), Encoding.UTF8, "application/json"));
        var userDetails = await response.Content.ReadAsStringAsync();
        var getUserDetailsByUserNameResult = JsonConvert.DeserializeObject<UserModel>(userDetails);
        getUserDetailsByUserNameResult.Should().NotBeNull();
        getUserDetailsByUserNameResult.UserName.Should().BeEquivalentTo(userName);

        var getSecurityQuestion_Url = BaseUrl + ApiRoutePathConstants.GetUserSecurityQuestions;
        var getSecurityQuestion_response = await FrontChannelClient
            .PostAsync(
                getSecurityQuestion_Url,
                new StringContent(JsonConvert.SerializeObject(getUserDetailsByUserNameResult.Id), Encoding.UTF8,
                    "application/json"));
        var addSecurityQuestionResult = await getSecurityQuestion_response.Content.ReadAsStringAsync();
        var questionResult = JsonConvert.DeserializeObject<IList<UserSecurityQuestionModel>>(addSecurityQuestionResult);
        questionResult.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "SuccessCase")]
    public async Task DeleteUserSecurityQuestionAsync_Success()
    {
        await Add_User_SecurityQuestion_Success();

        var token = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.access_token);
        // Get User
        var url = BaseUrl + ApiRoutePathConstants.GetUserByName;
        var response = await FrontChannelClient
            .PostAsync(url,
                new StringContent(JsonConvert.SerializeObject(userName), Encoding.UTF8, "application/json"));
        var userDetails = await response.Content.ReadAsStringAsync();
        var getUserDetailsByUserNameResult = JsonConvert.DeserializeObject<UserModel>(userDetails);
        getUserDetailsByUserNameResult.Should().NotBeNull();
        getUserDetailsByUserNameResult.UserName.Should().BeEquivalentTo(userName);

        var getSecurityQuestion_Url = BaseUrl + ApiRoutePathConstants.GetUserSecurityQuestions;
        var getSecurityQuestion_response = await FrontChannelClient
            .PostAsync(getSecurityQuestion_Url,
                new StringContent(JsonConvert.SerializeObject(getUserDetailsByUserNameResult.Id), Encoding.UTF8,
                    "application/json"));
        var addSecurityQuestionResult = await getSecurityQuestion_response.Content.ReadAsStringAsync();
        var questionResult = JsonConvert.DeserializeObject<IList<UserSecurityQuestionModel>>(addSecurityQuestionResult);
        questionResult.Should().NotBeNull();

        // Delete
        var tokenacc = await GetAccessToken();
        FrontChannelClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tokenacc.access_token);

        var deleteSecurityQuestion_Url = BaseUrl + ApiRoutePathConstants.DeleteUserSecurityQuestion;
        var deleteSecurityQuestion_response = await FrontChannelClient
            .PostAsync(
                deleteSecurityQuestion_Url,
                new StringContent(JsonConvert.SerializeObject(questionResult[0]), Encoding.UTF8, "application/json"));
        var deleteSecurityQuestionResult = await deleteSecurityQuestion_response.Content.ReadAsStringAsync();
        var frameworkResult = JsonConvert.DeserializeObject<FrameworkResult>(deleteSecurityQuestionResult);
        frameworkResult.Should().BeOfType<FrameworkResult>();
        frameworkResult.Status.Should().Be(ResultStatus.Success);
    }

    [Fact]
    [Trait("Category", "ErrorCase")]
    public async Task DeleteUserSecurityQuestionAsync_InvalidId_ReturnFail()
    {
        await Add_User_SecurityQuestion_Success();

        // Get User
        var url = BaseUrl + ApiRoutePathConstants.GetUserByName;
        var response = await FrontChannelClient
            .PostAsync(
                url,
                new StringContent(JsonConvert.SerializeObject(userName), Encoding.UTF8, "application/json"));
        var userDetails = await response.Content.ReadAsStringAsync();
        var getUserDetailsByUserNameResult = JsonConvert.DeserializeObject<UserModel>(userDetails);
        getUserDetailsByUserNameResult.Should().NotBeNull();
        getUserDetailsByUserNameResult.UserName.Should().BeEquivalentTo(userName);

        var getSecurityQuestion_Url = BaseUrl + ApiRoutePathConstants.GetUserSecurityQuestions;
        var getSecurityQuestion_response = await FrontChannelClient
            .PostAsync(
                getSecurityQuestion_Url,
                new StringContent(JsonConvert.SerializeObject(getUserDetailsByUserNameResult.Id), Encoding.UTF8,
                    "application/json"));
        var addSecurityQuestionResult = await getSecurityQuestion_response.Content.ReadAsStringAsync();
        var questionResult = JsonConvert.DeserializeObject<IList<UserSecurityQuestionModel>>(addSecurityQuestionResult);
        questionResult.Should().NotBeNull();

        questionResult[0].Id = Guid.NewGuid();

        // Delete
        var deleteSecurityQuestion_Url = BaseUrl + ApiRoutePathConstants.DeleteUserSecurityQuestion;
        var deleteSecurityQuestion_response = await FrontChannelClient
            .PostAsync(
                deleteSecurityQuestion_Url,
                new StringContent(JsonConvert.SerializeObject(questionResult[0]), Encoding.UTF8, "application/json"));
        var deleteSecurityQuestionResult = await deleteSecurityQuestion_response.Content.ReadAsStringAsync();
        var frameworkResult = JsonConvert.DeserializeObject<FrameworkResult>(deleteSecurityQuestionResult);
        frameworkResult.Should().BeOfType<FrameworkResult>();
        frameworkResult.Status.Should().Be(ResultStatus.Failed);
    }

    //[Fact]
    //[Trait("Category", "SuccessCase")]
    //public async Task GenerateAndSendEmailConfirmationTokenAsync_ValidInput_Success()
    //{
    //    TokenResponseResultModel token = await GetAccessToken();
    //    FrontChannelClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.access_token);
    //    var url = BaseUrl + ApiRoutePathConstants.GetUserByName;
    //    var response = await FrontChannelClient
    //                                        .PostAsync(url,
    //                                                   new StringContent(JsonConvert.SerializeObject(userName), Encoding.UTF8, "application/json"));
    //    var userDetails = await response.Content.ReadAsStringAsync();
    //    UserModel getUserDetailsByUserNameResult = JsonConvert.DeserializeObject<UserModel>(userDetails);
    //    getUserDetailsByUserNameResult.Should().NotBeNull();
    //    getUserDetailsByUserNameResult.UserName.Should().BeEquivalentTo(userName);

    //    // Arrange
    //    string username = getUserDetailsByUserNameResult.UserName;
    //    HCL.CS.SF.Domain.GlobalConfiguration.IsEmailConfigurationValid = true;
    //    var generateAndSendEmailConfirmationToken_Url = BaseUrl + ApiRoutePathConstants.GenerateEmailConfirmationToken;
    //    var generateAndSendEmailConfirmationToken_response = await FrontChannelClient
    //                                        .PostAsync( generateAndSendEmailConfirmationToken_Url,
    //                                         new StringContent(JsonConvert.SerializeObject(username), Encoding.UTF8, "application/json"));
    //    var generateAndSendEmailConfirmationTokennResult = await generateAndSendEmailConfirmationToken_response.Content.ReadAsStringAsync();
    //    ApiDomainModel.FrameworkResult frameworkResult = JsonConvert.DeserializeObject<ApiDomainModel.FrameworkResult>(generateAndSendEmailConfirmationTokennResult);
    //    frameworkResult.Should().BeOfType<ApiDomainModel.FrameworkResult>();
    //    frameworkResult.Status.Should().Be(ApiDomainModel.ResultStatus.Success);

    //}

    //[Fact]
    //[Trait("Category", "SuccessCase")]
    //public async Task GenerateAndSendEmailConfirmationTokenAsync_InvalidUserId_ReturnFail()
    //{
    //    TokenResponseResultModel token = await GetAccessToken();
    //    FrontChannelClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.access_token);

    //    // Arrange
    //    Guid userId = Guid.NewGuid();
    //    HCL.CS.SF.Domain.GlobalConfiguration.IsEmailConfigurationValid = true;

    //    var generateAndSendEmailConfirmationToken_Url = BaseUrl + ApiRoutePathConstants.GeneratePhoneNumberConfirmationToken;
    //    var generateAndSendEmailConfirmationToken_response = await FrontChannelClient
    //                                        .PostAsync(
    //                                                            generateAndSendEmailConfirmationToken_Url,
    //                                                            new StringContent(JsonConvert.SerializeObject(userId), Encoding.UTF8, "application/json"));
    //    var generateAndSendEmailConfirmationTokennResult = await generateAndSendEmailConfirmationToken_response.Content.ReadAsStringAsync();
    //    ApiDomainModel.FrameworkResult frameworkResult = JsonConvert.DeserializeObject<ApiDomainModel.FrameworkResult>(generateAndSendEmailConfirmationTokennResult);
    //    frameworkResult.Should().BeOfType<ApiDomainModel.FrameworkResult>();
    //    frameworkResult.Status.Should().Be(ApiDomainModel.ResultStatus.Failed);
    //    frameworkResult.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.InvalidUserId);
    //}

    //[Fact]
    //[Trait("Category", "SuccessCase")]
    //public async Task VerifyEmailConfirmationTokenAsync_ValidInput_Success()
    //{
    //    var url = BaseUrl + ApiRoutePathConstants.GetUserByName;
    //    var response = await FrontChannelClient
    //                                        .PostAsync(
    //                                                            url,
    //                                                            new StringContent(JsonConvert.SerializeObject(userName), Encoding.UTF8, "application/json"));
    //    var userDetails = await response.Content.ReadAsStringAsync();
    //    UserModel getUserDetailsByUserNameResult = JsonConvert.DeserializeObject<UserModel>(userDetails);
    //    getUserDetailsByUserNameResult.Should().NotBeNull();
    //    getUserDetailsByUserNameResult.UserName.Should().BeEquivalentTo(userName);

    //    // Arrange
    //    Guid userId = getUserDetailsByUserNameResult.Id;

    //    HCL.CS.SF.Domain.GlobalConfiguration.IsEmailConfigurationValid = true;
    //    string emailToken = "MDkwOTU4";
    //    HCL.CS.SF.Domain.GlobalConfiguration.IsEmailConfigurationValid = true;
    //    var generateUserToken_data = new
    //    {
    //        user_id = userId,
    //        email_token = emailToken,
    //    };

    //    var verifyEmailurl = BaseUrl + ApiRoutePathConstants.VerifyEmailConfirmationToken;
    //    var verifyEmailresponse = await FrontChannelClient
    //                                        .PostAsync(
    //                                                            verifyEmailurl,
    //                                                            new StringContent(JsonConvert.SerializeObject(generateUserToken_data), Encoding.UTF8, "application/json"));
    //    var verifyEmailDetails = await verifyEmailresponse.Content.ReadAsStringAsync();
    //    var verifyEmailDetailsResult = JsonConvert.DeserializeObject<FrameworkResult>(verifyEmailDetails);
    //    verifyEmailDetailsResult.Should().NotBeNull();
    //    verifyEmailDetailsResult.Should().BeOfType<FrameworkResult>();
    //    verifyEmailDetailsResult.Status.Should().Be(ResultStatus.Success);

    //}

    //[Fact]
    //[Trait("Category", "SuccessCase")]
    //public async Task GenerateAndSendPhoneNumberConfirmationTokenAsync_Validinput_Success()
    //{
    //    var url = BaseUrl + ApiRoutePathConstants.GetUserByName;
    //    var response = await FrontChannelClient
    //                                        .PostAsync(url,
    //                                                   new StringContent(JsonConvert.SerializeObject(userName), Encoding.UTF8, "application/json"));
    //    var userDetails = await response.Content.ReadAsStringAsync();
    //    UserModel getUserDetailsByUserNameResult = JsonConvert.DeserializeObject<UserModel>(userDetails);
    //    getUserDetailsByUserNameResult.Should().NotBeNull();
    //    getUserDetailsByUserNameResult.UserName.Should().BeEquivalentTo(userName);

    //    // Arrange
    //    Guid userId = getUserDetailsByUserNameResult.Id;

    //    HCL.CS.SF.Domain.GlobalConfiguration.IsSmsConfigurationValid = true;
    //    var generateAndSendPhoneNumberurl = BaseUrl + ApiRoutePathConstants.GeneratePhoneNumberConfirmationToken;
    //    var generateAndSendPhoneNumberUrlresponse = await FrontChannelClient
    //                                        .PostAsync(
    //                                                            generateAndSendPhoneNumberurl,
    //                                                            new StringContent(JsonConvert.SerializeObject(userId), Encoding.UTF8, "application/json"));
    //    var verifySmsDetails = await generateAndSendPhoneNumberUrlresponse.Content.ReadAsStringAsync();
    //    var verifySmsDetailsResult = JsonConvert.DeserializeObject<FrameworkResult>(verifySmsDetails);
    //    verifySmsDetailsResult.Should().BeOfType<FrameworkResult>();
    //    verifySmsDetailsResult.Status.Should().Be(ResultStatus.Success);
    //}

    //[Fact]
    //[Trait("Category", "ErrorCase")]
    //public async Task GenerateAndSendPhoneNumberConfirmationTokenAsync_InvalidUserId_ReturnFail()
    //{
    //    // Arrange
    //    Guid userId = Guid.NewGuid();

    //    HCL.CS.SF.Domain.GlobalConfiguration.IsSmsConfigurationValid = true;
    //    var generateAndSendPhoneNumberurl = BaseUrl + ApiRoutePathConstants.GeneratePhoneNumberConfirmationToken;
    //    var generateAndSendPhoneNumberUrlresponse = await FrontChannelClient
    //                                        .PostAsync(
    //                                                            generateAndSendPhoneNumberurl,
    //                                                            new StringContent(JsonConvert.SerializeObject(userId), Encoding.UTF8, "application/json"));
    //    var verifySmsDetails = await generateAndSendPhoneNumberUrlresponse.Content.ReadAsStringAsync();
    //    var verifySmsDetailsResult = JsonConvert.DeserializeObject<FrameworkResult>(verifySmsDetails);
    //    verifySmsDetailsResult.Should().BeOfType<FrameworkResult>();
    //    verifySmsDetailsResult.Status.Should().Be(ResultStatus.Failed);
    //    verifySmsDetailsResult.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.InvalidUserId);
    //}

    //[Fact]
    //[Trait("Category", "SuccessCase")]

    //public async Task VerifyPhoneNumberConfirmationTokenAsync_ValidInpt_Success()
    //{
    //    var url = BaseUrl + ApiRoutePathConstants.GetUserByName;
    //    var response = await FrontChannelClient
    //                                        .PostAsync(
    //                                                            url,
    //                                                            new StringContent(JsonConvert.SerializeObject(userName), Encoding.UTF8, "application/json"));
    //    var userDetails = await response.Content.ReadAsStringAsync();
    //    UserModel getUserDetailsByUserNameResult = JsonConvert.DeserializeObject<UserModel>(userDetails);
    //    getUserDetailsByUserNameResult.Should().NotBeNull();
    //    getUserDetailsByUserNameResult.UserName.Should().BeEquivalentTo(userName);

    //    // Arrange
    //    Guid userId = getUserDetailsByUserNameResult.Id;

    //    HCL.CS.SF.Domain.GlobalConfiguration.IsSmsConfigurationValid = true;
    //    var generateAndSendPhoneNumberurl = BaseUrl + ApiRoutePathConstants.GeneratePhoneNumberConfirmationToken;
    //    var generateAndSendPhoneNumberUrlresponse = await FrontChannelClient
    //                                        .PostAsync(
    //                                                            generateAndSendPhoneNumberurl,
    //                                                            new StringContent(JsonConvert.SerializeObject(userId), Encoding.UTF8, "application/json"));
    //    var sendSmsDetails = await generateAndSendPhoneNumberUrlresponse.Content.ReadAsStringAsync();
    //    var sendSmsDetailsResult = JsonConvert.DeserializeObject<FrameworkResult>(sendSmsDetails);
    //    sendSmsDetailsResult.Should().BeOfType<FrameworkResult>();
    //    sendSmsDetailsResult.Status.Should().Be(ResultStatus.Success);

    //    string smsToken = "663441";
    //    var generateUserToken_data = new
    //    {
    //        user_id = userId,
    //        sms_token = smsToken,
    //    };
    //    HCL.CS.SF.Domain.GlobalConfiguration.IsSmsConfigurationValid = true;

    //    var verifySmsurl = BaseUrl + ApiRoutePathConstants.VerifyPhoneNumberConfirmationToken;
    //    var verifySmsresponse = await FrontChannelClient
    //                                        .PostAsync(
    //                                                            verifySmsurl,
    //                                                            new StringContent(JsonConvert.SerializeObject(generateUserToken_data), Encoding.UTF8, "application/json"));
    //    var verifySmsDetails = await verifySmsresponse.Content.ReadAsStringAsync();
    //    var verifySmsDetailsResult = JsonConvert.DeserializeObject<FrameworkResult>(verifySmsDetails);
    //    verifySmsDetailsResult.Should().NotBeNull();

    //    verifySmsDetailsResult.Should().BeOfType<FrameworkResult>();
    //    verifySmsDetailsResult.Status.Should().Be(ResultStatus.Success);
    //}

    //[Fact]
    //[Trait("Category", "SuccessCase")]
    //public async Task GeneratePasswordResetTokenAsync_Validinput_Success()
    //{
    //    // Add User.
    //    // Register User
    //    Users_Info userInfo = await RegisterUserAsync_ReturnSuccess();

    //    //// Get User By Name
    //    var getUserByUserName_url = BaseUrl + ApiRoutePathConstants.GetUserByName;
    //    var getUserByUserName_response = await FrontChannelClient
    //                                          .PostAsync(
    //                                                      getUserByUserName_url,
    //                                                      new StringContent(JsonConvert.SerializeObject(userInfo.UserName), Encoding.UTF8, "application/json"));
    //    var userDetails = await getUserByUserName_response.Content.ReadAsStringAsync();
    //    UserModel getUserDetailsByUserNameResult = JsonConvert.DeserializeObject<UserModel>(userDetails);
    //    getUserDetailsByUserNameResult.Should().NotBeNull();
    //    getUserDetailsByUserNameResult.UserName.Should().BeEquivalentTo(userInfo.UserName);

    //    // Generate reset password token
    //    Guid userId = getUserDetailsByUserNameResult.Id;
    //    var generateResetPasswordToken_url = BaseUrl + ApiRoutePathConstants.GeneratePasswordResetToken;
    //    var generateResetPasswordToken_response = await FrontChannelClient
    //                                          .PostAsync(
    //                                                      generateResetPasswordToken_url,
    //                                                      new StringContent(JsonConvert.SerializeObject(userId), Encoding.UTF8, "application/json"));
    //    string resetPasswordToken = await generateResetPasswordToken_response.Content.ReadAsStringAsync();
    //    string resetPasswordTokenResult = JsonConvert.DeserializeObject<string>(resetPasswordToken);
    //    resetPasswordTokenResult.Should().NotBeNullOrWhiteSpace();
    //}

    //[Fact]
    //[Trait("Category", "ErrorCase")]
    //public async Task GeneratePasswordResetTokenAsync_InvalidUserId_ReturnFail()
    //{
    //    // Generate reset password token
    //    Guid userId = Guid.NewGuid();
    //    var generateResetPasswordToken_url = BaseUrl + ApiRoutePathConstants.GeneratePasswordResetToken;
    //    var generateResetPasswordToken_response = await FrontChannelClient
    //                                          .PostAsync(
    //                                                      generateResetPasswordToken_url,
    //                                                      new StringContent(JsonConvert.SerializeObject(userId), Encoding.UTF8, "application/json"));
    //    string resetPasswordToken = await generateResetPasswordToken_response.Content.ReadAsStringAsync();
    //    string resetPasswordTokenResult = JsonConvert.DeserializeObject<string>(resetPasswordToken);
    //    resetPasswordTokenResult.Should().NotBeNullOrWhiteSpace();
    //}

    //[Fact]
    //[Trait("Category", "SuccessCase")]
    //public async Task GenerateUserTokenAsync_Validinput_Success()
    //{
    //    // Add User.
    //    // Register User
    //    Users_Info userInfo = await RegisterUserAsync_ReturnSuccess();

    //    //// Get User By Name
    //    var getUserByUserName_url = BaseUrl + ApiRoutePathConstants.GetUserByName;
    //    var getUserByUserName_response = await FrontChannelClient
    //                                          .PostAsync(
    //                                                      getUserByUserName_url,
    //                                                      new StringContent(JsonConvert.SerializeObject(userInfo.UserName), Encoding.UTF8, "application/json"));
    //    var userDetails = await getUserByUserName_response.Content.ReadAsStringAsync();
    //    UserModel getUserDetailsByUserNameResult = JsonConvert.DeserializeObject<UserModel>(userDetails);
    //    getUserDetailsByUserNameResult.Should().NotBeNull();
    //    getUserDetailsByUserNameResult.UserName.Should().BeEquivalentTo(userInfo.UserName);

    //    Guid userId = getUserDetailsByUserNameResult.Id;

    //    string purpose = "GenerateUserToken";

    //    var generateUserToken_data = new
    //    {
    //        user_id = userId,
    //        token_purpose = purpose,
    //    };

    //    var generateUserTokenToken_url = BaseUrl + ApiRoutePathConstants.GenerateUserTokenAsync;
    //    var generateUserTokenToken_url_response = await FrontChannelClient
    //                                          .PostAsync(
    //                                                      generateUserTokenToken_url,
    //                                                      new StringContent(JsonConvert.SerializeObject(generateUserToken_data), Encoding.UTF8, "application/json"));
    //    string resetPasswordToken = await generateUserTokenToken_url_response.Content.ReadAsStringAsync();
    //    string resetPasswordTokenResult = JsonConvert.DeserializeObject<string>(resetPasswordToken);
    //    resetPasswordTokenResult.Should().NotBeNullOrWhiteSpace();

    //    string token = resetPasswordTokenResult;

    //    var userToken_data = new
    //    {
    //        user_id = userId,
    //        token_purpose = purpose,
    //        user_token = token,
    //    };

    //    var verifyUserTokenTokenToken_url = BaseUrl + ApiRoutePathConstants.VerifyUserToken;
    //    var verifyUserTokenTokenToken_url_response = await FrontChannelClient
    //                                          .PostAsync(
    //                                                      verifyUserTokenTokenToken_url,
    //                                                      new StringContent(JsonConvert.SerializeObject(userToken_data), Encoding.UTF8, "application/json"));
    //    string verifyUserToken = await verifyUserTokenTokenToken_url_response.Content.ReadAsStringAsync();
    //    var verifyuserTokenDetailsResult = JsonConvert.DeserializeObject<FrameworkResult>(verifyUserToken);
    //    verifyuserTokenDetailsResult.Should().NotBeNull();
    //    verifyuserTokenDetailsResult.Should().BeOfType<FrameworkResult>();
    //    verifyuserTokenDetailsResult.Status.Should().Be(ResultStatus.Success);
    //}

    //[Fact]
    //[Trait("Category", "SuccessCase")]
    //public async Task VerifyUserTokenAsync_Validinput_Success()
    //{
    //    // Register User
    //    Users_Info userInfo = await RegisterUserAsync_ReturnSuccess();

    //    //// Get User By Name
    //    var getUserByUserName_url = BaseUrl + ApiRoutePathConstants.GetUserByName;
    //    var getUserByUserName_response = await FrontChannelClient
    //                                          .PostAsync(
    //                                                      getUserByUserName_url,
    //                                                      new StringContent(JsonConvert.SerializeObject(userInfo.UserName), Encoding.UTF8, "application/json"));
    //    var userDetails = await getUserByUserName_response.Content.ReadAsStringAsync();
    //    UserModel getUserDetailsByUserNameResult = JsonConvert.DeserializeObject<UserModel>(userDetails);
    //    getUserDetailsByUserNameResult.Should().NotBeNull();
    //    getUserDetailsByUserNameResult.UserName.Should().BeEquivalentTo(userInfo.UserName);

    //    Guid userId = getUserDetailsByUserNameResult.Id;

    //    string purpose = "GenerateToken";
    //    string token = "231125";

    //    var generateUserToken_data = new
    //    {
    //        user_id = userId,
    //        token_purpose = purpose,
    //        user_token = token,
    //    };

    //    var verifyUserTokenTokenToken_url = BaseUrl + ApiRoutePathConstants.VerifyUserToken;
    //    var verifyUserTokenTokenToken_url_response = await FrontChannelClient
    //                                          .PostAsync(
    //                                                      verifyUserTokenTokenToken_url,
    //                                                      new StringContent(JsonConvert.SerializeObject(generateUserToken_data), Encoding.UTF8, "application/json"));
    //    string verifyUserToken = await verifyUserTokenTokenToken_url_response.Content.ReadAsStringAsync();
    //    string resetPasswordTokenResult = JsonConvert.DeserializeObject<string>(verifyUserToken);
    //    resetPasswordTokenResult.Should().NotBeNullOrWhiteSpace();
    //}

    //[Fact]
    //[Trait("Category", "SuccessCase")]
    //public async Task GenerateEmailTwoFactorTokenAsync_ValidInput_Success()
    //{
    //    // Register User
    //    Users_Info userInfo = await RegisterUserAsync_ReturnSuccess();

    //    //// Get User By Name
    //    var getUserByUserName_url = BaseUrl + ApiRoutePathConstants.GetUserByName;
    //    var getUserByUserName_response = await FrontChannelClient
    //                                          .PostAsync(
    //                                                      getUserByUserName_url,
    //                                                      new StringContent(JsonConvert.SerializeObject(userInfo.UserName), Encoding.UTF8, "application/json"));
    //    var userDetails = await getUserByUserName_response.Content.ReadAsStringAsync();
    //    UserModel getUserDetailsByUserNameResult = JsonConvert.DeserializeObject<UserModel>(userDetails);
    //    getUserDetailsByUserNameResult.Should().NotBeNull();
    //    getUserDetailsByUserNameResult.UserName.Should().BeEquivalentTo(userInfo.UserName);

    //    Guid userId = getUserDetailsByUserNameResult.Id;
    //    var generateUserTokenToken_url = BaseUrl + ApiRoutePathConstants.GenerateEmailTwoFactorToken;
    //    var generateUserTokenToken_url_response = await FrontChannelClient
    //                                          .PostAsync(
    //                                                      generateUserTokenToken_url,
    //                                                      new StringContent(JsonConvert.SerializeObject(userId), Encoding.UTF8, "application/json"));
    //    string resetPasswordToken = await generateUserTokenToken_url_response.Content.ReadAsStringAsync();
    //    string resetPasswordTokenResult = JsonConvert.DeserializeObject<string>(resetPasswordToken);
    //    resetPasswordTokenResult.Should().NotBeNullOrWhiteSpace();

    //    string emailToken = resetPasswordTokenResult;

    //    var generateUserToken_data = new
    //    {
    //        user_id = userId,
    //        email_token = emailToken,
    //    };

    //    var verifyEmailToken_url = BaseUrl + ApiRoutePathConstants.VerifyEmailTwoFactorToken;
    //    var verifyEmailTokenToken_url_response = await FrontChannelClient
    //                                          .PostAsync(
    //                                                      verifyEmailToken_url,
    //                                                      new StringContent(JsonConvert.SerializeObject(generateUserToken_data), Encoding.UTF8, "application/json"));
    //    string verifyEmailToken = await verifyEmailTokenToken_url_response.Content.ReadAsStringAsync();

    //    var verifyuserTokenDetailsResult = JsonConvert.DeserializeObject<FrameworkResult>(verifyEmailToken);
    //    verifyuserTokenDetailsResult.Should().NotBeNull();
    //    verifyuserTokenDetailsResult.Should().BeOfType<FrameworkResult>();
    //    verifyuserTokenDetailsResult.Status.Should().Be(ResultStatus.Success);

    //}

    //[Fact]
    //[Trait("Category", "SuccessCase")]
    //public async Task VerifyEmailTwoFactorTokenAsync_Validinput_Success()
    //{
    //    // Register User
    //    Users_Info userInfo = await RegisterUserAsync_ReturnSuccess();

    //    //// Get User By Name
    //    var getUserByUserName_url = BaseUrl + ApiRoutePathConstants.GetUserByName;
    //    var getUserByUserName_response = await FrontChannelClient
    //                                          .PostAsync(
    //                                                      getUserByUserName_url,
    //                                                      new StringContent(JsonConvert.SerializeObject(userInfo.UserName), Encoding.UTF8, "application/json"));
    //    var userDetails = await getUserByUserName_response.Content.ReadAsStringAsync();
    //    UserModel getUserDetailsByUserNameResult = JsonConvert.DeserializeObject<UserModel>(userDetails);
    //    getUserDetailsByUserNameResult.Should().NotBeNull();
    //    getUserDetailsByUserNameResult.UserName.Should().BeEquivalentTo(userInfo.UserName);

    //    Guid userId = getUserDetailsByUserNameResult.Id;

    //    string emailToken = "375207";

    //    var generateUserToken_data = new
    //    {
    //        user_id = userId,
    //        email_token = emailToken,
    //    };

    //    var generateUserTokenToken_url = BaseUrl + ApiRoutePathConstants.VerifyEmailTwoFactorToken;
    //    var generateUserTokenToken_url_response = await FrontChannelClient
    //                                          .PostAsync(
    //                                                      generateUserTokenToken_url,
    //                                                      new StringContent(JsonConvert.SerializeObject(generateUserToken_data), Encoding.UTF8, "application/json"));
    //    string resetPasswordToken = await generateUserTokenToken_url_response.Content.ReadAsStringAsync();
    //}

    //[Fact]
    //[Trait("Category", "SuccessCase")]
    //public async Task GenerateSmsTwoFactorTokenAsync_Validinput_Success()
    //{
    //    //// Register User
    //    Users_Info userInfo = await RegisterUserAsync_ReturnSuccess();

    //    //// Get User By Name
    //    var getUserByUserName_url = BaseUrl + ApiRoutePathConstants.GetUserByName;
    //    var getUserByUserName_response = await FrontChannelClient
    //                                          .PostAsync(
    //                                                      getUserByUserName_url,
    //                                                      new StringContent(JsonConvert.SerializeObject(userInfo.UserName), Encoding.UTF8, "application/json"));
    //    var userDetails = await getUserByUserName_response.Content.ReadAsStringAsync();
    //    UserModel getUserDetailsByUserNameResult = JsonConvert.DeserializeObject<UserModel>(userDetails);
    //    getUserDetailsByUserNameResult.Should().NotBeNull();
    //    getUserDetailsByUserNameResult.UserName.Should().BeEquivalentTo(userInfo.UserName);

    //    Guid userId = getUserDetailsByUserNameResult.Id;

    //    var generateSmsTwoFactorToken_url = BaseUrl + ApiRoutePathConstants.GenerateSmsTwoFactorToken;
    //    var generateSmsTwoFactorToken_response = await FrontChannelClient
    //                                          .PostAsync(
    //                                                      generateSmsTwoFactorToken_url,
    //                                                      new StringContent(JsonConvert.SerializeObject(userId), Encoding.UTF8, "application/json"));
    //    string generateSmsTwoToken = await generateSmsTwoFactorToken_response.Content.ReadAsStringAsync();
    //    string generateSmsTwoTokenResult = JsonConvert.DeserializeObject<string>(generateSmsTwoToken);
    //    generateSmsTwoTokenResult.Should().NotBeNullOrWhiteSpace();
    //}

    [Fact]
    private async Task CreateRole_Success()
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

    private async Task<TokenResponseResultModel> GetAccessToken()
    {
        // Login User.
        await LoginAsync(User);
        positiveCaseClientName = hCLCSS256ClientName;
        clientModel = await FetchClientDetails(positiveCaseClientName);
        clientModel.Should().NotBeNull();

        // Authorize endpoint calls.
        var nonce = Guid.NewGuid().ToString();
        var codeVerifier = 32.RandomString();
        var codeChallengeString = codeVerifier.GenerateCodeChallenge();
        FrontChannelClient.AllowAutoRedirect = false;
        var url = CreateAuthorizeRequestUrl(
            clientModel.ClientId,
            "code",
            "openid email profile offline_access phone HCL.CS.SF.apiresource HCL.CS.SF.client HCL.CS.SF.user HCL.CS.SF.role HCL.CS.SF.identityresource HCL.CS.SF.adminuser HCL.CS.SF.securitytoken",
            responseMode: "query",
            prompt: "none",
            codeChallenge: codeChallengeString, // Codeverifier
            codeChallengeMethod: "S256", // Plain
            maxAge: "60",
            redirectUri: "http://127.0.0.1:63562/",
            nonce: nonce);
        var returnQuery = await FrontChannelClient.GetAsync(url);

        // Token endpoint calls.
        var response = returnQuery.Headers.Location.ToString().ParseQueryString();
        var dict = CreateTokenRequest(
            clientModel.ClientId,
            clientModel.ClientSecret,
            response.Code,
            "http://127.0.0.1:63562/",
            OpenIdConstants.GrantTypes.AuthorizationCode,
            codeVerifier); // Code Challenge
        var tokenClient = BackChannelClient;
        var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(dict));
        var tokenResult = await tokenResponse.ParseTokenResponseResult();
        tokenResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        tokenResult.access_token.Should().NotBeNullOrEmpty();
        tokenResult.id_token.Should().NotBeNullOrEmpty();
        tokenResult.token_type.Should().NotBeNullOrEmpty();
        return tokenResult;
    }

    private static UserModel CreateUserRequestModel()
    {
        var modelList = new List<UserClaimModel>();
        modelList.Add(new UserClaimModel { ClaimType = "Forward", ClaimValue = "Ronaldo", CreatedBy = "Rosh" });

        var questionList = new List<UserSecurityQuestionModel>
        {
            new(),
            new(),
            new()
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
            PhoneNumberConfirmed = true,
            EmailConfirmed = true
        };

        return userRequestModel;
    }

    public class Users_Info
    {
        public string UserName { get; set; }

        public string Password { get; set; }

        public IList<UserSecurityQuestionModel> userSecurityQuestionModels { get; set; }
    }
}
