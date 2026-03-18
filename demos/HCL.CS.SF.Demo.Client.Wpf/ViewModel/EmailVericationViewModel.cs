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
using System.Windows.Input;
using HCL.CS.SF.DemoClientWpfApp.Components;
using HCL.CS.SF.DemoClientWpfApp.Constants;
using HCL.CS.SF.DemoClientWpfApp.Interface;
using HCL.CS.SF.DemoClientWpfApp.Services;
using DomainModel_FrameworkResult = HCL.CS.SF.DemoClientWpfApp.DomainModel.FrameworkResult;
using FrameworkResult = HCL.CS.SF.DemoClientWpfApp.DomainModel.FrameworkResult;

namespace HCL.CS.SF.DemoClientWpfApp.ViewModel
{
    internal class EmailVericationViewModel : BaseViewModel, IPageViewModel
    {
        ApplicationParameters ApplicationConstants = new ApplicationParameters();
        private string emailericationcode;
        private string phoneVerificationCode;

        public RelayCommand OnLoadCommand { get; set; }
        public string EmailVerificationCode
        {
            get
            {
                return emailericationcode;
            }
            set
            {
                emailericationcode = value;
                OnPropertyChanged("EmailVerificationCode");
            }
        }
        public string PhoneVerificationCode
        {
            get
            {
                return phoneVerificationCode;
            }
            set
            {
                phoneVerificationCode = value;
                OnPropertyChanged("PhoneVerificationCode");
            }
        }
        public RelayCommand HomeCommand { get; set; }
        public RelayCommand VerifyTokenCommand { get; set; }
        public RelayCommand GotoLoginCommand { get; set; }
        public RelayCommand ResendEmailCommand { get; set; }
        public RelayCommand ResendPhoneCommand { get; set; }
        public RelayCommand VerifyTokenCancelCommand { get; set; }
        public EmailVericationViewModel()
        {
            HomeCommand = new RelayCommand(param => OnHome());
            VerifyTokenCommand = new RelayCommand(param => VerifyAccount());
            GotoLoginCommand = new RelayCommand(param => GotoLogin());
            OnLoadCommand = new RelayCommand(param => OnLoadAsync());
            ResendEmailCommand = new RelayCommand(param => ResendEmail());
            ResendPhoneCommand = new RelayCommand(param => ResendPhone());
            VerifyTokenCancelCommand = new RelayCommand(param => OnCancel());
        }

