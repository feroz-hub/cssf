/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Text.Json;
using HCL.CS.SF.Admin.UI.Interfaces;
using HCL.CS.SF.Admin.UI.Models.Api;
using HCL.CS.SF.Domain.Constants;

namespace HCL.CS.SF.Admin.UI.Services;

public interface IAdminApiService
{
    // Dashboard
    Task<List<UserDisplayModel>> GetAllUsersAsync(CancellationToken ct = default);
    Task<List<RoleModel>> GetAllRolesAsync(CancellationToken ct = default);
    Task<List<ClientsModel>> GetAllClientsAsync(CancellationToken ct = default);
    Task<List<ApiResourcesModel>> GetAllApiResourcesAsync(CancellationToken ct = default);
    Task<List<IdentityResourcesModel>> GetAllIdentityResourcesAsync(CancellationToken ct = default);

    // Users
    Task<FrameworkResult<UserModel>> GetUserByIdAsync(string userId, CancellationToken ct = default);
    Task<FrameworkResult> RegisterUserAsync(UserModel user, CancellationToken ct = default);
    Task<FrameworkResult> UpdateUserAsync(UserModel user, CancellationToken ct = default);
    Task<FrameworkResult> DeleteUserByIdAsync(string userId, CancellationToken ct = default);
    Task<FrameworkResult> LockUserAsync(string userId, CancellationToken ct = default);
    Task<FrameworkResult> UnlockUserAsync(string userName, CancellationToken ct = default);
    Task<List<RoleModel>> GetUserRolesAsync(string userId, CancellationToken ct = default);
    Task<FrameworkResult> AddUserRoleAsync(string userId, string roleId, CancellationToken ct = default);
    Task<FrameworkResult> RemoveUserRoleAsync(string userId, string roleId, CancellationToken ct = default);
    Task<List<UserClaimModel>> GetUserClaimsAsync(string userId, CancellationToken ct = default);
    Task<FrameworkResult> AddUserClaimAsync(string userId, string claimType, string claimValue, CancellationToken ct = default);
    Task<FrameworkResult> RemoveUserClaimAsync(UserClaimModel claim, CancellationToken ct = default);

    // Roles
    Task<FrameworkResult<RoleModel>> GetRoleByIdAsync(string roleId, CancellationToken ct = default);
    Task<FrameworkResult> CreateRoleAsync(RoleModel role, CancellationToken ct = default);
    Task<FrameworkResult> UpdateRoleAsync(RoleModel role, CancellationToken ct = default);
    Task<FrameworkResult> DeleteRoleByIdAsync(string roleId, CancellationToken ct = default);
    Task<List<RoleClaimModel>> GetRoleClaimsAsync(string roleId, CancellationToken ct = default);
    Task<FrameworkResult> AddRoleClaimAsync(string roleId, string claimType, string claimValue, CancellationToken ct = default);
    Task<FrameworkResult> RemoveRoleClaimAsync(RoleClaimModel claim, CancellationToken ct = default);

    // Clients
    Task<FrameworkResult<ClientsModel>> GetClientAsync(string clientId, CancellationToken ct = default);
    Task<FrameworkResult> RegisterClientAsync(ClientsModel client, CancellationToken ct = default);
    Task<FrameworkResult> UpdateClientAsync(ClientsModel client, CancellationToken ct = default);
    Task<FrameworkResult> DeleteClientAsync(string clientId, CancellationToken ct = default);
    Task<FrameworkResult> GenerateClientSecretAsync(string clientId, CancellationToken ct = default);

    // Resources
    Task<FrameworkResult<ApiResourcesModel>> GetApiResourceByIdAsync(string resourceId, CancellationToken ct = default);
    Task<FrameworkResult> AddApiResourceAsync(ApiResourcesModel resource, CancellationToken ct = default);
    Task<FrameworkResult> UpdateApiResourceAsync(ApiResourcesModel resource, CancellationToken ct = default);
    Task<FrameworkResult> DeleteApiResourceByIdAsync(string resourceId, CancellationToken ct = default);
    Task<List<ApiScopesModel>> GetAllApiScopesAsync(CancellationToken ct = default);
    Task<FrameworkResult> AddApiScopeAsync(ApiScopesModel scope, CancellationToken ct = default);
    Task<FrameworkResult> UpdateApiScopeAsync(ApiScopesModel scope, CancellationToken ct = default);
    Task<FrameworkResult> DeleteApiScopeByIdAsync(string scopeId, CancellationToken ct = default);

