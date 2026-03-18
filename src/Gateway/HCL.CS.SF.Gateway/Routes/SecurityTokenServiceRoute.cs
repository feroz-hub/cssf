/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Newtonsoft.Json.Linq;
using HCL.CS.SF.Domain.Constants;
using HCL.CS.SF.Domain.Models.Api;
using HCL.CS.SF.Service.Interfaces.Interfaces.Api.Wrapper;

namespace HCL.CS.SF.ProxyService.Routes;

/// <summary>
/// Partial class of <see cref="ApiGateway"/> containing route handlers for security token
/// query operations. Handles routes for retrieving active and historical security tokens
/// filtered by client IDs, user IDs, or date ranges, with optional paging.
/// </summary>
internal partial class ApiGateway : BaseApiServiceInstance, IApiGateway
{
    /// <summary>
    /// Handles the route for retrieving active security tokens by a list of client IDs.
    /// Parses client IDs and optional paging from the JSON body.
    /// </summary>
    private async Task<bool> GetActiveSecurityTokensByClientIds(string jsonContent)
    {
        PagingModel paging = null;
        var jsonObjects = JObject.Parse(jsonContent);
        var clientsList = jsonObjects[ApiRouteParameterConstants.ClientsList].ToObject<IList<string>>();
        if (jsonObjects.ContainsKey(ApiRouteParameterConstants.PagingModel))
            paging = jsonObjects[ApiRouteParameterConstants.PagingModel].ToObject<PagingModel>();

        var tokenModel = await SecurityTokenService.GetClientsActiveSecurityTokensAsync(clientsList, paging);
        await GenerateApiResults(tokenModel);
        return true;
    }

    /// <summary>
    /// Handles the route for retrieving active security tokens by a list of user IDs.
    /// Parses user IDs and optional paging from the JSON body.
    /// </summary>
    private async Task<bool> GetActiveSecurityTokensByUserIds(string jsonContent)
    {
        PagingModel paging = null;
        var jsonObjects = JObject.Parse(jsonContent);
        var userList = jsonObjects[ApiRouteParameterConstants.UserList].ToObject<IList<string>>();
        if (jsonObjects.ContainsKey(ApiRouteParameterConstants.PagingModel))
            paging = jsonObjects[ApiRouteParameterConstants.PagingModel].ToObject<PagingModel>();

        var tokenModel = await SecurityTokenService.GetUsersActiveSecurityTokensAsync(userList, paging);
        await GenerateApiResults(tokenModel);
        return true;
    }

    /// <summary>
    /// Handles the route for retrieving active security tokens within a date range.
    /// Parses from/to dates and optional paging from the JSON body.
    /// </summary>
    private async Task<bool> GetActiveSecurityTokensBetweenDates(string jsonContent)
    {
        PagingModel paging = null;
        var jsonObjects = JObject.Parse(jsonContent);
        var fromdate = jsonObjects[ApiRouteParameterConstants.FromDate].ToObject<DateTime>();
        var todate = jsonObjects[ApiRouteParameterConstants.ToDate].ToObject<DateTime>();
        if (jsonObjects.ContainsKey(ApiRouteParameterConstants.PagingModel))
            paging = jsonObjects[ApiRouteParameterConstants.PagingModel].ToObject<PagingModel>();

        var tokenModel = await SecurityTokenService.GetActiveSecurityTokensAsync(fromdate, todate, paging);
        await GenerateApiResults(tokenModel);
        return true;
    }

    /// <summary>
    /// Handles the route for retrieving all security tokens (active and expired) within a date range.
    /// Parses from/to dates and optional paging from the JSON body.
    /// </summary>
    private async Task<bool> GetAllSecurityTokensBetweenDates(string jsonContent)
    {
        PagingModel paging = null;
        var jsonObjects = JObject.Parse(jsonContent);
        var fromdate = jsonObjects[ApiRouteParameterConstants.FromDate].ToObject<DateTime>();
        var todate = jsonObjects[ApiRouteParameterConstants.ToDate].ToObject<DateTime>();
        if (jsonObjects.ContainsKey(ApiRouteParameterConstants.PagingModel))
            paging = jsonObjects[ApiRouteParameterConstants.PagingModel].ToObject<PagingModel>();

        var tokenModel = await SecurityTokenService.GetAllSecurityTokensAsync(fromdate, todate, paging);
        await GenerateApiResults(tokenModel);
        return true;
    }
}
