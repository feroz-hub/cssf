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
using AutoMapper;
using FluentAssertions;
using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Enums;
using HCL.CS.SF.Domain.ErrorCodes;
using HCL.CS.SF.Domain.Models;
using HCL.CS.SF.DomainServices.Infra;
using HCL.CS.SF.Service.Interfaces;
using HCL.CS.SF.TestApp;
using HCL.CS.SF.TestApp.Helpers;

using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HCL.CS.SF.IntegrationTests.Api
{
    public class UserRoleServiceTests : HCLCSSFFakeSetup
    {
        private const string UserName = "PeterParker";
        private const string RoleName = "MajorAdmin";
        private readonly IUserAccountService userAccountService;
        private readonly IMapper mapper;
        private readonly IAuditTrailService auditTrailService;
        private readonly IRoleService roleService;
        private readonly IResourceStringHandler resourceString;

        public UserRoleServiceTests()
        {
            userAccountService = ServiceProvider.GetService<IUserAccountService>();
            mapper = ServiceProvider.GetService<IMapper>();
            auditTrailService = ServiceProvider.GetService<IAuditTrailService>();
            roleService = ServiceProvider.GetService<IRoleService>();
            resourceString = ServiceProvider.GetService<IResourceStringHandler>();
        }

        // Master User Added

        [Fact]
        [Trait("Category", "InitialUserRunSuccess")]
        public async Task CreateInitialUserRecord()
        {
            // Getting create User Model.
            var userModelInput = UserHelper.CreateUserRequestModel();
            userModelInput.UserName = UserName;
            userModelInput.CreatedBy = "Rosh";
            // Getting Security Questions
            var securityQuestionResult = await userAccountService.GetAllSecurityQuestionsAsync();
            // Adding the first Question Id to the User.
            if (securityQuestionResult != null && securityQuestionResult.Count > 0)
            {
                userModelInput.UserSecurityQuestion[0].SecurityQuestionId = securityQuestionResult[0].Id;
                userModelInput.UserSecurityQuestion[0].Answer = "Dwayne Johnson";
            }

            // Adding User.
            FrameworkResult result = await userAccountService.RegisterUserAsync(userModelInput);
            // Asserting the add is successful.
            result.Should().BeOfType<FrameworkResult>();
            result.Status.Should().Be(ResultStatus.Success);
        }

        [Fact]
        [Trait("Category", "AddSuccessCase")]
        public async Task<RoleModel> CreateRole_Success()
        {
            Random random = new Random();
            // Add/Create Role
            RoleModel roleModelInput = RoleHelper.CreateRoleModel();
            roleModelInput.Name = RoleName + random.Next();
            foreach (var roleClaim in roleModelInput.RoleClaims)
            {
                roleClaim.CreatedBy = roleModelInput.CreatedBy;
            }

            FrameworkResult result = await roleService.CreateRoleAsync(roleModelInput);
            result.Should().BeOfType<FrameworkResult>();
            result.Status.Should().Be(ResultStatus.Success);

            // GetRoleAsync for name
            RoleModel roleModelResult = await roleService.GetRoleAsync(roleModelInput.Name);
            roleModelResult.Should().NotBeNull();
            roleModelResult.Id.Should().NotBe(Guid.NewGuid());
            roleModelResult.Name.Should().BeEquivalentTo(roleModelInput.Name);
            return roleModelResult;
        }

        [Fact]
        public async Task CreateMasterUser()
        {
            // Getting create User Model.
            var userModelInput = UserHelper.CreateUserRequestModelForUserRole();
            userModelInput.UserName = "SuperMan";
            userModelInput.FirstName = "Clark";
            userModelInput.LastName = "Kent";
            userModelInput.Email = "SuperMan@JL.com";
            userModelInput.CreatedBy = " Jerry Siegel";
            // Getting Security Questions
            var securityQuestionResult = await userAccountService.GetAllSecurityQuestionsAsync();
            // Adding the first Question Id to the User.
            if (securityQuestionResult != null && securityQuestionResult.Count > 0)
            {
                userModelInput.UserSecurityQuestion[0].SecurityQuestionId = securityQuestionResult[0].Id;
                userModelInput.UserSecurityQuestion[0].Answer = "Eighteen";
            }

            // Adding User.
            FrameworkResult result = await userAccountService.RegisterUserAsync(userModelInput);
            // Asserting the add is successful.
            result.Should().BeOfType<FrameworkResult>();
            result.Status.Should().Be(ResultStatus.Success);
        }

        #region Add

        [Fact]
        [Trait("Category", "AddSuccessCase")]
        public async Task AddUserRoleAsync_ModelInput_Success()
        {
            // Get user by username
            UserModel userModelResult = await CreateUserWithModel();
            // Get Role by name
            RoleModel roleModelResult = await roleService.GetRoleAsync(RoleName);
            roleModelResult.Should().NotBeNull();
            // Arrange for UserRoleModel Input
            UserRoleModel userRoleModelInput = RoleHelper.CreateUserRoleModel();
            userRoleModelInput.UserId = userModelResult.Id;
            userRoleModelInput.RoleId = roleModelResult.Id;
            userRoleModelInput.CreatedBy = "Rosh";
            // Act : AddUserRoleAsync
            FrameworkResult addUserRoleResult = await userAccountService.AddUserRoleAsync(userRoleModelInput);
            addUserRoleResult.Status.Should().Be(ResultStatus.Success);
            // Check audit trail
            AuditCheck auditCheck = new AuditCheck();
            AuditResponseModel auditResponseModel = await auditCheck.GetAudit(userRoleModelInput.CreatedBy, AuditType.Create);
            auditResponseModel.Should().NotBeNull();
        }

        [Fact]
        [Trait("Category", "AddErrorCase")]
        public async Task AddUserRoleAsync_ModelInput_InvalidUserIdandInvalidRoleId_ReturnsError()
        {
            // Arrange for UserRoleModel Input
            UserRoleModel userRoleModelInput = RoleHelper.CreateUserRoleModel();
            userRoleModelInput.UserId = Guid.NewGuid();
            userRoleModelInput.CreatedBy = "Rosh";
            userRoleModelInput.RoleId = Guid.NewGuid();

            // Act : AddUserRoleAsync
            FrameworkResult result = await userAccountService.AddUserRoleAsync(userRoleModelInput);
            result.Should().BeOfType<FrameworkResult>();
            result.Status.Should().Be(ResultStatus.Failed);
            result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.InvalidUserId);

            // Get User Role
            UserModel userModelResult = await CreateUserWithModel();
            userModelResult.Should().NotBeNull();
            userRoleModelInput.UserId = userModelResult.Id;
            userRoleModelInput.CreatedBy = "Suresh";
            userRoleModelInput.RoleId = Guid.NewGuid();

            FrameworkResult result2 = await userAccountService.AddUserRoleAsync(userRoleModelInput);
            result2.Should().BeOfType<FrameworkResult>();
            result2.Status.Should().Be(ResultStatus.Failed);
            result2.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.InvalidRoleId);
        }

        [Fact]
        [Trait("Category", "AddErrorCase")]
        public async Task AddUserRoleAsync_ModelInput_UserRoleAlreadyExists_ReturnsError()
        {
            // Get User Role
            UserModel userModelResult = await CreateUserWithModel();
            userModelResult.Should().NotBeNull();
            // Get Role by name
            RoleModel roleModelResult = await CreateRole_Success();
            roleModelResult.Should().NotBeNull();

            // Arrange for UserRoleModel Input
            UserRoleModel userRoleModelInput = RoleHelper.CreateUserRoleModel();
            userRoleModelInput.UserId = userModelResult.Id;
            userRoleModelInput.RoleId = roleModelResult.Id;
            userRoleModelInput.CreatedBy = "Rosh";

            // Act : AddUserRoleAsync
            FrameworkResult addUserRoleResult = await userAccountService.AddUserRoleAsync(userRoleModelInput);
            FrameworkResult addUserRoleResult2 = await userAccountService.AddUserRoleAsync(userRoleModelInput);
            addUserRoleResult2.Status.Should().Be(ResultStatus.Failed);
            addUserRoleResult2.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.UserRoleAlreadyExists);
        }

        [Fact]
        [Trait("Category", "AddErrorCase")]
        public async Task AddUserRolesAsync_ModelListIist_UserRoleAlreadyExistsForAMember_ReturnsError()
        {
            // Get User Role
            UserModel userModelResult = await CreateUserWithModel();
            userModelResult.Should().NotBeNull();
            UserModel userModelResult1 = await CreateUserWithModel();
            userModelResult1.Should().NotBeNull();
            UserModel userModelResult2 = await CreateUserWithModel();
            userModelResult2.Should().NotBeNull();
            // Get Role by name
            RoleModel roleModelResult = await CreateRole_Success();
            roleModelResult.Should().NotBeNull();

            // Arrange for UserRoleModel Input
            List<UserRoleModel> userRoleModelListInput = RoleHelper.CreateUserRoleModelList();
            // First List Member
            userRoleModelListInput[0].UserId = userModelResult.Id;
            userRoleModelListInput[0].RoleId = roleModelResult.Id;
            userRoleModelListInput[0].CreatedBy = "Rosh";
            // Second List Member
            userRoleModelListInput[1].UserId = userModelResult.Id;
            userRoleModelListInput[1].RoleId = roleModelResult.Id;
            userRoleModelListInput[1].CreatedBy = "Rosh";
            // Third List Member
            userRoleModelListInput[2].UserId = userModelResult2.Id;
            userRoleModelListInput[2].RoleId = roleModelResult.Id;
            userRoleModelListInput[2].CreatedBy = "Rosh";
            // Act : AddUserRoleAsync
            FrameworkResult addUserRoleResult = await userAccountService.AddUserRolesAsync(userRoleModelListInput);
            addUserRoleResult.Status.Should().Be(ResultStatus.Failed);
            addUserRoleResult.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.DuplicateUserRoleInput);
        }

        [Fact]
        [Trait("Category", "AddSuccessCase")]
        public async Task AddUserRolesAsync_ModelListIist_Success()
        {
            // Get User Role
            UserModel userModelResult = await CreateUserWithModel();
            userModelResult.Should().NotBeNull();
            // Get Role by name
            RoleModel roleModelResult = await CreateRole_Success();
            roleModelResult.Should().NotBeNull();

            // Get User Role
            UserModel userModelResult1 = await userAccountService.GetUserByNameAsync("PeterParker");
            userModelResult1.Should().NotBeNull();
            UserModel userModelResult2 = await userAccountService.GetUserByNameAsync("DarkKnight");
            userModelResult2.Should().NotBeNull();
            // Get Role by name
            //RoleModel roleModelResult = await roleService.GetRoleAsync(RoleName);
            //roleModelResult.Should().NotBeNull();

            // Arrange for UserRoleModel Input
            List<UserRoleModel> userRoleModelListInput = RoleHelper.CreateUserRoleModelList();
            // First List Member
            userRoleModelListInput[0].UserId = userModelResult1.Id;
            userRoleModelListInput[0].RoleId = roleModelResult.Id;
            userRoleModelListInput[0].CreatedBy = "Rosh";
            userRoleModelListInput[0].Id = Guid.NewGuid();
            // Second List Member
            userRoleModelListInput[1].UserId = userModelResult2.Id;
            userRoleModelListInput[1].RoleId = roleModelResult.Id;
            userRoleModelListInput[1].CreatedBy = "Rosh";
            userRoleModelListInput[1].Id = Guid.NewGuid();
            // Act : AddUserRoleAsync
            FrameworkResult addUserRoleResult = await userAccountService.AddUserRolesAsync(userRoleModelListInput);
            addUserRoleResult.Status.Should().Be(ResultStatus.Success);
            // Check audit trail
            AuditCheck auditCheck = new AuditCheck();
            AuditResponseModel auditResponseModel = await auditCheck.GetAudit(userRoleModelListInput[0].CreatedBy, AuditType.Create);
            auditResponseModel.Should().NotBeNull();
        }

        #endregion

        #region Delete/Remove

        [Fact]
        [Trait("Category", "DeleteSuccessCase")]
        public async Task RemoveUserRoleAsync_ByUserRoleModel_Success()
        {
            // Get User using UserName
            UserModel userModelResult1 = await userAccountService.GetUserByNameAsync(UserName);
            userModelResult1.Should().NotBeNull();
            Guid userIdInput = userModelResult1.Id;

            // Get UserRole by UserId
            IList<string> userRolesResult = await userAccountService.GetUserRolesAsync(userIdInput);
            userRolesResult.Should().NotBeNullOrEmpty();

            // Get RoleModel using RoleNameLis
            List<RoleModel> rolesList = new List<RoleModel>();
            foreach (var roleName in userRolesResult)
            {
                RoleModel roleModelResult = await roleService.GetRoleAsync(roleName);
                roleModelResult.Should().NotBeNull();
                rolesList.Add(roleModelResult);
            }

            UserRoleModel deleteUserRoleModelInput = new UserRoleModel();
            deleteUserRoleModelInput.UserId = userIdInput;
            deleteUserRoleModelInput.RoleId = rolesList[0].Id;
            deleteUserRoleModelInput.CreatedBy = "Rosh";

            // Remove UserRole using UserRoleModel.
            FrameworkResult deleteUserRoleResult = await userAccountService.RemoveUserRoleAsync(deleteUserRoleModelInput);
            deleteUserRoleResult.Status.Should().Be(ResultStatus.Success);
            // Check audit trail
            AuditCheck auditCheck = new AuditCheck();
            AuditResponseModel auditResponseModel = await auditCheck.GetAudit(deleteUserRoleModelInput.CreatedBy, AuditType.Delete);
            auditResponseModel.Should().NotBeNull();
            // Filtering out the result to contain only UserRole Deleted records.
            List<AuditTrailModel> deleteUserRoleAuditList = auditResponseModel
                                                            .AuditList
                                                            .Select(auditTrail => auditTrail)
                                                            .Where(
                                                             auditrail => auditrail.TableName.Equals("UserRoles")
                                                            && auditrail.ActionType.Equals(AuditType.Delete)).ToList();
            // Check if the List contains the Deleted UserRole UserId and RoleId.
            deleteUserRoleAuditList.Any(_ => _.OldValue.Contains(deleteUserRoleModelInput.UserId.ToString())).Should().BeTrue();
            deleteUserRoleAuditList.Any(_ => _.OldValue.Contains(deleteUserRoleModelInput.RoleId.ToString())).Should().BeTrue();
        }

        [Fact]
        [Trait("Category", "DeleteErrorCase")]
        public async Task RemoveUserRoleAsync_ByUserRoleModel_UserRoleNotExist_ResturnsError()
        {
            // Case 1 : UserRoleId is not mapped.
            // Get User using UserName
            Guid userIdInput = Guid.NewGuid();
            // Get Role
            // Get Role by name
            RoleModel roleModelResult = await roleService.GetRoleAsync("TestRoleName");
            roleModelResult.Should().NotBeNull();
            // delete UserRoleModel.
            UserRoleModel deleteUserRoleModelInput = new UserRoleModel();
            deleteUserRoleModelInput.UserId = userIdInput;
            deleteUserRoleModelInput.RoleId = roleModelResult.Id;
            deleteUserRoleModelInput.CreatedBy = "Rosh";
            // Error Scenario.
            FrameworkResult deleteUserRoleResult = await userAccountService.RemoveUserRoleAsync(deleteUserRoleModelInput);
            deleteUserRoleResult.Status.Should().Be(ResultStatus.Failed);
            deleteUserRoleResult.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.UserRoleNotExists);
        }

        [Fact]
        [Trait("Category", "DeleteSuccessCase")]
        public async Task RemoveUserRolesAsync_ByUserRoleModelList_Success()
        {
            // Get User Role
            UserModel userModelResult1 = await userAccountService.GetUserByNameAsync("PeterParker");
            userModelResult1.Should().NotBeNull();
            UserModel userModelResult2 = await userAccountService.GetUserByNameAsync("DarkKnight");
            userModelResult2.Should().NotBeNull();
            // Get Role by name
            RoleModel roleModelResult = await roleService.GetRoleAsync(RoleName);
            roleModelResult.Should().NotBeNull();
            // Arrange for UserRoleModel Input
            List<UserRoleModel> userRoleModelListInput = RoleHelper.CreateUserRoleModelList();
            // First List Member
            userRoleModelListInput[0].UserId = userModelResult1.Id;
            userRoleModelListInput[0].RoleId = roleModelResult.Id;
            userRoleModelListInput[0].CreatedBy = "Rosh";
            userRoleModelListInput[0].Id = Guid.NewGuid();
            // Second List Member
            userRoleModelListInput[1].UserId = userModelResult2.Id;
            userRoleModelListInput[1].RoleId = roleModelResult.Id;
            userRoleModelListInput[1].CreatedBy = "Rosh";
            userRoleModelListInput[1].Id = Guid.NewGuid();
            // Act : Remove UserRole with UserRoleModelList as input.
            FrameworkResult deleteUserRoleResult = await userAccountService.RemoveUserRolesAsync(userRoleModelListInput);
            deleteUserRoleResult.Status.Should().Be(ResultStatus.Success);
            // Check audit trail
            AuditCheck auditCheck = new AuditCheck();
            AuditResponseModel auditResponseModel = await auditCheck.GetAudit(userRoleModelListInput[0].CreatedBy, AuditType.Delete);
            auditResponseModel.Should().NotBeNull();
            // Filtering out the result to contain only UserRole Deleted records.
            List<AuditTrailModel> deleteUserRoleAuditList = auditResponseModel
                                                            .AuditList
                                                            .Select(auditTrail => auditTrail)
                                                            .Where(
                                                             auditrail => auditrail.TableName.Equals("UserRoles")
                                                            && auditrail.ActionType.Equals(AuditType.Delete)).ToList();
            // Checking for values in the AudditList
            foreach (var userRole in userRoleModelListInput)
            {
                deleteUserRoleAuditList.Any(_ => _.OldValue.Contains(userRole.UserId.ToString())).Should().BeTrue();
                deleteUserRoleAuditList.Any(_ => _.OldValue.Contains(userRole.RoleId.ToString())).Should().BeTrue();
            }
        }

        #endregion

        #region Get

        [Fact]
        [Trait("Category", "GetSuccessCase")]
        public async Task GetUserRolesAsync_Sucess()
        {
            // Get User using UserName
            UserModel userModelResult1 = await userAccountService.GetUserByNameAsync("PeterParker");
            userModelResult1.Should().NotBeNull();
            Guid userIdInput = userModelResult1.Id;

            // Get UserRole by UserId
            IList<string> userRolesResult = await userAccountService.GetUserRolesAsync(userIdInput);
            userRolesResult.Should().NotBeNullOrEmpty();
        }

        [Fact]
        [Trait("Category", "GetErrorCase")]
        public async Task GetUserRolesAsync_NoRecordsFound_ReturnsEmptyResult()
        {
            // Get User using UserName
            Guid userIdInput = Guid.NewGuid();
            // Get UserRole by UserId
            await FluentActions
                .Invoking(() => userAccountService.GetUserRolesAsync(userIdInput))
                .Should().ThrowExactlyAsync<Exception>()
                .WithMessage(resourceString.GetResourceString(ApiErrorCodes.InvalidUserId));
        }

        [Fact]
        [Trait("Category", "GetSuccessCase")]
        public async Task GetUsersInRoleAsync_Success()
        {
            // AddUser for Role.
            UserModel userModelResult = await userAccountService.GetUserByNameAsync("SuperMan");
            userModelResult.Should().NotBeNull();
            // Get Role by name
            RoleModel roleModelResult = await roleService.GetRoleAsync(RoleName);
            roleModelResult.Should().NotBeNull();
            // Arrange for UserRoleModel Input
            UserRoleModel userRoleModelInput = RoleHelper.CreateUserRoleModel();
            userRoleModelInput.UserId = userModelResult.Id;
            userRoleModelInput.RoleId = roleModelResult.Id;
            userRoleModelInput.CreatedBy = "Rosh";
            // Act : AddUserRoleAsync
            FrameworkResult addUserRoleResult = await userAccountService.AddUserRoleAsync(userRoleModelInput);
            addUserRoleResult.Status.Should().Be(ResultStatus.Success);
            // Get Users in Role List.
            IList<UserModel> getUsersInRoleResult = await userAccountService.GetUsersInRoleAsync(RoleName);
            getUsersInRoleResult.Should().NotBeNullOrEmpty();
            // Assert the added user.
            getUsersInRoleResult.Any(_ => _.Id.Equals(userRoleModelInput.UserId)).Should().BeTrue();
        }

        [Fact]
        [Trait("Category", "GetErrorCase")]
        public async Task GetUsersInRoleAsync_NoRecordsFound_ReturnsEmptyResult()
        {
            // Arrange role which does not have any users associated.
            string roleName = "TestRoleName";
            // Get Users in Role List.
            IList<UserModel> getUsersInRoleResult = await userAccountService.GetUsersInRoleAsync(roleName);
            getUsersInRoleResult.Should().BeNullOrEmpty();
        }

        #endregion
        private async Task<UserModel> CreateUserWithModel()
        {
            Random random = new Random();
            // Getting create User Model.
            var userModelInput = UserHelper.CreateUserRequestModel();
            userModelInput.UserName = userModelInput.UserName + random.Next();
            userModelInput.FirstName = userModelInput.UserName.ToUpper();
            userModelInput.LastName = userModelInput.UserName.ToLower();
            userModelInput.Email = string.Concat(userModelInput.UserName, "@", "HCL.CS.SF.com");
            userModelInput.CreatedBy = "Suresh";
            // Getting Security Questions
            var securityQuestionResult = await userAccountService.GetAllSecurityQuestionsAsync();
            // Adding the first Question Id to the User.
            if (securityQuestionResult != null && securityQuestionResult.Count > 0)
            {
                userModelInput.UserSecurityQuestion[0].SecurityQuestionId = securityQuestionResult[0].Id;
                userModelInput.UserSecurityQuestion[0].Answer = "Dwayne Johnson";
            }

            // Adding User.
            FrameworkResult result = await userAccountService.RegisterUserAsync(userModelInput);
            // Asserting the add is successful.
            result.Should().BeOfType<FrameworkResult>();
            result.Status.Should().Be(ResultStatus.Success);

            // Getting added user details
            UserModel addedUserModel = await userAccountService.GetUserByNameAsync(userModelInput.UserName);
            addedUserModel.Should().NotBeNull();

            return addedUserModel;
        }
    }
}



