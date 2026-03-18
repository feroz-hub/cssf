/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

// * Copyright (c) 2021 HCL.CS.SF CORPORATION.
// * All rights reserved. HCL.CS.SF source code is an unpublished work and the use of a copyright notice does not imply otherwise.
// * This source code contains confidential, trade secret material of HCL.CS.SF. Any attempt or participation in deciphering,
// * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
// * HCL.CS.SF is obtained. This is proprietary and confidential to HCL.CS.SF.
// */

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Security.Claims;
//using System.Threading;
//using System.Threading.Tasks;
//using AutoMapper;
//using FluentAssertions;
//using HCL.CS.SF.Domain;
//using HCL.CS.SF.Domain.Entities;
//using HCL.CS.SF.Domain.Enums;
//using HCL.CS.SF.Domain.ErrorCodes;
//using HCL.CS.SF.Domain.Models;
//using HCL.CS.SF.DomainServices.UnitOfWork;
//using HCL.CS.SF.Service.Implementation;
//using HCL.CS.SF.Service.Interfaces;
//using HCL.CS.SF.TestApp.Helpers;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.Extensions.DependencyInjection;
//using Xunit;

//namespace HCL.CS.SF.IntegrationTests.Api
//{
//    public class UserAccountServiceTests : HCLCSSFFakeSetup
//    {
//        private const string UserNameForUpdateAndGet = "PeterParker";
//        private readonly AuditCheck auditCheck;
//        private readonly IUserAccountService userAccountService;
//        private readonly IAuthenticationService authenticationService;
//        private readonly IMapper mapper;
//        private readonly Argon2PasswordHasherWrapper<Users> argon2PasswordHasherWrapper;
//        Random random;
//        private readonly UserManagerWrapper<Users> userManager;
//        private readonly IUserManagementUnitOfWork userManagementUnitOfWork;

//        /// <summary>
//        /// Initializes a new instance of the <see cref="UserAccountServiceTests"/> class.
//        /// </summary>
//        public UserAccountServiceTests()
//        {
//            userAccountService = ServiceProvider.GetService<IUserAccountService>();
//            mapper = ServiceProvider.GetService<IMapper>();
//            auditCheck = new AuditCheck();
//            argon2PasswordHasherWrapper = new Argon2PasswordHasherWrapper<Users>();
//            authenticationService = ServiceProvider.GetService<IAuthenticationService>();
//            random = new Random();
//        }

//        /// <summary>
//        /// Register default HCL user.
//        /// </summary>
//        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
//        [Fact]
//        [Trait("Category", "InitialUserRunSuccess")]
//        public async Task RegisterUser_InitialUserRecord()
//        {
//            // Getting create User Model.
//            var userModelInput = UserHelper.CreateUserRequestModel();
//            userModelInput.UserName = UserNameForUpdateAndGet;
//            userModelInput.CreatedBy = "Rosh";
//            userModelInput.Email = "roshan.bashyam@HCL.CS.SF.com";
//            // Create security question
//            var securityQuestion = UserHelper.CreateSecurityQuestionModel();
//            securityQuestion.CreatedOn = DateTime.UtcNow;
//            securityQuestion.CreatedBy = "Rosh";
//            securityQuestion.Question = "What is your age ?" + string.Empty + random.Next() + string.Empty;
//            FrameworkResult result = await userAccountService.AddSecurityQuestionAsync(securityQuestion);
//            // Assert
//            result.Should().BeOfType<FrameworkResult>();
//            result.Status.Should().Be(ResultStatus.Success);
//            // Getting Security Questions
//            var securityQuestionResult = await userAccountService.GetAllSecurityQuestionsAsync();
//            // Adding the first Question Id to the User.
//            if (securityQuestionResult != null && securityQuestionResult.Count > 0)
//            {
//                userModelInput.UserSecurityQuestion[0].SecurityQuestionId = securityQuestionResult[0].Id;
//                userModelInput.UserSecurityQuestion[0].Answer = "Nineteen";
//            }

//            // Adding User.
//            FrameworkResult registerUserResult = await userAccountService.RegisterUserAsync(userModelInput);
//            // Asserting the add is successful.
//            registerUserResult.Should().BeOfType<FrameworkResult>();
//            registerUserResult.Status.Should().Be(ResultStatus.Success);

