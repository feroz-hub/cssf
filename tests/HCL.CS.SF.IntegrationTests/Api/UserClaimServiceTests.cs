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
using Newtonsoft.Json.Linq;
using Xunit;

namespace HCL.CS.SF.IntegrationTests.Api
{
   public class UserClaimServiceTests : HCLCSSFFakeSetup
    {
        private readonly IUserAccountService userAccountService;
        private readonly IAuditTrailService auditTrailService;

        public UserClaimServiceTests()
        {
            userAccountService = ServiceProvider.GetService<IUserAccountService>();
            auditTrailService = ServiceProvider.GetService<IAuditTrailService>();
        }


        [Fact]
        [Trait("Category", "SuccessCase")]
        public async Task AddClaimAsync_Success()
        {
            Random random = new Random();
            UserClaimModel userClaimModel = UserHelper.CreateUserClaimModel();
            Guid userid = new Guid("1026C140-2813-4FB8-A20D-E45834987AD7");
            userClaimModel.ClaimType = userClaimModel.ClaimType + string.Empty + random.Next();
            userClaimModel.UserId = userid;
            FrameworkResult result = await userAccountService.AddClaimAsync(userClaimModel);
            result.Should().BeOfType<FrameworkResult>();
            result.Status.Should().Be(ResultStatus.Success);

            // Audit
            DateTime fromDate = DateTime.Today;
            DateTime toDate = DateTime.Today.AddDays(1);

            PagingModel page = new PagingModel()
            {
                CurrentPage = 1,
                ItemsPerPage = 1000,
            };

            AuditResponseModel auditResponseModelResult = await auditTrailService.GetAuditDetailsAsync(userClaimModel.CreatedBy, fromDate, toDate, page);
            auditResponseModelResult.Should().NotBeNull();

            var resultaudit = auditResponseModelResult.AuditList.Find(i => i.NewValue.Contains(userClaimModel.ClaimType));
            resultaudit.Should().NotBeNull();
        }




        [Fact]
        [Trait("Category", "SuccessCase")]
        public async Task AddUserClaimAsync_SuccessCase()
        {
            Random random = new Random();
            UserClaimModel userClaimModel = UserHelper.CreateUserClaimModel();
            Guid userid = new Guid("1026C140-2813-4FB8-A24D-E40834986AD7");
            userClaimModel.ClaimType = userClaimModel.ClaimType + string.Empty + random.Next();
            userClaimModel.UserId = userid;
            FrameworkResult result = await userAccountService.AddClaimAsync(userClaimModel);
            result.Should().BeOfType<FrameworkResult>();
            result.Status.Should().Be(ResultStatus.Success);
        }


        [Fact]
        [Trait("Category", "SuccessCase")]
        public async Task AddClaimAsyncbyList_Success()
        {
            Guid userid = new Guid("1026C140-2813-4FB8-A24D-E40834986AD7");
            IList< UserClaimModel> userClaimModel = UserHelper.CreateUserClaimModel_List(userid);
            FrameworkResult result = await userAccountService.AddClaimAsync(userClaimModel);
            result.Should().BeOfType<FrameworkResult>();
            result.Status.Should().Be(ResultStatus.Success);


            // Audit
            DateTime fromDate = DateTime.Today;
            DateTime toDate = DateTime.Today.AddDays(1);

            PagingModel page = new PagingModel()
            {
                CurrentPage = 1,
                ItemsPerPage = 1000,
            };

            AuditResponseModel auditResponseModelResult = await auditTrailService.GetAuditDetailsAsync(userClaimModel[0].CreatedBy, fromDate, toDate, page);
            auditResponseModelResult.Should().NotBeNull();
            var resultaudit = auditResponseModelResult.AuditList.Find(i => i.NewValue.Contains(userClaimModel[0].ClaimType));
            resultaudit.Should().NotBeNull();

        }



        [Fact]
        [Trait("Category", "SuccessCase")]
        public async Task RemoveClaimAsync_Success()
        {
            Guid userid = new Guid("1026C140-2813-4FB8-A24D-E40834986AD7");
            IList<UserClaimModel> userClaimModel = UserHelper.CreateUserClaimModel_List(userid);
            FrameworkResult result = await userAccountService.RemoveClaimAsync(userClaimModel);
            result.Should().BeOfType<FrameworkResult>();
            result.Status.Should().Be(ResultStatus.Success);


            // Audit
            DateTime fromDate = DateTime.Today;
            DateTime toDate = DateTime.Today.AddDays(1);

            PagingModel page = new PagingModel()
            {
                CurrentPage = 1,
                ItemsPerPage = 1000,
            };

            AuditResponseModel auditResponseModelResult = await auditTrailService.GetAuditDetailsAsync(userClaimModel[0].CreatedBy, fromDate, toDate, page);
            auditResponseModelResult.Should().NotBeNull();
        }

        [Fact]
        [Trait("Category", "SuccessCase")]
        public async Task RemoveClaimAsyncbymodle_Success()
        {
            Random random = new Random();
            UserClaimModel userClaimModel = UserHelper.CreateUserClaimModel();
            Guid userid = new Guid("1026C140-2813-4FB8-A24D-E40834986AD7");
            userClaimModel.ClaimType = userClaimModel.ClaimType + string.Empty + random.Next();
            userClaimModel.UserId = userid;
            FrameworkResult result = await userAccountService.AddClaimAsync(userClaimModel);
            result.Should().BeOfType<FrameworkResult>();
            result.Status.Should().Be(ResultStatus.Success);

            // Remove Claim
            FrameworkResult result1 = await userAccountService.RemoveClaimAsync(userClaimModel);
            result1.Should().BeOfType<FrameworkResult>();
            result1.Status.Should().Be(ResultStatus.Success);

            // Audit
            DateTime fromDate = DateTime.Today;
            DateTime toDate = DateTime.Today.AddDays(1);

            PagingModel page = new PagingModel()
            {
                CurrentPage = 1,
                ItemsPerPage = 1000,
            };

            AuditResponseModel auditResponseModelResult = await auditTrailService.GetAuditDetailsAsync(userClaimModel.CreatedBy, fromDate, toDate, page);
            auditResponseModelResult.Should().NotBeNull();

            var resultaudit = auditResponseModelResult.AuditList.Find(i => i.NewValue.Contains(userClaimModel.ClaimType));
            resultaudit.Should().NotBeNull();
        }

    }
}



