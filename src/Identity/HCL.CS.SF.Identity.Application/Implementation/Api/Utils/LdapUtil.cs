/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.DirectoryServices.Protocols;
using System.Net;
using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Configurations.Api;
using HCL.CS.SF.Domain.Enums;
using HCL.CS.SF.Domain.ErrorCodes;
using HCL.CS.SF.Domain.Models.Api;
using HCL.CS.SF.DomainServices.Infra;
using HCL.CS.SF.Service.Implementation.Api.Validators;
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;
using HCL.CS.SF.Service.Interfaces.Interfaces.Api;

namespace HCL.CS.SF.Service.Implementation.Api.Utils;

/// <summary>
/// Utility class for LDAP (Lightweight Directory Access Protocol) integration, enabling
/// enterprise directory-based authentication. Handles LDAP bind operations to verify credentials,
/// user lookup via configurable search filters, and automatic provisioning/synchronization
/// of LDAP users into the local identity store for seamless federation.
/// </summary>

internal class LdapUtil
{
    /// <summary>LDAP search filter template for finding user entries by uid across common objectClass types.</summary>

    private const string LdapGetUserQuery =
        "(|(&(objectCategory=person)(objectClass=user)(uid={0}))(&(objectClass=person)(uid={0}))(&(objectClass=user)(uid={0})))";

    private readonly IFrameworkResultService frameworkResult;
    private readonly LdapConfig ldapConfig;
    private readonly ILoggerService loggerService;
    private readonly IUserAccountService userAccountService;

    /// <summary>
    /// Initializes a new instance of the <see cref="LdapUtil"/> class.
    /// </summary>
    public LdapUtil(
        ILoggerService loggerService,
        LdapConfig ldapConfig,
        IFrameworkResultService frameworkResult,
        IUserAccountService userAccountService)
    {
        this.loggerService = loggerService;
        this.ldapConfig = ldapConfig;
        this.frameworkResult = frameworkResult;
        this.userAccountService = userAccountService;
    }

    /// <summary>
    /// Authenticates a user against the configured LDAP directory. Searches for the user by username,
    /// then performs an LDAP bind with the user's distinguished name and password. On success,
    /// provisions or synchronizes the user in the local identity database.
    /// </summary>
    /// <param name="username">The LDAP username (uid) to authenticate.</param>
    /// <param name="password">The plaintext password for LDAP bind verification.</param>
    /// <returns>A framework result indicating success or the specific LDAP error.</returns>

    internal async Task<FrameworkResult> LdapLoginAsync(string username, string password)
    {
        try
        {
            var userList = await GetUserAsync(username, null);
            if (userList.ContainsAny())
            {
                if (userList.Count > 1)
                    return frameworkResult.Failed<FrameworkResult>(ApiErrorCodes.DuplicateLDAPUserFound);

                var user = userList[0];
                loggerService.WriteTo(Log.Debug, "Entered in Login : User count : " + user.Count);
                using var ldapConnection = await CreateLdapConnection();
                var dn = user["DN"];
                var credential = new NetworkCredential(dn, password);
                if (!ldapConfig.IsSecureConnection)
                {
                    ldapConnection.AuthType = AuthType.Basic;
                    ldapConnection.SessionOptions.SecureSocketLayer = false;
                    ldapConnection.SessionOptions.ProtocolVersion = 3;
                }
                else
                {
                    ldapConnection.AuthType = AuthType.Digest;
                    ldapConnection.SessionOptions.SecureSocketLayer = true;
                }

                ldapConnection.Bind(credential);
                loggerService.WriteTo(Log.Debug, "Ldap user login successful for user: " + username);
                return await CreateLdapUser(username, password, user);
            }

            return frameworkResult.Failed<FrameworkResult>(ApiErrorCodes.InvalidLDAPUserNameOrPassword);
        }
        catch (LdapException ldapException)
        {
            var errorMessage = ldapException.Message + " Server Error = " + ldapException.ServerErrorMessage;
            return frameworkResult.ConstructFailed(ldapException.ErrorCode.ToString(), errorMessage);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Error while logging ldap user.");
            throw;
        }
    }

