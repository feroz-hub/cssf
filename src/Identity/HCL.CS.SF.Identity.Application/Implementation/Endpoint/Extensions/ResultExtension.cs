/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain.Models.Endpoint.Request;
using HCL.CS.SF.Domain.Models.Endpoint.Response;
using HCL.CS.SF.Service.Implementation.Endpoint.Results;
using HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint;

namespace HCL.CS.SF.Service.Implementation.Endpoint.Extensions;

/// <summary>
/// Factory extension methods for creating OAuth/OIDC error endpoint results.
/// Provides convenient methods to construct <see cref="AuthorizeResult"/>,
/// <see cref="ErrorResult"/>, and <see cref="UserInfoResult"/> error responses.
/// </summary>
internal static class ResultExtension
{
    /// <summary>
    /// Performs the error operation.
    /// </summary>
    /// <returns>The operation result.</returns>
    internal static AuthorizeResult Error(this ValidatedAuthorizeRequestModel validationModel,
        ISessionManagementService session, string error, string errorDescription = null)
    {
        var validResponse = new AuthorizationResponseModel
        {
            Request = validationModel,
            IsError = true,
            ErrorCode = error,
            ErrorDescription = errorDescription
        };

        return new AuthorizeResult(validResponse, session);
    }

    /// <summary>
    /// Performs the error operation.
    /// </summary>
    /// <param name="error">The error.</param>
    /// <param name="errorDescription">The error description.</param>
    /// <returns>The operation result.</returns>
    internal static ErrorResult Error(this string error, string errorDescription = null)
    {
        var response = new ErrorResponseModel
        {
            ErrorCode = error,
            ErrorDescription = errorDescription
        };
        return new ErrorResult(response);
    }

    /// <summary>
    /// Performs the user info error operation.
    /// </summary>
    /// <param name="error">The error.</param>
    /// <param name="errorDescription">The error description.</param>
    /// <returns>The operation result.</returns>
    internal static UserInfoResult UserInfoError(this string error, string errorDescription = null)
    {
        var response = new ErrorResponseModel
        {
            ErrorCode = error,
            ErrorDescription = errorDescription
        };
        return new UserInfoResult(null, response);
    }
}