//            // Getting added user details
//            UserModel addedUserModel = await userAccountService.GetUserByNameAsync(userModelInput.UserName);
//            addedUserModel.Should().NotBeNull();
//            DateTime createdOnDate = addedUserModel.CreatedOn;
//            Guid addedUserId = addedUserModel.Id;

//            // User email Verification link generate
//            HCL.CS.SF.Domain.GlobalConfiguration.IsEmailConfigurationValid = true;
//            FrameworkResult generateEmailResult = await userAccountService.GenerateEmailConfirmationTokenAsync(addedUserId);
//            generateEmailResult.Status.Should().Be(ResultStatus.Success);

//            // Asserting audit entry.
//            AuditResponseModel auditResponseModel = await auditCheck.GetAudit(userModelInput.CreatedBy, AuditType.Create);
//            auditResponseModel.Should().NotBeNull();
//            // Filtering out the result to contain only User Add records.
//            List<AuditTrailModel> addUserAuditList = auditResponseModel
//                                                            .AuditList
//                                                            .Select(auditTrail => auditTrail)
//                                                            .Where(
//                                                             auditrail => auditrail.TableName.Equals("Users")
//                                                            && auditrail.ActionType.Equals(AuditType.Create)
//                                                            && createdOnDate < auditrail.CreatedOn
//                                                            && auditrail.CreatedOn < createdOnDate.AddMinutes(1)).ToList();
//            // Asserting the added valued check.
//            addUserAuditList.Any(_ => _.NewValue.Contains(addedUserModel.UserName)).Should().BeTrue();
//        }

//        /// <summary>
//        /// Verify email token.
//        /// </summary>
//        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
//        [Fact]
//        [Trait("Category", "SuccessCase")]
//        public async Task Verify_Email_Token_Success()
//        {
//            // Get User by UserName
//            UserModel userModelToVerify = await userAccountService.GetUserByNameAsync(UserNameForUpdateAndGet);
//            userModelToVerify.Should().NotBeNull();
//            Guid userIdToVerify = userModelToVerify.Id;
//            string usrEmailToken = "MTkxNzgw";

//            // Verify UsrEmailToken
//            FrameworkResult verifyEmailTokenResult = await userAccountService.VerifyEmailConfirmationTokenAsync(userIdToVerify, usrEmailToken);
//            verifyEmailTokenResult.Status.Should().Be(ResultStatus.Success);
//        }

//        /// <summary>
//        /// Register Error Case : When UserName is not unique.
//        /// </summary>
//        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
//        [Fact]
//        [Trait("Category", "Add/Register User TestCases")]
//        public async Task RegisterUser_User_Name_Already_Exists_Returns_Error()
//        {
//            // adding user with existing username
//            string existingUserName = UserNameForUpdateAndGet;
//            var userModelInput = UserHelper.CreateUserRequestModel();
//            userModelInput.UserName = existingUserName;
//            userModelInput.CreatedBy = "Rosh";
//            // Getting Security Questions
//            var securityQuestionResult = await userAccountService.GetAllSecurityQuestionsAsync();
//            // Adding the first Question Id to the User.
//            if (securityQuestionResult != null && securityQuestionResult.Count > 0)
//            {
//                userModelInput.UserSecurityQuestion[0].SecurityQuestionId = securityQuestionResult[0].Id;
//                userModelInput.UserSecurityQuestion[0].Answer = "Nineteen";
//            }

//            // Adding User.
//            FrameworkResult registerUserResult = await userAccountService.RegisterUserAsync(userModelInput);
//            // Asserting the add is Not Successful and returns a error.
//            registerUserResult.Should().BeOfType<FrameworkResult>();
//            registerUserResult.Status.Should().Be(ResultStatus.Failed);
//            registerUserResult.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.UserAlreadyExists);
//        }

//        /// <summary>
//        /// Register Error Case : When Email address is not unique.
//        /// </summary>
//        /// <returns></returns>
//        [Fact]
//        [Trait("Category", "Add/Register User TestCases")]
//        public async Task RegisterUser_Email_Already_Taken_Returns_Error()
//        {
//            // adding user with existing username
//            var userModelInput = UserHelper.CreateUserRequestModel();
//            string randomString = random.Next().ToString();
//            userModelInput.UserName = string.Concat(UserNameForUpdateAndGet, "_", randomString);
//            userModelInput.FirstName = userModelInput.UserName.ToUpper();
//            userModelInput.LastName = userModelInput.UserName.ToLower();
//            userModelInput.CreatedBy = "Rosh";
//            userModelInput.Email = "roshan.bashyam@HCL.CS.SF.com";
//            // Getting Security Questions
//            var securityQuestionResult = await userAccountService.GetAllSecurityQuestionsAsync();
//            // Adding the first Question Id to the User.
//            if (securityQuestionResult != null && securityQuestionResult.Count > 0)
//            {
//                userModelInput.UserSecurityQuestion[0].SecurityQuestionId = securityQuestionResult[0].Id;
//                userModelInput.UserSecurityQuestion[0].Answer = "Nineteen";
//            }

