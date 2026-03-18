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
using NotificationTypes = HCL.CS.SF.DemoClientWpfApp.DomainModel.NotificationTypes;

namespace HCL.CS.SF.DemoClientWpfApp.ViewModel
{
    internal  class ResetPasswordViewModel : BaseViewModel, IPageViewModel
    {
        ApplicationParameters ApplicationConstants = new ApplicationParameters();
        private string resetTokenUserName;
        private string resetPasswordToken;
        public string ResetTokenUserName
        {
            get
            {
                return resetTokenUserName;
            }
            set
            {
                resetTokenUserName = value;
                OnPropertyChanged("ResetUserNameToken");
            }
        }
        public string ResetPasswordToken
        {
            get
            {
                return resetPasswordToken;
            }
            set
            {
                resetPasswordToken = value;
                OnPropertyChanged("ResetPasswordToken");
            }
        }
        public string resetPanelVisibility;
        public string ResetPanelVisibility
        {
            get
            {
                return resetPanelVisibility;
            }
            set
            {
                resetPanelVisibility = value;
                OnPropertyChanged("ResetPanelVisibility");
            }
        }

        public RelayCommand OnLoadCommand { get; set; }
        public RelayCommand ResetPasswordTokenCommand { get; set; }
        public RelayCommand ResetPasswordChangeCommand { get; set; }
        public ResetPasswordViewModel()
        {
            ResetPasswordTokenCommand = new RelayCommand(param => ResetPasswordGenerateToken());
            ResetPasswordChangeCommand = new RelayCommand(param => ResetPassword(param));
            OnLoadCommand = new RelayCommand(param => Onload());
        }
        private void Onload()
        {
            ResetPanelVisibility = "Collapsed";
            ResetTokenUserName = Global.PropertyUserName;
        }
        public async Task ResetPasswordGenerateToken()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                if (ResetTokenUserName == null || ResetTokenUserName == string.Empty)
                {
                    Mouse.OverrideCursor = null;
                    MessageBox.Show("User name should not be smpty", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                var userName = ResetTokenUserName;
                Global.PropertyUserName = userName;

                var data = new
                {
                    user_name = userName,
                    notification_type = NotificationTypes.SMS,
                };

                var generateConfirmationToken_Url = ApplicationConstants.BaseUrl + ApiRoutePathConstants.GeneratePasswordResetToken;
                var generateToken_response = Http.Client.PostAsync(generateConfirmationToken_Url, new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json")).Result;
                var generateTokennResult = generateToken_response.Content.ReadAsStringAsync().Result;
                DomainModel_FrameworkResult frameworkPhoneResult = JsonConvert.DeserializeObject<DomainModel_FrameworkResult>(generateTokennResult);

                if (frameworkPhoneResult.Status == ResultStatus.Success)
                {
                    ResetPanelVisibility = "Visible";
                    MessageBox.Show("Reset token successfully sent to your registered mobile number.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    Mouse.OverrideCursor = null;
                    MessageBox.Show(frameworkPhoneResult.Errors.FirstOrDefault().Description, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            Mouse.OverrideCursor = null;
        }

        public async Task ResetPassword(Object parameter)
        {
            try
            {
                if (ResetPasswordToken == null || ResetPasswordToken == string.Empty)
                {
                    MessageBox.Show("Token should not be empty", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                var passwordBox = (PasswordBox)parameter;
                if (string.IsNullOrWhiteSpace(passwordBox.Password))
                {
                    MessageBox.Show("Password should not be empty", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                var data = new
                {
                    user_name = ResetTokenUserName,
                    password_reset_token = ResetPasswordToken,
                    new_password = passwordBox.Password,
                };

                var generateToken_Url = ApplicationConstants.BaseUrl + ApiRoutePathConstants.ResetPassword;
                var generateToken_response = Http.Client.PostAsync(generateToken_Url,
                                                               new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json")).Result;
                var generateTokenResult = generateToken_response.Content.ReadAsStringAsync().Result;
                DomainModel_FrameworkResult frameworkPhoneResult = JsonConvert.DeserializeObject<DomainModel_FrameworkResult>(generateTokenResult);

                if (frameworkPhoneResult.Status == ResultStatus.Success)
                {
                    MessageBox.Show("Reset password successful.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    ResetPasswordToken = "";
                    Mediator.Notify("LoginScreen", "");
                }
                else
                {
                    MessageBox.Show("Password reset failed..", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}


