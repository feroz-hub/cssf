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
using System.IdentityModel.Tokens.Jwt;
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
using static HCL.CS.SF.DemoClientWpfApp.DomainModel.LogoutMessageModel;

namespace HCL.CS.SF.DemoClientWpfApp.ViewModel
{
    internal class MainWindowViewModel : BaseViewModel
    {
        ApplicationParameters ApplicationConstants = new ApplicationParameters();
        public RelayCommand HomeCommand { get; set; }
        public RelayCommand UserManageCommend { get; set; }
        public RelayCommand APICallCommend { get; set; }
        public RelayCommand EmailCommend { get; set; }
        public RelayCommand EndPointCommend { get; set; }
        public RelayCommand GotoRegisterCommand { get; set; }

        private string stkDashboarddetails;
        public string StkDashboarddetails
        {
            get
            {
                return stkDashboarddetails;
            }
            set
            {
                if (stkDashboarddetails != value)
                {
                    stkDashboarddetails = value;
                    OnPropertyChanged("StkDashboarddetails");
                }
            }
        }

        public string DashboardUserRole;

        private IPageViewModel _currentPageViewModel;
        private List<IPageViewModel> _pageViewModels;

        public List<IPageViewModel> PageViewModels
        {
            get
            {
                if (_pageViewModels == null)
                    _pageViewModels = new List<IPageViewModel>();

                return _pageViewModels;
            }
        }

        public IPageViewModel CurrentPageViewModel
        {
            get
            {
                return _currentPageViewModel;
            }
            set
            {
                _currentPageViewModel = value;
                OnPropertyChanged("CurrentPageViewModel");
            }
        }
        private string aPICallVisibility;
        public string APICallVisibility
        {
            get
            {
                return aPICallVisibility;
            }
            set
            {
                aPICallVisibility = value;
                OnPropertyChanged("APICallVisibility");
            }
        }

        private string userManagebuttonVisibility;
        public string UserManagebuttonVisibility
        {
            get
            {
                return userManagebuttonVisibility;
            }
            set
            {
                userManagebuttonVisibility = value;
                OnPropertyChanged("UserManagebuttonVisibility");
            }
        }

        private string endPointVisibility;
        public string EndPointVisibility
        {
            get
            {
                return endPointVisibility;
            }
            set
            {
                endPointVisibility = value;
                OnPropertyChanged("EndPointVisibility");
            }
        }
        // Login Contorles


        private string loginUsername;
        public string LoginUserName
        {
            get
            {
                return loginUsername;
            }
            set
            {
                loginUsername = value;
                OnPropertyChanged("LoginUserName");
            }
        }
        public RelayCommand LoginCommand { get; set; }
        public RelayCommand GotoEmailVerifyCommand { get; set; }
        public RelayCommand GotoForgotPassworCommand { get; set; }
        public RelayCommand GotoLogoutCommand { get; set; }

