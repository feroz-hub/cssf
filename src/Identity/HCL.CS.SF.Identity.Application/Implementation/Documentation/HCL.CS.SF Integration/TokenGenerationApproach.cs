/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

/**
* @page TokenGenerationApproach Token Generation Approach
* <p>The token is generated in the HCL.CS.SF based on the input parameters specified in the request of the authorize endpoint (hybrid flow) and the token endpoint (all other flows - authorize code, client_credentials, refresh_token and the resource owner password.)</p>
* <p><strong> Identity token </strong></p>
* <p>Identity token is generated only when the scope input parameter contains &ldquo;openid&rdquo; as one of the space separated strings. Identity token is generated in a signed JWT format.</p>
* <p>The format is described below, </p>
* <table>
* <tr>
* <td>
* <p><strong> JWT Part </strong></p>
* </td>
* <td>
* <p><strong> Description </strong></p>
* </td>
* </tr>
* <tr>
* <td>
* <p><strong> Header </strong></p>
* </td>
* <td><strong> Algorithm &ldquo;none&rdquo; : </strong> If the client application does not specify a algorithm, JWT token will be generated without signature.
* <p>The header looks like below,</p>
* <p>
* \code
        {
            typ : at+jwt,
            alg : none
        }
* \endcode
* </p>
* <p><strong> HMAC : </strong> If the client application specifies HMAC algorithm, then the client secret will be used a key for signing.</p>
* <p>The header looks like below</p>
* <p>
* \code
        {
            type : at+jwt,
            alg : <<HS256>>
        }
* \endcode
* </p>
* <p><strong> RSA, PSA, ECDSA: </strong> If the client application specifies the asymmetric algorithms, the JWT token will be signed using the public key from the certificate configured during initial setup.</p>
* <p>The header looks like below</p>
* <p>
* \code
        {
            type : at+jwt,
            alg : <<RS512>>,
            x5t: <<certificate hash of RS512>>
        }
* \endcode
* </p>
* </td>
* </tr>
* <tr>
* <td>
* <p><strong> Payload </strong></p>
* </td>
* <td>
* <p>The payload consists of all the claims related to the User.</p>
* <p>These claims includes both standard and custom claims.</p>
* <p><strong> Standard Claims : </strong></p>
* <table>
* <tr>
* <td style="text-align: center;">
* <p><strong> Claim Name </strong></p>
* </td>
* <td style="text-align: center;">
* <p><strong> Description&nbsp;</strong></p>
* </td>
* </tr>
* <tr>
* <td>
* <p>Subject (sub)</p>
* </td>
* <td>
* <p>The "sub" (subject) claim identifies the principal that is the subject of the JWT.</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>Issuer (iss)</p>
* </td>
* <td>
* <p>The "iss" (issuer) claim identifies the principal that issued the JWT.</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>IssuedAt (iat)</p>
* </td>
* <td>
* <p>The "iat" (issued at) claim identifies the time at which the JWT was issued.</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>Expiration (exp)</p>
* </td>
* <td>
* <p>The "exp" (expiration time) claim identifies the expiration time on or after which the JWT MUST NOT be accepted for processing.</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>JwtId (jti)</p>
* </td>
* <td>
* <p>The "jti" (JWT ID) claim provides a unique identifier for the JWT.</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>AuthTime (auth_time)</p>
* </td>
* <td>
* <p>The time of end user authentication, represented in Unix time (seconds)</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>Audience (aud)</p>
* </td>
* <td>
* <p> The "aud" (audience) claim identifies the recipients that the JWT is intended for.</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>ClientId (client_id)</p>
* </td>
* <td>
* <p>The unique identifier of the client application requesting the token.</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>Session Id (sid)</p>
* </td>
* <td>
* <p>The unique identifier of session of end user on a particular device/user agent.</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>AccessTokenHash (at_hash)</p>
* </td>
* <td>
* <p>At_hash value is the base64url encoding of the left-most half of the hash of the octets of the ASCII representation of the access_token</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>AuthorizationCodeToHash (c_hash)</p>
* </td>
* <td>
* <p>The authorization code hash value. It is the base64url encoding of the left-most half of the hash of the octets of the ASCII representation of the authorization code value.</p>
* </td>
* </tr>
* </table>
* <p><strong> Custom Claims : </strong></p>
* <p>The scope input is matched with identity resources configured in the HCL.CS.SF. The identity claims that matches the identity resource are taken and the claims are created with identity resource claim type and actual value as claim value from the user table. Refer to \ref CustomClaimsinIdentityToken "Custom Claims in Identity Token"</p>
* </td>
* </tr>
* <tr>
* <td>
* <p><strong> Signature </strong></p>
* </td>
* <td>
* <p>The signature of the JWT is based on the client&rsquo;s preferred algorithm. If no algorithm is specified, it takes the HMAC SHA256 algorithm as a default with the client secret as the shared key.</p>
* <p>&nbsp;Else, based on the asymmetric algorithm selected by the client, RSA, PSA or ECDSA, the appropriate <strong> private key of the algorithm loaded in the keystore </strong> is fetched and signed.</p>
* </td>
* </tr>
* </table>
* <p><strong> Access token </strong></p>
* <table>
* <tr>
* <td>
* <p><strong> JWT Part </strong></p>
* </td>
* <td>
* <p><strong> Description </strong></p>
* </td>
* </tr>
* <tr>
* <td>
* <p><strong> Header </strong></p>
* </td>
* <td>
* <p><strong> Algorithm &ldquo;none&rdquo; : </strong>If the client application does not specify a algorithm, JWT token will be generated without signature. </p>
* <p>The header looks like below,</p>
* <p>
* \code
        {
            typ : at+jwt,
            alg : none
        }
* \endcode
* <p><strong> HMAC : </strong>If the client application specifies HMAC algorithm, then the client secret will be used a key for signing.</p>
* <p>The header looks like below,</p>
* <p>
* \code
        {
            type : at+jwt,
            alg : <<HS256>>,
        }
* \endcode
* </p>
* <p><strong> RSA, PSA, ECDSA: </strong>  If the client application specifies the asymmetric algorithms, the JWT token will be signed using the public key from the certificate configurred during initial setup.</p>
* <p>The header looks like below,</p>
* <p>
* \code
        {
            type : at+jwt,
            alg : <<RSA512>>,
            x5t: <<certificate hash of RSA512>>
        }
* \endcode
* </p>
* </td>
* </tr>
* <tr>
* <td>
* <p><strong> Payload </strong></p>
* </td>
* <td>
* <p>The payload consists of all the claims related to the authorization of the user.</p>
* <p><strong> Standard Claims : </strong></p>
* <table>
* <tr>
* <td align="center">
* <p><strong> Claim Name </strong></p>
* </td>
* <td align="center">
* <p><strong> Description </strong></p>
* </td>
* </tr>
* <tr>
* <td>
* <p>Subject (sub)</p>
* </td>
* <td>
* <p>The "sub" (subject) claim identifies the principal that is the subject of the JWT. Unique identifier of the logged-in user. If the grant type is &ldquo;client credentials&rdquo;, then client Id of the requesting application.</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>Issuer (iss)</p>
* </td>
* <td>
* <p>The "iss" (issuer) claim identifies the principal that issued the JWT.</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>IssuedAt (iat)</p>
* </td>
* <td>
* <p>The "iat" (issued at) claim identifies the time at which the JWT was issued.</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>Expiration (exp)</p>
* </td>
* <td>
* <p>The "exp" (expiration time) claim identifies the expiration time on or after which the JWT MUST NOT be accepted for processing.</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>JwtId (jti)</p>
* </td>
* <td>
* <p>The "jti" (JWT ID) claim provides a unique identifier for the JWT.</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>AuthTime (auth_time)</p>
* </td>
* <td>
* <p>The time of end user authentication, represented in Unix time (seconds).</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>Audience (aud)</p>
* </td>
* <td>
* <p>The "aud" (audience) claim identifies the recipients that the JWT is intended for..</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>ClientId (client_id)</p>
* </td>
* <td>
* <p>The unique identifier of the client application requesting the token.</p>
* </td>
* </tr>
* <tr>
* <td>
* <p>Session Id (sid)</p>
* </td>
* <td>
* <p>The unique identifier of session of end user on a particular device/user agent.</p>
* </td>
* </tr>
* </table>
* <p><strong> Custom Claims :</strong></p>
* <p><span style="background-color: transparent;">The scope input parameter string when matches the ApiResource, its ApiResourceClaims will be searched for HCL.CS.SF defined claimed types and the matching claim values in the RoleClaims/UserClaims will be added.&nbsp; Refer to \ref CustomClaimsAccessToken "Custom Claims in access token"</span></p>
* </td>
* </tr>
* <tr>
* <td>
* <p><strong> Signature </strong></p>
* </td>
* <td>
* <p>The signature of the JWT is based on the client&rsquo;s preferred algorithm. If no algorithm is specified, it takes the HMAC SHA256 algorithm as default algorithm with the client secret as the shared key.</p>
* <p>&nbsp;Else, based on the asymmetric algorithm selected by the client, RSA, PSA or ECDSA, the appropriate <strong> private key of the algorithm loaded in the keystore </strong> is fetched and signed.</p>
* </td>
* </tr>
* </table>
* <p><strong> Refresh token </strong></p>
* <ul>
* <li>The refresh token is a opaque string which is not a JWT. It is a 32 bit random string.</li>
* <li>This token is generated when the scope input parameter of any of the grant types contains &ldquo;offline_access&rdquo;.&nbsp;</li>
* <li>The refresh token is first stored in the database as a 32 bit random string, so as that the revocation and the renew of the refresh token can be done by the HCL.CS.SF.</li>
* <li>When the refresh token is created, the payload of the current access token is stored as a serialized JSON string in the database. So that when the renewal request arrives through the refresh token flow, the access token is constructed by deserializing this string from the database using the old refresh token.</li>
* <li>During the refresh token renewal process, the old refresh token is hard deleted from the database and the new 32 bit random string is created with the existing payload of the access token.</li>
* <li>The refresh token stores the payload of the access token.</li>
* </ul>
* <p>The following are the characteristics of the refresh token.</p>
* <ul>
* <li>The lifetime of a refresh token is much longer compared to the lifetime of an access token.</li>
* <li>Refresh tokens can also expire but are quiet long-lived.</li>
* <li>When current access tokens expire or become invalid, the HCL.CS.SF provides refresh tokens to the client to obtain new access token through the refresh_token flow.</li>
* <li>The normal refresh token lifetime is set as 1 day by default in the HCL.CS.SF. (86400 seconds) whereas the access token lifetime is set as 1 hour. (3600 seconds)</li>
* <li>During the refresh token renewal process, the old refresh token is hard deleted from the database and the new 32 bit random string is created with the existing payload of the access token.</li>
* <li>During refresh token creation, the access token&rsquo;s payload is stored, so whenever there is a renewal only the expiration time is extended with the existing claims instead of recreating it everytime.</li>
* </ul>
*/


