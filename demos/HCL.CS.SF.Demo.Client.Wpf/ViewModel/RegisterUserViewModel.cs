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
using IdentityProvider = HCL.CS.SF.DemoClientWpfApp.DomainModel.IdentityProvider;
using TwoFactorType = HCL.CS.SF.DemoClientWpfApp.DomainModel.TwoFactorType;

namespace HCL.CS.SF.DemoClientWpfApp.ViewModel
{
    internal class RegisterUserViewModel : BaseViewModel, IPageViewModel
    {
        ApplicationParameters ApplicationConstants = new ApplicationParameters();
        // Input Contoles
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

        public string SecrityQuestionAnswer
        {
            get
            {
                return secrityQuestionAnswer;
            }
            set
            {
                secrityQuestionAnswer = value;
                OnPropertyChanged("SecrityQuestionAnswer");
            }
        }

        public string SelectedSecurityQuestion
        {
            get
            {
                return selectedSecurityQuestion;
            }
            set
            {
                selectedSecurityQuestion = value;
                OnPropertyChanged("SelectedSecurityQuestion");
            }
        }

        private string userName;
        private string phonenumber;
        private string email;
        private string firstName;
        private string lastName;
        private string selectedSecurityQuestion;
        private string secrityQuestionAnswer;



        // Input Button Commands
        public RelayCommand RegisterCommand { get; set; }
        public RelayCommand RegistrationCancel { get; set; }

        public RegisterUserViewModel()
        {
            userName = string.Empty;
            email = string.Empty;
            phonenumber = string.Empty;
            firstName = string.Empty;
            lastName = string.Empty;

            // Button Commands
            RegisterCommand = new RelayCommand(param => Register(param), param => CanRegister());
            RegistrationCancel = new RelayCommand(param => ClearControleData());
        }

        private async Task Register(object parameter)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                var validation = ValidateInputControles().Result;

                if (validation)
                {
                    var passwordBox = (PasswordBox)parameter;
                    if (string.IsNullOrWhiteSpace(passwordBox.Password))
                    {
                        Mouse.OverrideCursor = null;
                        MessageBox.Show("Password should not be empty", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    UserModel userModelInput = CreateUserRequestModel();
                    userModelInput.UserName = UserName;
                    userModelInput.FirstName = FirstName;
                    userModelInput.LastName = LastName;
                    userModelInput.Email = Email;
                    userModelInput.CreatedBy = userName;
                    userModelInput.Password = passwordBox.Password;
                    userModelInput.PhoneNumber = "+91" + PhoneNumber;
                  //  userModelInput.TwoFactorEnabled = false;
                  //  userModelInput.TwoFactorType = 0;

                    Global.PropertyUserName = UserName;
                    var securityQuestionResult_url = ApplicationConstants.BaseUrl + ApiRoutePathConstants.GetAllSecurityQuestions;
                    var securityQuestionResul_response = Http.Client
                                                       .PostAsync(
                                                                  securityQuestionResult_url,
                                                                  null).Result;
                    var securityQuestionsDetails = securityQuestionResul_response.Content.ReadAsStringAsync().Result;

                    var securityQuestionsResult = JsonConvert.DeserializeObject<List<SecurityQuestionModel>>(securityQuestionsDetails).ToList();

                    if (securityQuestionsResult != null && securityQuestionsResult.Count > 0)
                    {
                        userModelInput.UserSecurityQuestion[0].SecurityQuestionId = securityQuestionsResult[0].Id;
                        userModelInput.UserSecurityQuestion[0].Answer = SecrityQuestionAnswer;
                        userModelInput.UserSecurityQuestion[1].SecurityQuestionId = securityQuestionsResult[1].Id;
                        userModelInput.UserSecurityQuestion[1].Answer = SecrityQuestionAnswer;
                        userModelInput.UserSecurityQuestion[2].SecurityQuestionId = securityQuestionsResult[2].Id;
                        userModelInput.UserSecurityQuestion[2].Answer = SecrityQuestionAnswer;
                    }
                    var registerUser_Url = ApplicationConstants.BaseUrl + ApiRoutePathConstants.RegisterUser;
                    var registerUser_response = Http.Client.PostAsync(registerUser_Url, new StringContent(JsonConvert.SerializeObject(userModelInput), Encoding.UTF8, "application/json")).Result;
                    var userDetails = registerUser_response.Content.ReadAsStringAsync().Result  ;
                    DomainModel_FrameworkResult registerUser_result = JsonConvert.DeserializeObject<DomainModel_FrameworkResult>(userDetails);

                    if (registerUser_result.Status == ResultStatus.Success)
                    {

                        Global.PropertyUserName = UserName;
                        MessageBox.Show("You have registered successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        Mediator.Notify("LoginScreen", "");
                        ClearControleData().GetAwaiter().GetResult();
                    }
                    else
                    {
                        Mouse.OverrideCursor = null;
                        MessageBox.Show(registerUser_result.Errors.FirstOrDefault().Description.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    Mouse.OverrideCursor = null;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            Mouse.OverrideCursor = null;

        }
        private bool CanRegister()
        {
            return true;
        }
        public async Task<bool> ValidateInputControles()
        {
            if (string.IsNullOrWhiteSpace(UserName))
            {
                MessageBox.Show("User name should not be smpty", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Mouse.OverrideCursor = null;
                return false;
            }

            if (string.IsNullOrWhiteSpace(FirstName))
            {
                Mouse.OverrideCursor = null;
                MessageBox.Show("First name should not be empty", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (string.IsNullOrWhiteSpace(LastName))
            {
                Mouse.OverrideCursor = null;
                MessageBox.Show("Last name should not be empty", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(Email))
            {
                Mouse.OverrideCursor = null;
                MessageBox.Show("Email should not be empty", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(PhoneNumber))
            {
                Mouse.OverrideCursor = null;
                MessageBox.Show("Phone number should not be empty", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }
        public async Task ClearControleData()
        {
            UserName = string.Empty;
            FirstName = string.Empty;
            LastName = string.Empty;
            Email = string.Empty;
            PhoneNumber = string.Empty;
        }
        private static UserModel CreateUserRequestModel()
        {
            var modelList = new List<UserClaimModel>();
            modelList.Add(new UserClaimModel { ClaimType = "Forward", ClaimValue = "Ronaldo", CreatedBy = "Rosh" });

            var questionList = new List<UserSecurityQuestionModel>()
            {
                new UserSecurityQuestionModel(),
                new UserSecurityQuestionModel(),
                new UserSecurityQuestionModel(),
            };
            UserModel userRequestModel = new UserModel()
            {
                UserName = "",
                Email = "",
                PhoneNumber = "",
                TwoFactorEnabled = true,
                TwoFactorType = TwoFactorType.Email,
                Password = "",
                FirstName = "",
                LastName = "",
                DateOfBirth = new DateTime(2001, 8, 10),
                RequiresDefaultPasswordChange = false,
                CreatedBy = "",
                ModifiedBy = "",
                IdentityProviderType = IdentityProvider.Local,
                UserSecurityQuestion = questionList,
                UserClaims = modelList,
               // PhoneNumberConfirmed = true,
              //  EmailConfirmed = true,
                LockoutEnabled = false

            };

            return userRequestModel;
        }
    }
}


