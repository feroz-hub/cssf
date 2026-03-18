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
using ResultStatus = HCL.CS.SF.DemoClientWpfApp.DomainModel.ResultStatus;

namespace HCL.CS.SF.DemoClientWpfApp.ViewModel
{
    internal class TwoFactorEmailViewModel : BaseViewModel, IPageViewModel
    {
        ApplicationParameters ApplicationConstants = new ApplicationParameters();
        private string emailVerificationCode;
        public string EmailVerificationCode
        {
            get
            {
                return emailVerificationCode;
            }
            set
            {
                emailVerificationCode = value;
                OnPropertyChanged("EmailVerificationCode");
            }
        }
        public RelayCommand VerifyEmailCommand { get; set; }
        public RelayCommand ResendEmailCommand { get; set; }

        public RelayCommand GotoAuthenicatorCommand { get; set; }
        public RelayCommand GotoSmsCommand { get; set; }
        public TwoFactorEmailViewModel()
        {
            emailVerificationCode = string.Empty;
            VerifyEmailCommand = new RelayCommand(param => VerifyEmail());
            GotoAuthenicatorCommand = new RelayCommand(param => GotoAuthenticatorScreen());
            GotoSmsCommand = new RelayCommand(param => GotoTwoFactorSms());
        }
        private async Task GotoTwoFactorSms()
        {
            Mediator.Notify("TwoFactorSMSScreen", "");
        }
        private async Task GotoAuthenticatorScreen()
        {
            Mediator.Notify("TwofactorAuthenticatorappScreen", "");
        }

        public async Task ResendEmail()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                var userName = Global.PropertyUserName;
                //Email
                GlobalConfiguration.IsSmsConfigurationValid = true;

                var generateAndSendEmailConfirmationToken_Url = ApplicationConstants.BaseUrl + ApiRoutePathConstants.GenerateEmailTwoFactorToken;
                var generateAndSendEmailConfirmationToken_response = Http.Client
                                                    .PostAsync(
                                                                        generateAndSendEmailConfirmationToken_Url,
                                                                        new StringContent(JsonConvert.SerializeObject(userName), Encoding.UTF8, "application/json")).Result;
                var generateAndSendEmailConfirmationTokennResult = generateAndSendEmailConfirmationToken_response.Content.ReadAsStringAsync().Result;
                DomainModel_FrameworkResult frameworkEmailResult = JsonConvert.DeserializeObject<DomainModel_FrameworkResult>(generateAndSendEmailConfirmationTokennResult);
                if (frameworkEmailResult.Status == ResultStatus.Success)
                {
                    MessageBox.Show("Email sent to your registered email.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(frameworkEmailResult.Errors.FirstOrDefault().Description, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            Mouse.OverrideCursor = null;
        }
        public async Task VerifyEmail()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                var userName = Global.PropertyUserName;
                var verifyemailurl = ApplicationConstants.BaseUrl + ApiRoutePathConstants.TwoFactorEmailSignIn;
                var verifyEmailresponse = Http.Client.PostAsync(verifyemailurl, new StringContent(JsonConvert.SerializeObject(EmailVerificationCode), Encoding.UTF8, "application/json")).Result;
                var verifyEmailDetails = verifyEmailresponse.Content.ReadAsStringAsync().Result;
                var verifyEmailDetailsResult = JsonConvert.DeserializeObject<LogoutMessageModel.SignInResponseModel>(verifyEmailDetails);

                if (verifyEmailDetailsResult.Succeeded)
                {
                    EmailVerificationCode = string.Empty;
                    MessageBox.Show("Email verified successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    var tokenResponse = GetTokenResponse().Result;
                    Global.AccessToken = tokenResponse.access_token;
                    Global.IdToken = tokenResponse.id_token;
                    Mediator.Notify("DashBoardScreen", "");
                }
                else
                {
                    MessageBox.Show(verifyEmailDetailsResult.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Information);
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

