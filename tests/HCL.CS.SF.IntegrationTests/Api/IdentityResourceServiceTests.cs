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
    public class IdentityResourceServiceTests : HCLCSSFFakeSetup
    {
        private const string ResourceName = "IDResource";

        private readonly IIdentityResourceService identityResourceService;

        public IdentityResourceServiceTests()
        {
            identityResourceService = ServiceProvider.GetService<IIdentityResourceService>();
        }

        [Fact]
        [Trait("Category", "SuccessCase")]
        public async Task<string> AddIdentityResourceAsync_Run()
        {
            var model = IdentityResourceHelper.CreateIdentityResourceModel();
            model.Name = string.Concat(ResourceName, "_", new Random().Next().ToString());
            FrameworkResult result = await identityResourceService.AddIdentityResourceAsync(model);
            result.Should()
                  .BeOfType<FrameworkResult>();
            result.Status.Should().Be(ResultStatus.Success);

            return model.Name;
        }

        [Fact]
        [Trait("Category", "SuccessCase")]
        public async Task AddIdentityResourceAsync_Success()
        {
            try
            {
                var model = IdentityResourceHelper.CreateIdentityResourceModel();
               // Random Name Generating
                Random random = new Random();
                model.Name = string.Concat(ResourceName, "_", random.Next().ToString());
                model.CreatedBy = "Suresh";
                model.ModifiedBy = "Test";
                model.CreatedOn = DateTime.Now;
                model.Enabled = true;
                model.Required = true;
                model.Emphasize = true;
                FrameworkResult result = await identityResourceService.AddIdentityResourceAsync(model);
                result.Should().BeOfType<FrameworkResult>();
                result.Status.Should().Be(ResultStatus.Success);
                // Get IdentityResourceBy Name
                IdentityResourcesModel identityResourcesModel = await identityResourceService.GetIdentityResourceAsync(model.Name);
                identityResourcesModel.Should().NotBeNull();
                identityResourcesModel.Should().BeOfType<IdentityResourcesModel>();
                identityResourcesModel.IdentityClaims.Any().Should().BeTrue();
                identityResourcesModel.IdentityClaims[0].Type.Contains(model.IdentityClaims[0].Type);
                // Check Child Table Count
                identityResourcesModel.IdentityClaims.Count().Equals(model.IdentityClaims.Count());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Fact]
        [Trait("Category", "ErrorCase")]
        public async Task AddIdentityResourceAsync_Nullcheck()
        {
            var model = IdentityResourceHelper.CreateIdentityResourceModel();
            Random random = new Random();
            model.Name = null;
            model.CreatedBy = null;
            model.ModifiedBy = null;

            FrameworkResult result = await identityResourceService.AddIdentityResourceAsync(model);
            result.Should().BeOfType<FrameworkResult>();
            result.Status.Should().Be(ResultStatus.Failed);
            result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.IdentityResourceNameRequired);

            model.Name = string.Concat(ResourceName, "_", random.Next().ToString());
            FrameworkResult result2 = await identityResourceService.AddIdentityResourceAsync(model);
            result2.Should().BeOfType<FrameworkResult>();
            result2.Status.Should().Be(ResultStatus.Success);

            IdentityResourcesModel identityResourcesModel = await identityResourceService.GetIdentityResourceAsync(model.Name);
            identityResourcesModel.Should().NotBeNull();
            identityResourcesModel.CreatedBy.Should().Be("Anonymous");
        }

        [Fact]
        [Trait("Category", "ErrorCase")]
        public async Task AddIdentityResourceAsync_Emptycheck()
        {
            var model = IdentityResourceHelper.CreateIdentityResourceModel();
            Random random = new Random();
            model.Name = string.Empty;
            model.CreatedBy = string.Empty;

            FrameworkResult result = await identityResourceService.AddIdentityResourceAsync(model);
            result.Should().BeOfType<FrameworkResult>();
            result.Status.Should().Be(ResultStatus.Failed);
            result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.IdentityResourceNameRequired);

            model.Name = string.Concat(ResourceName, "_", random.Next().ToString());
            FrameworkResult result2 = await identityResourceService.AddIdentityResourceAsync(model);
            result2.Should().BeOfType<FrameworkResult>();
            result2.Status.Should().Be(ResultStatus.Success);

            IdentityResourcesModel identityResourcesModel = await identityResourceService.GetIdentityResourceAsync(model.Name);
            identityResourcesModel.Should().NotBeNull();
            identityResourcesModel.CreatedBy.Should().Be("Anonymous");
        }

        [Fact]
        [Trait("Category", "ErrorCase")]
        public async Task AddIdentityResourceAsync_Childmodel_Empty()
        {
            var model = IdentityResourceHelper.CreateIdentityResourceModel();
            Random random = new Random();
            model.Name = string.Concat(ResourceName, "_", random.Next().ToString());
            model.IdentityClaims.Clear();

            FrameworkResult result = await identityResourceService.AddIdentityResourceAsync(model);
            result.Should().BeOfType<FrameworkResult>();
            result.Status.Should().Be(ResultStatus.Success);
            IdentityResourcesModel identityResourcesModel = await identityResourceService.GetIdentityResourceAsync(model.Name);
            identityResourcesModel.Id.Should().NotBe(Guid.Empty);
            identityResourcesModel.IdentityClaims.Count.Equals(model.IdentityClaims.Count);
        }

        [Fact]
        [Trait("Category", "ErrorCase")]
        public async Task AddIdentityResourceAsync_ChildModelNull()
        {
            var model = IdentityResourceHelper.CreateIdentityResourceModel();
            Random random = new Random();
            model.Name = string.Concat(ResourceName, "_", random.Next().ToString());
            model.IdentityClaims = null;

            FrameworkResult result = await identityResourceService.AddIdentityResourceAsync(model);
            result.Should().BeOfType<FrameworkResult>();
            result.Status.Should().Be(ResultStatus.Success);
        }

        [Fact]
        [Trait("Category", "ErrorCase")]
        public async Task AddIdentityResourceAsync_InvalidData()
        {
            var model = IdentityResourceHelper.CreateIdentityResourceModel();
            // Random Name Generating
            Random random = new Random();
            model.Name = "<¶§??? type=text/javascript>).ready(function () {{ gtg.core.showInfoBar('{0}', '{1}'); }});</script>" + random.Next() + "";
            model.CreatedBy = "<script type=text/javascript>$(document).ready(function () {{ gtg.core.showInfoBar('{0}', '{1}'); }});</script>";
            model.CreatedOn = Convert.ToDateTime("2022-11-11 06:38:47.4122384");
            model.Enabled = true;
            model.Required = true;
            model.Emphasize = true;
            FrameworkResult result = await identityResourceService.AddIdentityResourceAsync(model);
            result.Should().BeOfType<FrameworkResult>();
            result.Status.Should().Be(ResultStatus.Success);
            // Get IdentityResourceBy Name
            IdentityResourcesModel identityResourcesModel = await identityResourceService.GetIdentityResourceAsync(model.Name);
            identityResourcesModel.Should().NotBeNull();
            identityResourcesModel.Should().BeOfType<IdentityResourcesModel>();
            identityResourcesModel.IdentityClaims.Any().Should().BeTrue();
            identityResourcesModel.IdentityClaims[0].Type.Contains(model.IdentityClaims[0].Type);
            // Check Child Table Count
            identityResourcesModel.IdentityClaims.Count().Equals(model.IdentityClaims.Count());
        }

        [Fact]
        [Trait("Category", "ErrorCase")]
        public async Task AddIdentityResourceAsync_ChildtableTypeNULL()
        {
            var model = IdentityResourceHelper.CreateIdentityResourceModel();
            Random random = new Random();
            model.Name = string.Concat(ResourceName, "_", random.Next().ToString());
            model.IdentityClaims[0].Type = null;

            FrameworkResult result = await identityResourceService.AddIdentityResourceAsync(model);
            result.Should().BeOfType<FrameworkResult>();
            result.Status.Should().Be(ResultStatus.Failed);
            result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.IdentityResourceClaimTypeRequired);
        }

        [Fact]
        [Trait("Category", "ErrorCase")]
        public async Task AddIdentityResourceAsync_Duplicate_Error()
        {
            try
            {
                var identityResourceName = await AddIdentityResourceAsync_Run();
                if (identityResourceName == null)
                {
                    return;
                }

                var model = IdentityResourceHelper.CreateIdentityResourceModel();

                model.Name = identityResourceName;
                FrameworkResult result = await identityResourceService.AddIdentityResourceAsync(model);

                result.Should().BeOfType<FrameworkResult>();
                result.Status.Should().Be(ResultStatus.Failed);
                result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.IdentityResourceAlreadyExists);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Fact]
        [Trait("Category", "ErrorCase")]

        public async Task AddIdentityResourceAsync_NameLenthExceed_Failure()
        {
            var model = IdentityResourceHelper.CreateIdentityResourceModel();
            model.Name = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            FrameworkResult result = await identityResourceService.AddIdentityResourceAsync(model);

            result.Should().BeOfType<FrameworkResult>();
            result.Status.Should().Be(ResultStatus.Failed);
            result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.IdentityResourceNameTooLong);
        }

        [Fact]
        [Trait("Caregory", "ErrorCase")]
        public async Task AddIdentityResourceAsync_NameNull()
        {
            var model = IdentityResourceHelper.CreateIdentityResourceModel();
            model.Name = null;
            FrameworkResult result = await identityResourceService.AddIdentityResourceAsync(model);
            result.Should().BeOfType<FrameworkResult>();
            result.Status.Should().Be(ResultStatus.Failed);
            result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.IdentityResourceNameRequired);
        }
        [Fact]
        [Trait("Category", "SuccessCase")]
        public async Task UpdateIdentityResourceAsync_Success()
        {
            try
            {
                // Add
               var identityResourceName = await AddIdentityResourceAsync_Run();
               if (identityResourceName == null)
                {
                    return;
                }

                // Get
               var resource = await identityResourceService.GetIdentityResourceAsync(identityResourceName);
               resource.DisplayName = "HIII";

                // Update
               FrameworkResult result = await identityResourceService.UpdateIdentityResourceAsync(resource);
               result.Should().BeOfType<FrameworkResult>();
               result.Status.Should().Be(ResultStatus.Success);
                // Get
               IdentityResourcesModel identityResourcesModel = await identityResourceService.GetIdentityResourceAsync(identityResourceName);
               identityResourcesModel.Should().NotBeNull();
               identityResourcesModel.DisplayName.Contains(resource.DisplayName);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Fact]
        [Trait("Category", "ErrorCase")]
        public async Task UpdateIdentityResourceAsync_ChildModelNull()
        {
            try
            {
                var identityResourceName = await AddIdentityResourceAsync_Run();
                if (identityResourceName == null)
                {
                    return;
                }

                var resource = await identityResourceService.GetIdentityResourceAsync(identityResourceName);
                resource.IdentityClaims = null;
                FrameworkResult result = await identityResourceService.UpdateIdentityResourceAsync(resource);

                result.Should().BeOfType<FrameworkResult>();
                result.Status.Should().Be(ResultStatus.Success);

                IdentityResourcesModel identityResourcesModel = await identityResourceService.GetIdentityResourceAsync(identityResourceName);
                identityResourcesModel.Should().NotBeNull();
                identityResourcesModel.DisplayName.Contains(resource.DisplayName);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Fact]
        [Trait("Category", "ErrorCase")]
        public async Task UpdateIdentityResourceAsync_ChildModelEmpty()
        {
            try
            {
                var identityResourceName = await AddIdentityResourceAsync_Run();
                if (identityResourceName == null)
                {
                    return;
                }

                var resource = await identityResourceService.GetIdentityResourceAsync(identityResourceName);
                resource.IdentityClaims.Clear();
                FrameworkResult result = await identityResourceService.UpdateIdentityResourceAsync(resource);

                result.Should().BeOfType<FrameworkResult>();
                result.Status.Should().Be(ResultStatus.Success);

                IdentityResourcesModel identityResourcesModel = await identityResourceService.GetIdentityResourceAsync(resource.Id);
                identityResourcesModel.Should().NotBeNull();
                identityResourcesModel.DisplayName.Contains(resource.Name);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Fact]
        [Trait("Category", "UpdateErrorCase")]
        public async Task UpdateIdentityResource_ConcurrencyError()
        {
            // Arrange
            var identityResourceName = await AddIdentityResourceAsync_Run();
            if (identityResourceName == null)
            {
                return;
            }

            IdentityResourcesModel identityResourcesModel1 = await identityResourceService.GetIdentityResourceAsync(identityResourceName);
            IdentityResourcesModel identityResourcesModel2 = await identityResourceService.GetIdentityResourceAsync(identityResourceName);
            identityResourcesModel2.DisplayName = "Test1";
            identityResourcesModel2.Description = "test1";
            identityResourcesModel2.ModifiedBy = "Suresh";
            identityResourcesModel1.Description = "Jesu";
            identityResourcesModel1.DisplayName = "Jesu";
            identityResourcesModel1.ModifiedBy = "Jesu";
            // Act
            FrameworkResult result1 = await identityResourceService.UpdateIdentityResourceAsync(identityResourcesModel2);
            result1.Status.Should().Be(ResultStatus.Success);
            FrameworkResult result2 = await identityResourceService.UpdateIdentityResourceAsync(identityResourcesModel2);
            result2.Status.Should().Be(ResultStatus.Failed);
            result2.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.ConcurrencyFailure);
        }

        [Fact]
        [Trait("Category", "ErrorCase")]

        public async Task Update_IdentityResource_NameLengthExceed_Error()
        {
            var identityResourceName = await AddIdentityResourceAsync_Run();
            if (identityResourceName == null)
            {
                return;
            }

            var resource = await identityResourceService.GetIdentityResourceAsync(identityResourceName);
            resource.DisplayName = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";

            FrameworkResult result = await identityResourceService.UpdateIdentityResourceAsync(resource);
            result.Should().BeOfType<FrameworkResult>();
            result.Status.Should().Be(ResultStatus.Failed);
            result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.IdentityDisplayNameTooLong);
        }

        [Fact]
        [Trait("Category", "ErroCase")]
        public async Task Update_IdentityResource_NullCheck()
        {
            var identityResourceName = await AddIdentityResourceAsync_Run();
            if (identityResourceName == null)
            {
                return;
            }

            var resource = await identityResourceService.GetIdentityResourceAsync(identityResourceName);
            resource.CreatedBy = null;
            resource.Name = null;
            FrameworkResult result = await identityResourceService.UpdateIdentityResourceAsync(resource);
            result.Should().BeOfType<FrameworkResult>();
            result.Status.Should().Be(ResultStatus.Failed);
            result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.IdentityResourceNameRequired);

            // Get Create by Null
            resource.Name = identityResourceName;
            await FluentActions.Invoking(() => identityResourceService.UpdateIdentityResourceAsync(resource)).Should().ThrowAsync<Exception>();
        }

        [Fact]
        [Trait("Category", "ErroCase")]
        public async Task Update_IdentityResource_EmptyCheck()
        {
            var identityResourceName = await AddIdentityResourceAsync_Run();
            if (identityResourceName == null)
            {
                return;
            }

            var resource = await identityResourceService.GetIdentityResourceAsync(identityResourceName);
            resource.CreatedBy = string.Empty;
            resource.Name = string.Empty;
            FrameworkResult result = await identityResourceService.UpdateIdentityResourceAsync(resource);
            result.Should().BeOfType<FrameworkResult>();
            result.Status.Should().Be(ResultStatus.Failed);
            result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.IdentityResourceNameRequired);
        }

        [Fact]
        [Trait("Category", "SuccessCase")]
        public async Task AddIdentityResourceClaimAsync__Success()
        {
            try
            {
                var identityResourceName = await AddIdentityResourceAsync_Run();
                if (identityResourceName == null)
                {
                    return;
                }

                IdentityResourcesModel identityResource = await identityResourceService.GetIdentityResourceAsync(identityResourceName);
                identityResource.Should().NotBeNull();

                var model = IdentityResourceHelper.CreateIdentityResourceClaimModel();
                model.IdentityResourceId = identityResource.Id;
                FrameworkResult result = await identityResourceService.AddIdentityResourceClaimAsync(model);

                IdentityResourcesModel identityResourceResult = await identityResourceService.GetIdentityResourceAsync(identityResourceName);
                identityResource.Should().NotBeNull();

                result.Should().BeOfType<FrameworkResult>();
                result.Status.Should().Be(ResultStatus.Success);

                IList<IdentityClaimsModel> identityClaimsModel = await identityResourceService.GetIdentityResourceClaimsAsync(model.IdentityResourceId);
                identityClaimsModel.Should().NotBeNullOrEmpty();

                // Check Table Count
                identityResourceResult.IdentityClaims.Count.Equals(identityClaimsModel.Count);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Fact]
        [Trait("Category", "ErrorCase")]
        public async Task AddIdentityResourceClaimAsync_TypeLenthExceedd_Error()
        {
            try
            {
                var model = IdentityResourceHelper.CreateIdentityResourceClaimModel();
                model.Type = "hhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllll";

                FrameworkResult result = await identityResourceService.AddIdentityResourceClaimAsync(model);
                result.Should().BeOfType<FrameworkResult>();
                result.Status.Should().Be(ResultStatus.Failed);
                result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.IdentityResourceClaimTypeTooLong);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Fact]
        [Trait("Category", "ErrorCase")]
        public async Task AddIdentityResourceClaimAsync_TypeNull_Error()
        {
            try
            {
                var model = IdentityResourceHelper.CreateIdentityResourceClaimModel();
                model.Type = string.Empty;
                FrameworkResult result = await identityResourceService.AddIdentityResourceClaimAsync(model);
                result.Should().BeOfType<FrameworkResult>();
                result.Status.Should().Be(ResultStatus.Failed);
                result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.IdentityResourceClaimTypeRequired);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [Fact]
        [Trait("Category", "ErrorCase")]
        public async Task DeleteIdentityResourceAsync_ByID_Success()
        {
            try
            {
                // Add
                var identityResourceName = await AddIdentityResourceAsync_Run();
                if (identityResourceName == null)
                {
                    return;
                }

                // Get
                IdentityResourcesModel identityResourcesModel = await identityResourceService.GetIdentityResourceAsync(identityResourceName);
                identityResourcesModel.Should().NotBeNull();

                Guid resourceId = identityResourcesModel.Id;
                IList<IdentityClaimsModel> intialidentityClaimsModels = identityResourcesModel.IdentityClaims;
                // Delete
                FrameworkResult result = await identityResourceService.DeleteIdentityResourceAsync(identityResourcesModel.Id);
                result.Should().BeOfType<FrameworkResult>();
                result.Status.Should().Be(ResultStatus.Success);
                // Resource
                IdentityResourcesModel identityResourcesModel1 = await identityResourceService.GetIdentityResourceAsync(resourceId);
                identityResourcesModel1.Should().BeNull();
                // Claim
                IList<IdentityClaimsModel> identityClaimsModel = await identityResourceService.GetIdentityResourceClaimsAsync(resourceId);
                identityClaimsModel.Should().BeNull();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Fact]
        [Trait("Category", "ErrorCase")]
        public async Task DeleteIdentityResourceAsync_ByID_Error()
        {

            try
            {
                // Add
                var identityResourceName = await AddIdentityResourceAsync_Run();
                if (identityResourceName == null)
                {
                    return;
                }

                // Get
                IdentityResourcesModel identityResourcesModel = await identityResourceService.GetIdentityResourceAsync(identityResourceName);
                identityResourcesModel.Should().NotBeNull();

                var model = IdentityResourceHelper.CreateIdentityResourceClaimModel();
                model.IdentityResourceId = identityResourcesModel.Id;
                FrameworkResult resultclaim = await identityResourceService.AddIdentityResourceClaimAsync(model);
                resultclaim.Should().BeOfType<FrameworkResult>();
                resultclaim.Status.Should().Be(ResultStatus.Success);

                // Delete
                FrameworkResult result = await identityResourceService.DeleteIdentityResourceClaimAsync(identityResourcesModel.Id);
                result.Should().BeOfType<FrameworkResult>();
                result.Status.Should().Be(ResultStatus.Success);

                IdentityResourcesModel identityResourcesModel2 = await identityResourceService.GetIdentityResourceAsync(model.IdentityResourceId);

                // Claim
                IList<IdentityClaimsModel> identityClaimsModel = await identityResourceService.GetIdentityResourceClaimsAsync(model.IdentityResourceId);
                identityClaimsModel.Should().BeNull();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Fact]
        [Trait("Category", "ErrorCase")]
        public async Task DeleteIdentityResourceAsync_Name_Success()
        {
            try
            {
                var identityResourceName = await AddIdentityResourceAsync_Run();
                if (identityResourceName == null)
                {
                    return;
                }
                IdentityResourcesModel identityResourcesModel = await identityResourceService.GetIdentityResourceAsync(identityResourceName);
                identityResourcesModel.Should().NotBeNull();
                string resourceNamenew = identityResourcesModel.Name;
                Guid resourceid = identityResourcesModel.Id;
                IList<IdentityClaimsModel> intialidentityClaimsModels = identityResourcesModel.IdentityClaims;

                FrameworkResult result = await identityResourceService.DeleteIdentityResourceAsync(identityResourcesModel.Name);
                result.Should().BeOfType<FrameworkResult>();
                result.Status.Should().Be(ResultStatus.Success);

                IdentityResourcesModel identityResourcesModel1 = await identityResourceService.GetIdentityResourceAsync(resourceNamenew);
                identityResourcesModel1.Should().BeNull();

                IList<IdentityClaimsModel> identityClaimsModel = await identityResourceService.GetIdentityResourceClaimsAsync(resourceid);
                identityClaimsModel.Should().BeNull();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Fact]
        [Trait("Category", "SuccessCase")]
        public async Task DeleteIdentityResourceClaimAsync_Success()
        {
            try
            {
                var identityResourceName = await AddIdentityResourceAsync_Run();
                if (identityResourceName == null)
                {
                    return;
                }

                IdentityResourcesModel identityResource = await identityResourceService.GetIdentityResourceAsync(identityResourceName);
                identityResource.Should().NotBeNull();
                // Get Claim Availbilty
                IList<IdentityClaimsModel> identityClaimsModelsIntial = await identityResourceService.GetIdentityResourceClaimsAsync(identityResource.Id);
                identityClaimsModelsIntial.Should().NotBeNullOrEmpty();
                // Delete Claim
                FrameworkResult result = await identityResourceService.DeleteIdentityResourceClaimAsync(identityResource.Id);
                // Get Result Claim Availbility
                IList<IdentityClaimsModel> identityClaimsModelsResult = await identityResourceService.GetIdentityResourceClaimsAsync(identityResource.Id);
                identityClaimsModelsResult.Should().BeNull();
                result.Should().BeOfType<FrameworkResult>();
                result.Status.Should().Be(ResultStatus.Success);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Fact]
        [Trait("Category", "SuccessCase")]
        public async Task Delete_IdentityResourceClaim_By_Model_Success()
        {
            try
            {
                var identityResourceName = await AddIdentityResourceAsync_Run();
                if (identityResourceName == null)
                {
                    return;
                }

                IdentityResourcesModel identityResource = await identityResourceService.GetIdentityResourceAsync(identityResourceName);
                identityResource.Should().NotBeNull();

                IList<IdentityClaimsModel> claims = await identityResourceService.GetIdentityResourceClaimsAsync(identityResource.Id);
                claims.Should().NotBeNull();

                FrameworkResult result = await identityResourceService.DeleteIdentityResourceClaimAsync(claims.FirstOrDefault());
                result.Should().BeOfType<FrameworkResult>();
                result.Status.Should().Be(ResultStatus.Success);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Fact]
        [Trait("Category", "SuccessCase")]
        public async Task GetIdentityResourceForResourceId_Success()
        {
            var identityResourceName = await AddIdentityResourceAsync_Run();
            if (identityResourceName == null)
            {
                return;
            }

            IdentityResourcesModel identityResourcesModelResult = await identityResourceService.GetIdentityResourceAsync(identityResourceName);
            identityResourcesModelResult.Should().NotBeNull();
            identityResourcesModelResult.Should().BeOfType<IdentityResourcesModel>();
        }

        [Fact]
        [Trait("Category", "SuccessCase")]
        public async Task GetIdentityResourceForResourceName_Success()
        {
            var identityResourceName = await AddIdentityResourceAsync_Run();
            if (identityResourceName == null)
            {
                return;
            }

            IdentityResourcesModel identityResourcesModelResult = await identityResourceService.GetIdentityResourceAsync(identityResourceName);
            identityResourcesModelResult.Should().NotBeNull();
            identityResourcesModelResult.Should().BeOfType<IdentityResourcesModel>();
        }

        [Fact]
        [Trait("Category", "SuccessCase")]
        public async Task GetAllIdentityResource_Success()
        {
            // Act
            IList<IdentityResourcesModel> result = await identityResourceService.GetAllIdentityResourcesAsync();
            result.Should().NotBeNull();
            result.Should().HaveCountGreaterOrEqualTo(1);
        }
    }
}



