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
using HCL.CS.SF.Domain.Constants;
using HCL.CS.SF.DomainServices.Infra;

namespace HCL.CS.SF.Infrastructure.Services.Implementation;

/// <summary>
/// Provides a centralized service for constructing standardized <see cref="FrameworkResult"/> objects
/// for success, failure, and error scenarios. All failure paths are automatically logged with caller context.
/// </summary>
internal class FrameworkResultService : IFrameworkResultService
{
    /// <summary>
    /// Cached singleton success result to avoid repeated allocations.
    /// </summary>
    private static readonly FrameworkResult Success = new() { Status = ResultStatus.Succeeded };

    /// <summary>
    /// Logger service for recording error and diagnostic information on failure paths.
    /// </summary>
    private readonly ILoggerService loggerService;

    /// <summary>
    /// Handler for resolving localized error messages from error codes.
    /// </summary>
    private readonly IResourceStringHandler resourceStringHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="FrameworkResultService"/> class.
    /// </summary>
    /// <param name="instance">Logger instance registry for obtaining the default logger.</param>
    /// <param name="resourceStringHandler">Handler for resolving error code descriptions.</param>
    public FrameworkResultService(ILoggerInstance instance, IResourceStringHandler resourceStringHandler)
    {
        loggerService = instance.GetLoggerInstance(LoggerKeyConstants.DefaultLoggerKey);
        this.resourceStringHandler = resourceStringHandler;
    }

    /// <summary>
    /// Returns a cached success result instance.
    /// </summary>
    /// <returns>A <see cref="FrameworkResult"/> with <see cref="ResultStatus.Succeeded"/> status.</returns>
    public FrameworkResult Succeeded()
    {
        return Success;
    }

    /// <summary>
    /// Constructs a typed failure result from an error code, resolving the error message from resources
    /// and logging the failure with caller context.
    /// </summary>
    /// <typeparam name="T">The result type (typically <see cref="FrameworkResult"/>).</typeparam>
    /// <param name="errorCode">The error code to resolve into a human-readable message.</param>
    /// <param name="callerMemberName">Automatically captured caller method name.</param>
    /// <param name="sourceFilePath">Automatically captured source file path.</param>
    /// <returns>A failure result of type <typeparamref name="T"/>, or null if the error code is empty.</returns>
    public T Failed<T>(string errorCode, [CallerMemberName] string callerMemberName = null,
        [CallerFilePath] string sourceFilePath = null)
    {
        if (!string.IsNullOrWhiteSpace(errorCode))
        {
            var errorMessage = ResolveErrorMessage(errorCode, callerMemberName, sourceFilePath);
            loggerService.WriteToWithCaller(Log.Error, errorCode + " : " + errorMessage, null, callerMemberName,
                sourceFilePath);
            if (typeof(FrameworkResult) == typeof(T))
            {
                var result = new FrameworkResult
                {
                    Status = ResultStatus.Failed,
                    Errors = new[]
                    {
                        new FrameworkError
                        {
                            Code = errorCode,
                            Description = errorMessage
                        }
                    }
                };

                // Returning FrameworkResult as return type
                return (T)Convert.ChangeType(result, typeof(T));
            }
        }
        else
        {
            loggerService.WriteToWithCaller(Log.Error, "Error code not specified.", null, callerMemberName,
                sourceFilePath);
        }

        // Returning T as return type
        return (T)Convert.ChangeType(null, typeof(T));
    }

    /// <summary>
    /// Resolves an error code to its message, logs the failure, and throws an exception.
    /// </summary>
    /// <param name="errorCode">The error code to resolve and throw.</param>
    /// <param name="callerMemberName">Automatically captured caller method name.</param>
    /// <param name="sourceFilePath">Automatically captured source file path.</param>
    /// <exception cref="Exception">Always thrown with the resolved error message.</exception>
    public void Throw(string errorCode, [CallerMemberName] string callerMemberName = null,
        [CallerFilePath] string sourceFilePath = null)
    {
        if (!string.IsNullOrWhiteSpace(errorCode))
        {
            var errorMessage = ResolveErrorMessage(errorCode, callerMemberName, sourceFilePath);
            loggerService.WriteToWithCaller(Log.Error, errorCode + " : " + errorMessage, null, callerMemberName,
                sourceFilePath);
            throw new Exception(errorMessage);
        }

        throw new Exception("Error code not specified.");
    }

    /// <summary>
    /// Constructs a <see cref="ValidationError"/> for OpenID Connect endpoint scenarios where both
    /// an OpenID error code and a specific internal error code are needed.
    /// </summary>
    /// <param name="openIdErrorCode">The standard OpenID Connect error code to return to the client.</param>
    /// <param name="specificErrorCode">The internal error code used to resolve the descriptive message.</param>
    /// <param name="callerMemberName">Automatically captured caller method name.</param>
    /// <param name="sourceFilePath">Automatically captured source file path.</param>
    /// <returns>A <see cref="ValidationError"/> with the OpenID error code and resolved description.</returns>
    public ValidationError Failed(string openIdErrorCode, string specificErrorCode,
        [CallerMemberName] string callerMemberName = null, [CallerFilePath] string sourceFilePath = null)
    {
        // Getting two error codes for endpoint related scenarios, Need to pass orginal OpenId error codes as result.
        if (!string.IsNullOrWhiteSpace(specificErrorCode))
        {
            var errorMessage = ResolveErrorMessage(specificErrorCode, callerMemberName, sourceFilePath);
            loggerService.WriteToWithCaller(Log.Error, openIdErrorCode + " : " + errorMessage, null, callerMemberName,
                sourceFilePath);
            return new ValidationError
            {
                ErrorCode = openIdErrorCode,
                ErrorMessage = errorMessage
            };
        }

        ThrowCustomMessage("Error code not specified.");
        return null;
    }

