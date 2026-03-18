/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

/*
 * MIT License

 * Copyright (c) 2019 Henrique Dal Bello

 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:

 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.

 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

namespace DomainValidation.Validation;

/// <summary>
/// Aggregates zero or more <see cref="ValidationError"/> instances produced during entity validation.
/// When errors are present the result is considered invalid, and the <see cref="Message"/> property
/// is set to the error code of the first recorded error for quick access.
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Initializes a new, valid <see cref="ValidationResult"/> with no errors.
    /// </summary>
    public ValidationResult()
    {
        Errors = new ValidationError[] { };
    }

    /// <summary>
    /// Gets or sets a summary message. Automatically set to the first error code when errors exist.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Gets a value indicating whether validation passed (no errors recorded).
    /// </summary>
    public bool IsValid => !Errors.Any();

    /// <summary>
    /// Gets the collection of validation errors accumulated during validation.
    /// </summary>
    public IEnumerable<ValidationError> Errors { get; private set; }

    /// <summary>
    /// Adds a single <see cref="ValidationError"/> to the result.
    /// </summary>
    /// <param name="error">The validation error to add.</param>
    public void Add(ValidationError error)
    {
        var list = new List<ValidationError>(Errors) { error };
        SetErrors(list);
    }

    /// <summary>
    /// Merges errors from one or more other <see cref="ValidationResult"/> instances into this result.
    /// </summary>
    /// <param name="validationResults">The validation results whose errors should be merged.</param>
    public void Add(params ValidationResult[] validationResults)
    {
        var list = new List<ValidationError>(Errors);
        foreach (var validation in validationResults)
            list.AddRange(validation.Errors);

        SetErrors(list);
    }

    /// <summary>
    /// Removes a specific <see cref="ValidationError"/> from the result.
    /// </summary>
    /// <param name="error">The validation error to remove.</param>
    public void Remove(ValidationError error)
    {
        var list = new List<ValidationError>(Errors);
        list.Remove(error);
        SetErrors(list);
    }

    /// <summary>
    /// Replaces the internal error list and updates the <see cref="Message"/> to the first error code if invalid.
    /// </summary>
    /// <param name="errors">The new list of errors.</param>
    private void SetErrors(List<ValidationError> errors)
    {
        Errors = errors;

        if (!IsValid)
            Message = errors[0].ErrorCode;
    }
}
