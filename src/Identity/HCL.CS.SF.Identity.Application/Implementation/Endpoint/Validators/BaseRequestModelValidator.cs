/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Linq.Expressions;
using DomainValidation.Interfaces.Specification;
using DomainValidation.Validation;
using HCL.CS.SF.Domain.Constants.Endpoint;
using HCL.CS.SF.Domain.Models.Endpoint.Validation;
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;

namespace HCL.CS.SF.Service.Implementation.Endpoint.Validators;

/// <summary>
/// Specification that checks whether a required request parameter is null or empty.
/// Evaluates a delegate expression against the model to verify presence of a value.
/// </summary>
/// <typeparam name="TModel">The validated request model type.</typeparam>
public class IsRequestNull<TModel> : ISpecification<TModel>
    where TModel : ValidatedBaseModel
{
    private readonly Expression<Func<TModel, object>> expression;

    /// <summary>
    /// Initializes a new instance of the <see cref="IsRequestNull"/> class.
    /// </summary>
    public IsRequestNull(Expression<Func<TModel, object>> expression)
    {
        this.expression = expression;
    }

    /// <summary>
    /// Determines whether satisfied b.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
    public bool IsSatisfiedBy(TModel entity)
    {
        var value = expression.Compile()(entity);
        var type = value.GetType();
        var returnValue = false;
        if (type == typeof(string))
            returnValue = !string.IsNullOrWhiteSpace(value as string);
        else if (type == typeof(int))
            returnValue = (int)value <= 0;
        else if (type == typeof(DateTime)) returnValue = ((DateTime)value).Equals(DateTime.MinValue);

        return returnValue;
    }
}

/// <summary>
/// Specification that validates whether a request parameter is a well-formed absolute URI.
/// Used to validate redirect_uri and post_logout_redirect_uri per OAuth 2.0 requirements.
/// </summary>
/// <typeparam name="TModel">The validated request model type.</typeparam>
public class IsRequestValidUri<TModel> : ISpecification<TModel>
    where TModel : ValidatedBaseModel
{
    private readonly Expression<Func<TModel, string>> expression;

    /// <summary>
    /// Initializes a new instance of the <see cref="IsRequestValidUri"/> class.
    /// </summary>
    public IsRequestValidUri(Expression<Func<TModel, string>> expression)
    {
        this.expression = expression;
    }

    /// <summary>
    /// Determines whether satisfied b.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
    public bool IsSatisfiedBy(TModel entity)
    {
        var value = expression.Compile()(entity);
        var type = value.GetType();
        if (type.Equals(typeof(string))) return value.IsValidUrl();

        return false;
    }
}

/// <summary>
/// Specification that validates whether all URIs in a list are well-formed absolute URLs.
/// Used for batch validation of redirect URI collections.
/// </summary>
/// <typeparam name="TModel">The model type containing the URI list.</typeparam>
public class IsRequestValidUris<TModel> : ISpecification<TModel>
{
    private readonly Expression<Func<TModel, List<string>>> expression;

    /// <summary>
    /// Initializes a new instance of the <see cref="IsRequestValidUris"/> class.
    /// </summary>
    public IsRequestValidUris(Expression<Func<TModel, List<string>>> expression)
    {
        this.expression = expression;
    }

    /// <summary>
    /// Determines whether satisfied b.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
    public bool IsSatisfiedBy(TModel entity)
    {
        var value = expression.Compile()(entity);
        var type = value.GetType();
        var returnValue = false;
        if (type != typeof(List<string>)) return false;

        foreach (var valueString in value)
        {
            returnValue = valueString.IsValidUrl();
            if (!returnValue) return false;
        }

        return returnValue;
    }
}

/// <summary>
/// Specification that validates input length restrictions.
/// Compares a source string length against a configured maximum or minimum limit.
/// Used to enforce input length limits on grant types, scopes, client IDs, etc.
/// </summary>
/// <typeparam name="TModel">The model type to validate.</typeparam>
public class CheckLengthRestrictions<TModel> : ISpecification<TModel>
{
    private readonly Expression<Func<TModel, string>> compareExpression;
    private readonly Expression<Func<TModel, string>> sourceExpression;
    private readonly Expression<Func<TModel, int>> targetExpression;

