/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using HCL.CS.SF.DemoClientWpfApp.Components;
using HCL.CS.SF.DemoClientWpfApp.Interface;
using HCL.CS.SF.DemoClientWpfApp.Services;

namespace HCL.CS.SF.DemoClientWpfApp.ViewModel
{
    internal  class EndPointControllerViewModel : BaseViewModel, IPageViewModel
    {
        private string authorizationCode;
        private string accessToken;
        private string identityToken;
        private string refreshToken;

        private string audiences;
        private string issuer;
        private string signatureAlgorithm;
        private string validTo;
        private string validFrom;
        private string hybridCode;
        private string hybridState;
        private string clienScopes;
        private string refreshTokenExpiresAt;
        private string refreshTokenIssuedAt;

        // Tabs
        private bool authorizationTab;
        private bool hybridTab;
        private bool ropTab;
        private bool clientCredentialTab;
        private bool refreshTokenTab;
        private bool jWTPostTab;
        private bool userInfoTab;
        private bool refreshTokenActive;

        public string RefreshTokenExpiresAt
        {
            get
            {
                return refreshTokenExpiresAt;
            }
            set
            {
                refreshTokenExpiresAt = value;
                OnPropertyChanged("RefreshTokenExpiresAt");
            }
        }
        public string RefreshTokenIssuedAt
        {
            get
            {
                return refreshTokenIssuedAt;
            }
            set
            {
                refreshTokenIssuedAt = value;
                OnPropertyChanged("RefreshTokenIssuedAt");
            }
        }
        public bool RefreshTokenActive
        {
            get
            {
                return refreshTokenActive;
            }
            set
            {
                refreshTokenActive = value;
                OnPropertyChanged("RefreshTokenActive");
            }
        }
        public string ClienScopes
        {
            get
            {
                return clienScopes;
            }
            set
            {
                clienScopes = value;
                OnPropertyChanged("ClienScopes");
            }
        }
        public string HybridCode
        {
            get
            {
                return hybridCode;
            }
            set
            {
                hybridCode = value;
                OnPropertyChanged("HybridCode");
            }
        }
        public string HybridState
        {
            get
            {
                return hybridState;
            }
            set
            {
                hybridState = value;
                OnPropertyChanged("HybridState");
            }
        }
        public bool AuthorizationTab
        {
            get
            {
                return authorizationTab;
            }
            set
            {
                authorizationTab = value;
                OnPropertyChanged("AuthorizationTab");
            }
        }

        public bool HybridTab
        {
            get
            {
                return hybridTab;
            }
            set
            {
                hybridTab = value;
                OnPropertyChanged("HybridTab");
            }
        }
        public bool ROPTab
        {
            get
            {
                return ropTab;
            }
            set
            {
                ropTab = value;
                OnPropertyChanged("ROPTab");
            }
        }

        public bool ClientCredentialTab
        {
            get
            {
                return clientCredentialTab;
            }
            set
            {
                clientCredentialTab = value;
                OnPropertyChanged("ClientCredentialTab");
            }
        }
        public bool RefreshTokenTab
        {
            get
            {
                return refreshTokenTab;
            }
            set
            {
                refreshTokenTab = value;
                OnPropertyChanged("RefreshTokenTab");
            }
        }
        public bool JWTPostTab
        {
            get
            {
                return jWTPostTab;
            }
            set
            {
                jWTPostTab = value;
                OnPropertyChanged("JWTPostTab");
            }
        }
        public bool UserInfoTab
        {
            get
            {
                return userInfoTab;
            }
            set
            {
                userInfoTab = value;
                OnPropertyChanged("UserInfoTab");
            }
        }
        public string Audiences
        {
            get
            {
                return audiences;
            }
            set
            {
                audiences = value;
                OnPropertyChanged("Audiences");
            }
        }
        public string Issuer
        {
            get
            {
                return issuer;
            }
            set
            {
                issuer = value;
                OnPropertyChanged("Issuer");
            }
        }
        public string SignatureAlgorithm
        {
            get
            {
                return signatureAlgorithm;
            }
            set
            {
                signatureAlgorithm = value;
                OnPropertyChanged("SignatureAlgorithm");
            }
        }
        public string ValidTo
        {
            get
            {
                return validTo;
            }
            set
            {
                validTo = value;
                OnPropertyChanged("ValidTo");
            }
        }
        public string ValidFrom
        {
            get
            {
                return validFrom;
            }
            set
            {
                validFrom = value;
                OnPropertyChanged("ValidFrom");
            }
        }
        public string AuthorizationCode
        {
            get
            {
                return authorizationCode;
            }
            set
            {
                authorizationCode = value;
                OnPropertyChanged("AuthorizationCode");
            }
        }
        public string AccessToken
        {
            get
            {
                return accessToken;
            }
            set
            {
                accessToken = value;
                OnPropertyChanged("AccessToken");
            }
        }

