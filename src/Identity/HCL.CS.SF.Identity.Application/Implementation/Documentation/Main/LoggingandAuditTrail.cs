/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

/**
* @page LoggingAndAuditTrail Logging and Audit Trail
* <p><strong>Logging</strong></p>
* <p>HCL.CS.SF supports the diagnostic logging for the client application.
* The log file records the events that occur in the HCL.CS.SF. This type of diagnostic logging helps the developers to analyze the code and debug if an unexpected error scenario happens in the production environment. </p>
* <p>The developers can scan through the log file generated and fix the bug in the system based on the analysis of the log file.</p>
* <p>HCL.CS.SF supports logging to a file based on the criticality of the messages logged.
* There are two types of messages defined in the HCL.CS.SF.</p>
* <ul>
* <li><strong>Error Messages</strong> - Indicates a critical error in a function/process.</li>
* <li><strong>Debug Messages</strong> - These messages are written to track the flow in a particular process like user login, token generation etc. The sequence of messages are detected and if any particular sequence of message is missing, then the problem is identified.</li>
* </ul>
* <p>After the HCL.CS.SF is integrated, the server application can instantiate their own logger instance provided readily.</p>
* <p>The Log configuration is set by providing the InstanceName, Mode of Logging (Database or File), and the Log file path.</p>
* <p>The Logger instance shall be added using the .AddLoggerInstance(logConfig) method to the services collection in the Startup.ConfigureServices() function of the MVC server application.
* <p>The code for adding a named logger instance is shown as below :</p>
* \code
        var logConfig = new LogConfig();
        logConfig.LogFileConfig = new LogFileConfig();
        logConfig.InstanceName = LogKeyConstants.Authentication;
        logConfig.WriteLogTo = WriteLogTo.File;
        logConfig.LogFileConfig.FilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs\\Authentication.txt");

        services.AddHCLCSSF(
        "./Configurations/SystemSettings.json",
        "./Configurations/TokenSettings.json",
        "./Configurations/NotificationTemplateSettings.json")
        .AddAsymmetricKeystore(LoadAsymmetricCertificate())
        .AddLoggerInstance(logConfig);
* \endcode
* <p>The Instance name (LogKeyConstants.Authentication) that is set in the Logger is the unique instance that is loaded into the dependency class which can be injected in the controllers in the constructor using the following code.</p>
* \code
        public AccountController(ILoggerInstance instance)
        {
            loggerService = instance.GetLoggerInstance(LogKeyConstants.Authentication);
        }
* \endcode
* <p><strong>Note:&nbsp;</strong>A unique instance name is used for logging all the HCL.CS.SF related components.</p>
* <p><strong>AuditTrail</strong></p>
* <p>HCL.CS.SF supports logging all the detailed transactions relating to changes in any item in the fields of a table in the database supported.</p>
* The following is captured as part of the audit trail
* <ul>
* <li><strong>Created On</strong> - The timestamp when the audit trail entry was created.</li>
* <li><strong>Created By</strong> - The user who created the audit trail entry.</li>
* <li><strong>Action Type</strong> - The entry based on the following actions, 1 -> Addition, 2-> Modification, 3-Deletion.</li>
* <li><strong>TableName</strong> - The table name where the action happened.</li>
* <li><strong>OldValue</strong> - The old value that was there before the action(add/mod/del) happened.</li>
* <li><strong>NewValue</strong> - The New value entered after the action(add/mod/del) happened.</li>
* <li><strong>AffectedColumn </strong>- The columns in the table affected by the action(add/mod/del).</li>
* <li><strong>UserId</strong> - The unique id of the user who did the action.</li>
* </ul>
* <p>Please find the sample audit trail entries below</p></para>
* \code
 ID                                     CreatedOn       CreatedBy   ActionType  TableName   OldValue                                            NewValue                                                                                            Affected Column                     UserId
969E3852-00FD-4B6B-341C-08DA54E80684    14:34.32         BobAlice   2           Users       {"LastLoginDateTime":null,"ModifiedOn":null}        {"LastLoginDateTime":"2022-06-23T07:14:34.1935262Z","ModifiedOn":"2022-06-23T07:14:34.2269084Z"}    ["LastLoginDateTime","ModifiedOn"]  53FAE905-D41D-45B5-AA0D-3EBA2344E6D2
A499BC34-68EB-4C2B-341D-08DA54E80684    25:37.61         BobAlice   2           Users       {"LastLoginDateTime":"2022-06-23T07:14:34.1935262"} {"LastLoginDateTime":"2022-06-23T07:25:37.6332943Z","ModifiedOn":"2022-06-23T07:25:37.635081Z"}     ["LastLoginDateTime","ModifiedOn"]  53FAE905-D41D-45B5-AA0D-3EBA2344E6D2
3FE263EF-1042-4CB4-CEBB-08DA54EC7E19    46:33.07         JohnDoe    2           Users       {"LastLoginDateTime":null,"ModifiedOn":null}        {"LastLoginDateTime":"2022-06-23T07:46:32.883411Z","ModifiedOn":"2022-06-23T07:46:32.9436624Z"}     ["LastLoginDateTime","ModifiedOn"]  5D866F52-8B35-4394-B247-9C2442DE57C9
FAD0FF24-0088-47A5-A90E-08DA54EEF2BF    04:07.72         JohnDoe    2           Users       {"LastLoginDateTime":"2022-06-23T07:46:32.883411"}  {"LastLoginDateTime":"2022-06-23T08:04:07.55886Z","ModifiedOn":"2022-06-23T08:04:07.630626Z"}       ["LastLoginDateTime","ModifiedOn"]  5D866F52-8B35-4394-B247-9C2442DE57C9
* \endcode
* <para></para>
*/



