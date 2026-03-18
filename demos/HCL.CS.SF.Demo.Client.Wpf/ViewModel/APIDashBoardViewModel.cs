/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.DemoClientWpfApp.Components;
using HCL.CS.SF.DemoClientWpfApp.Constants;
using HCL.CS.SF.DemoClientWpfApp.Interface;

namespace HCL.CS.SF.DemoClientWpfApp.ViewModel
{
    internal  class APIDashBoardViewModel :BaseViewModel, IPageViewModel
    {
        ApplicationParameters ApplicationConstants = new ApplicationParameters();
        public RelayCommand APiResourceCommand { get; set; }
        public RelayCommand IdentityResourceCommand { get; set; }
        public RelayCommand CleintCommand { get; set; }
        public RelayCommand RoleCommand { get; set; }
        public RelayCommand HomeCommand { get; set; }
        public RelayCommand UserRoleCommand { get; set; }
        public APIDashBoardViewModel()
        {
            HomeCommand = new RelayCommand(param => OnHome());
            APiResourceCommand = new RelayCommand(param => GetApiResource());
            IdentityResourceCommand = new RelayCommand(param => GetIdentityResource());
            CleintCommand = new RelayCommand(param => GetClient());
            RoleCommand = new RelayCommand(param => GetRole());
            HomeCommand = new RelayCommand(param => OnHome());
            UserRoleCommand = new RelayCommand(param => UserRole());
        }
        private void OnHome()
        {
            Mediator.Notify("DashBoardScreen", "");
        }
        private void UserRole()
        {
            Mediator.Notify("UserRoleScreen", "");
        }
        private void GetApiResource()
        {
            Mediator.Notify("ApiResourceGridScreen", "");
        }
        private void GetIdentityResource()
        {
            Mediator.Notify("IdentityResourceScreen", "");
        }
        private void GetClient()
        {
            Mediator.Notify("ClientScreen", "");
        }
        private void GetRole()
        {
            Mediator.Notify("RoleScreen", "");
        }
    }
}


