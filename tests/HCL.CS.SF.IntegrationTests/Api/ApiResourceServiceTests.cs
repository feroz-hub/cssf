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
    public class ApiResourceServiceTests : HCLCSSFFakeSetup
    {
        private const string ResourceName = "AlphaClientOne";
        private const string ScopeName = "ClientApi.Create";
        private const string AnonymousResult = "Anonymous";
        private readonly Random random;
        private readonly IApiResourceService apiResourceService;

        public ApiResourceServiceTests()
        {
            apiResourceService = ServiceProvider.GetService<IApiResourceService>();
            random = new Random();
        }

        #region SUCCESS SCENARIOS
        /*APIRESOURCE*/

        [Fact]
        [Trait("Category", "AddSuccessCase")]
        public async Task AddApiResourceAsync_ReturnsSuccess()
        {
            // Adding Assert
            var apiResourcesModelInput = ApiResourceHelper.CreateApiResourceModel();
            IList<ApiResourcesModel> apiResourcesModelsList = await apiResourceService.GetAllApiResourcesAsync();
            if (apiResourcesModelsList != null && !apiResourcesModelsList.Any())
            {
                apiResourcesModelInput.Name = ResourceName;
            }
            else
            {
                apiResourcesModelInput.Name = string.Concat(ResourceName, "_", random.Next().ToString());
            }

            FrameworkResult result = await apiResourceService.AddApiResourceAsync(apiResourcesModelInput);
            result.Should().BeOfType<FrameworkResult>();
            result.Status.Should().Be(ResultStatus.Success);

            // Getting values from the db
            ApiResourcesModel resourcesModelResult = await apiResourceService.GetApiResourceAsync(apiResourcesModelInput.Name);
            resourcesModelResult.Should().NotBeNull();
            resourcesModelResult.Id.Should().NotBe(Guid.Empty);
            resourcesModelResult.Name.Should().BeEquivalentTo(apiResourcesModelInput.Name);
            resourcesModelResult.ApiResourceClaims.Count.Should().Be(apiResourcesModelInput.ApiResourceClaims.Count);
            resourcesModelResult.ApiScopes.Count.Should().Be(apiResourcesModelInput.ApiScopes.Count);
            resourcesModelResult.ApiScopes.Select(_ => _.ApiScopeClaims).Count().Should().Be(apiResourcesModelInput.ApiScopes.Select(_ => _.ApiScopeClaims).Count());
        }

        [Fact]
        [Trait("Category", "AddSuccessCase")]
        public async Task AddApiResourceAsync_ApiResourceClaimsNullEntryCheck_Success()
        {
            try
            {
                // Add : Case 1 : ApiResourceClaim as null.
                var apiResourcesModelInput = ApiResourceHelper.CreateApiResourceModel();
                apiResourcesModelInput.Name = string.Concat(ResourceName, "_", random.Next().ToString());
                apiResourcesModelInput.ApiResourceClaims = null;
                // Act
                FrameworkResult result = await apiResourceService.AddApiResourceAsync(apiResourcesModelInput);
                // Asssert
                result.Should().BeOfType<FrameworkResult>();
                result.Status.Should().Be(ResultStatus.Success);

                // Get APi Resource by resourcename :
                ApiResourcesModel apiResourcesModelResult = await apiResourceService.GetApiResourceAsync(apiResourcesModelInput.Name);
                apiResourcesModelResult.Should().NotBeNull();
                apiResourcesModelResult.ApiResourceClaims.Count.Should().Be(0);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Fact]
        [Trait("Category", "AddSuccessCase")]
        public async Task AddApiResourceAsync_ApiScopesNullEntryCheck_Success()
        {
            try
            {
                // Add : Case 1 : ApiResourceClaim as null.
                var apiResourcesModelInput = ApiResourceHelper.CreateApiResourceModel();
                apiResourcesModelInput.Name = string.Concat(ResourceName, "_", random.Next().ToString());
                apiResourcesModelInput.ApiScopes = null;
                // Act
                FrameworkResult result = await apiResourceService.AddApiResourceAsync(apiResourcesModelInput);
                // Asssert
                result.Should().BeOfType<FrameworkResult>();
                result.Status.Should().Be(ResultStatus.Success);

                // Get APi Resource by resourcename :
                ApiResourcesModel apiResourcesModelResult = await apiResourceService.GetApiResourceAsync(apiResourcesModelInput.Name);
                apiResourcesModelResult.Should().NotBeNull();
                apiResourcesModelResult.ApiScopes.Count.Should().Be(0);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Fact]
        [Trait("Category", "AddSuccessCase")]
        public async Task AddApiResourceAsync_CreatedByIsNull_InsertedAsAnonymous()
        {
            try
            {
                // Add ApiResource with CreatedBy as Null.
                var apiResourcesModel = ApiResourceHelper.CreateApiResourceModel();
                apiResourcesModel.Name = string.Concat(ResourceName, "_", random.Next().ToString());
                apiResourcesModel.CreatedBy = null;
                FrameworkResult result = await apiResourceService.AddApiResourceAsync(apiResourcesModel);
                result.Should().BeOfType<FrameworkResult>();
                result.Status.Should().Be(ResultStatus.Success);

                // Get ApiResource.
                ApiResourcesModel apiResourcesModelResult = await apiResourceService.GetApiResourceAsync(apiResourcesModel.Name);
                apiResourcesModelResult.Should().NotBeNull();
                apiResourcesModelResult.CreatedBy.Should().Be(AnonymousResult);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Fact]
        [Trait("Category", "UpdateSuccessCase")]
        public async Task UpdateApiResourceAsync_ReturnsSuccess()
        {
            try
            {
                // Get ApiResource By Name
                var apiResourcesModelInput = ApiResourceHelper.CreateApiResourceModel();
                apiResourcesModelInput.Name = string.Concat(ResourceName, "_", random.Next().ToString());
                FrameworkResult addAPiResourceResult = await apiResourceService.AddApiResourceAsync(apiResourcesModelInput);
                addAPiResourceResult.Should().BeOfType<FrameworkResult>();
                addAPiResourceResult.Status.Should().Be(ResultStatus.Success);

                // Get ApiResource
                ApiResourcesModel apiResourcesModelResult = await apiResourceService.GetApiResourceAsync(apiResourcesModelInput.Name);
                apiResourcesModelResult.Should().NotBeNull();

                // Update ApiResource
                apiResourcesModelResult.Name = apiResourcesModelInput.Name + 123;
                apiResourcesModelResult.DisplayName = "Client Api-One";
                FrameworkResult result = await apiResourceService.UpdateApiResourceAsync(apiResourcesModelResult);
                result.Should().BeOfType<FrameworkResult>();
                result.Status.Should().Be(ResultStatus.Success);

                // Get ApiResource by Id : Assert => GetModel.DisplayName.Equals(apiResourceInput.DisplayName)
                ApiResourcesModel getApiResourceModelResult = await apiResourceService.GetApiResourceAsync(apiResourcesModelResult.Id);
                getApiResourceModelResult.Should().NotBeNull();
                getApiResourceModelResult.DisplayName.Should().BeEquivalentTo(apiResourcesModelResult.DisplayName);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Fact]
        [Trait("Category", "UpdateSuccessCase")]
        public async Task UpdateApiResourceAsync_ChildTablesValues_Cleared_Sets_IsDeletedTrue_In_ChildTable_Success()
        {
            // Add an ApiResource.
            var apiResourcesModelInput = ApiResourceHelper.CreateApiResourceModel();
            apiResourcesModelInput.Name = string.Concat(ResourceName, "_", random.Next().ToString());
            FrameworkResult addAPiResourceResult = await apiResourceService.AddApiResourceAsync(apiResourcesModelInput);
            addAPiResourceResult.Should().BeOfType<FrameworkResult>();
            addAPiResourceResult.Status.Should().Be(ResultStatus.Success);

            // Get ApiResource
            ApiResourcesModel apiResourcesModelResult = await apiResourceService.GetApiResourceAsync(apiResourcesModelInput.Name);
            apiResourcesModelResult.Should().NotBeNull();

            // Update Api Resource with the getApiResource_ByName_Model
            apiResourcesModelResult.DisplayName = "AlphaOne";
            apiResourcesModelResult.ApiResourceClaims.Clear();
            FrameworkResult updateApiResourceResult = await apiResourceService.UpdateApiResourceAsync(apiResourcesModelResult);
            updateApiResourceResult.Should().BeOfType<FrameworkResult>();
            updateApiResourceResult.Status.Should().Be(ResultStatus.Success);

            // Get Updated ApiResource Model
            ApiResourcesModel getApiResourceModelResult = await apiResourceService.GetApiResourceAsync(apiResourcesModelResult.Id);
            getApiResourceModelResult.Should().NotBeNull();

            // Check for ApiResourceClaims => null
            getApiResourceModelResult.ApiResourceClaims.Should().BeNullOrEmpty();
        }

        [Fact]
        [Trait("Category", "SuccessCase")]
        public async Task UpdateApiResourceAsync_AddingChildTables_Success()
        {
            // Add an ApiResource.
            var apiResourcesModelInput = ApiResourceHelper.CreateApiResourceModel();
            apiResourcesModelInput.Name = string.Concat(ResourceName, "_", random.Next().ToString());
            apiResourcesModelInput.ApiResourceClaims.Clear();
            apiResourcesModelInput.ApiScopes.Clear();
            FrameworkResult addAPiResourceResult = await apiResourceService.AddApiResourceAsync(apiResourcesModelInput);
            addAPiResourceResult.Should().BeOfType<FrameworkResult>();
            addAPiResourceResult.Status.Should().Be(ResultStatus.Success);

            // Get ApiResource
            ApiResourcesModel apiResourcesModelResult = await apiResourceService.GetApiResourceAsync(apiResourcesModelInput.Name);
            apiResourcesModelResult.Should().NotBeNull();
            apiResourcesModelResult.ApiResourceClaims.Count.Should().Be(0);
            apiResourcesModelResult.ApiScopes.Count.Should().Be(0);

            // Update ApiResource
            Guid apiResourceId = apiResourcesModelResult.Id;
            apiResourcesModelResult.ApiResourceClaims = new List<ApiResourceClaimsModel>()
                {
                    new ApiResourceClaimsModel() { Type = "Type1", CreatedBy = "Test", ApiResourceId = apiResourceId },
                    new ApiResourceClaimsModel() { Type = "Type2", CreatedBy = "Test", ApiResourceId = apiResourceId },
                    new ApiResourceClaimsModel() { Type = "Type3", CreatedBy = "Test", ApiResourceId = apiResourceId },
                };
            FrameworkResult updateApiResourceResult = await apiResourceService.UpdateApiResourceAsync(apiResourcesModelResult);
            updateApiResourceResult.Should().BeOfType<FrameworkResult>();
            updateApiResourceResult.Status.Should().Be(ResultStatus.Success);

            // Get ApiResource
            ApiResourcesModel apiResourcesModelResult1 = await apiResourceService.GetApiResourceAsync(apiResourcesModelInput.Name);
            apiResourcesModelResult1.Should().NotBeNull();
        }

        [Fact]
        [Trait("Category", "DeleteSuccessCase")]
        public async Task DeleteApiResourceAsync_ById_Success()
        {
            try
            {
                // Add an ApiResource.
                var apiResourcesModelInput = ApiResourceHelper.CreateApiResourceModel();
                apiResourcesModelInput.Name = string.Concat(ResourceName, "_", random.Next().ToString());
                FrameworkResult addAPiResourceResult = await apiResourceService.AddApiResourceAsync(apiResourcesModelInput);
                addAPiResourceResult.Should().BeOfType<FrameworkResult>();
                addAPiResourceResult.Status.Should().Be(ResultStatus.Success);

                // Get ApiResource
                ApiResourcesModel apiResourcesModelResult = await apiResourceService.GetApiResourceAsync(apiResourcesModelInput.Name);
                apiResourcesModelResult.Should().NotBeNull();
                Guid resourceId = apiResourcesModelResult.Id;
                List<ApiScopesModel> apiScopesInitialResult = apiResourcesModelResult.ApiScopes;

                // Delete API Resource by Id
                FrameworkResult deleteApiResourceResult = await apiResourceService.DeleteApiResourceAsync(apiResourcesModelResult.Id);
                deleteApiResourceResult.Should().BeOfType<FrameworkResult>();
                deleteApiResourceResult.Status.Should().Be(ResultStatus.Success);

                // Get Api Resource => null
                ApiResourcesModel apiResourcesModelOneResult = await apiResourceService.GetApiResourceAsync(resourceId);
                apiResourcesModelOneResult.Should().BeNull();

                // Get Api Scopes  => null
                foreach (var apiscope in apiScopesInitialResult)
                {
                    ApiScopesModel apiScopesModelResult = await apiResourceService.GetApiScopeAsync(apiscope.Id);
                    apiScopesModelResult.Should().BeNull();
                }

                // Get Api ResourceClaims => null
                IList<ApiResourceClaimsModel> apiResourceClaimsModelResult = await apiResourceService.GetApiResourceClaimsAsync(resourceId);
                apiResourceClaimsModelResult.Should().BeNull();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Fact]
        [Trait("Category", "DeleteSuccessCase")]
        public async Task DeleteApiResourceAsync_ByName_Success()
        {
            try
            {
                // Add an ApiResource.
                var apiResourcesModelInput = ApiResourceHelper.CreateApiResourceModel();
                apiResourcesModelInput.Name = string.Concat(ResourceName, "_", random.Next().ToString());
                FrameworkResult addAPiResourceResult = await apiResourceService.AddApiResourceAsync(apiResourcesModelInput);
                addAPiResourceResult.Should().BeOfType<FrameworkResult>();
                addAPiResourceResult.Status.Should().Be(ResultStatus.Success);

                // Get ApiResource
                ApiResourcesModel apiResourcesModelResult = await apiResourceService.GetApiResourceAsync(apiResourcesModelInput.Name);
                apiResourcesModelResult.Should().NotBeNull();
                Guid resourceId = apiResourcesModelResult.Id;
                List<ApiScopesModel> apiScopesInitialResult = apiResourcesModelResult.ApiScopes;

                // Delete API Resource by name
                FrameworkResult result = await apiResourceService.DeleteApiResourceAsync(apiResourcesModelInput.Name);
                result.Should().BeOfType<FrameworkResult>();
                result.Status.Should().Be(ResultStatus.Success);

                // Get Api Resource => null
                ApiResourcesModel apiResourcesModelOneResult = await apiResourceService.GetApiResourceAsync(apiResourcesModelInput.Name);
                apiResourcesModelOneResult.Should().BeNull();

                // Get Api Scopes  => null
                foreach (var apiscope in apiScopesInitialResult)
                {
                    ApiScopesModel apiScopesModelResult = await apiResourceService.GetApiScopeAsync(apiscope.Id);
                    apiScopesModelResult.Should().BeNull();
                }

                // Get Api ResourceClaims => null
                IList<ApiResourceClaimsModel> apiResourceClaimsModelResult = await apiResourceService.GetApiResourceClaimsAsync(resourceId);
                apiResourceClaimsModelResult.Should().BeNull();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /*APISCOPES*/

        [Fact]
        [Trait("Category", "AddSuccessCase")]
        public async Task AddApiScopeAsync_Success()
        {
            // Add an ApiResource.
            var apiResourcesModelInput = ApiResourceHelper.CreateApiResourceModel();
            apiResourcesModelInput.Name = string.Concat(ResourceName, "_", random.Next().ToString());
            FrameworkResult addAPiResourceResult = await apiResourceService.AddApiResourceAsync(apiResourcesModelInput);
            addAPiResourceResult.Should().BeOfType<FrameworkResult>();
            addAPiResourceResult.Status.Should().Be(ResultStatus.Success);

            // Get ApiResource
            ApiResourcesModel apiResourcesModelResult = await apiResourceService.GetApiResourceAsync(apiResourcesModelInput.Name);
            apiResourcesModelResult.Should().NotBeNull();

            // Add Scopes : Arrange, Act , Assert
            var apiScopeModelInput = ApiResourceHelper.CreateApiScopeModel();
            apiScopeModelInput.Name = string.Concat(ScopeName, "_", random.Next().ToString());
            apiScopeModelInput.ApiResourceId = apiResourcesModelResult.Id;
            FrameworkResult result = await apiResourceService.AddApiScopeAsync(apiScopeModelInput);
            result.Should().BeOfType<FrameworkResult>();
            result.Status.Should().Be(ResultStatus.Success);

            // Get Scopes by name : Arrange , Act, Assert
            string apiScopeName = apiScopeModelInput.Name; // name of the input model
            ApiScopesModel apiScopeResult = await apiResourceService.GetApiScopeAsync(apiScopeName);
            apiScopeResult.Should().NotBeNull();
            apiScopeResult.Id.Should().NotBe(Guid.Empty);
            apiScopeResult.Name.Should().BeEquivalentTo(apiScopeModelInput.Name);

            // Assert for child tables
            apiScopeResult.ApiScopeClaims.Count.Should().Be(apiScopeModelInput.ApiScopeClaims.Count);
        }

        [Fact]
        [Trait("Category", "AddSuccessCase")]
        public async Task AddApiScopeAsync_CreatedByIsNull_InsertedAsAnonymous()
        {
            // Get resourcemodel by name: Arrange , Act, Assert.
            ApiResourcesModel apiResourceModelResult = await apiResourceService.GetApiResourceAsync(ResourceName);
            apiResourceModelResult.Should().NotBeNull();

            // Add Scopes : Arrange, Act , Assert.
            var apiScopeModelInput = ApiResourceHelper.CreateApiScopeModel();
            apiScopeModelInput.ApiResourceId = apiResourceModelResult.Id;
            apiScopeModelInput.Name = string.Concat(ScopeName, "_", random.Next().ToString());
            apiScopeModelInput.CreatedBy = null;
            FrameworkResult addApiScopeResult = await apiResourceService.AddApiScopeAsync(apiScopeModelInput);
            addApiScopeResult.Should().BeOfType<FrameworkResult>();
            addApiScopeResult.Status.Should().Be(ResultStatus.Success);

            // GetApiScopes and check if CreatedBy is Anonymous.
            var apiScopesModelResult = await apiResourceService.GetApiScopeAsync(apiScopeModelInput.Name);
            apiScopesModelResult.CreatedBy.Should().Be(AnonymousResult);
        }

        [Fact]
        [Trait("Category", "AddSuccessCase")]
        public async Task AddApiScopeAsync_ApiScopeClaimsNullEntryCheck_Success()
        {
            // Get APiResource by Name
            ApiResourcesModel apiResourceModelResult = await apiResourceService.GetApiResourceAsync(ResourceName);
            apiResourceModelResult.Should().NotBeNull();

            // Add ApiScopes with ApiScopeClaims as null.
            var apiScopeModelInput = ApiResourceHelper.CreateApiScopeModel();
            apiScopeModelInput.ApiResourceId = apiResourceModelResult.Id;
            apiScopeModelInput.ApiScopeClaims = null;
            apiScopeModelInput.Name = string.Concat(ScopeName, "_", random.Next().ToString());
            FrameworkResult addApiScopeResult = await apiResourceService.AddApiScopeAsync(apiScopeModelInput);
            addApiScopeResult.Should().BeOfType<FrameworkResult>();
            addApiScopeResult.Status.Should().Be(ResultStatus.Success);

            // Get ApiScope with ApiScopeName
            ApiScopesModel apiScopeModelResult = await apiResourceService.GetApiScopeAsync(apiScopeModelInput.Name);
            apiScopeModelResult.Should().NotBeNull();
            apiScopeModelResult.ApiScopeClaims.Should().BeNullOrEmpty();

            // GetApiScopeClaims for the ApiScopeId
            IList<ApiScopeClaimsModel> apiScopeClaimsList = await apiResourceService.GetApiScopeClaimsAsync(apiScopeModelResult.Id);
            apiScopeClaimsList.Should().BeNullOrEmpty();
        }

        [Fact]
        [Trait("Category", "UpdateSuccessCase")]
        public async Task UpdateApiScopeAsync_Success()
        {
            try
            {
                // Get ApiResource By Name
                var apiResourceResult = await apiResourceService.GetApiResourceAsync(ResourceName);
                apiResourceResult.Should().NotBeNull();

                // Add ApiScope for the ApiResource
                var apiScopeModelInput = ApiResourceHelper.CreateApiScopeModel();
                apiScopeModelInput.Name = string.Concat(ScopeName, "_", random.Next().ToString());
                apiScopeModelInput.ApiResourceId = apiResourceResult.Id;
                FrameworkResult addApiScoperesult = await apiResourceService.AddApiScopeAsync(apiScopeModelInput);
                addApiScoperesult.Should().BeOfType<FrameworkResult>();
                addApiScoperesult.Status.Should().Be(ResultStatus.Success);

                // Get ApiScope By name
                var apiScopeResult = await apiResourceService.GetApiScopeAsync(apiScopeModelInput.Name);
                apiScopeResult.Should().NotBeNull();

                // Update ApiScope.
                apiScopeResult.Description = "Test scope";
                FrameworkResult result = await apiResourceService.UpdateApiScopeAsync(apiScopeResult);
                result.Should().BeOfType<FrameworkResult>();
                result.Status.Should().Be(ResultStatus.Success);

                // Get ApiScope By Scope Name  : Assert GetModel.Descriiption.Equals(apiScopeInput.Description)
                ApiScopesModel getApiScopeModelResult = await apiResourceService.GetApiScopeAsync(apiScopeResult.Name);
                getApiScopeModelResult.Should().NotBeNull();
                getApiScopeModelResult.Description.Should().BeEquivalentTo(apiScopeResult.Description);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Fact]
        [Trait("Category", "DeleteSuccessCase")]
        public async Task DeleteApiScopeAsync_ById_Success()
        {
            try
            {
                // Get ApiResource By Name
                var apiResourceResult = await apiResourceService.GetApiResourceAsync(ResourceName);
                apiResourceResult.Should().NotBeNull();

                // Add ApiScope for the ApiResource
                var apiScopeModelInput = ApiResourceHelper.CreateApiScopeModel();
                apiScopeModelInput.Name = string.Concat(ScopeName, "_", random.Next().ToString());
                apiScopeModelInput.ApiResourceId = apiResourceResult.Id;
                FrameworkResult addApiScoperesult = await apiResourceService.AddApiScopeAsync(apiScopeModelInput);
                addApiScoperesult.Should().BeOfType<FrameworkResult>();
                addApiScoperesult.Status.Should().Be(ResultStatus.Success);

                // Get ApiScopes as per Scope Name
                ApiScopesModel apiScopeInitialResult = await apiResourceService.GetApiScopeAsync(apiScopeModelInput.Name);
                apiScopeInitialResult.Should().NotBeNull();

                // Delete ApiScope by ScopeId
                FrameworkResult result = await apiResourceService.DeleteApiScopeAsync(apiScopeInitialResult.Id);
                result.Should().BeOfType<FrameworkResult>();
                result.Status.Should().Be(ResultStatus.Success);

                // Assert GetApiScopes as per ScopeId => null
                ApiScopesModel apiScopeEndResult = await apiResourceService.GetApiScopeAsync(apiScopeInitialResult.Id);
                apiScopeEndResult.Should().BeNull();

                // GetApiScopeClaim_ById : Assert as null
                IList<ApiScopeClaimsModel> apiScopeClaimsModelResult = await apiResourceService.GetApiScopeClaimsAsync(apiScopeInitialResult.Id);
                apiScopeClaimsModelResult.Should().BeNullOrEmpty();

                // Check in the parent tabel call
                var apiResourceResultOne = await apiResourceService.GetApiResourceAsync(ResourceName);
                apiResourceResultOne.Should().NotBeNull();
                apiResourceResultOne.ApiScopes.Select(apiscopes => apiscopes.Name).Should().NotContain(apiScopeInitialResult.Name);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Fact]
        [Trait("Category", "DeleteSuccessCase")]
        public async Task DeleteApiScope_ByName_Success()
        {
            try
            {
                await CheckApiResoucerun();
                // Get ApiResource By Name
                var apiResourceResult = await apiResourceService.GetApiResourceAsync(ResourceName);
                apiResourceResult.Should().NotBeNull();

                // Add ApiScope for the ApiResource
                var apiScopeModelInput = ApiResourceHelper.CreateApiScopeModel();
                apiScopeModelInput.Name = string.Concat(ScopeName, "_", random.Next().ToString());
                apiScopeModelInput.ApiResourceId = apiResourceResult.Id;
                FrameworkResult addApiScoperesult = await apiResourceService.AddApiScopeAsync(apiScopeModelInput);
                addApiScoperesult.Should().BeOfType<FrameworkResult>();
                addApiScoperesult.Status.Should().Be(ResultStatus.Success);

                // Get ApiScopes as per Scope Name
                ApiScopesModel apiScopeInitialResult = await apiResourceService.GetApiScopeAsync(apiScopeModelInput.Name);
                apiScopeInitialResult.Should().NotBeNull();

                // Delete ApiScope by name
                FrameworkResult result = await apiResourceService.DeleteApiScopeAsync(apiScopeModelInput.Name);
                result.Should().BeOfType<FrameworkResult>();
                result.Status.Should().Be(ResultStatus.Success);

                // Assert GetApiScopes as per ScopeName => null
                ApiScopesModel apiScopeEndResult = await apiResourceService.GetApiScopeAsync(apiScopeModelInput.Name);
                apiScopeEndResult.Should().BeNull();

                // GetApiScopeClaim_ById : Assert as null
                IList<ApiScopeClaimsModel> apiScopeClaimsModelResult = await apiResourceService.GetApiScopeClaimsAsync(apiScopeInitialResult.Id);
                apiScopeClaimsModelResult.Should().BeNullOrEmpty();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /*APISCOPECLAIMS*/

        [Fact]
        [Trait("Category", "AddSuccessCase")]
        public async Task AddApiScopeClaimAsync_Success()
        {
            try
            {
                // Get ApiResource By Name
                var apiResourceResult = await apiResourceService.GetApiResourceAsync(ResourceName);
                apiResourceResult.Should().NotBeNull();

                // Get Scope
                string apiScopeName = apiResourceResult.ApiScopes.FirstOrDefault().Name;
                ApiScopesModel apiScopeModelResult = await apiResourceService.GetApiScopeAsync(apiScopeName);
                apiScopeModelResult.Should().NotBeNull();

                // Add ApiScopeClaimAsync with get ApiscopeId
                var apiScopeClaimModelInput = ApiResourceHelper.CreateApiScopeClaimModel();
                apiScopeClaimModelInput.ApiScopeId = apiScopeModelResult.Id;
                apiScopeClaimModelInput.Type = string.Concat("CR7", "_", random.Next().ToString());
                FrameworkResult result = await apiResourceService.AddApiScopeClaimAsync(apiScopeClaimModelInput);
                result.Should().BeOfType<FrameworkResult>();
                result.Status.Should().Be(ResultStatus.Success);

                // Get ApiScopeClaim by apiscopeId
                IList<ApiScopeClaimsModel> apiScopeClaimsModelResult = await apiResourceService.GetApiScopeClaimsAsync(apiScopeClaimModelInput.ApiScopeId);
                apiScopeClaimsModelResult.Count().Should().BeGreaterThan(0);
                apiScopeClaimsModelResult.FirstOrDefault().Id.Should().NotBe(Guid.Empty);
                apiScopeClaimsModelResult.FirstOrDefault().ApiScopeId.Should().Equals(apiScopeClaimModelInput.ApiScopeId);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Fact]
        [Trait("Category", "DeleteSuccessCase")]
        public async Task DeleteApiScopeClaim_ByClaimModel_Success()
        {
            try
            {
                // Get ApiResource By Name
                var apiResourceResult = await apiResourceService.GetApiResourceAsync(ResourceName);
                apiResourceResult.Should().NotBeNull();

                // Get Scope
                string apiScopeName = apiResourceResult.ApiScopes.FirstOrDefault().Name;
                ApiScopesModel apiScopeModelResult = await apiResourceService.GetApiScopeAsync(apiScopeName);
                apiScopeModelResult.Should().NotBeNull();

                // Add ApiScopeClaimAsync with get ApiscopeId
                var apiScopeClaimModelInput = ApiResourceHelper.CreateApiScopeClaimModel();
                apiScopeClaimModelInput.ApiScopeId = apiScopeModelResult.Id;
                apiScopeClaimModelInput.Type = string.Concat("CR7", "_", random.Next().ToString());
                FrameworkResult addApiScopeClaimResult = await apiResourceService.AddApiScopeClaimAsync(apiScopeClaimModelInput);
                addApiScopeClaimResult.Should().BeOfType<FrameworkResult>();
                addApiScopeClaimResult.Status.Should().Be(ResultStatus.Success);

                // Delete ApiScopeClaims using GetApiScopeClaimModel
                FrameworkResult result = await apiResourceService.DeleteApiScopeClaimAsync(apiScopeClaimModelInput);
                result.Should().BeOfType<FrameworkResult>();
                result.Status.Should().Be(ResultStatus.Success);

                // Get ApiScopeClaim for the apiScopeId => Assert the list not contain the delete input apiscopeclaim Type
                IList<ApiScopeClaimsModel> apiScopeClaimsModelResult = await apiResourceService.GetApiScopeClaimsAsync(apiScopeModelResult.Id);
                apiScopeClaimsModelResult.Select(x => x.Type).Should().NotContain(apiScopeClaimModelInput.Type);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Fact]
        [Trait("Category", "DeleteSuccessCase")]
        public async Task DeleteApiScopeClaim_ByScopeId_Success()
        {
            try
            {
                // Get ApiResource By Name
                var apiResourceResult = await apiResourceService.GetApiResourceAsync(ResourceName);
                apiResourceResult.Should().NotBeNull();

                // Add ApiScope
                var apiScopeModelInput = ApiResourceHelper.CreateApiScopeModel();
                apiScopeModelInput.Name = string.Concat(ScopeName, "_", random.Next().ToString());
                apiScopeModelInput.ApiResourceId = apiResourceResult.Id;
                FrameworkResult addApiScopeResult = await apiResourceService.AddApiScopeAsync(apiScopeModelInput);
                addApiScopeResult.Should().BeOfType<FrameworkResult>();
                addApiScopeResult.Status.Should().Be(ResultStatus.Success);

                // Get ApiScope By Name
                ApiScopesModel apiScopeModelResult1 = await apiResourceService.GetApiScopeAsync(apiScopeModelInput.Name);
                apiScopeModelResult1.Should().NotBeNull();

                // Delete all the ApiScopeClaims associated to the ApiScopeId
                FrameworkResult result = await apiResourceService.DeleteApiScopeClaimAsync(apiScopeModelResult1.Id);
                result.Should().BeOfType<FrameworkResult>();
                result.Status.Should().Be(ResultStatus.Success);

                // Get ApiScopeClaims by ApiScopeId : Assert as null
                IList<ApiScopeClaimsModel> apiScopeClaimsResult = await apiResourceService.GetApiScopeClaimsAsync(apiScopeModelResult1.Id);
                apiScopeClaimsResult.Should().BeNullOrEmpty();

                // Get Apiscope by ScopeId
                ApiScopesModel apiScopeModelResult2 = await apiResourceService.GetApiScopeAsync(apiScopeModelResult1.Id);
                apiScopeModelResult2.Should().NotBeNull();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /*APIRESOURCECLAIMS*/

        [Fact]
        [Trait("Category", "AddSuccessCase")]
        public async Task AddApiResourceClaim_Success()
        {
            try
            {
                // Get ApiResource By Resource Name
                ApiResourcesModel apiResourceResult = await apiResourceService.GetApiResourceAsync(ResourceName);
                apiResourceResult.Should().NotBeNull();

                // Add ApiResourceClaim using the ResourceId from the GetApiResource
                var apiResourcesClaimModelInput = ApiResourceHelper.CreateApiResourceClaimModel();
                apiResourcesClaimModelInput.ApiResourceId = apiResourceResult.Id;
                apiResourcesClaimModelInput.Type = string.Concat(apiResourcesClaimModelInput.Type, "_", random.Next().ToString());
                FrameworkResult result = await apiResourceService.AddApiResourceClaimAsync(apiResourcesClaimModelInput);
                result.Should().BeOfType<FrameworkResult>();
                result.Status.Should().Be(ResultStatus.Success);

                // GetApiResourceClaim by resourceId
                IList<ApiResourceClaimsModel> apiResourceClaimModelResult = await apiResourceService.GetApiResourceClaimsAsync(apiResourcesClaimModelInput.ApiResourceId);
                apiResourceClaimModelResult.Should().NotBeNull();
                apiResourceClaimModelResult.Count.Should().BeGreaterThan(0);

                // Checking if the list contains the resourceClaims
                apiResourceClaimModelResult.Select(x => x.Type).Contains(apiResourcesClaimModelInput.Type);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Fact]
        [Trait("Category", "DeleteSuccessCase")]
        public async Task DeleteApiResourceClaim_ById_Success()
        {
            try
            {
                // Add an ApiResource.
                var apiResourcesModelInput = ApiResourceHelper.CreateApiResourceModel();
                apiResourcesModelInput.Name = string.Concat(ResourceName, "_", random.Next().ToString());
                FrameworkResult addAPiResourceResult = await apiResourceService.AddApiResourceAsync(apiResourcesModelInput);
                addAPiResourceResult.Should().BeOfType<FrameworkResult>();
                addAPiResourceResult.Status.Should().Be(ResultStatus.Success);

                // Get ApiResource
                ApiResourcesModel apiResourcesModelResult = await apiResourceService.GetApiResourceAsync(apiResourcesModelInput.Name);
                apiResourcesModelResult.Should().NotBeNull();

                var apiResourcesClaimModelInput = ApiResourceHelper.CreateApiResourceClaimModel();
                apiResourcesClaimModelInput.ApiResourceId = apiResourcesModelResult.Id;
                FrameworkResult addApiResourceResult = await apiResourceService.AddApiResourceClaimAsync(apiResourcesClaimModelInput);
                addApiResourceResult.Should().BeOfType<FrameworkResult>();
                addApiResourceResult.Status.Should().Be(ResultStatus.Success);

                string resourceName = apiResourcesModelInput.Name;
                ApiResourcesModel apiResourcesModelResult4 = await apiResourceService.GetApiResourceAsync(resourceName);
                // Delete
                FrameworkResult deleteApiResourceClaimResult = await apiResourceService.DeleteApiResourceClaimAsync(apiResourcesModelResult.Id);
                deleteApiResourceClaimResult.Should().BeOfType<FrameworkResult>();
                deleteApiResourceClaimResult.Status.Should().Be(ResultStatus.Success);

                // Get ApiResourceClaimModel => null
                IList<ApiResourceClaimsModel> apiResourceClaimsResult = await apiResourceService.GetApiResourceClaimsAsync(apiResourcesModelResult.Id);
                apiResourceClaimsResult.Should().BeNullOrEmpty();
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Fact]
        [Trait("Category", "SuccessCase")]
        public async Task GetAPiResource()
        {
            // Arrange
            string resourceName = "AlphaClientOne";
            ApiResourcesModel apiResourcesModelResult = await apiResourceService.GetApiResourceAsync(resourceName);
            apiResourcesModelResult.Should().NotBeNull();

        }

        [Fact]
        [Trait("Category", "DeleteSuccessCase")]
        public async Task DeleteApiResourceClaim_ByClaimModel_Success()
        {
            try
            {
                //string resourcename = "ClientApi_152599900";
                //ApiResourcesModel apiResourcesModelResult = await apiResourceService.GetApiResourceAsync(resourcename);
                //apiResourcesModelResult.Should().NotBeNull();

                // Add an ApiResource.
                var apiResourcesModelInput = ApiResourceHelper.CreateApiResourceModel();
                apiResourcesModelInput.Name = string.Concat(ResourceName, "_", random.Next().ToString());
                FrameworkResult addAPiResourceResult = await apiResourceService.AddApiResourceAsync(apiResourcesModelInput);
                addAPiResourceResult.Should().BeOfType<FrameworkResult>();
                addAPiResourceResult.Status.Should().Be(ResultStatus.Success);

                // Get ApiResource
                ApiResourcesModel apiResourcesModelResult = await apiResourceService.GetApiResourceAsync(apiResourcesModelInput.Name);
                apiResourcesModelResult.Should().NotBeNull();

                // Add ApiResourceClaim
                var apiResourcesClaimModelInput = ApiResourceHelper.CreateApiResourceClaimModel();
                apiResourcesClaimModelInput.ApiResourceId = apiResourcesModelResult.Id;
                apiResourcesClaimModelInput.Type = string.Concat(apiResourcesClaimModelInput.Type, "_", random.Next().ToString());
                FrameworkResult addApiResourceClaimResult = await apiResourceService.AddApiResourceClaimAsync(apiResourcesClaimModelInput);
                addApiResourceClaimResult.Should().BeOfType<FrameworkResult>();
                addApiResourceClaimResult.Status.Should().Be(ResultStatus.Success);

                // Get ApiResourceClaimModel
                IList<ApiResourceClaimsModel> apiResourceClaimsResultList = await apiResourceService.GetApiResourceClaimsAsync(apiResourcesModelResult.Id);
                apiResourceClaimsResultList.Should().NotBeNull();
                ApiResourceClaimsModel apiResourceClaimsModelInput = null;
                foreach (var apiresourceclaim in apiResourceClaimsResultList)
                {
                    if (apiresourceclaim.Type.Equals(apiResourcesClaimModelInput.Type))
                    {
                        apiResourceClaimsModelInput = apiresourceclaim;
                    }
                }

                // Delete ApiResourceClaim by model
                string inputType = apiResourceClaimsModelInput.Type;
                FrameworkResult result = await apiResourceService.DeleteApiResourceClaimAsync(apiResourceClaimsModelInput);
                result.Should().BeOfType<FrameworkResult>();
                result.Status.Should().Be(ResultStatus.Success);

                // Get ApiResourceClaim By Model => Assert as null
                IList<ApiResourceClaimsModel> apiResourceClaimsModelResult = await apiResourceService.GetApiResourceClaimsAsync(apiResourcesModelResult.Id);
                apiResourceClaimsModelResult.Should().NotBeNull();
                apiResourceClaimsModelResult.Where(apiresourceclaim => apiresourceclaim.Type == inputType).Should().BeEmpty();

                // Delete ApiResource
                FrameworkResult deleteApiResourceResult = await apiResourceService.DeleteApiResourceAsync(apiResourcesModelResult.Id);
                deleteApiResourceResult.Should().BeOfType<FrameworkResult>();
                deleteApiResourceResult.Status.Should().Be(ResultStatus.Success);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Fact]
        [Trait("Category", "GetAllSuccessCase")]
        public async Task GetAll_ApiResource_Success()
        {
            try
            {
                List<ApiResourcesModel> apiResource = (List<ApiResourcesModel>)await apiResourceService.GetAllApiResourcesAsync();
                apiResource.Should().NotBeNull();
                apiResource.Should().HaveCountGreaterThan(0);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Fact]
        [Trait("Category", "GetAllSuccessCase")]
        public async Task GetAll_ApiScope_Success()
        {
            try
            {
                List<ApiScopesModel> apiResource = (List<ApiScopesModel>)await apiResourceService.GetAllApiScopesAsync();
                apiResource.Should().NotBeNull();
                apiResource.Should().HaveCountGreaterThan(0);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region ERROR SCENARIOS
        /* APIRESOURCE */

        [Fact]
        [Trait("Category", "AddErrorCase")]
        public async Task AddApiResourceAsync_NameIsNull_ReturnsError()
        {
            try
            {
                var apiResourcesModel = ApiResourceHelper.CreateApiResourceModel();
                apiResourcesModel.Name = null;
                FrameworkResult result = await apiResourceService.AddApiResourceAsync(apiResourcesModel);
                result.Should().BeOfType<FrameworkResult>();
                result.Status.Should().Be(ResultStatus.Failed);
                result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.ApiResourceNameRequired);
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
            try
            {
                var apiResourcesModel = ApiResourceHelper.CreateApiResourceModel();
                apiResourcesModel.Name = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";

                FrameworkResult result = await apiResourceService.AddApiResourceAsync(apiResourcesModel);
                result.Should().BeOfType<FrameworkResult>();
                result.Status.Should().Be(ResultStatus.Failed);
                result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.ApiResourceNameTooLong);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Fact]
        [Trait("Category", "AddErrorCase")]
        public async Task AddApiResourceAsync_DisplayName_LengthExceededLimit_DBExceptionThrown()
        {
            try
            {
                var apiResourcesModel = ApiResourceHelper.CreateApiResourceModel();
                apiResourcesModel.Name = string.Concat(ResourceName, "_", random.Next().ToString());
                apiResourcesModel.DisplayName = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
                FrameworkResult result = await apiResourceService.AddApiResourceAsync(apiResourcesModel);
                result.Should().NotBeNull();
                result.Status.Should().Be(ResultStatus.Failed);
                result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.ApiResourceDisplayNameTooLong);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Fact]
        [Trait("Category", "AddErrorCase")]
        public async Task AddApiResourceAsync_DuplicateExists_ForGivenResourceName_ReturnsError_AddFailure()
        {
          await CheckApiResoucerun();
          var apiResourcesModel = ApiResourceHelper.CreateApiResourceModel();
          apiResourcesModel.Name = ResourceName;
          FrameworkResult result = await apiResourceService.AddApiResourceAsync(apiResourcesModel);
          result.Should().BeOfType<FrameworkResult>();
          result.Status.Should().Be(ResultStatus.Failed);
        }

        /* APISCOPES */

        [Fact]
        [Trait("Category", "AddErrorCase")]
        public async Task AddApiScope_Name_IsEmpty_ErrorReturned_AddFailure()
        {
            // Get ApiResource by name
            ApiResourcesModel apiResourceModelResult = await apiResourceService.GetApiResourceAsync(ResourceName);
            apiResourceModelResult.Should().NotBeNull();

            // Add Scopes : Arrange, Act , Assert
            var apiScopeModelInput = ApiResourceHelper.CreateApiScopeModel();
            apiScopeModelInput.ApiResourceId = apiResourceModelResult.Id;
            apiScopeModelInput.Name = string.Empty;
            FrameworkResult result = await apiResourceService.AddApiScopeAsync(apiScopeModelInput);
            result.Should().BeOfType<FrameworkResult>();
            result.Status.Should().Be(ResultStatus.Failed);
            result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.ApiScopeNameRequired);
        }

        [Fact]
        [Trait("Category", "AddErrorCase")]
        public async Task AddApiScope_ApiScopeName_LengthExceeded_ThrowDbException()
        {
            // Get model by name: Arrange , Act, Assert
            ApiResourcesModel apiResourceModelResult = await apiResourceService.GetApiResourceAsync(ResourceName);
            apiResourceModelResult.Should().NotBeNull();
            // Add Scopes : Arrange, Act , Assert
            var apiScopeModelInput = ApiResourceHelper.CreateApiScopeModel();
            apiScopeModelInput.ApiResourceId = apiResourceModelResult.Id;
            apiScopeModelInput.Name = "hhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllll";
            // Assert
            FrameworkResult result = await apiResourceService.AddApiScopeAsync(apiScopeModelInput);
            result.Should().BeOfType<FrameworkResult>();
            result.Status.Should().Be(ResultStatus.Failed);
            result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.ApiScopeNameTooLong);
        }

        [Fact]
        [Trait("Category", "AddErrorCase")]
        public async Task AddApiScope_DuplicateExists_ReturnsError_AddFailure()
        {
            // Get model by name: Arrange , Act, Assert
            ApiResourcesModel apiResourceModelResult = await apiResourceService.GetApiResourceAsync(ResourceName);
            apiResourceModelResult.Should().NotBeNull();

            // Add Scopes : Arrange, Act , Assert
            var apiScopeModelInput = ApiResourceHelper.CreateApiScopeModel();
            apiScopeModelInput.ApiResourceId = apiResourceModelResult.Id;

            // Assigning already existing scope name.
            apiScopeModelInput.Name = apiResourceModelResult.ApiScopes.FirstOrDefault().Name;
            FrameworkResult result = await apiResourceService.AddApiScopeAsync(apiScopeModelInput);
            result.Should().BeOfType<FrameworkResult>();
            result.Status.Should().Be(ResultStatus.Failed);
            result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.ApiScopeAlreadyExists);
        }

        /* ApiScopeClaims */

        [Fact]
        [Trait("Category", "AddErrorCase")]
        public async Task AddApiScopeClaimAsyn_ApiScopeClaimType_LengthExceeded_ThrowDbException()
        {
            // Get model by name: Arrange , Act, Assert
            ApiResourcesModel apiResourceModelResult = await apiResourceService.GetApiResourceAsync(ResourceName);
            apiResourceModelResult.Should().NotBeNull();

            // Get apiscopeModelResult by scopeName
            ApiScopesModel apiscopeModelResult = await apiResourceService.GetApiScopeAsync(apiResourceModelResult.ApiScopes[0].Name);
            apiscopeModelResult.Should().NotBeNull();

            // Add ApiScopeClaimAsync with get ApiscopeId
            var apiScopeClaimModelInput = ApiResourceHelper.CreateApiScopeClaimModel();
            apiScopeClaimModelInput.ApiScopeId = apiscopeModelResult.Id;
            apiScopeClaimModelInput.Type = "hhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllll";

            // Assert
            FrameworkResult result = await apiResourceService.AddApiScopeClaimAsync(apiScopeClaimModelInput);
            result.Should().BeOfType<FrameworkResult>();
            result.Status.Should().Be(ResultStatus.Failed);
            result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.ApiScopeClaimTypeTooLong);
        }

        [Fact]
        [Trait("Category", "AddErrorCase")]
        public async Task AddApiScopeClaimAsyn_DuplicateExists_ForScopeIdAndType_ReturnsError_AddFailure()
        {
            await CheckApiResoucerun();
            // Get model by name: Arrange , Act, Assert
            ApiResourcesModel apiResourceModelResult = await apiResourceService.GetApiResourceAsync(ResourceName);
            apiResourceModelResult.Should().NotBeNull();

            // Get apiscopeModelResult by scopeName
            ApiScopesModel apiscopeModelResult = await apiResourceService.GetApiScopeAsync(apiResourceModelResult.ApiScopes[0].Name);
            apiscopeModelResult.Should().NotBeNull();

            // Add ApiScopeClaimAsync with get ApiscopeId
            var apiScopeClaimModelInput = ApiResourceHelper.CreateApiScopeClaimModel();
            apiScopeClaimModelInput.ApiScopeId = apiscopeModelResult.Id;
            apiScopeClaimModelInput.Type = apiscopeModelResult.ApiScopeClaims[0].Type;
            FrameworkResult result = await apiResourceService.AddApiScopeClaimAsync(apiScopeClaimModelInput);
            result.Should().BeOfType<FrameworkResult>();
            result.Status.Should().Be(ResultStatus.Failed);
            result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.ApiScopeClaimsAlreadyExists);
        }

        /* ApiResourceClaims */

        [Fact]
        [Trait("Category", "AddErrorCase")]
        public async Task AddApiResourceClaim_DuplicateExists_ForResourceIdAndType_ReturnsError_AddFailure()
        {
            await CheckApiResoucerun();
            // Get ApiResource By Resource Name
            ApiResourcesModel apiResourceResult = await apiResourceService.GetApiResourceAsync(ResourceName);
            apiResourceResult.Should().NotBeNull();

            // Add ApiResourceClaim using the ResourceId from the GetApiResource 
            // Assert for Duplicate Records
            var apiResourcesClaimModelInput = ApiResourceHelper.CreateApiResourceClaimModel();
            apiResourcesClaimModelInput.ApiResourceId = apiResourceResult.Id;
            apiResourcesClaimModelInput.Type = "Type3";
            FrameworkResult result = await apiResourceService.AddApiResourceClaimAsync(apiResourcesClaimModelInput);
            result.Should().BeOfType<FrameworkResult>();
            result.Status.Should().Be(ResultStatus.Failed);
            result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.ApiResourceClaimAlreadyExists);
        }

        [Fact]
        [Trait("Category", "AddErrorCase")]
        public async Task AddApiResourceClaim_Type_LengthExceedsLimit_ThrowException()
        {
            // Get ApiResource By Resource Name
            ApiResourcesModel apiResourceResult = await apiResourceService.GetApiResourceAsync(ResourceName);
            apiResourceResult.Should().NotBeNull();

            // Add ApiResourceClaim using the ResourceId from the GetApiResource
            var apiResourcesClaimModelInput = ApiResourceHelper.CreateApiResourceClaimModel();
            apiResourcesClaimModelInput.ApiResourceId = apiResourceResult.Id;
            apiResourcesClaimModelInput.Type = "hhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllll";

            FrameworkResult result = await apiResourceService.AddApiResourceClaimAsync(apiResourcesClaimModelInput);
            result.Should().BeOfType<FrameworkResult>();
            result.Status.Should().Be(ResultStatus.Failed);
            result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.ApiResourceClaimTypeTooLong);
        }

        /* APIRESOURCE */

        [Fact]
        [Trait("Category", "UpdateErrorCase")]
        public async Task UpdateApiResource_ConcurrencyError()
        {
            // Arrange
            ApiResourcesModel apiResourcesModel1 = await apiResourceService.GetApiResourceAsync(ResourceName);
            apiResourcesModel1.Should().NotBeNull();
            ApiResourcesModel apiResourcesModel2 = await apiResourceService.GetApiResourceAsync(ResourceName);
            apiResourcesModel2.DisplayName = "Test1";
            apiResourcesModel2.ModifiedBy = "Roshan";
            apiResourcesModel1.Description = "Jesu";
            apiResourcesModel1.DisplayName = "Jesu";
            apiResourcesModel1.ModifiedBy = "Jesu";
            // Act
            FrameworkResult result1 = await apiResourceService.UpdateApiResourceAsync(apiResourcesModel2);
            result1.Status.Should().Be(ResultStatus.Success);
            FrameworkResult result2 = await apiResourceService.UpdateApiResourceAsync(apiResourcesModel1);
            result2.Status.Should().Be(ResultStatus.Failed);
            result2.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.ConcurrencyFailure);
        }

        [Fact]
        [Trait("Category", "UpdateErrorCase")]
        public async Task UpdateApiResource_Name_LengthExceedsLimit_ThrowsException()
        {
            // Get ApiResource By Name
            var apiResourceInput = await apiResourceService.GetApiResourceAsync(ResourceName);
            apiResourceInput.Should().NotBeNull();

            // Update Api Resource With Name Exceeding character limit(510)
            apiResourceInput.Name = "hhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhhccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccccllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllllll";

            FrameworkResult result = await apiResourceService.UpdateApiResourceAsync(apiResourceInput);
            result.Should().BeOfType<FrameworkResult>();
            result.Status.Should().Be(ResultStatus.Failed);
            result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.ApiResourceNameTooLong);
        }

        [Fact]
        [Trait("Category", "UpdateErrorCase")]
        public async Task UpdateApiResource_NoActiveRecordsFound_ForGivenResourceID_ReturnsError()
        {
            // Add an ApiResource.
            var apiResourcesModelInput = ApiResourceHelper.CreateApiResourceModel();
            apiResourcesModelInput.Name = string.Concat(ResourceName, "_", random.Next().ToString());
            FrameworkResult addAPiResourceResult = await apiResourceService.AddApiResourceAsync(apiResourcesModelInput);
            addAPiResourceResult.Should().BeOfType<FrameworkResult>();
            addAPiResourceResult.Status.Should().Be(ResultStatus.Success);

            // Get ApiResource
            ApiResourcesModel apiResourcesModelResult = await apiResourceService.GetApiResourceAsync(apiResourcesModelInput.Name);
            apiResourcesModelResult.Should().NotBeNull();

            // Delete API Resource by Id
            FrameworkResult deleteApiResourceResult = await apiResourceService.DeleteApiResourceAsync(apiResourcesModelResult.Id);
            deleteApiResourceResult.Should().BeOfType<FrameworkResult>();
            deleteApiResourceResult.Status.Should().Be(ResultStatus.Success);

            // Update Api Resource with the getApiResource_ByName_Model
            apiResourcesModelResult.DisplayName = "AlphaOne";
            FrameworkResult updateApiResourceResult = await apiResourceService.UpdateApiResourceAsync(apiResourcesModelResult);
            updateApiResourceResult.Should().BeOfType<FrameworkResult>();
            updateApiResourceResult.Status.Should().Be(ResultStatus.Failed);
            updateApiResourceResult.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.ApiResourceIdInvalid);
        }

        /* ApiScope */

        [Fact]
        [Trait("Category", "UpdateErrorCase")]
        public async Task UpdateApiScope__NoActiveRecordsFoundForScopeName_ReturnsError_InvalidName()
        {
            // Get ApiResource By Name
            var apiResourceResult = await apiResourceService.GetApiResourceAsync(ResourceName);
            apiResourceResult.Should().NotBeNull();

            // Add ApiScope for the ApiResource
            var apiScopeModelInput = ApiResourceHelper.CreateApiScopeModel();
            apiScopeModelInput.Name = string.Concat(ScopeName, "_", random.Next().ToString());
            apiScopeModelInput.ApiResourceId = apiResourceResult.Id;
            FrameworkResult addApiScoperesult = await apiResourceService.AddApiScopeAsync(apiScopeModelInput);
            addApiScoperesult.Should().BeOfType<FrameworkResult>();
            addApiScoperesult.Status.Should().Be(ResultStatus.Success);

            // Get ApiScopes as per Scope Name
            ApiScopesModel apiScopeInitialResult = await apiResourceService.GetApiScopeAsync(apiScopeModelInput.Name);
            apiScopeInitialResult.Should().NotBeNull();

            // Delete APiScope By Name
            FrameworkResult deleteApiScopeResult = await apiResourceService.DeleteApiScopeAsync(apiScopeInitialResult.Name);
            deleteApiScopeResult.Should().BeOfType<FrameworkResult>();
            deleteApiScopeResult.Status.Should().Be(ResultStatus.Success);

            // Update ApiScope
            apiScopeInitialResult.DisplayName = "Restful Api";
            FrameworkResult result = await apiResourceService.UpdateApiScopeAsync(apiScopeInitialResult);
            result.Should().BeOfType<FrameworkResult>();
            result.Status.Should().Be(ResultStatus.Failed);
            result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.ApiScopeNameInvalid);
        }

        [Fact]
        [Trait("Category", "UpdateErrorCase")]
        public async Task UpdateApiScope_ConcurrencyError()
        {
            // Get ApiScopeModel1 by ScopeName
            ApiScopesModel apiScopesModel1 = await apiResourceService.GetApiScopeAsync(ScopeName);
            apiScopesModel1.Should().NotBeNull();

            // Get ApiScopeModel2 by ScopeName
            ApiScopesModel apiScopesModel2 = await apiResourceService.GetApiScopeAsync(ScopeName);
            apiScopesModel2.Should().NotBeNull();

            // Update Properties of Model2
            apiScopesModel2.Description = "Test1";
            FrameworkResult updateApiScopeResult2 = await apiResourceService.UpdateApiScopeAsync(apiScopesModel2);
            updateApiScopeResult2.Should().BeOfType<FrameworkResult>();
            updateApiScopeResult2.Status.Should().Be(ResultStatus.Success);

            // Update Properties of model1
            apiScopesModel1.DisplayName = "TestDisplayName1";
            FrameworkResult updateApiScopeResult1 = await apiResourceService.UpdateApiScopeAsync(apiScopesModel1);
            updateApiScopeResult1.Should().BeOfType<FrameworkResult>();
            updateApiScopeResult1.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.ConcurrencyFailure);

        }

        /* ApiResource */

        [Fact]
        [Trait("Category", "DeleteErrorCase")]
        public async Task DeleteApiResource_ByName_NoRecordsFound_ReturnsError_InvalidResourceName()
        {
            try
            {
                // Delete API Resource by invalid resourceName
                string invalidResourceName = string.Concat(ResourceName, "_", random.Next().ToString());
                FrameworkResult result = await apiResourceService.DeleteApiResourceAsync(invalidResourceName);
                result.Should().BeOfType<FrameworkResult>();
                result.Status.Should().Be(ResultStatus.Failed);
                result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.ApiResourceNameInvalid);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Fact]
        [Trait("Category", "DeleteErrorCase")]
        public async Task DeleteApiResource_ById_NoRecordsFound_ReturnsError_InvalidResourceId()
        {
            try
            {
                // Arrange
                Guid resourceId = Guid.NewGuid();
                // Act
                FrameworkResult result = await apiResourceService.DeleteApiResourceAsync(resourceId);
                // Assert
                result.Should().BeOfType<FrameworkResult>();
                result.Status.Should().Be(ResultStatus.Failed);
                result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.ApiResourceIdInvalid);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /* ApiResourceClaims */

        [Fact]
        [Trait("Category", "DeleteErrorCase")]
        public async Task DeleteApiResourceClaim_ById_NoRecordsFound_ReturnsError_InvalidResourceId()
        {
            // Arrange
            Guid invalidResourceIdInput = Guid.NewGuid();
            // Act
            FrameworkResult result = await apiResourceService.DeleteApiResourceClaimAsync(invalidResourceIdInput);
            // Assert
            result.Should().BeOfType<FrameworkResult>();
            result.Status.Should().Be(ResultStatus.Failed);
            result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.ApiResourceIdInvalid);
        }

        [Fact]
        [Trait("Category", "DeleteErrorCase")]
        public async Task DeleteApiResourceClaim_ByClaimModel_NoRecordsFoundForResourceIdAndType_ReturnsError()
        {
            // Add an ApiResource.
            var apiResourcesModelInput = ApiResourceHelper.CreateApiResourceModel();
            apiResourcesModelInput.Name = string.Concat(ResourceName, "_", random.Next().ToString());
            FrameworkResult addAPiResourceResult = await apiResourceService.AddApiResourceAsync(apiResourcesModelInput);
            addAPiResourceResult.Should().BeOfType<FrameworkResult>();
            addAPiResourceResult.Status.Should().Be(ResultStatus.Success);

            // Get ApiResource
            ApiResourcesModel apiResourcesModelResult = await apiResourceService.GetApiResourceAsync(apiResourcesModelInput.Name);
            apiResourcesModelResult.Should().NotBeNull();

            // Get ApiResourceClaim by Resource Id
            IList<ApiResourceClaimsModel> apiResourceClaimResult = await apiResourceService.GetApiResourceClaimsAsync(apiResourcesModelResult.Id);
            apiResourceClaimResult.Should().NotBeNull();
            ApiResourceClaimsModel apiResourceClaimsModelInput = apiResourceClaimResult.FirstOrDefault();

            // Delete by invalid ResoureId => Assert No Records Found and Error: InvalidApiResourceClaimTypeOrResourceId
            apiResourceClaimsModelInput.ApiResourceId = Guid.NewGuid();
            FrameworkResult resultOne = await apiResourceService.DeleteApiResourceClaimAsync(apiResourceClaimsModelInput);
            resultOne.Should().BeOfType<FrameworkResult>();
            resultOne.Status.Should().Be(ResultStatus.Failed);
            resultOne.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.InvalidApiResourceClaimTypeOrResourceId);

            // Delete by invalid Type => Assert No Records Found and Error: InvalidApiResourceClaimTypeOrResourceId
            apiResourceClaimsModelInput.Type = string.Concat(apiResourceClaimsModelInput.Type, "_", random.Next().ToString());
            FrameworkResult resultTwo = await apiResourceService.DeleteApiResourceClaimAsync(apiResourceClaimsModelInput);
            resultTwo.Should().BeOfType<FrameworkResult>();
            resultTwo.Status.Should().Be(ResultStatus.Failed);
            resultTwo.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.InvalidApiResourceClaimTypeOrResourceId);
        }

        /* ApiScope */

        [Fact]
        [Trait("Category", "DeleteErrorCase")]
        public async Task DeleteApiScope_ById_NoRecordsForScopeId_ReturnError()
        {
            try
            {
                // Arrange
                Guid invalidApiScopeId = Guid.NewGuid();
                // Act
                FrameworkResult result = await apiResourceService.DeleteApiScopeAsync(invalidApiScopeId);
                // Assert
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
        [Trait("Category", "DeleteErrorCase")]
        public async Task DeleteApiScope_ByName_NoRecordsForScopeName_ReturnError()
        {
            try
            {
                // Arrange
                string invalidApiScopeName = string.Concat("CR7", "_", random.Next().ToString());
                // Act
                FrameworkResult result = await apiResourceService.DeleteApiScopeAsync(invalidApiScopeName);
                // Assert
                result.Should().BeOfType<FrameworkResult>();
                result.Status.Should().Be(ResultStatus.Failed);
                result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.ApiScopeNameInvalid);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /* ApiScopeClaim */

        [Fact]
        [Trait("Category", "DeleteErrorCase")]
        public async Task DeleteApiScopeClaim_ByScopeId_NoRecordsFound_Returns_InvalidScopeId_Error()
        {
            // Arrange
            Guid invalidApiScopeId = Guid.NewGuid();

            // Act
            FrameworkResult result = await apiResourceService.DeleteApiScopeClaimAsync(invalidApiScopeId);
            result.Should().BeOfType<FrameworkResult>();
            result.Status.Should().Be(ResultStatus.Failed);
            result.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.ApiScopeIdInvalid);
        }

        [Fact]
        [Trait("Category", "DeleteErrorCase")]
        public async Task DeleteApiScopeClaim_ByClaimModel_NoRecordsFound_ForScopeIdAndType_ReturnsError_InvalidApiScopeClaimTypeOrScopeId()
        {
            await CheckApiResoucerun();
            // Get ApiResource By Name
            var apiResourceResult = await apiResourceService.GetApiResourceAsync(ResourceName);
            apiResourceResult.Should().NotBeNull();

            // Get Scope
            string apiScopeName = apiResourceResult.ApiScopes.FirstOrDefault().Name;
            ApiScopesModel apiScopeModelResult = await apiResourceService.GetApiScopeAsync(apiScopeName);
            apiScopeModelResult.Should().NotBeNull();

            // Add ApiScopeClaimAsync with get ApiscopeId
            var apiScopeClaimModelInput = ApiResourceHelper.CreateApiScopeClaimModel();
            apiScopeClaimModelInput.ApiScopeId = apiScopeModelResult.Id;
            apiScopeClaimModelInput.Type = string.Concat("name", "_", random.Next().ToString());
            FrameworkResult addApiScopeClaimResult = await apiResourceService.AddApiScopeClaimAsync(apiScopeClaimModelInput);
            addApiScopeClaimResult.Should().BeOfType<FrameworkResult>();
            addApiScopeClaimResult.Status.Should().Be(ResultStatus.Success);

            // Delete ApiScopeClaim by Model
            apiScopeClaimModelInput.Type = random.Next().ToString();
            FrameworkResult deleteAPiScopeClaimsResult = await apiResourceService.DeleteApiScopeClaimAsync(apiScopeClaimModelInput);
            deleteAPiScopeClaimsResult.Should().BeOfType<FrameworkResult>();
            deleteAPiScopeClaimsResult.Status.Should().Be(ResultStatus.Failed);
            deleteAPiScopeClaimsResult.Errors.FirstOrDefault().Code.Should().Be(ApiErrorCodes.InvalidApiScopeClaimTypeOrScopeId);
        }

        private async Task CheckApiResoucerun()
        {
            ApiResourcesModel apiResourcesModelsList = await apiResourceService.GetApiResourceAsync(ResourceName);
            if (apiResourcesModelsList == null)
            {
                var apiResourcesModel = ApiResourceHelper.CreateApiResourceModel();
                apiResourcesModel.Name = ResourceName;
                FrameworkResult result = await apiResourceService.AddApiResourceAsync(apiResourcesModel);

                result.Should().BeOfType<FrameworkResult>();
                result.Status.Should().Be(ResultStatus.Success);
            }
        }
        #endregion
    }
}