//            // Adding User.
//            FrameworkResult registerUserResult = await userAccountService.RegisterUserAsync(userModelInput);
//            // Asserting the add is Not Successful and returns a error.
//            registerUserResult.Should().BeOfType<FrameworkResult>();
//            registerUserResult.Status.Should().Be(ResultStatus.Failed);
//            registerUserResult.Errors.FirstOrDefault().Code.Should().Be("DuplicateEmail");
//           }

//        /// <summary>
//        /// The RegisterUserAsync_InvalidPassword_ReturnsErrorInvalidPassword.
//        /// </summary>
//        /// <param name="password">The password<see cref="string"/>.</param>
//        /// <returns>The <see cref="Task"/>.</returns>
//        [Theory]
//        [InlineData("Test123456")] // No Special Characters
//        [InlineData("TEsT@#!$%")] // No Nummbers
//        [InlineData("Y0.Thisis1supersecretandsuper,Duperlongpassword.Let'$testtheerr0rvalidationofthelongp@$$w0rdwhichi$$uperl0NG")]
//        [InlineData("Test12")] // Short Password
//        [InlineData("test@1989")] // No Upper Case
//        [InlineData("TEST@1989")] // No Lower Case
//        [Trait("Category", "Add/Register User TestCases")]
//        public async Task RegisterUserAsync_Invalid_Password_Format_Entered_ReturnsError(string password)
//        {
//            // adding user with existing username
//            var userModelInput = UserHelper.CreateUserRequestModel();
//            string randomString = random.Next().ToString();
//            userModelInput.UserName = string.Concat(UserNameForUpdateAndGet, "_", randomString);
//            userModelInput.FirstName = userModelInput.UserName.ToUpper();
//            userModelInput.LastName = userModelInput.UserName.ToLower();
//            userModelInput.CreatedBy = "Rosh";
//            userModelInput.Email = string.Concat(userModelInput.UserName, "@", "HCL.CS.SF.com");
//            // Getting Security Questions
//            var securityQuestionResult = await userAccountService.GetAllSecurityQuestionsAsync();
//            // Adding the first Question Id to the User.
//            if (securityQuestionResult != null && securityQuestionResult.Count > 0)
//            {
//                userModelInput.UserSecurityQuestion[0].SecurityQuestionId = securityQuestionResult[0].Id;
//                userModelInput.UserSecurityQuestion[0].Answer = "Nineteen";
//            }

//            // Adding different wrong formats of password
//            userModelInput.Password = password;
//            // Adding User.
//            FrameworkResult registerUserResult = await userAccountService.RegisterUserAsync(userModelInput);
//            // Asserting the add is Not Successful and returns a error.
//            registerUserResult.Should().BeOfType<FrameworkResult>();
//            registerUserResult.Status.Should().Be(ResultStatus.Failed);
//        }

//        /// <summary>
//        /// Update User Error Scenario : Updating with already existing email.
//        /// </summary>
//        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
//        [Fact]
//        [Trait("Category", "Update/Delete User TestCases")]
//        public async Task UpdateUser_With_Existing_Email_Address_Returns_Error()
//        {
//            // Add User
//            UserModel userModelForUpdate = await RegisterUser_RandomName_Generate_Success();
//            // Update Properties
//            userModelForUpdate.Email = "roshan.bashyam@HCL.CS.SF.com";
//            userModelForUpdate.UserClaims[0].ClaimValue = "AuntMay4";
//            userModelForUpdate.UserSecurityQuestion[0].Answer = "Test@123456421";
//            FrameworkResult updateUserResult = await userAccountService.UpdateUserAsync(userModelForUpdate);

//            updateUserResult.Should().BeOfType<FrameworkResult>();
//            updateUserResult.Status.Should().Be(ResultStatus.Failed);
//            updateUserResult.Errors.FirstOrDefault().Code.Should().Be("DuplicateEmail");
//        }