        public string IdentityToken
        {
            get
            {
                return identityToken;
            }
            set
            {
                identityToken = value;
                OnPropertyChanged("IdentityToken");
            }
        }

        public string RefreshToken
        {
            get
            {
                return refreshToken;
            }
            set
            {
                refreshToken = value;
                OnPropertyChanged("RefreshToken");
            }
        }

        private string expiresIn;
        public string ExpiresIn
        {
            get
            {
                return expiresIn;
            }
            set
            {
                expiresIn = value;
                OnPropertyChanged("ExpiresIn");
            }
        }
        private DataTable accessTokenClaimGrid;
        public DataTable AccessTokenClaimGrid
        {
            get
            {
                return accessTokenClaimGrid;
            }
            set
            {
                accessTokenClaimGrid = value;
                OnPropertyChanged("AccessTokenClaimGrid");
            }
        }

        private DataTable hybridFlowClaimGrid;
        public DataTable HybridFlowClaimGrid
        {
            get
            {
                return hybridFlowClaimGrid;
            }
            set
            {
                hybridFlowClaimGrid = value;
                OnPropertyChanged("HybridFlowClaimGrid");
            }
        }

        private DataTable clientCredentialClaimGrid;
        public DataTable ClientCredentialClaimGrid
        {
            get
            {
                return clientCredentialClaimGrid;
            }
            set
            {
                clientCredentialClaimGrid = value;
                OnPropertyChanged("ClientCredentialClaimGrid");
            }
        }
        private DataTable ropClaimGrid;
        public DataTable RopClaimGrid
        {
            get
            {
                return ropClaimGrid;
            }
            set
            {
                ropClaimGrid = value;
                OnPropertyChanged("RopClaimGrid");
            }
        }

        private DataTable userinfoGrid;
        public DataTable UserinfoGrid
        {
            get
            {
                return userinfoGrid;
            }
            set
            {
                userinfoGrid = value;
                OnPropertyChanged("UserinfoGrid");
            }
        }

        public RelayCommand OnLoadCommand { get; set; }
        public RelayCommand HomeCommand { get; set; }
        public RelayCommand UserHomeCommand { get; set; }
        public RelayCommand APIHomeCommand { get; set; }
        public RelayCommand GotoAuthorizationCommand { get; set; }
        public RelayCommand HybridCommand { get; set; }
        public RelayCommand ROPCommand { get; set; }
        public RelayCommand GotoClientCredentialCommand { get; set; }
        public RelayCommand RefreshTokenCommand { get; set; }
        public RelayCommand EndPointHomeCommand { get; set; }
        public RelayCommand JWTPostCommand { get; set; }
        public RelayCommand UserInfoCommand { get; set; }
        public RelayCommand DashBoardCommand { get; set; }

