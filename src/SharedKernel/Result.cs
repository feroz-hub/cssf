/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.SharedKernel;

/// <summary>
/// Represents the outcome of an operation that can either succeed or fail with an error message.
/// Instances are created via the <see cref="Success"/> and <see cref="Failure"/> factory methods
/// to enforce a valid state (success has no error; failure always carries an error message).
/// </summary>
public sealed class Result
{
    /// <summary>
    /// Initializes a new <see cref="Result"/> with the specified success state and optional error.
    /// </summary>
    /// <param name="isSuccess">Whether the operation succeeded.</param>
    /// <param name="error">The error message when the operation failed; <c>null</c> on success.</param>
    private Result(bool isSuccess, string? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// Gets a value indicating whether the operation completed successfully.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed. Inverse of <see cref="IsSuccess"/>.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the error message associated with a failed result, or <c>null</c> if the result is successful.
    /// </summary>
    public string? Error { get; }

    /// <summary>
    /// Creates a successful <see cref="Result"/> with no error.
    /// </summary>
    public static Result Success() => new(true, null);

    /// <summary>
    /// Creates a failed <see cref="Result"/> with the specified error message.
    /// </summary>
    /// <param name="error">A non-null message describing the failure.</param>
    public static Result Failure(string error) => new(false, error);
}