//        /// <summary>
//        /// Update User Error Scenario : Updating with already existing UserName.
//        /// </summary>
//        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
//        [Fact]
//        [Trait("Category", "Update/Delete User TestCases")]
//        public async Task UpdateUser_With_Existing_UserName_Returns_Error()
//        {
//            // Add User
//            UserModel userModelForUpdate = await RegisterUser_RandomName_Generate_Success();
//            // Update Properties
//            userModelForUpdate.UserName = UserNameForUpdateAndGet;
//            userModelForUpdate.UserClaims[0].ClaimValue = "AuntMay4";
//            userModelForUpdate.UserSecurityQuestion[0].Answer = "Test@123456421";
//            FrameworkResult updateUserResult = await userAccountService.UpdateUserAsync(userModelForUpdate);

//            updateUserResult.Should().BeOfType<FrameworkResult>();
//            updateUserResult.Status.Should().Be(ResultStatus.Failed);
//            updateUserResult.Errors.FirstOrDefault().Code.Should().Be("DuplicateUserName");
//        }

//        /// <summary>
//        /// Update User Success Scenario.
//        /// </summary>
//        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
//        [Fact]
//        [Trait("Category", "Update/Delete User TestCases")]
//        public async Task UpdateUser_Success()
//        {
//            // Add User
//            UserModel userModelForUpdate = await RegisterUser_RandomName_Generate_Success();
//            // Update Properties
//            string oldEmailValue = userModelForUpdate.Email;
//            userModelForUpdate.Email = "peterparkerspidyspidy55s@avaengers.com";
//            userModelForUpdate.UserClaims[0].ClaimValue = "AuntMay4";
//            userModelForUpdate.UserSecurityQuestion[0].Answer = "Test@123456421";
//            FrameworkResult updateUserResult = await userAccountService.UpdateUserAsync(userModelForUpdate);

//            updateUserResult.Should().BeOfType<FrameworkResult>();
//            updateUserResult.Status.Should().Be(ResultStatus.Success);

//            // Getting updated user details
//            UserModel updatedUserModel = await userAccountService.GetUserByNameAsync(userModelForUpdate.UserName);
//            updatedUserModel.Should().NotBeNull();
//            // Checking if the values updated properly.
//            updatedUserModel.Email.Should().BeEquivalentTo(userModelForUpdate.Email);
//            updatedUserModel.UserClaims[0].ClaimValue.Should().BeEquivalentTo(userModelForUpdate.UserClaims[0].ClaimValue);
//            DateTime updateModifiedOn = updatedUserModel.ModifiedOn ?? updatedUserModel.CreatedOn;

//            //string argonHashedAnswer = argon2PasswordHasherWrapper.HashPassword(null, userModelForUpdate.UserSecurityQuestion[0].Answer.ToUpper());
//            //string argonHashedAnswer2 = argon2PasswordHasherWrapper.HashPassword(null, "TEST@123456");
//            //argonHashedAnswer2.Should().BeEquivalentTo(argonHashedAnswer);

//            //updatedUserModel.UserSecurityQuestion[0].Answer.Should().BeEquivalentTo(argonHashedAnswer);
//            // Checking the audit entry.
//            AuditResponseModel auditResponseModel = await auditCheck.GetAudit(userModelForUpdate.CreatedBy, AuditType.Update);
//            auditResponseModel.Should().NotBeNull();
//            // Filtering out update details
//            List<AuditTrailModel> updateUserAuditList = auditResponseModel
//                                                           .AuditList
//                                                           .Select(auditTrail => auditTrail)
//                                                           .Where(
//                                                            auditrail => auditrail.TableName.Equals("Users")
//                                                           && auditrail.ActionType.Equals(AuditType.Update)
//                                                           && updateModifiedOn < auditrail.CreatedOn
//                                                           && auditrail.CreatedOn < updateModifiedOn.AddMinutes(1)).ToList();
//            // Asserting the updated values.
//            updateUserAuditList.Any(_ => _.NewValue.Contains(updatedUserModel.Email)).Should().BeTrue();
//            updateUserAuditList.Any(_ => _.OldValue.Contains(oldEmailValue)).Should().BeTrue();
//        }