    /// <summary>
    /// Constructs a failure result from a collection of <see cref="FrameworkError"/> objects,
    /// logging the concatenated error descriptions.
    /// </summary>
    /// <param name="errors">The collection of framework errors to include in the result.</param>
    /// <param name="callerMemberName">Automatically captured caller method name.</param>
    /// <param name="sourceFilePath">Automatically captured source file path.</param>
    /// <returns>A <see cref="FrameworkResult"/> with failed status and the provided errors.</returns>
    public FrameworkResult Failed(IEnumerable<FrameworkError> errors, [CallerMemberName] string callerMemberName = null,
        [CallerFilePath] string sourceFilePath = null)
    {
        if (errors.Any())
        {
            var errorMessage = string.Join(", ", errors.Select(error => error.Description));
            loggerService.WriteToWithCaller(Log.Error, errorMessage, null, callerMemberName, sourceFilePath);
            return new FrameworkResult
            {
                Status = ResultStatus.Failed,
                Errors = errors
            };
        }

        ThrowCustomMessage("Error code not specified.");
        return null;
    }

    /// <summary>
    /// Constructs a failure result from an explicit error code and message without resource resolution.
    /// Useful when the error message is already known or dynamically constructed.
    /// </summary>
    /// <param name="errorCode">The error code identifier.</param>
    /// <param name="errorMessage">The pre-resolved error description.</param>
    /// <param name="callerMemberName">Automatically captured caller method name.</param>
    /// <param name="sourceFilePath">Automatically captured source file path.</param>
    /// <returns>A <see cref="FrameworkResult"/> with failed status and the provided error.</returns>
    public FrameworkResult ConstructFailed(string errorCode, string errorMessage,
        [CallerMemberName] string callerMemberName = null, [CallerFilePath] string sourceFilePath = null)
    {
        if (!string.IsNullOrWhiteSpace(errorCode) && !string.IsNullOrWhiteSpace(errorMessage))
        {
            loggerService.WriteToWithCaller(Log.Error, errorCode + " : " + errorMessage, null, callerMemberName,
                sourceFilePath);
            var result = new FrameworkResult
            {
                Status = ResultStatus.Failed,
                Errors = new[]
                {
                    new FrameworkError
                    {
                        Code = errorCode,
                        Description = errorMessage
                    }
                }
            };

            return result;
        }

        ThrowCustomMessage("Error code not specified.");
        return null;
    }

    /// <summary>
    /// Logs a custom error message with caller context and throws an exception.
    /// </summary>
    /// <param name="customErrorMessage">The custom error message to log and throw.</param>
    /// <param name="callerMemberName">Automatically captured caller method name.</param>
    /// <param name="sourceFilePath">Automatically captured source file path.</param>
    /// <exception cref="Exception">Always thrown with the provided custom message.</exception>
    public void ThrowCustomMessage(string customErrorMessage, [CallerMemberName] string callerMemberName = null,
        [CallerFilePath] string sourceFilePath = null)
    {
        if (!string.IsNullOrWhiteSpace(customErrorMessage))
        {
            loggerService.WriteToWithCaller(Log.Error, customErrorMessage, null, callerMemberName, sourceFilePath);
            throw new Exception(customErrorMessage);
        }

        throw new Exception("Custom error message not specified.");
    }

    /// <summary>
    /// Returns a null/default result for the specified type, logging the error code at Debug level.
    /// Used for operations that should return empty rather than throw on expected conditions.
    /// </summary>
    /// <typeparam name="T">The expected return type.</typeparam>
    /// <param name="errorCode">The error code to log at debug level.</param>
    /// <param name="callerMemberName">Automatically captured caller method name.</param>
    /// <param name="sourceFilePath">Automatically captured source file path.</param>
    /// <returns>A null value cast to <typeparamref name="T"/>.</returns>
    public T EmptyResult<T>(string errorCode, [CallerMemberName] string callerMemberName = null,
        [CallerFilePath] string sourceFilePath = null)
    {
        if (!string.IsNullOrWhiteSpace(errorCode))
        {
            var errorMessage = ResolveErrorMessage(errorCode, callerMemberName, sourceFilePath);
            loggerService.WriteToWithCaller(Log.Debug, errorCode + " : " + errorMessage, null, callerMemberName,
                sourceFilePath);
        }

        return (T)Convert.ChangeType(null, typeof(T));
    }

    /// <summary>
    /// Resolves an error code to a human-readable message using the resource string handler.
    /// Falls back to returning the raw error code if no resource string is found.
    /// </summary>
    /// <param name="errorCode">The error code to resolve.</param>
    /// <param name="callerMemberName">Caller method name for warning-level logging on missing keys.</param>
    /// <param name="sourceFilePath">Source file path for warning-level logging on missing keys.</param>
    /// <returns>The resolved error message, or the error code itself as a fallback.</returns>
    private string ResolveErrorMessage(
        string errorCode,
        string callerMemberName,
        string sourceFilePath)
    {
        var errorMessage = resourceStringHandler.GetResourceString(errorCode, true);
        if (!string.IsNullOrWhiteSpace(errorMessage)) return errorMessage;

        loggerService.WriteToWithCaller(
            Log.Warning,
            $"Missing validation message key for error code '{errorCode}'. Falling back to error code.",
            null,
            callerMemberName,
            sourceFilePath);
        return errorCode;
    }
}