        public EndPointControllerViewModel()
        {
            authorizationCode = string.Empty;
            identityToken = string.Empty;
            accessToken = string.Empty;
            refreshToken = string.Empty;
            audiences = string.Empty;
            issuer = string.Empty;
            signatureAlgorithm = string.Empty;
            validTo = string.Empty;
            validFrom = string.Empty;
            clienScopes = string.Empty;
            refreshTokenExpiresAt = string.Empty;
            refreshTokenActive = false;
            refreshTokenIssuedAt = string.Empty;

            //tabs
            authorizationTab = false;
            hybridTab = false;
            ropTab = false;
            clientCredentialTab = false;
            refreshTokenTab = false;
            jWTPostTab = false;
            userInfoTab = false;

            OnLoadCommand = new RelayCommand(param => AuthCodeFlow());
            GotoAuthorizationCommand= new RelayCommand(param => AuthCodeFlow());
            GotoClientCredentialCommand = new RelayCommand(param => GetCleintCredentialAcessToken());
            HybridCommand = new RelayCommand(param => GetHybridFlowToken());
            UserInfoCommand = new RelayCommand(param => GetUserInfo());
            JWTPostCommand  = new RelayCommand(param => JWTPOST());
            RefreshTokenCommand = new RelayCommand(param => RefereshToken());
            ROPCommand = new RelayCommand(param => GetROPAcessToken());
            DashBoardCommand = new RelayCommand(param => OnDashBoard());
        }
        private void OnDashBoard()
        {
            Mediator.Notify("DashBoardScreen", "");

        }
        public async Task AuthCodeFlow()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                ClearInputControles();
                AccessTokenClaimGrid = null;
                AuthorizationTab = true;
                HttpService httpService = new HttpService();

                var tokenResult = httpService.AuthCodeFlow().Result;

