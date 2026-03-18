/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Security.Claims;
using FluentAssertions;
using Xunit;
using HCL.CS.SF.DemoServerApp.Services.ExternalAuth;

namespace HCL.CS.SF.UnitTests;

public class GoogleExternalAuthProviderTests
{
    [Fact]
    public void ParseIdentity_ShouldMapClaims()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("iss", "https://accounts.google.com"),
            new Claim("sub", "google-subject-1"),
            new Claim("email", "alice@example.com"),
            new Claim("email_verified", "true"),
            new Claim("name", "Alice Example")
        }, "google"));

        var provider = new GoogleExternalAuthProvider();
        var payload = provider.ParseIdentity(principal);

        payload.Issuer.Should().Be("https://accounts.google.com");
        payload.Subject.Should().Be("google-subject-1");
        payload.Email.Should().Be("alice@example.com");
        payload.EmailVerified.Should().BeTrue();
        payload.DisplayName.Should().Be("Alice Example");
    }

    [Fact]
    public void ParseIdentity_ShouldFallbackForMissingClaims()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "fallback-sub")
        }, "google"));

        var provider = new GoogleExternalAuthProvider();
        var payload = provider.ParseIdentity(principal);

        payload.Subject.Should().Be("fallback-sub");
        payload.Issuer.Should().Be("https://accounts.google.com");
        payload.Email.Should().BeEmpty();
        payload.EmailVerified.Should().BeFalse();
    }

    [Fact]
    public void CanHandle_ShouldMatchGoogleOnly()
    {
        var provider = new GoogleExternalAuthProvider();

        provider.CanHandle("Google").Should().BeTrue();
        provider.CanHandle("google").Should().BeTrue();
        provider.CanHandle("Ldap").Should().BeFalse();
    }
}
