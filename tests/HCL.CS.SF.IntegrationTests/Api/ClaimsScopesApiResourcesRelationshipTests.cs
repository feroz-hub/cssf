/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.ErrorCodes;
using HCL.CS.SF.Domain.Models;
using HCL.CS.SF.Service.Interfaces;
using HCL.CS.SF.TestApp.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HCL.CS.SF.IntegrationTests.Api;

/// <summary>
/// Tests for relationships between API Resources, API Scopes, API Resource Claims, API Scope Claims, and Role Claims,
/// and for verifying existence/active checks (Get by id/name, soft delete, Enabled).
/// </summary>
public class ClaimsScopesApiResourcesRelationshipTests : HCLCSSFFakeSetup
{
    private const string ResourceName = "AlphaClientOne";
    private const string ScopeName = "ClientApi.Create";
    private readonly Random _random = new();
    private readonly IApiResourceService _apiResourceService;
    private readonly IRoleService _roleService;

    public ClaimsScopesApiResourcesRelationshipTests()
    {
        _apiResourceService = ServiceProvider.GetService<IApiResourceService>();
        _roleService = ServiceProvider.GetService<IRoleService>();
    }

    #region Relationship: API Resource → API Resource Claims & API Scopes

    [Fact]
    [Trait("Category", "Relationship")]
    public async Task GetApiResource_ById_IncludesApiResourceClaimsAndApiScopes()
    {
        await EnsureResourceExistsAsync();
        var resource = await _apiResourceService.GetApiResourceAsync(ResourceName);
        resource.Should().NotBeNull();
        resource.ApiResourceClaims.Should().NotBeNull();
        resource.ApiScopes.Should().NotBeNull();
        resource.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    [Trait("Category", "Relationship")]
    public async Task GetApiResource_ById_EachScopeIncludesApiScopeClaims()
    {
        await EnsureResourceExistsAsync();
        var resource = await _apiResourceService.GetApiResourceAsync(ResourceName);
        resource.Should().NotBeNull();
        resource.ApiScopes.Should().NotBeNull();
        foreach (var scope in resource.ApiScopes)
        {
            scope.ApiScopeClaims.Should().NotBeNull();
        }
    }

    [Fact]
    [Trait("Category", "Relationship")]
    public async Task GetApiResourceClaims_ByResourceId_ReturnsOnlyClaimsForThatResource()
    {
        await EnsureResourceExistsAsync();
        var resource = await _apiResourceService.GetApiResourceAsync(ResourceName);
        resource.Should().NotBeNull();
        var claims = await _apiResourceService.GetApiResourceClaimsAsync(resource.Id);
        claims.Should().NotBeNull();
        foreach (var c in claims)
        {
            c.ApiResourceId.Should().Be(resource.Id);
        }
    }

    [Fact]
    [Trait("Category", "Relationship")]
    public async Task GetApiScopeClaims_ByScopeId_ReturnsOnlyClaimsForThatScope()
    {
        await EnsureResourceExistsAsync();
        var resource = await _apiResourceService.GetApiResourceAsync(ResourceName);
        resource.Should().NotBeNull();
        resource.ApiScopes.Should().NotBeNullOrEmpty();
        var scope = resource.ApiScopes.First();
        var claims = await _apiResourceService.GetApiScopeClaimsAsync(scope.Id);
        claims.Should().NotBeNull();
        foreach (var c in claims)
        {
            c.ApiScopeId.Should().Be(scope.Id);
        }
    }

    #endregion

    #region Relationship: Role → Role Claims

    [Fact]
    [Trait("Category", "Relationship")]
    public async Task GetRole_ByName_IncludesRoleClaims()
    {
        var role = await _roleService.GetRoleAsync("SystemAdmin");
        role.Should().NotBeNull();
        role.RoleClaims.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Relationship")]
    public async Task GetRoleClaims_ReturnsClaimsForRoleOnly()
    {
        var role = await _roleService.GetRoleAsync("SystemAdmin");
        role.Should().NotBeNull();
        var claims = await _roleService.GetRoleClaimAsync(role);
        claims.Should().NotBeNull();
    }

    #endregion

    #region Exists / Active: API Resource

    [Fact]
    [Trait("Category", "Exists")]
    public async Task GetApiResource_ByInvalidId_ReturnsNullOrThrows()
    {
        var invalidId = Guid.NewGuid();
        var result = await _apiResourceService.GetApiResourceAsync(invalidId);
        result.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Exists")]
    public async Task GetApiResource_ByInvalidName_ReturnsNullOrThrows()
    {
        var invalidName = "NonExistentResource_" + _random.Next();
        var result = await _apiResourceService.GetApiResourceAsync(invalidName);
        result.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Exists")]
    public async Task GetApiResource_AfterDelete_ReturnsNull()
    {
        var name = "TempResource_" + _random.Next();
        var model = ApiResourceHelper.CreateApiResourceModel();
        model.Name = name;
        model.ApiResourceClaims = new List<ApiResourceClaimsModel>();
        model.ApiScopes = new List<ApiScopesModel>();
        var addResult = await _apiResourceService.AddApiResourceAsync(model);
        addResult.Status.Should().Be(ResultStatus.Success);

        var created = await _apiResourceService.GetApiResourceAsync(name);
        created.Should().NotBeNull();
        var id = created.Id;

        var deleteResult = await _apiResourceService.DeleteApiResourceAsync(id);
        deleteResult.Status.Should().Be(ResultStatus.Success);

        var afterDelete = await _apiResourceService.GetApiResourceAsync(id);
        afterDelete.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Active")]
    public async Task ApiResource_Enabled_CanBeUpdatedAndRead()
    {
        await EnsureResourceExistsAsync();
        var resource = await _apiResourceService.GetApiResourceAsync(ResourceName);
        resource.Should().NotBeNull();
        var originalEnabled = resource.Enabled;
        resource.Enabled = !originalEnabled;
        var updateResult = await _apiResourceService.UpdateApiResourceAsync(resource);
        updateResult.Status.Should().Be(ResultStatus.Success);

        var updated = await _apiResourceService.GetApiResourceAsync(resource.Id);
        updated.Should().NotBeNull();
        updated.Enabled.Should().Be(!originalEnabled);
    }

    #endregion

    #region Exists / Active: API Scope

    [Fact]
    [Trait("Category", "Exists")]
    public async Task GetApiScope_ByInvalidId_ReturnsNull()
    {
        var invalidId = Guid.NewGuid();
        var result = await _apiResourceService.GetApiScopeAsync(invalidId);
        result.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Exists")]
    public async Task GetApiScope_AfterDelete_ReturnsNull()
    {
        await EnsureResourceExistsAsync();
        var resource = await _apiResourceService.GetApiResourceAsync(ResourceName);
        resource.Should().NotBeNull();
        var scopeName = "TempScope_" + _random.Next();
        var scopeModel = ApiResourceHelper.CreateApiScopeModel();
        scopeModel.Name = scopeName;
        scopeModel.ApiResourceId = resource.Id;
        scopeModel.ApiScopeClaims = new List<ApiScopeClaimsModel>();
        var addResult = await _apiResourceService.AddApiScopeAsync(scopeModel);
        addResult.Status.Should().Be(ResultStatus.Success);

        var created = await _apiResourceService.GetApiScopeAsync(scopeName);
        created.Should().NotBeNull();
        var scopeId = created.Id;

        var deleteResult = await _apiResourceService.DeleteApiScopeAsync(scopeId);
        deleteResult.Status.Should().Be(ResultStatus.Success);

        var afterDelete = await _apiResourceService.GetApiScopeAsync(scopeId);
        afterDelete.Should().BeNull();
    }

    #endregion

    #region Exists / Active: API Resource Claims & API Scope Claims

    [Fact]
    [Trait("Category", "Exists")]
    public async Task GetApiResourceClaims_ByInvalidResourceId_ReturnsNull()
    {
        var invalidResourceId = Guid.NewGuid();
        var claims = await _apiResourceService.GetApiResourceClaimsAsync(invalidResourceId);
        claims.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Exists")]
    public async Task GetApiScopeClaims_ByInvalidScopeId_ReturnsEmptyOrNull()
    {
        var invalidScopeId = Guid.NewGuid();
        var claims = await _apiResourceService.GetApiScopeClaimsAsync(invalidScopeId);
        claims.Should().BeNullOrEmpty();
    }

    #endregion

    #region Exists / Active: Role

    [Fact]
    [Trait("Category", "Exists")]
    public async Task GetRole_ByInvalidId_ReturnsNull()
    {
        var invalidId = Guid.NewGuid();
        var result = await _roleService.GetRoleAsync(invalidId);
        result.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Exists")]
    public async Task GetRole_ByInvalidName_ReturnsNull()
    {
        var invalidName = "NonExistentRole_" + _random.Next();
        var result = await _roleService.GetRoleAsync(invalidName);
        result.Should().BeNull();
    }

    #endregion

    #region Link: Requested scopes → ApiResourcesByScopes (claim types)

    [Fact]
    [Trait("Category", "Relationship")]
    public async Task GetAllApiResourcesByScopesAsync_WithValidScope_ReturnsMatchingResourceAndClaimTypes()
    {
        await EnsureResourceExistsAsync();
        var resource = await _apiResourceService.GetApiResourceAsync(ResourceName);
        resource.Should().NotBeNull();
        var scopeNames = resource.ApiScopes?.Select(s => s.Name).ToList() ?? new List<string>();
        if (scopeNames.Count == 0)
            return;
        var requestedScopes = new List<string> { scopeNames[0] };
        var byScopes = await _apiResourceService.GetAllApiResourcesByScopesAsync(requestedScopes);
        byScopes.Should().NotBeNull();
        byScopes.Should().Contain(x => requestedScopes.Contains(x.ApiScopeName) || requestedScopes.Contains(x.ApiResourceName));
    }

    #endregion

    private async Task EnsureResourceExistsAsync()
    {
        var existing = await _apiResourceService.GetApiResourceAsync(ResourceName);
        if (existing != null)
            return;
        var model = ApiResourceHelper.CreateApiResourceModel();
        model.Name = ResourceName;
        var result = await _apiResourceService.AddApiResourceAsync(model);
        result.Status.Should().Be(ResultStatus.Success);
    }
}
