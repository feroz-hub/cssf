/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

/**
* @page TokenFormat Token Format
* <p><strong> Token Types </strong></p>
* <p>The default token type ( <strong> token_type) </strong> that is supported in the HCL.CS.SF is the &ldquo; <strong> Bearer </strong> &rdquo; token type.</p>
* <p>The &ldquo; <strong> Reference </strong> &rdquo; token type is not supported in the HCL.CS.SF.</p>
* <p>A <strong> Bearer </strong> Token is an opaque string, not intended to have any meaning to clients using it. It can be a structured JWT token.&nbsp;</p>
* <p>The following are some of the &ldquo;Bearer&rdquo; tokens generated in the HCL.CS.SF.</p>
* <table>
* <tr>
* <td>
* <p><strong> S. No </strong></p>
* </td>
* <td>
* <p><strong> Token </strong></p>
* </td>
* <td>
* <p><strong> Description</strong></p>
* </td>
* </tr>
* <tr>
* <td>
* <p>1</p>
* </td>
* <td>
* <p>Access Token (access_token)</p>
* </td>
* <td>
* <p>The access token is used for authentication and authorization to get access to the resources from the resource server. It is a structured JWT token.</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>2</p>
* </td>
* <td>
* <p>Identity Token (id_token)</p>
* </td>
* <td>
* <p>Identity token is the token used as the proof of the user&rsquo;s authentication. It is a standard encoded JWT token. It is not encrypted, but is base64 encoded.</p>
* <p>The HCL.CS.SF provides the identity token when the scope input parameter value contains &ldquo;openid&rdquo;.</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>3</p>
* </td>
* <td>
* <p>Refresh token (refresh_token)</p>
* </td>
* <td>
* <p>The refresh token normally is sent together with the access token.</p>
* <p>The refresh token is used to get a new access token, when the old one expires, the client provides the refresh token, and receives a new access token.</p>
* <p>Using refresh tokens allows for having a short expiration time for access token to the resource server, and a long expiration time for access to the authorization server. This is a 32-bit random string.</p>
* <p>The HCL.CS.SF provides the refresh token when the scope input parameter value contains &ldquo;offline_access&rdquo;.</p>
* </td>
* </tr>
* </table>
* <p><strong> Token Format </strong></p>
* <p>The access token and the identity token supported by the HCL.CS.SF are in JWT format.&nbsp;</p>
* <p>JSON Web Token (JWT) is an open standard ( RFC 7519 ) that defines a compact and self-contained way for securely transmitting information between parties as a JSON object. This information can be verified and trusted because it is digitally signed. JWTs can be signed using a secret (with the&nbsp;HMAC&nbsp;algorithm) or a public/private key pair using&nbsp;RSA&nbsp;or&nbsp;ECDSA.&nbsp;</p>
* <p>JSON Web Tokens consist of three parts separated by dots (.), which are:</p>
* <table>
* <tr>
* <td>
* <p><strong> JWT part </strong></p>
* </td>
* <td>
* <p><strong> Description </strong></p>
* </td>
* </tr>
* <tr>
* <td>
* <p>Header</p>
* </td>
* <td>
* <p>The header&nbsp;typically&nbsp;consists of two parts: the type of the token, which is JWT, and the signing algorithm being used, such as <strong> HMAC </strong> <strong> SHA256 </strong> or <strong> RSA </strong></p>
* <p>
* \code
        {
            "alg": "HS256",
            "typ": "JWT"
        }
* \endcode
* </p>
* </td>
* </tr>
* <tr>
* <td>
* <p>Payload</p>
* </td>
* <td>
* <p>The second part of the token is the payload, which contains the claims. Claims are statements about an entity (typically, the user) and additional data. The payload is base64url encoded.</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>Signature</p>
* </td>
* <td>
* <p>The signature part is created by signing</p>
* <ul>
* <li>the encoded header and the</li>
* <li>the encoded payload&nbsp;</li>
* </ul>
* <p>The client application needs to trust that the claims encoded in a JWT have not been altered in any way. This is why a JWT includes a signature.&nbsp;</p>
* <p>If the signing algorithm is HMAC, then the <strong> client secret </strong> is used as the <strong> shared key </strong> for the encoding.</p>
* </td>
* </tr>
* </table>
* <p><strong> Supported Signing Algorithms </strong></p>
* <p>Different types of algorithms can be used for signing the JWT. Commonly they can be classified as</p>
* <ul>
* <li>HMAC</li>
* <li>RSA, PSA</li>
* <li>ECDSA&nbsp;</li>
* </ul>
* <ul>
* <li>The bit strength for the encryption supported by the HCL.CS.SF are 256, 384 and 512 for each type of algorithm.&nbsp;</li>
* <li><strong> HMAC </strong> uses <strong> the client id </strong> as the Shared Key for creating the signature.</li>
* <li>The default signing algorithm is HMAC-SHA256 if the client does not specify a signing algorithm while registering in the HCL.CS.SF.&nbsp;</li>
* <li>RSA and ECDSA algorithms use the private key obtained from the client to sign the JWT.&nbsp;</li>
* <li>The public keys of these alogrithms for the client are exposed using the JWKS endpoint.&nbsp;</li>
* </ul>
* <p>The following algorithms are supported by the HCL.CS.SF for signing the access and identity tokens.&nbsp;</p>
* <table>
* <tr>
* <td>
* <p><strong> S.No </strong></p>
* </td>
* <td>
* <p><strong> Algorithm </strong></p>
* </td>
* <td>
* <p><strong> Algorithm short name </strong></p>
* </td>
* </tr>
* <tr>
* <td>
* <p>1</p>
* </td>
* <td>
* <p>EcdsaSha256</p>
* </td>
* <td>
* <p>ES256</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>2</p>
* </td>
* <td>
* <p>EcdsaSha384</p>
* </td>
* <td>
* <p>ES384</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>3</p>
* </td>
* <td>
* <p>EcdsaSha512</p>
* </td>
* <td>
* <p>ES512</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>4</p>
* </td>
* <td>
* <p>HmacSha256</p>
* </td>
* <td>
* <p>S256</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>5</p>
* </td>
* <td>
* <p>HmacSha384</p>
* </td>
* <td>
* <p>S384</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>6</p>
* </td>
* <td>
* <p>HmacSha512</p>
* </td>
* <td>
* <p>S512</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>7</p>
* </td>
* <td>
* <p>RsaSha256</p>
* </td>
* <td>
* <p>RS256</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>8</p>
* </td>
* <td>
* <p>RsaSha384</p>
* </td>
* <td>
* <p>RS384</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>9</p>
* </td>
* <td>
* <p>RsaSha512</p>
* </td>
* <td>
* <p>RS512</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>10</p>
* </td>
* <td>
* <p>RsaSsaPssSha256</p>
* </td>
* <td>
* <p>PS256</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>11</p>
* </td>
* <td>
* <p>RsaSsaPssSha384</p>
* </td>
* <td>
* <p>PS384</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>12</p>
* </td>
* <td>
* <p>RsaSsaPssSha512</p>
* </td>
* <td>
* <p>PS512</p>
* </td>
* </tr>
* </table>
* <p><strong> Prompt Modes </strong></p>
* <p>The&nbsp;prompt&nbsp;parameter is used by the client to make sure that the logged in user is still present for the current session or to bring attention to the request. If this parameter contains&nbsp;none&nbsp;with any other value, an error is returned.</p>
* <p>The following prompt modes are supported in the authorization endpoint of the HCL.CS.SF&nbsp;</p>
* <table>
* <tr>
* <td>
* <p><strong> Prompt </strong></p>
* </td>
* <td>
* <p><strong> Description </strong></p>
* </td>
* </tr>
* <tr>
* <td>
* <p><strong> none </strong></p>
* </td>
* <td>
* <p>The Authorization Server shall not display any authentication or consent user interface pages. An error is returned if an application user is not already authenticated or the Client does not have pre-configured consent for the requested Claims or does not fulfill other conditions for processing the request.&nbsp;</p>
* <p>This can be used as a method to check for existing authentication and/or consent.&nbsp;</p>
* </td>
* </tr>
* <tr>
* <td>
* <p><strong> login </strong></p>
* </td>
* <td>
* <p>The Authorization Server shall prompt the user of the client application for reauthentication. If it cannot reauthenticate the logged in user, it should return an error as &ldquo;login _required.&rdquo;&nbsp;</p>
* <p><strong> Note </strong> : If explicit login is called, the login screen will be skipped&nbsp;</p>
* </td>
* </tr>
* <tr>
* <td>
* <p><strong> select_account </strong></p>
* </td>
* <td>
* <p>The Authorization Server shall prompt the application user to select a user account. This enables an application user who has multiple accounts at the Authorization Server to select amongst the multiple accounts that they might have current sessions for. If it cannot obtain an account selection choice made by the application user, it should return an error.</p>
* </td>
* </tr>
* </table>
* <p><strong> Note </strong> : From a native application, login has to be invoked explicitly, before the request to the authorization endpoint and the prompt has to be set to &ldquo;none&rdquo;. In a native application, re-authentication cannot be enforced.</p>
* <p><strong> Response Modes </strong></p>
* <p>The response mode is an input parameter to the authorization endpoint for returning authorization response parameters from the authorization Endpoint.</p>
* <table>
* <tr>
* <td>
* <p><strong> Response Mode </strong></p>
* </td>
* <td>
* <p><strong> Description </strong></p>
* </td>
* </tr>
* <tr>
* <td>
* <p><strong> query </strong></p>
* </td>
* <td>
* <p>In this mode, authorization response parameters are encoded in the query string added to the&nbsp; <strong> redirect_uri </strong> &nbsp;when redirecting back to the client.</p>
* </td>
* </tr>
* <tr>
* <td>
* <p><strong> fragment </strong></p>
* </td>
* <td>
* <p>In this mode, Authorization Response parameters are encoded in the fragment added to the <strong> redirect_uri </strong> when redirecting back to the Client.&nbsp;</p>
* </td>
* </tr>
* <tr>
* <td>
* <p><strong> form_post </strong></p>
* </td>
* <td>
* <p>In this mode, authorization response parameters are encoded as HTML form values that are auto-submitted in the User Agent, and thus are transmitted via the HTTP&nbsp;POST&nbsp;method to the Client, with the result parameters being encoded in the body using the&nbsp;&ldquo; <strong> application/x-www-form-urlencoded </strong> &rdquo;&nbsp;format.</p>
* </td>
* </tr>
* </table>
* <p><strong> Supported Grant Types </strong></p>
* <ul>
* <li>&ldquo;authorization_code&rdquo;: Authorization code grant type</li>
* <li>&ldquo;password&rdquo;: Resource owner password credentials grant type</li>
* <li>&ldquo;client_credentials&rdquo;: Client credentials grant type</li>
* <li>&ldquo;refresh_token&rdquo;: Refresh token grant type</li>
* </ul>
* <p><strong> Supported Response Types </strong></p>
* <ul>
* <li>code (authorization code)</li>
* <li>id_token (identity token)</li>
* <li>token (access token)</li>
* <li>id_token token</li>
* <li>code token</li>
* <li>code id_token</li>
* <li>code token id_token</li>
* </ul>
*/