//        /// <summary>
//        /// Delete user.
//        /// </summary>
//        /// <returns>Delete User.</returns>
//        [Fact]
//        [Trait("Category", "Update/Delete User TestCases")]
//        public async Task DeleteUser_By_Id_Success()
//        {
//            // Add User
//            UserModel userModelInput = await RegisterUser_RandomName_Generate_Success();
//            Guid userIdForDelete = userModelInput.Id;

//            // Delete the User by UserId
//            FrameworkResult deleteUserResult = await userAccountService.DeleteUserAsync(userIdForDelete);
//            deleteUserResult.Status.Should().Be(ResultStatus.Success);

//            // Checking for child tables
//            // UserClaims
//            IList<UserClaimModel> userClaimModelList = await userAccountService.GetUserClaimsAsync(userIdForDelete);
//            userClaimModelList.Should().BeNullOrEmpty();

//            // UserROles.
//            IList<string> userRoles = await userAccountService.GetUserRolesAsync(userIdForDelete);
//            userRoles.Should().BeNullOrEmpty();

//            // userSecurity Questions
//            IList<UserSecurityQuestionModel> userSecurityQuestionModelList = await userAccountService.GetUserSecurityQuestionsAsync(userIdForDelete);
//            userSecurityQuestionModelList.Should().BeNullOrEmpty();

//            // Audit Trail Check.
//            // Checking the audit entry.
//            AuditResponseModel auditResponseModel = await auditCheck.GetAudit(userModelInput.CreatedBy, AuditType.Delete);
//            auditResponseModel.Should().NotBeNull();
//            // Filtering out update details
//            List<AuditTrailModel> deleteUserAuditList = auditResponseModel
//                                                           .AuditList
//                                                           .Select(auditTrail => auditTrail)
//                                                           .Where(
//                                                            auditrail => auditrail.TableName.Equals("Users")
//                                                           && auditrail.ActionType.Equals(AuditType.Delete)
//                                                           ).ToList();
//            deleteUserAuditList.Any(_ => _.OldValue.Contains(userModelInput.Email)).Should().BeTrue();

//            // Login User not possible
//            SignInResponseModel loginResult = await authenticationService.PasswordSignInAsync(userModelInput.UserName, userModelInput.Password);
//            loginResult.Succeeded.Should().BeFalse();
//        }

//        /// <summary>
//        /// Getting User by UserName.
//        /// Getting User by Email.
//        /// Getting User by UserId.
//        /// </summary>
//        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
//        [Fact]
//        [Trait("Category", "GetUserCases TestCases")]
//        public async Task GetUser_ByUserName_ByEmail_ById_Success()
//        {
//            // Get User by UserName
//            UserModel getUserDetailsByUserNameResult = await userAccountService.GetUserByNameAsync(UserNameForUpdateAndGet);
//            getUserDetailsByUserNameResult.Should().NotBeNull();
//            getUserDetailsByUserNameResult.UserName.Should().BeEquivalentTo(UserNameForUpdateAndGet); // Asserting the correct value.

//            // setting values for other checks.
//            string emailId = getUserDetailsByUserNameResult.Email;
//            Guid userID = getUserDetailsByUserNameResult.Id;

//            // Get User by Email
//            UserModel getUserDetailsByEmailResult = await userAccountService.GetUserByEmailAsync(emailId);
//            getUserDetailsByEmailResult.Should().NotBeNull();
//            getUserDetailsByEmailResult.Email.Should().BeEquivalentTo(emailId);

//            // Get User by Id
//            UserModel getUserDetailsByIdResult = await userAccountService.GetUserByIdAsync(userID);
//            getUserDetailsByIdResult.Should().NotBeNull();
//            getUserDetailsByIdResult.Id.Should().Be(userID);
//        }

//        /// <summary>
//        /// Getting User by claims.
//        /// </summary>
//        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
//        [Fact]
//        [Trait("Category", "GetUserCases TestCases")]
//        public async Task GetUser_ByClaims_Success()
//        {
//            //var claim = new Claim("Forward", "Ronaldo");

//            //var users = await userAccountService.GetUsersForClaimAsync(claim);
//            //users.Should().NotBeNull();
//        }

//        /// <summary>
//        /// Check if UserExists SuccessCase.
//        /// </summary>
//        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
//        [Fact]
//        [Trait("Category", "UserAvailibility TestCases")]
//        public async Task IsUserExists_ByUserId_Success()
//        {
//            // Get User by UserName
//            UserModel userDetails = await userAccountService.GetUserByNameAsync(UserNameForUpdateAndGet);
//            userDetails.Should().NotBeNull();
//            Guid userId = userDetails.Id;