                if (tokenResult != null && tokenResult.ErrorResponseResult == null)
                {
                    var jwt = new JwtSecurityToken(tokenResult.access_token);
                    AccessToken = tokenResult.access_token;
                    IdentityToken = tokenResult.id_token;
                    RefreshToken = tokenResult.refresh_token;

                    DataTable dt = new DataTable();
                    dt.Columns.Add("Issuer");
                    dt.Columns.Add("ClaimType");
                    dt.Columns.Add("ClaimValue");
                    dt.Columns.Add("Subject");
                    dt.Columns.Add("ValueType");
                    dt.Columns.Add("OriginalIssuer");
                    var test = jwt.Claims.Count();
                    foreach (var item in jwt.Claims)
                    {
                        var issuer = item.Issuer;
                        var type = item.Type;
                        var value = item.Value;
                        var subject = item.Subject;
                        var valueType = item.ValueType;
                        var originalIssuer = item.OriginalIssuer;
                        dt.Rows.Add(issuer, type, value, subject, valueType, originalIssuer);
                    }
                    AccessTokenClaimGrid = dt;
                    Audiences = jwt.Audiences.FirstOrDefault();
                    Issuer = jwt.Issuer;
                    SignatureAlgorithm = jwt.SignatureAlgorithm;
                    ValidTo = jwt.ValidTo.ToString();
                    ValidFrom = jwt.ValidFrom.ToString();
                }
                else
                {
                    MessageBox.Show(tokenResult.ErrorResponseResult.error_description, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                Mouse.OverrideCursor = null;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task GetCleintCredentialAcessToken()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                ClearInputControles();

                ClientCredentialTab = true;
                ClientCredentialClaimGrid = null;
                HttpService httpService = new HttpService();
                var tokenResult = httpService.ClientCredentialFlow().Result;

                if (tokenResult != null && tokenResult.ErrorResponseResult == null)
                {
                    var jwt = new JwtSecurityToken(tokenResult.access_token);
                    AccessToken = tokenResult.access_token;
                    IdentityToken = tokenResult.id_token;
                    RefreshToken = tokenResult.refresh_token;
                    ClienScopes = tokenResult.scope;

                    DataTable dt = new DataTable();
                    dt.Columns.Add("Issuer");
                    dt.Columns.Add("ClaimType");
                    dt.Columns.Add("ClaimValue");
                    dt.Columns.Add("Subject");
                    dt.Columns.Add("ValueType");
                    dt.Columns.Add("OriginalIssuer");
                    var test = jwt.Claims.Count();
                    foreach (var item in jwt.Claims)
                    {
                        var issuer = item.Issuer;
                        var type = item.Type;
                        var value = item.Value;
                        var subject = item.Subject;
                        var valueType = item.ValueType;
                        var originalIssuer = item.OriginalIssuer;
                        dt.Rows.Add(issuer, type, value, subject, valueType, originalIssuer);
                    }
                    ClientCredentialClaimGrid = dt;

                    Audiences = jwt.Audiences.FirstOrDefault();
                    Issuer = jwt.Issuer;
                    SignatureAlgorithm = jwt.SignatureAlgorithm;
                    ValidTo = jwt.ValidTo.ToString();
                    ValidFrom = jwt.ValidFrom.ToString();
                }
                else
                {
                    MessageBox.Show(tokenResult.ErrorResponseResult.error_description, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            Mouse.OverrideCursor =null;
        }

        public async Task GetROPAcessToken()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                ClearInputControles();

                ROPTab = true;
                RopClaimGrid = null;
                HttpService httpService = new HttpService();
                var tokenResult = httpService.ROP(Global.PropertyUserName, Global.PropertyPassword).Result;

                if (tokenResult != null && tokenResult.ErrorResponseResult == null)
                {
                    var jwt = new JwtSecurityToken(tokenResult.access_token);
                    AccessToken = tokenResult.access_token;
                    IdentityToken = tokenResult.id_token;
                    RefreshToken = tokenResult.refresh_token;
                    ClienScopes = tokenResult.scope;


                    DataTable dt = new DataTable();
                    dt.Columns.Add("Issuer");
                    dt.Columns.Add("ClaimType");
                    dt.Columns.Add("ClaimValue");
                    dt.Columns.Add("Subject");
                    dt.Columns.Add("ValueType");
                    dt.Columns.Add("OriginalIssuer");
                    var test = jwt.Claims.Count();
                    foreach (var item in jwt.Claims)
                    {
                        var issuer = item.Issuer;
                        var type = item.Type;
                        var value = item.Value;
                        var subject = item.Subject;
                        var valueType = item.ValueType;
                        var originalIssuer = item.OriginalIssuer;
                        dt.Rows.Add(issuer, type, value, subject, valueType, originalIssuer);
                    }
                    RopClaimGrid = dt;

                    Audiences = jwt.Audiences.FirstOrDefault();
                    Issuer = jwt.Issuer;
                    SignatureAlgorithm = jwt.SignatureAlgorithm;
                    ValidTo = jwt.ValidTo.ToString();
                    ValidFrom = jwt.ValidFrom.ToString();
                }
                else
                {
                    MessageBox.Show(tokenResult.ErrorResponseResult.error_description, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            Mouse.OverrideCursor = null;
        }

        public async Task GetHybridFlowToken()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                ClearInputControles();
                HybridFlowClaimGrid = null;
                HybridTab = true;

                HttpService httpService = new HttpService();
                var tokenResult = httpService.HyBridFlow().Result;

                if (tokenResult != null && tokenResult.ErrorDescription == null)
                {
                    var jwt = new JwtSecurityToken(tokenResult.AccessToken);
                    AccessToken = tokenResult.AccessToken;
                    IdentityToken = tokenResult.IdentityToken;
                    RefreshToken = tokenResult.RefreshToken;
                    HybridCode = tokenResult.Code;
                    HybridState = tokenResult.State;

                    DataTable dt = new DataTable();
                    dt.Columns.Add("Issuer");
                    dt.Columns.Add("ClaimType");
                    dt.Columns.Add("ClaimValue");
                    dt.Columns.Add("Subject");
                    dt.Columns.Add("ValueType");
                    dt.Columns.Add("OriginalIssuer");
                    var test = jwt.Claims.Count();
                    foreach (var item in jwt.Claims)
                    {
                        var issuer = item.Issuer;
                        var type = item.Type;
                        var value = item.Value;
                        var subject = item.Subject;
                        var valueType = item.ValueType;
                        var originalIssuer = item.OriginalIssuer;
                        dt.Rows.Add(issuer, type, value, subject, valueType, originalIssuer);
                    }

                    HybridFlowClaimGrid = dt;
                    Audiences = jwt.Audiences.FirstOrDefault();
                    Issuer = jwt.Issuer;
                    SignatureAlgorithm = jwt.SignatureAlgorithm;
                    ValidTo = jwt.ValidTo.ToString();
                    ValidFrom = jwt.ValidFrom.ToString();
                }
                else
                {
                    MessageBox.Show(tokenResult.ErrorDescription, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            Mouse.OverrideCursor = null;
        }

        public async Task GetUserInfo()
        {
            try
            {
                ClearInputControles();
                Mouse.OverrideCursor = Cursors.Wait;
                UserInfoTab = true;
                UserinfoGrid = null;

                HttpService httpService = new HttpService();
                var tokenResult = httpService.UserInfo().Result;

                if (tokenResult != null)
                {
                    DataTable dt = new DataTable();
                    dt.Columns.Add("ClaimType");
                    dt.Columns.Add("ClaimValue");
                    foreach (var item in tokenResult)
                    {
                        var claimtype = item.Key;
                        var claimvalue = item.Value;

                        dt.Rows.Add(claimtype, claimvalue);
                    }
                    UserinfoGrid = dt;
                }
                else
                {
                    MessageBox.Show("Invalid use info", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            Mouse.OverrideCursor = null;
        }

        public async Task JWTPOST()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                ClearInputControles();
                JWTPostTab = true;
                HttpService httpService = new HttpService();

                var tokenResult = httpService.JWTPost().Result;

                if (tokenResult != null && tokenResult.ErrorResponseResult == null)
                {
                    var jwt = new JwtSecurityToken(tokenResult.access_token);
                    AccessToken = tokenResult.access_token;
                    IdentityToken = tokenResult.id_token;
                    RefreshToken = tokenResult.refresh_token;

                    Audiences = jwt.Audiences.FirstOrDefault();
                    Issuer = jwt.Issuer;
                    SignatureAlgorithm = jwt.SignatureAlgorithm;
                    ValidTo = jwt.ValidTo.ToString();
                    ValidFrom = jwt.ValidFrom.ToString();
                }
                else
                {
                    MessageBox.Show(tokenResult.ErrorResponseResult.error_description, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                Mouse.OverrideCursor = null;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task RefereshToken()
        {
            try
            {
                ClearInputControles();
                RefreshTokenTab = true;
                HttpService httpService = new HttpService();
                var tokenResult = httpService.RefreshToken().Result;

                if (tokenResult != null && tokenResult.ErrorResponseResult == null)
                {
                    AccessToken = tokenResult.access_token;
                    RefreshToken = tokenResult.refresh_token;
                    ExpiresIn = tokenResult.expires_in.ToString();
                }
                else
                {
                    MessageBox.Show(tokenResult.ErrorResponseResult.error_description, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ClearInputControles()
        {
            AuthorizationTab = false;
            AccessTokenClaimGrid = null;
            HybridTab = false;
            HybridFlowClaimGrid = null;
            UserinfoGrid = null;
            ClientCredentialTab = false;
            ClientCredentialClaimGrid = null;
            RopClaimGrid = null;
            ROPTab = false;
            JWTPostTab = false;
            RefreshTokenTab = false;
            UserInfoTab = false;
            ROPTab = false;
            hybridFlowClaimGrid = null;
            AccessToken = null;
            RefreshToken = null;
            IdentityToken = null;
            ValidFrom = null;
            ValidTo = null;
            ExpiresIn = null;
        }
    }
}


