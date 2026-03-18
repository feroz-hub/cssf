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
using HCL.CS.SF.DemoClientWpfApp.View;
using static HCL.CS.SF.DemoClientWpfApp.DomainModel.AllowedScopesParserModel;
using DomainModel_FrameworkResult = HCL.CS.SF.DemoClientWpfApp.DomainModel.FrameworkResult;
using FrameworkResult = HCL.CS.SF.DemoClientWpfApp.DomainModel.FrameworkResult;

namespace HCL.CS.SF.DemoClientWpfApp.ViewModel
{
    internal  class IdentityResourceGridViewModel :BaseViewModel, IPageViewModel
    {
        ApplicationParameters ApplicationConstants = new ApplicationParameters();
        public RelayCommand OnLoadCommand { get; set; }
        public RelayCommand HomeCommand { get; set; }
        public RelayCommand AddResourceCommand { get; set; }
        public RelayCommand UpdateResourceCommand { get; set; }
        public RelayCommand DeleteResourceCommand { get; set; }
        public RelayCommand AddResourceClaimCommand { get; set; }
        public RelayCommand DeleteResourceClaimCommand { get; set; }
        public RelayCommand RefreshCommand { get; set; }

        private AllowedScopesParserModel.IdentityResourcesModel identityResourcesSelected;
        private AllowedScopesParserModel.IdentityClaimsModel identityResourceClaimsSelected;

        private List<AllowedScopesParserModel.IdentityResourcesModel> identityResource;
        public List<AllowedScopesParserModel.IdentityResourcesModel> IdentityResource
        {
            get
            {
                return identityResource;
            }
            set
            {
                identityResource = value;
                OnPropertyChanged("IdentityResource");
            }
        }
        public AllowedScopesParserModel.IdentityResourcesModel IdentityResourcesSelected
        {
            get
            {
                return identityResourcesSelected;
            }
            set
            {
                identityResourcesSelected = value;
                OnPropertyChanged("IdentityResourcesSelected");
            }
        }
        public AllowedScopesParserModel.IdentityClaimsModel IdentityResourceClaimsSelected
        {
            get
            {
                return identityResourceClaimsSelected;
            }
            set
            {
                identityResourceClaimsSelected = value;
                OnPropertyChanged("IdentityResourceClaimsSelected");
            }
        }
        public IdentityResourceGridViewModel()
        {
            HomeCommand = new RelayCommand(param => OnHome());
            OnLoadCommand = new RelayCommand(param => Onload());
            AddResourceCommand = new RelayCommand(param => AddIdentityResource(param));
            UpdateResourceCommand = new RelayCommand(param => UpdateIdentityResource(param));
            DeleteResourceCommand = new RelayCommand(param => DeleteIdentityResource(param));
            AddResourceClaimCommand = new RelayCommand(param => AddIdentityResourceClaim(param));
            DeleteResourceClaimCommand = new RelayCommand(param => DeleteApiResourceClaim(param));
            RefreshCommand = new RelayCommand(param => Refresh());
        }
        private void Refresh()
        {
            IdentityResource = null;
            IdentityResource = GetIdnetityResource().Result;
        }
        private void Onload()
        {
            IdentityResource = null;
            IdentityResource = GetIdnetityResource().Result;
        }
        private async Task<List<AllowedScopesParserModel.IdentityResourcesModel>> GetIdnetityResource()
        {
            List<AllowedScopesParserModel.IdentityResourcesModel> identityResourcesModels = new List<AllowedScopesParserModel.IdentityResourcesModel>();
            try
            {
                Http.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Global.AccessToken);

                var identityResourcesurl = ApplicationConstants.BaseUrl + ApiRoutePathConstants.GetAllIdentityResources;
                var identityResourcesresponse = Http.Client.PostAsync(identityResourcesurl, new StringContent(JsonConvert.SerializeObject(string.Empty), Encoding.UTF8, "application/json")).Result;
                var identityResultResponse = identityResourcesresponse.Content.ReadAsStringAsync().Result;
                identityResourcesModels = JsonConvert.DeserializeObject<List<AllowedScopesParserModel.IdentityResourcesModel>>(identityResultResponse);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
                return identityResourcesModels;

        }
        private Task OnHome()
        {
            Mediator.Notify("DashBoardScreen", "");
            return Task.CompletedTask;
        }

