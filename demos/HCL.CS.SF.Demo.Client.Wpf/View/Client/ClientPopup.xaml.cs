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
    public partial class ClientPopup : Window
    {
        ApplicationParameters ApplicationConstants = new ApplicationParameters();
        public List<ClientRedirectUrisModel> ClientRedirectUris { get; set; }
        public List<ClientPostLogoutRedirectUrisModel> ClientPostLogoutRedirectUris { get; set; }
        public string methodName;
        public ClientsModel ClientsModel;
        public ClientPopup()
        {
            InitializeComponent();
            GetClient();
        }
        private void Add_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                methodName = Global.PopupMethodName;
                if (methodName == "AddClient")
                {
                    _ = AddClient();
                }
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
                _ = UpdateClient();
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
        private void GetClient()
        {
            try
            {
                methodName = Global.PopupMethodName;
                if (methodName == "AddClient")
                {
                    stkAdd.Visibility = Visibility.Visible;
                    stkUpdate.Visibility = Visibility.Collapsed;

                }
                else if (methodName == "UpdateClient")
                {
                    stkAdd.Visibility = Visibility.Collapsed;
                    stkUpdate.Visibility = Visibility.Visible;

                    ClientsModel = Global.ClientsGetdata;

                    txtClientName.Text = ClientsModel.ClientName;
                    txtClientUri.Text = ClientsModel.ClientUri;
                    txtAllowedScopes.Text = string.Join(" ", ClientsModel.AllowedScopes.ToArray());

                    if (ClientsModel.SupportedGrantTypes.Count == 5)
                    {
                        chkall.IsChecked = true;
                        cmbgranttype.SelectedIndex = 0;
                    }
                    if (ClientsModel.SupportedGrantTypes.Contains("authorization_code"))
                    {
                        chkAuth.IsChecked = true;
                    }
                    if (ClientsModel.SupportedGrantTypes.Contains("client_credentials"))
                    {
                        chkclient.IsChecked = true;
                    }
                    if (ClientsModel.SupportedGrantTypes.Contains("password"))
                    {
                        chkPassword.IsChecked = true;
                    }
                    if (ClientsModel.SupportedGrantTypes.Contains("refresh_token"))
                    {
                        chkRefresh.IsChecked = true;
                    }
                    if (ClientsModel.SupportedGrantTypes.Contains("hybrid"))
                    {
                        chkHybrid.IsChecked = true;
                    }

                    if (ClientsModel.SupportedResponseTypes.Count == 3)
                    {
                        chkallresponcetype.IsChecked = true;
                        cmbresponseType.SelectedIndex = 0;
                    }
                    if (ClientsModel.SupportedResponseTypes.Contains("code"))
                    {
                        chkcode.IsChecked = true;
                    }
                    if (ClientsModel.SupportedResponseTypes.Contains("id_token"))
                    {
                        chkidtoken.IsChecked = true;
                    }
                    if (ClientsModel.SupportedResponseTypes.Contains("token"))
                    {
                        chktoken.IsChecked = true;
                    }
                    txtAllowedSigningAlgorithm.Text = ClientsModel.AllowedSigningAlgorithm.ToString();
                    txtAuthCodeExpiration.Text = ClientsModel.AuthorizationCodeExpiration.ToString();
                    txtRefreshTokenExpiration.Text = ClientsModel.RefreshTokenExpiration.ToString();
                    txtIdentityTokenExpiration.Text = ClientsModel.IdentityTokenExpiration.ToString();
                    txtAccessTokenExpiration.Text = ClientsModel.AccessTokenExpiration.ToString();

                    foreach (var item in ClientsModel.RedirectUris)
                    {
                        listRedirectUri.Items.Add(item.RedirectUri);
                    }
                    foreach (var item in ClientsModel.PostLogoutRedirectUris)
                    {
                        listClientPostLogoutUri.Items.Add(item.PostLogoutRedirectUri);
                    }
                }
            }catch(Exception ex)
            { MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }
        private void All_Click(object sender, RoutedEventArgs e)
        {
            if (chkall.IsChecked == true)
            {
                chkAuth.IsChecked = true;
                chkclient.IsChecked = true;
                chkHybrid.IsChecked = true;
                chkPassword.IsChecked = true;
                chkRefresh.IsChecked = true;
            }
            else
            {
                chkAuth.IsChecked = false;
                chkclient.IsChecked = false;
                chkHybrid.IsChecked = false;
                chkPassword.IsChecked = false;
                chkRefresh.IsChecked = false;
            }
        }
        private void chkallresponcetype_Click(object sender, RoutedEventArgs e)
        {
            if (chkallresponcetype.IsChecked == true)
            {
                chkcode.IsChecked = true;
                chkidtoken.IsChecked = true;
                chktoken.IsChecked = true;
            }
            else
            {
                chkcode.IsChecked = false;
                chkidtoken.IsChecked = false;
                chktoken.IsChecked = false;
            }
        }
        private void btnAddRedirct_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                methodName = Global.PopupMethodName;
                if (txtAddRedirectUri.Text!=string.Empty || txtAddRedirectUri.Text != "")
                {
                    if (!listRedirectUri.Items.Contains(txtAddRedirectUri.Text))
                    {
                        listRedirectUri.Items.Add(txtAddRedirectUri.Text);
                        MessageBox.Show("RedirectUri added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void btndeleteRedirct_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                methodName = Global.PopupMethodName;
                
                if (listRedirectUri.Items.Count == 1)
                {
                    listClientPostLogoutUri.Items.Refresh();
                }

                listRedirectUri.Items.Remove(listRedirectUri.SelectedItem);
                MessageBox.Show("RedirctURI Removed successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void btnAddFrontChannelLogoutUri_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                methodName = Global.PopupMethodName;
                if (txtFrontChannelLogoutUri.Text != string.Empty || txtFrontChannelLogoutUri.Text != "")
                {
                    if (!listClientPostLogoutUri.Items.Contains(txtFrontChannelLogoutUri.Text))
                    {
                        listClientPostLogoutUri.Items.Add(txtFrontChannelLogoutUri.Text);
                        MessageBox.Show("ClientpostlogoutUri added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void btndeleteFrontChannelLogoutUri_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                methodName = Global.PopupMethodName;
                if (listClientPostLogoutUri.Items.Count == 1)
                {
                    listClientPostLogoutUri.Items.Refresh();
                }

                listClientPostLogoutUri.Items.Remove(listClientPostLogoutUri.SelectedItem);
                MessageBox.Show("ClientpostlogoutURI removed successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task AddClient()
        {
            if (InputValidtion() == false)
            {
                return;
            }
            var clientInputModel = CreateClientsModel();
            clientInputModel.ClientName = txtClientName.Text;
            clientInputModel.ClientUri = txtClientUri.Text;

            long number1 = 0;
            if (long.TryParse(txtAuthCodeExpiration.Text, out number1))
            {
                clientInputModel.AuthorizationCodeExpiration = Convert.ToInt32(txtAuthCodeExpiration.Text);
            }
            else
            {
                MessageBox.Show("Invalid Authorizationcode expiration value.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (long.TryParse(txtIdentityTokenExpiration.Text, out number1))
            {
                clientInputModel.IdentityTokenExpiration = Convert.ToInt32(txtIdentityTokenExpiration.Text);
            }
            else
            {
                MessageBox.Show("Invalid Identitytoken expiration value.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (long.TryParse(txtAccessTokenExpiration.Text, out number1))
            {
                clientInputModel.AccessTokenExpiration = Convert.ToInt32(txtAccessTokenExpiration.Text);
            }
            else
            {
                MessageBox.Show("Invalid Accesstoken expiration value.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (long.TryParse(txtRefreshTokenExpiration.Text, out number1))
            {
                clientInputModel.RefreshTokenExpiration = Convert.ToInt32(txtRefreshTokenExpiration.Text);
            }
            else
            {
                MessageBox.Show("Invalid Refreshtoken expiration value.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            List<string> lstscopes = new List<string>();
            string[] scopes = txtAllowedScopes.Text.Split(" ");
            foreach (var item in scopes)
            {
                 lstscopes.Add(item);
            }
            clientInputModel.AllowedScopes = lstscopes;
            

            clientInputModel.AllowedSigningAlgorithm = txtAllowedSigningAlgorithm.Text;

            ClientRedirectUris = new List<ClientRedirectUrisModel>();
            foreach (var item in listRedirectUri.Items)
            {
                ClientRedirectUris.Add(new ClientRedirectUrisModel { RedirectUri = item.ToString() });
            }
            ClientPostLogoutRedirectUris = new List<ClientPostLogoutRedirectUrisModel>();
            foreach (var item in listClientPostLogoutUri.Items)
            {
                ClientPostLogoutRedirectUris.Add(new ClientPostLogoutRedirectUrisModel { PostLogoutRedirectUri = item.ToString() });
            }

            clientInputModel.RedirectUris = ClientRedirectUris;
            clientInputModel.PostLogoutRedirectUris = ClientPostLogoutRedirectUris;

            List<string> grantTypes = new List<string>();
            if (chkall.IsChecked == true)
            {
                grantTypes = new List<string>() { "authorization_code", "client_credentials", "password", "refresh_token", "hybrid" };
            }
            else if (chkAuth.IsChecked == true) { grantTypes.Add("authorization_code"); }
            if (chkclient.IsChecked == true) { grantTypes.Add("client_credentials"); }
            if (chkHybrid.IsChecked == true) { grantTypes.Add("hybrid"); }
            if (chkRefresh.IsChecked == true) { grantTypes.Add("refresh_token"); }
            if (chkPassword.IsChecked == true) { grantTypes.Add("password"); }


            List<string> responseTypes = new List<string>();
            if (chkallresponcetype.IsChecked == true)
            {
                responseTypes = new List<string>() { "code token id_token" };
            }
            else if (chkcode.IsChecked == true) { responseTypes.Add("code"); }
            if (chktoken.IsChecked == true) { responseTypes.Add("token"); }
            if (chkidtoken.IsChecked == true) { responseTypes.Add("id_token"); }

            if (grantTypes.Count < 1)
            {
                MessageBox.Show("SupportedGrantTypes are required.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (responseTypes.Count < 1)
            {
                MessageBox.Show("Supported Response types are required.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            clientInputModel.SupportedGrantTypes = grantTypes;
            clientInputModel.SupportedResponseTypes = responseTypes;

            Http.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Global.AccessToken);

            var clienturl = ApplicationConstants.BaseUrl + ApiRoutePathConstants.RegisterClient;
            var roleResponse = Http.Client.PostAsync(clienturl, new StringContent(JsonConvert.SerializeObject(clientInputModel), Encoding.UTF8, "application/json")).Result;
            var clientResultResponse = roleResponse.Content.ReadAsStringAsync().Result;
            var clientRoleresourceModel = JsonConvert.DeserializeObject<ClientsModel>(clientResultResponse);
            if (clientRoleresourceModel.ClientId != null)
            {
               
                MessageBox.Show("Client added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
                Mediator.Notify("ClientScreen", "");
            }
            else
            {
              
                MessageBox.Show(clientRoleresourceModel.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
                Mediator.Notify("ClientScreen", "");
            }
        }
        private async Task UpdateClient()
        {
            var clientInputModel = Global.ClientsGetdata;
            if (InputValidtion() == false)
            {
                return;
            }

            clientInputModel.ClientName = txtClientName.Text;
            clientInputModel.ClientUri = txtClientUri.Text;

            long number1 = 0;
            if (long.TryParse(txtAuthCodeExpiration.Text, out number1))
            {
                clientInputModel.AuthorizationCodeExpiration = Convert.ToInt32(txtAuthCodeExpiration.Text);
            }
            else
            {
                MessageBox.Show("Invalid Authorizationcode expiration value.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (long.TryParse(txtIdentityTokenExpiration.Text, out number1))
            {
                clientInputModel.IdentityTokenExpiration = Convert.ToInt32(txtIdentityTokenExpiration.Text);
            }
            else
            {
                MessageBox.Show("Invalid Identitytoken expiration value.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (long.TryParse(txtAccessTokenExpiration.Text, out number1))
            {
                clientInputModel.AccessTokenExpiration = Convert.ToInt32(txtAccessTokenExpiration.Text);
            }
            else
            {
                MessageBox.Show("Invalid Accesstoken expiration value.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (long.TryParse(txtRefreshTokenExpiration.Text, out number1))
            {
                clientInputModel.RefreshTokenExpiration = Convert.ToInt32(txtRefreshTokenExpiration.Text);
            }
            else
            {
                MessageBox.Show("Invalid Refreshtoken expiration value.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            clientInputModel.AllowedScopes= txtAllowedScopes.Text.Split(" ").ToList();
            clientInputModel.AllowedSigningAlgorithm = txtAllowedSigningAlgorithm.Text;
            List<string> redirectUris = new List<string>();
            List<string> PostLogoutUris = new List<string>();
            foreach (var item in listRedirectUri.Items)
            {
                redirectUris.Add(item.ToString());
            }
            foreach (var item in listClientPostLogoutUri.Items)
            {
                PostLogoutUris.Add(item.ToString());
            }
            if (clientInputModel.RedirectUris.Count > redirectUris.Count)
            {
                //delete
                for (int i = 0; i < clientInputModel.RedirectUris.Count; i++)
                {
                    if (!redirectUris.Contains(clientInputModel.RedirectUris[i].RedirectUri))
                    {
                        clientInputModel.RedirectUris.Remove(clientInputModel.RedirectUris[i]);
                    }
                }
            }
            else if (clientInputModel.RedirectUris.Count < redirectUris.Count)
            {
                foreach (var item in clientInputModel.RedirectUris)
                {
                    if (redirectUris.Contains(item.RedirectUri))
                    {
                        redirectUris.Remove(item.RedirectUri);
                    }
                }
                foreach (var item in redirectUris)
                {
                    clientInputModel.RedirectUris.Add(new ClientRedirectUrisModel() { ClientId= clientInputModel.Id, RedirectUri = item.ToString(),});
                }
            }

            // Post Logout
            if (clientInputModel.PostLogoutRedirectUris.Count > PostLogoutUris.Count)
            {
                //delete
                for (int i = 0; i < clientInputModel.PostLogoutRedirectUris.Count; i++)
                {
                    if (!PostLogoutUris.Contains(clientInputModel.PostLogoutRedirectUris[i].PostLogoutRedirectUri))
                    {
                        clientInputModel.PostLogoutRedirectUris.Remove(clientInputModel.PostLogoutRedirectUris[i]);
                    }
                }
            }
            else if (clientInputModel.PostLogoutRedirectUris.Count < PostLogoutUris.Count)
            {
                foreach (var item in clientInputModel.PostLogoutRedirectUris)
                {
                    if (PostLogoutUris.Contains(item.PostLogoutRedirectUri))
                    {
                        PostLogoutUris.Remove(item.PostLogoutRedirectUri);
                    }
                }
                foreach (var item in PostLogoutUris)
                {
                    clientInputModel.PostLogoutRedirectUris.Add(new ClientPostLogoutRedirectUrisModel() { ClientId = clientInputModel.Id, PostLogoutRedirectUri = item.ToString(), });
                }
            }

            clientInputModel.PolicyUri = clientInputModel.ClientUri.ToString();
            clientInputModel.TermsOfServiceUri = clientInputModel.ClientUri.ToString();
            clientInputModel.LogoUri = clientInputModel.ClientUri.ToString();

            List<string> grantTypes = new List<string>();
            if (chkall.IsChecked == true)
            {
                grantTypes = new List<string>() { "authorization_code", "client_credentials", "password", "refresh_token", "hybrid" };
            }
            else if (chkAuth.IsChecked == true) { grantTypes.Add("authorization_code"); }
            if (chkclient.IsChecked == true) { grantTypes.Add("client_credentials"); }
            if (chkHybrid.IsChecked == true) { grantTypes.Add("hybrid"); }
            if (chkRefresh.IsChecked == true) { grantTypes.Add("refresh_token"); }
            if (chkPassword.IsChecked == true) { grantTypes.Add("password"); }

            List<string> responseTypes = new List<string>();
            if (chkallresponcetype.IsChecked == true)
            {
                responseTypes = new List<string>() { "code token id_token" };
            }
            else if (chkcode.IsChecked == true) { responseTypes.Add("code"); }
            if (chkidtoken.IsChecked == true) { responseTypes.Add("token"); }
            if (chkidtoken.IsChecked == true) { responseTypes.Add("id_token"); }

            if (grantTypes.Count < 1)
            {
                MessageBox.Show("SupportedGrantTypes are required.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (responseTypes.Count < 1)
            {
                MessageBox.Show("Supported Response types are required.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            clientInputModel.SupportedGrantTypes = grantTypes;
            clientInputModel.SupportedResponseTypes = responseTypes;
           

            Http.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Global.AccessToken);
            Mouse.OverrideCursor = Cursors.Wait;
           
            var clienturl = ApplicationConstants.BaseUrl + ApiRoutePathConstants.UpdateClient;
            var roleResponse = Http.Client.PostAsync(clienturl, new StringContent(JsonConvert.SerializeObject(clientInputModel), Encoding.UTF8, "application/json")).Result;

            if (roleResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var clientResultResponse = roleResponse.Content.ReadAsStringAsync().Result;
                var clientRoleresourceModel = JsonConvert.DeserializeObject<ClientsModel>(clientResultResponse);
                if (clientRoleresourceModel.ClientId != null)
                {
                    Mouse.OverrideCursor = null;
                    MessageBox.Show("Client updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    Close();
                    Mediator.Notify("ClientScreen", "");
                }
            }
            else
            {
                var clientResultResponse = roleResponse.Content.ReadAsStringAsync().Result;
                var clientRoleresourceModel = JsonConvert.DeserializeObject(clientResultResponse);
                Mouse.OverrideCursor = null;
                MessageBox.Show(clientRoleresourceModel.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            Mouse.OverrideCursor = null;
        }
        private bool InputValidtion()
        {
            if (txtClientName.Text == string.Empty || txtClientName.Text == null)
            {
                MessageBox.Show("Clientname is required", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (txtClientUri.Text == string.Empty || txtClientUri.Text == null)
            {
                MessageBox.Show("Clienturi is required", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (txtAllowedScopes.Text == string.Empty || txtAllowedScopes.Text == null)
            {
                MessageBox.Show("AllowedScopes is required", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (listRedirectUri.Items.Count == 0 || listRedirectUri.Items == null)
            {
                MessageBox.Show("RedirectUri is required", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (listClientPostLogoutUri.Items.Count == 0 || listClientPostLogoutUri.Items == null)
            {
                MessageBox.Show("Front Channel LogoutUri is required", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (txtAuthCodeExpiration.Text == string.Empty || txtAuthCodeExpiration.Text == null)
            {
                MessageBox.Show("Authorization code expiration value is required", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (txtRefreshTokenExpiration.Text == string.Empty || txtRefreshTokenExpiration.Text == null)
            {
                MessageBox.Show("Refresh token expiration value is required", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (txtIdentityTokenExpiration.Text == string.Empty || txtIdentityTokenExpiration.Text == null)
            {
                MessageBox.Show("Identity token expiration value is required", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (txtIdentityTokenExpiration.Text == string.Empty || txtIdentityTokenExpiration.Text == null)
            {
                MessageBox.Show("Identity token expiration value is required", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (txtAccessTokenExpiration.Text == string.Empty || txtAccessTokenExpiration.Text == null)
            {
                MessageBox.Show("Access token expiration value is required", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }
        public static ClientsModel CreateClientsModel()
        {
            ClientsModel clientModel = new ClientsModel
            {
                ClientSecretExpiresAt = DateTime.Now.AddDays(10),
                LogoUri = "https://localhost:44300/logo.png",
                TermsOfServiceUri = "https://localhost:44300/Tos.cshtml",
                PolicyUri = "https://localhost:44300/Policy.cshtml",

                AccessTokenType = 0,
                RequirePkce = true,
                IsPkceTextPlain = true,
                RequireClientSecret = true,
                IsFirstPartyApp = false,

                RedirectUris = new List<ClientRedirectUrisModel> {
                    new ClientRedirectUrisModel { RedirectUri = "https://localhost:44300/index.html" },
                    new ClientRedirectUrisModel { RedirectUri = "https://localhost:44300/callback.html" }
                },

                PostLogoutRedirectUris = new List<ClientPostLogoutRedirectUrisModel> {
                    new ClientPostLogoutRedirectUrisModel { PostLogoutRedirectUri = "https://localhost:44300/index.html" },
                    new ClientPostLogoutRedirectUrisModel { PostLogoutRedirectUri = "https://localhost:44300/callback.html" }
                },

                AllowedSigningAlgorithm = Algorithms.HmacSha512,

                AllowOfflineAccess = true,

                ApplicationType = ApplicationType.SinglePageApp,
            };
            return clientModel;
        }
    }
}


