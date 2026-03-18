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
using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Configurations.Endpoint;
using HCL.CS.SF.Domain.Constants;
using HCL.CS.SF.Domain.Constants.Endpoint;
using HCL.CS.SF.Service.Extension;
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;

namespace HCL.CS.SF.Service.Implementation.Api.Validators;

/// <summary>
/// Validates that an OAuth2 token expiration value falls within the allowed range
/// for the specific token type (access token, identity token, refresh token, authorization code, logout token).
/// Token lifetime bounds are defined in the server's TokenExpiration configuration.
/// </summary>
/// <typeparam name="TModel">The model type being validated.</typeparam>

public class CheckTokenExpirationRange<TModel>(
    Expression<Func<TModel, object>> expression,
    string tokenType,
    TokenExpiration tokenExpiration)
    : ISpecification<TModel>
    where TModel : BaseTrailModel
{
    /// <summary>
    /// Determines whether satisfied b.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
    public bool IsSatisfiedBy(TModel? entity)
    {
        if (entity == null) return false;

        var value = expression.Compile()(entity);
        if (value == null) return false;
        var type = value.GetType();
        if (!(type == typeof(int))) return false;
        var tokenValue = (int)value;

        return tokenType switch
        {
            OpenIdConstants.TokenType.AccessToken => tokenValue >= tokenExpiration.MinAccessTokenExpiration &&
                                                     tokenValue <= tokenExpiration.MaxAccessTokenExpiration,
            OpenIdConstants.TokenType.IdentityToken => tokenValue >= tokenExpiration.MinIdentityTokenExpiration &&
                                                       tokenValue <= tokenExpiration.MaxIdentityTokenExpiration,
            OpenIdConstants.TokenType.RefreshToken => tokenValue >= tokenExpiration.MinRefreshTokenExpiration &&
                                                      tokenValue <= tokenExpiration.MaxRefreshTokenExpiration,
            OpenIdConstants.TokenType.AuthorizationCode =>
                tokenValue >= tokenExpiration.MinAuthorizationCodeExpiration &&
                tokenValue <= tokenExpiration.MaxAuthorizationCodeExpiration,
            OpenIdConstants.TokenType.LogoutToken => tokenValue >= tokenExpiration.MinLogoutTokenExpiration &&
                                                     tokenValue <= tokenExpiration.MaxLogoutTokenExpiration,
            _ => false
        };
    }
}

/// <summary>
/// Generic null/empty check specification that validates a single property is not null,
/// not empty (for strings), not zero (for integers), not MinValue (for DateTime), and not Empty (for Guid).
/// </summary>
/// <typeparam name="TModel">The model type being validated.</typeparam>

public class IsNotNull<TModel>(Expression<Func<TModel, object>> expression) : ISpecification<TModel>
    where TModel : BaseTrailModel
{
    /// <summary>
    /// Determines whether satisfied b.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns><c>true</c> if the operation succeeds; otherwise, <c>false</c>.</returns>
    public bool IsSatisfiedBy(TModel? entity)
    {
        if (entity == null) return false;

        var value = expression.Compile()(entity);
        if (value == null) return false;
        var type = value.GetType();
        if (type == typeof(string)) return !string.IsNullOrWhiteSpace(value as string);

        if (type == typeof(int)) return (int)value > 0;

        if (type == typeof(DateTime)) return !((DateTime)value).Equals(DateTime.MinValue);

        if (type == typeof(Guid)) return !((Guid)value).Equals(Guid.Empty);

        return value != null;
    }
}

/// <summary>
/// Validates that a list of strings contains no null or whitespace entries.
/// </summary>
/// <typeparam name="TModel">The model type being validated.</typeparam>

public class AreNotNull<TModel> : ISpecification<TModel>
    where TModel : BaseTrailModel
{
    private readonly Expression<Func<TModel, List<string>>> expression;

    /// <summary>
    /// Initializes a new instance of the <see cref="AreNotNull"/> class.
    /// </summary>
    public AreNotNull(Expression<Func<TModel, List<string>>> expression)
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
        if (value == null) return false;

        var type = value.GetType();
        if (type.Equals(typeof(List<string>)))
            foreach (var valueString in value)
                if (string.IsNullOrWhiteSpace(valueString))
                    return false;

        return true;
    }
}

