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
using System.Windows.Controls;
using System.Windows.Input;
using HCL.CS.SF.DemoClientWpfApp.Components;
using HCL.CS.SF.DemoClientWpfApp.Constants;
using HCL.CS.SF.DemoClientWpfApp.DomainModel;
using HCL.CS.SF.DemoClientWpfApp.Interface;
using HCL.CS.SF.DemoClientWpfApp.Services;
using DomainModel_FrameworkResult = HCL.CS.SF.DemoClientWpfApp.DomainModel.FrameworkResult;
using FrameworkResult = HCL.CS.SF.DemoClientWpfApp.DomainModel.FrameworkResult;

namespace HCL.CS.SF.DemoClientWpfApp.ViewModel
{
    internal class ChangePasswordViewModel : BaseViewModel, IPageViewModel
    {
        ApplicationParameters ApplicationConstants = new ApplicationParameters();
        private string loginOldPassword;
        public RelayCommand HomeCommand { get; set; }
        public RelayCommand LoginPasswordChangeCommand { get; set; }
        public string LoginOldPassword
        {
            get
            {
                return loginOldPassword;
            }
            set
            {
                loginOldPassword = value;
                OnPropertyChanged("LoginOldPassword");
            }
        }
        public ChangePasswordViewModel()
        {
            HomeCommand = new RelayCommand(param => OnHome());
            LoginPasswordChangeCommand = new RelayCommand(param => ChangePassword(param), param => CanLogin());
        }
        private void OnHome()
        {
            Mediator.Notify("DashBoardScreen", "");
        }
        private bool CanLogin()
        {
            return true;
        }

        private async Task ChangePassword(object parameter)
        {
            try
            {

                Mouse.OverrideCursor = Cursors.Wait;
                string username = Global.PropertyUserName;

                PasswordBox passwordBox = (PasswordBox)parameter;
                if (string.IsNullOrWhiteSpace(passwordBox.Password))
                {
                    Mouse.OverrideCursor = null;
                    MessageBox.Show("Please enter new password.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                Http.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Global.AccessToken);

                string getUserByUserName_url = ApplicationConstants.BaseUrl + ApiRoutePathConstants.GetUserByName;
                HttpResponseMessage getUserByUserName_response = Http.Client.PostAsync(getUserByUserName_url, new StringContent(JsonConvert.SerializeObject(username), Encoding.UTF8, "application/json")).Result;
                string userDetails = getUserByUserName_response.Content.ReadAsStringAsync().Result;
                UserModel getUserDetailsByUserNameResult = JsonConvert.DeserializeObject<UserModel>(userDetails);

                if (getUserDetailsByUserNameResult != null)
                {
                    Guid userId = getUserDetailsByUserNameResult.Id;
                    string newPassword = passwordBox.Password;
                    string oldPassWord = LoginOldPassword;

                    var data = new
                    {
                        user_id = userId,
                        current_password = oldPassWord,
                        new_password = newPassword,
                    };

                    Http.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Global.AccessToken);

                    string changePassword_url = ApplicationConstants.BaseUrl + ApiRoutePathConstants.ChangePassword;
                    HttpResponseMessage response = Http.Client.PostAsync(changePassword_url, new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json")).Result;
                    string changePassword_Resopnse = response.Content.ReadAsStringAsync().Result;
                    DomainModel_FrameworkResult changePassword_Result = JsonConvert.DeserializeObject<DomainModel_FrameworkResult>(changePassword_Resopnse);

                    if (changePassword_Result.Status == ResultStatus.Success)
                    {
                        MessageBox.Show("Password changed sucessfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        Mediator.Notify("DashBoardScreen", "");
                    }
                    else
                    {
                        Mouse.OverrideCursor = null;
                        LoginOldPassword = string.Empty;
                        passwordBox.Password = string.Empty;
                        MessageBox.Show(changePassword_Result.Errors.FirstOrDefault().Description.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                Mouse.OverrideCursor = null;

            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}


