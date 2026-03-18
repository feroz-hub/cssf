/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Transactions;
using AutoMapper;
using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Configurations.Endpoint;
using HCL.CS.SF.Domain.Constants;
using HCL.CS.SF.Domain.Constants.Endpoint;
using HCL.CS.SF.Domain.Entities.Api;
using HCL.CS.SF.Domain.Entities.Endpoint;
using HCL.CS.SF.Domain.ErrorCodes;
using HCL.CS.SF.Domain.Models.Endpoint;
using HCL.CS.SF.DomainServices;
using HCL.CS.SF.DomainServices.Infra;
using HCL.CS.SF.DomainServices.Repository.Api;
using HCL.CS.SF.DomainServices.UnitOfWork.Endpoint;
using HCL.CS.SF.Service.Implementation.Api.Specifications;
using HCL.CS.SF.Service.Implementation.Endpoint.Comparers;
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;
using HCL.CS.SF.Service.Interfaces.Interfaces.Api;

namespace HCL.CS.SF.Service.Implementation.Api.Services;

/// <summary>
/// Service for managing OAuth2/OIDC client registrations. Handles the full lifecycle of
/// client applications including registration (with auto-generated client_id and client_secret),
/// updates (with secret verification), deletion (with cascading token cleanup), and
/// client secret rotation. Enforces OAuth2 Dynamic Client Registration validation rules
/// including grant types, response types, redirect URIs, and PKCE requirements.
/// </summary>

