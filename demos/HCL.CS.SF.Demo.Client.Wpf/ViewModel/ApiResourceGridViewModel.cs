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
using HCL.CS.SF.DemoClientWpfApp.View.API;

namespace HCL.CS.SF.DemoClientWpfApp.ViewModel
{
    internal class ApiResourceGridViewModel : BaseViewModel, IPageViewModel
    {
        ApplicationParameters ApplicationConstants = new ApplicationParameters();
        public RelayCommand OnLoadCommand { get; set; }
        public RelayCommand AddResourceCommand { get; set; }
        public RelayCommand UpdateResourceCommand { get; set; }
        public RelayCommand DeleteResourceCommand { get; set; }
        public RelayCommand AddResourceClaimCommand { get; set; }
        public RelayCommand DeleteResourceClaimCommand { get; set; }
        public RelayCommand AddScopeCommand { get; set; }
        public RelayCommand UpdateScopeCommand { get; set; }
        public RelayCommand DeleteScopeCommand { get; set; }
        public RelayCommand AddScopeClaimCommand { get; set; }
        public RelayCommand DeleteScopeClaimCommand { get; set; }
        public RelayCommand PopupButton { get; set; }
        public RelayCommand HomeCommand { get; set; }
        public RelayCommand RefreshCommand { get; set; }
        public ApiResourceGridViewModel()
        {
            OnLoadCommand = new RelayCommand(param => OnLoad());
            AddResourceCommand = new RelayCommand(param => AddApiResource(param));
            UpdateResourceCommand = new RelayCommand(param => UpdateApiResource(param));
            DeleteResourceCommand = new RelayCommand(param => DeleteApiResource(param));
            AddResourceClaimCommand = new RelayCommand(param => AddApiResourceClaim(param));
            DeleteResourceClaimCommand = new RelayCommand(param => DeleteApiResourceClaim(param));
            AddScopeCommand = new RelayCommand(param => AddScope(param));
            UpdateScopeCommand = new RelayCommand(param => UpdateScope(param));
            DeleteScopeCommand = new RelayCommand(param => DeleteScope(param));
            AddScopeClaimCommand = new RelayCommand(param => AddScopeClaim(param));
            DeleteScopeClaimCommand = new RelayCommand(param => DeleteApiScopeClaim(param));
            PopupButton = new RelayCommand(param => AddApiResource(param));
            HomeCommand = new RelayCommand(param => OnHome());
            RefreshCommand = new RelayCommand(param => Refresh());
           
        }

        private List<ApiResourcesModel> apiResource;
        public List<ApiResourcesModel> ApiResource
        {
            get
            {
                return apiResource;
            }
            set
            {
                apiResource = value;
                OnPropertyChanged("ApiResource");
            }
        }
        private ApiScopesModel apiScopeSelected;
        private ApiResourcesModel apiResourcesSelected;
        private ApiResourceClaimsModel apiResourceClaimsSelected;
        private ApiScopeClaimsModel apiScopeClaimsSelected;
        public ApiScopesModel ApiScopeSelected
        {
            get
            {
                return apiScopeSelected;
            }
            set
            {
                apiScopeSelected = value;
                OnPropertyChanged("ApiScopeSelected");
            }
        }
        public ApiResourcesModel ApiResourcesSelected
        {
            get
            {
                return apiResourcesSelected;
            }
            set
            {
                apiResourcesSelected = value;
                OnPropertyChanged("ApiResourcesSelected");
            }
        }
        public ApiResourceClaimsModel ApiResourceClaimsSelected
        {
            get
            {
                return apiResourceClaimsSelected;
            }
            set
            {
                apiResourceClaimsSelected = value;
                OnPropertyChanged("ApiResourceClaimsSelected");
            }
        }
        public ApiScopeClaimsModel ApiScopeClaimsSelected
        {
            get
            {
                return apiScopeClaimsSelected;
            }
            set
            {
                apiScopeClaimsSelected = value;
                OnPropertyChanged("ApiScopeClaimsSelected");
            }
        }
        private void Refresh()
        {
            ApiResource = null;
            ApiResource = GetApiResourceAsync().Result;
        }