    // Identity Resources
    Task<FrameworkResult<IdentityResourcesModel>> GetIdentityResourceByIdAsync(string resourceId, CancellationToken ct = default);
    Task<FrameworkResult> AddIdentityResourceAsync(IdentityResourcesModel resource, CancellationToken ct = default);
    Task<FrameworkResult> UpdateIdentityResourceAsync(IdentityResourcesModel resource, CancellationToken ct = default);
    Task<FrameworkResult> DeleteIdentityResourceByIdAsync(string resourceId, CancellationToken ct = default);

    // Audit
    Task<AuditResponseModel> GetAuditDetailsAsync(AuditSearchRequestModel request, CancellationToken ct = default);

    // Revocation
    Task<List<SecurityTokensModel>> GetActiveTokensByClientIdsAsync(List<string> clientIds, CancellationToken ct = default);
    Task<List<SecurityTokensModel>> GetActiveTokensByUserIdsAsync(List<string> userIds, CancellationToken ct = default);
    Task<bool> RevokeTokenAsync(string token, string tokenTypeHint, CancellationToken ct = default);

    // Notifications
    Task<NotificationLogResponseModel> GetNotificationLogsAsync(NotificationSearchRequestModel request, CancellationToken ct = default);
    Task<List<ProviderConfigModel>> GetAllProviderConfigsAsync(CancellationToken ct = default);
    Task<FrameworkResult> SaveProviderConfigAsync(ProviderConfigModel config, CancellationToken ct = default);
    Task<FrameworkResult> DeleteProviderConfigAsync(string configId, CancellationToken ct = default);
    Task<FrameworkResult> SendTestNotificationAsync(object request, CancellationToken ct = default);

    // External Auth
    Task<List<ExternalAuthProviderConfigModel>> GetAllExternalAuthProvidersAsync(CancellationToken ct = default);
    Task<FrameworkResult> SaveExternalAuthProviderAsync(ExternalAuthProviderConfigModel config, CancellationToken ct = default);
    Task<FrameworkResult> DeleteExternalAuthProviderAsync(string id, CancellationToken ct = default);
}

