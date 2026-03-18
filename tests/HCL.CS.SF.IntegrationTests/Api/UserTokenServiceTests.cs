/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using FluentAssertions;
//using HCL.CS.SF.Domain;
//using HCL.CS.SF.Domain.Entities;
//using HCL.CS.SF.Domain.ErrorCodes;
//using HCL.CS.SF.Domain.Models;
//using HCL.CS.SF.Service.Interfaces;
//using HCL.CS.SF.TestApp.Helpers;
//using Microsoft.Extensions.DependencyInjection;
//using Xunit;

//namespace HCL.CS.SF.IntegrationTests.Api
//{
//    /// <summary>
//    ///  UserTokenServiceTests.
//    /// </summary>
//    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
//    public class UserTokenServiceTests : HCLCSSFFakeSetup
//    {
//        private readonly IUserAccountService userAccountService;
//        private readonly IAuditTrailService auditTrailService;

//        /// <summary>
//        /// Initializes a new instance of the <see cref="UserTokenServiceTests"/> class.
//        ///  SecurityQuestionServiceTests.
//        /// </summary>
//        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
//        public UserTokenServiceTests()
//        {
//            userAccountService = ServiceProvider.GetService<IUserAccountService>();
//            auditTrailService = ServiceProvider.GetService<IAuditTrailService>();
//        }

//        /// <summary>
//        /// Initializes a new instance of the <see cref="UserTokenServiceTests"/> class.
//        ///  SecurityQuestionServiceTests.
//        /// </summary>
//        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
//        [Fact]
//        [Trait("Category", "SuccessCase")]
//        public async Task GenerateAndSendEmailConfirmationTokenAsync_ValidInput_Success()
//        {
//            // Arrange
//            Users users = UserHelper.GetUser();
//            Guid guid = new Guid("A73E64A8-C33B-4475-8D30-1028E4FE30CA");
//            users.Id = guid;
//            HCL.CS.SF.Domain.GlobalConfiguration.IsEmailConfigurationValid = true;

//            FrameworkResult result = await userAccountService.GenerateEmailConfirmationTokenAsync(users.Id);
//            result.Should().BeOfType<FrameworkResult>();
//            result.Status.Should().Be(ResultStatus.Success);
//        }

//        /// <summary>
//        /// VerifyEmailConfirmationTokenAsync Validinput Success.
//        /// </summary>
//        /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous unit test.</placeholder></returns>
//        [Fact]
//        [Trait("Category", "SuccessCase")]
//        public async Task VerifyEmailConfirmationTokenAsync_ValidInput_Success()
//        {
//            // Arrange
//            Users existingUser = UserHelper.GetUser();
//            Guid guid = new Guid("A73E64A8-C33B-4475-8D30-1028E4FE30CA");
//            existingUser.Id = guid;
//            string emailToken = "MDkwOTU4";
//            HCL.CS.SF.Domain.GlobalConfiguration.IsEmailConfigurationValid = true;
//            FrameworkResult result = await userAccountService.VerifyEmailConfirmationTokenAsync(existingUser.Id, emailToken);
//            result.Should().BeOfType<FrameworkResult>();
//            result.Status.Should().Be(ResultStatus.Success);
//        }

//        /// <summary>
//        /// Initializes a new instance of the <see cref="UserTokenServiceTests"/> class.
//        ///  SecurityQuestionServiceTests.
//        /// </summary>
//        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
//        [Fact]
//        [Trait("Category", "SuccessCase")]
//        public async Task GenerateAndSendPhoneNumberConfirmationTokenAsync_Validinput_Success()
//        {
//            // Arrange
//            Users existingUser = UserHelper.GetUser();
//            Guid guid = new Guid("A73E64A8-C33B-4475-8D30-1028E4FE30CA");
//            existingUser.Id = guid;
//            HCL.CS.SF.Domain.GlobalConfiguration.IsSmsConfigurationValid = true;
//            FrameworkResult result = await userAccountService.GeneratePhoneNumberConfirmationTokenAsync(existingUser.Id);
//            result.Should().BeOfType<FrameworkResult>();
//            result.Status.Should().Be(ResultStatus.Success);
//        }

//        /// <summary>
//        /// Initializes a new instance of the <see cref="UserTokenServiceTests"/> class.
//        ///  SecurityQuestionServiceTests.
//        /// </summary>
//        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
//        [Fact]
//        [Trait("Category", "SuccessCase")]

//        public async Task VerifyPhoneNumberConfirmationTokenAsync_ValidInpt_Success()
//        {
//            Users existingUserId = UserHelper.GetUser();
//            Guid guid = new Guid("1026C140-2813-4FB8-A24D-E40834986AD7");
//            existingUserId.Id = guid;
//            string smsToken = "MDQ1MzAz";
//            HCL.CS.SF.Domain.GlobalConfiguration.IsSmsConfigurationValid = true;
//            FrameworkResult result = await userAccountService.VerifyPhoneNumberConfirmationTokenAsync(existingUserId.Id, smsToken);
//            result.Should().BeOfType<FrameworkResult>();
//            result.Status.Should().Be(ResultStatus.Success);
//        }

