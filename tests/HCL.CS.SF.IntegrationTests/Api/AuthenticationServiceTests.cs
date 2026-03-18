/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System;
using System.Threading.Tasks;
using FluentAssertions;
using HCL.CS.SF.Domain;
using HCL.CS.SF.Service.Interfaces;
using HCL.CS.SF.TestApp.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HCL.CS.SF.IntegrationTests.Api
{
    public class AuthenticationServiceTests : HCLCSSFFakeSetup
    {
        private IAuthenticationService authenticationService;
        private IUserAccountService userAccountService;

        public AuthenticationServiceTests()
        {
            userAccountService = ServiceProvider.GetService<IUserAccountService>();
            authenticationService = ServiceProvider.GetService<IAuthenticationService>();
        }

        [Fact]
        public async Task Login()
        {
            //var result = await authenticationService.PasswordSignInAsync("", "");

            //result = await authenticationService.PasswordSignInAsync("adminuser", "");

            //result = await authenticationService.PasswordSignInAsync("adminUshyjger", "Test@123");

            //result = await authenticationService.PasswordSignInAsync("adminUser", "Test@123456");

            //var result = await authenticationService.PasswordSignInAsync("Jesu", "Test@123");

            // Success
            var result = await authenticationService.PasswordSignInAsync("adminUser", "Test@123");
        }

        [Fact]
        public async Task TwoFactorSMS()
        {
            var result = await authenticationService.TwoFactorEmailSignInAsync("Test@123");
        }

        [Fact]
        public async Task SignOutAsync()
        {
            await authenticationService.SignOutAsync();
        }

        [Fact]
        public async Task SetupAuthenticatorAppAsync_Tests()
        {
            var usermodel = UserHelper.CreateModel();
            usermodel.UserName = "TestingTestingdfsdf";
            usermodel.Email = "testingsdfsdf@HCL.CS.SF.com";
            var securityQuestion = await userAccountService.GetAllSecurityQuestionsAsync();
            if (securityQuestion != null && securityQuestion.Count > 0)
            {
                usermodel.UserSecurityQuestion[0].SecurityQuestionId = securityQuestion[0].Id;
            }

            FrameworkResult framResult = await userAccountService.RegisterUserAsync(usermodel);

            var result = await authenticationService.SetupAuthenticatorAppAsync(new Guid("763C134B-B796-41F5-B22E-08D9C46BAFAF"), "HCL.CS.SF");

            framResult.Should().BeOfType<FrameworkResult>();
            framResult.Status.Should().Be(ResultStatus.Success);
        }
    }
}


