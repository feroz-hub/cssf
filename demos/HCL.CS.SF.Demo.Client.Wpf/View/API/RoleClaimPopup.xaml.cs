/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Linq;
using HCL.CS.SF.DemoClientWpfApp.Constants;
using HCL.CS.SF.DemoClientWpfApp.DomainModel;
using HCL.CS.SF.DemoClientWpfApp.Services;
using HCL.CS.SF.DemoClientWpfApp.ViewModel;

namespace HCL.CS.SF.DemoClientWpfApp.View.API
{
    public partial class RoleClaimPopup : Window
    {
        ApplicationParameters ApplicationConstants = new ApplicationParameters();
        public RoleClaimPopup(Guid roleId)
        {
            InitializeComponent();
            txtRoleId.Text = roleId.ToString();
            getClaims();
        }
        private void getClaims()
        {
            try
            {
                stkAddClaim.Visibility = Visibility.Collapsed;
                stkbtn.Visibility = Visibility.Visible;
                var getClaims = Global.RoleClaimModelGetData;
                if (getClaims!=null)
                {
                    foreach (var item in getClaims)
                    {
                        lstRoleClaims.Items.Add(item.ClaimType + " : " + item.ClaimValue);
                        //txtRoleId.Text = item.RoleId.ToString();
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void btnAddpanel_Click(object sender, RoutedEventArgs e)
        {
            stkAddClaim.Visibility = Visibility.Visible;
            stkbtn.Visibility = Visibility.Collapsed;
        }

        private void btndelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _ = DeleteRoleClaim();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void btnAddClaim_Click(object sender, RoutedEventArgs e)
        {
            try
            {
               await AddRoleClaim();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnAddCancel_Click(object sender, RoutedEventArgs e)
        {
            stkAddClaim.Visibility = Visibility.Collapsed;
            stkbtn.Visibility = Visibility.Visible;
            Close();
        }
        public async Task AddRoleClaim()
        {
            if (txtClaimType.Text !=string.Empty && txtClaimValue.Text!=string.Empty)
            {
                RoleClaimModel roleClaimModel = new RoleClaimModel();
                roleClaimModel.ClaimType = txtClaimType.Text;
                roleClaimModel.ClaimValue = txtClaimValue.Text;
                roleClaimModel.RoleId = new Guid(txtRoleId.Text);

                Http.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Global.AccessToken);
                
                var roleurl = ApplicationConstants.BaseUrl + ApiRoutePathConstants.AddRoleClaim;
                var roleresponse =await Http.Client.PostAsync(
                    roleurl,
                    new StringContent(JsonConvert.SerializeObject(roleClaimModel), Encoding.UTF8, "application/json"));
                var roleResultResponse = roleresponse.Content.ReadAsStringAsync().Result;
                var claimresult = JsonConvert.DeserializeObject<FrameworkResult>(roleResultResponse);
                if (claimresult.Status== ResultStatus.Success)
                {
                    lstRoleClaims.Items.Add(roleClaimModel.ClaimType + " : " + roleClaimModel.ClaimValue);
                    MessageBox.Show("Role claim added successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    txtClaimType.Text = string.Empty;
                    txtClaimValue.Text = string.Empty;
                }
                else
                {
                    MessageBox.Show(claimresult.Errors.FirstOrDefault().Description, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    txtClaimType.Text = string.Empty;
                    txtClaimValue.Text = string.Empty;
                }
            }
            else
            {
                MessageBox.Show(" Please enter claimtype/claimvalue ", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public async Task DeleteRoleClaim()
        {
            if (lstRoleClaims.SelectedItems.Count > 0)
            {
                var getClaim = lstRoleClaims.SelectedItem.ToString().Split(":");
                RoleClaimModel roleClaimModel = new RoleClaimModel();
                roleClaimModel.ClaimType = getClaim[0].Trim().ToString();
                roleClaimModel.ClaimValue = getClaim[1].Trim().ToString();
                roleClaimModel.RoleId = new Guid(txtRoleId.Text);

                string message = "Are you sure want to delete role claim?";
                string caption = "Confirmation";
                MessageBoxButton buttons = MessageBoxButton.YesNo;
                MessageBoxImage icon = MessageBoxImage.Warning;

                if (MessageBox.Show(message, caption, buttons, icon) == MessageBoxResult.Yes)
                {
                    Http.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Global.AccessToken);

                    var roleurl = ApplicationConstants.BaseUrl + ApiRoutePathConstants.RemoveRoleClaim;
                    var roleresponse = Http.Client.PostAsync(roleurl, new StringContent(JsonConvert.SerializeObject(roleClaimModel), Encoding.UTF8, "application/json")).Result;
                    var roleResultResponse = roleresponse.Content.ReadAsStringAsync().Result;
                    var claimresult = JsonConvert.DeserializeObject<FrameworkResult>(roleResultResponse);

                    if (claimresult.Status == ResultStatus.Success)
                    {
                        lstRoleClaims.Items.Remove(lstRoleClaims.SelectedItem);
                        MessageBox.Show("Role claim deleted successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show(claimresult.Errors.FirstOrDefault().Description, "Errors", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select the roleclaim item", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}


