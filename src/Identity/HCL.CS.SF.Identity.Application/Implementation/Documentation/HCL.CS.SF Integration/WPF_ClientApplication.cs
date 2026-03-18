/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

/**
* @page WPF_ClientApplication WPF Client application
* <p> <strong> Prerequisites </strong> </p>
* <ul>
* <li>Login has to be done explicitly before calling endpoints</li>
* <li>Call the specific endpoints, based on previous login, the user will be authenticated and token is generated.</li>
* <li>Http Listener for getting the redirect-response from server based on the redirect url has to be configured in the following url format, <strong>HostName:PortNumber</strong> (loop-back address)</li>
* <li>Http client object should be used across the application.</li>
* <li>If refresh token is used, it needs to be automatically renewed in the background service based on defined time-interval.</li>
* <li>Access Token, Refresh token and Identity token will be maintained in Global variables and will be referenced for changes.</li>
* </ul>
* <p><strong> WPF client application configuration </strong></p>
* <p>The client application configures the </p>
* \code
    <appSettings>
        <add key="ClientId" value="ZqWJUx2H09BegYdhYCfNqyaRR/5WKdPW7PYQYC8jG3U="/>
        <add key="ClientSecret" value="4JjFMCgRmQ1aI1jGIkHF5BoX3klaVfDe1yOrl3ENY1Q="/>
        <add key="BaseUrl" value="https://localhost:5000"/>
        <add key="AuthorizeEndpoint" value="/security/authorize"/>
        <add key="TokenEndpoint" value="/security/token"/>
        <add key="UserInfoEndpoint" value="/security/userinfo"/>
        <add key="IntrospectionEndpoint" value="/security/introspect"/>
        <add key="AuthResponseType" value="code"/>
        <add key="AuthScope" value="openid email profile offline_access phone HCL.CS.SF.apiresource HCL.CS.SF.client HCL.CS.SF.user HCL.CS.SF.role HCL.CS.SF.identityresource HCL.CS.SF.adminuser HCL.CS.SF.securitytoken"/>
        <add key="AuthResponseMode" value="query"/>
        <add key="Prompt" value="none"/>
        <add key="RedirectUri" value="http://127.0.0.1:63562/"/>
        <add key="ClientScope" value="HCL.CS.SF.client"/>
        <add key="HybridScope" value="openid HCL.CS.SF.client email profile offline_access"/>
        <add key="HybridResponseMode" value="form_post"/>
        <add key="HybridResponseType" value="code id_token token"/>
        <add key="UserInfoScope" value="openid email profile offline_access phone"/>
        <add key="UserInfoResponseMode" value="form_post"/>
        <add key="UserInfoResponseType" value="code id_token token"/>
        <add key="JwtScope" value="openid email profile phone"/>
        <add key="JwtResponseMode" value="query"/>
        <add key="JwtResponseType" value="code"/>
        <add key="ROPScope" value="openid email profile"/>
        <add key="RefreshTokenScope" value="openid email profile phone HCL.CS.SF.client offline_access"/>
        <add key="RefreshTokenMode" value="fragment"/>
        <add key="RefreshTokenType" value="code id_token token"/>
    </appSettings>
* \endcode
* <p>The configured values are inserted into the application using the ApplicationParameters object by calling the SetInitialParameters() function as described below</p>
* \code
        private void SetInitialParameters()
        {
            try
            {
                ClientId = ConfigurationManager.AppSettings["ClientId"];
                ClientSecret = ConfigurationManager.AppSettings["ClientSecret"];
                BaseUrl = ConfigurationManager.AppSettings["BaseUrl"];
                AuthorizeEndpoint = BaseUrl + ConfigurationManager.AppSettings["AuthorizeEndpoint"];
                TokenEndpoint = BaseUrl + ConfigurationManager.AppSettings["TokenEndpoint"];
                UserInfoEndpoint = BaseUrl + ConfigurationManager.AppSettings["UserInfoEndpoint"];
                IntrospectionEndpoint = BaseUrl + ConfigurationManager.AppSettings["IntrospectionEndpoint"];
                AuthResponseType = ConfigurationManager.AppSettings["AuthResponseType"];
                AuthScope = ConfigurationManager.AppSettings["AuthScope"];
                AuthResponseMode = ConfigurationManager.AppSettings["AuthResponseMode"];
                Prompt = ConfigurationManager.AppSettings["Prompt"];
                RedirectUri = ConfigurationManager.AppSettings["RedirectUri"];
                ClientScope = ConfigurationManager.AppSettings["ClientScope"];
                HybridScope = ConfigurationManager.AppSettings["HybridScope"];
                HybridResponseMode = ConfigurationManager.AppSettings["HybridResponseMode"];
                HybridResponseType = ConfigurationManager.AppSettings["HybridResponseType"];
                UserInfoScope = ConfigurationManager.AppSettings["UserInfoScope"];
                UserInfoResponseMode = ConfigurationManager.AppSettings["UserInfoResponseMode"];
                UserInfoResponseType = ConfigurationManager.AppSettings["UserInfoResponseType"];
                JwtScope = ConfigurationManager.AppSettings["JwtScope"];
                JwtResponseMode = ConfigurationManager.AppSettings["JwtResponseMode"];
                JwtResponseType = ConfigurationManager.AppSettings["JwtResponseType"];
                ROPScope = ConfigurationManager.AppSettings["ROPScope"];
                RefreshTokenScope = ConfigurationManager.AppSettings["RefreshTokenScope"];
                RefreshTokenMode = ConfigurationManager.AppSettings["RefreshTokenMode"];
                RefreshTokenType = ConfigurationManager.AppSettings["RefreshTokenType"];
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error-While loading configuration.", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
* \endcode
* <p><strong>HTTP Client</strong></p>
* The HTTP client class returns a new httpclient which is extended with the custom methods for the http client that is incorporated into the WPF application.
* This class is used to call the endpoints with the appropriate input values using the endpoint extension methods.
* \code
    internal static class Http
    {
        private static HttpClient httpClient = new HttpClient();
        internal static HttpClient Client
        {
            get
            {
                return httpClient;
            }
        }
    }
* \endcode
* <p><strong>Extensions for the WPF application</strong></p>
* <ul>
* <li>Endpoint Extensions</li>
* <li>DataProtection Extensions</li>
* </ul>
* <p><strong>Using HTTPListener for calling the endpoints</strong></p>
* <p>First, the Redirect uri is taken from the application parameter constants and a new HTTP listener is created using the .NET Core APIs.</p>
* <p>The listener adds the <strong>redirectUri</strong> as a prefix and then the listener is started as shown below.</p>
* \code
        string redirectUri = ApplicationConstants.RedirectUri;
        Log("redirect URI: " + redirectUri);
        var http = new HttpListener();
        http.Prefixes.Add(redirectUri);
        Log("Listening..");
        http.Start();
* \endcode
* <p><strong>Creating the input parameters as query string for the authorize endpoint.</strong></p>
* <p>The below code creates the request with input parameters as querystring using the endpoint extension method CreateAuthorizeRequestUrl()</p>
* \code
        var authcodeRequest = EndpointExtension.CreateAuthorizeRequestUrl(
        clientId: ApplicationConstants.ClientId,
        responseType: ApplicationConstants.AuthResponseType,
        scope: ApplicationConstants.AuthScope,
        responseMode: ApplicationConstants.AuthResponseMode,
        prompt: ApplicationConstants.Prompt,
        codeChallenge: codeChallengeString,
        codeChallengeMethod: "S256",
        maxAge: "60",
        redirectUri: redirectUri,
        nonce: nonce,
        state:state);
* \endcode
* <p><strong>Calling the endpoint</strong></p>
* <p>The below code calls the authorize endpoint using the endpoint extension GetAsync() method.</p>
* \code
         var returnQuery = await Http.Client.GetAsync(authcodeRequest);
* \endcode
* <p><strong>Parsing the response from the authorize endpoint</strong></p>
* <p>The below code parses the authorize endpoint response using the ParseQueryString() extension method.</p>
* \code
        var authResponse = returnQuery.Headers.Location.ToString().ParseQueryString();
* \endcode
* <p><strong>Creating the input parameters as Form variables for the Token endpoint.</strong></p>
* <p>The below code calls the Token endpoint using the endpoint extension method CreateTokenRequest()</p>
* \code
        var tokenRequest = EndpointExtension.CreateTokenRequest(
        clientId: ApplicationConstants.ClientId,
        clientSecret: ApplicationConstants.ClientSecret,
        code: authResponse.Code,
        redirectUri: redirectUri,
        grantType: OpenIdConstants.GrantTypes.AuthorizationCode,
        codeVerifier: codeVerifier);
* \endcode
* <p><strong>Posting the request with input parameters to the token endpoint</strong></p>
* <p>The below code calls the Token endpoint using the endpoint extension method CreateTokenRequest()</p>
* \code
        var tokenResponse = await Http.Client.PostAsync(ApplicationConstants.TokenEndpoint, new FormUrlEncodedContent(tokenRequest));
* \endcode
* <p><strong>Parsing the response from the token endpoint</strong></p>
* <p>The below code parses the token endpoint response using the ParseTokenResponseResult() extension method.</p>
* \code
        tokenResponseResultModel = await tokenResponse.ParseTokenResponseResult();
* \endcode
<p><strong>Parsing the error response from the token endpoint</strong></p>
* <p>The below code parses the token endpoint error response using the ParseQueryString() extension method.</p>
* \code
      var errorResponse = tokenResponse.ParseTokenResponseResult();
* \endcode
* <p><strong>Stopping the HTTP Listener.</strong></p>
* <p>The Listener is stopped in after the response from the endpoints is received by the application.</p>
* \code
     http.Stop();
* \endcode
*/


