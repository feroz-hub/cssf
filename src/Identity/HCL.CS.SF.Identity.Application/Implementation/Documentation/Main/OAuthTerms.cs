/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

/**
* @page OAuthTerms OAuth2.0 Terms
* <p><strong> Introduction : </strong></p>
* <p>&nbsp;The OAuth 2.0 authorization framework enables a third-party application to obtain limited access to an HTTP service, either on behalf of a resource owner by orchestrating an approval interaction between the resource owner and the HTTP service, or by allowing the third-party application to obtain access on its own behalf.&nbsp;</p>
* <p><strong> Traditional model problems : </strong></p>
* <p>In the traditional client-server authentication model, the client requests an access-restricted resource (protected resource) on the server by authenticating with the server using the resource owner's credentials. In order to provide third-party applications access to restricted resources, the resource owner shares its credentials with the third party. This creates several problems and limitations:</p>
* <ul>
* <li>Third-party applications are required to store the resource owner's credentials for future use, typically a password in clear-text.&nbsp;</li>
* <li>Servers are required to support password authentication, despite the security weaknesses inherent in passwords.&nbsp;</li>
* <li>Third-party applications gain overly broad access to the resource owner's protected resources, leaving resource owners without any ability to restrict duration or access to a limited subset of resources.&nbsp;</li>
* <li>Resource owners cannot revoke access to an individual third party without revoking access to all third parties, and must do so by changing the third party's password.&nbsp;</li>
* <li>Compromise of any third-party application results in compromise of the end-user's password and all of the data protected by that password.&nbsp;</li>
* </ul>
* <p>OAuth addresses these issues by introducing an authorization layer and separating the role of the client from that of the resource owner. In OAuth, the client requests access to resources controlled by the resource owner and hosted by the resource server, and is issued a different set of credentials than those of the resource owner.&nbsp;</p>
* <p>Instead of using the resource owner's credentials to access protected resources, the client obtains an access token -- a string denoting a specific scope, lifetime, and other access attributes. Access tokens are issued to third-party clients by an authorization server with the approval of the resource owner. The client uses the access token to access the protected resources hosted by the resource server&nbsp;</p>
* \image html OAuthProtocolflow.jpg
* <p><center><strong> OAuth Protocol Flow </strong></center></p>
* <p><strong> OAuth defines the following four roles: </strong></p>
* <table>
* <tr>
* <td>
* <p><strong> Role Name </strong></p>
* </td>
* <td>
* <p><strong> Description </strong></p>
* </td>
* </tr>
* <tr>
* <td>
* <p><strong> Resource Owner </strong></p>
* </td>
* <td>An entity capable of granting access to a protected resource. When the resource owner is a person, it is referred to as an end-user.</td>
* </tr>
* <tr>
* <td>
* <p><strong> Resource server </strong></p>
* </td>
* <td>The server hosting the protected resources, capable of accepting and responding to protected resource requests using access tokens.</td>
* </tr>
* <tr>
* <td>
* <p><strong> Client </strong></p>
* </td>
* <td>An application making protected resource requests on behalf of the resource owner and with its authorization. The term "client" does not imply any particular implementation characteristics (e.g., whether the application executes on a server, a desktop, or other devices).</td>
* </tr>
* <tr>
* <td>
* <p><strong> Authorization Server </strong></p>
* </td>
* <td>The server issuing access tokens to the client after successfully authenticating the resource owner and obtaining authorization</td>
* </tr>
* </table>
* <p>. <strong> Note : </strong></p>
* <ul>
* <li>The authorization server may be the same server as the resource server or a separate entity.</li>
* <li>A single authorization server may issue access tokens accepted by multiple resource servers.</li>
* </ul>
* <p><strong> The abstract OAuth 2.0 flow illustrated in the above figure describes the interaction between the four roles and includes the following steps: </strong></p>
* <ol>
* <li>The client requests authorization from the resource owner. The authorization request can be made directly to the resource owner (as shown in the above figure), or preferably indirectly via the authorization server as an intermediary.</li>
* <li>&nbsp;The client receives an authorization grant, which is a credential representing the resource owner's authorization, expressed using one of four grant types defined in this specification or using an extension grant type. The authorization grant type depends on the method used by the client to request authorization and the types supported by the authorization server.&nbsp;</li>
* <li>The client requests an access token by authenticating with the authorization server and presenting the authorization grant.&nbsp;</li>
* <li>The authorization server authenticates the client and validates the authorization grant, and if valid, issues an access token.&nbsp;</li>
* <li>The client requests the protected resource from the resource server and authenticates by presenting the access token.&nbsp;</li>
* <li>The resource server validates the access token, and if valid, serves the request.</li>
* </ol>
* <p><strong> Configuring HCL.CS.SF for a Web application client : </strong></p>
* <p>The following process is done for a web app client to configure the HCL.CS.SF for its OAuth 2.0 features to be utilized.</p>
* <ul>
* <li>The application opens a browser to send the user to the Authentication server. For this, a server application needs to be developed and deployed in the server. See configuring <u> Authentication Server application </u> .</li>
* <li>The user is redirected back to the application with an authorization code in the query string (redirect url). This is necessary as some flows supported by the HCL.CS.SF needs the client application to get the code and process the same by posting it to the endpoints as necessary. For e.g. in the authorization code flow, the authorization code is given to the application back via a redirect url and then this code is again send back to the token endpoint to get the access/identity token.</li>
* <li>The application exchanges the authorization code (posting it to the token endpoint) for an access token (access_token).</li>
* </ul>
* <p><strong> Get the User&rsquo;s Permission </strong></p>
* <p>HCL.CS.SF enables users to grant limited access to applications. The application first needs to decide which permissions it is requesting, then send the user to a browser to get their permission. To begin the authorization flow, the application constructs a URL like the following and opens a browser to that URL.</p>
* \code
https://localhost:5002/security/auth?response_type=code
&client_id=29352915982374239857
&redirect_uri=https%3A%2F%2Fexample-app.com%2Fcallback
&scope=create+delete
&state=xcoiv98y2kd22vusuye3kch
* \endcode
* <p><strong> Redirect Back to the Application </strong></p>
* <p>The HCL.CS.SF (authorization server) will redirect the browser back to the <strong> redirect_uri </strong> specified by the application, adding a&nbsp;code&nbsp;and&nbsp;state&nbsp;to the query string.</p>
* <p>For example, the user will be redirected back to a URL such as</p>
* \code
https://localhost:5002/redirect?code=g0ZGZmNjVmOWIjNTk2NTk4ZTYyZGI3&state=xcoiv98y2kd22vusuye3kch
* \endcode
* <p>The <strong> state </strong> value will be the same value that the application initially set in the request. The application is expected to check that the state in the redirect matches the state it originally set. This protects against <strong> CSRF </strong> and other related attacks.</p>
* <p>The <strong> code </strong> is the authorization code generated by the authorization server. This code is relatively short-lived, typically lasting between 1 to 10 minutes depending on the configuration doen in the HCL.CS.SF.</p>
* <p><strong> Exchange the Authorization Code for an Access Token </strong></p>
* <p>We&rsquo;re about ready to wrap up the flow. Now that the application has the authorization code, it can use that to get an access token.</p>
* <p>The application makes a POST request to the service&rsquo;s token endpoint with the following parameters:</p>
* <ul>
* <li><strong> grant_type=authorization_code </strong> - This tells the token endpoint that the application is using the Authorization Code grant type.</li>
* <li><strong> code </strong> - The application includes the authorization code it was given in the redirect.</li>
* <li><strong> redirect_uri </strong> - The same redirect URI that was used when requesting the code.</li>
* <li><strong> client_id </strong> - The application&rsquo;s client ID.</li>
* <li><strong> client_secret </strong> - The application&rsquo;s client secret. This ensures that the request to get the access token is made only from the application, and not from a potential attacker that may have intercepted the authorization code.</li>
* </ul>
* <p>The token endpoint will verify all the parameters in the request, ensuring the code hasn&rsquo;t expired and that the client ID and secret match. If everything checks out, it will generate an access token and return it in the response.</p>
* <p>The Authorization Code flow is complete. The application now has an access token it can use when making API requests.</p>
* <p><strong> Front Channel Logout</strong></p>
* <ul>
* <li>Front-Channel Logout is handled through the browser of the server application.</li>
* <li>For each registered client in the server that has a session for the logged-in user from the HCL.CS.SF, an iframe is rendered to the browser. This means that logout requests of all clients of the same logged-in user are performed in parallel.</li>
* </ul>
* <p>The HCL.CS.SF adds the below query parameters when rendering the logout URI from the front channel.</p>
* <table>
* <tr>
* <td>
* <p><strong> Query Parameter Name</strong></p>
* </td>
* <td>
* <p><strong> Description</strong></p>
* </td>
* </tr>
* <tr>
* <td>
* <p>iss</p>
* </td>
* <td>
* <p>Issuer Identifier URL for the authentication provider (HCL.CS.SF) issuing the front-channel logout request.</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>sid</p>
* </td>
* <td>
* <p>Identifier for the Session</p>
* </td>
* </tr>
* </table>
* <p>&nbsp;</p>
* <p><strong> Back channel Logout</strong></p>
* <ul>
* <li>Back-Channel Logout specifies a server-to-server communication for the logout request.Therefore, there is no dependency on the browser from the client As a result, users will get logged out from the client even in case the user agent(browser) was closed.</li>
* <li>HCL.CS.SF supports both front channel and back-channel logouts. This is specified in the discovery end point.</li>
* </ul>
* <p><strong> Logout Process </strong></p>
* <p>The following are the steps that happens when the user initiates logout from the client :</p>
* <ul>
* <li>When the user initiates the logout from the client application, the HCL.CS.SF builds the frontend logout URLs by getting each client logged-in by the user from the session and it&rsquo; s registered frontchannellogouturi and loads it in the host application&rsquo;s logout Uri through the iframe, thus initiating the logout in the respective client application&rsquo;s logout page.</li>
* <li>Similarly, for the backchannel logout, each Uri is constructed with a logout token and a post is initiated from the server application to the client application.</li>
* <li>The client application logout page shall have the logic to verify the logout token and initiating the logout.</li>
* </ul>
* <p><strong> General Logout Procedure</strong></p>
* <ul>
* <li>The client cleans up any security context for the user after logout initiation.</li>
* <li>The browser session is updated to reflect the logout (e.g.cookies are deleted). The user gets redirected to the&nbsp;end_session_endpoint.</li>
* <li>After logout, the HCL.CS.SF triggers a logout at other clients using the front- or back-channel logout mechanism or a combination of both based on the configuration.</li>
* <li>After successful logout, the user will return to the client using the <strong> post_ logout_redirect_uri</strong> if specified.</li>
* </ul>
* <p><strong> Logout Token </strong></p>
* <p>Logout tokens are JWT Tokens which are a back-channel mechanism for notifying subscribed client applications that an end-user has been logged out of the HCL.CS.SF(authentication provider).</p>
* <ul>
* <li>The logout can be explicit, or result from the expiration of the end-user session with the identity provider(HCL.CS.SF)</li>
* <li>An application receiving a logout token at the notification URI must validate its type, signature and claims to ensure the token originates from the authentication provider(HCL.CS.SF) and is not a&nbsp; forgery.</li>
* <li>The token is secured with the same signing algorithms as those of the ID tokens.</li>
* </ul>
* <p><strong>Authorization using OAuth 2.0 can fail in several ways:</strong></p>
* <ol>
* <li>The user aborts the authorization process by closing the browser view or navigating to a different URL.</li>
* <li>The authorization request has been denied by the server or has failed or is temporarily unavailable</li>
* <li>Network failures or internal server errors in HCL.CS.SF, or HCL.CS.SF is temporarily unavailable</li>
* <li>The application is incorrectly registered or implemented</li>
* </ol>
* <p>Handling scenario (1) is trivial for web application clients, since this means that the user has navigated away from the client.</p>
* <p>For native applications that open a browser window, this means that the application may have to detect that the window was closed.</p>
* <p>In the worst case, the only solution is to implement a timeout or to simply do nothing if no answer is received on the redirection endpoint. For native applications that embed a browser view, the user may not even have the possibility to close the view. However, the application should provide a means to cancel authorization, e.g., by means of a "cancel" button.</p>
* <p>In scenario (3), error messages will be presented to the user directly in the browser view. The user may have the option to retry authorization. If the error persists, the user will eventually give up and cancel authorization, which is then identical to scenario (1).</p>
* <p>In scenario (2), the HCL.CS.SF will send an error response in one of the following ways:</p>
* <ul>
* <li>If authorization is denied in the authorization request, then server redirects to the client's redirection endpoint with an&nbsp;<em>authorization error response</em>&nbsp;in the URL.</li>
* <li>If authorization is denied in the token request, then the token request returns an error status code (400-599) and a&nbsp;<em>token error response</em>&nbsp;in the response body.</li>
* </ul>
* <p><strong>Authorization Error Response</strong></p>
* <p>The authorization request redirects to the client's redirection endpoint URI as follows:</p>
* <table>
* <tr>
* <td>error_code</td>
* <td>Error_code indicating something went wrong.</td>
* </tr>
* <tr>
* <td>error_description</td>
* <td>
* <p>Human-readable description of the error to assist the client developer in understanding the error.</p>
* <p>This description MAY be shown to the user, but is not necessarily useful or understandable for the user and may not be localized to the user's language.</p>
* <p>According to the OAuth 2.0 specification, the error description is encoded as ASCII.</p>
* </td>
* </tr>
* </table>
*/


