/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

/**
* @page ModulesDescription Module Description
* <p>Below are the high-level modules developed as part of HCL.CS.SF, it uses 3 <sup> rd </sup> party libraries as well.</p>
* <p><table>
* <tr>
* <td><p><strong> Module Name </strong></p></td>
* <td><p><strong> Description </strong></p></td>
* <td><p><strong> Make/Buy/Re-Use </strong></p></td>
* <td><p><strong> Verification / Validation for Buy/re-use </strong></p></td>
* <td><p><strong> Remarks </strong></p></td>
* </tr>
* <tr>
* <td><p><strong> User Management </strong></p></td>
* <td><p>User Management module is used to manage user accounts such as create, view, update, validate, delete, lock/unlock, enable/disable accounts.</p></td>
* <td><p>Make</p></td>
* <td><p>No verification required</p></td>
* <td><p>It is internally developed module based on ASP.NET identity framework.</p></td>
* </tr>
* <tr>
* <td><p><strong> Authentication </strong></p></td>
* <td><p>Used to authenticate the user based on credentials with MFA (Multi-factor authentication), it also capable to integrate with LDAP(Lightweight Directory Access Protocol).</p></td>
* <td><p>Make</p></td>
* <td><p>No verification required</p></td>
* <td><p>It is internally developed module based on ASP.NET identity framework.</p></td>
* </tr>
* <tr>
* <td><p><strong> Authorization </strong></p></td>
* <td><p>Used to verify the user permission based on token generated for the user. JWT token carries all user roles and permission which is used to validate user permission.</p></td>
* <td><p>Make</p></td>
* <td><p>No verification required</p></td>
* <td><p>It is internally developed module based on ASP.NET identity framework.</p></td>
* </tr>
* <tr>
* <td><p><strong> Notification </strong></p></td>
* <td><p>Used to send notification (SMS or email) for account and system related changes such as password expiry.</p></td>
* <td><p>Buy</p></td>
* <td><p>Verification required</p></td>
* <td><p>Notification modules uses Twilio, it is 3 <sup> rd </sup> party tool.</p></td>
* </tr>
* <tr>
* <td><p><strong> Audit Trail </strong></p></td>
* <td><p>For auditing the all user and system actions with old and new states.</p></td>
* <td><p>Make</p></td>
* <td><p>No verification required</p></td>
* <td><p>Internally developed component</p></td>
* </tr>
* <tr>
* <td><p><strong> Logger </strong></p></td>
* <td><p>Used to log all user and system related actions with user id, date time, activity etc.</p></td>
* <td><p>Buy</p></td>
* <td><p>Verification required</p></td>
* <td><p>Logger modules uses Serilog, a 3 <sup> rd </sup> party tool.</p></td>
* </tr>
* </table></p>
* <p>&nbsp;</p>
*/


