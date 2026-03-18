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
using System.Windows.Input;
using HCL.CS.SF.DemoClientWpfApp.Components;
using HCL.CS.SF.DemoClientWpfApp.Constants;
using HCL.CS.SF.DemoClientWpfApp.DomainModel;
using HCL.CS.SF.DemoClientWpfApp.Interface;
using HCL.CS.SF.DemoClientWpfApp.Services;
using DomainModel_FrameworkResult = HCL.CS.SF.DemoClientWpfApp.DomainModel.FrameworkResult;
using FrameworkResult = HCL.CS.SF.DemoClientWpfApp.DomainModel.FrameworkResult;
using ResultStatus = HCL.CS.SF.DemoClientWpfApp.DomainModel.ResultStatus;

namespace HCL.CS.SF.DemoClientWpfApp.ViewModel
{
    internal class UpdateProfileViewModel : BaseViewModel, IPageViewModel
    {
        ApplicationParameters ApplicationConstants = new ApplicationParameters();
        // Profile
        private string userName;
        private string phonenumber;
        private string email;
        private string firstName;
        private string lastName;

        // Profile
        public RelayCommand UpdateProfileCommand { get; set; }
        public RelayCommand DeleteProfileCommand { get; set; }
        public RelayCommand CancelProfileCommand { get; set; }
        public RelayCommand GotoLoginCommand { get; set; }

        public string UserName
        {
            get
            {
                return userName;
            }
            set
            {
                userName = value;
                OnPropertyChanged("UserName");
            }
        }

        public string PhoneNumber
        {
            get
            {
                return phonenumber;
            }
            set
            {
                phonenumber = value;
                OnPropertyChanged("PhoneNumber");
            }
        }

        public string Email
        {
            get
            {
                return email;
            }
            set
            {
                email = value;
                OnPropertyChanged("Email");
            }
        }

        public string FirstName
        {
            get
            {
                return firstName;
            }
            set
            {
                firstName = value;
                OnPropertyChanged("FirstName");
            }
        }
        public string LastName
        {
            get
            {
                return lastName;
            }
            set
            {
                lastName = value;
                OnPropertyChanged("LastName");
            }
        }
        public async Task OnLoadUserProfile()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                string username = Global.PropertyUserName;
                Http.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Global.AccessToken);
                var getUserByUserName_url = ApplicationConstants.BaseUrl + ApiRoutePathConstants.GetUserByName;
                var getUserByUserName_response = Http.Client.PostAsync(getUserByUserName_url, new StringContent(JsonConvert.SerializeObject(username), Encoding.UTF8, "application/json")).Result;
                var userDetails = getUserByUserName_response.Content.ReadAsStringAsync().Result;
                var getUserDetailsByUserNameResult = JsonConvert.DeserializeObject<UserModel>(userDetails);
                if (getUserDetailsByUserNameResult != null)
                {
                    UserName = getUserDetailsByUserNameResult.UserName;
                    FirstName = getUserDetailsByUserNameResult.FirstName;
                    LastName = getUserDetailsByUserNameResult.LastName;
                    Email = getUserDetailsByUserNameResult.Email;
                    PhoneNumber = getUserDetailsByUserNameResult.PhoneNumber;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            Mouse.OverrideCursor = null;
        }
        private async Task UpdaterProfile()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                Http.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Global.AccessToken);
                // Get User by Name
                string username = Global.PropertyUserName;
                var getUserByUserName_url = ApplicationConstants.BaseUrl + ApiRoutePathConstants.GetUserByName;
                var getUserByUserName_response = Http.Client.PostAsync(getUserByUserName_url, new StringContent(JsonConvert.SerializeObject(username), Encoding.UTF8, "application/json")).Result;
                var userDetails = getUserByUserName_response.Content.ReadAsStringAsync().Result;
                UserModel getUserDetailsByUserNameResult = JsonConvert.DeserializeObject<UserModel>(userDetails);

                if (getUserDetailsByUserNameResult != null)
                {
                    if (getUserDetailsByUserNameResult.FirstName == FirstName &&
                       getUserDetailsByUserNameResult.Email == Email &&
                       getUserDetailsByUserNameResult.PhoneNumber == PhoneNumber &&
                       getUserDetailsByUserNameResult.LastName == LastName
                       )
                    {
                        Mouse.OverrideCursor = null;
                        MessageBox.Show("No changes are made.", "Success", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    getUserDetailsByUserNameResult.FirstName = FirstName;
                    getUserDetailsByUserNameResult.Email = Email;
                    getUserDetailsByUserNameResult.PhoneNumber = PhoneNumber;
                    getUserDetailsByUserNameResult.LastName = LastName;

                    Http.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Global.AccessToken);
                    var updateUser_Url = ApplicationConstants.BaseUrl + ApiRoutePathConstants.UpdateUser;
                    var updateUser_response = Http.Client.PostAsync(updateUser_Url, new StringContent(JsonConvert.SerializeObject(getUserDetailsByUserNameResult), Encoding.UTF8, "application/json")).Result;
                    var updated_userDetails = updateUser_response.Content.ReadAsStringAsync().Result;
                    DomainModel_FrameworkResult updateUser_result = JsonConvert.DeserializeObject<DomainModel_FrameworkResult>(updated_userDetails);

                    if (updateUser_result.Status == ResultStatus.Success)
                    {

                        MessageBox.Show("User profile updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        Mediator.Notify("DashBoardScreen", "");
                    }
                    else
                    {
                        Mouse.OverrideCursor = null;
                        MessageBox.Show(updateUser_result.Errors.FirstOrDefault().Description.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    Mouse.OverrideCursor = null;
                    MessageBox.Show("Invalid user", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            Mouse.OverrideCursor = null;
        }

        public RelayCommand OnLoadCommand { get; set; }
        public UpdateProfileViewModel()
        {
            OnLoadCommand = new RelayCommand(param => OnLoadUserProfile());
            UpdateProfileCommand = new RelayCommand(param => UpdaterProfile());
            CancelProfileCommand = new RelayCommand(param => CancelProfile());
        }
        private async Task CancelProfile()
        {
            string username = Global.PropertyUserName;
            Http.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Global.AccessToken);
            var getUserByUserName_url = ApplicationConstants.BaseUrl + ApiRoutePathConstants.GetUserByName;
            var getUserByUserName_response = Http.Client.PostAsync(getUserByUserName_url, new StringContent(JsonConvert.SerializeObject(username), Encoding.UTF8, "application/json")).Result;
            var userDetails = getUserByUserName_response.Content.ReadAsStringAsync().Result;
            UserModel getUserDetailsByUserNameResult = JsonConvert.DeserializeObject<UserModel>(userDetails);
            if (getUserDetailsByUserNameResult != null)
            {
                UserName = getUserDetailsByUserNameResult.UserName;
                FirstName = getUserDetailsByUserNameResult.FirstName;
                LastName = getUserDetailsByUserNameResult.LastName;
                Email = getUserDetailsByUserNameResult.Email;
                PhoneNumber = getUserDetailsByUserNameResult.PhoneNumber;
            }
        }
    }
}


