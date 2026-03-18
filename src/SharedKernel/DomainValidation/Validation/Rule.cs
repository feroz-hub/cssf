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

using DomainValidation.Interfaces.Specification;
using DomainValidation.Interfaces.Validation;

namespace DomainValidation.Validation;

/// <summary>
/// A concrete validation rule that delegates its check to an <see cref="ISpecification{T}"/>.
/// When the specification is not satisfied, the rule exposes an error code and optional message
/// for inclusion in a <see cref="ValidationResult"/>.
/// </summary>
/// <typeparam name="TEntity">The type of entity this rule validates.</typeparam>
public class Rule<TEntity> : IRule<TEntity>
{
    /// <summary>The specification that defines the business condition this rule enforces.</summary>
    private readonly ISpecification<TEntity> _specification;

    /// <summary>
    /// Initializes a new rule with a specification, error code, and descriptive error message.
    /// </summary>
    /// <param name="spec">The specification that defines the condition to evaluate.</param>
    /// <param name="errorCode">A machine-readable code identifying the rule violation.</param>
    /// <param name="errorMessage">A human-readable description of the rule violation.</param>
    public Rule(ISpecification<TEntity> spec, string errorCode, string errorMessage)
    {
        _specification = spec;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }

    /// <summary>
    /// Initializes a new rule with a specification and error code, using an empty error message.
    /// </summary>
    /// <param name="spec">The specification that defines the condition to evaluate.</param>
    /// <param name="errorCode">A machine-readable code identifying the rule violation.</param>
    public Rule(ISpecification<TEntity> spec, string errorCode)
    {
        _specification = spec;
        ErrorCode = errorCode;
        ErrorMessage = string.Empty;
    }

    /// <summary>
    /// Gets the machine-readable error code reported when this rule is violated.
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// Gets the human-readable error message reported when this rule is violated.
    /// </summary>
    public string ErrorMessage { get; }

    /// <summary>
    /// Evaluates the underlying specification against the given entity.
    /// </summary>
    /// <param name="entity">The entity to validate.</param>
    /// <returns><c>true</c> if the entity satisfies the specification; otherwise <c>false</c>.</returns>
    public bool Validate(TEntity entity)
    {
        return _specification.IsSatisfiedBy(entity);
    }
}
