/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Entities.Api;
using HCL.CS.SF.Infrastructure.Data;

namespace IntegrationTests.ExternalAuth;

public class ExternalIdentityPersistenceTests : HCLCSSFFakeSetup
{
    [Fact]
    public async Task ExternalIdentity_UniqueProviderIssuerSubject_MustEnforceUniqueness()
    {
        using var scope = ServiceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var firstUser = await dbContext.Users.FirstAsync();
        var secondUser = await dbContext.Users.OrderBy(user => user.Id).Skip(1).FirstAsync();
        var issuer = "https://accounts.google.com";
        var subject = "google-subject-duplicate";

        dbContext.ExternalIdentities.Add(new ExternalIdentities
        {
            Id = Guid.NewGuid(),
            UserId = firstUser.Id,
            Provider = "Google",
            Issuer = issuer,
            Subject = subject,
            Email = "one@example.com",
            EmailVerified = true,
            TenantId = "tenant-a",
            LinkedAt = DateTime.UtcNow,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "Test"
        });

        await dbContext.SaveChangesAsync();

        dbContext.ExternalIdentities.Add(new ExternalIdentities
        {
            Id = Guid.NewGuid(),
            UserId = secondUser.Id,
            Provider = "Google",
            Issuer = issuer,
            Subject = subject,
            Email = "two@example.com",
            EmailVerified = true,
            TenantId = "tenant-b",
            LinkedAt = DateTime.UtcNow,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "Test"
        });

        var result = await dbContext.SaveChangesAsync();

        result.Status.Should().Be(ResultStatus.Failed);
        result.Errors.Should().NotBeNull();

        var duplicateCount = dbContext.ExternalIdentities.Count(identity =>
            identity.Provider == "Google" &&
            identity.Issuer == issuer &&
            identity.Subject == subject);

        duplicateCount.Should().Be(1);
    }
}
