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
using HCL.CS.SF.DemoClientWpfApp.View.API;

namespace HCL.CS.SF.DemoClientWpfApp.ViewModel
{
  internal  class RoleViewModel : BaseViewModel, IPageViewModel
    {
        ApplicationParameters ApplicationConstants = new ApplicationParameters();
        public RelayCommand OnLoadCommand { get; set; }
        public RelayCommand HomeCommand { get; set; }
        public RelayCommand AddRoleCommand { get; set; }
        public RelayCommand UpdateRoleCommand { get; set; }
        public RelayCommand DeleteRoleCommand { get; set; }
        public RelayCommand ViewRoleClaimCommand { get; set; }
        public RelayCommand RefreshCommand { get; set; }

        private RoleModel roleModelSelected;
        private RoleClaimModel roleClaimModelSelected;
        private List<RoleModel> roleModels = null;

        public RoleModel RoleSelected
        {
            get
            {
                return roleModelSelected;
            }
            set
            {
                roleModelSelected = value;
                OnPropertyChanged("RoleSelected");
            }
        }
        public List<RoleModel> RoleModels
        {
            get
            {
                return roleModels;
            }
            set
            {
                roleModels = value;
                OnPropertyChanged("RoleModels");
            }
        }

        public RoleViewModel()
        {
            HomeCommand = new RelayCommand(param => OnHome());
            OnLoadCommand = new RelayCommand(param => Onload());
            AddRoleCommand = new RelayCommand(param => AddRole(param));
            UpdateRoleCommand = new RelayCommand(param => UpdateRole(param));
            DeleteRoleCommand = new RelayCommand(param => DeleteRole(param));
            ViewRoleClaimCommand = new RelayCommand(param => ViewRoleClaim(param));
            RefreshCommand = new RelayCommand(param => Refresh());
        }
        private void Refresh()
        {
            RoleModels = null;
            RoleModels = GetRole().Result;
        }
        private void Onload()
        {
            RoleModels = null;
            RoleModels = GetRole().Result;
        }
        private async Task <List<RoleModel>> GetRole()
        {
            List<RoleModel> roleModels = new List<RoleModel>();
            try
            {
                Http.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Global.AccessToken);
                var roleurl = ApplicationConstants.BaseUrl + ApiRoutePathConstants.GetAllRoles;
                var roleresponse = Http.Client.PostAsync(roleurl, new StringContent(JsonConvert.SerializeObject(string.Empty), Encoding.UTF8, "application/json")).Result;
                var roleResultResponse = roleresponse.Content.ReadAsStringAsync().Result;
                roleModels = JsonConvert.DeserializeObject<List<RoleModel>>(roleResultResponse);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            return roleModels;
        }
        private void AddRole(object item)
        {
            try
            {
                RoleSelected = item as RoleModel;
                Global.PopupMethodName = "AddRole";

                RolePopup rolePopup = new RolePopup();
                rolePopup.ShowDialog();
                RoleModels = null;
                RoleModels = GetRole().Result;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private void UpdateRole(object item)
        {
            try
            {
                RoleSelected = item as RoleModel;
                Global.PopupMethodName = "UpdateRole";
                Global.RoleModelGetData = RoleSelected;
                RolePopup rolePopup = new RolePopup();
                rolePopup.ShowDialog();

                RoleModels = null;
                RoleModels = GetRole().Result;
            }

            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private async Task DeleteRole(object item)
        {
            try
            {
                string message = "Are you sure want to delete role?";
                string caption = "Confirmation";
                MessageBoxButton buttons = MessageBoxButton.YesNo;
                MessageBoxImage icon = MessageBoxImage.Warning;

                if (MessageBox.Show(message, caption, buttons, icon) == MessageBoxResult.Yes)
                {
                    RoleSelected = item as RoleModel;
                    Http.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Global.AccessToken);
                    Mouse.OverrideCursor = Cursors.Wait;

                    var roleurl = ApplicationConstants.BaseUrl + ApiRoutePathConstants.DeleteRoleById;
                    var roleResponse = Http.Client.PostAsync(roleurl, new StringContent(JsonConvert.SerializeObject(RoleSelected.Id), Encoding.UTF8, "application/json")).Result;
                    var roleResultResponse = roleResponse.Content.ReadAsStringAsync().Result;
                    var resultRoleModel = JsonConvert.DeserializeObject<FrameworkResult>(roleResultResponse);

                    if (resultRoleModel.Status == ResultStatus.Success)
                    {
                        Mouse.OverrideCursor = null;
                        MessageBox.Show("Role deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        Mouse.OverrideCursor = null;
                        MessageBox.Show(resultRoleModel.Errors.FirstOrDefault().Description, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    RoleModels = null;
                    RoleModels = GetRole().Result;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async Task ViewRoleClaim(object item)
        {
            try
            {
                var getRole = item as RoleModel;
               
                Http.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Global.AccessToken);
                var roleurl = ApplicationConstants.BaseUrl + ApiRoutePathConstants.GetRoleClaim;
                var roleresponse = Http.Client.PostAsync(roleurl, new StringContent(JsonConvert.SerializeObject(getRole), Encoding.UTF8, "application/json")).Result;
                var roleResultResponse = roleresponse.Content.ReadAsStringAsync().Result;
                var claims = JsonConvert.DeserializeObject<IList<RoleClaimModel>>(roleResultResponse);
                Global.RoleClaimModelGetData = claims as List<RoleClaimModel>;
                RoleClaimPopup roleClaimPopup = new RoleClaimPopup(getRole.Id);
                roleClaimPopup.ShowDialog();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private void OnHome()
        {
            Mediator.Notify("DashBoardScreen", "");
        }
    }
}