        // Add Api Resource
        private void AddApiResource(object item)
        {
            try
            {
                ApiResourcesSelected = item as ApiResourcesModel;
                Global.PopupMethodName = "AddApiResource";
                Global.ApiResourcesGetData = ApiResourcesSelected;

                ApiResourcePopup resourcePopup = new ApiResourcePopup();
                resourcePopup.ShowDialog();

                ApiResource = null;
                ApiResource = GetApiResourceAsync().Result;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Update Api Resource
        private void UpdateApiResource(object item)
        {
            try
            {
                ApiResourcesSelected = item as ApiResourcesModel;
                Global.PopupMethodName = "UpdateApiResource";
                Global.ApiResourcesGetData = ApiResourcesSelected;

                ApiResourcePopup resourcePopup = new ApiResourcePopup();
                resourcePopup.ShowDialog();
                ApiResource = null;
                ApiResource = GetApiResourceAsync().Result;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeleteApiResource(object item)
        {
            try
            {
                string message = "Are you sure want to delete Api Resource?";
                string caption = "Confirmation";
                MessageBoxButton buttons = MessageBoxButton.YesNo;
                MessageBoxImage icon = MessageBoxImage.Warning;

                if (MessageBox.Show(message, caption, buttons, icon) == MessageBoxResult.Yes)
                {
                    ApiResourcesSelected = item as ApiResourcesModel;
                    Http.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Global.AccessToken);
                    Mouse.OverrideCursor = Cursors.Wait;

                    var apiResourcesurl = ApplicationConstants.BaseUrl + ApiRoutePathConstants.DeleteApiResourceByName;
                    var apiResourcesresponse = Http.Client.PostAsync(
                        apiResourcesurl,
                        new StringContent(JsonConvert.SerializeObject(ApiResourcesSelected.Name), Encoding.UTF8, "application/json")).Result;
                    var apiResourceResponse = apiResourcesresponse.Content.ReadAsStringAsync().Result;
                    var resultApiresourceModel = JsonConvert.DeserializeObject<FrameworkResult>(apiResourceResponse);

                    if (resultApiresourceModel.Status == ResultStatus.Success)
                    {
                        Mouse.OverrideCursor = null;
                        MessageBox.Show("Api resource deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        Mouse.OverrideCursor = null;
                        MessageBox.Show(resultApiresourceModel.Errors.FirstOrDefault().Description, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    ApiResource = null;
                    ApiResource = GetApiResourceAsync().Result;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddApiResourceClaim(object item)
        {
            try
            {
                ApiResourceClaimsSelected = item as ApiResourceClaimsModel;
                Global.PopupMethodName = "AddApiResourceClaim";
                Global.ApiResourceClaimsGetData = ApiResourceClaimsSelected;

                ApiResourcePopup resourcePopup = new ApiResourcePopup();
                resourcePopup.ShowDialog();
                ApiResource = null;
                ApiResource = GetApiResourceAsync().Result;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeleteApiResourceClaim(object item)
        {
            try
            {
                string message = "Are you sure want to delete Api Resource claim?";
                string caption = "Confirmation";
                MessageBoxButton buttons = MessageBoxButton.YesNo;
                MessageBoxImage icon = MessageBoxImage.Warning;

                if (MessageBox.Show(message, caption, buttons, icon) == MessageBoxResult.Yes)
                {
                    ApiResourceClaimsSelected = item as ApiResourceClaimsModel;
                    Http.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Global.AccessToken);
                    Mouse.OverrideCursor = Cursors.Wait;

                    var apiResourcesurl = ApplicationConstants.BaseUrl + ApiRoutePathConstants.DeleteApiResourceClaimModel;
                    var apiResourcesresponse = Http.Client.PostAsync(apiResourcesurl, new StringContent(JsonConvert.SerializeObject(ApiResourceClaimsSelected), Encoding.UTF8, "application/json")).Result;
                    var apiResourceResponse = apiResourcesresponse.Content.ReadAsStringAsync().Result;
                    var resultApiresourceModel = JsonConvert.DeserializeObject<FrameworkResult>(apiResourceResponse);

                    if (resultApiresourceModel.Status == ResultStatus.Success)
                    {
                        Mouse.OverrideCursor = null;
                        MessageBox.Show("Api resource claim deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        Mouse.OverrideCursor = null;
                        MessageBox.Show(resultApiresourceModel.Errors.FirstOrDefault().Description, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    ApiResource = null;
                    ApiResource = GetApiResourceAsync().Result;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeleteApiScopeClaim(object item)
        {
            try
            {
                string message = "Are you sure want to delete Api Scope claim?";
                string caption = "Confirmation";
                MessageBoxButton buttons = MessageBoxButton.YesNo;
                MessageBoxImage icon = MessageBoxImage.Warning;

                if (MessageBox.Show(message, caption, buttons, icon) == MessageBoxResult.Yes)
                {
                    ApiScopeClaimsSelected = item as ApiScopeClaimsModel;
                    Http.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Global.AccessToken);
                    Mouse.OverrideCursor = Cursors.Wait;

                    var apiScopeClaimurl = ApplicationConstants.BaseUrl + ApiRoutePathConstants.DeleteApiScopeClaimModel;
                    var apiScopeClaimResponse = Http.Client.PostAsync(apiScopeClaimurl, new StringContent(JsonConvert.SerializeObject(ApiScopeClaimsSelected), Encoding.UTF8, "application/json")).Result;
                    var claimResponse = apiScopeClaimResponse.Content.ReadAsStringAsync().Result;
                    var claimresult = JsonConvert.DeserializeObject<FrameworkResult>(claimResponse);

                    if (claimresult.Status == ResultStatus.Success)
                    {
                        Mouse.OverrideCursor = null;
                        MessageBox.Show("Api Scope claim deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        Mouse.OverrideCursor = null;
                        MessageBox.Show(claimresult.Errors.FirstOrDefault().Description, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    ApiResource = null;
                    ApiResource = GetApiResourceAsync().Result;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private Task AddScope(object item)
        {
            try
            {
                var apiscope = item as ApiScopesModel;
                Global.PopupMethodName = "AddScope";

                ApiScopeSelected = apiscope;
                Global.ApiScopeGetData = ApiScopeSelected;

                ApiResourcePopup resourcePopup = new ApiResourcePopup();
                resourcePopup.ShowDialog();

                ApiResource = null;
                ApiResource = GetApiResourceAsync().Result;
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return Task.CompletedTask;
        }
        private Task UpdateScope(object item)
        {
            try
            {
                var apiscope = item as ApiScopesModel;
                Global.PopupMethodName = "UpdateScope";

                ApiScopeSelected = apiscope;
                Global.ApiScopeGetData = ApiScopeSelected;

                ApiResourcePopup resourcePopup = new ApiResourcePopup();
                resourcePopup.ShowDialog();

                ApiResource = null;
                ApiResource = GetApiResourceAsync().Result;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return Task.CompletedTask;
        }

        private async Task DeleteScope(object item)
        {
            try
            {

                string message = "Are you sure want to delete Api Scope?";
                string caption = "Confirmation";
                MessageBoxButton buttons = MessageBoxButton.YesNo;
                MessageBoxImage icon = MessageBoxImage.Warning;

                if (MessageBox.Show(message, caption, buttons, icon) == MessageBoxResult.Yes)
                {
                    var apiscope = item as ApiScopesModel;

                    Http.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Global.AccessToken);
                    Mouse.OverrideCursor = Cursors.Wait;
                    // Get ApiResource
                    var apiResourcesurl = ApplicationConstants.BaseUrl + ApiRoutePathConstants.DeleteApiScopeByName;
                    var apiResourcesresponse = Http.Client.PostAsync(apiResourcesurl, new StringContent(JsonConvert.SerializeObject(apiscope.Name), Encoding.UTF8, "application/json")).Result;
                    var apiResourceResponse = apiResourcesresponse.Content.ReadAsStringAsync().Result;
                    var resultApiresourceModel = JsonConvert.DeserializeObject<FrameworkResult>(apiResourceResponse);

                    if (resultApiresourceModel.Status == ResultStatus.Success)
                    {
                        MessageBox.Show("Api scope deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show(resultApiresourceModel.Errors.FirstOrDefault().Description, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    Mouse.OverrideCursor = null;
                    ApiResource = null;
                    ApiResource = GetApiResourceAsync().Result;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private Task AddScopeClaim(object item)
        {
            try
            {
                var result = item as ApiScopeClaimsModel;
                Global.PopupMethodName = "AddScopeClaim";

                ApiScopeClaimsSelected = result;
                Global.ApiScopeClaimsGetData = ApiScopeClaimsSelected;

                ApiResourcePopup resourcePopup = new ApiResourcePopup();
                resourcePopup.ShowDialog();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return Task.CompletedTask;
        }

        private  void OnLoad()
        {
            try
            {
                ApiResource = null;
                ApiResource = GetApiResourceAsync().Result;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async Task <List<ApiResourcesModel>> GetApiResourceAsync()
        {
            List<ApiResourcesModel> apiResourcesModels = new List<ApiResourcesModel>();
            try
            {
              
                Http.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Global.AccessToken);
                var apiResourcesurl = ApplicationConstants.BaseUrl + ApiRoutePathConstants.GetAllApiResources;
                var apiResourcesresponse = Http.Client.PostAsync(apiResourcesurl, new StringContent(JsonConvert.SerializeObject(string.Empty), Encoding.UTF8, "application/json")).Result;
                var apiResourceResponse = apiResourcesresponse.Content.ReadAsStringAsync().Result;
                apiResourcesModels = JsonConvert.DeserializeObject<List<ApiResourcesModel>>(apiResourceResponse);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return apiResourcesModels;
        }
        private Task OnHome()
        {
            Mediator.Notify("DashBoardScreen", "");
            return Task.CompletedTask;
        }
    }
}



