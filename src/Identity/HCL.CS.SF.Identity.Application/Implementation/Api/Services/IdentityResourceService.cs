/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using AutoMapper;
using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Constants;
using HCL.CS.SF.Domain.Entities.Api;
using HCL.CS.SF.Domain.ErrorCodes;
using HCL.CS.SF.Domain.Models.Api;
using HCL.CS.SF.DomainServices;
using HCL.CS.SF.DomainServices.Infra;
using HCL.CS.SF.DomainServices.Repository.Api;
using HCL.CS.SF.Service.Extension;
using HCL.CS.SF.Service.Implementation.Api.Specifications;
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;
using HCL.CS.SF.Service.Interfaces.Interfaces.Api;

namespace HCL.CS.SF.Service.Implementation.Api.Services;

/// <summary>
/// Service for managing OIDC identity resources and their claims. Identity resources represent
/// standard OIDC scopes (openid, profile, email, address, phone) and custom scopes that map
/// to sets of user claims included in ID tokens. Each identity resource can have multiple
/// associated claim types that define which user attributes are returned when the scope is requested.
/// </summary>

public class IdentityResourceService(
    ILoggerInstance instance,
    IMapper mapper,
    IFrameworkResultService frameworkResult,
    IIdentityResourceRepository identityResourceRepository,
    IRepository<IdentityClaims> identityClaimRepository)
    : SecurityBase, IIdentityResourceService
{
    private readonly ILoggerService loggerService = instance.GetLoggerInstance(LoggerKeyConstants.DefaultLoggerKey);

    /// <summary>
    /// Creates a new identity resource after validating the model against specification rules.
    /// </summary>
    /// <param name="identityResourceModel">The identity resource model to create.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating success or the validation error.</returns>
    public virtual async Task<FrameworkResult> AddIdentityResourceAsync(IdentityResourcesModel identityResourceModel)
    {
        if (identityResourceModel == null)
            return frameworkResult.Failed<FrameworkResult>(ApiErrorCodes.IdentityResourceIsNull);

        try
        {
            var identityResourceModelValidation =
                new IdentityResourceModelSpecification(CrudMode.Add, identityResourceRepository);
            var validationError = await identityResourceModelValidation.ValidateAsync(identityResourceModel);
            if (identityResourceModelValidation.IsValid)
            {
                loggerService.WriteTo(Log.Debug, "Entered into add identity resource :" + identityResourceModel.Name);
                var identityModelEntity =
                    mapper.Map<IdentityResourcesModel, IdentityResources>(identityResourceModel);

                await identityResourceRepository.InsertAsync(identityModelEntity);
                return await identityResourceRepository.SaveChangesAsync();
            }

            return frameworkResult.Failed<FrameworkResult>(validationError.ErrorCode);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Failed to add Identity resources.");
            throw;
        }
    }

    /// <summary>
    /// Updates an existing identity resource after validation.
    /// </summary>
    /// <param name="identityResourceModel">The identity resource model with updated values.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating success or the validation error.</returns>
    public virtual async Task<FrameworkResult> UpdateIdentityResourceAsync(IdentityResourcesModel identityResourceModel)
    {
        if (identityResourceModel == null)
            return frameworkResult.Failed<FrameworkResult>(ApiErrorCodes.IdentityResourceIsNull);

        try
        {
            var identityResourceModelValidation =
                new IdentityResourceModelSpecification(CrudMode.Update, identityResourceRepository);
            var validationError = await identityResourceModelValidation.ValidateAsync(identityResourceModel);
            if (identityResourceModelValidation.IsValid)
            {
                var identityResourceEntity =
                    await identityResourceRepository.GetIdentityResourcesAsync(identityResourceModel.Id);
                if (identityResourceEntity != null)
                {
                    identityResourceEntity = mapper.Map(identityResourceModel, identityResourceEntity);
                    loggerService.WriteTo(Log.Debug,
                        "Entered into update identity resource :" + identityResourceEntity.Name);
                    await identityResourceRepository.UpdateAsync(identityResourceEntity);
                    return await identityResourceRepository.SaveChangesAsync();
                }

                return frameworkResult.Failed<FrameworkResult>(ApiErrorCodes.IdentityResourceIdInvalid);
            }

            return frameworkResult.Failed<FrameworkResult>(validationError.ErrorCode);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Failed to update Identity resources.");
            throw;
        }
    }

    /// <summary>
    /// Deletes an identity resource by its unique identifier.
    /// </summary>
    /// <param name="identityResourceId">The unique identifier of the identity resource to delete.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating success or failure.</returns>
    public virtual async Task<FrameworkResult> DeleteIdentityResourceAsync(Guid identityResourceId)
    {
        try
        {
            var identityResource = await identityResourceRepository.GetIdentityResourcesAsync(identityResourceId);
            if (identityResource != null)
            {
                loggerService.WriteTo(Log.Debug, "Entered into remove identity resource :" + identityResource.Name);
                await identityResourceRepository.DeleteAsync(identityResource);
                return await identityResourceRepository.SaveChangesAsync();
            }

            return frameworkResult.Failed<FrameworkResult>(ApiErrorCodes.IdentityResourceIdInvalid);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Failed to delete Identity resources using Id.");
            throw;
        }
    }

    /// <summary>
    /// Deletes an identity resource by its name.
    /// </summary>
    /// <param name="identityResourceName">The name of the identity resource to delete.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating success or failure.</returns>
    public virtual async Task<FrameworkResult> DeleteIdentityResourceAsync(string identityResourceName)
    {
        try
        {
            var identityResource = await identityResourceRepository.GetIdentityResourcesAsync(identityResourceName);
            if (identityResource != null)
            {
                loggerService.WriteTo(Log.Debug, "Entered into remove identity resource :" + identityResource.Name);
                await identityResourceRepository.DeleteAsync(identityResource);
                return await identityResourceRepository.SaveChangesAsync();
            }

            return frameworkResult.Failed<FrameworkResult>(ApiErrorCodes.IdentityResourceNameRequired);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Failed to delete Identity resources using Id.");
            throw;
        }
    }

    /// <summary>
    /// Retrieves a single identity resource by its unique identifier.
    /// </summary>
    /// <param name="identityResourceId">The unique identifier of the identity resource.</param>
    /// <returns>The matching identity resource model, or an empty result if not found.</returns>
    public virtual async Task<IdentityResourcesModel> GetIdentityResourceAsync(Guid identityResourceId)
    {
        try
        {
            var identityResource = await identityResourceRepository.GetIdentityResourcesAsync(identityResourceId);
            if (identityResource != null)
            {
                loggerService.WriteTo(Log.Debug, "Entered into get identity resource :" + identityResourceId);
                return mapper.Map<IdentityResources, IdentityResourcesModel>(identityResource);
            }

            return frameworkResult.EmptyResult<IdentityResourcesModel>(ApiErrorCodes.NoRecordsFound);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Failed to retrieve Identity resources using Id.");
            throw;
        }
    }

    /// <summary>
    /// Retrieves a single identity resource by its name.
    /// </summary>
    /// <param name="identityResourceName">The name of the identity resource.</param>
    /// <returns>The matching identity resource model, or an empty result if not found.</returns>
    public virtual async Task<IdentityResourcesModel> GetIdentityResourceAsync(string identityResourceName)
    {
        try
        {
            var identityResource = await identityResourceRepository.GetIdentityResourcesAsync(identityResourceName);
            if (identityResource != null)
            {
                loggerService.WriteTo(Log.Debug, "Entered into get identity resource :" + identityResourceName);
                return mapper.Map<IdentityResources, IdentityResourcesModel>(identityResource);
            }

            return frameworkResult.EmptyResult<IdentityResourcesModel>(ApiErrorCodes.NoRecordsFound);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Failed to retrieve Identity resources using Name.");
            throw;
        }
    }

    /// <summary>
    /// Retrieves all registered identity resources with their associated claims.
    /// </summary>
    /// <returns>A list of all identity resource models, or an empty result if none exist.</returns>
    public virtual async Task<IList<IdentityResourcesModel>> GetAllIdentityResourcesAsync()
    {
        try
        {
            var identityResourcesEntity = await identityResourceRepository.GetAllAsync(new System.Linq.Expressions.Expression<Func<IdentityResources, object>>[] { x => x.IdentityClaims });
            if (identityResourcesEntity.ContainsAny())
            {
                loggerService.WriteTo(Log.Debug,
                    "Entered into get all identity resources - Count :" + identityResourcesEntity.Count);
                return mapper.Map<List<IdentityResources>, List<IdentityResourcesModel>>(
                    identityResourcesEntity.ToList());
            }

            return frameworkResult.EmptyResult<IList<IdentityResourcesModel>>(ApiErrorCodes.NoRecordsFound);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Failed to retrieve all Identity resources.");
            throw;
        }
    }

    /// <summary>
    /// Retrieves identity resources filtered by the requested scope names.
    /// </summary>
    /// <param name="requestedScopes">The scope names to filter by.</param>
    /// <returns>Identity resources matching the requested scopes.</returns>
    public virtual async Task<IList<IdentityResourcesByScopesModel>> GetAllIdentityResourcesByScopesAsync(
        IList<string> requestedScopes)
    {
        if (!requestedScopes.ContainsAny()) frameworkResult.Throw(EndpointErrorCodes.InvalidScopeOrNotAllowed);

        try
        {
            var identityResourcesEntity =
                await identityResourceRepository.GetAllIdentityResourcesByScopesAsync(requestedScopes);
            if (identityResourcesEntity.ContainsAny())
            {
                loggerService.WriteTo(Log.Debug,
                    "Entered into get identity resources by scopes - Count :" + identityResourcesEntity.Count);
                return identityResourcesEntity.ToList();
            }

            return frameworkResult.EmptyResult<IList<IdentityResourcesByScopesModel>>(ApiErrorCodes.NoRecordsFound);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Failed to retrieve all Identity resources.");
            throw;
        }
    }

    /// <summary>
    /// Adds a claim type to an identity resource.
    /// </summary>
    /// <param name="identityClaimsModel">The claim to associate with the identity resource.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating success or the validation error.</returns>
    public virtual async Task<FrameworkResult> AddIdentityResourceClaimAsync(IdentityClaimsModel identityClaimsModel)
    {
        if (identityClaimsModel == null)
            return frameworkResult.Failed<FrameworkResult>(ApiErrorCodes.IdentityClaimIsNull);

        try
        {
            var identityClaimModelSpecification =
                new IdentityClaimModelSpecification(CrudMode.Add, identityClaimRepository);
            var identityResourceModelValidation =
                await identityClaimModelSpecification.ValidateAsync(identityClaimsModel);
            if (identityClaimModelSpecification.IsValid)
            {
                loggerService.WriteTo(Log.Debug,
                    "Entered into add identity resource claim :" + identityClaimsModel.IdentityResourceId);
                var identityResourceClaimEntity = mapper.Map<IdentityClaimsModel, IdentityClaims>(identityClaimsModel);

                await identityClaimRepository.InsertAsync(identityResourceClaimEntity);
                return await identityClaimRepository.SaveChangesAsync();
            }

            return frameworkResult.Failed<FrameworkResult>(identityResourceModelValidation.ErrorCode);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Failed to add identity resource claim.");
            throw;
        }
    }

    /// <summary>
    /// Deletes all claims associated with the specified identity resource.
    /// </summary>
    /// <param name="identityResourceId">The identity resource whose claims should be deleted.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating success or failure.</returns>
    public virtual async Task<FrameworkResult> DeleteIdentityResourceClaimByResourceIdAsync(Guid identityResourceId)
    {
        if (!identityResourceId.IsValid())
            return frameworkResult.Failed<FrameworkResult>(ApiErrorCodes.IdentityResourceIdInvalid);

        try
        {
            var identityResource =
                await identityClaimRepository.GetAsync(x => x.IdentityResourceId == identityResourceId);
            if (identityResource.ContainsAny())
            {
                loggerService.WriteTo(Log.Debug, "Entered into remove identity resource claim :" + identityResourceId);
                await identityClaimRepository.DeleteAsync(identityResource);
                return await identityClaimRepository.SaveChangesAsync();
            }

            return frameworkResult.Failed<FrameworkResult>(ApiErrorCodes.IdentityResourceIdInvalid);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Failed to remove identity resource claims.");
            throw;
        }
    }

    /// <summary>
    /// Deletes a single identity resource claim by its unique identifier.
    /// </summary>
    /// <param name="identityResourceClaimId">The unique identifier of the claim to delete.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating success or failure.</returns>
    public virtual async Task<FrameworkResult> DeleteIdentityResourceClaimByIdAsync(Guid identityResourceClaimId)
    {
        if (!identityResourceClaimId.IsValid())
            return frameworkResult.Failed<FrameworkResult>(ApiErrorCodes.IdentityResourceClaimIdInvalid);

        try
        {
            var identityResourceClaims = await identityClaimRepository.GetAsync(x => x.Id == identityResourceClaimId);
            if (identityResourceClaims.ContainsAny())
            {
                loggerService.WriteTo(Log.Debug,
                    "Entered into remove identity resource claim :" + identityResourceClaimId);
                await identityClaimRepository.DeleteAsync(identityResourceClaims);
                return await identityClaimRepository.SaveChangesAsync();
            }

            return frameworkResult.Failed<FrameworkResult>(ApiErrorCodes.IdentityResourceClaimIdInvalid);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Failed to remove identity resource claims.");
            throw;
        }
    }

    /// <summary>
    /// Deletes an identity resource claim matching the specified resource ID and claim type.
    /// </summary>
    /// <param name="identityClaimsModel">The model identifying the resource and claim type to remove.</param>
    /// <returns>A <see cref="FrameworkResult"/> indicating success or failure.</returns>
    public virtual async Task<FrameworkResult> DeleteIdentityResourceClaimAsync(IdentityClaimsModel identityClaimsModel)
    {
        if (identityClaimsModel == null)
            return frameworkResult.Failed<FrameworkResult>(ApiErrorCodes.IdentityClaimIsNull);

        try
        {
            var identityClaimModelSpecification =
                new IdentityClaimModelSpecification(CrudMode.Delete, identityClaimRepository);
            var identityResourceModelValidation =
                await identityClaimModelSpecification.ValidateAsync(identityClaimsModel);
            if (identityClaimModelSpecification.IsValid)
            {
                var identityResource = await identityClaimRepository.GetAsync(x =>
                    x.IdentityResourceId == identityClaimsModel.IdentityResourceId &&
                    x.Type == identityClaimsModel.Type);
                if (identityResource.ContainsAny())
                {
                    loggerService.WriteTo(Log.Debug,
                        "Entered into remove identity resource claim :" + identityClaimsModel.IdentityResourceId);
                    await identityClaimRepository.DeleteAsync(identityResource);
                    return await identityClaimRepository.SaveChangesAsync();
                }

                return frameworkResult.Failed<FrameworkResult>(ApiErrorCodes
                    .InvalidIdentityResourceClaimTypeOrResourceId);
            }

            return frameworkResult.Failed<FrameworkResult>(identityResourceModelValidation.ErrorCode);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Failed to remove identity resource claims.");
            throw;
        }
    }

    /// <summary>
    /// Retrieves all claims associated with the specified identity resource.
    /// </summary>
    /// <param name="identityResourceId">The identity resource whose claims to retrieve.</param>
    /// <returns>A list of claim models, or an empty result if none exist.</returns>
    public virtual async Task<IList<IdentityClaimsModel>> GetIdentityResourceClaimsAsync(Guid identityResourceId)
    {
        if (!identityResourceId.IsValid()) frameworkResult.Throw(ApiErrorCodes.IdentityResourceIdInvalid);

        try
        {
            var identityResource =
                await identityClaimRepository.GetAsync(x => x.IdentityResourceId == identityResourceId);
            if (identityResource.ContainsAny())
            {
                loggerService.WriteTo(Log.Debug, "Entered into get identity resource claims :" + identityResourceId);
                return mapper.Map<IList<IdentityClaims>, IList<IdentityClaimsModel>>(identityResource);
            }

            return frameworkResult.EmptyResult<IList<IdentityClaimsModel>>(ApiErrorCodes.NoRecordsFound);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, "Failed to get identity resource claims.");
            throw;
        }
    }
}