/// <summary>
/// Validates that an identifier (Guid or int) is a valid non-default value.
/// </summary>
/// <typeparam name="TModel">The model type being validated.</typeparam>

public class IsValidIdentifier<TModel> : ISpecification<TModel>
    where TModel : BaseTrailModel
{
    private readonly Expression<Func<TModel, object>> expression;

    /// <summary>
    /// Initializes a new instance of the <see cref="IsValidIdentifier"/> class.
    /// </summary>
    public IsValidIdentifier(Expression<Func<TModel, object>> expression)
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
        if (entity == null) return false;

        var value = expression.Compile()(entity);
        if (value != null)
        {
            var type = value.GetType();
            if (type.Equals(typeof(Guid))) return ((Guid)value).IsValid();

            if (type.Equals(typeof(int))) return (int)value > 0;
        }

        return value != null;
    }
}

/// <summary>
/// Validates that all Guid identifiers in a list are valid (non-empty).
/// </summary>
/// <typeparam name="TModel">The model type being validated.</typeparam>

public class IsValidIdentifierExists<TModel> : ISpecification<TModel>
    where TModel : BaseTrailModel
{
    private readonly Expression<Func<TModel, List<Guid>>> expression;

    /// <summary>
    /// Initializes a new instance of the <see cref="IsValidIdentifierExists"/> class.
    /// </summary>
    public IsValidIdentifierExists(Expression<Func<TModel, List<Guid>>> expression)
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
        if (value.ContainsAny())
        {
            var type = value.GetType();
            if (type.Equals(typeof(List<Guid>)) && value.ContainsAny())
                foreach (var valueString in value)
                    if (!valueString.IsValid())
                        return false;
        }

        return true;
    }
}

/// <summary>
/// Validates that all URIs in a list are well-formed URLs.
/// </summary>
/// <typeparam name="TModel">The model type being validated.</typeparam>

public class IsValidUris<TModel> : ISpecification<TModel>
    where TModel : BaseTrailModel
{
    private readonly Expression<Func<TModel, List<string>>> expression;

    /// <summary>
    /// Initializes a new instance of the <see cref="IsValidUris"/> class.
    /// </summary>
    public IsValidUris(Expression<Func<TModel, List<string>>> expression)
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
        if (value.ContainsAny())
        {
            var type = value.GetType();
            if (type.Equals(typeof(List<string>)))
                foreach (var valueString in value)
                    if (!valueString.IsValidUrl())
                        return false;
        }

        return true;
    }
}

/// <summary>
/// Validates that a single URI string is a well-formed URL.
/// </summary>
/// <typeparam name="TModel">The model type being validated.</typeparam>

public class IsValidUri<TModel> : ISpecification<TModel>
    where TModel : BaseTrailModel
{
    private readonly Expression<Func<TModel, string>> expression;

    /// <summary>
    /// Initializes a new instance of the <see cref="IsValidUri"/> class.
    /// </summary>
    public IsValidUri(Expression<Func<TModel, string>> expression)
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
        if (!string.IsNullOrWhiteSpace(value))
        {
            var type = value.GetType();
            if (type.Equals(typeof(string))) return value.IsValidUrl();
        }

        return false;
    }
}

/// <summary>
/// Validates that a string property does not exceed 255 characters (standard database column limit).
/// </summary>
/// <typeparam name="TModel">The model type being validated.</typeparam>

public class IsValid255CharLength<TModel> : ISpecification<TModel>
    where TModel : BaseTrailModel
{
    private readonly Expression<Func<TModel, string>> expression;

    /// <summary>
    /// Initializes a new instance of the <see cref="IsValid255CharLength"/> class.
    /// </summary>
    public IsValid255CharLength(Expression<Func<TModel, string>> expression)
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
        if (!string.IsNullOrWhiteSpace(value))
        {
            var type = value.GetType();
            if (type.Equals(typeof(string)) && value.Length > Constants.ColumnLength255) return false;
        }

        return true;
    }
}

/// <summary>
/// Validates that all strings in a list do not exceed 255 characters.
/// </summary>
/// <typeparam name="TModel">The model type being validated.</typeparam>

