/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using HCL.CS.SF.DemoClientWpfApp.Components;
using HCL.CS.SF.DemoClientWpfApp.Constants;
using HCL.CS.SF.DemoClientWpfApp.DomainModel;
using HCL.CS.SF.DemoClientWpfApp.Interface;
using HCL.CS.SF.DemoClientWpfApp.Services;
using HCL.CS.SF.DemoClientWpfApp.ViewModel;

namespace HCL.CS.SF.DemoClientWpfApp.View
{
    public partial class ResourcePopup : Window,IPageViewModel
    {
        ApplicationParameters ApplicationConstants = new ApplicationParameters();
        public ApiScopesModel ApiScope;
        public ApiResourcesModel ApiResources;
        public ApiScopeClaimsModel ApiScopeClaims;
        public ApiResourceClaimsModel ApiResourceClaims;
        public string methodName;
        public ResourcePopup()
        {
            InitializeComponent();
            GetResource();
        }
        public void GetResource()
        {
            methodName = Global.PopupMethodName;
      
         if (methodName == "AddApiResource")
            {
                EnableResourcerInputControle();
                DisableResourcerClaimInputControle();
                ApiResources = Global.ApiResourcesGetData;
                stkUpdate.Visibility = Visibility.Hidden;
                lblName.Content = "Resource Name";
                lblDisplayName.Content = "Resource Display Name";
                lblDescription.Content = "Resource Description";

                txtName.Text = string.Empty;
                txtDisplayName.Text = string.Empty;
                txtDescription.Text = string.Empty;
                stkUpdate.Visibility = Visibility.Collapsed;
                stkAdd.Visibility = Visibility.Visible;
               
            }
            else if (methodName == "UpdateApiResource")
            {
                EnableResourcerInputControle();
                DisableResourcerClaimInputControle();
                ApiResources = Global.ApiResourcesGetData;
                stkUpdate.Visibility = Visibility.Hidden;
                lblName.Content = "Resource Name";
                lblDisplayName.Content = "Resource Display Name";
                lblDescription.Content = "Resource Description";

                txtName.Text = ApiResources.Name;
                txtDisplayName.Text = ApiResources.DisplayName;
                txtDescription.Text = ApiResources.Description;
                stkUpdate.Visibility = Visibility.Visible;
                stkAdd.Visibility = Visibility.Collapsed;

            }
            else if (methodName == "AddApiResourceClaim")
            {
                DisableResourcerInputControle();
                EnableResourcerClaimInputControle();
                ApiResourceClaims = Global.ApiResourceClaimsGetData;
                stkUpdate.Visibility = Visibility.Hidden;
                lblClaimType.Content = "Claim Type";
                txtClaimType.Text = string.Empty;

                stkUpdate.Visibility = Visibility.Collapsed;
                stkAdd.Visibility = Visibility.Visible;
               
            }
            else if (methodName == "UpdateApiResourceClaim")
            {
                DisableResourcerInputControle();
                EnableResourcerClaimInputControle();
                ApiResourceClaims = Global.ApiResourceClaimsGetData;
                stkUpdate.Visibility = Visibility.Hidden;
                lblClaimType.Content = "Claim Type";
                txtClaimType.Text = ApiResourceClaims.Type;

                stkUpdate.Visibility = Visibility.Collapsed;
                stkAdd.Visibility = Visibility.Visible;
              
            }
            else if (methodName == "AddScope")
            {
                EnableResourcerInputControle();
                DisableResourcerClaimInputControle();
                ApiScope = Global.ApiScopeGetData;
                stkUpdate.Visibility = Visibility.Hidden;
                lblName.Content = "Scope Name";
                lblDisplayName.Content = "Scope Display Name";
                lblDescription.Content = "Scope Description";

                txtName.Text = string.Empty;
                txtDisplayName.Text = string.Empty;
                txtDescription.Text = string.Empty;
                stkAdd.Visibility = Visibility.Visible;
                stkUpdate.Visibility = Visibility.Collapsed;

            }
            else  if (methodName == "UpdateScope")
            {
                EnableResourcerInputControle();
                DisableResourcerClaimInputControle();

                ApiScope = Global.ApiScopeGetData;
                stkUpdate.Visibility = Visibility.Hidden;
                lblName.Content = "Scope Name";
                lblDisplayName.Content = "Scope Display Name";
                lblDescription.Content = "Scope Description";

                txtName.Text = ApiScope.Name;
                txtDisplayName.Text = ApiScope.DisplayName;
                txtDescription.Text = ApiScope.Description;
                stkAdd.Visibility = Visibility.Collapsed;
                stkUpdate.Visibility = Visibility.Visible;
            }
            else if (methodName == "AddScopeClaim")
            {
                DisableResourcerInputControle();
                EnableResourcerClaimInputControle();

                ApiScopeClaims = Global.ApiScopeClaimsGetData;
                stkUpdate.Visibility = Visibility.Hidden;
                DisableResourcerInputControle();
                lblClaimType.Content = "Scope Claim Type";

                txtClaimType.Text = string.Empty;
                stkAdd.Visibility = Visibility.Visible;
                stkUpdate.Visibility = Visibility.Collapsed;

            }
            else if (methodName == "UpdateScopeClaim")
            {
                DisableResourcerInputControle();
                EnableResourcerClaimInputControle();

                ApiScopeClaims = Global.ApiScopeClaimsGetData;
                stkUpdate.Visibility = Visibility.Hidden;
                DisableResourcerInputControle();
                lblClaimType.Content = "Scope Claim Type";

                txtClaimType.Text = ApiScopeClaims.Type;
                stkAdd.Visibility = Visibility.Visible;
                stkUpdate.Visibility = Visibility.Collapsed;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void DisableResourcerInputControle()
        {
            lblName.Visibility = Visibility.Collapsed;
            lblDisplayName.Visibility = Visibility.Collapsed;
            lblDescription.Visibility = Visibility.Collapsed;

            txtName.Visibility = Visibility.Collapsed;
            txtDisplayName.Visibility = Visibility.Collapsed;
            txtDescription.Visibility = Visibility.Collapsed;
        }
        private void DisableResourcerClaimInputControle()
        {
            lblClaimType.Visibility = Visibility.Collapsed;
            txtClaimType.Visibility = Visibility.Collapsed;
        }
        private void EnableResourcerClaimInputControle()
        {
            lblClaimType.Visibility = Visibility.Visible;
            txtClaimType.Visibility = Visibility.Visible;
        }

        private void EnableResourcerInputControle()
        {
            lblName.Visibility = Visibility.Visible;
            lblDisplayName.Visibility = Visibility.Visible;
            lblDescription.Visibility = Visibility.Visible;

            txtName.Visibility = Visibility.Visible;
            txtDisplayName.Visibility = Visibility.Visible;
            txtDescription.Visibility = Visibility.Visible;
        }
        private async Task Add()
        {
            if (methodName == "AddApiResource")
            {
                var apiResourcesModelInput = CreateApiResourceModel();
                apiResourcesModelInput.Name = txtName.Text;
                apiResourcesModelInput.DisplayName = txtDisplayName.Text;
                apiResourcesModelInput.Description = txtDescription.Text;

                Http.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Global.AccessToken);
                Mouse.OverrideCursor = Cursors.Wait;
                var apiResourcesurl = ApplicationConstants.BaseUrl + ApiRoutePathConstants.AddApiResource;
                var apiResourcesresponse = Http.Client.PostAsync(apiResourcesurl, new StringContent(JsonConvert.SerializeObject(apiResourcesModelInput), Encoding.UTF8, "application/json")).Result;
                var apiResourceResponse = apiResourcesresponse.Content.ReadAsStringAsync().Result;
                var resultApiresourceModel = JsonConvert.DeserializeObject<FrameworkResult>(apiResourceResponse);

                if (resultApiresourceModel.Status == ResultStatus.Success)
                {
                    Mouse.OverrideCursor = null;
                    MessageBox.Show("Api resource added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    Mouse.OverrideCursor = null;
                    MessageBox.Show(resultApiresourceModel.Errors.FirstOrDefault().Description, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (methodName == "AddApiResourceClaim")
            {
                ApiResourceClaims = Global.ApiResourceClaimsGetData;
                var apiResourcesClaimModelInput = CreateApiResourceClaimModel();
                apiResourcesClaimModelInput.ApiResourceId = ApiResourceClaims.ApiResourceId;
                apiResourcesClaimModelInput.Type = txtClaimType.Text;

                Http.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Global.AccessToken);
                Mouse.OverrideCursor = Cursors.Wait;
                var apiResourcesurl = ApplicationConstants.BaseUrl + ApiRoutePathConstants.AddApiResourceClaim;
                var apiResourcesresponse = Http.Client.PostAsync(apiResourcesurl, new StringContent(JsonConvert.SerializeObject(apiResourcesClaimModelInput), Encoding.UTF8, "application/json")).Result;
                var apiResourceResponse = apiResourcesresponse.Content.ReadAsStringAsync().Result;
                var resultApiresourceModel = JsonConvert.DeserializeObject<FrameworkResult>(apiResourceResponse);

                if (resultApiresourceModel.Status== ResultStatus.Success)
                {
                    Mouse.OverrideCursor = null;
                    MessageBox.Show("Api resource claim added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    Mouse.OverrideCursor = null;
                    MessageBox.Show(resultApiresourceModel.Errors.FirstOrDefault().Description, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (methodName == "AddScope")
            {
                ApiScope = Global.ApiScopeGetData;
                var apiScopeModelInput = CreateApiScopeModel();
                apiScopeModelInput.ApiResourceId = ApiScope.ApiResourceId;
                apiScopeModelInput.Name = txtName.Text;
                apiScopeModelInput.DisplayName= txtDisplayName.Text;
                apiScopeModelInput.Description= txtDescription.Text;

                Http.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Global.AccessToken);
                Mouse.OverrideCursor = Cursors.Wait;
                var apiResourcesurl = ApplicationConstants.BaseUrl + ApiRoutePathConstants.AddApiScope;
                var apiResourcesresponse = Http.Client.PostAsync(apiResourcesurl, new StringContent(JsonConvert.SerializeObject(apiScopeModelInput), Encoding.UTF8, "application/json")).Result;
                var apiResourceResponse = apiResourcesresponse.Content.ReadAsStringAsync().Result;
                var resultApiresourceModel = JsonConvert.DeserializeObject<FrameworkResult>(apiResourceResponse);

                if (resultApiresourceModel.Status== ResultStatus.Success)
                {
                    Mouse.OverrideCursor = null;
                    MessageBox.Show("Api Scope added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    Mouse.OverrideCursor = null;
                    MessageBox.Show(resultApiresourceModel.Errors.FirstOrDefault().Description, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }
            else if (methodName == "AddScopeClaim")
            {
                ApiScopeClaims = Global.ApiScopeClaimsGetData;
                var apiScopeModelInput = CreateApiScopeModel();
                var apiScopeClaimModelInput = CreateApiScopeClaimModel();
                apiScopeClaimModelInput.ApiScopeId = ApiScopeClaims.ApiScopeId;
                apiScopeClaimModelInput.Type = txtClaimType.Text;

                Http.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Global.AccessToken);
                Mouse.OverrideCursor = Cursors.Wait;
                var apiResourcesurl = ApplicationConstants.BaseUrl + ApiRoutePathConstants.AddApiScope;
                var apiResourcesresponse = Http.Client.PostAsync(apiResourcesurl, new StringContent(JsonConvert.SerializeObject(apiScopeModelInput), Encoding.UTF8, "application/json")).Result;
                var apiResourceResponse = apiResourcesresponse.Content.ReadAsStringAsync().Result;
                var resultApiresourceModel = JsonConvert.DeserializeObject<FrameworkResult>(apiResourceResponse);

                if (resultApiresourceModel.Status == ResultStatus.Success)
                {
                    Mouse.OverrideCursor = null;
                    MessageBox.Show("Api Scope added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    Mouse.OverrideCursor = null;
                    MessageBox.Show(resultApiresourceModel.Errors.FirstOrDefault().Description, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task Update()
        {
            if (methodName == "UpdateApiResource")
            {
                var apiResourcesModelInput = Global.ApiResourcesGetData;
                apiResourcesModelInput.Name = txtName.Text;
                apiResourcesModelInput.DisplayName = txtDisplayName.Text;
                apiResourcesModelInput.Description = txtDescription.Text;

                Http.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Global.AccessToken);
                Mouse.OverrideCursor = Cursors.Wait;
                var apiResourcesurl = ApplicationConstants.BaseUrl + ApiRoutePathConstants.UpdateApiResource;
                var apiResourcesresponse = Http.Client.PostAsync(apiResourcesurl, new StringContent(JsonConvert.SerializeObject(apiResourcesModelInput), Encoding.UTF8, "application/json")).Result;
                var apiResourceResponse = apiResourcesresponse.Content.ReadAsStringAsync().Result;
                var resultApiresourceModel = JsonConvert.DeserializeObject<FrameworkResult>(apiResourceResponse);

                if (resultApiresourceModel.Status== ResultStatus.Success)
                {
                    Mouse.OverrideCursor = null;
                    MessageBox.Show("Api resource updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    Mouse.OverrideCursor = null;
                    MessageBox.Show(resultApiresourceModel.Errors.FirstOrDefault().Description, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if (methodName == "UpdateScope")
            {
                ApiScope = Global.ApiScopeGetData;
                ApiScope.DisplayName = txtDisplayName.Text;
                ApiScope.Description = txtDescription.Text;

                Http.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Global.AccessToken);
                Mouse.OverrideCursor = Cursors.Wait;
                var apiScopeurl = ApplicationConstants.BaseUrl + ApiRoutePathConstants.UpdateApiScope;
                var apiScoperesponse = Http.Client.PostAsync(apiScopeurl, new StringContent(JsonConvert.SerializeObject(ApiScope), Encoding.UTF8, "application/json")).Result;
                var apiScopeResultResponse = apiScoperesponse.Content.ReadAsStringAsync().Result;
                var resultScopeModel = JsonConvert.DeserializeObject<FrameworkResult>(apiScopeResultResponse);

                if (resultScopeModel.Status== ResultStatus.Success)
                {
                    Mouse.OverrideCursor = null;
                    MessageBox.Show("Api scope updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    Mouse.OverrideCursor = null;
                    MessageBox.Show(resultScopeModel.Errors.FirstOrDefault().Description, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void Add_Click(object sender, RoutedEventArgs e)
        {
            _ = Add();
          Mediator.Notify("ApiResourceGridScreen", "");
            this.Close();
        }
        private void AddCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
      
        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            _ = Update();
            Mediator.Notify("ApiResourceGridScreen", "");
            Close();
        }
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        public ApiResourcesModel CreateApiResourceModel()
        {
            ApiResourcesModel apiResourceModel = new ApiResourcesModel
            {
                Name = "ClientApi",
                Description = "To register and manage client applications",
                DisplayName = "Client Api",
                Enabled = false,
                CreatedBy = "Test",
                ApiResourceClaims = new List<ApiResourceClaimsModel>()
                {
                    new ApiResourceClaimsModel() {Type = "Type1", CreatedBy = "Test" },
                },
                ApiScopes = new List<ApiScopesModel>()
                {
                    new ApiScopesModel()
                    {
                        Name = "ClientApi.Create", DisplayName = "DisplayName - ApiScope1",
                        Description = "ClientApi Description", Emphasize = false, Required = true,
                        CreatedBy = "Test",
                        ApiScopeClaims = new List<ApiScopeClaimsModel>()
                        {
                            new ApiScopeClaimsModel() {Type = "ApiScopeClaim - 1", CreatedBy = "Test" },
                        }
                    },
                },
            };
            return apiResourceModel;
        }
        public  ApiResourceClaimsModel CreateApiResourceClaimModel()
        {
            ApiResourceClaimsModel apiResourceClaimsModel = new ApiResourceClaimsModel
            {
                ApiResourceId = Guid.NewGuid(),
                CreatedBy = "Test",
                Type = "Test Claim",
            };
            return apiResourceClaimsModel;
        }
        public  ApiScopesModel CreateApiScopeModel()
        {
            ApiScopesModel apiScopeModel = new ApiScopesModel
            {
                Name = "ApiScopeOne",
                DisplayName = "DisplayName - ApiScope1",
                Description = "ClientApi Description",
                Emphasize = false,
                Required = true,
                CreatedBy = "Test",
                ApiScopeClaims = new List<ApiScopeClaimsModel>()
                        {
                            new ApiScopeClaimsModel() {Type = "ApiScopeClaim - 1", CreatedBy = "Test" },
                            new ApiScopeClaimsModel() {Type = "ApiScopeClaim - 2", CreatedBy = "Test" },
                        }
            };
            return apiScopeModel;
        }
        public static ApiScopeClaimsModel CreateApiScopeClaimModel()
        {
            System.Guid guid = System.Guid.NewGuid();
            ApiScopeClaimsModel apiScopeClaimsModel = new ApiScopeClaimsModel
            {
                ApiScopeId = guid,
                CreatedBy = "Test",
                Type = "Test",
            };
            return apiScopeClaimsModel;
        }
    }
}


