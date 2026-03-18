/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

/**
* @page CustomClaims Custom Claims
* \anchor CustomClaimsinIdentityToken
* <p><strong> Custom claims in Identity token: </strong></p>
* <ul>
* <li>Each scope that is applicable for the generation of the identity token is configured in the database as IdentityResources and corresponding claim values to each identity resource are configured in the Identity Claim table.</li>
* <li>When the scope input parameter encounters an IdentityResource, all the claim types configured for IdentityResource in the Claims are given.</li>
* <li>&nbsp;For the generation of identity token, the &ldquo;openid&rdquo; scope is a must.</li>
* <li>&nbsp;If the &ldquo;profile&rdquo; scope is given in the input, then, the respective identity claims will be loaded by matching the type with the fields in the User table and the userclaims table and the corresponding custom claims with ClaimType as the IdentityClaimType and the ClaimValue as the actual value in the user table or the user claims table will be constructed.</li>
* <li>These claims will be added as custom claims in the Identity token generation.</li>
* </ul>
* <p>The following table shows the Identity resources and the identity claims configured in the HCL.CS.SF.</p>
* <table>
* <tr>
* <td style="text-align: center;">
* <p><strong> Identity Resource </strong></p>
* </td>
* <td style="text-align: center;" colspan="3">
* <p><strong> IdentityClaim </strong></p>
* </td>
* </tr>
* <tr>
* <td>&nbsp;</td>
* <td style="text-align: center;">
* <p><strong> Type </strong></p>
* </td>
* <td style="text-align: center;">
* <p><strong> AliasType </strong></p>
* </td>
* <td style="text-align: center;"><strong> Description </strong></td>
* </tr>
* <tr>
* <td>
* <p><strong> openid </strong></p>
* </td>
* <td>
* <p>subject</p>
* </td>
* <td>
* <p>sub</p>
* </td>
* <td>
* <p>The user id of the user</p>
* </td>
* </tr>
* <tr>
* <td>
* <p><strong> profile </strong></p>
* </td>
* <td>
* <p>name</p>
* </td>
* <td>
* <p>username</p>
* </td>
* <td>
* <p>The name of the User</p>
* </td>
* </tr>
* <tr>
* <td>&nbsp;</td>
* <td>
* <p>family_name</p>
* </td>
* <td>
* <p>lastname</p>
* </td>
* <td>
* <p>The last name of the user</p>
* </td>
* </tr>
* <tr>
* <td>&nbsp;</td>
* <td>
* <p>given_name</p>
* </td>
* <td>
* <p>firstname</p>
* </td>
* <td>
* <p>The first name of the user</p>
* </td>
* </tr>
* <tr>
* <td>&nbsp;</td>
* <td>
* <p>middle_name</p>
* </td>
* <td>
* <p>&nbsp;-</p>
* </td>
* <td>&nbsp;</td>
* </tr>
* <tr>
* <td>&nbsp;</td>
* <td>
* <p>nickname</p>
* </td>
* <td>
* <p>&nbsp;-</p>
* </td>
* <td>&nbsp;</td>
* </tr>
* <tr>
* <td>&nbsp;</td>
* <td>
* <p>preferred_username</p>
* </td>
* <td>
* <p>&nbsp;-</p>
* </td>
* <td>&nbsp;</td>
* </tr>
* <tr>
* <td>&nbsp;</td>
* <td>
* <p>profile</p>
* </td>
* <td>
* <p>&nbsp;-</p>
* </td>
* <td>&nbsp;</td>
* </tr>
* <tr>
* <td>&nbsp;</td>
* <td>
* <p>picture</p>
* </td>
* <td>
* <p>&nbsp;-</p>
* </td>
* <td>&nbsp;</td>
* </tr>
* <tr>
* <td>&nbsp;</td>
* <td>
* <p>website</p>
* </td>
* <td>
* <p>&nbsp;-</p>
* </td>
* <td>&nbsp;</td>
* </tr>
* <tr>
* <td>&nbsp;</td>
* <td>
* <p>Gender</p>
* </td>
* <td>
* <p>&nbsp;-</p>
* </td>
* <td>&nbsp;</td>
* </tr>
* <tr>
* <td>&nbsp;</td>
* <td>
* <p>birthdate</p>
* </td>
* <td>
* <p>dateofbirth</p>
* </td>
* <td>
* <p>The date of birth of the user.</p>
* </td>
* </tr>
* <tr>
* <td>&nbsp;</td>
* <td>
* <p>zoneinfo</p>
* </td>
* <td>
* <p>&nbsp;-</p>
* </td>
* <td>&nbsp;</td>
* </tr>
* <tr>
* <td>&nbsp;</td>
* <td>
* <p>locale</p>
* </td>
* <td>
* <p>&nbsp;-</p>
* </td>
* <td>&nbsp;</td>
* </tr>
* <tr>
* <td>&nbsp;</td>
* <td>
* <p>updated_at</p>
* </td>
* <td>
* <p>&nbsp;-</p>
* </td>
* <td>&nbsp;</td>
* </tr>
* <tr>
* <td>&nbsp;</td>
* <td>
* <p>City</p>
* </td>
* <td>
* <p>&nbsp;-</p>
* </td>
* <td>&nbsp;</td>
* </tr>
* <tr>
* <td>&nbsp;</td>
* <td>
* <p>PinCode</p>
* </td>
* <td>
* <p>&nbsp;-</p>
* </td>
* <td>&nbsp;</td>
* </tr>
* <tr>
* <td>&nbsp;</td>
* <td>
* <p>Street</p>
* </td>
* <td>
* <p>&nbsp;-</p>
* </td>
* <td>&nbsp;</td>
* </tr>
* <tr>
* <td>
* <p><strong> email </strong></p>
* </td>
* <td>
* <p>email</p>
* </td>
* <td>
* <p>&nbsp;-</p>
* </td>
* <td>
* <p>Email of the user</p>
* </td>
* </tr>
* <tr>
* <td>&nbsp;</td>
* <td>
* <p>email_verified</p>
* </td>
* <td>
* <p>emailconfirmed</p>
* </td>
* <td>
* <p>The verified email of the user.</p>
* </td>
* </tr>
* <tr>
* <td> <p><strong> phone </strong></p></td>
* <td>
* <p>phone_number</p>
* </td>
* <td>
* <p>phonenumber</p>
* </td>
* <td>
* <p>The phone number given by the user during user registration.</p>
* </td>
* </tr>
* <tr>
* <td>&nbsp;</td>
* <td>
* <p>phone_number_verified</p>
* </td>
* <td>
* <p>phonenumberverified</p>
* </td>
* <td>
* <p>The phone number that has been verified.</p>
* </td>
* </tr>
* <tr>
* <td>
* <p><strong> address </strong></p>
* </td>
* <td>
* <p>address</p>
* </td>
* <td>
* <p>&nbsp;-</p>
* </td>
* <td>The address of the user.</td>
* </tr>
* </table>
* <p>&nbsp;</p>
* \anchor CustomClaimsAccessToken
* <p><strong>Custom Claims in Access Token</strong></p>
* <p>There are four tables available in the HCL.CS.SF to configure the access to the apis available. These tables can be used by the client application to provide authorization to their own api&rsquo;s also.</p>
* <p>&nbsp;The tables are as below:</p>
* <ul>
* <li><strong>ApiResources</strong></li>
* <li><strong>ApiResourceClaims</strong></li>
* <li><strong>ApiScopes</strong></li>
* <li><strong>ApiScopeClaims</strong></li>
* </ul>
* <p>These are used to create the custom claims ( permissions) available for the APIs in the token.</p>
* <p>When the scope parameter is given as input in any of the grant type flows to the endpoints, then first the allowed scopes for the client is checked. If there is mismatch between the allowed scope of the client and the requested scopes, invalid_scope error is thrown.</p>
* <p>If the requested scope matches, the Identity resource claims are constructed for the identity token as specified in the custom claims in identity token.</p>
* <p>Similarly for the access token, the ApiResource and ApiResource claims are evaluated to find the permissions available for the user.</p>
* <p>The ApiScopes table consists of all the permissions available for the API with the following convention.</p>
* <p>&lt;&lt;<strong>Api</strong>&gt;&gt;.&lt;&lt;<strong>Permission</strong>&gt;&gt; For. Eg. if there is an api named client, then the permission would be &ldquo;client.read&rdquo;</p>
* <p>The following are the permissions defined in the HCL.CS.SF</p>
* <ul>
* <li>Read</li>
* <li>Write</li>
* <li>Delete</li>
* <li>Manage</li>
* </ul>
* <p>The <strong> default </strong> permission for an api is &ldquo;Read&rdquo;.</p>
* <p>So, if the scope input parameter contains a value &ldquo;client&rdquo; it implicitly means &ldquo;client.read&rdquo;</p>
* <p>The permission &ldquo;Manage&rdquo; means all the permissions &ldquo;Read&rdquo;, &ldquo;Write&rdquo; and &ldquo;Delete&rdquo;</p>
* <p>If the scope input parameter contains a value &ldquo;client.manage&rdquo;, it means, &ldquo;client.read&rdquo; plus &ldquo;client.write&rdquo; plus &ldquo;client.delete&rdquo;.</p>
* <p>The Api is defined as the Api Resource and the permissions of these APIs are defined as the ApiScopes</p>
* <p>For e.g. &ldquo;client&rdquo; is the ApiResource.</p>
* <p>Then for this resource, the apiscopes are the permissions defined e.g. &ldquo;client.read&rdquo;, &ldquo;client.write&rdquo;, &ldquo;client.delete&rdquo; and &ldquo;client.manage&rdquo;</p>
* <p>The ApiResourceClaim is the <strong> claimtype </strong> that needs to be added to the custom claim if the scope input parameter matches the ApiResource.</p>
* <p>The types that can be added to the ApiResourceClaim or ApiScopeClaim are as follows:</p>
* <ul>
* <li><strong> Role </strong> -&gt; Claims are fetched from the UserRole/Role table for the logged in user</li>
* <li><strong> Permission </strong> -&gt; Claims that match the claimType in the RoleClaims for that particular user role table is fetched</li>
* <li><strong> Locale </strong> -&gt; Claims that match the claimType in the UserClaims table is fetched</li>
* </ul>
* <p>The input scope generally consists of the following</p>
* <ul>
* <li>IdentityResource</li>
* <li>ApiResource</li>
* <li>ApiScope</li>
* </ul>
* <p><strong>Scope Input matches IdentityResource</strong></p>
* <ul>
* <li>If the scope input is Identityresource, then the custom claims generated are based on the Identity reource claims configured.</li>
* <li>The Identity resource claim's <strong>type or the aliastype&nbsp;</strong>will be matched with the Identity claim and the corresponding value in the UserModel will be fetched and added as claim value and these are given as custom claims in the Identity token.</li>
* </ul>
* <p><strong>Scope input matches ApiResource </strong></p>
* <ul>
* <li>If the scope input is apiresource, then the custom claims generated are based on the ApiResource claims configured.</li>
* <li>If the apiresource claim contains the &ldquo;permission&rdquo; claimtype, then the permissions that are configured for the user, i.e. the roleclaims and the userclaims are added.</li>
* <li>If the apiresource claim contains the &ldquo;role&rdquo; claimtype, then the roles that are configured for the user are added to the access token custom claims.</li>
* </ul>
* <p><strong>Scope Input matches ApiScope</strong></p>
* <ul>
* <li>If the scope input is apiscope, then the custom claims generated are based on the ApiScope claims configured.</li>
* <li>if the apiscope claim contains the "permission" claimtype, then the permissions that are configured for the user, i.e. the roleclaims and the userclaims are added.</li>
* <li>If the apiscope claim contains the "role" claimtype, then the roles that are configured for the user are added to the access token custom claims.</li>
* </ul>
* <p><strong>Note: </strong>The client application can define their own <strong> custom claim types </strong> and shall include it in the ApiResourceClaim or ApiScopeClaim so that the values for these claims will be fetched from the UserClaims/RoleClaims.</p>
*/