public class IsValid255CharLengths<TModel> : ISpecification<TModel>
    where TModel : BaseTrailModel
{
    private readonly Expression<Func<TModel, List<string>>> expression;

    /// <summary>
    /// Initializes a new instance of the <see cref="IsValid255CharLengths"/> class.
    /// </summary>
    public IsValid255CharLengths(Expression<Func<TModel, List<string>>> expression)
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
        if (value.ContainsAny())
        {
            var type = value.GetType();
            if (type.Equals(typeof(List<string>)))
                foreach (var valueString in value)
                    if (!string.IsNullOrWhiteSpace(valueString) && valueString.Length > Constants.ColumnLength255)
                        return false;
        }

        return true;
    }
}

/// <summary>
/// Validates that all strings in a list do not exceed 2048 characters (used for URI fields).
/// </summary>
/// <typeparam name="TModel">The model type being validated.</typeparam>

public class IsValid2048CharLengths<TModel> : ISpecification<TModel>
    where TModel : BaseTrailModel
{
    private readonly Expression<Func<TModel, List<string>>> expression;

    /// <summary>
    /// Initializes a new instance of the <see cref="IsValid2048CharLengths"/> class.
    /// </summary>
    public IsValid2048CharLengths(Expression<Func<TModel, List<string>>> expression)
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
        if (value.ContainsAny())
        {
            var type = value.GetType();
            if (type.Equals(typeof(List<string>)))
                foreach (var valueString in value)
                    if (!string.IsNullOrWhiteSpace(valueString) && valueString.Length > Constants.ColumnLength2048)
                        return false;
        }

        return true;
    }
}

/// <summary>
/// Base domain model validator that provides async validation with error collection.
/// Serves as the foundation for all specification-based validators in the identity service,
/// implementing the Specification pattern for composable business rule validation.
/// </summary>
/// <typeparam name="TModel">The domain model type to validate.</typeparam>

public class BaseDomainModelValidator<TModel> : Validator<TModel>
    where TModel : BaseTrailModel
{
    /// <summary>Gets or sets whether the last validation passed all rules.</summary>

    public bool IsValid { get; set; }

    /// <summary>
    /// Validates a single model against all configured rules, returning the first error found.
    /// </summary>
    /// <param name="model">The model to validate.</param>
    /// <returns>A validation error containing the error code and message, or empty if valid.</returns>

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

    /// <summary>
    /// Validates a collection of models against all configured rules, collecting all errors.
    /// </summary>
    /// <param name="models">The models to validate.</param>
    /// <returns>A list of framework errors for all validation failures.</returns>

    public async Task<List<FrameworkError>> ValidateAsync(IList<TModel> models)
    {
        var validationErrors = new List<FrameworkError>();
        foreach (var model in models)
        {
            var validationResult = ValidateAll(model);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors;
                if (!errors.ContainsAny()) return validationErrors;

                foreach (var error in errors)
                {
                    var validationError = new FrameworkError
                    {
                        Code = error.ErrorCode,
                        Description = error.ErrorMessage
                    };
                    validationErrors.Add(validationError);
                }
            }
        }

        IsValid = !validationErrors.ContainsAny();
        return await Task.FromResult(validationErrors);
    }
}

/// <summary>
/// Validates that a signing algorithm is one of the allowed values (RS256, ES256).
/// Used during OAuth2 client registration to ensure only supported JWT signing algorithms are configured.
/// </summary>
/// <typeparam name="TModel">The model type being validated.</typeparam>

public class CheckAlgorithm<TModel> : ISpecification<TModel>
    where TModel : BaseTrailModel
{
    private readonly Expression<Func<TModel, object>> expression;

    /// <summary>
    /// Initializes a new instance of the <see cref="CheckAlgorithm"/> class.
    /// </summary>
    public CheckAlgorithm(Expression<Func<TModel, object>> expression)
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
        if (type.Equals(typeof(string)))
        {
            if (string.IsNullOrWhiteSpace(value.ToString())) return false;

            var allowedSigningAlgorithms = new[]
            {
                OpenIdConstants.Algorithms.RsaSha256,
                OpenIdConstants.Algorithms.EcdsaSha256
            };
            if (!allowedSigningAlgorithms.Contains(value.ToString(), StringComparer.Ordinal)) return false;
        }

        return true;
    }
}
