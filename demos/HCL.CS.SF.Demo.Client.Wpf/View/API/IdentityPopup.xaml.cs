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
using static HCL.CS.SF.DemoClientWpfApp.DomainModel.AllowedScopesParserModel;

namespace HCL.CS.SF.DemoClientWpfApp.View
{
    public partial class IdentityPopup : Window
    {
        ApplicationParameters ApplicationConstants = new ApplicationParameters();
        public string methodName;
        public AllowedScopesParserModel.IdentityResourcesModel IdentityResources;
        public AllowedScopesParserModel.IdentityClaimsModel IdentityClaims;
        public IdentityPopup()
        {
            InitializeComponent();
            GetResource();
        }
        public void GetResource()
        {
            try
            {
                methodName = Global.PopupMethodName;


                if (methodName == "AddIdentityResource")
                {
                    EnableResourcerInputControle();
                    DisableResourcerClaimInputControle();
                    IdentityResources = Global.IdentityResourcesGetdata;
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
                else if (methodName == "UpdateIdentityResource")
                {
                    EnableResourcerInputControle();
                    DisableResourcerClaimInputControle();
                    IdentityResources = Global.IdentityResourcesGetdata;
                    stkUpdate.Visibility = Visibility.Hidden;
                    lblName.Content = "Resource Name";
                    lblDisplayName.Content = "Resource Display Name";
                    lblDescription.Content = "Resource Description";

                    txtName.Text = IdentityResources.Name;
                    txtDisplayName.Text = IdentityResources.DisplayName;
                    txtDescription.Text = IdentityResources.Description;
                    stkUpdate.Visibility = Visibility.Visible;
                    stkAdd.Visibility = Visibility.Collapsed;

                }
                else if (methodName == "AddIdentityResourceClaim")
                {
                    DisableResourcerInputControle();
                    EnableResourcerClaimInputControle();
                    IdentityClaims = Global.IdentityResourcesClaimGetdata;
                    stkUpdate.Visibility = Visibility.Hidden;
                    lblClaimType.Content = "Claim Type";
                    txtClaimType.Text = string.Empty;

                    stkUpdate.Visibility = Visibility.Collapsed;
                    stkAdd.Visibility = Visibility.Visible;

                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
        private void Add_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _ = Add();
                Task.Delay(200000);
                Mediator.Notify("IdentityResourceScreen", "");
                this.Close();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }



        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _ = Update();
                Task.Delay(200000);
                Mediator.Notify("IdentityResourceScreen", "");
                Close();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async Task Add()
        {
            try
            {
                if (methodName == "AddIdentityResource")
                {
                    var identityResourcesModelInput = CreateIdentityResourceModel();


                    identityResourcesModelInput.Name = txtName.Text;
                    identityResourcesModelInput.DisplayName = txtDisplayName.Text;
                    identityResourcesModelInput.Description = txtDescription.Text;

                    Http.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Global.AccessToken);
                    Mouse.OverrideCursor = Cursors.Wait;
                    // Get ApiResource
                    var identityResourcesurl = ApplicationConstants.BaseUrl + ApiRoutePathConstants.AddIdentityResource;
                    var identityResponse = Http.Client.PostAsync(identityResourcesurl, new StringContent(JsonConvert.SerializeObject(identityResourcesModelInput), Encoding.UTF8, "application/json")).Result;
                    var identityResultResponse = identityResponse.Content.ReadAsStringAsync().Result;
                    var resultIdentityresourceModel = JsonConvert.DeserializeObject<FrameworkResult>(identityResultResponse);

                    if (resultIdentityresourceModel.Status == ResultStatus.Success)
                    {
                        Mouse.OverrideCursor = null;
                        MessageBox.Show("Identity resource added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    }
                    else
                    {
                        Mouse.OverrideCursor = null;
                        MessageBox.Show(resultIdentityresourceModel.Errors.FirstOrDefault().Description, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                }
                else if (methodName == "AddIdentityResourceClaim")
                {
                    IdentityClaims = Global.IdentityResourcesClaimGetdata;
                    var identityResourcesClaimModelInput = CreateIdentityResourceClaimModel();
                    identityResourcesClaimModelInput.IdentityResourceId = IdentityClaims.IdentityResourceId;
                    identityResourcesClaimModelInput.Type = txtClaimType.Text;

                    Http.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Global.AccessToken);
                    Mouse.OverrideCursor = Cursors.Wait;
                    // Get ApiResource
                    var identityResourcesurl = ApplicationConstants.BaseUrl + ApiRoutePathConstants.AddIdentityResourceClaim;
                    var identityResourcesresponse = Http.Client.PostAsync(identityResourcesurl, new StringContent(JsonConvert.SerializeObject(identityResourcesClaimModelInput), Encoding.UTF8, "application/json")).Result;
                    var identityResultResponse = identityResourcesresponse.Content.ReadAsStringAsync().Result;
                    var resultIdentityModel = JsonConvert.DeserializeObject<FrameworkResult>(identityResultResponse);

                    if (resultIdentityModel.Status == ResultStatus.Success)
                    {
                        Mouse.OverrideCursor = null;
                        MessageBox.Show("Identity resource claim added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    }
                    else
                    {
                        Mouse.OverrideCursor = null;
                        MessageBox.Show(resultIdentityModel.Errors.FirstOrDefault().Description, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task Update()
        {
            if (methodName == "UpdateIdentityResource")
            {
                var identityResourcesModelInput = Global.IdentityResourcesGetdata;
                identityResourcesModelInput.Name = txtName.Text;
                identityResourcesModelInput.DisplayName = txtDisplayName.Text;
                identityResourcesModelInput.Description = txtDescription.Text;

                Http.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Global.AccessToken);
                Mouse.OverrideCursor = Cursors.Wait;
                // Get ApiResource
                var identityResourcesurl = ApplicationConstants.BaseUrl + ApiRoutePathConstants.UpdateIdentityResource;
                var identityResourcesresponse = Http.Client.PostAsync(identityResourcesurl, new StringContent(JsonConvert.SerializeObject(identityResourcesModelInput), Encoding.UTF8, "application/json")).Result;
                var identityResultResponse = identityResourcesresponse.Content.ReadAsStringAsync().Result;
                var resultIdentityModel = JsonConvert.DeserializeObject<FrameworkResult>(identityResultResponse);

                if (resultIdentityModel.Status == ResultStatus.Success)
                {
                    Mouse.OverrideCursor = null;
                    MessageBox.Show("Identity resource updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                }
                else
                {
                    Mouse.OverrideCursor = null;
                    MessageBox.Show(resultIdentityModel.Errors.FirstOrDefault().Description, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }

        }

        public static AllowedScopesParserModel.IdentityResourcesModel CreateIdentityResourceModel()
        {
            AllowedScopesParserModel.IdentityResourcesModel identityResourceModel = new AllowedScopesParserModel.IdentityResourcesModel
            {
                Enabled = false,
                CreatedBy = "Test",
                IdentityClaims = new List<AllowedScopesParserModel.IdentityClaimsModel>()
                {
                    new AllowedScopesParserModel.IdentityClaimsModel() { Type = "Type1",CreatedBy = "Test" },
                },
            };
            return identityResourceModel;
        }

        public static AllowedScopesParserModel.IdentityClaimsModel CreateIdentityResourceClaimModel()
        {
            AllowedScopesParserModel.IdentityClaimsModel identityResourceClaimsModel = new AllowedScopesParserModel.IdentityClaimsModel
            {
                IdentityResourceId = Guid.NewGuid(),
                CreatedBy = "Test",
                Type = "Test Claim"
            };
            return identityResourceClaimsModel;
        }
    }
}