public sealed class AdminApiService : IAdminApiService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IApiClientService apiClient;
    private readonly ILogger<AdminApiService> logger;

    public AdminApiService(IApiClientService apiClient, ILogger<AdminApiService> logger)
    {
        this.apiClient = apiClient;
        this.logger = logger;
    }

    // -----------------------------------------------------------------------
    // Dashboard
    // -----------------------------------------------------------------------

    public Task<List<UserDisplayModel>> GetAllUsersAsync(CancellationToken ct = default)
        => PostAndDeserializeListAsync<object, UserDisplayModel>(ApiRoutePathConstants.GetAllUsers, new { }, ct);

    public Task<List<RoleModel>> GetAllRolesAsync(CancellationToken ct = default)
        => PostAndDeserializeListAsync<object, RoleModel>(ApiRoutePathConstants.GetAllRoles, new { }, ct);

    public async Task<List<ClientsModel>> GetAllClientsAsync(CancellationToken ct = default)
    {
        const string path = ApiRoutePathConstants.GetAllClient;

        try
        {
            var response = await apiClient.PostAsync(path, new { }, ct);

            if (!response.Succeeded || string.IsNullOrWhiteSpace(response.ResponseBody))
            {
                logger.LogWarning("API call to {Path} failed. Status: {StatusCode}, Error: {Error}",
                    path, response.StatusCode, response.ErrorMessage);
                return new List<ClientsModel>();
            }

            using var json = JsonDocument.Parse(response.ResponseBody);
            if (json.RootElement.ValueKind == JsonValueKind.Array)
                return JsonSerializer.Deserialize<List<ClientsModel>>(response.ResponseBody, JsonOptions)
                       ?? new List<ClientsModel>();

            if (json.RootElement.ValueKind == JsonValueKind.Object)
            {
                var clientMap = JsonSerializer.Deserialize<Dictionary<string, string?>>(response.ResponseBody, JsonOptions)
                                ?? new Dictionary<string, string?>();

                return clientMap
                    .Select(entry => new ClientsModel
                    {
                        Id = entry.Key,
                        ClientId = entry.Key,
                        ClientName = entry.Value
                    })
                    .OrderBy(client => client.ClientName ?? client.ClientId)
                    .ToList();
            }

            logger.LogWarning("Unexpected client list payload shape returned from {Path}.", path);
            return new List<ClientsModel>();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception while calling API endpoint {Path}", path);
            return new List<ClientsModel>();
        }
    }

    public Task<List<ApiResourcesModel>> GetAllApiResourcesAsync(CancellationToken ct = default)
        => PostAndDeserializeListAsync<object, ApiResourcesModel>(ApiRoutePathConstants.GetAllApiResources, new { }, ct);

    public Task<List<IdentityResourcesModel>> GetAllIdentityResourcesAsync(CancellationToken ct = default)
        => PostAndDeserializeListAsync<object, IdentityResourcesModel>(ApiRoutePathConstants.GetAllIdentityResources, new { }, ct);

    // -----------------------------------------------------------------------
    // Users
    // -----------------------------------------------------------------------

    public Task<FrameworkResult<UserModel>> GetUserByIdAsync(string userId, CancellationToken ct = default)
        => PostAndDeserializeAsync<object, FrameworkResult<UserModel>>(ApiRoutePathConstants.GetUserById, new { userId }, ct);

    public Task<FrameworkResult> RegisterUserAsync(UserModel user, CancellationToken ct = default)
        => PostAndDeserializeAsync<UserModel, FrameworkResult>(ApiRoutePathConstants.RegisterUser, user, ct);

    public Task<FrameworkResult> UpdateUserAsync(UserModel user, CancellationToken ct = default)
        => PostAndDeserializeAsync<UserModel, FrameworkResult>(ApiRoutePathConstants.UpdateUser, user, ct);

    public Task<FrameworkResult> DeleteUserByIdAsync(string userId, CancellationToken ct = default)
        => PostAndDeserializeAsync<object, FrameworkResult>(ApiRoutePathConstants.DeleteUserById, new { userId }, ct);

    public Task<FrameworkResult> LockUserAsync(string userId, CancellationToken ct = default)
        => PostAndDeserializeAsync<object, FrameworkResult>(ApiRoutePathConstants.LockUser, new { userId }, ct);

    public Task<FrameworkResult> UnlockUserAsync(string userName, CancellationToken ct = default)
        => PostAndDeserializeAsync<object, FrameworkResult>(ApiRoutePathConstants.UnLockUser, new { userName }, ct);

    public Task<List<RoleModel>> GetUserRolesAsync(string userId, CancellationToken ct = default)
        => PostAndDeserializeListAsync<object, RoleModel>(ApiRoutePathConstants.GetUserRoles, new { userId }, ct);

    public Task<FrameworkResult> AddUserRoleAsync(string userId, string roleId, CancellationToken ct = default)
        => PostAndDeserializeAsync<object, FrameworkResult>(ApiRoutePathConstants.AddUserRole, new { userId, roleId }, ct);

    public Task<FrameworkResult> RemoveUserRoleAsync(string userId, string roleId, CancellationToken ct = default)
        => PostAndDeserializeAsync<object, FrameworkResult>(ApiRoutePathConstants.RemoveUserRole, new { userId, roleId }, ct);

    public Task<List<UserClaimModel>> GetUserClaimsAsync(string userId, CancellationToken ct = default)
        => PostAndDeserializeListAsync<object, UserClaimModel>(ApiRoutePathConstants.GetUserClaims, new { userId }, ct);

    public Task<FrameworkResult> AddUserClaimAsync(string userId, string claimType, string claimValue, CancellationToken ct = default)
        => PostAndDeserializeAsync<object, FrameworkResult>(ApiRoutePathConstants.AddClaim, new { userId, claimType, claimValue }, ct);

    public Task<FrameworkResult> RemoveUserClaimAsync(UserClaimModel claim, CancellationToken ct = default)
        => PostAndDeserializeAsync<UserClaimModel, FrameworkResult>(ApiRoutePathConstants.RemoveClaim, claim, ct);

    // -----------------------------------------------------------------------
    // Roles
    // -----------------------------------------------------------------------

    public Task<FrameworkResult<RoleModel>> GetRoleByIdAsync(string roleId, CancellationToken ct = default)
        => PostAndDeserializeAsync<object, FrameworkResult<RoleModel>>(ApiRoutePathConstants.GetRoleById, new { roleId }, ct);

    public Task<FrameworkResult> CreateRoleAsync(RoleModel role, CancellationToken ct = default)
        => PostAndDeserializeAsync<RoleModel, FrameworkResult>(ApiRoutePathConstants.CreateRole, role, ct);

    public Task<FrameworkResult> UpdateRoleAsync(RoleModel role, CancellationToken ct = default)
        => PostAndDeserializeAsync<RoleModel, FrameworkResult>(ApiRoutePathConstants.UpdateRole, role, ct);

    public Task<FrameworkResult> DeleteRoleByIdAsync(string roleId, CancellationToken ct = default)
        => PostAndDeserializeAsync<object, FrameworkResult>(ApiRoutePathConstants.DeleteRoleById, new { roleId }, ct);

    public Task<List<RoleClaimModel>> GetRoleClaimsAsync(string roleId, CancellationToken ct = default)
        => PostAndDeserializeListAsync<object, RoleClaimModel>(ApiRoutePathConstants.GetRoleClaim, new { roleId }, ct);

    public Task<FrameworkResult> AddRoleClaimAsync(string roleId, string claimType, string claimValue, CancellationToken ct = default)
        => PostAndDeserializeAsync<object, FrameworkResult>(ApiRoutePathConstants.AddRoleClaim, new { roleId, claimType, claimValue }, ct);

    public Task<FrameworkResult> RemoveRoleClaimAsync(RoleClaimModel claim, CancellationToken ct = default)
        => PostAndDeserializeAsync<RoleClaimModel, FrameworkResult>(ApiRoutePathConstants.RemoveRoleClaim, claim, ct);

    // -----------------------------------------------------------------------
    // Clients
    // -----------------------------------------------------------------------

    public Task<FrameworkResult<ClientsModel>> GetClientAsync(string clientId, CancellationToken ct = default)
        => PostAndDeserializeAsync<object, FrameworkResult<ClientsModel>>(ApiRoutePathConstants.GetClient, new { clientId }, ct);

    public Task<FrameworkResult> RegisterClientAsync(ClientsModel client, CancellationToken ct = default)
        => PostAndDeserializeAsync<ClientsModel, FrameworkResult>(ApiRoutePathConstants.RegisterClient, client, ct);

    public Task<FrameworkResult> UpdateClientAsync(ClientsModel client, CancellationToken ct = default)
        => PostAndDeserializeAsync<ClientsModel, FrameworkResult>(ApiRoutePathConstants.UpdateClient, client, ct);

    public Task<FrameworkResult> DeleteClientAsync(string clientId, CancellationToken ct = default)
        => PostAndDeserializeAsync<object, FrameworkResult>(ApiRoutePathConstants.DeleteClient, new { clientId }, ct);

    public Task<FrameworkResult> GenerateClientSecretAsync(string clientId, CancellationToken ct = default)
        => PostAndDeserializeAsync<object, FrameworkResult>(ApiRoutePathConstants.GenerateClientSecret, new { clientId }, ct);

    // -----------------------------------------------------------------------
    // API Resources
    // -----------------------------------------------------------------------

    public Task<FrameworkResult<ApiResourcesModel>> GetApiResourceByIdAsync(string resourceId, CancellationToken ct = default)
        => PostAndDeserializeAsync<object, FrameworkResult<ApiResourcesModel>>(ApiRoutePathConstants.GetApiResourceById, new { resourceId }, ct);

    public Task<FrameworkResult> AddApiResourceAsync(ApiResourcesModel resource, CancellationToken ct = default)
        => PostAndDeserializeAsync<ApiResourcesModel, FrameworkResult>(ApiRoutePathConstants.AddApiResource, resource, ct);

    public Task<FrameworkResult> UpdateApiResourceAsync(ApiResourcesModel resource, CancellationToken ct = default)
        => PostAndDeserializeAsync<ApiResourcesModel, FrameworkResult>(ApiRoutePathConstants.UpdateApiResource, resource, ct);

    public Task<FrameworkResult> DeleteApiResourceByIdAsync(string resourceId, CancellationToken ct = default)
        => PostAndDeserializeAsync<object, FrameworkResult>(ApiRoutePathConstants.DeleteApiResourceById, new { resourceId }, ct);

    public Task<List<ApiScopesModel>> GetAllApiScopesAsync(CancellationToken ct = default)
        => PostAndDeserializeListAsync<object, ApiScopesModel>(ApiRoutePathConstants.GetAllApiScopes, new { }, ct);

    public Task<FrameworkResult> AddApiScopeAsync(ApiScopesModel scope, CancellationToken ct = default)
        => PostAndDeserializeAsync<ApiScopesModel, FrameworkResult>(ApiRoutePathConstants.AddApiScope, scope, ct);

    public Task<FrameworkResult> UpdateApiScopeAsync(ApiScopesModel scope, CancellationToken ct = default)
        => PostAndDeserializeAsync<ApiScopesModel, FrameworkResult>(ApiRoutePathConstants.UpdateApiScope, scope, ct);

    public Task<FrameworkResult> DeleteApiScopeByIdAsync(string scopeId, CancellationToken ct = default)
        => PostAndDeserializeAsync<object, FrameworkResult>(ApiRoutePathConstants.DeleteApiScopeById, new { scopeId }, ct);

    // -----------------------------------------------------------------------
    // Identity Resources
    // -----------------------------------------------------------------------

    public Task<FrameworkResult<IdentityResourcesModel>> GetIdentityResourceByIdAsync(string resourceId, CancellationToken ct = default)
        => PostAndDeserializeAsync<object, FrameworkResult<IdentityResourcesModel>>(ApiRoutePathConstants.GetIdentityResourceById, new { resourceId }, ct);

    public Task<FrameworkResult> AddIdentityResourceAsync(IdentityResourcesModel resource, CancellationToken ct = default)
        => PostAndDeserializeAsync<IdentityResourcesModel, FrameworkResult>(ApiRoutePathConstants.AddIdentityResource, resource, ct);

    public Task<FrameworkResult> UpdateIdentityResourceAsync(IdentityResourcesModel resource, CancellationToken ct = default)
        => PostAndDeserializeAsync<IdentityResourcesModel, FrameworkResult>(ApiRoutePathConstants.UpdateIdentityResource, resource, ct);

    public Task<FrameworkResult> DeleteIdentityResourceByIdAsync(string resourceId, CancellationToken ct = default)
        => PostAndDeserializeAsync<object, FrameworkResult>(ApiRoutePathConstants.DeleteIdentityResourceById, new { resourceId }, ct);

    // -----------------------------------------------------------------------
    // Audit
    // -----------------------------------------------------------------------

    public Task<AuditResponseModel> GetAuditDetailsAsync(AuditSearchRequestModel request, CancellationToken ct = default)
        => PostAndDeserializeAsync<AuditSearchRequestModel, AuditResponseModel>(ApiRoutePathConstants.GetAuditDetails, request, ct);

    // -----------------------------------------------------------------------
    // Revocation
    // -----------------------------------------------------------------------

    public Task<List<SecurityTokensModel>> GetActiveTokensByClientIdsAsync(List<string> clientIds, CancellationToken ct = default)
        => PostAndDeserializeListAsync<object, SecurityTokensModel>(ApiRoutePathConstants.GetActiveSecurityTokensByClientIds, new { clientIds }, ct);

    public Task<List<SecurityTokensModel>> GetActiveTokensByUserIdsAsync(List<string> userIds, CancellationToken ct = default)
        => PostAndDeserializeListAsync<object, SecurityTokensModel>(ApiRoutePathConstants.GetActiveSecurityTokensByUserIds, new { userIds }, ct);

    public async Task<bool> RevokeTokenAsync(string token, string tokenTypeHint, CancellationToken ct = default)
    {
        var response = await apiClient.PostAsync("Security/Api/Token/RevokeToken", new { token, tokenTypeHint }, ct);
        return response.Succeeded;
    }

    // -----------------------------------------------------------------------
    // Notifications
    // -----------------------------------------------------------------------

    public Task<NotificationLogResponseModel> GetNotificationLogsAsync(NotificationSearchRequestModel request, CancellationToken ct = default)
        => PostAndDeserializeAsync<NotificationSearchRequestModel, NotificationLogResponseModel>(ApiRoutePathConstants.GetNotificationLogs, request, ct);

    public Task<List<ProviderConfigModel>> GetAllProviderConfigsAsync(CancellationToken ct = default)
        => PostAndDeserializeListAsync<object, ProviderConfigModel>(ApiRoutePathConstants.GetAllProviderConfigs, new { }, ct);

    public Task<FrameworkResult> SaveProviderConfigAsync(ProviderConfigModel config, CancellationToken ct = default)
        => PostAndDeserializeAsync<ProviderConfigModel, FrameworkResult>(ApiRoutePathConstants.SaveProviderConfig, config, ct);

    public Task<FrameworkResult> DeleteProviderConfigAsync(string configId, CancellationToken ct = default)
        => PostAndDeserializeAsync<object, FrameworkResult>(ApiRoutePathConstants.DeleteProviderConfig, new { configId }, ct);

    public Task<FrameworkResult> SendTestNotificationAsync(object request, CancellationToken ct = default)
        => PostAndDeserializeAsync<object, FrameworkResult>(ApiRoutePathConstants.SendTestNotification, request, ct);

    // -----------------------------------------------------------------------
    // External Auth
    // -----------------------------------------------------------------------

    public Task<List<ExternalAuthProviderConfigModel>> GetAllExternalAuthProvidersAsync(CancellationToken ct = default)
        => PostAndDeserializeListAsync<object, ExternalAuthProviderConfigModel>(ApiRoutePathConstants.GetAllExternalAuthProviders, new { }, ct);

    public Task<FrameworkResult> SaveExternalAuthProviderAsync(ExternalAuthProviderConfigModel config, CancellationToken ct = default)
        => PostAndDeserializeAsync<ExternalAuthProviderConfigModel, FrameworkResult>(ApiRoutePathConstants.SaveExternalAuthProvider, config, ct);

    public Task<FrameworkResult> DeleteExternalAuthProviderAsync(string id, CancellationToken ct = default)
        => PostAndDeserializeAsync<object, FrameworkResult>(ApiRoutePathConstants.DeleteExternalAuthProvider, new { id }, ct);

    // -----------------------------------------------------------------------
    // Private helpers
    // -----------------------------------------------------------------------

    private async Task<TResponse> PostAndDeserializeAsync<TRequest, TResponse>(
        string path, TRequest payload, CancellationToken ct) where TResponse : new()
    {
        try
        {
            var response = await apiClient.PostAsync(path, payload, ct);

            if (!response.Succeeded || string.IsNullOrWhiteSpace(response.ResponseBody))
            {
                logger.LogWarning("API call to {Path} failed. Status: {StatusCode}, Error: {Error}",
                    path, response.StatusCode, response.ErrorMessage);
                return new TResponse();
            }

            return JsonSerializer.Deserialize<TResponse>(response.ResponseBody, JsonOptions) ?? new TResponse();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception while calling API endpoint {Path}", path);
            return new TResponse();
        }
    }

    private async Task<List<TItem>> PostAndDeserializeListAsync<TRequest, TItem>(
        string path, TRequest payload, CancellationToken ct)
    {
        try
        {
            var response = await apiClient.PostAsync(path, payload, ct);

            if (!response.Succeeded || string.IsNullOrWhiteSpace(response.ResponseBody))
            {
                logger.LogWarning("API call to {Path} failed. Status: {StatusCode}, Error: {Error}",
                    path, response.StatusCode, response.ErrorMessage);
                return new List<TItem>();
            }

            return JsonSerializer.Deserialize<List<TItem>>(response.ResponseBody, JsonOptions) ?? new List<TItem>();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception while calling API endpoint {Path}", path);
            return new List<TItem>();
        }
    }
}
