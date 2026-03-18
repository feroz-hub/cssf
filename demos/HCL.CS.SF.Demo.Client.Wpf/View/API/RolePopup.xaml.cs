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
using HCL.CS.SF.DemoClientWpfApp.Services;
using HCL.CS.SF.DemoClientWpfApp.ViewModel;

namespace HCL.CS.SF.DemoClientWpfApp.View
{
    public partial class RolePopup : Window
    {
        ApplicationParameters ApplicationConstants = new ApplicationParameters();
        public string methodName;
        public RoleModel RoleModel;
        public RoleClaimModel RoleClaimModel;
        public RolePopup()
        {
            InitializeComponent();
            GetRoles();
        }
        public void GetRoles()
        {
            methodName = Global.PopupMethodName;
            if (methodName == "AddRole")
            {
                EnableRoleInputControle();
                DisableRoleClaimInputControle();
                RoleModel = Global.RoleModelGetData;
                stkUpdate.Visibility = Visibility.Hidden;
                lblName.Content = "Role Name";
                lblDescription.Content = "Role Description";

                txtName.Text = string.Empty;
                txtDescription.Text = string.Empty;
                stkUpdate.Visibility = Visibility.Collapsed;
                stkAdd.Visibility = Visibility.Visible;
            }
            else if (methodName == "UpdateRole")
            {
                EnableRoleInputControle();
                DisableRoleClaimInputControle();
                RoleModel = Global.RoleModelGetData;
                stkUpdate.Visibility = Visibility.Hidden;
                lblName.Content = "Role Name";
                lblDescription.Content = "Role Description";

                txtName.Text = RoleModel.Name;
                txtName.IsEnabled=false;
                txtDescription.Text = RoleModel.Description;
                stkUpdate.Visibility = Visibility.Visible;
                stkAdd.Visibility = Visibility.Collapsed;
            }
        }
        private void DisableRoleClaimInputControle()
        {
            lblClaimType.Visibility = Visibility.Collapsed;
            txtClaimType.Visibility = Visibility.Collapsed;

            lblClaimType.Visibility = Visibility.Collapsed;
            txtClaimValue.Visibility = Visibility.Collapsed;
        }
        private void EnableRoleInputControle()
        {
            lblName.Visibility = Visibility.Visible;
            lblDescription.Visibility = Visibility.Visible;
            txtName.Visibility = Visibility.Visible;
            txtDescription.Visibility = Visibility.Visible;
        }
        private void Add_Click(object sender, RoutedEventArgs e)
        {
            _= Add();
            Close();
        }

        private void AddCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            _ = Update();
            Task.Delay(2000);
            Mediator.Notify("RoleScreen", "");
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private async Task Add()
        {
            if (methodName == "AddRole")
            {
                var roleModelInput = CreateRoleModel();

                roleModelInput.Name = txtName.Text;
                roleModelInput.Description = txtDescription.Text;

                Http.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Global.AccessToken);
                Mouse.OverrideCursor = Cursors.Wait;
                // Get ApiResource
                var roleurl = ApplicationConstants.BaseUrl + ApiRoutePathConstants.CreateRole;
                var roleResponse = Http.Client.PostAsync(roleurl, new StringContent(JsonConvert.SerializeObject(roleModelInput), Encoding.UTF8, "application/json")).Result;
                var roleResultResponse = roleResponse.Content.ReadAsStringAsync().Result;
                var resultRoleresourceModel = JsonConvert.DeserializeObject<FrameworkResult>(roleResultResponse);

                if (resultRoleresourceModel.Status == ResultStatus.Success)
                {
                    Mouse.OverrideCursor = null;
                    _ = MessageBox.Show("Role added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    Mediator.Notify("RoleScreen", "");
                }
                else
                {
                    Mouse.OverrideCursor = null;
                    MessageBox.Show(resultRoleresourceModel.Errors.FirstOrDefault().Description, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private async Task Update()
        {
            if (methodName == "UpdateRole")
            {
                var roleModelInput = Global.RoleModelGetData;
                roleModelInput.Name = txtName.Text;
                roleModelInput.Description = txtDescription.Text;

                Http.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Global.AccessToken);
                Mouse.OverrideCursor = Cursors.Wait;
               
                var roleurl = ApplicationConstants.BaseUrl + ApiRoutePathConstants.UpdateRole;
                var roleresponse = Http.Client.PostAsync(roleurl, new StringContent(JsonConvert.SerializeObject(roleModelInput), Encoding.UTF8, "application/json")).Result;
                var roleResultResponse = roleresponse.Content.ReadAsStringAsync().Result;
                var resultRoleModel = JsonConvert.DeserializeObject<FrameworkResult>(roleResultResponse);

                if (resultRoleModel.Status == ResultStatus.Success)
                {
                    Mouse.OverrideCursor = null;
                    MessageBox.Show("Role updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    Mouse.OverrideCursor = null;
                    MessageBox.Show(resultRoleModel.Errors.FirstOrDefault().Description, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public static RoleModel CreateRoleModel()
        {
            RoleModel roleModel = new RoleModel()
            {
                Description = "To perform security administration for CS Framework",
                Name = "SecurityAdmin",
                CreatedBy = "Test",
                IsDeleted = false,
                //RoleClaims = new List<RoleClaimModel>()
                //{
                //new RoleClaimModel { ClaimType = "Permisson", ClaimValue = "HCL.CS.SF.role.read", CreatedBy="Test", CreatedOn=DateTime.UtcNow },
                //}
            };
            return roleModel;
        }
    }
}


