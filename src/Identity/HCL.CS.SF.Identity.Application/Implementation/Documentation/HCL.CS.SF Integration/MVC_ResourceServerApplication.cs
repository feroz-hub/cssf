/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

/**
* @page MVC_ResourceServerApplication MVC Resource server application
* <p><strong>Resource Server Api Middleware invoke</strong></p>
* <p>In the Resource Server application, the resources that are configured for the client application other than the default HCL.CS.SF features are called via the configured URL.&nbsp; So, the following code will be added to the resource server application in the Startup.Configure() method inthe resource server applicatiion.</p>
* \code
  public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseApiMiddleware();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
* \endcode
* <p><strong>ApiMiddleware Extension.</strong></p>
* \code
    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class ApiMiddlewareExtensions
    {
        public static IApplicationBuilder UseApiMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ApiMiddleware>();
        }
    }
* \endcode
* <p>The app.UseMiddlware() extension method invokes the following method for validating the access token and the permissions in the access token with respect to the API Resources and API Scopes.</p>
* \code
public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                if (AppConstants.SecureApi.Contains(httpContext.Request.Path.Value))
                {
                    var authorization = httpContext.Request.Headers[HeaderNames.Authorization];
                    if (AuthenticationHeaderValue.TryParse(authorization, out var headerValue))
                    {
                        if (!string.IsNullOrWhiteSpace(headerValue.Parameter))
                        {
                            string accessToken = headerValue.Parameter;
                            var introspectionRequest = EndpointExtension.CreateIntroSpecRequest(
                                token: accessToken,
                                tokenTypeHint: "access_token",
                                clientId: ApplicationConstants.ClientId,
                                clientSecret: ApplicationConstants.ClientSecret);
                            var httpClient = new HttpClient();
                            var introspecResponse = await httpClient.PostAsync(ApplicationConstants.IntrospectionEndpoint, new FormUrlEncodedContent(introspectionRequest));
                            var introspectokenResponseresult = await introspecResponse.ParseIntrospectionResponse();
                            if (!introspectokenResponseresult.Active)
                            {
                                httpContext.Response.StatusCode = OpenIdConstants.HTTPStatusCodes.bad_request;
                                return;
                            }

                            var permission = AppConstants.AppPermission.Where(x => x.Key == httpContext.Request.Path.Value).ToList();
                            if (permission != null && permission.Count > 0)
                            {
                                var claims = new JwtSecurityToken(accessToken).Claims;
                                string[] permissionValues = null;
                                List<Claim> existingClaim = null;
                                if (permission[0].Value.Contains(","))
                                {
                                    permissionValues = permission[0].Value.Split(',');
                                    existingClaim = claims.ToList().Where(x => permissionValues.Contains(x.Value)).ToList();
                                }
                                else
                                {
                                    existingClaim = claims.ToList().Where(x => x.Value == permission[0].Value).ToList();
                                }

                                if(existingClaim == null || existingClaim.Count <= 0)
                                {
                                    httpContext.Response.StatusCode = OpenIdConstants.HTTPStatusCodes.access_denied;
                                    return;
                                }

                            }
                        }
                    }
                    else
                    {
                        httpContext.Response.StatusCode = OpenIdConstants.HTTPStatusCodes.invalid_token;
                        return;
                    }
                }
            }
            catch(Exception ex)
            {
                httpContext.Response.StatusCode = OpenIdConstants.HTTPStatusCodes.bad_request;
                await httpContext.Response.WriteResponseJsonAsync(ex.Message);
            }

            await _next(httpContext);
        }
* \endcode
*/



