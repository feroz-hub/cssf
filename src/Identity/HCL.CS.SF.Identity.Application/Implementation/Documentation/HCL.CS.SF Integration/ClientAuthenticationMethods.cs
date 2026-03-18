/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

/**
* @page ClientAuthenticationMethods Client Authentication Methods
* <p><strong> Client Authentication Methods </strong></p>
* <p>HCL.CS.SF supports one client authentication method for confidential clients. Authentication is performed once the endpoint is invoked from the client application.</p>
* <p>When a client is registered, the client id and client secret is given to the client application. When the client requests a token, it embeds the client id and client secret in the request and the HCL.CS.SF will check for its validity in the back-end.</p>
* <p>The default authentication method for a client is Basic ( <strong> client_secret_basic </strong> ).</p>
* <table>
* <tr>
* <td>
* <p><strong> S. No </strong></p>
* </td>
* <td>
* <p><strong> Authentication Method </strong></p>
* </td>
* <td>
* <p><strong> Description </strong></p>
* </td>
* </tr>
* <tr>
* <td>
* <p>1</p>
* </td>
* <td>
* <p>Basic (client_secret_basic)</p>
* </td>
* <td>
* <p>The client id and client secret is first concatenated using a &ldquo;:&rdquo; (colon). Then it is encoded using Base64 encoding and then this string is embedded in the Authorization header in the token request.</p>
* </td>
* </tr>
* </table>
* <p><strong>The following extension method sets the client id and client secret in the authentication header.</strong></p>
* \code
        public static void SetBasicAuthentication(this HttpClient client, string clientId, string clientSecret)
        {
            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentNullException(nameof(clientId));
            }

            if (clientSecret == null)
            {
                clientSecret = string.Empty;
            }

            Encoding encoding = Encoding.UTF8;
            var credential = $"{HttpUtility.UrlEncode(clientId)}:{HttpUtility.UrlEncode(clientSecret)}";
            var scheme = "Basic";

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme, Convert.ToBase64String(encoding.GetBytes(credential)));
        }
* \endcode
* <p><strong> Not supported Client Authentication Methods </strong> :</p>
* <ul>
* <li>Mutual TLS (Planned in next phase)</li>
* <li>PrivateKeyJWT (Planned in next phase)</li>
* </ul>
* <p>The client without any authentication method (none) is not supported in the HCL.CS.SF due to supporting only confidential clients</p>
* <p><strong> Client Types </strong></p>
* <p>Confidential clients are applications that are able to securely authenticate with the HCL.CS.SF so as to keep their registered client secret safe.</p>
* <p>Public clients are unable to use registered client secrets, such as applications running in a browser or on a mobile device.</p>
* <p>The HCL.CS.SF considers all clients that are registered as <strong> confidential </strong> clients. So client secret is mandatory for all endpoint requests except authorize code flow during the login process.</p>
* <p><strong> Public </strong> clients are not supported by the HCL.CS.SF.</p>
*/


