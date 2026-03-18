/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

/**
* @page Initialization HCL.CS.SF Initialization
* <p><strong>Initialization parameters</strong></p>
* <p><strong>1. System settings :</strong></p>
* <ul>
* <li>DBConfig** - Configure database type and connection string Mandatory</li>
* <li>LoginConfig - Configure cookie persistence</li>
* <li>UserConfig - Configure for user management</li>
* <li>PasswordConfig - Configure password management settings like minimum length, maximum length.</li>
* <li>EmailConfig** - Configure email notification type and SMTP server port, password options</li>
* <li>SMSConfig* - Configure for triggering SMS via third party lib. </li>
* <li>LdapConfig* - Configure LDAP domain, host, port settings</li>
* <li>CryptoConfig - Configure random string length for token generation.</li>
* <li>LogConfig** - Configure Log file or database for logging.</li>
* </ul>
* <p><strong>2. TokenSettings</strong></p>
* <ul>
* <li>TokenConfig** - Configure token settings like IssuerUri, client secret length, secret expiration days etc.</li>
* <li>AuthenticationConfig* - Configure CSP headers emitting on the end session callback endpoint which renders iframes to clients for front-channel signout notification.</li>
* <li>InputLengthRestrictionsConfig* - Configure maximum and minimum lengths of various parameters used in the client application.</li>
* <li>UserInteractionConfig* - Configure the login, logout and error urls used by the client application.</li>
* <li>EndpointsConfig* - Configure to enable/disable the OAuth endpoints.</li>
* <li>TokenExpiration* - Configure token expiration settings (minimum/maximum) for identity/access/refresh token.</li>
* </ul>
* <p><strong>3. NotificationSettings</strong></p>
* <ul>
* <li>SMS templates* - Configure sms template settings with name.</li>
* <li>Email�templates* - Configure email template settings like name, subject, from-address etc.</li>
* </ul>
* <p><strong>4. Configure above parameters as part of HCL.CS.SF initialization</strong></p>
* <ul>
* <li>System settings</li>
* <li>Token settings</li>
* <li>Template settings (includes Email and SMS templates for notification)</li>
* <li>Certificate for signing tokens.</li>
* </ul>
* <p><strong>5. Configure authentication scheme and cookie.</strong></p>
* \code
public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

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
            var rng = RandomNumberGenerator.Create();;
            var bytes = new byte[size];
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }
    }
* \endcode
* <br/>
* <para>* - indicates that the configuration has defaults but can be set explicitly.</para>
* <para>** - indicates that the configuration has to be mandatorily set.</para>
*/


