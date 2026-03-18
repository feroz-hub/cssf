/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

/**
* @page LDAPIntegration LDAP Integration
* <p><strong> LDAP Integration </strong></p>
* <p>Below is the LDAP integration approach&nbsp;followed to authenticate HCL.CS.SF user:</p>
* \image html LDAPIntegration.jpg
* <ul>
* <li>HCL.CS.SF is designed to interface with LDAP to authenticate user if it is enabled, it will bypass the user registration and verification process. System local account will be created based on LDAP inputs (e.g: including user id, first name, email, phone number etc.) and by default user account will be marked as verified. It requires few configuration parameters to establish connection such as domain name and port at which LDAP is currently running. HCL.CS.SF will verify the LDAP parameters while initializing, if any error occurred and then LDAP integration option will be disabled.</li>
* <li>HCL.CS.SF will establish connection with LDAP in both secure and non-secure mode, secure connection requires TLS client cerificates and port 636 shall be enabled.</li>
* <li>HCL.CS.SF also capable to sync LDAP user parameters to local system account if any changes, it includes email, phone number and any additional information configured as part of LDA. User information syncing is done when user sucessuflly authenticated with LDAP. LDAP sync process will be executed in a separate thread, it will not impact regular LDAP calls.</li>
* <li>HCL.CS.SF also enables multifactor authentication such as SMS OTP, Mobile Authenticator and email token. Certain user management features are restricted to LDAP user such as Change password, Reset password, Generate password reset token, lock user and unlock user. Rest of the user management features will be same as regular user account which is created via user registration and verification process</li>
* </ul>
*/