//        /// <summary>
//        /// Initializes a new instance of the <see cref="UserTokenServiceTests"/> class.
//        ///  SecurityQuestionServiceTests.
//        /// </summary>
//        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
//        //[Fact]
//        //[Trait("Category", "SuccessCase")]
//        //public async Task GeneratePasswordResetTokenAsync_Validinput_Success()
//        //{
//        //    //Arrange
//        //    Users existinguser = UserHelper.GetUser();
//        //    Guid guid = new Guid("1026C140-2813-4FB8-A24D-E40834986AD7");
//        //    existinguser.Id = guid;
//        //    string smsToken = "MDQ1MzAz";
//        //    string tokenResult = await userAccountService.GeneratePasswordResetTokenAsync(existinguser.Id);
//        //    //Assert
//        //    tokenResult.Should().NotBeNullOrEmpty();

//        //}

//        /// <summary>
//        /// Initializes a new instance of the <see cref="UserTokenServiceTests"/> class.
//        ///  SecurityQuestionServiceTests.
//        /// </summary>
//        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
//        //[Fact]
//        //[Trait("Category", "SuccessCase")]
//        //public async Task GenerateEmailTwoFactorTokenAsync_ValidInput_Success()
//        //{
//        //    Users existinguser = UserHelper.GetUser();
//        //    Guid guid = new Guid("1026C140-2813-4FB8-A24D-E40834986AD7");
//        //    existinguser.Id = guid;
//        //    string smsToken = "MDQ1MzAz";
//        //    string Result = await userAccountService.GenerateEmailTwoFactorTokenAsync(existinguser.Id);
//        //    Result.Should().NotBeNullOrEmpty();

//        //}

//        /// <summary>
//        /// Initializes a new instance of the <see cref="UserTokenServiceTests"/> class.
//        ///  SecurityQuestionServiceTests.
//        /// </summary>
//        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
//        //[Fact]
//        //[Trait("Category", "SuccessCase")]
//        //public async Task GenerateUserTokenAsync_Validinput_Success()
//        //{
//        //    Users existinguser = UserHelper.GetUser();
//        //    Guid guid = new Guid("1026C140-2813-4FB8-A24D-E40834986AD7");
//        //    existinguser.Id = guid;
//        //    string purpose = "NWE0OWQ1OGEtY2FiOC00ZDhmLTgzNDEtNmZkYWQ2YmViNWU4";
//        //    string TokenResult = await userAccountService.GenerateUserTokenAsync(existinguser.Id, purpose);
//        //    TokenResult.Should().NotBeNullOrEmpty();

//        //}

//        /// <summary>
//        /// Initializes a new instance of the <see cref="UserTokenServiceTests"/> class.
//        ///  SecurityQuestionServiceTests.
//        /// </summary>
//        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
//        [Fact]
//        [Trait("Category", "SuccessCase")]
//        public async Task VerifyEmailTwoFactorTokenAsync_Validinput_Success()
//        {
//            Users existinguser = UserHelper.GetUser();
//            Guid guid = new Guid("1026C140-2813-4FB8-A24D-E40834986AD7");
//            existinguser.Id = guid;
//            string emailToken = "MDQ1MzAz";
//            FrameworkResult result = await userAccountService.VerifyEmailTwoFactorTokenAsync(existinguser.Id, emailToken);
//            result.Should().BeOfType<FrameworkResult>();
//            result.Should().NotBeNull();
//            result.Status.Should().Be(ResultStatus.Success);
//        }

//        /// <summary>
//        /// Initializes a new instance of the <see cref="UserTokenServiceTests"/> class.
//        ///  SecurityQuestionServiceTests.
//        /// </summary>
//        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
//        [Fact]
//        [Trait("Category", "SuccessCase")]
//        public async Task VerifySmsTwoFactorTokenAsync_Validinput_Success()
//        {
//            Users existinguser = UserHelper.GetUser();
//            Guid guid = new Guid("1026C140-2813-4FB8-A24D-E40834986AD7");
//            existinguser.Id = guid;
//            string smsToken = "MDQ1MzAz";

//            FrameworkResult result = await userAccountService.VerifySmsTwoFactorTokenAsync(existinguser.Id, smsToken);
//            result.Should().BeOfType<FrameworkResult>();
//            result.Should().NotBeNull();
//            result.Status.Should().Be(ResultStatus.Success);

//        }

//        /// <summary>
//        /// Initializes a new instance of the <see cref="UserTokenServiceTests"/> class.
//        ///  SecurityQuestionServiceTests.
//        /// </summary>
//        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
//        //[Fact]
//        //[Trait("Category", "SuccessCase")]
//        //public async Task GenerateSmsTwoFactorTokenAsync_Validinput_Success()
//        //{
//        //    Users existinguser = UserHelper.GetUser();
//        //    Guid guid = new Guid("1026C140-2813-4FB8-A24D-E40834986AD7");
//        //    existinguser.Id = guid;
//        //    string result = await userAccountService.GenerateSmsTwoFactorTokenAsync(existinguser.Id);
//        //    result.Should().NotBeNullOrEmpty();
//        //}
//    }
//}