//            // IsUser Check by userId
//            bool isUserExists = await userAccountService.IsUserExistsAsync(userId);
//            isUserExists.Should().BeTrue();
//        }

//        /// <summary>
//        /// change password Check.
//        /// </summary>
//        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
//        [Fact]
//        [Trait("Category", "UserProfileEdit TestCases")]
//        public async Task Change_Password_Tests_Success()
//        {
//            // Add User
//            var userModelInput = UserHelper.CreateUserRequestModel();
//            userModelInput.UserName = string.Concat(UserNameForUpdateAndGet, "_", new Random().Next().ToString());
//            userModelInput.Email = string.Concat(userModelInput.UserName, "@", "HCL.CS.SF.com");
//            userModelInput.Password = "Roshan@2022";
//            userModelInput.CreatedBy = "Rosh";
//            // Getting Security Questions9
//            var securityQuestionResult = await userAccountService.GetAllSecurityQuestionsAsync();
//            // Adding the first Question Id to the User.
//            if (securityQuestionResult != null && securityQuestionResult.Count > 0)
//            {
//                userModelInput.UserSecurityQuestion[0].SecurityQuestionId = securityQuestionResult[0].Id;
//                userModelInput.UserSecurityQuestion[0].Answer = "Test";
//            }

//            // Adding User.
//            FrameworkResult registerUserResult = await userAccountService.RegisterUserAsync(userModelInput);
//            // Asserting the add is successful.
//            registerUserResult.Should().BeOfType<FrameworkResult>();
//            registerUserResult.Status.Should().Be(ResultStatus.Success);

//            // Get User by UserName
//            UserModel userDetails = await userAccountService.GetUserByNameAsync(userModelInput.UserName);
//            userDetails.Should().NotBeNull();
//            Guid userId = userDetails.Id;
//            // Change Password.
//            FrameworkResult result = await userAccountService.ChangePasswordAsync(
//                userId,
//                "Roshan@2022",
//                "TestUsers@2023");

//            result.Should().BeOfType<FrameworkResult>();
//            result.Status.Should().Be(ResultStatus.Success);
//        }

//        /// <summary>
//        /// change password Check.
//        /// </summary>
//        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
//        [Fact]
//        [Trait("Category", "UserProfileEdit TestCases")]

//        public async Task Reset_Password_Tests_Success()
//        {
//            // Get User by UserName
//            UserModel userDetails = await userAccountService.GetUserByNameAsync(UserNameForUpdateAndGet);
//            userDetails.Should().NotBeNull();
//            Guid userId = userDetails.Id;
//            // Change Password.
//            FrameworkResult result = await userAccountService.ResetPasswordAsync(
//                userId,
//                "869157",
//                "TestUsers@2025");

//            result.Should().BeOfType<FrameworkResult>();
//            result.Status.Should().Be(ResultStatus.Success);
//        }

//        /// <summary>
//        /// Lock Out User CasesError Case : Invalid UserId given.
//        /// </summary>
//        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
//        [Fact]
//        [Trait("Category", "UserProfileEdit TestCases")]
//        public async Task LockUser_ById_Invalid_UserID_Given_Returns_Error()
//        {
//            Guid userIdForLocking = Guid.NewGuid();
//            // Lock User by Id
//            FrameworkResult result = await userAccountService.LockUserAsync(userIdForLocking);
//            result.Should().BeOfType<FrameworkResult>();
//            result.Status.Should().Be(ResultStatus.Failed);
//            result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.InvalidUserId);

//        }
//        /// <summary>
//        /// Lock Out User Cases.
//        /// </summary>
//        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
//        [Fact]
//        [Trait("Category", "UserProfileEdit TestCases")]
//        public async Task LockUser_ById_NoLockoutEndDateGiven_Success()
//        {
//            // Get User To Update by UserName
//            UserModel userModelForLock = await userAccountService.GetUserByNameAsync(UserNameForUpdateAndGet);
//            userModelForLock.Should().NotBeNull();
//            Guid userIdForLocking = userModelForLock.Id;

//            // Lock User by Id
//            FrameworkResult result = await userAccountService.LockUserAsync(userIdForLocking);
//            result.Should().BeOfType<FrameworkResult>();
//            result.Status.Should().Be(ResultStatus.Success);
//        }

