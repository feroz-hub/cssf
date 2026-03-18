/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using HCL.CS.SF.DemoClientWpfApp.Constants;
using HCL.CS.SF.DemoClientWpfApp.DomainModel;
using HCL.CS.SF.DemoClientWpfApp.Services;
using HCL.CS.SF.DemoClientWpfApp.ViewModel;

namespace HCL.CS.SF.DemoClientWpfApp.View.API
{
    public partial class UserClaimPopup : Window
    {
        ApplicationParameters ApplicationConstants = new ApplicationParameters();
        public UserClaimPopup()
        {
            InitializeComponent();
            getClaims();
        }
        private void getClaims()
        {
            try
            {
                stkAddClaim.Visibility = Visibility.Collapsed;
                stkbtn.Visibility = Visibility.Visible;

                var getClaims = Global.UserClaimModelGetData;
                foreach (var item in getClaims)
                {
                    if (item.ClaimType != null && item.ClaimValue != null)
                    {
                        lstUserClaims.Items.Add(item.ClaimType + " : " + item.ClaimValue);
                    }
                    txtUserId.Text = item.UserId.ToString();
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
                _ = DeleteUserClaim();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnAddClaim_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _ = AddUserClaim();
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
        public async Task AddUserClaim()
        {
            if (txtClaimType.Text != string.Empty && txtClaimValue.Text != string.Empty)
            {
                UserClaimModel userClaimModel = new UserClaimModel();
                userClaimModel.ClaimType = txtClaimType.Text;
                userClaimModel.ClaimValue = txtClaimValue.Text;
                userClaimModel.UserId = new Guid(txtUserId.Text);

                Http.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Global.AccessToken);

                var claimurl = ApplicationConstants.BaseUrl + ApiRoutePathConstants.AddAdminClaim;
                var claimresponse = Http.Client.PostAsync(claimurl, new StringContent(JsonConvert.SerializeObject(userClaimModel), Encoding.UTF8, "application/json")).Result;
                var claimResultResponse = claimresponse.Content.ReadAsStringAsync().Result;
                var claimresult = JsonConvert.DeserializeObject<FrameworkResult>(claimResultResponse);
                if (claimresult.Status == ResultStatus.Success)
                {
                    lstUserClaims.Items.Add(userClaimModel.ClaimType + " : " + userClaimModel.ClaimValue);
                    MessageBox.Show("User claim added successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    txtClaimType.Text = string.Empty;
                    txtClaimValue.Text = string.Empty;
                }
            }
            else
            {
                MessageBox.Show(" Please enter claimtype/claimvalue ", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public async Task DeleteUserClaim()
        {
            if (lstUserClaims.SelectedItems.Count > 0)
            {
                var getClaim = lstUserClaims.SelectedItem.ToString().Split(":");
                UserClaimModel userClaimModel = new UserClaimModel();
                userClaimModel.ClaimType = getClaim[0].Trim().ToString();
                userClaimModel.ClaimValue = getClaim[1].Trim().ToString();
                userClaimModel.UserId = new Guid(txtUserId.Text);

                string message = "Are you sure want to delete User claim?";
                string caption = "Confirmation";
                MessageBoxButton buttons = MessageBoxButton.YesNo;
                MessageBoxImage icon = MessageBoxImage.Warning;

                if (MessageBox.Show(message, caption, buttons, icon) == MessageBoxResult.Yes)
                {
                    Http.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Global.AccessToken);

                    var roleurl = ApplicationConstants.BaseUrl + ApiRoutePathConstants.RemoveAdminClaim;
                    var roleresponse = Http.Client.PostAsync(roleurl, new StringContent(JsonConvert.SerializeObject(userClaimModel), Encoding.UTF8, "application/json")).Result;
                    var roleResultResponse = roleresponse.Content.ReadAsStringAsync().Result;
                    var claimresult = JsonConvert.DeserializeObject<FrameworkResult>(roleResultResponse);

                    if (claimresult.Status == ResultStatus.Success)
                    {
                        lstUserClaims.Items.Remove(lstUserClaims.SelectedItem);
                        MessageBox.Show("User claim deleted successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        Close();
                    }
                    else
                    {
                        MessageBox.Show(claimresult.Errors.FirstOrDefault().Description, "Errors", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select the userclaim item", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}


