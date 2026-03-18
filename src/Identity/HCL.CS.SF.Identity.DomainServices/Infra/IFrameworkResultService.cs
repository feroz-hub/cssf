/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Runtime.CompilerServices;
using DomainValidation.Validation;
using HCL.CS.SF.Domain;

namespace HCL.CS.SF.DomainServices.Infra;

/// <summary>
/// Factory service for creating standardized <see cref="FrameworkResult"/> and
/// <see cref="ValidationError"/> instances. Automatically captures caller information
/// (method name, file path) for diagnostic logging. All domain services use this to
/// produce consistent success/failure responses.
/// </summary>
public interface IFrameworkResultService
{
    /// <summary>Creates a successful <see cref="FrameworkResult"/>.</summary>
    FrameworkResult Succeeded();

    /// <summary>
    /// Creates a failed result of type <typeparamref name="T"/> with the specified error code.
    /// Caller information is captured automatically for logging.
    /// </summary>
    /// <typeparam name="T">The result type to return (typically <see cref="FrameworkResult"/>).</typeparam>
    /// <param name="errorCode">The resource key for the error message.</param>
    /// <param name="callerMemberName">Auto-captured caller method name.</param>
    /// <param name="sourceFilePath">Auto-captured source file path.</param>
    T Failed<T>(string errorCode, [CallerMemberName] string callerMemberName = null,
        [CallerFilePath] string sourceFilePath = null);

    /// <summary>
    /// Throws a framework exception with the specified error code. Used for unrecoverable failures.
    /// </summary>
    /// <param name="errorCode">The resource key for the error message.</param>
    /// <param name="callerMemberName">Auto-captured caller method name.</param>
    /// <param name="sourceFilePath">Auto-captured source file path.</param>
    void Throw(string errorCode, [CallerMemberName] string callerMemberName = null,
        [CallerFilePath] string sourceFilePath = null);

    /// <summary>
    /// Creates a <see cref="ValidationError"/> with both an OpenID error code and a specific error code.
    /// Used in OAuth/OIDC endpoint validation flows.
    /// </summary>
    /// <param name="openIdErrorCode">The standard OpenID Connect error code.</param>
    /// <param name="specificErrorCode">A framework-specific error code for detailed diagnostics.</param>
    /// <param name="callerMemberName">Auto-captured caller method name.</param>
    /// <param name="sourceFilePath">Auto-captured source file path.</param>
    ValidationError Failed(string openIdErrorCode, string specificErrorCode,
        [CallerMemberName] string callerMemberName = null, [CallerFilePath] string sourceFilePath = null);

    /// <summary>
    /// Creates a failed <see cref="FrameworkResult"/> from a collection of existing errors.
    /// </summary>
    /// <param name="errors">The framework errors to include.</param>
    /// <param name="callerMemberName">Auto-captured caller method name.</param>
    /// <param name="sourceFilePath">Auto-captured source file path.</param>
    FrameworkResult Failed(IEnumerable<FrameworkError> errors, [CallerMemberName] string callerMemberName = null,
        [CallerFilePath] string sourceFilePath = null);

    /// <summary>
    /// Constructs a failed <see cref="FrameworkResult"/> with an explicit error code and message.
    /// </summary>
    /// <param name="errorCode">The error code.</param>
    /// <param name="errorMessage">The human-readable error message.</param>
    /// <param name="callerMemberName">Auto-captured caller method name.</param>
    /// <param name="sourceFilePath">Auto-captured source file path.</param>
    FrameworkResult ConstructFailed(string errorCode, string errorMessage,
        [CallerMemberName] string callerMemberName = null, [CallerFilePath] string sourceFilePath = null);

    /// <summary>
    /// Throws a framework exception with a custom error message (not from resource strings).
    /// </summary>
    /// <param name="customErrorMessage">The custom error message to include in the exception.</param>
    /// <param name="callerMemberName">Auto-captured caller method name.</param>
    /// <param name="sourceFilePath">Auto-captured source file path.</param>
    void ThrowCustomMessage(string customErrorMessage, [CallerMemberName] string callerMemberName = null,
        [CallerFilePath] string sourceFilePath = null);

    /// <summary>
    /// Returns an empty/default result of type <typeparamref name="T"/> with the specified error code,
    /// used when an operation finds no data but should not throw.
    /// </summary>
    /// <typeparam name="T">The result type to return.</typeparam>
    /// <param name="errorCode">The resource key for the error message.</param>
    /// <param name="callerMemberName">Auto-captured caller method name.</param>
    /// <param name="sourceFilePath">Auto-captured source file path.</param>
    T EmptyResult<T>(string errorCode, [CallerMemberName] string callerMemberName = null,
        [CallerFilePath] string sourceFilePath = null);
}
