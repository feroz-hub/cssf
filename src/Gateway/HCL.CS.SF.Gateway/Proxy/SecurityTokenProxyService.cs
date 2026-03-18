/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Models.Api;
using HCL.CS.SF.Domain.Models.Endpoint;
using HCL.CS.SF.DomainServices.Infra;
using HCL.CS.SF.DomainServices.Repository.Api;
using HCL.CS.SF.Service.Implementation.Api.Services;
using HCL.CS.SF.Service.Interfaces.Interfaces.Api;
using HCL.CS.SF.Service.Interfaces.Interfaces.Api.Wrapper;

namespace HCL.CS.SF.ProxyService.Proxy;

/// <summary>
/// Gateway proxy for the security token service. Routes requests to the backend
/// <see cref="SecurityTokenService"/> after enforcing API-level permission validation.
/// Provides operations for querying active and historical security tokens by client IDs,
/// user IDs, or date ranges.
/// </summary>
public sealed class SecurityTokenProxyService : SecurityTokenService, ISecurityTokenService
{
    /// <summary>
    /// Validator that checks whether the caller has permission to invoke the requested operation.
    /// </summary>
    private readonly IApiValidator apiValidator;

    /// <summary>
    /// Service used to construct failure responses when validation fails.
    /// </summary>
    private readonly IFrameworkResultService frameworkResult;

    /// <summary>
    /// Initializes a new instance of the <see cref="SecurityTokenProxyService"/> class.
    /// </summary>
    /// <param name="apiValidator">The API permission validator.</param>
    /// <param name="instance">Logger instance factory.</param>
    /// <param name="frameworkResult">Service for constructing framework result objects.</param>
    /// <param name="securityTokenRepository">Repository for security token persistence.</param>
    public SecurityTokenProxyService(
        IApiValidator apiValidator,
        ILoggerInstance instance,
        IFrameworkResultService frameworkResult,
        ISecurityTokenRepository securityTokenRepository)
        : base(
            instance,
            frameworkResult,
            securityTokenRepository)
    {
        this.apiValidator = apiValidator;
        this.frameworkResult = frameworkResult;
    }

    /// <summary>
    /// Retrieves active security tokens for the specified client IDs after permission validation.
    /// </summary>
    /// <param name="clientIds">The list of client identifiers to query tokens for.</param>
    /// <param name="page">Optional paging parameters.</param>
    /// <returns>A list of active token models for the requested clients.</returns>
    public override async Task<IList<TokenModel>> GetClientsActiveSecurityTokensAsync(IList<string> clientIds,
        PagingModel page = null)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.GetClientsActiveSecurityTokensAsync(clientIds, page);
    }

    /// <summary>
    /// Retrieves active security tokens for the specified user IDs after permission validation.
    /// </summary>
    /// <param name="userIds">The list of user identifiers to query tokens for.</param>
    /// <param name="page">Optional paging parameters.</param>
    /// <returns>A list of active token models for the requested users.</returns>
    public override async Task<IList<TokenModel>> GetUsersActiveSecurityTokensAsync(IList<string> userIds,
        PagingModel page = null)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.GetUsersActiveSecurityTokensAsync(userIds, page);
    }

    /// <summary>
    /// Retrieves active security tokens within the specified date range after permission validation.
    /// </summary>
    /// <param name="fromdate">The start of the date range.</param>
    /// <param name="todate">The end of the date range.</param>
    /// <param name="page">Optional paging parameters.</param>
    /// <returns>A list of active token models within the date range.</returns>
    public override async Task<IList<TokenModel>> GetActiveSecurityTokensAsync(DateTime fromdate, DateTime todate,
        PagingModel page = null)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.GetActiveSecurityTokensAsync(fromdate, todate, page);
    }

    /// <summary>
    /// Retrieves all security tokens (active and expired) within the specified date range
    /// after permission validation.
    /// </summary>
    /// <param name="fromdate">The start of the date range.</param>
    /// <param name="todate">The end of the date range.</param>
    /// <param name="page">Optional paging parameters.</param>
    /// <returns>A list of all token models within the date range.</returns>
    public override async Task<IList<TokenModel>> GetAllSecurityTokensAsync(DateTime fromdate, DateTime todate,
        PagingModel page = null)
    {
        var result = await apiValidator.ValidateRequest();
        if (result.Status == ResultStatus.Failed)
            frameworkResult.ThrowCustomMessage(result.Errors.FirstOrDefault().Description);

        return await base.GetAllSecurityTokensAsync(fromdate, todate, page);
    }
}
