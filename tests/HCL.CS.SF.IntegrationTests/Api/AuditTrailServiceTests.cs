/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Enums;
using HCL.CS.SF.Domain.Models;
using HCL.CS.SF.Service.Interfaces;
using HCL.CS.SF.TestApp.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HCL.CS.SF.IntegrationTests.Api
{
    public class AuditTrailServiceTests : HCLCSSFFakeSetup
    {
        private readonly IAuditTrailService auditTrailService;

        public AuditTrailServiceTests()
        {
            auditTrailService = ServiceProvider.GetService<IAuditTrailService>();
        }

    }
}



