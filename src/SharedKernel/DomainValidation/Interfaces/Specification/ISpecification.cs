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

namespace DomainValidation.Interfaces.Specification;

/// <summary>
/// Defines a single business condition that an entity of type <typeparamref name="T"/> must satisfy.
/// Implementations encapsulate a boolean predicate and are composed into validation rules via
/// <see cref="DomainValidation.Validation.Rule{TEntity}"/>.
/// </summary>
/// <typeparam name="T">The type of entity the specification evaluates.</typeparam>
public interface ISpecification<in T>
{
    /// <summary>
    /// Determines whether the given <paramref name="entity"/> satisfies this specification.
    /// </summary>
    /// <param name="entity">The entity to evaluate.</param>
    /// <returns><c>true</c> if the entity satisfies the specification; otherwise <c>false</c>.</returns>
    bool IsSatisfiedBy(T entity);
}
