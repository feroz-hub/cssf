/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

/**
* @page MVC_ServerApplication MVC Server application.
* <p><strong>The HCL.CS.SF needs to be added to the services collection of the MVC Server application in the Startup class using the following code.</strong></p>
* \code
    services.AddHCLCSSF(systemSettings, tokenSettings, new NotificationTemplateSettings())
            .AddAsymmetricKeystore(LoadAsymmetricKey());
* \endcode
* <ul><li>The services.AddHCLCSSF() method has three parameters which are the System settings, tokensettings and notification template settings.</li>
* <li>The AddSymmetricKeyStore() method is used to pass the Asymmetric Keys used. This consists of all the public keys and is inserted by the LoadAsymmetricKeys() method.</li>
* <li>The GenerateRandomSalt() method is used for the strength of the salt. It is recommended to use a 16-bit strength for the salt size.</li>
* </ul>
* \code
        private List<AsymmetricKeyInfoModel> LoadAsymmetricKey()
        {
            var ecdsaCertificate = new X509Certificate2(
                "./Certificates/ECDSACertificate.pfx",
                "password",
                X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);

            var rsaCertificate = new X509Certificate2(
                "./Certificates/RSACertificate.pfx",
                "password",
                X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);

            List<AsymmetricKeyInfoModel> keyInfos = new List<AsymmetricKeyInfoModel>();

            var asymmetricKeyInfo = new AsymmetricKeyInfoModel()
            {
                Certificate = ecdsaCertificate,
                Algorithm = SigningAlgorithm.ES256,
                KeyId = GenerateRandomSalt(16),
            };
            keyInfos.Add(asymmetricKeyInfo);

            asymmetricKeyInfo = new AsymmetricKeyInfoModel()
            {
                Certificate = rsaCertificate,
                Algorithm = SigningAlgorithm.RS256,
                KeyId = GenerateRandomSalt(16),
            };
            keyInfos.Add(asymmetricKeyInfo);
            return keyInfos;
        }

        public static string GenerateRandomSalt(int size)
        {
            var rng = RandomNumberGenerator.Create();
            var bytes = new byte[size];
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }
* \endcode
* <para><strong>Add Authentication to the service collection with a default Scheme name and Add Cookie options using AddCookie(DefaultScheme).</strong></para>
* \code
        services.AddAuthentication(config =>
            {
                config.DefaultScheme = "MVCServer";
            })
           .AddCookie("MVCServer", options =>
           {
               options.Cookie.SameSite = SameSiteMode.None;
               options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
               options.Cookie.IsEssential = true;
               options.ExpireTimeSpan = TimeSpan.FromHours(10);
           });
* \endcode
* <p>To sum it up, the Configure services collection for adding the HCL.CS.SF Extension looks like below  by passing the server objects to the AddHCLCSSF() method.</strong></p>
\code
        public void ConfigureServices(IServiceCollection services)
        {
            SystemSettings systemSettings = new SystemSettings();
            systemSettings.DBConfig.Database = DbTypes.SqlServer;
            systemSettings.DBConfig.DBConnectionString = "Server=localhost\\SQLEXPRESS;Database=HCLCSSFFrameworkToken;Trusted_Connection=True;MultipleActiveResultSets=true";

            TokenSettings tokenSettings = new TokenSettings();
            tokenSettings.TokenConfig.IssuerUri = "security.HCL.CS.SF.com";
            tokenSettings.UserInteractionConfig.LoginUrl = "/account/login";
            tokenSettings.UserInteractionConfig.LogoutUrl = "/account/logout";
            tokenSettings.UserInteractionConfig.ErrorUrl = "/home/error";
            tokenSettings.InputLengthRestrictionsConfig.Nonce = 200;

            services.AddHCLCSSF(systemSettings, tokenSettings, new NotificationTemplateSettings())
                .AddAsymmetricKeystore(LoadAsymmetricKey());

            services.AddAuthentication(config =>
            {
                config.DefaultScheme = "MVCServer";
            })
           .AddCookie("MVCServer", options =>
           {
               options.Cookie.SameSite = SameSiteMode.None;
               options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
               options.Cookie.IsEssential = true;
               options.ExpireTimeSpan = TimeSpan.FromHours(10);
           });

            services.AddControllersWithViews();
            services.AddRazorPages();

            IdentityModelEventSource.ShowPII = true;
        }
* \endcode
* <para>The AddHCLCSSF() method can also be added by passing the JSON to the method as shown below</para>
* \code
        var systemSettingPath = @"C:\Source\HCL.CS.SF-Dev\IntegrationTests\Configurations\SystemSettings.json";
        var tokenConfigPath = @"C:\Source\HCL.CS.SF-Dev\IntegrationTests\Configurations\TokenSettings.json";
        var notificationTemplatePath = @"C:\Source\HCL.CS.SF-Dev\IntegrationTests\Configurations\NotificationTemplateSettings.json";
        services.AddHCLCSSF(systemSettingPath, tokenConfigPath, notificationTemplatePath);
* \endcode
* <p>Similarly, the Configure() method should add the middleware of HCL.CS.SF as in the code shown below:</p>
* \code
            app.UseHCLCSSFEndpoint();
            app.UseHCLCSSFApi();
* \endcode
* <p>The above code should be added before the code as below:</p>
* \code
            app.UseAuthentication();
            app.UseAuthorization();
* \endcode
* <p>The end points are configured using the code as below :</p>
* \code
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
* \endcode
* <p>So the Configure method in the startup class looks like the code below:</p>
* \code
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseHCLCSSFEndpoint();
            app.UseHCLCSSFApi();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
* \endcode
*/



