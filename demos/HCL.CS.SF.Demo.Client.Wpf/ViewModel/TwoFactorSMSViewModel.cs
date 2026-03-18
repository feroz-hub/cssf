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
using HCL.CS.SF.DemoClientWpfApp.DomainModel;
using HCL.CS.SF.DemoClientWpfApp.Interface;
using HCL.CS.SF.DemoClientWpfApp.Services;
using static HCL.CS.SF.DemoClientWpfApp.DomainModel.LogoutMessageModel;
using DomainModel_FrameworkResult = HCL.CS.SF.DemoClientWpfApp.DomainModel.FrameworkResult;
using FrameworkResult = HCL.CS.SF.DemoClientWpfApp.DomainModel.FrameworkResult;

namespace HCL.CS.SF.DemoClientWpfApp.ViewModel
{
    internal class TwoFactorSMSViewModel : BaseViewModel, IPageViewModel
    {
        ApplicationParameters ApplicationConstants = new ApplicationParameters();
        private string phoneVerificationCode;
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
        public RelayCommand VerifyPhoneCommand { get; set; }
        public RelayCommand ResendPhoneCommand { get; set; }
        public RelayCommand GoTwoFactorAuthenticatorCommand { get; set; }
        public RelayCommand GoTwoFactorEmailCommand { get; set; }

        public TwoFactorSMSViewModel()
        {
            phoneVerificationCode = string.Empty;
            VerifyPhoneCommand = new RelayCommand(param => VerifyPhone());
            ResendPhoneCommand = new RelayCommand(param => ResendPhone());
            GoTwoFactorAuthenticatorCommand = new RelayCommand(param => GotoTwoFactorAuthenticator());
            GoTwoFactorEmailCommand = new RelayCommand(param => GotoTwoFactorEmail());
        }
        private void GotoTwoFactorEmail()
        {
            Mediator.Notify("TwoFactorEmailScreen", "");
        }
        private void GotoTwoFactorAuthenticator()
        {
            Mediator.Notify("TwofactorAuthenticatorappScreen", "");
        }

        public async Task ResendPhone()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                var userName = Global.PropertyUserName;
                //Phone
                GlobalConfiguration.IsSmsConfigurationValid = true;

                var generateAndSendPhoneConfirmationToken_Url = ApplicationConstants.BaseUrl + ApiRoutePathConstants.GeneratePhoneNumberConfirmationToken;
                var generateAndSendPhoneConfirmationToken_response = Http.Client.PostAsync(generateAndSendPhoneConfirmationToken_Url, new StringContent(JsonConvert.SerializeObject(userName), Encoding.UTF8, "application/json")).Result;
                var generateAndSendPhoneConfirmationTokennResult = generateAndSendPhoneConfirmationToken_response.Content.ReadAsStringAsync().Result;
                DomainModel_FrameworkResult frameworkPhoneResult = JsonConvert.DeserializeObject<DomainModel_FrameworkResult>(generateAndSendPhoneConfirmationTokennResult);
                if (frameworkPhoneResult.Status == DomainModel.ResultStatus.Success)
                {
                    MessageBox.Show("SMS sent to your registered phone number.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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
            Mouse.OverrideCursor = null;
        }
        public async Task VerifyPhone()
        {
            try
            {
                var userName = Global.PropertyUserName;

                GlobalConfiguration.IsSmsConfigurationValid = true;

                var verifyphoneurl = ApplicationConstants.BaseUrl + ApiRoutePathConstants.TwoFactorSmsSignInAsync;
                var verifyPhoneresponse = Http.Client
                                                    .PostAsync(
                                                                        verifyphoneurl,
                                                                        new StringContent(JsonConvert.SerializeObject(PhoneVerificationCode), Encoding.UTF8, "application/json")).Result;
                var verifyPhoneDetails = verifyPhoneresponse.Content.ReadAsStringAsync().Result;
                var verifyPhoneDetailsResult = JsonConvert.DeserializeObject<LogoutMessageModel.SignInResponseModel>(verifyPhoneDetails);

                if (verifyPhoneDetailsResult.Succeeded)
                {
                    PhoneVerificationCode = string.Empty;
                    MessageBox.Show("SMS verified successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    var tokenResponse = GetTokenResponse().Result;
                    Global.AccessToken = tokenResponse.access_token;
                    Global.IdToken = tokenResponse.id_token;
                    Mediator.Notify("DashBoardScreen", "");
                }
                else
                {
                    MessageBox.Show(verifyPhoneDetailsResult.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public async Task<TokenResponseResultModel> GetTokenResponse()
        {
            TokenResponseResultModel tokenResponseResultModel = new TokenResponseResultModel();
            try
            {
                HttpService httpService = new HttpService();
                if (Global.AccessToken == null || Global.AccessToken == string.Empty)
                {
                    tokenResponseResultModel = httpService.AuthCodeFlow().Result;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return tokenResponseResultModel;
        }
    }
}


