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
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using FrameworkResult = HCL.CS.SF.DemoClientWpfApp.DomainModel.FrameworkResult;
using System.Linq;
using HCL.CS.SF.DemoClientWpfApp.Components;
using HCL.CS.SF.DemoClientWpfApp.Constants;
using HCL.CS.SF.DemoClientWpfApp.DomainModel;
using HCL.CS.SF.DemoClientWpfApp.Interface;
using HCL.CS.SF.DemoClientWpfApp.Services;
using HCL.CS.SF.DemoClientWpfApp.View;

namespace HCL.CS.SF.DemoClientWpfApp.ViewModel
{
    internal class ClientViewModel : BaseViewModel,IPageViewModel
    {
        ApplicationParameters ApplicationConstants = new ApplicationParameters();
        public RelayCommand OnLoadCommand { get; set; }
        public RelayCommand HomeCommand { get; set; }
        public RelayCommand AddClientCommand { get; set; }
        public RelayCommand UpdateClientCommand { get; set; }
        public RelayCommand DeleteClientCommand { get; set; }
        public RelayCommand RefreshCommand { get; set; }

        private ClientsModel clientSelected;

        private Dictionary<string, string> clients;
        public Dictionary<string, string> Clients
        {
            get
            {
                return clients;
            }
            set
            {
                clients = value;
                OnPropertyChanged("Clients");
            }
        }
        public ClientsModel ClientSelected
        {
            get
            {
                return clientSelected;
            }
            set
            {
                clientSelected = value;
                OnPropertyChanged("ClientSelected");
            }
        }
        public ClientViewModel()
        {
            HomeCommand = new RelayCommand(param => OnHome());
            OnLoadCommand = new RelayCommand(param => Onload());
            AddClientCommand = new RelayCommand(param => AddCleint(param));
            UpdateClientCommand = new RelayCommand(param => UpdateCleint(param));
            DeleteClientCommand = new RelayCommand(param => DeleteClient(param));
            RefreshCommand = new RelayCommand(param => Refresh());
        }
        private void Refresh()
        {
            _ = GetClients();
        }
        private void Onload()
        {
            _ = GetClients();
        }
        private async Task GetClients()
        {
            try
            {
                Clients = null;
                Http.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Global.AccessToken);
                var clienturl = ApplicationConstants.BaseUrl + ApiRoutePathConstants.GetAllClient;
                var clientresponse = Http.Client.PostAsync(clienturl, new StringContent(JsonConvert.SerializeObject(string.Empty), Encoding.UTF8, "application/json")).Result;
                var cleintResultResponse = clientresponse.Content.ReadAsStringAsync().Result;
                Clients = JsonConvert.DeserializeObject<Dictionary<string, string>>(cleintResultResponse);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void OnHome()
        {
            Mediator.Notify("DashBoardScreen", "");
        }

        private async Task AddCleint(object item)
        {
            try
            {
                ClientSelected = item as ClientsModel;
                Global.PopupMethodName = "AddClient";

                ClientPopup clientPopup = new ClientPopup();
                clientPopup.ShowDialog();
                GetClients().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task UpdateCleint(object item)
        {
            string error = string.Empty;
            try
            {
                var data = item.ToString().Replace("[", "").Replace("]", "").Split(",");
                var clientId = data[0].ToString();
                Http.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Global.AccessToken);
                var clienturl = ApplicationConstants.BaseUrl + ApiRoutePathConstants.GetClient;
                var clientresponse = Http.Client.PostAsync(clienturl, new StringContent(JsonConvert.SerializeObject(clientId), Encoding.UTF8, "application/json")).Result;
                var cleintResultResponse = clientresponse.Content.ReadAsStringAsync().Result;
                if (cleintResultResponse.Contains("ClientId"))
                {
                    ClientSelected = JsonConvert.DeserializeObject<ClientsModel>(cleintResultResponse);

                    Global.PopupMethodName = "UpdateClient";
                    Global.ClientsGetdata = ClientSelected;

                    ClientPopup clientPopup = new ClientPopup();
                    clientPopup.ShowDialog();
                    GetClients().GetAwaiter().GetResult();
                }
                else
                {
                    error= JsonConvert.DeserializeObject<string>(cleintResultResponse);
                    MessageBox.Show(error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }
        private async Task DeleteClient(object item)
        {
            try
            {
                string message = "Are you sure want to delete client?";
                string caption = "Confirmation";
                MessageBoxButton buttons = MessageBoxButton.YesNo;
                MessageBoxImage icon = MessageBoxImage.Warning;

                if (MessageBox.Show(message, caption, buttons, icon) == MessageBoxResult.Yes)
                {
                    var data = item.ToString().Replace("[", "").Replace("]", "").Split(",");
                    var clientId = data[0].ToString();
                    Http.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Global.AccessToken);

                    var clienturl = ApplicationConstants.BaseUrl + ApiRoutePathConstants.DeleteClient;
                    var clientresponse = Http.Client.PostAsync(clienturl, new StringContent(JsonConvert.SerializeObject(clientId), Encoding.UTF8, "application/json")).Result;
                    var cleintResultResponse = clientresponse.Content.ReadAsStringAsync().Result;

                    var result = JsonConvert.DeserializeObject<FrameworkResult>(cleintResultResponse);
                    if (result.Status == ResultStatus.Success)
                    {
                        MessageBox.Show("Client successfully deleted.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show(result.Errors.FirstOrDefault().Description, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    GetClients().GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}


