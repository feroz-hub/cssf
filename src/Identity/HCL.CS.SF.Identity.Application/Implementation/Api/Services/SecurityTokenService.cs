/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Constants;
using HCL.CS.SF.Domain.Enums;
using HCL.CS.SF.Domain.ErrorCodes;
using HCL.CS.SF.Domain.Models.Api;
using HCL.CS.SF.Domain.Models.Endpoint;
using HCL.CS.SF.DomainServices.Infra;
using HCL.CS.SF.DomainServices.Repository.Api;
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;
using HCL.CS.SF.Service.Interfaces.Interfaces.Api;

namespace HCL.CS.SF.Service.Implementation.Api.Services;

/// <summary>
/// Service for querying OAuth2/OIDC security tokens (access tokens, refresh tokens) issued
/// by the identity server. Provides administrative views of active tokens filtered by client,
/// user, or date range -- used for token revocation dashboards and security monitoring.
/// </summary>

public class SecurityTokenService(
    ILoggerInstance instance,
    IFrameworkResultService frameworkResult,
    ISecurityTokenRepository securityTokenRepository)
    : SecurityBase, ISecurityTokenService
{
    private readonly ILoggerService loggerService = instance.GetLoggerInstance(LoggerKeyConstants.DefaultLoggerKey);

    /// <summary>
    /// Retrieves active security tokens for the specified client IDs with optional paging.
    /// </summary>
    /// <param name="clientIds">The client identifiers to filter tokens by.</param>
    /// <param name="page">Optional paging parameters.</param>
    /// <returns>A list of active tokens for the specified clients.</returns>
    public virtual async Task<IList<TokenModel>> GetClientsActiveSecurityTokensAsync(IList<string> clientIds,
        PagingModel page = null)
    {
        if (!clientIds.ContainsAny()) frameworkResult.Throw(EndpointErrorCodes.ClientIdIsRequired);

        try
        {
            loggerService.WriteTo(Log.Debug, "Entered into get active security tokens for clients");
            return await securityTokenRepository.GetSecurityTokenAsync(page, SecurityTokenOption.Client,
                clientIds: clientIds);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Retrieves active security tokens for the specified user IDs with optional paging.
    /// </summary>
    /// <param name="userIds">The user identifiers to filter tokens by.</param>
    /// <param name="page">Optional paging parameters.</param>
    /// <returns>A list of active tokens for the specified users.</returns>
    public virtual async Task<IList<TokenModel>> GetUsersActiveSecurityTokensAsync(IList<string> userIds,
        PagingModel page = null)
    {
        if (!userIds.ContainsAny()) frameworkResult.Throw(EndpointErrorCodes.InvalidUser);

        try
        {
            loggerService.WriteTo(Log.Debug, "Entered into get active security tokens for users");
            return await securityTokenRepository.GetSecurityTokenAsync(page, SecurityTokenOption.User,
                userIds: userIds);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Retrieves active (non-expired) security tokens issued within the specified date range.
    /// </summary>
    /// <param name="fromdate">The start of the date range (inclusive).</param>
    /// <param name="todate">The end of the date range (inclusive).</param>
    /// <param name="page">Optional paging parameters.</param>
    /// <returns>A list of active tokens within the date range.</returns>
    public virtual async Task<IList<TokenModel>> GetActiveSecurityTokensAsync(DateTime fromdate, DateTime todate,
        PagingModel page = null)
    {
        if (fromdate > todate) frameworkResult.Throw(ApiErrorCodes.FromDateGreaterThanToDate);

        try
        {
            loggerService.WriteTo(Log.Debug, "Entered into get active security tokens based on given dates");
            return await securityTokenRepository.GetSecurityTokenAsync(page, SecurityTokenOption.BetweenDates, fromdate,
                todate);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Retrieves all security tokens (active and expired) issued within the specified date range.
    /// </summary>
    /// <param name="fromdate">The start of the date range (inclusive).</param>
    /// <param name="todate">The end of the date range (inclusive).</param>
    /// <param name="page">Optional paging parameters.</param>
    /// <returns>A list of all tokens within the date range.</returns>
    public virtual async Task<IList<TokenModel>> GetAllSecurityTokensAsync(DateTime fromdate, DateTime todate,
        PagingModel page = null)
    {
        if (fromdate > todate) frameworkResult.Throw(ApiErrorCodes.FromDateGreaterThanToDate);

        try
        {
            loggerService.WriteTo(Log.Debug, "Entered into get active security tokens based on given dates");
            return await securityTokenRepository.GetSecurityTokenAsync(page, SecurityTokenOption.All, fromdate, todate);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, ex.Message);
            throw;
        }
    }
}
