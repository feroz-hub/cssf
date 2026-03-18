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
/// Represents a single validation error produced when a domain rule is not satisfied.
/// Contains the rule name, a machine-readable error code, and a human-readable message.
/// </summary>
public class ValidationError
{
    /// <summary>
    /// Initializes a new <see cref="ValidationError"/> with the specified rule name, error code, and message.
    /// </summary>
    /// <param name="name">The name of the validation rule that failed.</param>
    /// <param name="errorCode">A machine-readable code identifying the type of validation failure.</param>
    /// <param name="errorMessage">A human-readable description of the validation failure.</param>
    public ValidationError(string name, string errorCode, string errorMessage)
    {
        Name = name;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }


    /// <summary>
    /// Initializes a new empty <see cref="ValidationError"/>.
    /// </summary>
    public ValidationError()
    {
    }

    /// <summary>
    /// Gets or sets the name of the validation rule that produced this error.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the machine-readable error code for this validation failure.
    /// </summary>
    public string ErrorCode { get; set; }

    /// <summary>
    /// Gets or sets the human-readable description of the validation failure.
    /// </summary>
    public string ErrorMessage { get; set; }
}