        private void AddIdentityResource(object item)
        {
            try
            {
                IdentityResourcesSelected = item as AllowedScopesParserModel.IdentityResourcesModel;
                Global.PopupMethodName = "AddIdentityResource";

                IdentityPopup identityPopup = new IdentityPopup();
                identityPopup.ShowDialog();
                IdentityResource = null;
                IdentityResource = GetIdnetityResource().Result;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateIdentityResource(object item)
        {
            try
            {
                IdentityResourcesSelected = item as AllowedScopesParserModel.IdentityResourcesModel;
                Global.PopupMethodName = "UpdateIdentityResource";
                Global.IdentityResourcesGetdata = IdentityResourcesSelected;

                IdentityPopup identityPopup = new IdentityPopup();
                identityPopup.ShowDialog();

                IdentityResource = null;
                IdentityResource = GetIdnetityResource().Result;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeleteIdentityResource(object item)
        {
            try
            {
                string message = "Are you sure want to delete Identity Resource?";
                string caption = "Confirmation";
                MessageBoxButton buttons = MessageBoxButton.YesNo;
                MessageBoxImage icon = MessageBoxImage.Warning;

                if (MessageBox.Show(message, caption, buttons, icon) == MessageBoxResult.Yes)
                {
                    IdentityResourcesSelected = item as AllowedScopesParserModel.IdentityResourcesModel;
                    Http.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Global.AccessToken);

                    Mouse.OverrideCursor = Cursors.Wait;

                    var identityResourcesurl = ApplicationConstants.BaseUrl + ApiRoutePathConstants.DeleteIdentityResourceByName;
                    var identityResourcesresponse = Http.Client.PostAsync(identityResourcesurl, new StringContent(JsonConvert.SerializeObject(IdentityResourcesSelected.Name), Encoding.UTF8, "application/json")).Result;
                    var identityResultResourceResponse = identityResourcesresponse.Content.ReadAsStringAsync().Result;
                    var resultIdentityModel = JsonConvert.DeserializeObject<DomainModel_FrameworkResult>(identityResultResourceResponse);

                    if (resultIdentityModel.Status == ResultStatus.Success)
                    {
                        Mouse.OverrideCursor = null;
                        MessageBox.Show("Identity resource deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        Mouse.OverrideCursor = null;
                        MessageBox.Show(resultIdentityModel.Errors.FirstOrDefault().Description, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    IdentityResource = null;
                    IdentityResource = GetIdnetityResource().Result;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddIdentityResourceClaim(object item)
        {
            try
            {
                IdentityResourceClaimsSelected = item as AllowedScopesParserModel.IdentityClaimsModel;
                Global.PopupMethodName = "AddIdentityResourceClaim";
                Global.IdentityResourcesClaimGetdata = IdentityResourceClaimsSelected;

                IdentityPopup identityPopup = new IdentityPopup();
                identityPopup.ShowDialog();

                IdentityResource = null;
                IdentityResource = GetIdnetityResource().Result;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeleteApiResourceClaim(object item)
        {
            try
            {
                string message = "Are you sure want to delete Identity Resource claim?";
                string caption = "Confirmation";
                MessageBoxButton buttons = MessageBoxButton.YesNo;
                MessageBoxImage icon = MessageBoxImage.Warning;

                if (MessageBox.Show(message, caption, buttons, icon) == MessageBoxResult.Yes)
                {
                    IdentityResourceClaimsSelected = item as AllowedScopesParserModel.IdentityClaimsModel;
                    Http.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Global.AccessToken);
                    Mouse.OverrideCursor = Cursors.Wait;

                    var identityResourcesurl = ApplicationConstants.BaseUrl + ApiRoutePathConstants.DeleteIdentityResourceClaimModel;
                    var identityResourcesresponse = Http.Client.PostAsync(identityResourcesurl, new StringContent(JsonConvert.SerializeObject(IdentityResourceClaimsSelected), Encoding.UTF8, "application/json")).Result;
                    var IdentittResourceResultResponse = identityResourcesresponse.Content.ReadAsStringAsync().Result;
                    var resultApiresourceModel = JsonConvert.DeserializeObject<DomainModel_FrameworkResult>(IdentittResourceResultResponse);

                    if (resultApiresourceModel.Status == ResultStatus.Success)
                    {
                        MessageBox.Show("Identity resource claim deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show(resultApiresourceModel.Errors.FirstOrDefault().Description, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    Mouse.OverrideCursor = null;
                    IdentityResource = null;
                    IdentityResource = GetIdnetityResource().Result;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}