    /// <summary>
    /// Searches the LDAP directory for a user by username, optionally retrieving extra attributes.
    /// </summary>
    /// <param name="userName">The username to search for in the LDAP directory.</param>
    /// <param name="extraFields">Optional additional LDAP attributes to retrieve.</param>
    /// <returns>A list of dictionaries containing user attributes, or null if not found.</returns>

    internal async Task<IList<Dictionary<string, string>>> GetUserAsync(string userName, string[] extraFields)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userName)) frameworkResult.Throw(ApiErrorCodes.InvalidLDAPUserName);

            loggerService.WriteTo(Log.Debug, "Fetching ldap User details from user: " + userName);
            var ldapSearchFilter = string.Format(LdapGetUserQuery, userName);
            var userList = await LdapSearch(ldapConfig.LdapDomainName, ldapSearchFilter, extraFields);
            return userList;
        }
        catch (LdapException ldapException)
        {
            var errorMessage = ldapException.Message + " Server Error = " + ldapException.ServerErrorMessage;
            frameworkResult.Throw(errorMessage);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Error while fetching ldap user.");
            throw;
        }

        return null;
    }

    /// <summary>
    /// Creates and configures an LDAP connection using the hostname, port, and security settings
    /// from the LDAP configuration. Supports both SSL and non-SSL connections.
    /// </summary>

    private Task<LdapConnection> CreateLdapConnection()
    {
        if (string.IsNullOrWhiteSpace(ldapConfig.LdapHostName))
            frameworkResult.Throw(ApiErrorCodes.InvalidLDAPHostname);
        else if (ldapConfig.LdapPort <= 0) frameworkResult.Throw(ApiErrorCodes.InvalidLDAPPort);

        loggerService.WriteTo(Log.Debug, "Entered in create ldap connection" + ldapConfig.LdapHostName);
        var serverId = new LdapDirectoryIdentifier(ldapConfig.LdapHostName, ldapConfig.LdapPort);
        var ldapConnection = new LdapConnection(serverId);
        ldapConnection.SessionOptions.ProtocolVersion = 3;
        if (ldapConfig.IsSecureConnection)
        {
            ldapConnection.AuthType = AuthType.External;
            ldapConnection.SessionOptions.SecureSocketLayer = true;
        }
        else
        {
            ldapConnection.AuthType = AuthType.Basic;
            ldapConnection.SessionOptions.SecureSocketLayer = false;
        }

        return Task.FromResult(ldapConnection);
    }

    /// <summary>
    /// Executes an LDAP search request and converts the results into a list of attribute dictionaries.
    /// </summary>

    private async Task<List<Dictionary<string, string>>> LdapSearch(string distinguishedName, string ldapSearchFilter,
        string[] extraFields)
    {
        using var ldapConnection = await CreateLdapConnection();
        var searchRequest = new SearchRequest(distinguishedName, ldapSearchFilter, SearchScope.Subtree, extraFields);
        var searchResponse = (SearchResponse)ldapConnection.SendRequest(searchRequest);
        if (searchResponse != null)
        {
            loggerService.WriteTo(Log.Debug, "Entered in Ldap search for " + distinguishedName);
            var result = new List<Dictionary<string, string>>();
            foreach (SearchResultEntry entry in searchResponse.Entries)
            {
                var tempResult = new Dictionary<string, string>
                {
                    ["DN"] = entry.DistinguishedName
                };
                if (entry.Attributes?.AttributeNames != null && entry.Attributes.AttributeNames.Count > 0)
                    foreach (string attrName in entry.Attributes.AttributeNames)
                        tempResult[attrName] = string.Join(
                            ",",
                            entry.Attributes[attrName].GetValues(typeof(string)));

                result.Add(tempResult);
            }

            return result;
        }

        return null;
    }

    /// <summary>
    /// Provisions or synchronizes an LDAP user in the local identity database.
    /// Creates a new local user if none exists, or updates the existing user's attributes.
    /// </summary>

    private async Task<FrameworkResult> CreateLdapUser(string username, string password,
        Dictionary<string, string> user)
    {
        var userModel = await userAccountService.GetUserByNameAsync(username);
        if (userModel == null) return await CreateLdapUserAsync(username, password, user);

        return await SyncLdapUserAsync(userModel, username, password, user);
    }

    /// <summary>
    /// Creates a new local identity user from LDAP directory attributes.
    /// Maps LDAP fields (mail, mobile, givenname, sn) to the local user model.
    /// </summary>

    private async Task<FrameworkResult> CreateLdapUserAsync(string username, string password,
        Dictionary<string, string> user)
    {
        loggerService.WriteTo(Log.Debug, "Creating LDAP user in local for user: " + username);
        try
        {
            var commonHelper = new UserManagementValidator();
            var userModel = new UserModel();
            userModel = AssignModel(userModel, username, password, user);
            if (!commonHelper.IsValidEmailAddress(userModel.Email))
                return frameworkResult.Failed<FrameworkResult>(ApiErrorCodes.EmailRequired);

            userModel.EmailConfirmed = true;
            userModel.PhoneNumberConfirmed = true;
            userModel.TwoFactorEnabled = ldapConfig.IsTwoFactorAuthenticationRequired;
            userModel.TwoFactorType = ldapConfig.TwoFactorType;
            userModel.CreatedBy = username;
            userModel.IdentityProviderType = IdentityProvider.Ldap;
            return await userAccountService.RegisterUserAsync(userModel);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Failed to create LDAP user.");
            throw;
        }
    }

    /// <summary>
    /// Synchronizes an existing local user's attributes with the latest LDAP directory data.
    /// </summary>

    private async Task<FrameworkResult> SyncLdapUserAsync(UserModel userModel, string username, string password,
        Dictionary<string, string> user)
    {
        loggerService.WriteTo(Log.Debug, "Updating LDAP user in local for user: " + username);
        try
        {
            userModel = AssignModel(userModel, username, password, user);
            return await userAccountService.UpdateUserAsync(userModel);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Failed to sync LDAP user.");
            throw;
        }
    }

    /// <summary>
    /// Maps LDAP directory attributes to the local user model, handling various LDAP
    /// attribute naming conventions (e.g., mail/email/emailaddress, gn/givenname/cn).
    /// </summary>

    private UserModel AssignModel(UserModel userModel, string username, string password,
        Dictionary<string, string> user)
    {
        userModel.UserName = username;
        userModel.Password = password;

        if (user.ContainsKey("mail"))
            userModel.Email = user["mail"];
        else if (user.ContainsKey("email"))
            userModel.Email = user["email"];
        else if (user.ContainsKey("emailaddress"))
            userModel.Email = user["emailaddress"];
        else
            userModel.Email = string.Empty;

        if (user.ContainsKey("mobile"))
            userModel.PhoneNumber = user["mobile"];
        else if (user.ContainsKey("mobileTelephoneNumber")) userModel.PhoneNumber = user["mobileTelephoneNumber"];

        if (user.ContainsKey("gn"))
            userModel.FirstName = user["gn"];
        else if (user.ContainsKey("givenname"))
            userModel.FirstName = user["givenname"];
        else if (user.ContainsKey("cn")) userModel.FirstName = user["cn"];

        if (user.ContainsKey("sn"))
            userModel.LastName = user["sn"];
        else if (user.ContainsKey("surname"))
            userModel.LastName = user["surname"];
        else
            userModel.LastName = string.Empty;

        return userModel;
    }
}