        public MainWindowViewModel()
        {
            HomeCommand = new RelayCommand(param => Home());
            UserManageCommend = new RelayCommand(param => UserProfile());
            APICallCommend = new RelayCommand(param => APICall());
            EndPointCommend = new RelayCommand(param => EndpointCall());
            GotoRegisterCommand= new RelayCommand(param => Register());

            EmailCommend = new RelayCommand(param => Email());
            LoginCommand = new RelayCommand(param => Login(param));
            GotoEmailVerifyCommand = new RelayCommand(param => GotoEmailVerifycation());
            GotoForgotPassworCommand = new RelayCommand(param => ResetPasswordTab());
            GotoLogoutCommand = new RelayCommand(param => Logout());
            APICallVisibility = "Collapsed";
            StkDashboarddetails = "Collapsed";
            UserManagebuttonVisibility= "Collapsed";
            EndPointVisibility = "Collapsed";
            // Add available pages and set page
            PageViewModels.Add(new LoginViewModel());
            PageViewModels.Add(new RegisterUserViewModel());
            PageViewModels.Add(new EmailVericationViewModel());
            PageViewModels.Add(new ManageAccountViewModel());
            PageViewModels.Add(new EndPointControllerViewModel());
            PageViewModels.Add(new TwoFactorSMSViewModel());
            PageViewModels.Add(new DashBoardViewModel());
            PageViewModels.Add(new UpdateProfileViewModel());
            PageViewModels.Add(new ChangePasswordViewModel());
            PageViewModels.Add(new TwoFactorEmailViewModel());
            PageViewModels.Add(new TwofactorAuthenticatorappViewModel());
            PageViewModels.Add(new ApiResourceGridViewModel());
            PageViewModels.Add(new APIDashBoardViewModel());
            PageViewModels.Add(new IdentityResourceGridViewModel());
            PageViewModels.Add(new RoleViewModel());
            PageViewModels.Add(new ClientViewModel());
            PageViewModels.Add(new ResetPasswordViewModel());
            PageViewModels.Add(new UserRoleViewModel());

            CurrentPageViewModel = PageViewModels[0];

            Mediator.Subscribe("LoginScreen", LoginScreen);
            Mediator.Subscribe("RegisterScreen", RegisterScreen);
            Mediator.Subscribe("EmailVerificationScreen", EmailVerificationScreen);
            Mediator.Subscribe("ManageAccountScreen", ManageAccountScreen);
            Mediator.Subscribe("EndPointControllerScreen", EndPointControllerScreen);
            Mediator.Subscribe("TwoFactorSMSScreen", TwoFactorSMSScreen);
            Mediator.Subscribe("DashBoardScreen", DashBoardScreen);
            Mediator.Subscribe("UpdateProfileScreen", UpdateProfileScreen);
            Mediator.Subscribe("ChangePasswordScreen", ChangePasswordScreen);
            Mediator.Subscribe("TwoFactorEmailScreen", TwoFactorEmailScreen);
            Mediator.Subscribe("TwofactorAuthenticatorappScreen", TwofactorAuthenticatorappScreen);
            Mediator.Subscribe("ApiResourceGridScreen", ApiResourceGridScreen);
            Mediator.Subscribe("APIDashBoardScreen", APIDashBoardScreen);
            Mediator.Subscribe("IdentityResourceScreen", IdentityResourceScreen);
            Mediator.Subscribe("RoleScreen", RoleScreen);
            Mediator.Subscribe("ClientScreen", ClientScreen);
            Mediator.Subscribe("ResetPasswordScreen", ResetPasswordScreen);
            Mediator.Subscribe("UserRoleScreen", UserRoleScreen);
        }

        public IPageViewModel SetScreenId(ScreenIndex index)
        {
            return PageViewModels[(int)index];
        }

