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
using HCL.CS.SF.DemoClientWpfApp.Components;
using HCL.CS.SF.DemoClientWpfApp.Constants;
using HCL.CS.SF.DemoClientWpfApp.DomainModel;
using HCL.CS.SF.DemoClientWpfApp.Interface;
using HCL.CS.SF.DemoClientWpfApp.Services;
using HCL.CS.SF.DemoClientWpfApp.View.API;
using static HCL.CS.SF.DemoClientWpfApp.DomainModel.LogoutMessageModel;
using DomainModel_FrameworkResult = HCL.CS.SF.DemoClientWpfApp.DomainModel.FrameworkResult;
using FrameworkResult = HCL.CS.SF.DemoClientWpfApp.DomainModel.FrameworkResult;

namespace HCL.CS.SF.DemoClientWpfApp.ViewModel
{
    internal class UserRoleViewModel : BaseViewModel, IPageViewModel
    {
        ApplicationParameters ApplicationConstants = new ApplicationParameters();
        public RelayCommand AddUserRoleCommand { get; set; }
        public RelayCommand UpdateUserRoleCommand { get; set; }
        public RelayCommand DeleteUserRoleCommand { get; set; }
        public RelayCommand OnLoadCommand { get; set; }
        public RelayCommand LockUnLockCommand { get; set; }
        public RelayCommand ViewUserClaimCommand { get; set; }

        private List<UserDisplayModel> user;
        public List<UserDisplayModel> User
        {
            get
            {
                return user;
            }
            set
            {
                user = value;
                OnPropertyChanged("User");
            }
        }

        private List<string> roles;
        public List<string> Roles
        {
            get
            {
                return roles;
            }
            set
            {
                roles = value;
                OnPropertyChanged("Roles");
            }
        }

        private LogoutMessageModel.UserRoleModel userRoleModel;
        public LogoutMessageModel.UserRoleModel UserRoleModel
        {
            get
            {
                return userRoleModel;
            }
            set
            {
                userRoleModel = value;
                OnPropertyChanged("UserRoleModel");
            }
        }

        private LogoutMessageModel.UserRoleModel userRoleModelSelected;
        public LogoutMessageModel.UserRoleModel UserRoleModelSelected
        {
            get
            {
                return userRoleModelSelected;
            }
            set
            {
                userRoleModelSelected = value;
                OnPropertyChanged("UserRoleModelSelected");
            }
        }

        private int toSerialNumber;
        public int ToSerialNumber
        {
            get
            {
                return toSerialNumber;
            }
            set
            {
                toSerialNumber = value;
                OnPropertyChanged("ToSerialNumber");
            }
        }