    /// <summary>
    /// Initializes a new instance of the <see cref="CheckLengthRestrictions"/> class.
    /// </summary>
    public CheckLengthRestrictions(
        Expression<Func<TModel, string>> sourceExpression,
        Expression<Func<TModel, int>> targetExpression,
        Expression<Func<TModel, string>> compareExpression)
    {
        this.sourceExpression = sourceExpression;
        this.targetExpression = targetExpression;
        this.compareExpression = compareExpression;
    }

    /// <summary>
    /// Determines whether satisfied b.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
    public bool IsSatisfiedBy(TModel entity)
    {
        var sourceValue = sourceExpression.Compile()(entity);
        var targetValue = targetExpression.Compile()(entity);
        var compareValue = compareExpression.Compile()(entity);
        var sourceType = sourceValue.GetType();
        var targetType = targetValue.GetType();
        var compareType = compareValue.GetType();
        var id = string.Empty;
        var compare = string.Empty;
        var lengthRestrictions = 0;

        if (sourceType == typeof(string)) id = sourceValue;

        if (targetType == typeof(int)) lengthRestrictions = targetValue;

        if (compareType == typeof(string)) compare = compareValue;

        return compare switch
        {
            ">" when id.Length > lengthRestrictions => false,
            ">" => true,
            "<" when id.Length < lengthRestrictions => false,
            "<" => true,
            _ => false
        };
    }
}

/// <summary>
/// Specification that verifies a client is authorized to use the requested grant type.
/// Checks the client's configured supported grant types against the requested grant type.
/// </summary>
/// <typeparam name="TModel">The validated request model type.</typeparam>
public class CheckClientAuthorizedForGrantType<TModel> : ISpecification<TModel>
    where TModel : ValidatedBaseModel
{
    private readonly List<string> grantTypes;

    /// <summary>
    /// Initializes a new instance of the <see cref="CheckClientAuthorizedForGrantType"/> class.
    /// </summary>
    public CheckClientAuthorizedForGrantType(List<string> grantTypes)
    {
        this.grantTypes = grantTypes;
    }

    /// <summary>
    /// Determines whether satisfied b.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
    public bool IsSatisfiedBy(TModel entity)
    {
        foreach (var grantType in grantTypes)
            if (entity.Client.SupportedGrantTypes.ToList().Contains(grantType))
                return true;

        return false;
    }
}

/// <summary>
/// Base validator that orchestrates specification-based validation for OAuth/OIDC request models.
/// Runs a chain of specification rules and returns the first validation error encountered.
/// </summary>
/// <typeparam name="TModel">The validated request model type.</typeparam>
public class BaseRequestModelValidator<TModel> : Validator<TModel>
    where TModel : ValidatedBaseModel
{
    /// <summary>
    /// Gets or sets the is valid.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Validates the .
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns>The operation result.</returns>
    public async Task<ValidationError> ValidateAsync(TModel model)
    {
        var validationError = new ValidationError();
        var validationResult = Validate(model);
        if (!validationResult.IsValid)
        {
            IsValid = false;
            var error = validationResult.Errors.FirstOrDefault();
            if (error == null) return validationError;

            validationError.ErrorCode = error.ErrorCode;
            validationError.ErrorMessage = error.ErrorMessage;
        }
        else
        {
            IsValid = true;
        }

        return await Task.FromResult(validationError);
    }
}

/// <summary>
/// Specification that ensures requested scopes are present in the request.
/// If no scopes are explicitly requested, falls back to the client's allowed scopes.
/// </summary>
/// <typeparam name="TModel">The validated request model type.</typeparam>
public class CheckRequestedScopes<TModel> : ISpecification<TModel>
    where TModel : ValidatedBaseModel
{
    /// <summary>
    /// Determines whether satisfied b.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
    public bool IsSatisfiedBy(TModel entity)
    {
        var scopes = entity.GetValue(OpenIdConstants.TokenRequest.Scope);
        if (string.IsNullOrWhiteSpace(scopes))
        {
            if (entity.Client.AllowedScopes.ContainsAny())
            {
                scopes = string.Join(" ", entity.Client.AllowedScopes.ToArray());
                entity.RequestRawData["scope"] = scopes;
            }
            else
            {
                return false;
            }
        }

        if (string.IsNullOrWhiteSpace(scopes)) return false;

        return true;
    }
}
