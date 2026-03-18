/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

/**
* @page MVC_ClientApplication MVC Client application
* <p><strong>Client Application configuration</strong></p>
* For configuring the HCL.CS.SF in the client web application,
* <p><strong>Add Authentication</strong></p>
* <p>Authentication is the process of determining a user's identity. Authorization is the process of determining whether a user has access to a resource. In ASP.NET Core, authentication is handled by the authentication service,<strong> IAuthenticationService</strong>, which is used by authentication&nbsp; middleware . The authentication service uses registered authentication handlers to complete authentication-related actions.</p>
* <p>Examples of authentication-related actions include:</p>
* <ul>
* <li>Authenticating a user.</li>
* <li>Responding when an unauthenticated user tries to access a restricted resource.</li>
* </ul>
* <p>The registered authentication handlers and their configuration options are called "schemes".</p>
* <p>Authentication schemes are specified by registering authentication services in <strong>Startup.ConfigureServices</strong></p>
* <p>The services.AddAuthentication() method registers services required by authentication services and configures the AuthenticationOptions </p>
* <p>Schemes are useful as a mechanism for referring to the authentication, challenge, and forbid behaviors of the associated handler. For example, an authorization policy can use scheme names to specify which authentication scheme (or schemes) should be used to authenticate the user. When configuring authentication, it's common to specify the default authentication scheme. The default scheme is used unless a resource requests a specific scheme</p>
* \code
        services.AddAuthentication(config =>
            {
                config.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                config.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                config.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
* \endcode
* <p><strong>Add Cookie Authentication Defaults</strong></p>
* \code
        services.AddAuthentication(config =>
        {
            config.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            config.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            config.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        })
        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
        {
            options.Cookie.SameSite = SameSiteMode.None;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.IsEssential = true;
            options.ExpireTimeSpan = TimeSpan.FromHours(10);
        };
* \endcode
* <p><strong>Refresh Token Request from Client.</strong></p>
* <p>Since the event is fired everytime the cookie is validated in the cookie middleware, so in every authenticated request, the decyrption of the cookie has alreay happened and so the access to user claims is present.</p>
* <p>If the refresh token threshold configured is less than the current time remaining form the access token expiration configuration setting, then a new refresh token request is given as shown in the code below</p>
* <p>This can be configured by the client, if the need for refreshing the token is desired.</p>
* \code
 options.Events = new CookieAuthenticationEvents
               {
                   // this event is fired everytime the cookie has been validated by the cookie middleware,
                   // so basically during every authenticated request
                   // the decryption of the cookie has already happened so we have access to the user claims
                   // and cookie properties - expiration, etc..
                   OnValidatePrincipal = async cookieCtx =>
                   {
                       // since our cookie lifetime is based on the access token one,
                       // check if we're more than halfway of the cookie lifetime
                       var now = DateTimeOffset.UtcNow;
                       //var timeElapsed = now.Subtract(x.Properties.IssuedUtc.Value);
                       //var timeRemaining = x.Properties.ExpiresUtc.Value.Subtract(now);

                       var expiresAt = cookieCtx.Properties.GetTokenValue("expires_at");
                       var accessTokenExpiration = DateTimeOffset.Parse(expiresAt);
                       var timeRemaining = accessTokenExpiration.Subtract(now);
                       var refreshThreshold = TimeSpan.FromSeconds(ApplicationConstants.RenewRefreshTokenBeforeSeconds);

                       if (timeRemaining < refreshThreshold)
                       {
                           var refreshToken = cookieCtx.Properties.GetTokenValue("refresh_token");

                           // if we have to refresh, grab the refresh token from the claims, and request
                           // new access token and refresh token
                           var request = EndpointExtension.CreateRefreshTokenRequest(
                               ApplicationConstants.ClientId,
                               ApplicationConstants.ClientSecret,
                               OpenIdConstants.GrantTypes.RefreshToken,
                               refreshToken);

                           var httpClient = new HttpClient();
                           var tokenResponse = await httpClient.PostAsync(ApplicationConstants.RefreshTokenEndpoint, new FormUrlEncodedContent(request));
                           var tokenResult = await tokenResponse.ParseTokenResponseResult();
                           if (tokenResult != null)
                           {
                               var expiresInSeconds = tokenResult.expires_in;
                               var updatedExpiresAt = DateTimeOffset.UtcNow.AddSeconds(expiresInSeconds);
                               cookieCtx.Properties.UpdateTokenValue("expires_at", updatedExpiresAt.ToString());
                               cookieCtx.Properties.UpdateTokenValue("access_token", tokenResult.access_token);
                               cookieCtx.Properties.UpdateTokenValue("refresh_token", tokenResult.refresh_token);

                               // Indicate to the cookie middleware that the cookie should be remade (since we have updated it)
                               cookieCtx.ShouldRenew = true;
                           }
                           else
                           {
                               cookieCtx.RejectPrincipal();
                               await cookieCtx.HttpContext.SignOutAsync();
                           }
                       }
                   }
* \endcode
* <p><strong>Add Open ID Connect</strong></p>
* <p>Adds OpenId Connect authentication to AuthenticationBuilder using the default scheme. The default scheme is specified by AuthenticationScheme. OpenID Connect is an identity layer on top of the OAuth 2.0 protocol. It allows clients to request and receive information about authenticated sessions and end-users.</p>
* \code
           .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
           {
               options.Authority = ApplicationConstants.AuthenticationServerBaseUrl;
               options.ClientId = ApplicationConstants.ClientId;
               options.ClientSecret = ApplicationConstants.ClientSecret;

               var authflow = Configuration.GetValue<string>("OpenIdFlow");
               if (authflow == "Hybrid")
               {
                   //Hybrid flow
                   options.ResponseType = OpenIdConnectResponseType.CodeIdToken;
               }
               else
               {
                   //Authorize code flow
                   options.ResponseType = OpenIdConnectResponseType.Code;
               }

               options.RequireHttpsMetadata = false;
               options.GetClaimsFromUserInfoEndpoint = true;
               options.SaveTokens = true;
               options.MetadataAddress = ApplicationConstants.MetadataAddress;
               options.UseTokenLifetime = true;
               options.UsePkce = true;
               options.ResponseMode = OpenIdConnectResponseMode.Query;
               options.AuthenticationMethod = OpenIdConnectRedirectBehavior.RedirectGet;
               options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
               IdentityModelEventSource.ShowPII = true;

               options.Scope.Add("openid");
               options.Scope.Add("email");
               options.Scope.Add("profile");
               options.Scope.Add("offline_access");
               options.Scope.Add("phone");
               options.Scope.Add("HCL.CS.SF.apiresource");
               options.Scope.Add("HCL.CS.SF.client");
               options.Scope.Add("HCL.CS.SF.user");
               options.Scope.Add("HCL.CS.SF.role");
               options.Scope.Add("HCL.CS.SF.identityresource");
               options.Scope.Add("HCL.CS.SF.adminuser");
               options.Scope.Add("HCL.CS.SF.securitytoken");

               options.Events = new OpenIdConnectEvents
               {
                   OnTokenResponseReceived = context =>
                   {
                       return Task.FromResult(0);
                   },
                   OnRemoteSignOut = context =>
                   {
                       return Task.FromResult(0);
                   },
                   OnSignedOutCallbackRedirect = context =>
                   {
                       return Task.FromResult(0);
                   },
                   OnMessageReceived = context =>
                   {
                       return Task.FromResult(0);
                   },
                   OnTokenValidated = context =>
                   {
                       return Task.FromResult(0);
                   },
                   OnUserInformationReceived = context =>
                   {
                       string rawAccessToken = context.ProtocolMessage.AccessToken;
                       var handler = new JwtSecurityTokenHandler();
                       var accessToken = handler.ReadJwtToken(rawAccessToken);
                       var claims = accessToken.Claims.Where(x => !AuthenticationConstants.AccessTokenFilters.ClaimsFilter.Contains(x.Type));
                       if (claims != null)
                       {
                           var newIdentity = new ClaimsIdentity(context.Principal.Identity, claims, "", OpenIdConstants.ClaimTypes.Name, OpenIdConstants.ClaimTypes.Role);
                           context.Principal = new ClaimsPrincipal(newIdentity);
                       }

                       return Task.FromResult(0);
                   },
                   OnAuthorizationCodeReceived = context =>
                   {
                       return Task.FromResult(0);
                   },
                   OnAuthenticationFailed = context =>
                   {
                       return Task.FromResult(0);
                   },
                   OnRedirectToIdentityProvider = context =>
                   {
                        return Task.FromResult(0);
                   },
                   OnRedirectToIdentityProviderForSignOut = context =>
                   {
                       return Task.CompletedTask;
                   }
               };
            });
* \endcode
*/



