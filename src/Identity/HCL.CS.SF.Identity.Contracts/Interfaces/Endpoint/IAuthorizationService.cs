/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Entities.Endpoint;
using HCL.CS.SF.Domain.Models.Endpoint;
using HCL.CS.SF.Domain.Models.Endpoint.Request;
using HCL.CS.SF.Domain.Models.Endpoint.Response;

namespace HCL.CS.SF.Service.Interfaces.Interfaces.Endpoint;

/// <summary>
/// Core authorization service handling the OAuth 2.0 authorization code flow lifecycle.
/// Manages authorization code persistence, return URL validation, verification codes,
/// security token cleanup, and the final authorization response generation.
/// </summary>
public interface IAuthorizationService
{
    /// <summary>Persists an authorization code and returns the code string issued to the client.</summary>
    /// <param name="authCodeRequest">The authorization code details to persist.</param>
    Task<string> SaveAuthorizationCodeAsync(AuthorizationCodeModel authCodeRequest);

    /// <summary>Deletes (consumes) an authorization code after it has been exchanged for tokens.</summary>
    /// <param name="authorizationCode">The authorization code string to delete.</param>
    Task<FrameworkResult> DeleteAuthorizationCodeAsync(string authorizationCode);

    /// <summary>Retrieves the authorization code model associated with the given code string.</summary>
    /// <param name="authorizationCode">The authorization code string.</param>
    Task<AuthorizationCodeModel> GetAuthorizationCodeAsync(string authorizationCode);

    /// <summary>Persists the return URL from a validated authorize request and returns a request identifier.</summary>
    /// <param name="authCodeRequest">The validated authorize request containing the return URL.</param>
    Task<Guid> SaveReturnUrlAsync(ValidatedAuthorizeRequestModel authCodeRequest);

    /// <summary>Validates and retrieves the stored return URL parameters for the given request ID.</summary>
    /// <param name="requestId">The request identifier issued by <see cref="SaveReturnUrlAsync"/>.</param>
    Task<Dictionary<string, string>> ValidateReturnUrlAsync(string requestId);

    /// <summary>Generates and persists a verification code for device or email verification flows.</summary>
    /// <param name="name">The name/purpose associated with the verification code.</param>
    Task<string> SaveVerificationCodeAsync(string name);

    /// <summary>Validates a verification code and returns the associated security token.</summary>
    /// <param name="tokenValue">The verification code value to validate.</param>
    Task<SecurityTokens> ValidateVerificationCodeAsync(string tokenValue);

    /// <summary>Deletes a security token by its unique identifier.</summary>
    /// <param name="id">The unique identifier of the security token.</param>
    Task<FrameworkResult> DeleteSecurityTokenByIdAsync(Guid id);

    /// <summary>Deletes a security token by its token value string.</summary>
    /// <param name="tokenValue">The token value to delete.</param>
    Task<FrameworkResult> DeleteSecurityTokenByTokenValueAsync(string tokenValue);

    /// <summary>Determines the navigation action (redirect, error, login) for the authorize request.</summary>
    /// <param name="requestValidationModel">The validated authorize request.</param>
    Task<NavigationModel> CheckNavigationAsync(ValidatedAuthorizeRequestModel requestValidationModel);

    /// <summary>Processes a validated authorize request and produces the authorization response (code, state, etc.).</summary>
    /// <param name="requestValidationModel">The validated authorize request.</param>
    Task<AuthorizationResponseModel> ProcessAuthorizationCodeAsync(
        ValidatedAuthorizeRequestModel requestValidationModel);
}
