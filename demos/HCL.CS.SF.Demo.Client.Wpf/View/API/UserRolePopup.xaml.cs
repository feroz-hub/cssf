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
using System.Windows.Controls;
using HCL.CS.SF.DemoClientWpfApp.Constants;
using HCL.CS.SF.DemoClientWpfApp.DomainModel;
using HCL.CS.SF.DemoClientWpfApp.Services;
using HCL.CS.SF.DemoClientWpfApp.ViewModel;
using static HCL.CS.SF.DemoClientWpfApp.DomainModel.LogoutMessageModel;
using DomainModel_FrameworkResult = HCL.CS.SF.DemoClientWpfApp.DomainModel.FrameworkResult;
using FrameworkResult = HCL.CS.SF.DemoClientWpfApp.DomainModel.FrameworkResult;

namespace HCL.CS.SF.DemoClientWpfApp.View.API
{
    public partial class UserRolePopup : Window
    {
        ApplicationParameters ApplicationConstants = new ApplicationParameters();
        public static Guid parase;
        public UserRolePopup()
        {
            InitializeComponent();
            GetRoles();
        }
        public bool Isremoved;
        private void GetRoles()
        {
            try
            {
                if (Global.PopupMethodName == "AddUserRole")
                {
                    stkAdd.Visibility = Visibility.Visible;
                    var userrole = Global.UserRoleGetdata;
                    var roles = Global.RolesGetdata;

                    txtuserid.Text = Global.DisplayModelGetdata.Id.ToString();
                    txtuserName.Content = Global.DisplayModelGetdata.UserName.ToString();
                    foreach (var item in roles)
                    {
                        CheckBox checkBox = new CheckBox();
                        if (userrole != null && userrole.Contains(item.Name))
                        {
                            checkBox.IsChecked = true;
                        }
                        checkBox.Content = item.Name.ToString();
                        checkBox.Uid = item.Id.ToString();

                        listroles.Items.Add(checkBox);
                    }
                }
                if (Global.PopupMethodName == "UpdateUserRole")
                {
                    stkAdd.Visibility = Visibility.Collapsed;

                    var userrole = Global.UserRoleGetdata;
                    var roles = Global.RolesGetdata;

                    txtuserid.Text = Global.DisplayModelGetdata.Id.ToString();
                    txtuserName.Content = Global.DisplayModelGetdata.UserName.ToString();
                    foreach (var item in roles)
                    {
                        CheckBox checkBox = new CheckBox();
                        if (userrole != null && userrole.Contains(item.Name))
                        {
                            checkBox.IsChecked = true;
                            checkBox.Content = item.Name.ToString();
                            checkBox.Uid = item.Id.ToString();

                            listroles.Items.Add(checkBox);
                        }
                    }
                }
            }catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async Task<DomainModel_FrameworkResult> AddUserrole(List<LogoutMessageModel.UserRoleModel> assignRoles)
        {
            Http.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Global.AccessToken);
            var userroleUrl = ApplicationConstants.BaseUrl + ApiRoutePathConstants.AddUserRolesList;
            var response = Http.Client.PostAsync(userroleUrl, new StringContent(JsonConvert.SerializeObject(assignRoles), Encoding.UTF8, "application/json")).Result;
            var resultResponse = response.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<DomainModel_FrameworkResult>(resultResponse);
        }
        private async Task<DomainModel_FrameworkResult> RemoveUserRole(List<LogoutMessageModel.UserRoleModel> assignRoles)
        {
            Http.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Global.AccessToken);
            var userroleUrl = ApplicationConstants.BaseUrl + ApiRoutePathConstants.RemoveUserRoleList;
            var response = Http.Client.PostAsync(userroleUrl, new StringContent(JsonConvert.SerializeObject(assignRoles), Encoding.UTF8, "application/json")).Result;
            var resultResponse = response.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<DomainModel_FrameworkResult>(resultResponse);
        }
        private List<LogoutMessageModel.UserRoleModel> GetassignedUserRoles()
        {
            List<LogoutMessageModel.UserRoleModel> userRoles = new List<LogoutMessageModel.UserRoleModel>();
            var currentUserrRoles = Global.UserRoleGetdata;
            string userId = txtuserid.Text;
            Guid guid = Guid.Parse(userId);

            if (currentUserrRoles == null)
            {
                foreach (CheckBox item in listroles.Items)
                {
                    if (item.IsChecked == true)
                    {
                        Guid roleId = Guid.Parse(item.Uid.ToString());
                        userRoles.Add(new LogoutMessageModel.UserRoleModel { UserId = guid, RoleId = roleId, });
                    }
                }
            }
            else
            {
                userRoles.Clear();
                foreach (CheckBox item in listroles.Items)
                {
                    if (item.IsChecked == false && currentUserrRoles.Contains(item.Content))
                    {
                        Guid roleId = Guid.Parse(item.Uid.ToString());
                        userRoles.Add(new LogoutMessageModel.UserRoleModel { UserId = guid, RoleId = roleId, });
                    }
                }
                if (userRoles.Count > 0)
                {
                    var addUserroleresult = RemoveUserRole(userRoles).Result;
                    Isremoved = true;
                }

                userRoles.Clear();
                foreach (CheckBox item in listroles.Items)
                {
                    if (item.IsChecked == true && !currentUserrRoles.Contains(item.Content))
                    {
                        Guid roleId = Guid.Parse(item.Uid.ToString());
                        userRoles.Add(new LogoutMessageModel.UserRoleModel { UserId = guid, RoleId = roleId, });
                    }
                }
            }
            return userRoles;

        }
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var assignRoles = GetassignedUserRoles();
                if (assignRoles == null || assignRoles.Count < 1)
                {
                    if (Isremoved)
                    {
                        MessageBox.Show("Role changes updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        Close();
                    }
                    else
                    {
                        MessageBox.Show("No changes are made", "Success", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else
                if (assignRoles.Count > 0 && assignRoles != null)
                {
                    var result = AddUserrole(assignRoles).Result;
                    if (result.Status == ResultStatus.Success)
                    {
                        MessageBox.Show("User role successfully added.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show(result.Errors.FirstOrDefault().Description, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    Close();
                }
                else
                {
                    MessageBox.Show("No changes are made", "Success", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void btnAddCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var assignRoles = GetassignedUserRoles();
                if (assignRoles.Count > 0)
                {
                    string message = "Are you sure want to modify the user role?";
                    string caption = "Confirmation";
                    MessageBoxButton buttons = MessageBoxButton.YesNo;
                    MessageBoxImage icon = MessageBoxImage.Warning;

                    if (MessageBox.Show(message, caption, buttons, icon) == MessageBoxResult.Yes)
                    {
                        var result = RemoveUserRole(assignRoles).Result;
                        if (result.Status == ResultStatus.Success)
                        {
                            MessageBox.Show("User role successfully updated.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show(result.Errors.FirstOrDefault().Description, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        Close();
                    }
                }
                else
                {
                    MessageBox.Show("No changes are made", "Success", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}


