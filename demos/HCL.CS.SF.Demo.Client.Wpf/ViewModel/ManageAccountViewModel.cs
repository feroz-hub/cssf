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
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using IronBarCode;
using HCL.CS.SF.DemoClientWpfApp.View;
using System.Security.Policy;
using System.Text.Json.Nodes;
using HCL.CS.SF.DemoClientWpfApp.Components;
using HCL.CS.SF.DemoClientWpfApp.Constants;
using HCL.CS.SF.DemoClientWpfApp.DomainModel;
using HCL.CS.SF.DemoClientWpfApp.Interface;
using HCL.CS.SF.DemoClientWpfApp.Services;

namespace HCL.CS.SF.DemoClientWpfApp.ViewModel
{
    internal class ManageAccountViewModel : BaseViewModel, IPageViewModel
    {
        ApplicationParameters ApplicationConstants = new ApplicationParameters();
        private bool updateProfileTab;
        private string qrCodeVisibilty;
        private string recoveryCodeTextVisibilty;
        private string recoveryCode;

        public RelayCommand GotoLoginCommand { get; set; }
        public RelayCommand twoFactorPhoneCommand { get; set; }
        public RelayCommand twoFactorEmailCommand { get; set; }
        public RelayCommand twoFactorAuthenticatorCommand { get; set; }


        private ImageSource image;
        public ImageSource Picture
        {
            get { return image; }
            set
            {
                image = value;
                OnPropertyChanged("Picture");
            }
        }

        public string QrCodeVisibilty
        {
            get
            {
                return qrCodeVisibilty;
            }
            set
            {
                qrCodeVisibilty = value;
                OnPropertyChanged("QrCodeVisibilty");
            }
        }
        public string RecoveryCodeTextVisibilty
        {
            get
            {
                return recoveryCodeTextVisibilty;
            }
            set
            {
                recoveryCodeTextVisibilty = value;
                OnPropertyChanged("RecoveryCodeTextVisibilty");
            }
        }

        public string RecoveryCode
        {
            get
            {
                return recoveryCode;
            }
            set
            {
                recoveryCode = value;
                OnPropertyChanged("RecoveryCode");
            }
        }

        public RelayCommand OnLoadCommand { get; set; }
        public RelayCommand DashBoardCommand { get; set; }
        public RelayCommand GenerateRecoveryCodeCommand { get; set; }
        public RelayCommand TwoFactorNoneCommand { get; set; }
        public ManageAccountViewModel()
        {
            OnLoadCommand = new RelayCommand(param => OnLoad());
            twoFactorPhoneCommand = new RelayCommand(param => TwoFactorPhone());
            twoFactorEmailCommand = new RelayCommand(param => TwoFactorEmail());
            twoFactorAuthenticatorCommand = new RelayCommand(param => TwoFactorAuthenticatorapp());

            // Master Commads
            DashBoardCommand = new RelayCommand(param => OnDashBoard());
            GenerateRecoveryCodeCommand = new RelayCommand(param => GetRecoverycodes());
            TwoFactorNoneCommand = new RelayCommand(param => Twofactornone());
        }

        private void OnDashBoard()
        {
            Mouse.OverrideCursor = null;
            Mediator.Notify("DashBoardScreen", "");
        }
        public void OnLoad()
        {
            Mouse.OverrideCursor = null;
            QrCodeVisibilty = "Hidden";
            RecoveryCodeTextVisibilty = "Hidden";
        }
        public async Task Twofactornone()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                RecoveryCodeTextVisibilty = "Hidden";
                QrCodeVisibilty = "Hidden";

                Http.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Global.AccessToken);
                string userName = Global.PropertyUserName;
                var getUser_url = ApplicationConstants.BaseUrl + ApiRoutePathConstants.GetUserByName;
                
                var getUser_response = Http.Client.PostAsync(getUser_url, new StringContent(JsonConvert.SerializeObject(userName), Encoding.UTF8, "application/json")).Result;

                 var userDetails = getUser_response.Content.ReadAsStringAsync().Result;
                var getUserResult = JsonConvert.DeserializeObject<UserModel>(userDetails);


                if (getUserResult.UserName != null)
                {
                    Http.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Global.AccessToken);
                    var updateUser_Url = ApplicationConstants.BaseUrl + ApiRoutePathConstants.ResetAuthenticatorApp;
                    var updateUser_response = Http.Client.PostAsync(updateUser_Url, new StringContent(JsonConvert.SerializeObject(getUserResult.Id), Encoding.UTF8, "application/json")).Result;
                    var updated_userDetails = updateUser_response.Content.ReadAsStringAsync().Result;
                    FrameworkResult updateUser_result = JsonConvert.DeserializeObject<FrameworkResult>(updated_userDetails);