public class ClientService(
    ILoggerInstance instance,
    IMapper mapper,
    IFrameworkResultService frameworkResult,
    IClientsUnitOfWork unitOfWork,
    HCLCSSFConfig securityConfig,
    IApiResourceRepository apiResourceRepository,
    IRepository<ApiScopes> apiScopeRepository,
    IIdentityResourceRepository identityResourceRepository)
    : SecurityBase, IClientServices
{
    private readonly ILoggerService loggerService = instance.GetLoggerInstance(LoggerKeyConstants.DefaultLoggerKey);
    private readonly TokenConfig tokenConfig = securityConfig.TokenSettings.TokenConfig;
    private readonly TokenExpiration tokenExpiration = securityConfig.TokenSettings.TokenExpiration;

    /// <summary>
    /// Registers a new OAuth2 client with auto-generated client_id and SHA-256 hashed client_secret.
    /// Validates the client model against OAuth2 specification rules before persisting.
    /// </summary>
    /// <param name="clientsModel">The client registration model containing name, grant types, scopes, etc.</param>
    /// <returns>The registered client model with the plaintext client_secret (returned only once).</returns>

    public virtual async Task<ClientsModel> RegisterClientAsync(ClientsModel clientsModel)
    {
        if (clientsModel == null) frameworkResult.Throw(EndpointErrorCodes.ArgumentNullError);

        try
        {
            var clientModelValidation = new ClientModelSpecification(unitOfWork, tokenExpiration, CrudMode.Add,
                apiResourceRepository, apiScopeRepository, identityResourceRepository);
            var validationError = await clientModelValidation.ValidateAsync(clientsModel);
            if (clientModelValidation.IsValid)
            {
                loggerService.WriteTo(Log.Debug, "Entered into register Client :" + clientsModel.ClientName);
                var generatedClientSecret = AuthenticationConstants.KeySize32.RandomString();
                clientsModel.ClientId = AuthenticationConstants.KeySize32.RandomString();
                clientsModel.ClientIdIssuedAt = DateTime.UtcNow;
                clientsModel.ClientSecret = generatedClientSecret.Sha256();
                clientsModel.ClientSecretExpiresAt = DateTime.UtcNow.AddDays(tokenConfig.ClientSecretExpirationInDays);
                clientsModel.RequireClientSecret = true;
                clientsModel.IsFirstPartyApp = true;

                var clientEntity = mapper.Map<ClientsModel, Clients>(clientsModel);
                await unitOfWork.ClientRepository.InsertAsync(clientEntity);
                var result = await unitOfWork.SaveChangesAsync();
                if (result.Status != ResultStatus.Succeeded)
                    frameworkResult.ThrowCustomMessage(result.Errors.ToList()[0].Description);

                var registeredClient = mapper.Map<Clients, ClientsModel>(clientEntity);
                registeredClient.ClientSecret = generatedClientSecret;
                return registeredClient;
            }

            frameworkResult.Throw(validationError.ErrorCode);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, ex.Message);
            throw;
        }

        return null;
    }

    /// <summary>
    /// Updates an existing OAuth2 client. Verifies the client secret before allowing modifications.
    /// Handles updates to redirect URIs and post-logout redirect URIs with proper diff-based
    /// insert/update/delete operations.
    /// </summary>
    /// <param name="clientsModel">The updated client model.</param>
    /// <returns>The updated client model.</returns>

    public virtual async Task<ClientsModel> UpdateClientAsync(ClientsModel clientsModel)
    {
        if (clientsModel == null) frameworkResult.Throw(EndpointErrorCodes.ArgumentNullError);

        try
        {
            var clientModelValidation = new ClientModelSpecification(unitOfWork, tokenExpiration, CrudMode.Update,
                apiResourceRepository, apiScopeRepository, identityResourceRepository);
            var validationError = await clientModelValidation.ValidateAsync(clientsModel);
            if (clientModelValidation.IsValid)
            {
                loggerService.WriteTo(Log.Debug, "Entered into update Client :" + clientsModel.ClientName);
                var clientsEntity = await unitOfWork.ClientRepository.GetAsync(client =>
                    client.ClientId == clientsModel.ClientId);

                var redirectUris = new List<ClientRedirectUrisModel>(clientsModel.RedirectUris);
                var postRedirectUris = new List<ClientPostLogoutRedirectUrisModel>(clientsModel.PostLogoutRedirectUris);
                if (clientsEntity.ContainsAny())
                {
                    var clientEntity = clientsEntity.FirstOrDefault();
                    var incomingSecret = clientsModel.ClientSecret;
                    var secretMatches = clientEntity.ClientSecret.CompareStrings(incomingSecret)
                                        || clientEntity.ClientSecret.CompareStrings(incomingSecret.Sha256())
                                        || clientEntity.ClientSecret.CompareStrings(incomingSecret.Sha512());
                    if (!secretMatches) frameworkResult.Throw(EndpointErrorCodes.ClientSecretInvalid);

                    var clientSecretExpiresAt = clientEntity.ClientSecretExpiresAt.ToDateTime();
                    if (DateTime.Compare(clientSecretExpiresAt, DateTime.UtcNow) < 0)
                        frameworkResult.Throw(EndpointErrorCodes.ClientSecretExpired);

                    clientsModel.ClientIdIssuedAt = clientEntity.ClientIdIssuedAt.ToDateTime();
                    clientsModel.ClientSecretExpiresAt = clientEntity.ClientSecretExpiresAt.ToDateTime();
                    clientsModel.RequireClientSecret = true;
                    clientsModel.IsFirstPartyApp = true;

                    clientEntity = mapper.Map(clientsModel, clientEntity);
                    var redirectUrisEntity = mapper.Map<List<ClientRedirectUris>>(redirectUris);
                    var postRedirectUrisEntity = mapper.Map<List<ClientPostLogoutRedirectUris>>(postRedirectUris);
                    await unitOfWork.ClientRepository.UpdateAsync(clientEntity);
                    await UpdateClientReferencesAsync(clientEntity, redirectUrisEntity, postRedirectUrisEntity);
                    var result = await unitOfWork.SaveChangesAsync();
                    if (result.Status != ResultStatus.Succeeded)
                        frameworkResult.ThrowCustomMessage(result.Errors.ToList()[0].Description);

                    return mapper.Map<Clients, ClientsModel>(clientEntity);
                }

                frameworkResult.Throw(EndpointErrorCodes.ClientDoesNotExist);
            }
            else
            {
                frameworkResult.Throw(validationError.ErrorCode);
            }
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, ex.Message);
            throw;
        }

        return null;
    }

    /// <summary>
    /// Deletes an OAuth2 client and all associated security tokens within a transaction scope.
    /// Uses hard-delete for tokens since they should not be recoverable after client removal.
    /// </summary>
    /// <param name="clientId">The client_id of the OAuth2 client to delete.</param>
    /// <returns>The result of the deletion operation.</returns>

    public virtual async Task<FrameworkResult> DeleteClientAsync(string clientId)
    {
        if (string.IsNullOrWhiteSpace(clientId))
            return frameworkResult.Failed<FrameworkResult>(EndpointErrorCodes.ClientIdIsRequired);

        try
        {
            var clientsEntity = await GetClientDetailsAsync(clientId);
            if (clientsEntity != null)
                // Transaction scope required - because delete client token is hard delete. we can't use UOF here.
                using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    loggerService.WriteTo(Log.Debug, "Entered into remove Client :" + clientsEntity.ClientName);
                    await unitOfWork.ClientRepository.DeleteAsync(clientsEntity);
                    var result = await unitOfWork.SaveChangesAsync();
                    if (result.Status == ResultStatus.Succeeded)
                        result = await DeleteClientTokens(clientsEntity.ClientId);

                    if (result.Status == ResultStatus.Succeeded) transactionScope.Complete();

                    return result;
                }

            return frameworkResult.Failed<FrameworkResult>(EndpointErrorCodes.ClientDoesNotExist);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Retrieves a client by client_id, including redirect URIs. Validates that the client secret has not expired.
    /// </summary>
    /// <param name="clientId">The client_id to look up.</param>
    /// <returns>The client model, or empty result if not found.</returns>

    public virtual async Task<ClientsModel> GetClientAsync(string clientId)
    {
        if (string.IsNullOrWhiteSpace(clientId)) frameworkResult.Throw(EndpointErrorCodes.InactiveClient);

        ClientsModel clientModel;
        try
        {
            var clientsEntity = await GetClientDetailsAsync(clientId);
            if (clientsEntity != null)
            {
                clientModel = mapper.Map<Clients, ClientsModel>(clientsEntity);
                loggerService.WriteTo(Log.Debug, "Entered into Get client by id : " + clientModel.ClientName);
                if (DateTime.Compare(clientModel.ClientSecretExpiresAt, DateTime.UtcNow) < 0)
                    frameworkResult.Throw(EndpointErrorCodes.ClientSecretExpired);

                return clientModel;
            }

            return frameworkResult.EmptyResult<ClientsModel>(ApiErrorCodes.NoRecordsFound);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Retrieves all registered OAuth2 clients as a dictionary of client_id to client_name.
    /// </summary>
    /// <returns>A dictionary mapping client_id to client_name for all active clients.</returns>

    public virtual async Task<Dictionary<string, string>> GetAllClientAsync()
    {
        try
        {
            var clientList = new Dictionary<string, string>();
            var clientsEntityList = await unitOfWork.ClientRepository.GetAllAsync();
            if (clientsEntityList.ContainsAny())
            {
                loggerService.WriteTo(Log.Debug, "Entered into get all clients - Count : " + clientsEntityList.Count);
                foreach (var clientEntity in clientsEntityList)
                    if (!clientList.ContainsKey(clientEntity.ClientId))
                        clientList.Add(clientEntity.ClientId, clientEntity.ClientName);

                return clientList;
            }

            return frameworkResult.EmptyResult<Dictionary<string, string>>(ApiErrorCodes.NoRecordsFound);
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Generates a new client secret for an existing OAuth2 client, replacing the previous one.
    /// The new secret is SHA-256 hashed before storage; the plaintext is returned to the caller only once.
    /// </summary>
    /// <param name="clientId">The client_id of the client to rotate the secret for.</param>
    /// <returns>The client model with the new plaintext client_secret.</returns>

    public virtual async Task<ClientsModel> GenerateClientSecret(string clientId)
    {
        // Change to specific required error code for clientId and clientSecret.
        if (string.IsNullOrWhiteSpace(clientId)) frameworkResult.Throw(EndpointErrorCodes.ClientIdIsRequired);

        var clientModel = new ClientsModel();
        try
        {
            var clientEntityList = await unitOfWork.ClientRepository.GetAsync(client =>
                client.ClientId == clientId);
            if (clientEntityList.ContainsAny())
            {
                var clientEntity = clientEntityList.FirstOrDefault();
                loggerService.WriteTo(Log.Debug,
                    "Entered into Generate Client secret for Client :" + clientEntity.ClientName);

                var generatedClientSecret = AuthenticationConstants.KeySize32.RandomString();
                clientEntity.ClientSecret = generatedClientSecret.Sha256();
                clientEntity.ClientSecretExpiresAt =
                    DateTime.UtcNow.AddDays(tokenConfig.ClientSecretExpirationInDays).ToUnixTime();

                await unitOfWork.ClientRepository.UpdateAsync(clientEntity);
                await unitOfWork.SaveChangesAsync();

                clientModel = mapper.Map<Clients, ClientsModel>(clientEntity);
                clientModel.ClientSecret = generatedClientSecret;
            }
            else
            {
                frameworkResult.Throw(EndpointErrorCodes.ClientDoesNotExist);
            }
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, ex.Message);
            throw;
        }

        return clientModel;
    }

    private async Task<Clients> GetClientDetailsAsync(string clientId)
    {
        Clients clientEntity = null;
        try
        {
            var clientsEntity = await unitOfWork.ClientRepository.GetAsync(client => client.ClientId == clientId,
                new System.Linq.Expressions.Expression<Func<Clients, object>>[] { x => x.RedirectUris, x => x.PostLogoutRedirectUris });
            if (clientsEntity.ContainsAny())
            {
                clientEntity = clientsEntity.ToList()[0];
                loggerService.WriteTo(Log.Debug,
                    "Entered into get client :" + clientEntity.ClientName); // To Be Discussed.
            }

            return clientEntity;
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, ex.Message);
            throw;
        }
    }

    private async Task UpdateClientReferencesAsync(Clients clientEntity, List<ClientRedirectUris> redirectUris,
        List<ClientPostLogoutRedirectUris> postLogoutRedirectUris)
    {
        try
        {
            var redirectUriEntities =
                await unitOfWork.RedirectUrisRepository.GetAsync(uri => uri.ClientId == clientEntity.Id);
            var redirectUrisToBeDeleted = redirectUriEntities.Except(redirectUris, new ClientRedirectUriComparer());

            if (clientEntity.RedirectUris.ContainsAny())
                foreach (var redirectUri in clientEntity.RedirectUris)
                {
                    var redirectURIList = await unitOfWork.RedirectUrisRepository.GetAsync(x => x.Id == redirectUri.Id);
                    if (redirectURIList.ContainsAny())
                    {
                        var redirectUriEntity = redirectURIList.FirstOrDefault();

                        if (redirectUrisToBeDeleted.Where(uri => uri.Id == redirectUriEntity.Id).ContainsAny())
                        {
                            await unitOfWork.RedirectUrisRepository.DeleteAsync(redirectUriEntity);
                        }
                        else
                        {
                            redirectUriEntity = mapper.Map(redirectUri, redirectUriEntity);
                            await unitOfWork.RedirectUrisRepository.UpdateAsync(redirectUriEntity);
                        }
                    }
                    else
                    {
                        await unitOfWork.RedirectUrisRepository.InsertAsync(redirectUri);
                    }
                }

            var postLogoutRedirectUriEntities =
                await unitOfWork.PostLogoutRedirectUrisRepository.GetAsync(uri => uri.ClientId == clientEntity.Id);
            var postLogoutRedirectUrisToBeDeleted =
                postLogoutRedirectUriEntities.Except(postLogoutRedirectUris, new ClientPostLogoutRedirectUriComparer());
            if (clientEntity.PostLogoutRedirectUris.ContainsAny())
                foreach (var postRedirectUri in clientEntity.PostLogoutRedirectUris)
                {
                    var postRedirectURIList =
                        await unitOfWork.PostLogoutRedirectUrisRepository.GetAsync(x => x.Id == postRedirectUri.Id);
                    if (postRedirectURIList.ContainsAny())
                    {
                        var postRedirectUriEntity = postRedirectURIList.FirstOrDefault();
                        if (postLogoutRedirectUrisToBeDeleted.Where(uri => uri.Id == postRedirectUriEntity.Id)
                            .ContainsAny())
                        {
                            await unitOfWork.PostLogoutRedirectUrisRepository.DeleteAsync(postRedirectUriEntity);
                        }
                        else
                        {
                            postRedirectUriEntity = mapper.Map(postRedirectUri, postRedirectUriEntity);
                            await unitOfWork.PostLogoutRedirectUrisRepository.UpdateAsync(postRedirectUriEntity);
                        }
                    }
                    else
                    {
                        await unitOfWork.PostLogoutRedirectUrisRepository.InsertAsync(postRedirectUri);
                    }
                }
        }
        catch (Exception ex)
        {
            loggerService.WriteToWithCaller(Log.Error, ex, ex.Message);
            throw;
        }
    }

    private async Task<FrameworkResult> DeleteClientTokens(string clientId)
    {
        var securityTokenList = await unitOfWork.SecurityTokensRepository.GetAsync(x => x.ClientId == clientId);
        if (securityTokenList.ContainsAny())
        {
            await unitOfWork.SecurityTokensRepository.DeleteAsync(securityTokenList);
            return await unitOfWork.SaveChangesWithHardDeleteAsync();
        }

        return frameworkResult.Succeeded();
    }
}
