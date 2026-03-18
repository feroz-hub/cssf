/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace IntegrationTests.Endpoint.Setup;

public class MockExternalAuthenticationHandler :
    IAuthenticationHandler,
    IAuthenticationSignInHandler,
    IAuthenticationRequestHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;


    public Func<HttpContext, Task<bool>> OnFederatedSignout =
        async context =>
        {
            await context.SignOutAsync();
            return true;
        };

    public MockExternalAuthenticationHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private HttpContext HttpContext => _httpContextAccessor.HttpContext;

    // TODO-Session_Id to be tested with real application how it is passed from user login
    public Task<AuthenticateResult> AuthenticateAsync()
    {
        if (!string.IsNullOrEmpty(GlobalConfiguration.UserName) && !string.IsNullOrEmpty(GlobalConfiguration.UserId))
        {
            var claimsPrincipal = new FrameworkUser(GlobalConfiguration.UserId, GlobalConfiguration.UserName)
                .CreatePrincipal();
            var authenticationScheme = "Security.Identity";
            IDictionary<string, string> dict = new Dictionary<string, string>
            {
                { "session_id", "TestSessionId" }
            };
            var properties = new AuthenticationProperties(dict);

            return Task.FromResult(
                AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, properties,
                    authenticationScheme)));
        }

        return Task.FromResult(AuthenticateResult.NoResult());
    }

    public Task ChallengeAsync(AuthenticationProperties properties)
    {
        return Task.CompletedTask;
    }

    public Task ForbidAsync(AuthenticationProperties properties)
    {
        return Task.CompletedTask;
    }

    public Task InitializeAsync(AuthenticationScheme scheme, HttpContext context)
    {
        return Task.CompletedTask;
    }

    public Task<bool> HandleRequestAsync()
    {
        //if (HttpContext.Request.Path == IdentityServerPipeline.FederatedSignOutPath)
        //{
        //    return await OnFederatedSignout(HttpContext);
        //}

        return Task.FromResult(false);
    }

    public Task SignInAsync(ClaimsPrincipal user, AuthenticationProperties properties)
    {
        return Task.CompletedTask;
    }

    public Task SignOutAsync(AuthenticationProperties properties)
    {
        return Task.CompletedTask;
    }
}