                    if (updateUser_result.Status == ResultStatus.Success)
                    {
                        Mouse.OverrideCursor = null;
                        MessageBox.Show("Twofactor authentication disabled successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                        Http.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Global.AccessToken);
                        Mediator.Notify("DashBoardScreen", "");
                    }
                    else
                    {
                        Mouse.OverrideCursor = null;
                        MessageBox.Show(updateUser_result.Errors.FirstOrDefault().Description, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            Mouse.OverrideCursor = null;
        }
        public async Task GetRecoverycodes()
        {
            try
            {
                QrCodeVisibilty = "Collapsed";
                Mouse.OverrideCursor = Cursors.Wait;
                Http.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Global.AccessToken);
                string userName = Global.PropertyUserName;
                var getUserByUserName_url = ApplicationConstants.BaseUrl + ApiRoutePathConstants.GetUserByName;
                var getUserByUserName_response = Http.Client.PostAsync(getUserByUserName_url, new StringContent(JsonConvert.SerializeObject(userName), Encoding.UTF8, "application/json")).Result;
                var userDetails = getUserByUserName_response.Content.ReadAsStringAsync().Result;
                var getUserDetailsByUserNameResult = JsonConvert.DeserializeObject<UserModel>(userDetails);

                if (getUserDetailsByUserNameResult.UserName != null)
                {
                    HttpService httpService = new HttpService();
                    var recoveryCodeCount = httpService.PostSecureAsync<int>(ApiRoutePathConstants.CountRecoveryCodesAsync, getUserDetailsByUserNameResult.Id).Result;
                    if (recoveryCodeCount <= 0)
                    {
                        var getrecoveryCodes = httpService.PostSecureAsync<IEnumerable<string>>(ApiRoutePathConstants.GenerateRecoveryCodes, getUserDetailsByUserNameResult.Id).Result;
                        if (getrecoveryCodes != null)
                        {
                            var recoveryCodes = string.Join(",", getrecoveryCodes.ToArray());
                            RecoveryCode = recoveryCodes;
                            RecoveryCodeTextVisibilty = "Visible";
                            MessageBox.Show("Recoverycodes generated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            RecoveryCodeTextVisibilty = "Collapsed";
                        }

                    }
                    else
                    {
                        RecoveryCodeTextVisibilty = "Collapsed";
                        MessageBox.Show("Exceeded the limit of recovery code generation ", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            Mouse.OverrideCursor = null;
        }
        private async Task TwoFactorPhone()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                RecoveryCodeTextVisibilty = "Collapsed";
                QrCodeVisibilty = "Collapsed";

                Http.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Global.AccessToken);
                string userName = Global.PropertyUserName;
                var getUser_url = ApplicationConstants.BaseUrl + ApiRoutePathConstants.GetUserByName;
                var getUser_response = Http.Client.PostAsync(getUser_url, new StringContent(JsonConvert.SerializeObject(userName), Encoding.UTF8, "application/json")).Result;
                var userDetails = getUser_response.Content.ReadAsStringAsync().Result;
                var getUserResult = JsonConvert.DeserializeObject<UserModel>(userDetails);

                if (getUserResult.UserName != null)
                {
                    getUserResult.TwoFactorEnabled = true;
                    getUserResult.TwoFactorType = TwoFactorType.Sms;

                    Http.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Global.AccessToken);

                    var updateUser_Url = ApplicationConstants.BaseUrl + ApiRoutePathConstants.UpdateUser;
                    var updateUser_response = Http.Client.PostAsync(updateUser_Url, new StringContent(JsonConvert.SerializeObject(getUserResult), Encoding.UTF8, "application/json")).Result;
                    var updated_userDetails = updateUser_response.Content.ReadAsStringAsync().Result;
                    FrameworkResult updateUser_result = JsonConvert.DeserializeObject<FrameworkResult>(updated_userDetails);

                    if (updateUser_result.Status == ResultStatus.Success)
                    {
                        Mouse.OverrideCursor = null;
                        MessageBox.Show("Twofactor phone authentication successfully updated.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        Http.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Global.AccessToken);
                        Mediator.Notify("DashBoardScreen", "");
                    }
                    else
                    {
                        MessageBox.Show(updateUser_result.Errors.FirstOrDefault().Description, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            Mouse.OverrideCursor = null;
        }
        private async Task TwoFactorEmail()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                RecoveryCodeTextVisibilty = "Collapsed";
                QrCodeVisibilty = "Collapsed";
                string userName = Global.PropertyUserName;
                Http.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Global.AccessToken);

                var getUser_url = ApplicationConstants.BaseUrl + ApiRoutePathConstants.GetUserByName;
                var getUser_response = Http.Client.PostAsync(getUser_url, new StringContent(JsonConvert.SerializeObject(userName), Encoding.UTF8, "application/json")).Result;
                var userDetails = getUser_response.Content.ReadAsStringAsync().Result;
                UserModel getUserResult = JsonConvert.DeserializeObject<UserModel>(userDetails);

                if (getUserResult.UserName != null)
                {

                    getUserResult.TwoFactorEnabled = true;
                    getUserResult.TwoFactorType = TwoFactorType.Email;
                    Http.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Global.AccessToken);
                    var updateUser_Url = ApplicationConstants.BaseUrl + ApiRoutePathConstants.UpdateUser;
                    var updateUser_response = Http.Client.PostAsync(updateUser_Url, new StringContent(JsonConvert.SerializeObject(getUserResult), Encoding.UTF8, "application/json")).Result;
                    var updated_userDetails = updateUser_response.Content.ReadAsStringAsync().Result;
                    FrameworkResult updateUser_result = JsonConvert.DeserializeObject<FrameworkResult>(updated_userDetails);
                    if (updateUser_result.Status == ResultStatus.Success)
                    {
                        MessageBox.Show("Twofactor email authentication updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show(updateUser_result.Errors.FirstOrDefault().Description, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            Mouse.OverrideCursor = null;
        }
        private async Task TwoFactorAuthenticatorapp()
        {
            try
            {
                RecoveryCodeTextVisibilty = "Collapsed";
                Mouse.OverrideCursor = Cursors.Wait;
                string userName = Global.PropertyUserName;
                Http.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Global.AccessToken);

                var getUser_url = ApplicationConstants.BaseUrl + ApiRoutePathConstants.GetUserByName;
                var getUser_response = Http.Client.PostAsync(getUser_url, new StringContent(JsonConvert.SerializeObject(userName), Encoding.UTF8, "application/json")).Result;
                var userDetails = getUser_response.Content.ReadAsStringAsync().Result;
                UserModel getUserResult = JsonConvert.DeserializeObject<UserModel>(userDetails);

                if (getUserResult.UserName != null)
                {
                    getUserResult.TwoFactorEnabled = true;
                    getUserResult.TwoFactorType = TwoFactorType.AuthenticatorApp;
                    Http.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Global.AccessToken);
                    var updateUser_Url = ApplicationConstants.BaseUrl + ApiRoutePathConstants.UpdateUser;
                    var updateUser_response = Http.Client.PostAsync(updateUser_Url, new StringContent(JsonConvert.SerializeObject(getUserResult), Encoding.UTF8, "application/json")).Result;
                    var updated_userDetails = updateUser_response.Content.ReadAsStringAsync().Result;
                    FrameworkResult updateUser_result = JsonConvert.DeserializeObject<FrameworkResult>(updated_userDetails);
                    if (updateUser_result.Status == ResultStatus.Success)
                    {
                        Mouse.OverrideCursor = null;
                        MessageBox.Show("Twofactor authenticator app authentication updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                        if (getUserResult.Id != null)
                        {
                            var data = new
                            {
                                user_id = getUserResult.Id,
                                application_name = "HCL.CS.SF HCL.CS.SF",

                            };

                            var setupAuthenticator_SignInUrl = ApplicationConstants.BaseUrl + ApiRoutePathConstants.SetupAuthenticatorApp;
                            var setupAuthenticatorresponse = Http.Client.PostAsync(setupAuthenticator_SignInUrl, new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json")).Result;
                            var setupAuthenticatorresponse_SignIn_Resopnse = setupAuthenticatorresponse.Content.ReadAsStringAsync().Result;
                            var authenticator_SignIn_Result = JsonConvert.DeserializeObject<AuthenticatorAppSetupResponseModel>(setupAuthenticatorresponse_SignIn_Resopnse);
                            var value = HttpUtility.UrlDecode(authenticator_SignIn_Result.AuthenticatorUri);
                            if (authenticator_SignIn_Result.AuthenticatorUri != null)
                            {
                                var myBarcode = BarcodeWriter.CreateBarcode(value, BarcodeWriterEncoding.QRCode);
                                myBarcode.ResizeTo(300, 300);
                                var image = new Bitmap(myBarcode.ToBitmap());
                                
                                using (var stream = new MemoryStream())
                                {
                                    image.Save(stream, ImageFormat.Png);

                                    BitmapImage bi = new BitmapImage();
                                    bi.BeginInit();
                                    stream.Seek(0, SeekOrigin.Begin);
                                    bi.StreamSource = stream;
                                    bi.CacheOption = BitmapCacheOption.OnLoad;
                                    bi.EndInit();
                                    Picture = bi; //A WPF Image control
                                    QrCodeVisibilty = "Visible";
                                }
                            }
                        }
                    }
                    else
                    {
                        Mouse.OverrideCursor = null;
                        QrCodeVisibilty = "Collapsed";
                        MessageBox.Show(updateUser_result.Errors.FirstOrDefault().Description, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            Mouse.OverrideCursor = null;
        }
    }
}


