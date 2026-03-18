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
using UserModel = HCL.CS.SF.DemoClientWpfApp.DomainModel.UserModel;

namespace HCL.CS.SF.DemoClientWpfApp.ViewModel
{
    internal class TwofactorAuthenticatorappViewModel : BaseViewModel, IPageViewModel
    {
        ApplicationParameters ApplicationConstants = new ApplicationParameters();
        public RelayCommand HomeCommand { get; set; }
        public RelayCommand UserHomeCommand { get; set; }
        public RelayCommand VerifyauthenticatorCommand { get; set; }
        public RelayCommand GotoEmailCommand { get; set; }
        public RelayCommand GotoSmsCommand { get; set; }

        public TwofactorAuthenticatorappViewModel()
        {
            GotoEmailCommand = new RelayCommand(param => GotoTwoFactorEmailScreen());
            GotoSmsCommand = new RelayCommand(param => GotoTwoFactorSMSScreenScreen());
            VerifyauthenticatorCommand =new RelayCommand(param => VerifyTwoFactor());
        }
        private string authenticatorVerificationCode;
        public string AuthenticatorVerificationCode
        {
            get
            {
                return authenticatorVerificationCode;
            }
            set
            {
                authenticatorVerificationCode = value;
                OnPropertyChanged("AuthenticatorVerificationCode");
            }
        }
        private void GotoTwoFactorEmailScreen()
        {
            Mediator.Notify("TwoFactorEmailScreen", "");
        }
        private void GotoTwoFactorSMSScreenScreen()
        {
            Mediator.Notify("TwoFactorSMSScreen", "");
        }
        public async Task VerifyTwoFactor()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                var userName = Global.PropertyUserName;

                GlobalConfiguration.IsSmsConfigurationValid = true;
                var generateUserToken_data = new
                {
                    user_name = userName,
                    sms_token = authenticatorVerificationCode,
                };

                var verifyauthenticatorurl = ApplicationConstants.BaseUrl + ApiRoutePathConstants.TwoFactorAuthenticatorAppSignIn;
                var verifyauthenticatorresponse = Http.Client.PostAsync(verifyauthenticatorurl, new StringContent(JsonConvert.SerializeObject(authenticatorVerificationCode), Encoding.UTF8, "application/json")).Result;
                var verifyauthenticatorDetails = verifyauthenticatorresponse.Content.ReadAsStringAsync().Result;
                var verifyauthenticatorDetailsResult = JsonConvert.DeserializeObject<LogoutMessageModel.SignInResponseModel>(verifyauthenticatorDetails);

                if (verifyauthenticatorDetailsResult.Succeeded)
                {
                    AuthenticatorVerificationCode = string.Empty;
                    MessageBox.Show("Authenticator code verified successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    var tokenResponse =  GetTokenResponse().Result;
                    Global.AccessToken = tokenResponse.access_token;
                    Global.IdToken = tokenResponse.id_token;
                    Mediator.Notify("DashBoardScreen", "");
                }
                else
                {
                    MessageBox.Show(verifyauthenticatorDetailsResult.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
           catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            Mouse.OverrideCursor = null;
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