        public UserRoleViewModel()
        {
            OnLoadCommand = new RelayCommand(param => GetUsers());
            AddUserRoleCommand = new RelayCommand(param => AddUserRole(param));
            UpdateUserRoleCommand = new RelayCommand(param => UpdateRole(param));
            LockUnLockCommand = new RelayCommand(param => LockUnLOck(param));
            ViewUserClaimCommand = new RelayCommand(param => GetClaims(param));
        }
        private async Task GetUsers()
        {
            try
            {
                User = null;
                Http.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Global.AccessToken);
                var userurl = ApplicationConstants.BaseUrl + ApiRoutePathConstants.GetAllUsers;
                var userResponse = Http.Client.PostAsync(userurl, new StringContent(JsonConvert.SerializeObject(string.Empty), Encoding.UTF8, "application/json")).Result;
                var userResultResponse = userResponse.Content.ReadAsStringAsync().Result;
                User = JsonConvert.DeserializeObject<List<UserDisplayModel>>(userResultResponse);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task GetClaims(object item)
        {
            try
            {
                var userModel = item as UserDisplayModel;
                Http.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Global.AccessToken);
                var userurl = ApplicationConstants.BaseUrl + ApiRoutePathConstants.GetAdminUserClaims;
                var userResponse = Http.Client.PostAsync(userurl, new StringContent(JsonConvert.SerializeObject(userModel.Id), Encoding.UTF8, "application/json")).Result;
                var cleintResultResponse = userResponse.Content.ReadAsStringAsync().Result;
                var claims = JsonConvert.DeserializeObject<List<UserClaimModel>>(cleintResultResponse);

                if (claims == null)
                {
                    List<UserClaimModel> userClaimModel = new List<UserClaimModel>();
                    userClaimModel.Add(new UserClaimModel() { UserId = userModel.Id, });
                    Global.UserClaimModelGetData = userClaimModel;
                }
                else
                {
                    Global.UserClaimModelGetData = claims;
                }

                UserClaimPopup userClaimPopup = new UserClaimPopup();
                userClaimPopup.ShowDialog();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddUserRole(object item)
        {
            try
            {
                var usrrolesresult = GetUserRole(item).Result;
                var rolesResult = GetRole().Result;
                Global.UserRoleGetdata = usrrolesresult;
                Global.RolesGetdata = rolesResult;

                Global.PopupMethodName = "AddUserRole";
                UserRolePopup userRolePopup = new UserRolePopup();
                userRolePopup.ShowDialog();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void UpdateRole(object item)
        {
            try
            {
                var usrrolesresult = GetUserRole(item).Result;
                var rolesResult = GetRole().Result;
                Global.UserRoleGetdata = usrrolesresult;
                Global.RolesGetdata = rolesResult;
                Global.PopupMethodName = "UpdateUserRole";
                UserRolePopup userRolePopup = new UserRolePopup();
                userRolePopup.ShowDialog();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async Task <List<RoleModel>> GetRole()
        {
           
                Http.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Global.AccessToken);
                var roleurl = ApplicationConstants.BaseUrl + ApiRoutePathConstants.GetAllRoles;
                var roleresponse = Http.Client.PostAsync(roleurl, new StringContent(JsonConvert.SerializeObject(string.Empty), Encoding.UTF8, "application/json")).Result;
                var roleResultResponse = roleresponse.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<List<RoleModel>>(roleResultResponse);
        }

        private async Task<List<string>> GetUserRole(object item)
        {
            var userModel = item as UserDisplayModel;
            Global.DisplayModelGetdata = userModel;
            Http.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Global.AccessToken);
            var userroleUrl = ApplicationConstants.BaseUrl + ApiRoutePathConstants.GetUserRoles;
            var response = Http.Client.PostAsync(userroleUrl, new StringContent(JsonConvert.SerializeObject(userModel.Id), Encoding.UTF8, "application/json")).Result;
            var resultResponse = response.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<List<string>>(resultResponse);
        }

        private async Task LockUnLOck( object item)
        {
            try
            {
                var lockModel = item as UserDisplayModel;
                var locked = lockModel.LockoutEnabled;
                if (locked)
                {
                    string message = "Are you sure want to unlock user?";
                    string caption = "Confirmation";
                    MessageBoxButton buttons = MessageBoxButton.YesNo;
                    MessageBoxImage icon = MessageBoxImage.Warning;

                    if (MessageBox.Show(message, caption, buttons, icon) == MessageBoxResult.Yes)
                    {
                        Http.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Global.AccessToken);
                        var lockurl = ApplicationConstants.BaseUrl + ApiRoutePathConstants.UnLockUser;
                        var lockResponse = Http.Client.PostAsync(lockurl, new StringContent(JsonConvert.SerializeObject(lockModel.UserName), Encoding.UTF8, "application/json")).Result;
                        var lockResultResponse = lockResponse.Content.ReadAsStringAsync().Result;
                        var result = JsonConvert.DeserializeObject<DomainModel_FrameworkResult>(lockResultResponse);
                        if (result.Status == ResultStatus.Success)
                        {
                            GetUsers().GetAwaiter().GetResult();
                            MessageBox.Show("User unlocked successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show(result.Errors.FirstOrDefault().Description, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                else
                {
                    string message = "Are you sure, want to lock the user account?";
                    string caption = "Confirmation";
                    MessageBoxButton buttons = MessageBoxButton.YesNo;
                    MessageBoxImage icon = MessageBoxImage.Warning;

                    if (MessageBox.Show(message, caption, buttons, icon) == MessageBoxResult.Yes)
                    {
                        Http.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Global.AccessToken);
                        var lockurl = ApplicationConstants.BaseUrl + ApiRoutePathConstants.LockUser;
                        var lockResponse = Http.Client.PostAsync(lockurl, new StringContent(JsonConvert.SerializeObject(lockModel.Id), Encoding.UTF8, "application/json")).Result;
                        var lockResultResponse = lockResponse.Content.ReadAsStringAsync().Result;
                        var result = JsonConvert.DeserializeObject<DomainModel_FrameworkResult>(lockResultResponse);
                        if (result.Status == ResultStatus.Success)
                        {
                            GetUsers().GetAwaiter().GetResult();
                            MessageBox.Show("User locked successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show(result.Errors.FirstOrDefault().Description, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}


