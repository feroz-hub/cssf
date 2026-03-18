/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.DemoClientWpfApp.Components;
using HCL.CS.SF.DemoClientWpfApp.Interface;

namespace HCL.CS.SF.DemoClientWpfApp.ViewModel
{
    internal  class DashBoardViewModel :BaseViewModel, IPageViewModel
    {
        public RelayCommand UserProfileCommand { get; set; }
        public RelayCommand PasswordChangeCommand { get; set; }
        public RelayCommand TwoFactorCommand { get; set; }

        public DashBoardViewModel()
        {
            UserProfileCommand = new RelayCommand(param => GoToUserProfile());
            PasswordChangeCommand = new RelayCommand(param => GoToPasswordChange());
            TwoFactorCommand = new RelayCommand(param => GoToTwoFactor());
        }

        public void GoToUserProfile()
        {
            Mediator.Notify("UpdateProfileScreen", "");
        }
        public void GoToPasswordChange()
        {
            Mediator.Notify("ChangePasswordScreen", "");
        }
        public void GoToTwoFactor()
        {
            Mediator.Notify("ManageAccountScreen", "");

        }
    }
}


