/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

/**
* @page IntegrationApproach HCL.CS.SF Integration
* <p><strong> HCL.CS.SF Hosting Prerequisites </strong></p>
* <ul>
* <li>.NET Core 3.1 or above has to be installed in the server.</li>
* <li><strong>RSA</strong> or <strong>ECDSA</strong> Certificates that contains public and private keys for JWT token signing in #PKCS12 format.</li>
* <li>Twilio key has to be configured for triggering SMS notification. This is optional. If no SMS notification needed, Emall can be used as the alternate notification method.</li>
* <li>LDAP configuration with domain, port, and certificates (for SSL connection), If HCL.CS.SF needs to create user account from existing LDAP server. This is optional. If not configured, the user account will be created with direct input from the user.</li>
* <li>Authenticator app, If authenticator app is used as 2FA, supported authenticator app to be installed and configured in the end-user mobile device. E.g. Microsoft authenticator, Google authenticator.</li>
* </ul>
* <p><strong> HCL.CS.SF Integration Approach </strong></p>
* <ul>
* <li>HCL.CS.SF can be integrated only to .NET core based server application, since all inputs are validated using HTTPContext values such as user authentication information, token etc.</li>
* <li>HCL.CS.SF endpoints can be invoked only via server hosted Restful services.</li>
* <li>Adminitrative and user self service APIs are only exposed as public methods, it can be consumed from controller layer and also it integrated with Restful service.</li>
* <p>The following steps needs to be followed for integrating and utilizing the HCL.CS.SF authorization/authentication features</p>
* <p><strong> Step 1 :&nbsp;</strong>Create a MVC Server application which will handle the requests at the server from the client application and design the controllers/views for various user related functionalities for Login, Logout, Forgot pasword, Two factor signin-options, using recovery codes for sign-in etc.</p>
* <p><strong> Step 2 :&nbsp;</strong>Create a resource server application which will validate the token using the introspection endpoint and will parse the token for the permissions of the logged in user.</p>
* <p><strong> Step 3 :&nbsp;</strong>Configure the client application based on the type of the application (MVC/WPF) for specifying the OpendID connect options and events for various endpoints used, set the Authentication with the Cookie scheme,&nbsp;and set the claims principal from the user info endpoint in the client to obtain the scopes and permissions for the user and also provide option to filter out the claims not needed to be set in the client.</p>
* <ul>
* <li>Configuring \subpage MVC_ServerApplication</li>
* <li>Configuring \subpage MVC_ResourceServerApplication</li>
* <li>Configuring \subpage MVC_ClientApplication</li>
* <li>Configuring \subpage WPF_ClientApplication</li>
* </ul>
*/