//        /// <summary>
//        /// Lock Out User Cases with LOckOutEnd Date provided.
//        /// </summary>
//        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
//        [Fact]
//        [Trait("Category", "UserProfileEdit TestCases")]
//        public async Task LockUser_ById_LockEndDateGiven_Success()
//        {
//            // Get User To Update by UserName
//            UserModel userModelForLock = await userAccountService.GetUserByNameAsync(UserNameForUpdateAndGet);
//            userModelForLock.Should().NotBeNull();
//            Guid userIdForLocking = userModelForLock.Id;
//            DateTime? lockOutEndDate = DateTime.UtcNow.AddSeconds(25); // User to be locked for 2 minutes.

//            // Lock User by Id
//            FrameworkResult result = await userAccountService.LockUserAsync(userIdForLocking, lockOutEndDate);
//            result.Should().BeOfType<FrameworkResult>();
//            result.Status.Should().Be(ResultStatus.Success);
//            // Login Check
//            string password = "TestUser@2021";

//            SignInResponseModel loginBeforeLockEndTimeResult = await authenticationService.PasswordSignInAsync(userModelForLock.UserName, password);
//            loginBeforeLockEndTimeResult.Succeeded.Should().BeFalse();

//            int miliseconds = 25 * 1000;
//            Thread.Sleep(miliseconds);

//            SignInResponseModel loginAfterLockEndTimeResult = await authenticationService.PasswordSignInAsync(userModelForLock.UserName, password);
//            loginAfterLockEndTimeResult.Succeeded.Should().BeTrue();
//        }

//        /// <summary>
//        /// UnLock Out User CasesError Case : Invalid UserId given.
//        /// </summary>
//        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
//        [Fact]
//        [Trait("Category", "UserProfileEdit TestCases")]
//        public async Task UnLockUser_ById_Invalid_UserID_Given_Returns_Error()
//        {
//            Guid userIdForUnLocking = Guid.NewGuid();
//            // Lock User by Id
//            FrameworkResult result = await userAccountService.UnLockUserAsync(userIdForUnLocking);
//            result.Should().BeOfType<FrameworkResult>();
//            result.Status.Should().Be(ResultStatus.Failed);
//            result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.InvalidUserId);

//        }

//        /// <summary>
//        /// Unlock User Cases with LOckOutEnd Date provided.
//        /// </summary>
//        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
//        [Fact]
//        [Trait("Category", "UserProfileEdit TestCases")]
//        public async Task UnLockUser_ById__Success()
//        {
//            // Get User To Update by UserName
//            UserModel userModelForLock = await userAccountService.GetUserByNameAsync(UserNameForUpdateAndGet);
//            userModelForLock.Should().NotBeNull();
//            Guid userIdForUnLocking = userModelForLock.Id;
//            // Lock User by Id
//            FrameworkResult lockUsrResult = await userAccountService.LockUserAsync(userIdForUnLocking);
//            lockUsrResult.Should().BeOfType<FrameworkResult>();
//            lockUsrResult.Status.Should().Be(ResultStatus.Success);
//            // UnLock User by Id
//            FrameworkResult unLockUserResult = await userAccountService.UnLockUserAsync(userIdForUnLocking);
//            unLockUserResult.Should().BeOfType<FrameworkResult>();
//            unLockUserResult.Status.Should().Be(ResultStatus.Success);
//            // Get User To Update by UserName
//            UserModel unLockedUserModel = await userAccountService.GetUserByNameAsync(UserNameForUpdateAndGet);
//            unLockedUserModel.Should().NotBeNull();

//            string password = "TestUser@2021";
//            SignInResponseModel loginAfterLockEndTimeResult = await authenticationService.PasswordSignInAsync(userModelForLock.UserName, password);
//            loginAfterLockEndTimeResult.Succeeded.Should().BeTrue();
//        }

//        /// <summary>
//        /// Lock Out User Cases with LOckOutEnd Date provided.
//        /// </summary>
//        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
//        [Fact]
//        [Trait("Category", "UserProfileEdit TestCases")]
//        public async Task UnLockUser_By_Id_Token_And_Purpose_Success()
//        {
//            // Get User To Update by UserName
//            UserModel userModelForLock = await userAccountService.GetUserByNameAsync(UserNameForUpdateAndGet);
//            userModelForLock.Should().NotBeNull();
//            Guid userIdForUnLocking = userModelForLock.Id;
//            string token = "NWE0OWQ1OGEtY2FiOC00ZDhmLTgzNDEtNmZkYWQ2YmViNWU4";
//            string purpose = "TestToken";

