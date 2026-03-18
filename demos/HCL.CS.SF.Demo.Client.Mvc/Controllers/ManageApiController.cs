/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Lib.AspNetCore.Mvc.JqGrid.Core.Request;
using Lib.AspNetCore.Mvc.JqGrid.Core.Response;
using Lib.AspNetCore.Mvc.JqGrid.Core.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HCL.CS.SF.DemoClientMvc.Extension;
using HCL.CS.SF.DemoClientMvc.Interface;
using HCL.CS.SF.DemoClientMvc.Models;
using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Constants;
using HCL.CS.SF.Domain.Constants.Endpoint;
using HCL.CS.SF.Domain.Enums;
using HCL.CS.SF.Domain.Models.Api;
using HCL.CS.SF.Domain.Models.Endpoint;

namespace HCL.CS.SF.DemoClientMvc.Controllers;

[Authorize(Roles = "HCLCSSFAdmin")]
public class ManageApiController(IHttpService httpService) : Controller
{
    [Authorize]
    public IActionResult ApiResource(string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    public async Task<IActionResult> GetAllApiResources(JqGridRequest request, int? rowId)
    {
        try
        {
            var apiresourceList =
                await httpService.PostSecureAsync<IList<ApiResourcesModel>>(ApiRoutePathConstants.GetAllApiResources,
                    string.Empty);
            if (apiresourceList != null)
            {
                var totalRecords = apiresourceList.Count();

                var response = new JqGridResponse
                {
                    TotalPagesCount = (int)Math.Ceiling(totalRecords / (float)request.RecordsCount),
                    PageIndex = request.PageIndex,
                    TotalRecordsCount = totalRecords
                };

                apiresourceList = apiresourceList.OrderBy(x => x.Name).ToList();
                foreach (var resource in apiresourceList.Skip(request.PageIndex * request.RecordsCount)
                             .Take(request.PagesCount * request.RecordsCount))
                    response.Records.Add(new JqGridRecord(Convert.ToString(resource.Id), resource));

                response.Reader.RepeatItems = false;
                return new JqGridJsonResult(response);
            }

            return View();
        }
        catch (Exception ex)
        {
            return RedirectToAction("Error", "Error", new { errorCode = "", errorMessage = ex.Message });
        }
    }

    public async Task<IActionResult> UpdateApiResources(ApiResourcesModel apiResourcesModel)
    {
        try
        {
            if (apiResourcesModel != null)
            {
                ApiResourcesModel apiResource = null;
                if (apiResourcesModel.Id != Guid.Empty && apiResourcesModel.Id != default)
                    apiResource =
                        await httpService.PostSecureAsync<ApiResourcesModel>(ApiRoutePathConstants.GetApiResourceById,
                            apiResourcesModel.Id);

                if (apiResource == null)
                {
                    apiResourcesModel.Enabled = true;
                    apiResourcesModel.CreatedBy = User.Identity.Name;
                    var result =
                        await httpService.PostSecureAsync<FrameworkResult>(ApiRoutePathConstants.AddApiResource,
                            apiResourcesModel);
                    if (result.Status == ResultStatus.Failed) return Json(result.Errors.FirstOrDefault().Description);
                }
                else
                {
                    apiResourcesModel.CreatedBy = apiResource.CreatedBy;
                    apiResourcesModel.CreatedOn = apiResource.CreatedOn;
                    apiResourcesModel.Enabled = apiResource.Enabled;
                    apiResourcesModel.ModifiedBy = User.Identity.Name;
                    var result =
                        await httpService.PostSecureAsync<FrameworkResult>(ApiRoutePathConstants.UpdateApiResource,
                            apiResourcesModel);
                    if (result.Status == ResultStatus.Failed) return Json(result.Errors.FirstOrDefault().Description);
                }

                return Json("Success");
            }

            return Json("No Records Found");
        }
        catch (Exception ex)
        {
            return RedirectToAction("Error", "Error", new { errorCode = "", errorMessage = ex.Message });
        }
    }

    public async Task<IActionResult> DeleteApiResources(Guid id)
    {
        try
        {
            var result =
                await httpService.PostSecureAsync<FrameworkResult>(ApiRoutePathConstants.DeleteApiResourceById, id);
            if (result.Status == ResultStatus.Failed)
                //ModelState.AddModelError("", "Api Resource deletion failed");
                return Json(false);

            //TempData["Message"] = "Api Resource deleted successfully!";
            return Json(true);
        }
        catch (Exception ex)
        {
            return RedirectToAction("Error", "Error", new { errorCode = "", errorMessage = ex.Message });
        }
    }

    [Authorize]
    public IActionResult ApiScope(Guid apiResourceId, string apiResourceName, string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        ViewData["resourceid"] = apiResourceId;
        ViewData["resourcename"] = apiResourceName;
        return View();
    }

    public async Task<IActionResult> GetApiScopeByResourceId(JqGridRequest request, int? rowId, Guid apiResourceId)
    {
        try
        {
            var apiresource =
                await httpService.PostSecureAsync<ApiResourcesModel>(ApiRoutePathConstants.GetApiResourceById,
                    apiResourceId);
            if (apiresource != null)
            {
                var apiscopeList = apiresource.ApiScopes;
                JqGridResponse response = null;
                if (apiscopeList != null && apiscopeList.ContainsAny())
                {
                    var totalRecords = apiscopeList.Count();
                    response = new JqGridResponse
                    {
                        TotalPagesCount = (int)Math.Ceiling(totalRecords / (float)request.RecordsCount),
                        PageIndex = request.PageIndex,
                        TotalRecordsCount = totalRecords
                    };

                    apiscopeList = apiscopeList.OrderBy(x => x.Name).ToList();
                    foreach (var scope in apiscopeList.Skip(request.PageIndex * request.RecordsCount)
                                 .Take(request.PagesCount * request.RecordsCount))
                        response.Records.Add(new JqGridRecord(Convert.ToString(scope.Id), scope));
                }
                else
                {
                    response = new JqGridResponse
                    {
                        TotalPagesCount = 0,
                        PageIndex = request.PageIndex,
                        TotalRecordsCount = 0
                    };
                }

                response.Reader.RepeatItems = false;
                return new JqGridJsonResult(response);
            }

            return View();
        }
        catch (Exception ex)
        {
            return RedirectToAction("Error", "Error", new { errorCode = "", errorMessage = ex.Message });
        }
    }

    public async Task<IActionResult> UpdateApiScope(ApiScopesModel apiScopeModel, Guid apiResourceId)
    {
        try
        {
            if (apiScopeModel != null)
            {
                ApiScopesModel apiScope = null;
                if (apiScopeModel.Id != Guid.Empty && apiScopeModel.Id != default)
                    apiScope = await httpService.PostSecureAsync<ApiScopesModel>(ApiRoutePathConstants.GetApiScopeById,
                        apiScopeModel.Id);

                if (apiScope == null)
                {
                    apiScopeModel.Required = true;
                    apiScopeModel.CreatedBy = User.Identity.Name;
                    apiScopeModel.ApiResourceId = apiResourceId;
                    var result =
                        await httpService.PostSecureAsync<FrameworkResult>(ApiRoutePathConstants.AddApiScope,
                            apiScopeModel);
                    if (result.Status == ResultStatus.Failed) return Json(result.Errors.FirstOrDefault().Description);
                }
                else
                {
                    apiScopeModel.ApiResourceId = apiResourceId;
                    apiScopeModel.CreatedBy = apiScope.CreatedBy;
                    apiScopeModel.CreatedOn = apiScope.CreatedOn;
                    apiScopeModel.Required = apiScope.Required;
                    apiScopeModel.ModifiedBy = User.Identity.Name;
                    var result =
                        await httpService.PostSecureAsync<FrameworkResult>(ApiRoutePathConstants.UpdateApiScope,
                            apiScopeModel);
                    if (result.Status == ResultStatus.Failed) return Json(result.Errors.FirstOrDefault().Description);
                }

                return Json("Success");
            }

            return Json("No records found.");
        }
        catch (Exception ex)
        {
            return RedirectToAction("Error", "Error", new { errorCode = "", errorMessage = ex.Message });
        }
    }

    public async Task<IActionResult> DeleteApiScope(Guid id)
    {
        try
        {
            var result =
                await httpService.PostSecureAsync<FrameworkResult>(ApiRoutePathConstants.DeleteApiScopeById, id);
            if (result.Status == ResultStatus.Failed) return Json(false);

            return Json(true);
        }
        catch (Exception ex)
        {
            return RedirectToAction("Error", "Error", new { errorCode = "", errorMessage = ex.Message });
        }
    }

    [Authorize]
    public IActionResult ApiResourceClaims(Guid apiResourceId, string apiResourceName, string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        ViewData["resourceid"] = apiResourceId;
        ViewData["resourcename"] = apiResourceName;
        return View();
    }

    public async Task<IActionResult> GetApiResourceClaimsByResourceId(JqGridRequest request, int? rowId,
        Guid apiResourceId)
    {
        try
        {
            var apiresource =
                await httpService.PostSecureAsync<ApiResourcesModel>(ApiRoutePathConstants.GetApiResourceById,
                    apiResourceId);
            if (apiresource != null)
            {
                var apiResourceClaimsList = apiresource.ApiResourceClaims;
                JqGridResponse response = null;
                if (apiResourceClaimsList != null && apiResourceClaimsList.ContainsAny())
                {
                    var totalRecords = apiResourceClaimsList.Count();
                    response = new JqGridResponse
                    {
                        TotalPagesCount = (int)Math.Ceiling(totalRecords / (float)request.RecordsCount),
                        PageIndex = request.PageIndex,
                        TotalRecordsCount = totalRecords
                    };

                    apiResourceClaimsList = apiResourceClaimsList.OrderBy(x => x.Type).ToList();
                    foreach (var scope in apiResourceClaimsList.Skip(request.PageIndex * request.RecordsCount)
                                 .Take(request.PagesCount * request.RecordsCount))
                        response.Records.Add(new JqGridRecord(Convert.ToString(scope.Id), scope));
                }
                else
                {
                    response = new JqGridResponse
                    {
                        TotalPagesCount = 0,
                        PageIndex = request.PageIndex,
                        TotalRecordsCount = 0
                    };
                }

                response.Reader.RepeatItems = false;
                return new JqGridJsonResult(response);
            }

            return View();
        }
        catch (Exception ex)
        {
            return RedirectToAction("Error", "Error", new { errorCode = "", errorMessage = ex.Message });
        }
    }

    public async Task<IActionResult> ManageApiResourceClaims(ApiResourceClaimsModel apiResourceClaimsModel,
        Guid apiResourceId)
    {
        try
        {
            if (apiResourceClaimsModel != null)
            {
                if (apiResourceClaimsModel.Id != Guid.Empty && apiResourceClaimsModel.Id != default)
                {
                    var result = await httpService.PostSecureAsync<FrameworkResult>(
                        ApiRoutePathConstants.DeleteApiResourceClaimById, apiResourceClaimsModel.Id);
                    if (result.Status == ResultStatus.Failed) return Json(result.Errors.FirstOrDefault().Description);
                }
                else
                {
                    apiResourceClaimsModel.CreatedBy = User.Identity.Name;
                    apiResourceClaimsModel.ApiResourceId = apiResourceId;
                    var result =
                        await httpService.PostSecureAsync<FrameworkResult>(ApiRoutePathConstants.AddApiResourceClaim,
                            apiResourceClaimsModel);
                    if (result.Status == ResultStatus.Failed) return Json(result.Errors.FirstOrDefault().Description);
                }

                return Json("Success");
            }

            return Json("No Records found");
        }
        catch (Exception ex)
        {
            return RedirectToAction("Error", "Error", new { errorCode = "", errorMessage = ex.Message });
        }
    }

    [Authorize]
    public IActionResult ApiScopeClaims(Guid apiScopeId, string apiScopeName, string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        ViewData["scopeid"] = apiScopeId;
        ViewData["scopename"] = apiScopeName;
        return View();
    }

    public async Task<IActionResult> GetApiScopeClaimsByScopeId(JqGridRequest request, int? rowId, Guid apiScopeId)
    {
        try
        {
            var apiScope =
                await httpService.PostSecureAsync<ApiScopesModel>(ApiRoutePathConstants.GetApiScopeById, apiScopeId);
            if (apiScope != null)
            {
                var apiScopeClaimsList = apiScope.ApiScopeClaims;
                JqGridResponse response = null;
                if (apiScopeClaimsList != null && apiScopeClaimsList.ContainsAny())
                {
                    var totalRecords = apiScopeClaimsList.Count();
                    response = new JqGridResponse
                    {
                        TotalPagesCount = (int)Math.Ceiling(totalRecords / (float)request.RecordsCount),
                        PageIndex = request.PageIndex,
                        TotalRecordsCount = totalRecords
                    };

                    apiScopeClaimsList = apiScopeClaimsList.OrderBy(x => x.Type).ToList();
                    foreach (var scope in apiScopeClaimsList.Skip(request.PageIndex * request.RecordsCount)
                                 .Take(request.PagesCount * request.RecordsCount))
                        response.Records.Add(new JqGridRecord(Convert.ToString(scope.Id), scope));
                }
                else
                {
                    response = new JqGridResponse
                    {
                        TotalPagesCount = 0,
                        PageIndex = request.PageIndex,
                        TotalRecordsCount = 0
                    };
                }

                response.Reader.RepeatItems = false;
                return new JqGridJsonResult(response);
            }

            return View();
        }
        catch (Exception ex)
        {
            return RedirectToAction("Error", "Error", new { errorCode = "", errorMessage = ex.Message });
        }
    }

    public async Task<IActionResult> ManageApiScopeClaims(ApiScopeClaimsModel apiScopeClaimsModel, Guid apiScopeId)
    {
        try
        {
            if (apiScopeClaimsModel != null)
            {
                if (apiScopeClaimsModel.Id != Guid.Empty && apiScopeClaimsModel.Id != default)
                {
                    var result =
                        await httpService.PostSecureAsync<FrameworkResult>(
                            ApiRoutePathConstants.DeleteApiScopeClaimById, apiScopeClaimsModel.Id);
                    if (result.Status == ResultStatus.Failed) return Json(result.Errors.FirstOrDefault().Description);
                }
                else
                {
                    apiScopeClaimsModel.CreatedBy = User.Identity.Name;
                    apiScopeClaimsModel.ApiScopeId = apiScopeId;
                    var result =
                        await httpService.PostSecureAsync<FrameworkResult>(ApiRoutePathConstants.AddApiScopeClaim,
                            apiScopeClaimsModel);
                    if (result.Status == ResultStatus.Failed) return Json(result.Errors.FirstOrDefault().Description);
                }

                return Json("Success");
            }

            return Json("No records found");
        }
        catch (Exception ex)
        {
            return RedirectToAction("Error", "Error", new { errorCode = "", errorMessage = ex.Message });
        }
    }

    [Authorize]
    public IActionResult IdentityResource(string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    public async Task<IActionResult> GetAllIdentityResources(JqGridRequest request, int? rowId)
    {
        try
        {
            var resourceList =
                await httpService.PostSecureAsync<IList<IdentityResourcesModel>>(
                    ApiRoutePathConstants.GetAllIdentityResources, string.Empty);
            if (resourceList != null)
            {
                var totalRecords = resourceList.Count();

                var response = new JqGridResponse
                {
                    TotalPagesCount = (int)Math.Ceiling(totalRecords / (float)request.RecordsCount),
                    PageIndex = request.PageIndex,
                    TotalRecordsCount = totalRecords
                };

                resourceList = resourceList.OrderBy(x => x.Name).ToList();
                foreach (var resource in resourceList.Skip(request.PageIndex * request.RecordsCount)
                             .Take(request.PagesCount * request.RecordsCount))
                    response.Records.Add(new JqGridRecord(Convert.ToString(resource.Id), resource));

                response.Reader.RepeatItems = false;
                return new JqGridJsonResult(response);
            }

            return View();
        }
        catch (Exception ex)
        {
            return RedirectToAction("Error", "Error", new { errorCode = "", errorMessage = ex.Message });
        }
    }

    public async Task<IActionResult> UpdateIdentityResources(IdentityResourcesModel identityResourcesModel)
    {
        try
        {
            if (identityResourcesModel != null)
            {
                IdentityResourcesModel identityResource = null;
                if (identityResourcesModel.Id != Guid.Empty && identityResourcesModel.Id != default)
                    identityResource =
                        await httpService.PostSecureAsync<IdentityResourcesModel>(
                            ApiRoutePathConstants.GetIdentityResourceById, identityResourcesModel.Id);

                if (identityResource == null)
                {
                    identityResourcesModel.Enabled = true;
                    identityResourcesModel.Required = true;
                    identityResourcesModel.Emphasize = false;
                    identityResourcesModel.CreatedBy = User.Identity.Name;
                    var result =
                        await httpService.PostSecureAsync<FrameworkResult>(ApiRoutePathConstants.AddIdentityResource,
                            identityResourcesModel);
                    if (result.Status == ResultStatus.Failed)
                        //ModelState.AddModelError("", "Api Resource insert failed");
                        return Json(result.Errors.FirstOrDefault().Description);

                    //TempData["Message"] = "Api Resource created successfully!";
                }
                else
                {
                    identityResource.Name = identityResourcesModel.Name;
                    identityResource.DisplayName = identityResourcesModel.DisplayName;
                    identityResource.Description = identityResourcesModel.Description;
                    identityResource.ModifiedBy = User.Identity.Name;
                    var result =
                        await httpService.PostSecureAsync<FrameworkResult>(ApiRoutePathConstants.UpdateIdentityResource,
                            identityResource);
                    if (result.Status == ResultStatus.Failed)
                        //ModelState.AddModelError("", "Api Resource update failed");
                        return Json(result.Errors.FirstOrDefault().Description);

                    //TempData["Message"] = "Api Resource updated successfully!";
                }

                return Json("Success");
            }

            return Json("No records found");
        }
        catch (Exception ex)
        {
            return RedirectToAction("Error", "Error", new { errorCode = "", errorMessage = ex.Message });
        }
    }

    public async Task<IActionResult> DeleteIdentityResources(Guid id)
    {
        try
        {
            var result =
                await httpService.PostSecureAsync<FrameworkResult>(ApiRoutePathConstants.DeleteIdentityResourceById,
                    id);
            if (result.Status == ResultStatus.Failed)
                //ModelState.AddModelError("", "Api Resource deletion failed");
                return Json(false);

            //TempData["Message"] = "Api Resource deleted successfully!";
            return Json(true);
        }
        catch (Exception ex)
        {
            return RedirectToAction("Error", "Error", new { errorCode = "", errorMessage = ex.Message });
        }
    }

    [Authorize]
    public IActionResult IdentityResourceClaims(Guid identityResourceId, string identityResourceName,
        string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        ViewData["resourceid"] = identityResourceId;
        ViewData["resourcename"] = identityResourceName;
        return View();
    }

    public async Task<IActionResult> GetIdentityResourceClaimsById(JqGridRequest request, int? rowId,
        Guid identityResourceId)
    {
        try
        {
            var identityResource =
                await httpService.PostSecureAsync<IdentityResourcesModel>(ApiRoutePathConstants.GetIdentityResourceById,
                    identityResourceId);
            if (identityResource != null)
            {
                var identityResourceClaimsList = identityResource.IdentityClaims;
                JqGridResponse response = null;
                if (identityResourceClaimsList != null && identityResourceClaimsList.ContainsAny())
                {
                    var totalRecords = identityResourceClaimsList.Count();
                    response = new JqGridResponse
                    {
                        TotalPagesCount = (int)Math.Ceiling(totalRecords / (float)request.RecordsCount),
                        PageIndex = request.PageIndex,
                        TotalRecordsCount = totalRecords
                    };

                    identityResourceClaimsList = identityResourceClaimsList.OrderBy(x => x.Type).ToList();
                    foreach (var scope in identityResourceClaimsList.Skip(request.PageIndex * request.RecordsCount)
                                 .Take(request.PagesCount * request.RecordsCount))
                        response.Records.Add(new JqGridRecord(Convert.ToString(scope.Id), scope));
                }
                else
                {
                    response = new JqGridResponse
                    {
                        TotalPagesCount = 0,
                        PageIndex = request.PageIndex,
                        TotalRecordsCount = 0
                    };
                }

                response.Reader.RepeatItems = false;
                return new JqGridJsonResult(response);
            }

            return View();
        }
        catch (Exception ex)
        {
            return RedirectToAction("Error", "Error", new { errorCode = "", errorMessage = ex.Message });
        }
    }

    public async Task<IActionResult> ManageIdentityResourceClaims(IdentityClaimsModel identityResourceClaimsModel,
        Guid identityResourceId)
    {
        try
        {
            if (identityResourceClaimsModel != null)
            {
                if (identityResourceClaimsModel.Id != Guid.Empty && identityResourceClaimsModel.Id != default)
                {
                    var result = await httpService.PostSecureAsync<FrameworkResult>(
                        ApiRoutePathConstants.DeleteIdentityResourceClaimById, identityResourceClaimsModel.Id);
                    if (result.Status == ResultStatus.Failed) return Json(result.Errors.FirstOrDefault().Description);
                }
                else
                {
                    identityResourceClaimsModel.CreatedBy = User.Identity.Name;
                    identityResourceClaimsModel.IdentityResourceId = identityResourceId;
                    var result = await httpService.PostSecureAsync<FrameworkResult>(
                        ApiRoutePathConstants.AddIdentityResourceClaim, identityResourceClaimsModel);
                    if (result.Status == ResultStatus.Failed) return Json(result.Errors.FirstOrDefault().Description);
                }

                return Json("Success");
            }

            return Json("No records found.");
        }
        catch (Exception ex)
        {
            return RedirectToAction("Error", "Error", new { errorCode = "", errorMessage = ex.Message });
        }
    }

    [Authorize]
    public IActionResult Roles(string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    public async Task<IActionResult> GetAllRoles(JqGridRequest request, int? rowId)
    {
        try
        {
            var roles = await httpService.PostSecureAsync<IList<RoleModel>>(ApiRoutePathConstants.GetAllRoles,
                string.Empty);
            if (roles != null)
            {
                var totalRecords = roles.Count();

                var response = new JqGridResponse
                {
                    TotalPagesCount = (int)Math.Ceiling(totalRecords / (float)request.RecordsCount),
                    PageIndex = request.PageIndex,
                    TotalRecordsCount = totalRecords
                };

                roles = roles.OrderBy(x => x.Name).ToList();
                foreach (var role in roles.Skip(request.PageIndex * request.RecordsCount)
                             .Take(request.PagesCount * request.RecordsCount))
                    response.Records.Add(new JqGridRecord(Convert.ToString(role.Id), role));

                response.Reader.RepeatItems = false;
                return new JqGridJsonResult(response);
            }

            return View();
        }
        catch (Exception ex)
        {
            return RedirectToAction("Error", "Error", new { errorCode = "", errorMessage = ex.Message });
        }
    }

    public async Task<IActionResult> UpdateRoles(RoleModel roleModel)
    {
        try
        {
            if (roleModel != null)
            {
                RoleModel roles = null;
                if (roleModel.Id != Guid.Empty && roleModel.Id != default)
                    roles = await httpService.PostSecureAsync<RoleModel>(ApiRoutePathConstants.GetRoleById,
                        roleModel.Id);

                if (roles == null)
                {
                    roleModel.CreatedBy = User.Identity.Name;
                    var result =
                        await httpService.PostSecureAsync<FrameworkResult>(ApiRoutePathConstants.CreateRole, roleModel);
                    if (result.Status == ResultStatus.Failed)
                        //ModelState.AddModelError("", "Api Resource insert failed");
                        return Json(result.Errors.FirstOrDefault().Description);

                    //TempData["Message"] = "Api Resource created successfully!";
                }
                else
                {
                    roleModel.CreatedBy = roles.CreatedBy;
                    roleModel.CreatedOn = roles.CreatedOn;
                    roleModel.ModifiedBy = User.Identity.Name;
                    var result =
                        await httpService.PostSecureAsync<FrameworkResult>(ApiRoutePathConstants.UpdateRole, roleModel);
                    if (result.Status == ResultStatus.Failed)
                        //ModelState.AddModelError("", "Api Resource update failed");
                        return Json(result.Errors.FirstOrDefault().Description);

                    //TempData["Message"] = "Api Resource updated successfully!";
                }

                return Json("Success");
            }

            return Json("No records found.");
        }
        catch (Exception ex)
        {
            return RedirectToAction("Error", "Error", new { errorCode = "", errorMessage = ex.Message });
        }
    }

    public async Task<IActionResult> DeleteRoles(Guid id)
    {
        try
        {
            var result = await httpService.PostSecureAsync<FrameworkResult>(ApiRoutePathConstants.DeleteRoleById, id);
            if (result.Status == ResultStatus.Failed)
                //ModelState.AddModelError("", "Api Resource deletion failed");
                return Json(false);

            //TempData["Message"] = "Api Resource deleted successfully!";
            return Json(true);
        }
        catch (Exception ex)
        {
            return RedirectToAction("Error", "Error", new { errorCode = "", errorMessage = ex.Message });
        }
    }

    [Authorize]
    public IActionResult RoleClaims(Guid roleId, string roleName, string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        ViewData["roleId"] = roleId;
        ViewData["roleName"] = roleName;
        return View();
    }

    public async Task<IActionResult> GetRoleClaimsById(JqGridRequest request, int? rowId, Guid roleId)
    {
        try
        {
            var roles = await httpService.PostSecureAsync<RoleModel>(ApiRoutePathConstants.GetRoleById, roleId);
            if (roles != null)
            {
                var roleClaimsList = roles.RoleClaims;
                JqGridResponse response = null;
                if (roleClaimsList != null && roleClaimsList.ContainsAny())
                {
                    var totalRecords = roleClaimsList.Count();
                    response = new JqGridResponse
                    {
                        TotalPagesCount = (int)Math.Ceiling(totalRecords / (float)request.RecordsCount),
                        PageIndex = request.PageIndex,
                        TotalRecordsCount = totalRecords
                    };

                    roleClaimsList = roleClaimsList.OrderBy(x => x.ClaimType).ToList();
                    foreach (var roleClaim in roleClaimsList.Skip(request.PageIndex * request.RecordsCount)
                                 .Take(request.PagesCount * request.RecordsCount))
                        response.Records.Add(new JqGridRecord(Convert.ToString(roleClaim.Id), roleClaim));
                }
                else
                {
                    response = new JqGridResponse
                    {
                        TotalPagesCount = 0,
                        PageIndex = request.PageIndex,
                        TotalRecordsCount = 0
                    };
                }

                response.Reader.RepeatItems = false;
                return new JqGridJsonResult(response);
            }

            return View();
        }
        catch (Exception ex)
        {
            return RedirectToAction("Error", "Error", new { errorCode = "", errorMessage = ex.Message });
        }
    }

    public async Task<IActionResult> ManageRoleClaims(RoleClaimModel roleClaimsModel, Guid roleId)
    {
        try
        {
            if (roleClaimsModel != null)
            {
                if (roleClaimsModel.Id > 0)
                {
                    var result =
                        await httpService.PostSecureAsync<FrameworkResult>(ApiRoutePathConstants.RemoveRoleClaimsById,
                            roleClaimsModel.Id);
                    if (result.Status == ResultStatus.Failed) return Json(result.Errors.FirstOrDefault().Description);
                }
                else
                {
                    roleClaimsModel.CreatedBy = User.Identity.Name;
                    roleClaimsModel.RoleId = roleId;
                    var result =
                        await httpService.PostSecureAsync<FrameworkResult>(ApiRoutePathConstants.AddRoleClaim,
                            roleClaimsModel);
                    if (result.Status == ResultStatus.Failed) return Json(result.Errors.FirstOrDefault().Description);
                }

                return Json("Success");
            }

            return Json("No records found.");
        }
        catch (Exception ex)
        {
            return RedirectToAction("Error", "Error", new { errorCode = "", errorMessage = ex.Message });
        }
    }

    [Authorize]
    public IActionResult Client(string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [Authorize]
    public IActionResult UserRole(string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    public async Task<IActionResult> GetAllClients(JqGridRequest request, int? rowId)
    {
        try
        {
            var clients =
                await httpService.PostSecureAsync<Dictionary<string, string>>(ApiRoutePathConstants.GetAllClient,
                    string.Empty);
            if (clients != null)
            {
                var ClientsModel = new List<ClientViewModel>();
                var i = 1;
                foreach (var client in clients)
                {
                    ClientsModel.Add(new ClientViewModel { Id = i, ClientId = client.Key, ClientName = client.Value });
                    i++;
                }

                var totalRecords = ClientsModel.Count();

                var response = new JqGridResponse
                {
                    TotalPagesCount = (int)Math.Ceiling(totalRecords / (float)request.RecordsCount),
                    PageIndex = request.PageIndex,
                    TotalRecordsCount = totalRecords
                };

                foreach (var client in ClientsModel.Skip(request.PageIndex * request.RecordsCount)
                             .Take(request.PagesCount * request.RecordsCount))
                    response.Records.Add(new JqGridRecord(Convert.ToString(client.ClientId), client));

                response.Reader.RepeatItems = false;
                return new JqGridJsonResult(response);
            }

            return View();
        }
        catch (Exception ex)
        {
            return RedirectToAction("Error", "Error", new { errorCode = "", errorMessage = ex.Message });
        }
    }

    public async Task<IActionResult> GetAllUsers(JqGridRequest request, int? rowId)
    {
        try
        {
            var users = await httpService.PostSecureAsync<IList<UserDisplayModel>>(ApiRoutePathConstants.GetAllUsers,
                string.Empty);
            if (users != null)
            {
                var userRoleViewModels = new List<UserViewModel>();

                foreach (var item in users)
                    userRoleViewModels.Add(new UserViewModel
                    {
                        UserId = item.Id.ToString(), UserName = item.UserName,
                        AccountStatus = item.LockoutEnabled ? "In-Active" : "Active"
                    });
                var totalRecords = userRoleViewModels.Count();
                var response = new JqGridResponse
                {
                    TotalPagesCount = (int)Math.Ceiling(totalRecords / (float)request.RecordsCount),
                    PageIndex = request.PageIndex,
                    TotalRecordsCount = totalRecords
                };

                userRoleViewModels = userRoleViewModels.OrderBy(x => x.UserName).ToList();
                foreach (var user in userRoleViewModels.Skip(request.PageIndex * request.RecordsCount)
                             .Take(request.PagesCount * request.RecordsCount))
                    response.Records.Add(new JqGridRecord(Convert.ToString(user.UserId), user));

                response.Reader.RepeatItems = false;
                return new JqGridJsonResult(response);
            }

            return View();
        }
        catch (Exception ex)
        {
            return RedirectToAction("Error", "Error", new { errorCode = "", errorMessage = ex.Message });
        }
    }

    [Authorize]
    public IActionResult AddClient(string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    //[Authorize(Roles = "HCLCSSFAdmin, HCLCSSFUser")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> AddClient(ManageClientViewModel model, string returnUrl = null)
    {
        try
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var clientsModel = new ClientsModel
                {
                    CreatedBy = User.Identity.Name,
                    ClientName = model.ClientName,
                    ClientUri = model.ClientUri,
                    LogoUri = model.LogoUri,
                    TermsOfServiceUri = model.TermsOfServiceUri,
                    PolicyUri = model.PolicyUri,
                    RefreshTokenExpiration = model.RefreshTokenExpiration,
                    AccessTokenExpiration = model.AccessTokenExpiration,
                    IdentityTokenExpiration = model.IdentityTokenExpiration,
                    LogoutTokenExpiration = model.LogoutTokenExpiration,
                    AuthorizationCodeExpiration = model.AuthorizationCodeExpiration,
                    AccessTokenType = AccessTokenType.JWT,
                    RequirePkce = true,
                    IsPkceTextPlain = false,
                    RequireClientSecret = true,
                    IsFirstPartyApp = true,
                    AllowOfflineAccess = true,
                    AllowAccessTokensViaBrowser = false,
                    ApplicationType = ApplicationType.RegularWeb,
                    AllowedSigningAlgorithm = model.AllowedSigningAlgorithm,
                    FrontChannelLogoutSessionRequired = true,
                    BackChannelLogoutSessionRequired = true,
                    FrontChannelLogoutUri = model.FrontChannelLogoutUri,
                    BackChannelLogoutUri = model.BackChannelLogoutUri
                };

                clientsModel.SupportedGrantTypes = NormalizeGrantTypes(model.SupportedGrantTypes);
                clientsModel.SupportedResponseTypes = NormalizeResponseTypes();

                clientsModel.AllowedScopes = new List<string>();
                clientsModel.AllowedScopes = model.AllowedScopes.Split(' ').ToList();

                var client =
                    await httpService.PostSecureAsync<ClientsModel>(ApiRoutePathConstants.RegisterClient, clientsModel);
                if (!string.IsNullOrWhiteSpace(client.ClientId))
                {
                    ViewData["clientid"] = client.ClientId;
                    ViewData["clientsecret"] = client.ClientSecret;
                }
            }

            return View();
        }
        catch (Exception ex)
        {
            return RedirectToAction("Error", "Error", new { errorCode = "", errorMessage = ex.Message });
        }
    }

    [Authorize]
    public async Task<ActionResult> UpdateClient(string clientId, string returnUrl = null)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(clientId))
            {
                var clients =
                    await httpService.PostSecureAsync<ClientsModel>(ApiRoutePathConstants.GetClient, clientId);
                if (clients != null)
                {
                    var clientsModel = new ManageClientViewModel
                    {
                        ClientName = clients.ClientName,
                        ClientUri = clients.ClientUri,
                        LogoUri = clients.LogoUri,
                        TermsOfServiceUri = clients.TermsOfServiceUri,
                        PolicyUri = clients.PolicyUri,
                        RefreshTokenExpiration = clients.RefreshTokenExpiration,
                        AccessTokenExpiration = clients.AccessTokenExpiration,
                        IdentityTokenExpiration = clients.IdentityTokenExpiration,
                        LogoutTokenExpiration = clients.LogoutTokenExpiration,
                        AuthorizationCodeExpiration = clients.AuthorizationCodeExpiration,
                        AllowedSigningAlgorithm = clients.AllowedSigningAlgorithm,
                        FrontChannelLogoutUri = clients.FrontChannelLogoutUri,
                        BackChannelLogoutUri = clients.BackChannelLogoutUri,
                        SupportedGrantTypes = string.Join(" ", clients.SupportedGrantTypes.ToArray()),
                        SupportedResponseTypes = string.Join(" ", clients.SupportedResponseTypes.ToArray()),
                        AllowedScopes = string.Join(" ", clients.AllowedScopes.ToArray())
                    };

                    return View(clientsModel);
                }
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }
        catch (Exception ex)
        {
            return RedirectToAction("Error", "Error", new { errorCode = "", errorMessage = ex.Message });
        }
    }


    [Authorize]
    [HttpGet]
    public IActionResult ManageRoles(string userId)
    {
        try
        {
            var getUserroles = httpService.PostSecureAsync<List<string>>(ApiRoutePathConstants.GetUserRoles, userId)
                .GetAwaiter().GetResult();
            var getallRoles = httpService
                .PostSecureAsync<List<RoleModel>>(ApiRoutePathConstants.GetAllRoles, string.Empty).GetAwaiter()
                .GetResult();
            var manageRoles = new List<ManageRoleModel>();
            foreach (var item in getallRoles)
                if (getUserroles != null && getUserroles.Contains(item.Name))
                    manageRoles.Add(new ManageRoleModel
                        { ClaimType = item.Id.ToString(), ClaimValue = item.Name, IsChecked = true });
                else
                    manageRoles.Add(new ManageRoleModel
                        { ClaimType = item.Id.ToString(), ClaimValue = item.Name, IsChecked = false });

            TempData["UserId"] = userId;
            return View(manageRoles);
        }
        catch (Exception ex)
        {
            return RedirectToAction("Error", "Error", new { errorCode = "", errorMessage = ex.Message });
        }
    }

    [Authorize]
    public IActionResult ManageClaims(string userId, string returnUrl = null)
    {
        TempData["ClaimUserId"] = userId;
        return View();
    }

    public async Task<IActionResult> GetUserClaimsByUserId(JqGridRequest request, string claimUserId)
    {
        try
        {
            var userId = claimUserId;

            var getUserClaims =
                await httpService.PostSecureAsync<List<UserClaimModel>>(ApiRoutePathConstants.GetAdminUserClaims,
                    userId);

            if (getUserClaims != null)
            {
                JqGridResponse response = null;
                if (getUserClaims != null && getUserClaims.ContainsAny())
                {
                    var totalRecords = getUserClaims.Count();
                    response = new JqGridResponse
                    {
                        TotalPagesCount = (int)Math.Ceiling(totalRecords / (float)request.RecordsCount),
                        PageIndex = request.PageIndex,
                        TotalRecordsCount = totalRecords
                    };

                    getUserClaims = getUserClaims.OrderBy(x => x.ClaimType).ToList();
                    foreach (var scope in getUserClaims.Skip(request.PageIndex * request.RecordsCount)
                                 .Take(request.PagesCount * request.RecordsCount))
                        response.Records.Add(new JqGridRecord(Convert.ToString(scope.Id), scope));
                }
                else
                {
                    response = new JqGridResponse
                    {
                        TotalPagesCount = 0,
                        PageIndex = request.PageIndex,
                        TotalRecordsCount = 0
                    };
                }

                response.Reader.RepeatItems = false;
                TempData["ClaimUserId"] = userId;
                return new JqGridJsonResult(response);
            }

            return View();
        }
        catch (Exception ex)
        {
            return RedirectToAction("Error", "Error", new { errorCode = "", errorMessage = ex.Message });
        }
    }

    public async Task<IActionResult> UpdateUserClaim(UserClaimModel userClaimmodel, string claimUserId)
    {
        try
        {
            var userClaimInputModel = new UserClaimModel();
            var userId = claimUserId;

            if (userClaimmodel != null)
            {
                if (userClaimmodel.Id > 0 && userClaimmodel.Id != default)
                {
                    var getUserClaims =
                        await httpService.PostSecureAsync<List<UserClaimModel>>(
                            ApiRoutePathConstants.GetAdminUserClaims, userId);

                    var getCurrentClaim = getUserClaims.Find(x => x.Id == Convert.ToInt32(userClaimmodel.Id));


                    userClaimInputModel.UserId = new Guid(userId);
                    userClaimInputModel.IsAdminClaim = true;
                    userClaimInputModel.ClaimType = getCurrentClaim.ClaimType;
                    userClaimInputModel.ClaimValue = getCurrentClaim.ClaimValue;
                    userClaimInputModel.Id = getCurrentClaim.Id;
                    var result =
                        await httpService.PostSecureAsync<FrameworkResult>(ApiRoutePathConstants.RemoveAdminClaim,
                            userClaimInputModel);
                    if (result.Status == ResultStatus.Failed) return Json(false);
                }
                else
                {
                    userClaimInputModel.CreatedBy = User.Identity.Name;
                    userClaimInputModel.UserId = new Guid(userId);
                    userClaimInputModel.ClaimType = userClaimmodel.ClaimType;
                    userClaimInputModel.ClaimValue = userClaimmodel.ClaimValue;
                    userClaimInputModel.IsAdminClaim = true;
                    var result = await httpService.PostSecureAsync<FrameworkResult>(ApiRoutePathConstants.AddAdminClaim,
                        userClaimInputModel);
                    if (result.Status == ResultStatus.Failed) return Json(false);
                    TempData["ClaimUserId"] = userClaimInputModel.UserId;
                }

                TempData["msg"] = "UserClaim changes updated Successfully";
                return RedirectToAction("UserRole", "ManageApi");
            }

            return Json(false);
        }
        catch (Exception ex)
        {
            return RedirectToAction("Error", "Error", new { errorCode = "", errorMessage = ex.Message });
        }
    }

    public async Task<IActionResult> UpdateAccountStatus(string userId, string dataValue, string returnUrl = null)
    {
        try
        {
            if (dataValue == "In-Active")
            {
                var getUser = await httpService.PostSecureAsync<UserModel>(ApiRoutePathConstants.GetUserById, userId);
                var lockResponse =
                    await httpService.PostSecureAsync<FrameworkResult>(ApiRoutePathConstants.UnLockUser,
                        getUser.UserName);
                if (lockResponse.Status == ResultStatus.Succeeded)
                    TempData["msg"] = "Successfully unlocked the user.";
                else
                    TempData["msg"] = lockResponse.Errors.FirstOrDefault().Description;
            }
            else
            {
                var lockResponse =
                    await httpService.PostSecureAsync<FrameworkResult>(ApiRoutePathConstants.LockUser, userId);
                if (lockResponse.Status == ResultStatus.Succeeded)
                    TempData["msg"] = "Successfully locked the user.";
                else
                    TempData["msg"] = lockResponse.Errors.FirstOrDefault().Description;
            }

            return View("UserRole");
        }
        catch (Exception ex)
        {
            return RedirectToAction("Error", "Error", new { errorCode = "", errorMessage = ex.Message });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult ManageRoles(List<ManageRoleModel> ManageRoles, string userId, string returnUrl = null)
    {
        try
        {
            var currentRoles = httpService.PostSecureAsync<List<string>>(ApiRoutePathConstants.GetUserRoles, userId)
                .GetAwaiter().GetResult();
            var unCheckedRoles = ManageRoles.Where(x => !x.IsChecked).ToList();
            var checkedRoles = ManageRoles.Where(x => x.IsChecked).ToList();
            var userRoles = new List<UserRoleModel>();

            if (currentRoles != null)
            {
                foreach (var item in unCheckedRoles)
                    if (currentRoles.Contains(item.ClaimValue))
                        userRoles.Add(
                            new UserRoleModel { UserId = new Guid(userId), RoleId = new Guid(item.ClaimType) });

                if (userRoles.Count > 0)
                {
                    var result = httpService
                        .PostSecureAsync<FrameworkResult>(ApiRoutePathConstants.RemoveUserRoleList, userRoles)
                        .GetAwaiter().GetResult();

                    userRoles.Clear();
                    TempData["msg"] = "Changes successfully updated.";
                }

                foreach (var item in checkedRoles)
                    if (!currentRoles.Contains(item.ClaimValue))
                        userRoles.Add(
                            new UserRoleModel { UserId = new Guid(userId), RoleId = new Guid(item.ClaimType) });

                if (userRoles.Count > 0)
                {
                    var result = httpService
                        .PostSecureAsync<FrameworkResult>(ApiRoutePathConstants.AddUserRolesList, userRoles)
                        .GetAwaiter().GetResult();

                    userRoles.Clear();
                    TempData["msg"] = "Changes successfully updated.";
                }
            }
            else
            {
                foreach (var item in checkedRoles)
                {
                    userRoles.Add(new UserRoleModel { UserId = new Guid(userId), RoleId = new Guid(item.ClaimType) });
                    var result = httpService
                        .PostSecureAsync<FrameworkResult>(ApiRoutePathConstants.AddUserRolesList, userRoles)
                        .GetAwaiter().GetResult();
                    userRoles.Clear();
                    TempData["msg"] = "User role successfully added.";
                }
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View("UserRole");
        }
        catch (Exception ex)
        {
            return RedirectToAction("Error", "Error", new { errorCode = "", errorMessage = ex.Message });
        }
    }

    [HttpPost]
    //[Authorize(Roles = "HCLCSSFAdmin, HCLCSSFUser")]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> UpdateClient(ManageClientViewModel model, string returnUrl = null)
    {
        try
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var clients =
                    await httpService.PostSecureAsync<ClientsModel>(ApiRoutePathConstants.GetClient, model.ClientId);
                if (clients != null)
                {
                    clients.ClientName = model.ClientName;
                    clients.ClientUri = model.ClientUri;
                    clients.LogoUri = model.LogoUri;
                    clients.TermsOfServiceUri = model.TermsOfServiceUri;
                    clients.PolicyUri = model.PolicyUri;
                    clients.RefreshTokenExpiration = model.RefreshTokenExpiration;
                    clients.AccessTokenExpiration = model.AccessTokenExpiration;
                    clients.IdentityTokenExpiration = model.IdentityTokenExpiration;
                    clients.LogoutTokenExpiration = model.LogoutTokenExpiration;
                    clients.AuthorizationCodeExpiration = model.AuthorizationCodeExpiration;
                    clients.AllowedSigningAlgorithm = model.AllowedSigningAlgorithm;
                    clients.FrontChannelLogoutUri = model.FrontChannelLogoutUri;
                    clients.BackChannelLogoutUri = model.BackChannelLogoutUri;
                    clients.RequirePkce = true;
                    clients.IsPkceTextPlain = false;
                    clients.AllowAccessTokensViaBrowser = false;
                    clients.SupportedGrantTypes = NormalizeGrantTypes(model.SupportedGrantTypes);
                    clients.SupportedResponseTypes = NormalizeResponseTypes();
                    clients.AllowedScopes = model.AllowedScopes.Split(' ').ToList();

                    var client =
                        await httpService.PostSecureAsync<ClientsModel>(ApiRoutePathConstants.UpdateClient, clients);
                    return RedirectToAction("Client", "ManageApi");
                }

                ModelState.AddModelError("", "Invalid client identifier");
            }

            return View();
        }
        catch (Exception ex)
        {
            return RedirectToAction("Error", "Error", new { errorCode = "", errorMessage = ex.Message });
        }
    }

    [Authorize]
    public async Task<ActionResult> DeleteClient(string clientId, string returnUrl = null)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(clientId))
            {
                var result =
                    await httpService.PostSecureAsync<FrameworkResult>(ApiRoutePathConstants.DeleteClient, clientId);
                if (result.Status == ResultStatus.Succeeded) return RedirectToAction("Client", "ManageApi");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }
        catch (Exception ex)
        {
            return RedirectToAction("Error", "Error", new { errorCode = "", errorMessage = ex.Message });
        }
    }

    [Authorize]
    public IActionResult ManageClient(string clientId, string clientName, string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        ViewData["clientId"] = clientId;
        ViewData["clientName"] = clientName;
        return View();
    }

    public async Task<IActionResult> GetClientRedirectUrlByClientId(JqGridRequest request, int? rowId, string clientId)
    {
        try
        {
            var clients = await httpService.PostSecureAsync<ClientsModel>(ApiRoutePathConstants.GetClient, clientId);
            if (clients != null)
            {
                var clientList = clients.RedirectUris;
                JqGridResponse response = null;
                if (clientList != null && clientList.ContainsAny())
                {
                    var totalRecords = clientList.Count();
                    response = new JqGridResponse
                    {
                        TotalPagesCount = (int)Math.Ceiling(totalRecords / (float)request.RecordsCount),
                        PageIndex = request.PageIndex,
                        TotalRecordsCount = totalRecords
                    };

                    clientList = clientList.OrderBy(x => x.RedirectUri).ToList();
                    foreach (var client in clientList.Skip(request.PageIndex * request.RecordsCount)
                                 .Take(request.PagesCount * request.RecordsCount))
                        response.Records.Add(new JqGridRecord(Convert.ToString(client.Id), client));
                }
                else
                {
                    response = new JqGridResponse
                    {
                        TotalPagesCount = 0,
                        PageIndex = request.PageIndex,
                        TotalRecordsCount = 0
                    };
                }

                response.Reader.RepeatItems = false;
                return new JqGridJsonResult(response);
            }

            return View();
        }
        catch (Exception ex)
        {
            return RedirectToAction("Error", "Error", new { errorCode = "", errorMessage = ex.Message });
        }
    }

    public async Task<IActionResult> ManageClientRedirectUrl(ClientRedirectUrisModel clientRedirectUrisModel,
        string clientId)
    {
        try
        {
            if (clientRedirectUrisModel != null)
            {
                var client = await httpService.PostSecureAsync<ClientsModel>(ApiRoutePathConstants.GetClient, clientId);
                if (client != null)
                {
                    if (clientRedirectUrisModel.Id != Guid.Empty && clientRedirectUrisModel.Id != default)
                    {
                        var redirectUri = client.RedirectUris.Find(x => x.Id == clientRedirectUrisModel.Id);
                        if (redirectUri != null)
                        {
                            redirectUri.ModifiedBy = User.Identity.Name;
                            redirectUri.RedirectUri = clientRedirectUrisModel.RedirectUri;
                            await httpService.PostSecureAsync<ClientsModel>(ApiRoutePathConstants.UpdateClient, client);
                            return Json(true);
                        }

                        return Json(false);
                    }

                    clientRedirectUrisModel.CreatedBy = User.Identity.Name;
                    clientRedirectUrisModel.ClientId = client.Id;
                    client.RedirectUris.Add(clientRedirectUrisModel);
                    await httpService.PostSecureAsync<ClientsModel>(ApiRoutePathConstants.UpdateClient, client);

                    return Json(true);
                }
            }

            return Json(false);
        }
        catch (Exception ex)
        {
            return RedirectToAction("Error", "Error", new { errorCode = "", errorMessage = ex.Message });
        }
    }

    public async Task<IActionResult> DeleteClientRedirectUrl(Guid id, string clientId)
    {
        try
        {
            var client = await httpService.PostSecureAsync<ClientsModel>(ApiRoutePathConstants.GetClient, clientId);
            if (client != null)
            {
                var redirectUri = client.RedirectUris.Find(x => x.Id == id);
                if (redirectUri != null)
                {
                    client.RedirectUris.Remove(redirectUri);
                    await httpService.PostSecureAsync<ClientsModel>(ApiRoutePathConstants.UpdateClient, client);
                }
            }

            return Json(true);
        }
        catch (Exception ex)
        {
            return RedirectToAction("Error", "Error", new { errorCode = "", errorMessage = ex.Message });
        }
    }

    public async Task<IActionResult> GetClientPostRedirectUrlByClientId(JqGridRequest request, int? rowId,
        string clientId)
    {
        try
        {
            var clients = await httpService.PostSecureAsync<ClientsModel>(ApiRoutePathConstants.GetClient, clientId);
            if (clients != null)
            {
                var clientList = clients.PostLogoutRedirectUris;
                JqGridResponse response = null;
                if (clientList != null && clientList.ContainsAny())
                {
                    var totalRecords = clientList.Count();
                    response = new JqGridResponse
                    {
                        TotalPagesCount = (int)Math.Ceiling(totalRecords / (float)request.RecordsCount),
                        PageIndex = request.PageIndex,
                        TotalRecordsCount = totalRecords
                    };

                    clientList = clientList.OrderBy(x => x.PostLogoutRedirectUri).ToList();
                    foreach (var client in clientList.Skip(request.PageIndex * request.RecordsCount)
                                 .Take(request.PagesCount * request.RecordsCount))
                        response.Records.Add(new JqGridRecord(Convert.ToString(client.Id), client));
                }
                else
                {
                    response = new JqGridResponse
                    {
                        TotalPagesCount = 0,
                        PageIndex = request.PageIndex,
                        TotalRecordsCount = 0
                    };
                }

                response.Reader.RepeatItems = false;
                return new JqGridJsonResult(response);
            }

            return View();
        }
        catch (Exception ex)
        {
            return RedirectToAction("Error", "Error", new { errorCode = "", errorMessage = ex.Message });
        }
    }

    public async Task<IActionResult> ManageClientPostRedirectUrl(
        ClientPostLogoutRedirectUrisModel clientPostLogoutRedirect, string clientId)
    {
        try
        {
            if (clientPostLogoutRedirect != null)
            {
                var client = await httpService.PostSecureAsync<ClientsModel>(ApiRoutePathConstants.GetClient, clientId);
                if (client != null)
                {
                    if (clientPostLogoutRedirect.Id != Guid.Empty && clientPostLogoutRedirect.Id != default)
                    {
                        var redirectUri = client.PostLogoutRedirectUris.Find(x => x.Id == clientPostLogoutRedirect.Id);
                        if (redirectUri != null)
                        {
                            redirectUri.ModifiedBy = User.Identity.Name;
                            redirectUri.PostLogoutRedirectUri = clientPostLogoutRedirect.PostLogoutRedirectUri;
                            await httpService.PostSecureAsync<ClientsModel>(ApiRoutePathConstants.UpdateClient, client);
                            return Json(true);
                        }

                        return Json(false);
                    }

                    clientPostLogoutRedirect.CreatedBy = User.Identity.Name;
                    clientPostLogoutRedirect.ClientId = client.Id;
                    client.PostLogoutRedirectUris.Add(clientPostLogoutRedirect);
                    await httpService.PostSecureAsync<ClientsModel>(ApiRoutePathConstants.UpdateClient, client);

                    return Json(true);
                }
            }

            return Json(false);
        }
        catch (Exception ex)
        {
            return RedirectToAction("Error", "Error", new { errorCode = "", errorMessage = ex.Message });
        }
    }

    public async Task<IActionResult> DeleteClientPostRedirectUrl(Guid id, string clientId)
    {
        try
        {
            var client = await httpService.PostSecureAsync<ClientsModel>(ApiRoutePathConstants.GetClient, clientId);
            if (client != null)
            {
                var redirectUri = client.PostLogoutRedirectUris.Find(x => x.Id == id);
                if (redirectUri != null)
                {
                    client.PostLogoutRedirectUris.Remove(redirectUri);
                    await httpService.PostSecureAsync<ClientsModel>(ApiRoutePathConstants.UpdateClient, client);
                }
            }

            return Json(true);
        }
        catch (Exception ex)
        {
            return RedirectToAction("Error", "Error", new { errorCode = "", errorMessage = ex.Message });
        }
    }

    private static List<string> NormalizeGrantTypes(string grantTypesRaw)
    {
        var allowedGrantTypes = new HashSet<string>(StringComparer.Ordinal)
        {
            OpenIdConstants.GrantTypes.AuthorizationCode,
            OpenIdConstants.GrantTypes.RefreshToken,
            OpenIdConstants.GrantTypes.ClientCredentials,
            OpenIdConstants.GrantTypes.Password
        };

        var requestedGrantTypes = (grantTypesRaw ?? string.Empty)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(grantType => allowedGrantTypes.Contains(grantType))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (!requestedGrantTypes.Contains(OpenIdConstants.GrantTypes.AuthorizationCode, StringComparer.Ordinal))
            requestedGrantTypes.Insert(0, OpenIdConstants.GrantTypes.AuthorizationCode);

        return requestedGrantTypes;
    }

    private static List<string> NormalizeResponseTypes()
    {
        return new List<string> { OpenIdConstants.ResponseTypes.Code };
    }
}