        public void ResetPasswordTab()
        {


            if (string.IsNullOrWhiteSpace(LoginUserName))
            {
                MessageBox.Show("User name is empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Mouse.OverrideCursor = null;
                return;
            }
            else
            {
                Global.PropertyUserName = LoginUserName;
                Mediator.Notify("ResetPasswordScreen", "");
            }

        }
        private async Task<LogoutMessageModel.SignInResponseModel> GetSigninResponse(string password)
        {
            LogoutMessageModel.SignInResponseModel model = new LogoutMessageModel.SignInResponseModel();
            Global.PropertyUserName = LoginUserName;
            var data = new
            {
                user_name = LoginUserName,
                password = password,
            };
            var loginUrl = ApplicationConstants.BaseUrl + ApiRoutePathConstants.PasswordSignIn;
            var response = Http.Client.PostAsync(
                loginUrl,
                new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json")).Result;
        var content = response.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<LogoutMessageModel.SignInResponseModel>(content);
        }

        private async Task<FrameworkResult> GenerateTwofactorSMS()
        {
            var twofactorVerificationUrl = ApplicationConstants.BaseUrl + ApiRoutePathConstants.GenerateSmsTwoFactorToken;
            var response = Http.Client.PostAsync(twofactorVerificationUrl, new StringContent(JsonConvert.SerializeObject(LoginUserName), Encoding.UTF8, "application/json")).Result;
            var result = response.Content.ReadAsStringAsync().Result;
            return  JsonConvert.DeserializeObject<FrameworkResult>(result);
        }
        private async Task<FrameworkResult> GenerateTwofactorEmail()
        {
            var twofactorEmailUrl = ApplicationConstants.BaseUrl + ApiRoutePathConstants.GenerateEmailTwoFactorToken;
            var response = Http.Client.PostAsync(twofactorEmailUrl, new StringContent(JsonConvert.SerializeObject(LoginUserName), Encoding.UTF8, "application/json")).Result;
            var result = response.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<FrameworkResult>(result);

        }

        private void Login(object parameter)        
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                if (string.IsNullOrWhiteSpace(LoginUserName))
                {
                    MessageBox.Show("User name is empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Mouse.OverrideCursor = null;
                    return;
                }
                var passwordBox = (PasswordBox)parameter;
                if (string.IsNullOrWhiteSpace(passwordBox.Password))
                {
                    Mouse.OverrideCursor = null;
                    MessageBox.Show("Password is empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                var model = GetSigninResponse(passwordBox.Password).Result;
             
                Global.PropertyUserName = LoginUserName;
                Global.PropertyPassword = passwordBox.Password;
                if (!model.RequiresTwoFactor && model.Succeeded)
                {
                    var tokenResponse =  GetTokenResponse().Result;
                    Global.AccessToken = tokenResponse.access_token;
                    Global.IdToken = tokenResponse.id_token;
                    if (tokenResponse.access_token!=null)
                    {
                        Mediator.Notify("DashBoardScreen", "");
                    }
                   
                }
                else if (model.RequiresTwoFactor && !model.Succeeded)
                {
                    if (model.TwoFactorVerificationMode == TwoFactorType.Sms)
                    {
                        var result = GenerateTwofactorSMS().Result;
                        if (result.Status == ResultStatus.Success)
                        {
                            Mediator.Notify("TwoFactorSMSScreen", "");
                        }
                        else
                        {
                            Mouse.OverrideCursor = null;
                            MessageBox.Show(result.Errors.FirstOrDefault().Description, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                    else
                    if (model.TwoFactorVerificationMode == TwoFactorType.Email)
                    {
                        var result = GenerateTwofactorEmail().Result;
                        if (result.Status == ResultStatus.Success)
                        {
                            Mediator.Notify("TwoFactorEmailScreen", "");
                        }
                        else
                        {
                            Mouse.OverrideCursor = null;
                            MessageBox.Show(result.Errors.FirstOrDefault().Description, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else if (model.TwoFactorVerificationMode == TwoFactorType.AuthenticatorApp)
                    {
                        Mediator.Notify("TwofactorAuthenticatorappScreen", "");
                    }

                }
                else if (model.Message.Contains("Please verify Email"))
                {
                    Mediator.Notify("EmailVerificationScreen", "");
                }
                else
                {
                    Mouse.OverrideCursor = null;
                    MessageBox.Show(model.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            Mouse.OverrideCursor = null;
        }
        public void GotoEmailVerifycation()
        {
            try
            {
                if (LoginUserName != null && LoginUserName != string.Empty)
                {
                    Global.PropertyUserName = LoginUserName; //EmailVerificationScreen
                    Mediator.Notify("EmailVerificationScreen", "");
                }
                else
                {
                    MessageBox.Show("User name should not be empty", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task <TokenResponseResultModel> GetTokenResponse()
        {
            TokenResponseResultModel tokenResponseResultModel = new TokenResponseResultModel();
            try
            {
                HttpService httpService = new HttpService();
                if (Global.AccessToken == null || Global.AccessToken == string.Empty)
                {
                    tokenResponseResultModel = httpService.AuthCodeFlow().Result;
                    if (tokenResponseResultModel.access_token!=null)
                    {
                        return tokenResponseResultModel;
                    }
                    else
                    {
                        MessageBox.Show(tokenResponseResultModel.ErrorResponseResult.error_description, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return tokenResponseResultModel;
        }


        public void Home()
        {
            APICallVisibility = "Collapsed";
            StkDashboarddetails = "Collapsed";
            UserManagebuttonVisibility = "Collapsed";
            EndPointVisibility= "Collapsed";
            _ = Logout();
            CurrentPageViewModel = SetScreenId(ScreenIndex.Home);
        }
        public void Register()
        {
            APICallVisibility = "Collapsed";
            StkDashboarddetails = "Collapsed";
            Mediator.Notify("RegisterScreen", "");
        }
        public void UserProfile()
        {
            CurrentPageViewModel = SetScreenId(ScreenIndex.DashBoard);
        }
        public void APICall()
        {
            CurrentPageViewModel = SetScreenId(ScreenIndex.APIDashBoard);
        }
        public void EndpointCall()
        {
            CurrentPageViewModel = SetScreenId(ScreenIndex.EndPointController);
        }
        public void Email()
        {
            CurrentPageViewModel = SetScreenId(ScreenIndex.EmailVerification);
        }
        private void ChangeViewModel(IPageViewModel viewModel)
        {
            if (!PageViewModels.Contains(viewModel))
            {
                PageViewModels.Add(viewModel);
            }

            CurrentPageViewModel = PageViewModels.FirstOrDefault(vm => vm == viewModel);
        }


        private void LoginScreen(object obj)
        {
            //string message = "Are you sure want to logout and go to login page?";
            //string caption = "Confirmation";
            //MessageBoxButton buttons = MessageBoxButton.YesNo;
            //MessageBoxImage icon = MessageBoxImage.Warning;
            //if (Global.AccessToken != null)
            //{

            //    if (MessageBox.Show(message, caption, buttons, icon) == MessageBoxResult.Yes)
            //    {
            //        APICallVisibility = "Collapsed";
            //        StkDashboarddetails = "Collapsed";
            //        EndPointVisibility = "Collapsed";
            //        UserManagebuttonVisibility = "Collapsed";
            //        ChangeViewModel(SetScreenId(ScreenIndex.Home));
            //    }
            //}
            //else
            //{
            //
                APICallVisibility = "Collapsed";
                StkDashboarddetails = "Collapsed";
                EndPointVisibility = "Collapsed";
                UserManagebuttonVisibility = "Collapsed";
                ChangeViewModel(SetScreenId(ScreenIndex.Home));
            //}
        }

        private void RegisterScreen(object obj)
        {
            ChangeViewModel(SetScreenId(ScreenIndex.Register));
        }

        private void EmailVerificationScreen(object obj)
        {
            ChangeViewModel(SetScreenId(ScreenIndex.EmailVerification));
        }
        private void ManageAccountScreen(object obj)
        {
            ChangeViewModel(SetScreenId(ScreenIndex.ManageAccount));
        }

        private void EndPointControllerScreen(object obj)
        {
            ChangeViewModel(SetScreenId(ScreenIndex.EndPointController));
        }
        private void TwoFactorSMSScreen(object obj)
        {
            ChangeViewModel(SetScreenId(ScreenIndex.TwoFactorSMS));
        }
        private void DashBoardScreen(object obj)
        {
            StkDashboarddetails = "Visible";
            UserManagebuttonVisibility = "Visible";

            if (Global.AccessToken != null)
            {
                var jwt = new JwtSecurityToken(Global.AccessToken);
                DashboardUserRole = string.Empty;

                var roles = jwt.Claims.Where(a => a.Type.Contains("role")).ToArray();

                foreach (var item in roles)
                {
                    DashboardUserRole += item.Value.ToString() + " ";
                }

                RoleName = DashboardUserRole;
                if (DashboardUserRole.ToLower().Contains("admin"))
                {
                    APICallVisibility = "Visible";
                    EndPointVisibility = "Visible";
                    //InputDisplayFields();
                }
                else
                {
                    //InputDisplayNonFields();
                }
            }
            if (Global.IdToken!= null)
            {
                var jwt = new JwtSecurityToken(Global.IdToken);
                DashboardUserRole = string.Empty;
                var name = jwt.Claims.Where(a => a.Type.Contains("firstname")).ToArray();
                LoginUserName = name.FirstOrDefault().Value.ToString();
            }

            ChangeViewModel(SetScreenId(ScreenIndex.DashBoard));
        }
        private void UpdateProfileScreen(object obj)
        {
            ChangeViewModel(SetScreenId(ScreenIndex.UpdateProfile));
        }
        private void ChangePasswordScreen(object obj)
        {
            ChangeViewModel(SetScreenId(ScreenIndex.ChangePassword));
        }

        private void TwoFactorEmailScreen(object obj)
        {
            ChangeViewModel(SetScreenId(ScreenIndex.TwoFactorEmail));
        }
        private void TwofactorAuthenticatorappScreen(object obj)
        {
            ChangeViewModel(SetScreenId(ScreenIndex.TwofactorAuthenticatorapp));
        }

        private void ApiResourceGridScreen(object obj)
        {
            ChangeViewModel(SetScreenId(ScreenIndex.ApiResourceGrid));
        }
        private void APIDashBoardScreen(object obj)
        {
            ChangeViewModel(SetScreenId(ScreenIndex.APIDashBoard));
        }
        private void IdentityResourceScreen(object obj)
        {
            ChangeViewModel(SetScreenId(ScreenIndex.IdentityResource));
        }
        private void RoleScreen(object obj)
        {
            ChangeViewModel(SetScreenId(ScreenIndex.Role));
        }
        private void ClientScreen(object obj)
        {
            ChangeViewModel(SetScreenId(ScreenIndex.Client));
        }
        private void ResetPasswordScreen(object obj)
        {
            ChangeViewModel(SetScreenId(ScreenIndex.ResetPassword));
        }
        private void UserRoleScreen(object obj)
        {
            ChangeViewModel(SetScreenId(ScreenIndex.UserRole));
        }

        public async Task Logout()
        {
            try
            {
                Http.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Global.AccessToken);
                var signout_url = ApplicationConstants.BaseUrl + ApiRoutePathConstants.SignOut;
                var signout_response = Http.Client.PostAsync(signout_url, new StringContent(JsonConvert.SerializeObject(string.Empty), Encoding.UTF8, "application/json")).Result;
                var signoutresult = signout_response.Content.ReadAsStringAsync().Result;
                var result = JsonConvert.DeserializeObject<FrameworkResult>(signoutresult);
                ClearGloblInputs();
                if (result.Status == ResultStatus.Success)
                {
                    Global.AccessToken = string.Empty;
                }
                Mediator.Notify("LoginScreen", "");
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string roleName;
        public string RoleName
        {
            get
            {
                return roleName;
            }
            set
            {
                if (roleName != value)
                {
                    roleName = value;
                }
                OnPropertyChanged("RoleName");
            }
        }
     public void ClearGloblInputs()
        {
            Global.PropertyUserName = null;
            Global.PropertyPassword = null;
            Global.PropertyTabName = null;
            Global.AccessToken = null;
            Global.IdToken = null;
            Global.ApiScopeGetData = null;
            Global.ApiResourcesGetData = null;
            Global.ApiResourceClaimsGetData = null;
            Global.ApiScopeClaimsGetData = null;
            Global.IdentityResourcesGetdata = null;
            Global.IdentityResourcesClaimGetdata = null;
            Global.ClientsGetdata = null;
            Global.UserRoleGetdata = null;
            Global.DisplayModelGetdata = null;
            Global.RolesGetdata = null;
            Global.RoleClaimModelGetData = null;
            Global.UserClaimModelGetData = null;
            Global.PopupMethodName = null;
        }
        public enum ScreenIndex
        {
            Home = 0,
            Register = 1,
            EmailVerification = 2,
            ManageAccount = 3,
            EndPointController = 4,
            TwoFactorSMS = 5,
            DashBoard = 6,
            UpdateProfile = 7,
            ChangePassword = 8,
            TwoFactorEmail = 9,
            TwofactorAuthenticatorapp = 10,
            ApiResourceGrid = 11,
            APIDashBoard = 12,
            IdentityResource = 13,
            Role = 14,
            Client = 15,
            ResetPassword = 16,
            UserRole= 17
        }

    }
}