//            // Lock User by Id
//            FrameworkResult result = await userAccountService.UnLockUserAsync(userIdForUnLocking, token, purpose);
//            result.Should().BeOfType<FrameworkResult>();
//            result.Status.Should().Be(ResultStatus.Success);
//            // Get User To Update by UserName
//            UserModel unLockedUserModel = await userAccountService.GetUserByNameAsync(UserNameForUpdateAndGet);
//            unLockedUserModel.Should().NotBeNull();
//        }

//        /// <summary>
//        /// Lock Out User Cases with LOckOutEnd Date provided.
//        /// </summary>
//        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
//        [Fact]
//        [Trait("Category", "UserProfileEdit TestCases")]
//        public async Task UnLockUser_By_Id_ListOfUserQuestions_Success()
//        {
//            // Get User To Update by UserName
//            UserModel userModelForUnlock = await userAccountService.GetUserByNameAsync(UserNameForUpdateAndGet);
//            userModelForUnlock.Should().NotBeNull();
//            Guid userIdForUnLocking = userModelForUnlock.Id;
//            IList<UserSecurityQuestionModel> userSecurityQuestionList = userModelForUnlock.UserSecurityQuestion;
//            // Lock User by Id
//            FrameworkResult result = await userAccountService.UnlockUserAsync(userIdForUnLocking, userSecurityQuestionList);
//            result.Should().BeOfType<FrameworkResult>();
//            result.Status.Should().Be(ResultStatus.Success);
//            // Get User To Update by UserName
//            UserModel unLockedUserModel = await userAccountService.GetUserByNameAsync(UserNameForUpdateAndGet);
//            unLockedUserModel.Should().NotBeNull();
//            unLockedUserModel.LockoutEnd.Should().BeNull();
//        }

//        [Fact]
//        [Trait("Category", "ErrorCase")]
//        public async Task ArgonHashError()
//        {
//            string argonHashedAnswer = argon2PasswordHasherWrapper.HashPassword(null, UserNameForUpdateAndGet.ToUpper());
//            string argonHashedAnswer2 = argon2PasswordHasherWrapper.HashPassword(null, UserNameForUpdateAndGet.ToUpper());
//            argonHashedAnswer2.Should().BeEquivalentTo(argonHashedAnswer);
//        }
//        /// <summary>
//        /// Register User success method.
//        /// </summary>
//        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
//        [Fact]
//        [Trait("Category", "Add/Register User TestCases")]
//        private async Task<UserModel> RegisterUser_RandomName_Generate_Success()
//        {
//            // Getting create User Model.
//            var userModelInput = UserHelper.CreateUserRequestModel();
//            string randomString = random.Next().ToString();
//            userModelInput.UserName = string.Concat(UserNameForUpdateAndGet, "_", randomString);
//            userModelInput.FirstName = userModelInput.UserName.ToUpper();
//            userModelInput.LastName = userModelInput.UserName.ToLower();
//            userModelInput.Email = string.Concat(userModelInput.UserName, "@", "HCL.CS.SF.com");
//            userModelInput.CreatedBy = "Rosh";
//            // Getting Security Questions
//            var securityQuestionResult = await userAccountService.GetAllSecurityQuestionsAsync();
//            // Adding the first Question Id to the User.
//            if (securityQuestionResult != null && securityQuestionResult.Count > 0)
//            {
//                userModelInput.UserSecurityQuestion[0].SecurityQuestionId = securityQuestionResult[0].Id;
//                userModelInput.UserSecurityQuestion[0].Answer = "Dwayne Johnson";
//            }

//            // Adding User.
//            FrameworkResult result = await userAccountService.RegisterUserAsync(userModelInput);
//            // Asserting the add is successful.
//            result.Should().BeOfType<FrameworkResult>();
//            result.Status.Should().Be(ResultStatus.Success);

//            // Getting added user details
//            UserModel addedUserModel = await userAccountService.GetUserByNameAsync(userModelInput.UserName);
//            addedUserModel.Should().NotBeNull();

//            return addedUserModel;
//        }
//    }
//}


