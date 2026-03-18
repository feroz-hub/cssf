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
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Enums;
using HCL.CS.SF.Domain.ErrorCodes;
using HCL.CS.SF.Domain.Models;
using HCL.CS.SF.Service.Interfaces;
using HCL.CS.SF.TestApp.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HCL.CS.SF.IntegrationTests.Api
{
    public class RoleServiceTests : HCLCSSFFakeSetup
    {
        private const string RoleName = "SystemAdmin";
        private const string CreatedByTestData = "Master";
        private const int DefaultPagesPerItem = 20;
        private const string Roles = "Roles";
        private const string RoleClaims = "RoleClaims";
        private readonly IRoleService roleService;
        private readonly IAuditTrailService auditTrailService;
        private readonly Random random;

        public RoleServiceTests()
        {
            roleService = ServiceProvider.GetService<IRoleService>();
            auditTrailService = ServiceProvider.GetService<IAuditTrailService>();
            random = new Random();
        }

        #region SuccessCases

        [Fact]
        [Trait("Category", "AddSuccessCase")]
        public async Task CreateRole_Success()
        {
            // Add/Create Role
            RoleModel roleModelInput = RoleHelper.CreateRoleModel();
            string randomNumberString = random.Next().ToString();
            roleModelInput.Name = string.Concat(RoleName, "_", randomNumberString);
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

            // Get Audit Trail
            await RolesAuditTrailEntryCheck(roleModelResult, AuditType.Create);
        }

        [Fact]
        [Trait("Category", "AddSuccessCase")]
        public async Task CreateRoleAsync_RoleClaimsNullEntryCheck_Success()
        {
            // Add/Create Role
            RoleModel roleModelInput = RoleHelper.CreateRoleModel();
            string randomNumberString = random.Next().ToString();
            roleModelInput.Name = "RoleWithNoClaims" + random.Next();
            roleModelInput.CreatedBy = string.Concat(CreatedByTestData, "_", randomNumberString);
            // Test Scenario
            roleModelInput.RoleClaims = null;
            // Add Act and Assert
            FrameworkResult result = await roleService.CreateRoleAsync(roleModelInput);
            result.Should().BeOfType<FrameworkResult>();
            result.Status.Should().Be(ResultStatus.Success);

            // GetRoleAsync for name
            RoleModel roleModelResult = await roleService.GetRoleAsync(roleModelInput.Name);
            roleModelResult.Should().NotBeNull();
            roleModelResult.Id.Should().NotBe(Guid.NewGuid());
            roleModelResult.Name.Should().BeEquivalentTo(roleModelInput.Name);
            roleModelInput.RoleClaims.Should().BeNull();

            // Get Audit Trail
            await RolesAuditTrailEntryCheck(roleModelResult, AuditType.Create);
        }

        [Fact]
        [Trait("Category", "AddSuccessCase")]
        public async Task AddRoleClaimAsync_ByClaimModel_Success()
        {
            // Get Role by name
            RoleModel roleModelResult = await roleService.GetRoleAsync(RoleName);
            roleModelResult.Should().NotBeNull();

            // Add RoleClaims by model
            RoleClaimModel addRoleClaimModelInput = RoleHelper.CreateRoleClaimModel();
            addRoleClaimModelInput.RoleId = roleModelResult.Id;
            string randomString = random.Next().ToString();
            addRoleClaimModelInput.ClaimType = string.Concat("Access", "_", randomString);
            addRoleClaimModelInput.ClaimValue = string.Concat("Access.Role", "_", randomString);
            addRoleClaimModelInput.CreatedBy = string.Concat(CreatedByTestData, "_", random.Next().ToString());
            FrameworkResult addRoleClaimResult = await roleService.AddRoleClaimAsync(addRoleClaimModelInput);
            addRoleClaimResult.Should().BeOfType<FrameworkResult>();
            addRoleClaimResult.Status.Should().Be(ResultStatus.Success);

            // Get Role
            RoleModel roleModelResultAfterAddClaim = await roleService.GetRoleAsync(RoleName);
            roleModelResultAfterAddClaim.Should().NotBeNull();
            roleModelResultAfterAddClaim.RoleClaims.Any(roleClaim => roleClaim.CreatedBy.Contains(addRoleClaimModelInput.CreatedBy)).Should().BeTrue();
            roleModelResultAfterAddClaim.RoleClaims.Any(roleclaim => roleclaim.ClaimType.Contains(addRoleClaimModelInput.ClaimType)).Should().BeTrue();
            roleModelResultAfterAddClaim.RoleClaims.Any(roleclaim => roleclaim.ClaimValue.Contains(addRoleClaimModelInput.ClaimValue)).Should().BeTrue();

            // Asserting Audit Entry
            await RoleClaimAuditTrailCheck(roleModelResult.Id, addRoleClaimModelInput, AuditType.Create);
        }

        [Fact]
        [Trait("Category", "AddSuccessCase")]
        public async Task AddRoleClaimAsync_ByClaimModelList_Success()
        {
            // Get Role by name
            RoleModel roleModelResult = await roleService.GetRoleAsync(RoleName);
            roleModelResult.Should().NotBeNull();

            // Add RoleClaims by modellist.
            List<RoleClaimModel> roleClaimModelInputList = RoleHelper.CreateRoleClaimModel_LabTechnician_2();
            string roleClaimCreatedBy = "Test";
            string randomString = random.Next().ToString();
            foreach (var roleClaim in roleClaimModelInputList)
            {
                roleClaim.RoleId = roleModelResult.Id;
                roleClaim.CreatedBy = roleClaimCreatedBy;
                roleClaim.ClaimType = string.Concat(roleClaim.ClaimType, "_", randomString);
                roleClaim.ClaimValue = string.Concat(roleClaim.ClaimValue, "_", randomString);
            }

            FrameworkResult addRoleClaimsResult = await roleService.AddRoleClaimsAsync(roleClaimModelInputList);
            addRoleClaimsResult.Should().BeOfType<FrameworkResult>();
            addRoleClaimsResult.Status.Should().Be(ResultStatus.Success);

            // storing the count
            int roleClaimModelInputListCount = roleClaimModelInputList.Count;

            // Checking if the list models are inserted properly.
            RoleModel roleModelResultAfterAddClaim = await roleService.GetRoleAsync(RoleName);
            roleModelResultAfterAddClaim.Should().NotBeNull();
            var commonRoleClaim = roleModelResultAfterAddClaim
                                                        .RoleClaims
                                                        .Where(x => roleClaimModelInputList.Any(y => x.ClaimType == y.ClaimType && x.ClaimValue == y.ClaimValue)).ToList();
            int countInResult = commonRoleClaim.Count;
            Assert.Equal(roleClaimModelInputListCount, countInResult);

            // Checking the audit trail
        }

        [Fact]
        [Trait("Category", "UpdateConcurrencyCase")]
        public async Task UpdateRole_ByAddingANewRole()
        {
            // Add Role
            RoleModel roleModelInput = RoleHelper.CreateRoleModel();
            string randomNumberString = random.Next().ToString();
            roleModelInput.Name = string.Concat(RoleName, "_", randomNumberString);
            roleModelInput.CreatedBy = string.Concat(CreatedByTestData, "_", randomNumberString);
            foreach (var roleClaim in roleModelInput.RoleClaims)
            {
                roleClaim.CreatedBy = roleModelInput.CreatedBy;
            }

            FrameworkResult createRoleResult = await roleService.CreateRoleAsync(roleModelInput);
            createRoleResult.Should().BeOfType<FrameworkResult>();
            createRoleResult.Status.Should().Be(ResultStatus.Success);

            // Get Role by RoleName
            RoleModel updateRoleModelInput = await roleService.GetRoleAsync(roleModelInput.Name);
            updateRoleModelInput.Should().NotBeNull();

            // Update Properties and Call UpdateRoleAsync API
            updateRoleModelInput.Description = "Gets Some Work";
            updateRoleModelInput.RoleClaims[0].ClaimType = string.Concat("RoleClaimType", "_", randomNumberString);
            updateRoleModelInput.RoleClaims[0].ClaimValue = string.Concat("RoleClaimValue", "_", randomNumberString);
            FrameworkResult result = await roleService.UpdateRoleAsync(updateRoleModelInput);
            result.Should().BeOfType<FrameworkResult>();
            result.Status.Should().Be(ResultStatus.Success);

            // Get Role by RoleName
            RoleModel updatedRoleModelResult = await roleService.GetRoleAsync(updateRoleModelInput.Name);
            updatedRoleModelResult.Should().NotBeNull();

            var resultroleclaimType = updatedRoleModelResult.RoleClaims.Where(i => i.ClaimType == updateRoleModelInput.RoleClaims[0].ClaimType);
            resultroleclaimType.Should().NotBeNullOrEmpty();
            // Check the updated values.
            updatedRoleModelResult.Description.Should().BeEquivalentTo(updateRoleModelInput.Description);
        }

        [Fact]
        [Trait("Category", "UpdateConcurrencyCase")]
        public async Task UpdateRole_UsingExisting()
        {
            // Add Role
            RoleModel roleModelInput = RoleHelper.CreateRoleModel();
            string randomNumberString = random.Next().ToString();
            roleModelInput.Name = string.Concat(RoleName, "_", randomNumberString);
            roleModelInput.CreatedBy = string.Concat(CreatedByTestData, "_", randomNumberString);
            foreach (var roleClaim in roleModelInput.RoleClaims)
            {
                roleClaim.CreatedBy = roleModelInput.CreatedBy;
            }

            FrameworkResult createRoleResult = await roleService.CreateRoleAsync(roleModelInput);
            createRoleResult.Should().BeOfType<FrameworkResult>();
            createRoleResult.Status.Should().Be(ResultStatus.Success);
            // Get Role by RoleName
            RoleModel updateRoleModelInput = await roleService.GetRoleAsync(roleModelInput.Name);
            updateRoleModelInput.Should().NotBeNull();

            // Update Properties and Call UpdateRoleAsync API
            updateRoleModelInput.Description = "Gets the work done";
            updateRoleModelInput.RoleClaims[0].ClaimType = string.Concat("RoleClaimType", "_", randomNumberString);
            updateRoleModelInput.RoleClaims[0].ClaimValue = string.Concat("RoleClaimValue", "_", randomNumberString);
            FrameworkResult result = await roleService.UpdateRoleAsync(updateRoleModelInput);
            result.Should().BeOfType<FrameworkResult>();
            result.Status.Should().Be(ResultStatus.Success);

            // Get Role by RoleName
            RoleModel updatedRoleModelResult = await roleService.GetRoleAsync(updateRoleModelInput.Name);
            updatedRoleModelResult.Should().NotBeNull();

            var resultroleclaimType = updatedRoleModelResult.RoleClaims.Where(i => i.ClaimType == updateRoleModelInput.RoleClaims[0].ClaimType);
            resultroleclaimType.Should().NotBeNullOrEmpty();

            // Check the updated values.
            updatedRoleModelResult.Description.Should().BeEquivalentTo(updateRoleModelInput.Description);
        }

        [Fact]
        [Trait("Category", "DeleteSuccessCase")]
        public async Task DeleteRole_ByRoleName__Success_NewRoleAdded()
        {
            // Add Role
            RoleModel roleModelInput = RoleHelper.CreateRoleModel();
            string randomNumberString = random.Next().ToString();
            roleModelInput.Name = string.Concat(RoleName, "_", randomNumberString);
            roleModelInput.CreatedBy = string.Concat(CreatedByTestData, "_", randomNumberString);
            foreach (var roleClaim in roleModelInput.RoleClaims)
            {
                roleClaim.CreatedBy = roleModelInput.CreatedBy;
            }

            FrameworkResult createRoleResult = await roleService.CreateRoleAsync(roleModelInput);
            createRoleResult.Should().BeOfType<FrameworkResult>();
            createRoleResult.Status.Should().Be(ResultStatus.Success);

            // Get Role by RoleName
            RoleModel roleModelResult = await roleService.GetRoleAsync(roleModelInput.Name);
            roleModelResult.Should().NotBeNull();

            // Delete Role By Name
            FrameworkResult result = await roleService.DeleteRoleAsync(roleModelResult.Name);
            result.Should().BeOfType<FrameworkResult>();
            result.Status.Should().Be(ResultStatus.Success);

            // Check Audit Trial
            await RolesAuditTrailEntryCheck(roleModelResult, AuditType.Delete);
        }

        [Fact]
        [Trait("Category", "DeleteSuccessCase")]
        public async Task DeleteRole_ByRoleName__Success_ExistingRoleWithNoClaims()
        {
            // Add Role
            RoleModel roleModelInput = RoleHelper.CreateRoleModel();
            string randomNumberString = random.Next().ToString();
            roleModelInput.Name = string.Concat(RoleName, "_", randomNumberString);
            roleModelInput.CreatedBy = string.Concat(CreatedByTestData, "_", randomNumberString);
            foreach (var roleClaim in roleModelInput.RoleClaims)
            {
                roleClaim.CreatedBy = roleModelInput.CreatedBy;
            }

            FrameworkResult createRoleResult = await roleService.CreateRoleAsync(roleModelInput);
            createRoleResult.Should().BeOfType<FrameworkResult>();
            createRoleResult.Status.Should().Be(ResultStatus.Success);

            // Get Role by RoleName
            string roleName = roleModelInput.Name;
            RoleModel roleModelResult = await roleService.GetRoleAsync(roleName);
            roleModelResult.Should().NotBeNull();

            // Delete Role By Name
            FrameworkResult result = await roleService.DeleteRoleAsync(roleModelResult.Name);
            result.Should().BeOfType<FrameworkResult>();
            result.Status.Should().Be(ResultStatus.Success);

            // Check Audit Trial
            await RolesAuditTrailEntryCheck(roleModelResult, AuditType.Delete);
        }

        [Fact]
        [Trait("Category", "DeleteSuccessCase")]
        public async Task RemoveRoleClaimAsync_ByClaimModel_Success()
        {
            // Get existing Role.
            RoleModel roleModelResult = await roleService.GetRoleAsync(RoleName);
            roleModelResult.Should().NotBeNull();

            // Add Role claim to the existing Role.
            RoleClaimModel roleClaimModelInput = RoleHelper.CreateRoleClaimModel();
            roleClaimModelInput.RoleId = roleModelResult.Id;
            roleClaimModelInput.ClaimType = "Write";
            roleClaimModelInput.ClaimValue = "Write.Email";
            FrameworkResult addRoleClaimModelResult = await roleService.AddRoleClaimAsync(roleClaimModelInput);
            addRoleClaimModelResult.Status.Should().Be(ResultStatus.Success);

            // Remove Added RoleClaim by Model
            FrameworkResult deleteRoleClaimResult = await roleService.RemoveRoleClaimAsync(roleClaimModelInput);
            deleteRoleClaimResult.Status.Should().Be(ResultStatus.Success);

        }

        [Fact]
        [Trait("Category", "DeleteSuccessCase")]
        public async Task RemoveRoleClaimsAsync_ByClaimModelList_Success()
        {
            // Get Role By RoleName
            RoleModel roleModelResult = await roleService.GetRoleAsync(RoleName);
            roleModelResult.Should().NotBeNull();

            // Add RoleClaimList to the existing role.
            List<RoleClaimModel> roleClaimModelInputList = RoleHelper.CreateRoleClaimModel_SystemAdmin();
            foreach (var roleClaim in roleClaimModelInputList)
            {
                roleClaim.RoleId = roleModelResult.Id;
                roleClaim.CreatedBy = roleModelResult.CreatedBy;
            }

            FrameworkResult addRoleClaimsResult = await roleService.AddRoleClaimsAsync(roleClaimModelInputList);
            addRoleClaimsResult.Should().BeOfType<FrameworkResult>();
            addRoleClaimsResult.Status.Should().Be(ResultStatus.Success);

            // Get Role Claim List
            RoleModel roleModelResultPostAdd = await roleService.GetRoleAsync(RoleName);
            roleModelResultPostAdd.Should().NotBeNull();
            // Getting the common values in the two lists.
            var commonRoleClaim = roleModelResultPostAdd.RoleClaims
                                  .Where(x => roleClaimModelInputList.Any(y => x.ClaimType == y.ClaimType && x.ClaimValue == y.ClaimValue));
            List<RoleClaimModel> roleClaimList = commonRoleClaim.ToList();

            // Add RoleClaim List to the existing role.
            FrameworkResult deleteRoleClaimListResult = await roleService.RemoveRoleClaimsAsync(roleClaimList);
            deleteRoleClaimListResult.Status.Should().Be(ResultStatus.Success);
            // Delete Audit Check
        }

        [Fact]
        [Trait("Category", "GetSuccessCase")]
        public async Task GetRoleAsync_ByRoleId_Success()
        {
            // Add Role
            RoleModel roleModelInput = RoleHelper.CreateRoleModel();
            string randomNumberString = random.Next().ToString();
            roleModelInput.Name = string.Concat(RoleName, "_", randomNumberString);
            roleModelInput.CreatedBy = string.Concat(CreatedByTestData, "_", randomNumberString);
            foreach (var roleClaim in roleModelInput.RoleClaims)
            {
                roleClaim.CreatedBy = roleModelInput.CreatedBy;
            }

            FrameworkResult createRoleResult = await roleService.CreateRoleAsync(roleModelInput);
            createRoleResult.Should().BeOfType<FrameworkResult>();
            createRoleResult.Status.Should().Be(ResultStatus.Success);

            // Get Role by RoleName
            RoleModel roleModelResult = await roleService.GetRoleAsync(roleModelInput.Name);
            roleModelResult.Should().NotBeNull();

            // Act
            RoleModel getRoleModelByIdResult = await roleService.GetRoleAsync(roleModelResult.Id);
            // Assert
            getRoleModelByIdResult.Should().NotBeNull();
            getRoleModelByIdResult.Id.Should().Be(roleModelResult.Id);
        }

        [Fact]
        [Trait("Category","GetSuccessCase")]
        public async Task GetRoleClaimsAsync_Success()
        {
            // Get Role By Name
            RoleModel getRoleModelResult = await roleService.GetRoleAsync(RoleName);
            getRoleModelResult.Should().NotBeNull();

            // Get Claims for roleModel
            IList<System.Security.Claims.Claim> claimsResult = await roleService.GetRoleClaimAsync(getRoleModelResult);
            claimsResult.Should().NotBeNullOrEmpty();
        }
        #endregion
        #region Error Cases

        [Fact]
        [Trait("Category", "AddErrorCase")]
        public async Task CreateRoleAsync_DuplicateRoleExists_ReturnsError_RoleAlreadyExists()
        {
            // Arrange
            RoleModel addRoleModelInput = RoleHelper.CreateRoleModel();
            addRoleModelInput.Name = RoleName;
            // Act
            FrameworkResult createRoleResult = await roleService.CreateRoleAsync(addRoleModelInput);
            // Assert
            createRoleResult.Should().BeOfType<FrameworkResult>();
            createRoleResult.Status.Should().Be(ResultStatus.Failed);
            createRoleResult.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.RoleAlreadyExists);
        }

        [Fact]
        [Trait("Category", "AddErrorCase")]
        public async Task CreateRoleAsync_NameLengthExceedsLimit_ReturnsError_ThrowsException()
        {
            // Add Role
            // Arrange
            RoleModel addRoleModelInput = RoleHelper.CreateRoleModel();
            addRoleModelInput.Name = "hhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllll";
            // Act
            FrameworkResult createRoleResult = await roleService.CreateRoleAsync(addRoleModelInput);
            // Assert
            createRoleResult.Should().BeOfType<FrameworkResult>();
            createRoleResult.Status.Should().Be(ResultStatus.Failed);
            createRoleResult.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.RoleNameTooLong);
        }

        [Fact]
        [Trait("Category", "AddErrorCase")]
        public async Task AddRoleClaimAsync_RecordsFound_ForRoleIdAndRoleClaim_ReturnsError()
        {
            // Get Role by name
            RoleModel roleModelResult = await roleService.GetRoleAsync(RoleName);
            roleModelResult.Should().NotBeNull();
            // Adding a claim with the existing claimtype and claimvalue
            RoleClaimModel addRoleClaimModelInput = RoleHelper.CreateRoleClaimModel();
            addRoleClaimModelInput.RoleId = roleModelResult.Id;
            addRoleClaimModelInput.ClaimType = roleModelResult.RoleClaims[0].ClaimType;
            addRoleClaimModelInput.ClaimValue = roleModelResult.RoleClaims[0].ClaimValue;
            addRoleClaimModelInput.CreatedBy = string.Concat(CreatedByTestData, "_", random.Next().ToString());

            // Act
            FrameworkResult addRoleClaimResult = await roleService.AddRoleClaimAsync(addRoleClaimModelInput);

            // Assert
            addRoleClaimResult.Should().BeOfType<FrameworkResult>();
            addRoleClaimResult.Status.Should().Be(ResultStatus.Failed);
            addRoleClaimResult.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.RoleClaimExists);
        }

        [Fact]
        [Trait("Category", "DeleteErrorCase")]
        public async Task RemoveRoleClaimAsync_ByClaimModel_NoRecordsFound_ReturnsError()
        {
            // Arrange
            Guid roleId = Guid.NewGuid();
            RoleClaimModel roleClaimModelInput = RoleHelper.CreateRoleClaimModel();
            roleClaimModelInput.RoleId = roleId;
            roleClaimModelInput.ClaimType = "Delete";
            roleClaimModelInput.ClaimValue = "Delete.Role";

            // Act
            FrameworkResult deleteRoleClaimModel = await roleService.RemoveRoleClaimAsync(roleClaimModelInput);

            // Assert
            deleteRoleClaimModel.Status.Should().Be(ResultStatus.Failed);
            deleteRoleClaimModel.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.RoleClaimNotExists);

        }

        [Fact]
        [Trait("Category", "DeleteErrorCase")]
        public async Task RemoveRoleClaimsAsync_ByClaimModelList_NoRecordsFound_ReturnsError()
        {
            // Arrange
            List<RoleClaimModel> roleClaimModelsInput = RoleHelper.CreateRoleClaimModel_LabTechnician_2();
            roleClaimModelsInput[0].ClaimType = "Delete";
            roleClaimModelsInput[0].ClaimValue = "Delete.Role";
            foreach (var roleClaim in roleClaimModelsInput)
            {
                roleClaim.RoleId = Guid.NewGuid();
            }

            // Act
            FrameworkResult deleteRoleClaimResult = await roleService.RemoveRoleClaimsAsync(roleClaimModelsInput);

            // Assert
            deleteRoleClaimResult.Status.Should().Be(ResultStatus.Failed);
            deleteRoleClaimResult.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.RoleClaimNotExists);

        }
        #endregion

        private async Task RolesAuditTrailEntryCheck(RoleModel roleModelResult, AuditType auditType)
        {
            DateTime fromDate = DateTime.Today;
            DateTime toDate = DateTime.Today.AddDays(1);
            string roleCreatedModifedBy = roleModelResult.CreatedBy;
            Guid roleId = roleModelResult.Id;
            PagingModel page = new PagingModel()
            {
                CurrentPage = 1,
                ItemsPerPage = 1000,
            };
            // Audit Type => None:0, Create/Add:1, Update:2, Delete:3
            AuditResponseModel auditResponseModelResult = await auditTrailService.GetAuditDetailsAsync(roleCreatedModifedBy, auditType, fromDate, toDate, page);
            auditResponseModelResult.Should().NotBeNull();

            //var role = auditResponseModelResult.AuditList.FindAll(i => i.NewValue.Contains(roleModelResult.Name));
            //role.Should().NotBeNull();

            foreach (var auditTrail in auditResponseModelResult.AuditList)
            {
                if (auditTrail.TableName.Equals(Roles))
                {
                    switch (auditType)
                    {
                        case AuditType.Create:
                            auditTrail.NewValue.Should().NotBeNullOrWhiteSpace();
                            auditTrail.OldValue.Should().BeNull();
                            var addRoleTrial = JsonSerializer.Deserialize<RoleModel>(auditTrail.NewValue);
                            var datetime = roleModelResult.CreatedOn;
                            if (datetime < auditTrail.CreatedOn && auditTrail.CreatedOn < datetime.AddMinutes(1))
                            {
                                Assert.Equal(roleModelResult.Name, addRoleTrial.Name);
                            }

                            break;
                        case AuditType.Update:

                            break;
                        case AuditType.Delete:
                            auditTrail.OldValue.Should().NotBeNullOrWhiteSpace();
                            auditTrail.NewValue.Should().BeNull();
                            RoleModel deleteRoleTrial = JsonSerializer.Deserialize<RoleModel>(auditTrail.OldValue);
                            if (deleteRoleTrial != null)
                            {
                                deleteRoleTrial.Name.Should().BeEquivalentTo(roleModelResult.Name);
                            }

                            break;
                        default:
                            break;
                    }
                }

                if (auditTrail.TableName.Equals(RoleClaims))
                {
                    switch (auditType)
                    {
                        case AuditType.Create:
                            auditTrail.NewValue.Should().NotBeNullOrWhiteSpace();
                            auditTrail.OldValue.Should().BeNull();
                            RoleClaimModel addRoleClaimTrail = JsonSerializer.Deserialize<RoleClaimModel>(auditTrail.NewValue);
                            var datetime = roleModelResult.CreatedOn;
                            if (datetime < auditTrail.CreatedOn && auditTrail.CreatedOn < datetime.AddSeconds(1))
                            {
                                addRoleClaimTrail.RoleId.Should().Be(roleId);
                            }

                            break;
                        case AuditType.Update:

                            break;
                        case AuditType.Delete:
                            auditTrail.OldValue.Should().NotBeNullOrWhiteSpace();
                            auditTrail.NewValue.Should().BeNull();
                            RoleClaimModel deleteRoleClaimTrail = JsonSerializer.Deserialize<RoleClaimModel>(auditTrail.OldValue);
                            if (deleteRoleClaimTrail != null)
                            {
                                deleteRoleClaimTrail.RoleId.Should().Be(roleId);
                            }

                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private async Task RoleClaimAuditTrailCheck(Guid roleId, RoleClaimModel addRoleClaimModelInput, AuditType auditType)
        {
            DateTime fromDate = DateTime.Today;
            DateTime toDate = DateTime.Today.AddDays(1);
            string roleClaimCreatedModifedBy = addRoleClaimModelInput.CreatedBy;
            PagingModel page = new PagingModel()
            {
                CurrentPage = 1,
                ItemsPerPage = 10,
            };

            AuditResponseModel auditResponseModelResult = await auditTrailService.GetAuditDetailsAsync(roleClaimCreatedModifedBy,auditType, fromDate, toDate, page);
            auditResponseModelResult.Should().NotBeNull();

            foreach (var auditTrail in auditResponseModelResult.AuditList)
            {
                if (auditTrail.TableName.Equals(RoleClaims))
                {
                    auditTrail.ActionType.Should().Be(Domain.Enums.AuditType.Create);
                    RoleClaimModel roleClaimTrail = JsonSerializer.Deserialize<RoleClaimModel>(auditTrail.NewValue);
                    if (roleClaimTrail != null)
                    {
                        roleClaimTrail.RoleId.Should().Be(roleId);
                        roleClaimTrail.CreatedBy.Should().Be(addRoleClaimModelInput.CreatedBy);
                        roleClaimTrail.ClaimType.Should().Be(addRoleClaimModelInput.ClaimType);
                        roleClaimTrail.ClaimValue.Should().Be(addRoleClaimModelInput.ClaimValue);
                    }
                }
            }
        }
    }
}



