/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

/**
* @page NotificationTemplateFormat Notification Template Format
* <p>Notifications are sent to the user and administrators in a format defined in the HCL.CS.SF.
* <para>There are separate template formats for <strong>Email</strong> and <strong>SMS</strong>.</para>
* <para>This format is specified in the NotificationTemplateSettings.JSON file. Please find below a sample settings file.</para>
* <para><strong>Template structure for Email</strong></para>
* \code
        NotificationTemplateSettings": {

        "EmailTemplateCollection": [
        {
        "Name": "DefaultTemplate",
        "Subject": "Your verification code - HCL.CS.SF",
        "FromAddress": "no-reply@HCL.CS.SF.com",
        "FromName": "HCL.CS.SF",
        "CC": "",
        "TemplateFormat": "<html><head><meta charset=\"utf-8\"><title></title></head><body><p>Hello {USERNAME},<br />Your HCL.CS.SF verification code is {TOKEN}.<br /></body></html>"
        },

        {
        "Name": "EmailVerificationUsingLink",
        "Subject": "Verify your email - HCL.CS.SF",
        "FromAddress": "no-reply@HCL.CS.SF.com",
        "FromName": "HCL.CS.SF",
        "CC": "",
        "TemplateFormat": "<html><head><meta charset=\"utf-8\"><title></title></head><body><p>Hello {USERNAME},<br />Click the following link to verify your HCL.CS.SF email address.<br /><br /><a href=\"https://localhost:50001/Account/ConfirmEmail?uid={USERID}&token={TOKEN}\">Verify email</a></body></html>"
        },
* \endcode
* <para>The sample settings file consists of the EmailTemplateCollection header in the JSON, which acts as the template The Default template name given <strong>should not be changed.</strong></para>
* The Subject, FromAddress and FromName are mandatory items to be filled. The "TemplateFormat" defined can have HCL.CS.SF defined placeholders like {USERNAME}, {FIRSTNAME} etc.
* These placeholders will be filled by the security framewok with the actual values while the email template is processed. More than one email template can be configured.

* <para><strong>Template structure for SMS</strong></para>
* \code
        NotificationTemplateSettings": {

        "SMSTemplateCollection": [
        {

        "Name": "PhoneNumberVerificationToken",
        "TemplateFormat": "HCL.CS.SF: Hello {USERNAME}, your phone verification code is {TOKEN}."
        },

        {
        "Name": "ResetPasswordUsingToken",
        "TemplateFormat": "HCL.CS.SF: Hello {USERNAME}, your password reset code is {TOKEN}."
        },

* \endcode
* <para>The sample settings file consists of the SMSTemplateCollection header in the JSON, which acts as a template. The Default template name given <strong>should not be changed.</strong></para>
* The "TemplateFormat" defined can have HCL.CS.SF defined placeholders similar to the email template like {USERNAME}, {FIRSTNAME} etc. The placeholders are case sensitive and should be encapsulated within the braces ({}).
* These placeholders will be filled by the security framewok with the actual values while the sms template is processed. More than one sms template can be configured in the JSON file.
* <para><strong>PlaceHolder Names and their meanings in the template format.</strong></para>
* <table>
* <tr>
* <td><strong>PlaceHolder</strong> </td>
* <td><strong>Description</strong> </td>
* </tr>
* <tr>
* <td>{"USERNAME"}</td>
* <td>The user's Login Name </td>
* </tr>
* <tr>
* <td>{"FIRSTNAME"}</td>
* <td>User's First Name</td>
* </tr>
* <tr>
* <td>{"LASTNAME"} </td>
* <td>User's Last Name</td>
* </tr>
* <tr>
* <td>{"FULLNAME"}</td>
* <td>Users' Full Name including FirstName and LastName</td>
* </tr>
* <tr>
* <td>{"TOKEN"}</td>
* <td>Token for SMS/Email</td>
* </tr>
* <tr>
* <td>{"USERID"}</td>
* <td>Unique reference number for activating or confirming user email address</td>
* </tr>
* <tr>
* <td>{"EMAIL"}</td>
* <td>User's email address</td>
* </tr>
* </table>
*/