        public async Task VerifyAccount()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                if (EmailVerificationCode == null || EmailVerificationCode == string.Empty)
                {
                    Mouse.OverrideCursor = null;
                    MessageBox.Show(" Eamil verification code required", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (PhoneVerificationCode == null || PhoneVerificationCode == string.Empty)
                {
                    Mouse.OverrideCursor = null;
                    MessageBox.Show("Sms verification code required", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                var validEmail = VerifyEmail().GetAwaiter().GetResult();
                var validPhone = VerifyPhone().GetAwaiter().GetResult();
                if (validEmail && validPhone)
                {
                    Mediator.Notify("LoginScreen", "");
                }

                Mouse.OverrideCursor = null;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnHome()
        {
            Mediator.Notify("LoginScreen", "");
        }
        private void OnCancel()
        {
            EmailVerificationCode = string.Empty;
            PhoneVerificationCode = string.Empty;
            Mouse.OverrideCursor = null;
        }

        public async Task<bool> ResendEmail()
        {
            try
            {
                var userName = Global.PropertyUserName;
                GlobalConfiguration.IsEmailConfigurationValid = true;

                var emailConfirmationToken_Url = ApplicationConstants.BaseUrl + ApiRoutePathConstants.GenerateEmailConfirmationToken;
                var emailConfirmationToken_response = Http.Client
                                                    .PostAsync(emailConfirmationToken_Url,
                     new StringContent(JsonConvert.SerializeObject(userName), Encoding.UTF8, "application/json")).Result;
                var emailConfirmationTokenResult = emailConfirmationToken_response.Content.ReadAsStringAsync().Result;
                DomainModel_FrameworkResult frameworkResult = JsonConvert.DeserializeObject<DomainModel_FrameworkResult>(emailConfirmationTokenResult);

                if (frameworkResult.Status == DomainModel.ResultStatus.Success)
                {
                    MessageBox.Show("Verification code sent to your registred email", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(frameworkResult.Errors.FirstOrDefault().Description, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return true;
        }
        public async Task<bool> ResendPhone()
        {
            try
            {
                var userName = Global.PropertyUserName;
                GlobalConfiguration.IsSmsConfigurationValid = true;

                var phoneConfirmationToken_Url = ApplicationConstants.BaseUrl + ApiRoutePathConstants.GeneratePhoneNumberConfirmationToken;
                var phoneConfirmationToken_response = Http.Client.PostAsync(phoneConfirmationToken_Url, new StringContent(JsonConvert.SerializeObject(userName), Encoding.UTF8, "application/json")).Result;
                var phoneConfirmationTokennResult = phoneConfirmationToken_response.Content.ReadAsStringAsync().Result;
                DomainModel_FrameworkResult frameworkPhoneResult = JsonConvert.DeserializeObject<DomainModel_FrameworkResult>(phoneConfirmationTokennResult);
                if (frameworkPhoneResult.Status == DomainModel.ResultStatus.Success)
                {
                    MessageBox.Show("SMS sent to your registered mobile number.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(frameworkPhoneResult.Errors.FirstOrDefault().Description, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return true;
        }

        public async Task OnLoadAsync()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                _ = ResendEmail().GetAwaiter().GetResult();
                _ = ResendPhone().GetAwaiter().GetResult();
                Mouse.OverrideCursor = null;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task<bool> VerifyEmail()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                var userName = Global.PropertyUserName;

                GlobalConfiguration.IsEmailConfigurationValid = true;
                var generateUserToken_data = new
                {
                    username = userName,
                    emailToken = EmailVerificationCode,
                };

                var verifyEmailurl = ApplicationConstants.BaseUrl + ApiRoutePathConstants.VerifyEmailConfirmationToken;
                var verifyEmailresponse = Http.Client.PostAsync(verifyEmailurl, new StringContent(JsonConvert.SerializeObject(generateUserToken_data), Encoding.UTF8, "application/json")).Result;
                var verifyEmailDetails = verifyEmailresponse.Content.ReadAsStringAsync().Result;
                var verifyEmailDetailsResult = JsonConvert.DeserializeObject<DomainModel_FrameworkResult>(verifyEmailDetails);

                if (verifyEmailDetailsResult.Status == DomainModel.ResultStatus.Success)
                {
                    MessageBox.Show("Email successfully verified .", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    EmailVerificationCode = string.Empty;
                }
                else
                {
                    Mouse.OverrideCursor = null;
                    EmailVerificationCode = string.Empty;
                    MessageBox.Show(verifyEmailDetailsResult.Errors.FirstOrDefault().Description, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
                Mouse.OverrideCursor = null;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return true;
        }

        public async Task<bool> VerifyPhone()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                var userName = Global.PropertyUserName;

                GlobalConfiguration.IsSmsConfigurationValid = true;
                var generateUserToken_data = new
                {
                    user_name = userName,
                    sms_token = PhoneVerificationCode,
                };

                var verifyphoneurl = ApplicationConstants.BaseUrl + ApiRoutePathConstants.VerifyPhoneNumberConfirmationToken;
                var verifyPhoneresponse = Http.Client.PostAsync(verifyphoneurl, new StringContent(JsonConvert.SerializeObject(generateUserToken_data), Encoding.UTF8, "application/json")).Result;
                var verifyPhoneDetails = verifyPhoneresponse.Content.ReadAsStringAsync().Result;
                var verifyPhoneDetailsResult = JsonConvert.DeserializeObject<DomainModel_FrameworkResult>(verifyPhoneDetails);

                if (verifyPhoneDetailsResult.Status == DomainModel.ResultStatus.Success)
                {
                    MessageBox.Show("SMS verified successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    PhoneVerificationCode = string.Empty;
                }
                else
                {
                    PhoneVerificationCode = string.Empty;
                    MessageBox.Show(verifyPhoneDetailsResult.Errors.FirstOrDefault().Description, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Mouse.OverrideCursor = null;
                    return false;
                }
                Mouse.OverrideCursor = null;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return true;
        }
        private void GotoLogin()
        {
            Mediator.Notify("LoginScreen", "");
        }
    }
}


