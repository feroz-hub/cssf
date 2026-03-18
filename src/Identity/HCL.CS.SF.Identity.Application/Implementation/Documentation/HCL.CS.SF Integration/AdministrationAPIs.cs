/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

/**
* @page AdministrationAPIs Administration Apis
* <p><strong>Admin User Capablities in the HCL.CS.SF </strong></p>
* <p>The Admin user is responsible to configure metadata/master data to run the HCL.CS.SF as intended.</p>
* <p>Admin User capabilities can be broadly classified into two</p>
* <ul>
* <li><strong>Initial master data creation</strong>&nbsp;
* <ul>
* <li>Roles and Role claims</li>
* <li>Api resources, Api scopes, Api resource claims and Api scope claims</li>
* <li>Identity resources and claims</li>
* <li>Client</li>
* <li>User security questions and user claims for administration</li>
* </ul>
* </li>
* <li><strong>Managing the data</strong>
* <ul>
* <li>Tokens - access and refresh token revocation.</li>
* <li>Client addition, updation and deletion.</li>
* </ul>
* </li>
* <li><strong>Configuration for system initial setup&nbsp;</strong>
* <ul>
* <li>LDAP, SMS, Email</li>
* <li>Notification Templates</li>
* <li>Logger configuration.</li>
* </ul>
* </li>
* </ul>
<p>If the above information is not configured by the administrator, the HCL.CS.SF will fail due to insufficient dependent data that are need to generate tokens.</p>
* <p><strong>The Calling procedure for APIs in the HCL.CS.SF</strong></p>
* <table>
* <tr>
* <td align="center"><strong>Steps</strong></td>
* <td align="center"><strong>Description</strong></td>
* </tr>
* <tr>
* <td><strong>Step 1</strong></td>
* <td><p>Authentication header value is set with the access token obtained via endpoints. The access token shall be stored and accessed across (global variable) the application.</p>
* \code
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Global.AccessToken);
* \endcode
* <p><strong>Note:&nbsp;</strong>The authentication header is needed only for secure API calls.</p>
* </td></tr>
* <tr><td><strong>Step 2</strong></td>
* <td><p>The absolute Url for the corresponding API to make a call to the hosted APIs.</p>
* \code
        var roleurl = ApplicationConstants.BaseUrl + ApiRoutePathConstants.GetRoleClaim;
* \endcode
* </td></tr>
* <tr><td><strong>Step 3</strong></td>
* <td><p>The input parameter is passed by serializing into a JSON object with UTF8 encoding into the HTTP Client's PostAsync() method.</p>
* \code
        var roleresponse = await HttpClient.PostAsync(
        roleurl,
        new StringContent(JsonConvert.SerializeObject(getRole), Encoding.UTF8, "application/json"));
* \endcode
* </td></tr>
* <tr><td><strong>Step 4</strong></td>
* <td><p>Read the response from the Http Post call.</p>
* \code
        var roleResultResponse = await roleresponse.Content.ReadAsStringAsync();
* \endcode
* </td></tr>
* <tr><td><strong>Step 5</strong></td>
* <td><p>Deserialize the response received form the Http Post call and consume the response model wherever needed.</p>
* \code
       var claims = JsonConvert.DeserializeObject<IList<RoleClaimModel>>(roleResultResponse);
* \endcode
* </td></tr>
* </table>
* <p><strong>The sample code for making API calls and receiving response is as shown below </strong></p>
* \code
        var getRole = item as RoleModel;
        Http.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Global.AccessToken);
        var roleurl = ApplicationConstants.BaseUrl + ApiRoutePathConstants.GetRoleClaim;
        var roleresponse = await Http.Client.PostAsync(
        roleurl,
        new StringContent(JsonConvert.SerializeObject(getRole), Encoding.UTF8, "application/json"));
        var roleResultResponse = await roleresponse.Content.ReadAsStringAsync();
        var claims = JsonConvert.DeserializeObject<IList<RoleClaimModel>>(roleResultResponse);
* \endcode
*/


