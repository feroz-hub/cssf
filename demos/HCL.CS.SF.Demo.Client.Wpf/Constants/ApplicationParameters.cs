/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System;
using System.Configuration;
using System.Windows;

namespace HCL.CS.SF.DemoClientWpfApp.Constants
{
    public class ApplicationParameters
    {
        public string BaseUrl;
        public string ClientId ;
        public string ClientSecret;
        public string AuthorizeEndpoint;
        public string TokenEndpoint ;
        public string UserInfoEndpoint ;
        public string IntrospectionEndpoint ;
        public string AuthResponseType;
        public string AuthScope ;
        public string AuthResponseMode;
        public string Prompt ;
        public string RedirectUri;
        public string ClientScope;
        public string HybridScope ;
        public string HybridResponseMode ;
        public string HybridResponseType;
        public string UserInfoScope ;
        public string UserInfoResponseMode ;
        public string UserInfoResponseType;
        public string JwtScope;
        public string JwtResponseMode;
        public string JwtResponseType;
        public string ROPScope;
        public string RefreshTokenScope;
        public string RefreshTokenMode;
        public string RefreshTokenType;

        public ApplicationParameters()
        {
            SetIntialParameters();
        }

        private void SetIntialParameters()
        {
            try
            {
                ClientId = ConfigurationManager.AppSettings["ClientId"];
                ClientSecret = ConfigurationManager.AppSettings["ClientSecret"];
                BaseUrl = ConfigurationManager.AppSettings["BaseUrl"];
                AuthorizeEndpoint = BaseUrl + ConfigurationManager.AppSettings["AuthorizeEndpoint"];
                TokenEndpoint = BaseUrl + ConfigurationManager.AppSettings["TokenEndpoint"];
                UserInfoEndpoint = BaseUrl + ConfigurationManager.AppSettings["UserInfoEndpoint"];
                IntrospectionEndpoint = BaseUrl + ConfigurationManager.AppSettings["IntrospectionEndpoint"];
                AuthResponseType = ConfigurationManager.AppSettings["AuthResponseType"];
                AuthScope = ConfigurationManager.AppSettings["AuthScope"];
                AuthResponseMode = ConfigurationManager.AppSettings["AuthResponseMode"];
                Prompt = ConfigurationManager.AppSettings["Prompt"];
                RedirectUri = ConfigurationManager.AppSettings["RedirectUri"];
                ClientScope = ConfigurationManager.AppSettings["ClientScope"];
                HybridScope = ConfigurationManager.AppSettings["HybridScope"];
                HybridResponseMode = ConfigurationManager.AppSettings["HybridResponseMode"];
                HybridResponseType = ConfigurationManager.AppSettings["HybridResponseType"];
                UserInfoScope = ConfigurationManager.AppSettings["UserInfoScope"];
                UserInfoResponseMode = ConfigurationManager.AppSettings["UserInfoResponseMode"];
                UserInfoResponseType = ConfigurationManager.AppSettings["UserInfoResponseType"];
                JwtScope = ConfigurationManager.AppSettings["JwtScope"];
                JwtResponseMode = ConfigurationManager.AppSettings["JwtResponseMode"];
                JwtResponseType = ConfigurationManager.AppSettings["JwtResponseType"];
                ROPScope = ConfigurationManager.AppSettings["ROPScope"];
                RefreshTokenScope = ConfigurationManager.AppSettings["RefreshTokenScope"];
                RefreshTokenMode = ConfigurationManager.AppSettings["RefreshTokenMode"];
                RefreshTokenType = ConfigurationManager.AppSettings["RefreshTokenType"];
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error-While loading configuration.", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}


