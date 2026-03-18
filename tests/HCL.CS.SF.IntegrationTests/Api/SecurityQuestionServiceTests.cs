/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.ErrorCodes;
using HCL.CS.SF.Domain.Models;
using HCL.CS.SF.Service.Interfaces;
using HCL.CS.SF.TestApp.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HCL.CS.SF.IntegrationTests.Api
{
    public class SecurityQuestionServiceTests : HCLCSSFFakeSetup
    {
        private readonly IUserAccountService userAccountService;
        private readonly IAuditTrailService auditTrailService;

        public SecurityQuestionServiceTests()
        {
            userAccountService = ServiceProvider.GetService<IUserAccountService>();
            auditTrailService = ServiceProvider.GetService<IAuditTrailService>();
        }

        [Fact]
        [Trait("Category", "ErrorCase")]
        public async Task AddSecurityQuestionAsync_NullCheck()
        {
            var securityQuestion = UserHelper.CreateSecurityQuestionModel();
            securityQuestion.CreatedBy = null;
            securityQuestion.Question = null;
            FrameworkResult result = await userAccountService.AddSecurityQuestionAsync(securityQuestion);

            // Asseret
            result.Should().BeOfType<FrameworkResult>();
            result.Status.Should().Be(ResultStatus.Failed);
            result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.InvalidSecurityQuestion);
        }

        [Fact]
        [Trait("Category", "ErrorCase")]
        public async Task AddSecurityQuestionAsync_EmptyCheck()
        {
            var securityQuestion = UserHelper.CreateSecurityQuestionModel();
            Random random = new Random();
            securityQuestion.CreatedBy = string.Empty;
            securityQuestion.Question = string.Empty;
            FrameworkResult result = await userAccountService.AddSecurityQuestionAsync(securityQuestion);
            // Asseret
            result.Should().BeOfType<FrameworkResult>();
            result.Status.Should().Be(ResultStatus.Failed);
            result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.InvalidSecurityQuestion);
        }

        [Fact]
        [Trait("Category", "ErrorCase")]
        public async Task AddSecurityQuestionAsync_LengthCheck()
        {
            var securityQuestion = UserHelper.CreateSecurityQuestionModel();
            Random random = new Random();
            securityQuestion.CreatedOn = DateTime.UtcNow;
            securityQuestion.CreatedBy = "HHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLLHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHCCCCCCCCCCCCCCLLLLLLLLLLLLLLL";
            securityQuestion.Question = "What is your age ?" + string.Empty + random.Next() + "";
            FrameworkResult result = await userAccountService.AddSecurityQuestionAsync(securityQuestion);
            // Asseret
            result.Should().BeOfType<FrameworkResult>();
            result.Status.Should().Be(ResultStatus.Failed);
            result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.CreatedByTooLong);
        }

        [Fact]
        [Trait("Category", "SuccessCase")]
        public async Task AddSecurityQuestionAsync_Success()
        {
            // Arrange
            var securityQuestion = UserHelper.CreateSecurityQuestionModel();
            Random random = new Random();
            securityQuestion.CreatedOn = DateTime.UtcNow;
            securityQuestion.CreatedBy = "Suresh";
            securityQuestion.Question = "What is your age ?" + string.Empty + random.Next() + string.Empty;
            FrameworkResult result = await userAccountService.AddSecurityQuestionAsync(securityQuestion);
            // Asseret
            result.Should().BeOfType<FrameworkResult>();
            result.Status.Should().Be(ResultStatus.Success);
        }

        [Fact]
        [Trait("Category", "SuccessCase")]
        public async Task AddSecurityQuestionAsync_DuplicateExist()
        {
            // Arrange
            var securityQuestion = UserHelper.CreateSecurityQuestionModel();
            Random random = new Random();
            securityQuestion.CreatedOn = DateTime.UtcNow;
            securityQuestion.CreatedBy = "Suresh";
            securityQuestion.Question = "What is your age ?" + string.Empty + random.Next() + string.Empty;
            FrameworkResult result = await userAccountService.AddSecurityQuestionAsync(securityQuestion);
            // Asseret
            result.Should().BeOfType<FrameworkResult>();
            result.Status.Should().Be(ResultStatus.Success);

            FrameworkResult result2 = await userAccountService.AddSecurityQuestionAsync(securityQuestion);
            // Asseret
            result2.Should().BeOfType<FrameworkResult>();
            result2.Status.Should().Be(ResultStatus.Failed);
        }

        [Fact]
        [Trait("Category", "ErrorCase")]
        public async Task AddSecurityQuestionAsync_Duplicate()
        {
            // Arrange
            var securityQuestion = UserHelper.CreateSecurityQuestionModel();
            Random random = new Random();
            securityQuestion.CreatedOn = DateTime.UtcNow;
            securityQuestion.CreatedBy = "Suresh";
            securityQuestion.Question = "What is your age ?" + string.Empty + random.Next() + string.Empty;
            FrameworkResult result = await userAccountService.AddSecurityQuestionAsync(securityQuestion);
            // Asseret
            result.Should().BeOfType<FrameworkResult>();
            result.Status.Should().Be(ResultStatus.Success);

            // Re Insert
            FrameworkResult result2 = await userAccountService.AddSecurityQuestionAsync(securityQuestion);
            // Asseret
            result2.Should().BeOfType<FrameworkResult>();
            result2.Status.Should().Be(ResultStatus.Failed);
            result2.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.SecurityQuestionAlreadyExists);
        }

        [Fact]
        [Trait("Category", "ErrorCase")]
        public async Task AddSecurityQuestionAsync_CreatedByNull()
        {
            // Arrange
            var securityQuestion = UserHelper.CreateSecurityQuestionModel();
            Random random = new Random();
            securityQuestion.CreatedOn = DateTime.UtcNow;
            securityQuestion.CreatedBy = null;
            securityQuestion.Question = "What is your age ?" + string.Empty + random.Next() + string.Empty;
            FrameworkResult result = await userAccountService.AddSecurityQuestionAsync(securityQuestion);
            // Asseret
            result.Should().BeOfType<FrameworkResult>();
            result.Status.Should().Be(ResultStatus.Success);
        }

        [Fact]
        [Trait("Category", "SuccessCase")]
        public async Task GetAllSecurityQuestionsAsync_Success()
        {
            // Add
            Random random = new Random();
            var securityQuestion = UserHelper.CreateSecurityQuestionModel();
            securityQuestion.Question = "Suresh " + string.Empty + random.Next() + string.Empty;

            IList<SecurityQuestionModel> securityQuestionModels = await userAccountService.GetAllSecurityQuestionsAsync();
            securityQuestionModels.Should().NotBeNull();
        }

        [Fact]
        [Trait("Category", "SuccessCase")]
        public async Task DeleteSecurityQuestionAsync_Success()
        {
            // Add
            Random random = new Random();
            var securityQuestion = UserHelper.CreateSecurityQuestionModel();
            securityQuestion.Question = "What is your favourite Subject ?"+string.Empty+random.Next() +"";
            FrameworkResult result = await userAccountService.AddSecurityQuestionAsync(securityQuestion);
            result.Should().BeOfType<FrameworkResult>();
            result.Status.Should().Be(ResultStatus.Success);
            // Delete
            FrameworkResult result1 = await userAccountService.DeleteSecurityQuestionAsync(securityQuestion.Id);
            result1.Should().BeOfType<FrameworkResult>();
            result1.Status.Should().Be(ResultStatus.Success);
        }

        [Fact]
        [Trait("Category", "ErrorCase")]
        public async Task DeleteSecurityQuestionAsync_Recorddoesnotexist()
        {
            // Add
            Random random = new Random();
            var securityQuestion = UserHelper.CreateSecurityQuestionModel();
            securityQuestion.Question = "What is your favourite Subject ?" + string.Empty + random.Next() + string.Empty;
            FrameworkResult result = await userAccountService.AddSecurityQuestionAsync(securityQuestion);
            result.Should().BeOfType<FrameworkResult>();
            result.Status.Should().Be(ResultStatus.Success);
            securityQuestion.Id = Guid.NewGuid();
            // Delete
            FrameworkResult result1 = await userAccountService.DeleteSecurityQuestionAsync(securityQuestion.Id);
            result1.Should().BeOfType<FrameworkResult>();
            result1.Status.Should().Be(ResultStatus.Failed);
            result1.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.SecurityQuestionNotExists);
        }

        [Fact]
        [Trait("Category", "SuccessCase")]
        public async Task UpdateUserSecurityQuestionAsync_Success()
        {
            // Add SecurityQuestion
            var securityQuestion = UserHelper.CreateSecurityQuestionModel();
            Random random = new Random();
            securityQuestion.CreatedOn = DateTime.UtcNow;
            securityQuestion.CreatedBy = "Suresh";
            securityQuestion.Question = "What is your age ?" + string.Empty + random.Next() + string.Empty;
            FrameworkResult result = await userAccountService.AddSecurityQuestionAsync(securityQuestion);
            // Asseret
            result.Should().BeOfType<FrameworkResult>();
            result.Status.Should().Be(ResultStatus.Success);

            // Add User SecurityQuestion
            IList<SecurityQuestionModel> securityQuestionModel = await userAccountService.GetAllSecurityQuestionsAsync();
            securityQuestionModel.Should().NotBeNull();
            var user = await userAccountService.GetUserByNameAsync("JacobIsmail");
            user.Should().NotBeNull();
            var securityQuestion2 = UserHelper.CreateUserSecurityQuestionModel();
            var question = securityQuestionModel.Where(x => x.Question == securityQuestion.Question);
            securityQuestion2.SecurityQuestionId = question.FirstOrDefault().Id;
            securityQuestion2.UserId = user.Id;
            securityQuestion2.CreatedBy = "Test";
            FrameworkResult result2 = await userAccountService.AddUserSecurityQuestionAsync(securityQuestion2);
            result2.Should().BeOfType<FrameworkResult>();
            result2.Status.Should().Be(ResultStatus.Success);

            // Get User SecurityQuestion
            IList<UserSecurityQuestionModel> userSecurityQuestionModel = await userAccountService.GetUserSecurityQuestionsAsync(user.Id);
            userSecurityQuestionModel.Should().NotBeNull();

            // Update
            userSecurityQuestionModel[0].Answer = "Yes Correct Ans";
            FrameworkResult result3 = await userAccountService.UpdateUserSecurityQuestionAsync(userSecurityQuestionModel[0]);
            result3.Should().BeOfType<FrameworkResult>();
            result3.Status.Should().Be(ResultStatus.Success);
        }

        [Fact]
        [Trait("Category", "SuccessCase")]
        public async Task DeleteUserSecurityQuestionAsync_Success()
        {
            var securityQuestion = UserHelper.CreateSecurityQuestionModel();
            Random random = new Random();
            securityQuestion.CreatedOn = DateTime.UtcNow;
            securityQuestion.CreatedBy = "Suresh";
            securityQuestion.Question = "What is your age ?" + string.Empty + random.Next() + string.Empty;
            FrameworkResult result = await userAccountService.AddSecurityQuestionAsync(securityQuestion);
            // Asseret
            result.Should().BeOfType<FrameworkResult>();
            result.Status.Should().Be(ResultStatus.Success);

            IList<SecurityQuestionModel> securityQuestionModel = await userAccountService.GetAllSecurityQuestionsAsync();
            securityQuestionModel.Should().NotBeNull();
            var user = await userAccountService.GetUserByNameAsync("JacobIsmail");
            user.Should().NotBeNull();
            var securityQuestion2 = UserHelper.CreateUserSecurityQuestionModel();
            var question = securityQuestionModel.Where(x => x.Question == securityQuestion.Question);
            securityQuestion2.SecurityQuestionId = question.FirstOrDefault().Id;
            securityQuestion2.UserId = user.Id;
            securityQuestion2.CreatedBy = "Test";
            FrameworkResult result2 = await userAccountService.AddUserSecurityQuestionAsync(securityQuestion2);

            result2.Should().BeOfType<FrameworkResult>();
            result2.Status.Should().Be(ResultStatus.Success);

            IList<UserSecurityQuestionModel> userSecurityQuestionModel = await userAccountService.GetUserSecurityQuestionsAsync(user.Id);

            FrameworkResult result3 = await userAccountService.DeleteUserSecurityQuestionAsync(userSecurityQuestionModel);
            result3.Should().BeOfType<FrameworkResult>();
            result3.Status.Should().Be(ResultStatus.Success);
        }

        [Fact]
        [Trait("Category", "SuccessCase")]
        public async Task GetAllSecurityQuestionsAsync_SuccessCase()
        {
            IList<SecurityQuestionModel> securityQuestionModels = await userAccountService.GetAllSecurityQuestionsAsync();
            securityQuestionModels.Should().NotBeNullOrEmpty();
        }

        [Fact]
        [Trait("Category", "ErrorCase")]
        public async Task UpdateSecurityQuestionAsync_DuplicateExists_RetrurnSecurityQuestionAlreadyExists()
        {
            // Add
            Random random = new Random();
            var securityQuestion = UserHelper.CreateSecurityQuestionModel();
            securityQuestion.Question = "What is your age ?" + string.Empty + random.Next() + string.Empty;
            FrameworkResult result = await userAccountService.AddSecurityQuestionAsync(securityQuestion);
            result.Should().BeOfType<FrameworkResult>();
            result.Status.Should().Be(ResultStatus.Success);

            var securityQuestionResult = await userAccountService.GetAllSecurityQuestionsAsync();
            securityQuestionResult[0].Question = securityQuestion.Question;
            FrameworkResult result2 = await userAccountService.UpdateSecurityQuestionAsync(securityQuestionResult[0]);
            result2.Should().BeOfType<FrameworkResult>();
            result2.Status.Should().Be(ResultStatus.Failed);
            result2.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.SecurityQuestionAlreadyExists);
        }

        [Fact]
        [Trait("Category", "SuccessCase")]
        public async Task UpdateSecurityQuestionAsync_SuccessCase()
        {
            // Add
            Random random = new Random();
            var securityQuestion = UserHelper.CreateSecurityQuestionModel();
            securityQuestion.Question = "What is your age ?" + string.Empty + random.Next() + string.Empty;
            FrameworkResult result = await userAccountService.AddSecurityQuestionAsync(securityQuestion);
            result.Should().BeOfType<FrameworkResult>();
            result.Status.Should().Be(ResultStatus.Success);

            var securityQuestionResult = await userAccountService.GetAllSecurityQuestionsAsync();
            securityQuestionResult[0].Question = "UpdatedSecurityQuestion" + random.Next();
            FrameworkResult result2 = await userAccountService.UpdateSecurityQuestionAsync(securityQuestionResult[0]);
            result2.Should().BeOfType<FrameworkResult>();
            result2.Status.Should().Be(ResultStatus.Success);
        }

        [Fact]
        [Trait("Category", "SuccessCase")]
        public async Task Add_User_SecurityQuestion_Tests()
        {
            var securityQuestion = UserHelper.CreateSecurityQuestionModel();
            Random random = new Random();
            securityQuestion.CreatedOn = DateTime.UtcNow;
            securityQuestion.CreatedBy = "Suresh";
            securityQuestion.Question = "What is your age ?" + string.Empty + random.Next() + string.Empty;
            FrameworkResult result = await userAccountService.AddSecurityQuestionAsync(securityQuestion);
            // Asseret
            result.Should().BeOfType<FrameworkResult>();
            result.Status.Should().Be(ResultStatus.Success);

            IList<SecurityQuestionModel> securityQuestionModel = await userAccountService.GetAllSecurityQuestionsAsync();
            securityQuestionModel.Should().NotBeNull();
            var user = await userAccountService.GetUserByNameAsync("JacobIsmail");
            user.Should().NotBeNull();
            var securityQuestion2 = UserHelper.CreateUserSecurityQuestionModel();
            var question = securityQuestionModel.Where(x => x.Question == securityQuestion.Question);
            securityQuestion2.SecurityQuestionId = question.FirstOrDefault().Id;
            securityQuestion2.UserId = user.Id;
            securityQuestion2.CreatedBy = "Test";
            FrameworkResult result2 = await userAccountService.AddUserSecurityQuestionAsync(securityQuestion2);

            result2.Should().BeOfType<FrameworkResult>();
            result2.Status.Should().Be(ResultStatus.Success);
        }

        [Fact]
        public async Task Get_All_SecurityQuestion_Tests()
        {
            var result = await userAccountService.GetAllSecurityQuestionsAsync();
            result.Should().NotBeEmpty();
        }
    }
}



