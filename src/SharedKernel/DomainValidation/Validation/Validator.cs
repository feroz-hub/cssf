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

using DomainValidation.Interfaces.Validation;

namespace DomainValidation.Validation;

/// <summary>
/// Generic validator that evaluates a collection of named <see cref="IRule{TEntity}"/> instances
/// against an entity. Subclasses register rules via <see cref="Add"/> and can retrieve or remove
/// them by name. Implements a fail-fast <see cref="Validate"/> and a collect-all <see cref="ValidateAll"/> strategy.
/// </summary>
/// <typeparam name="TEntity">The type of entity to validate.</typeparam>
public class Validator<TEntity> : IValidator<TEntity> where TEntity : class
{
    /// <summary>Dictionary of registered validation rules keyed by rule name.</summary>
    private readonly Dictionary<string, IRule<TEntity>> _rules;
    // private bool validateAllRules = true;

    /// <summary>
    /// Initializes a new <see cref="Validator{TEntity}"/> with an empty rule set.
    /// </summary>
    public Validator()
    {
        _rules = new Dictionary<string, IRule<TEntity>>();
    }

    /// <summary>
    /// Validates the entity against all registered rules, returning on the first failure (fail-fast).
    /// </summary>
    /// <param name="entity">The entity to validate.</param>
    /// <returns>A <see cref="ValidationResult"/> containing at most one error if a rule failed.</returns>
    public ValidationResult Validate(TEntity entity)
    {
        var validation = new ValidationResult();
        foreach (var rule in _rules)
            if (!rule.Value.Validate(entity))
            {
                // Return immediately on the first rule violation
                validation.Add(new ValidationError(rule.Key, rule.Value.ErrorCode, rule.Value.ErrorMessage));
                return validation;
            }

        return validation;
    }

    /// <summary>
    /// Validates the entity against all registered rules, collecting every failure rather than stopping early.
    /// </summary>
    /// <param name="entity">The entity to validate.</param>
    /// <returns>A <see cref="ValidationResult"/> containing all validation errors found.</returns>
    public ValidationResult ValidateAll(TEntity entity)
    {
        var validation = new ValidationResult();
        foreach (var rule in _rules)
            if (!rule.Value.Validate(entity))
            {
                validation.Add(new ValidationError(rule.Key, rule.Value.ErrorCode, rule.Value.ErrorMessage));
                if (entity == null)
                    break;
            }

        return validation;
    }

    /// <summary>
    /// Registers a named validation rule. Subclasses call this during construction to build the rule set.
    /// </summary>
    /// <param name="name">A unique name identifying the rule.</param>
    /// <param name="rule">The rule implementation to register.</param>
    protected virtual void Add(string name, IRule<TEntity> rule)
    {
        _rules.Add(name, rule);
    }

    /// <summary>
    /// Retrieves a previously registered rule by its name.
    /// </summary>
    /// <param name="name">The name of the rule to retrieve.</param>
    /// <returns>The <see cref="IRule{TEntity}"/> registered under the specified name.</returns>
    protected IRule<TEntity> GetRule(string name)
    {
        return _rules[name];
    }

    /// <summary>
    /// Removes a previously registered rule by its name.
    /// </summary>
    /// <param name="name">The name of the rule to remove.</param>
    protected virtual void Remove(string name)
    {
        _rules.Remove(name);
    }
}
