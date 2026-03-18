/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using FluentAssertions;
using IntegrationTests.ApiDomainModel;
using IntegrationTests.Endpoint.Setup;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using HCL.CS.SF.Domain;
using HCL.CS.SF.Domain.Constants;
using HCL.CS.SF.Domain.Constants.Endpoint;
using HCL.CS.SF.Domain.Entities.Api;
using HCL.CS.SF.Domain.Entities.Endpoint;
using HCL.CS.SF.Domain.Models.Api.Response;
using HCL.CS.SF.Domain.Models.Endpoint;
using HCL.CS.SF.DomainServices.Infra;
using HCL.CS.SF.DomainServices.Wrappers;
using HCL.CS.SF.Hosting.Extensions;
using HCL.CS.SF.Infrastructure.Data;
using HCL.CS.SF.Infrastructure.Resources;
using HCL.CS.SF.Service.Implementation.Endpoint.Extensions;
using AccessTokenType = HCL.CS.SF.Domain.Enums.AccessTokenType;
using ApplicationType = HCL.CS.SF.Domain.Enums.ApplicationType;
using ClientsModel = IntegrationTests.ApiDomainModel.ClientsModel;
using DbTypes = HCL.CS.SF.Domain.DbTypes;
using IdentityProvider = HCL.CS.SF.Domain.Enums.IdentityProvider;
using Log = HCL.CS.SF.Domain.Log;
using LogoutMessageModel = HCL.CS.SF.Domain.Models.Endpoint.LogoutMessageModel;
using SigningAlgorithm = HCL.CS.SF.Domain.Enums.SigningAlgorithm;
using UserModel = HCL.CS.SF.Domain.Models.Api.UserModel;
using WriteLogTo = HCL.CS.SF.Domain.WriteLogTo;

namespace IntegrationTests;

internal struct ClientInfoDictonary
{
    public string ClientId;
    public string ClientSecret;
}

internal struct UserInfoDictonary
{
    public string UserID;
    public string Password;
    public string UserName;
}

public abstract class HCLCSSFFakeSetup
{
    private const string SolutionFileName = "HCL.CS.SF.sln";
    private const string AllowInsecureHttpDevEnvVar = "HCL.CS.SF_ALLOW_INSECURE_HTTP_DEV";
    private static readonly string[] SqliteSeedScriptNames = ["HCL.CS.SFSqliteV1.sql", "HCLCSSFSqliteV1.sql"];

    public const string BaseUrl = "https://server";

    public const string LoginPage = BaseUrl + "/security/login";

    public const string ErrorPage = BaseUrl + "/home/error";

    public const string DiscoveryEndpoint = BaseUrl + "/.well-known/openid-configuration";

    public const string DiscoveryKeysEndpoint = BaseUrl + "/.well-known/openid-configuration/jwks";

    public const string AuthorizeEndpoint = BaseUrl + "/security/authorize";

    public const string TokenEndpoint = BaseUrl + "/security/token";

    public const string RevocationEndpoint = BaseUrl + "/security/revocation";

    public const string UserInfoEndpoint = BaseUrl + "/security/userinfo";

    public const string IntrospectionEndpoint = BaseUrl + "/security/introspect";

    public const string EndSessionEndpoint = BaseUrl + "/security/endsession";

    public const string EndSessionCallbackEndpoint = BaseUrl + "/security/endsession/callback";

    private static readonly object TemplateDatabaseLock = new();
    private static readonly object AsymmetricKeyLock = new();

    private static readonly string IntegrationRuntimeDirectory =
        Path.Combine(Path.GetTempPath(), "HCL.CS.SF-integration-tests");

    private static readonly string TemplateDatabasePath = Path.Combine(IntegrationRuntimeDirectory, "template.db");
    private static bool templateDatabaseReady;
    private static int runtimeDatabaseCounter;
    private static List<AsymmetricKeyInfoModel> cachedAsymmetricKeys;

    internal readonly Dictionary<string, ClientInfoDictonary> clientMasterData = new()
    {
        {
            "HCL.CS.SF Plain PKCE Client",
            new ClientInfoDictonary
            {
                ClientId = "ZqWJUx2H09BegYdhYCfNqyaRR/5WKdPW7PYQYC8jG3U=",
                ClientSecret = "4JjFMCgRmQ1aI1jGIkHF5BoX3klaVfDe1yOrl3ENY1Q="
            }
        },
        {
            "HCL.CS.SF S256 Client",
            new ClientInfoDictonary
            {
                ClientId = "ZqWJUx2H09BegYdhYCfNqyaRR/5WKdPW7PYQYC8jG3U=",
                ClientSecret = "4JjFMCgRmQ1aI1jGIkHF5BoX3klaVfDe1yOrl3ENY1Q="
            }
        },
        {
            "HCL.CS.SF Early Token Expire Client",
            new ClientInfoDictonary
            {
                ClientId = "1S/q8V3g2lSgHXCBgvrp8fQmg6FPbC6gTk97jk/y5NY=",
                ClientSecret = "f48+r5BnSCRJbhUfImAmmfa1pnlPDshxUgWpW6/002Y="
            }
        },
        {
            "Client Secret Expire Client",
            new ClientInfoDictonary
            {
                ClientId = "mayUgshFdr/ZQHftlS7U8tbAgiBnso7DrOyCzkhUsek=",
                ClientSecret = "CcWhRQBv2MeNrxspc4OS/SqCfEesAhb5OtvkWVTMF/I="
            }
        },
        {
            "HCL.CS.SF HS256",
            new ClientInfoDictonary
            {
                ClientId = "EHnrboVfa6o4MerBkHknMIwsizxW1j9erega/00sfn4=",
                ClientSecret = "ovxWKDE0t6krF44qFxBhPQnprtMLzc+ZToZR+SIIQjo="
            }
        },
        {
            "HCL.CS.SF HS512",
            new ClientInfoDictonary
            {
                ClientId = "hcEvdzPzQesqwBFh81Hu/gVcnJvydis/IgAAuCfgG3E=",
                ClientSecret = "d5t99Qv05UWZH5NvzAIFn3PTqao2qnusLrtss8GfY+A="
            }
        },
        {
            "HCL.CS.SF HS384",
            new ClientInfoDictonary
            {
                ClientId = "rYH/wy+ND32LH97lkCKx/K9Ts8I0pnNz0e+abr9g4UM=",
                ClientSecret = "PIqOGwLC+FozpvGBhx5CIAS0erEmikM8yWotayNZWOw="
            }
        },
        {
            "HCL.CS.SF RS256",
            new ClientInfoDictonary
            {
                ClientId = "M2FMHy6ORqM7+ou2WbMxQAjs44vdCjGTVNMyqtwCCIQ=",
                ClientSecret = "ODP5IbN9hJGEoDE5JjXoxgP0IuKOnKURwD0bhFwM2kc="
            }
        },
        {
            "HCL.CS.SF RS512",
            new ClientInfoDictonary
            {
                ClientId = "ygUjWHNvRpZfPEtgVEELAD3Zxi343op3guOuI2LX3co=",
                ClientSecret = "ZUjMMbt9mpIkIJxuyXB+dAgwc+Fi0FvtdoPVaucDU8k="
            }
        },
        {
            "HCL.CS.SF RS384",
            new ClientInfoDictonary
            {
                ClientId = "t4pYkl2ig4K/4YMYOmZ2azePXh2lHj1YXE5L44ggGVU=",
                ClientSecret = "qki70cOU/fv7md+pWJjW6Vfar27GEzrSof6EWPL7bAg="
            }
        },
        {
            "HCL.CS.SF PS256",
            new ClientInfoDictonary
            {
                ClientId = "8Y004Tgl9WETzWJqLWetE/xVBpgx9O2leioGx5eREb4=",
                ClientSecret = "QiIsviZ/UOmb8ZtLPf+aoUDkQicsxR2mVRdfChWSW0o="
            }
        },
        {
            "HCL.CS.SF PS512",
            new ClientInfoDictonary
            {
                ClientId = "uAePfBZhPl/MZkTOKB2cbNGWMUoNCoGvYo0vAibsuxA=",
                ClientSecret = "eWdwhdS8UhcXI3PE6wv4PbRErZ29x2fLSjrEsx6pG1A="
            }
        },
        {
            "HCL.CS.SF PS384",
            new ClientInfoDictonary
            {
                ClientId = "PNwf0Rs9ET4424K0ycSav8LV3AvTulgTV6eQz91DekI=",
                ClientSecret = "+wf5+pYOkHYZhJiVzBaBqIzsNiACbeVDEWoFluL7w50="
            }
        },
        {
            "HCL.CS.SF ES256",
            new ClientInfoDictonary
            {
                ClientId = "1mbMZjpr6Fo1oYntUTsySd1j+CzvjB4BngeO+xt0A0A=",
                ClientSecret = "AcS9YsAxh8LdPULptEUUbBb9khyybhof59Lp81ZaPfc="
            }
        },
        {
            "HCL.CS.SF ES512",
            new ClientInfoDictonary
            {
                ClientId = "6OLE/ogt2TG++Tcwg5YS46X2olOLPMPVf/zRLx9Zt+g=",
                ClientSecret = "ou7c+Gy5Hry70S0R7lr05xRpv9GCvt775uHcgq0XsEU="
            }
        },
        {
            "HCL.CS.SF ES384",
            new ClientInfoDictonary
            {
                ClientId = "0Vlmwf7nA8Oc/NyOd7qk0b9CAvqRARKv75oeps+XU2g=",
                ClientSecret = "CKutTZGMAe55/MELhqtKgZ/kCTGCw+fNm05MeG2kAaE="
            }
        },
        {
            "HCL.CS.SF ES256 Algorithm Client",
            new ClientInfoDictonary
            {
                ClientId = "1mbMZjpr6Fo1oYntUTsySd1j+CzvjB4BngeO+xt0A0A=",
                ClientSecret = "AcS9YsAxh8LdPULptEUUbBb9khyybhof59Lp81ZaPfc="
            }
        }
    };

    private readonly string hCLCSS256ClientName = "HCL.CS.SF S256 Client";

    private readonly AuthenticationProperties props = new();

    internal readonly Dictionary<string, UserInfoDictonary> userMasterData = new()
    {
        { "BobUser", new UserInfoDictonary { UserID = "A72D718E-93C7-44B1-4CC3-08DA1D440E53", UserName = "BobUser" } },
        {
            "JacobIsmail",
            new UserInfoDictonary { UserID = "5772F27E-8D69-4C9D-4CC4-08DA1D440E53", UserName = "JacobIsmail" }
        },
        {
            "BOBALICE",
            new UserInfoDictonary { UserID = "8F1CD805-555E-4B19-4CC5-08DA1D440E53", UserName = "JacobIsmail" }
        }
    };

    // private readonly string schema = "Security.Identity";
    private string positiveCaseClientName = "HCL.CS.SF Plain PKCE Client";

    private IResourceStringHandler resourceStringHandler;
    private UserModel user;

    public HCLCSSFFakeSetup(string basePath = null)
    {
        Environment.SetEnvironmentVariable(AllowInsecureHttpDevEnvVar, "true");
        ConfigureClientMasterData();
        Initialize();
        UserName = "checktest";
        Password = "Test@123456789";
    }

    public string LoginUrl { get; set; } = "/security/login";

    public string LogoutUrl { get; set; } = "/security/logout";

    public string IssueUrl { get; set; } = string.Empty;

    public string ErrorUrl { get; set; } = "/home/error";

    public List<AsymmetricKeyInfoModel> AsymmetricKeys { get; set; }

    public ServiceProvider ServiceProvider { get; set; }

    public UserAgent FrontChannelClient { get; set; }

    public HttpClient BackChannelClient { get; set; }

    public ClaimsPrincipal Subject { get; set; }

    public IConfiguration Configuration { get; }

    public bool LoginPageCalled { get; set; }

    public bool LogoutPageCalled { get; set; }

    public LogoutMessageModel LogoutRequest { get; set; }

    public string UserName { get; set; }

    public string Password { get; set; }

    public IResourceStringHandler ResourceStringHandler
    {
        get
        {
            if (resourceStringHandler != null) return resourceStringHandler;

            resourceStringHandler = new ResourceStringHandler();
            return resourceStringHandler;
        }
    }

    public UserModel User
    {
        get
        {
            if (user == null)
            {
                user = new UserModel();
                user.UserName = UserName;
                user.Password = Password;
            }

            return user;
        }
    }

    // This method gets called by the runtime. Use this method to add services to the container.
    public event Action<IServiceCollection> OnPostConfigureServices = services => { };

    public void ConfigureServices(IServiceCollection services)
    {
        var integrationRootPath = GetIntegrationTestsRootPath();
        var notificationSettings = LoadNotificationTemplateSettings(integrationRootPath);
        var runtimeDatabasePath = CreateIsolatedDatabase(notificationSettings);
        var systemSettings = CreateSystemSettings(runtimeDatabasePath);
        var tokenSettings = CreateTokenSettings();

        services.AddHCLCSSF(systemSettings, tokenSettings, notificationSettings)
            .AddAsymmetricKeystore(LoadAsymmetricKey());
        OnPostConfigureServices(services);

        ServiceProvider = services.BuildServiceProvider();
    }

    private void ConfigureClientMasterData()
    {
        var clientNames = clientMasterData.Keys.ToList();
        foreach (var clientName in clientNames)
            clientMasterData[clientName] = new ClientInfoDictonary
            {
                ClientId = CreateDeterministicCredential($"client-id:{clientName}"),
                ClientSecret = CreateDeterministicCredential($"client-secret:{clientName}")
            };
    }

    private static string CreateDeterministicCredential(string value)
    {
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(value));
        return Convert.ToBase64String(hash);
    }

    private string CreateIsolatedDatabase(NotificationTemplateSettings notificationSettings)
    {
        lock (TemplateDatabaseLock)
        {
            Directory.CreateDirectory(IntegrationRuntimeDirectory);
            EnsureTemplateDatabase(notificationSettings);

            var runtimeDatabasePath = Path.Combine(
                IntegrationRuntimeDirectory,
                $"integration-{Interlocked.Increment(ref runtimeDatabaseCounter)}.db");

            if (File.Exists(runtimeDatabasePath)) File.Delete(runtimeDatabasePath);

            File.Copy(TemplateDatabasePath, runtimeDatabasePath, true);
            return runtimeDatabasePath;
        }
    }

    private void EnsureTemplateDatabase(NotificationTemplateSettings notificationSettings)
    {
        if (templateDatabaseReady && File.Exists(TemplateDatabasePath)) return;

        if (File.Exists(TemplateDatabasePath)) File.Delete(TemplateDatabasePath);

        var templateServices = new ServiceCollection();
        templateServices.AddHCLCSSF(
                CreateSystemSettings(TemplateDatabasePath),
                CreateTokenSettings(),
                notificationSettings)
            .AddAsymmetricKeystore(LoadAsymmetricKey());

        using var templateProvider = templateServices.BuildServiceProvider();
        SeedTemplateDatabaseAsync(templateProvider).GetAwaiter().GetResult();
        templateDatabaseReady = true;
    }

    private async Task SeedTemplateDatabaseAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var scopedProvider = scope.ServiceProvider;
        var dbContext = scopedProvider.GetRequiredService<ApplicationDbContext>();

        dbContext.Database.EnsureDeleted();
        ApplySqliteSchema(dbContext);

        SeedIdentityResources(dbContext);
        SeedApiResources(dbContext);
        SeedSecurityQuestions(dbContext);
        SeedClients(dbContext);
        PopulateMissingRowVersions(dbContext);
        await ((DbContext)dbContext).SaveChangesAsync();

        await SeedRolesAsync(scopedProvider);
        PopulateMissingRowVersions(dbContext);
        await ((DbContext)dbContext).SaveChangesAsync();

        await SeedRoleClaimsAsync(scopedProvider);
        PopulateMissingRowVersions(dbContext);
        await ((DbContext)dbContext).SaveChangesAsync();

        await SeedUsersAsync(scopedProvider);
        PopulateMissingRowVersions(dbContext);
        await ((DbContext)dbContext).SaveChangesAsync();

        await SeedUserRolesAsync(scopedProvider);
        PopulateMissingRowVersions(dbContext);
        await ((DbContext)dbContext).SaveChangesAsync();
    }

    private static void PopulateMissingRowVersions(DbContext dbContext)
    {
        var utcNow = DateTime.UtcNow;
        foreach (var entry in dbContext.ChangeTracker.Entries())
        {
            var createdOnProperty =
                entry.Properties.FirstOrDefault(property => property.Metadata.Name == nameof(BaseEntity.CreatedOn));
            if (createdOnProperty != null && createdOnProperty.CurrentValue is DateTime createdOn &&
                createdOn == default) createdOnProperty.CurrentValue = utcNow;

            var createdByProperty =
                entry.Properties.FirstOrDefault(property => property.Metadata.Name == nameof(BaseEntity.CreatedBy));
            if (createdByProperty != null && string.IsNullOrWhiteSpace(createdByProperty.CurrentValue?.ToString()))
                createdByProperty.CurrentValue = "Seed";

            var modifiedByProperty =
                entry.Properties.FirstOrDefault(property => property.Metadata.Name == nameof(BaseEntity.ModifiedBy));
            if (modifiedByProperty != null && string.IsNullOrWhiteSpace(modifiedByProperty.CurrentValue?.ToString()))
                modifiedByProperty.CurrentValue = createdByProperty?.CurrentValue?.ToString() ?? "Seed";

            var rowVersionProperty =
                entry.Properties.FirstOrDefault(property => property.Metadata.Name == nameof(BaseEntity.RowVersion));
            if (rowVersionProperty == null) continue;

            if (rowVersionProperty.CurrentValue is byte[] currentValue && currentValue.Length > 0) continue;

            rowVersionProperty.CurrentValue = new byte[] { 1 };
        }
    }

    private static void ApplySqliteSchema(ApplicationDbContext dbContext)
    {
        ExecuteSqlScript(dbContext, ResolveSqliteSchemaPath());
        ApplySqliteCompatibilityMigrations(dbContext);
    }

    private static string GetRepositoryRootPath()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null)
        {
            if (File.Exists(Path.Combine(directory.FullName, SolutionFileName))) return directory.FullName;

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Unable to resolve repository root path.");
    }

    private static string ResolveSqliteSchemaPath()
    {
        var repositoryRootPath = GetRepositoryRootPath();

        foreach (var fileName in SqliteSeedScriptNames)
        {
            var candidatePath = Path.Combine(
                repositoryRootPath,
                "scripts",
                "seed",
                "Sqlite",
                fileName);

            if (File.Exists(candidatePath)) return candidatePath;
        }

        throw new FileNotFoundException(
            "SQLite seed schema script not found.",
            Path.Combine(repositoryRootPath, "scripts", "seed", "Sqlite", SqliteSeedScriptNames[0]));
    }

    private static void ApplySqliteCompatibilityMigrations(ApplicationDbContext dbContext)
    {
        ExecuteSqliteStatementIgnoringDuplicateColumn(
            dbContext,
            "ALTER TABLE \"HCL.CS.SF_Clients\" ADD COLUMN \"PreferredAudience\" TEXT NULL;");

        dbContext.Database.ExecuteSqlRaw(
            """
            CREATE TABLE IF NOT EXISTS "HCL.CS.SF_NotificationProviderConfig" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_HCL.CS.SF_NotificationProviderConfig" PRIMARY KEY,
                "IsDeleted" INTEGER NOT NULL DEFAULT 0,
                "CreatedOn" TEXT NOT NULL,
                "ModifiedOn" TEXT NULL,
                "CreatedBy" TEXT NOT NULL,
                "ModifiedBy" TEXT NULL,
                "ProviderName" TEXT NOT NULL,
                "ChannelType" INTEGER NOT NULL,
                "IsActive" INTEGER NOT NULL,
                "ConfigJson" TEXT NOT NULL,
                "LastTestedOn" TEXT NULL,
                "LastTestSuccess" INTEGER NULL
            );

            CREATE INDEX IF NOT EXISTS "IX_NPC_CHANNEL_TYPE"
                ON "HCL.CS.SF_NotificationProviderConfig" ("ChannelType");

            CREATE INDEX IF NOT EXISTS "IX_NPC_CHANNEL_ACTIVE"
                ON "HCL.CS.SF_NotificationProviderConfig" ("ChannelType", "IsActive");
            """);

        dbContext.Database.ExecuteSqlRaw(
            """
            CREATE TABLE IF NOT EXISTS "HCL.CS.SF_ExternalAuthProviderConfig" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_HCL.CS.SF_ExternalAuthProviderConfig" PRIMARY KEY,
                "IsDeleted" INTEGER NOT NULL DEFAULT 0,
                "CreatedOn" TEXT NOT NULL,
                "ModifiedOn" TEXT NULL,
                "CreatedBy" TEXT NOT NULL,
                "ModifiedBy" TEXT NULL,
                "ProviderName" TEXT NOT NULL,
                "ProviderType" INTEGER NOT NULL,
                "IsEnabled" INTEGER NOT NULL,
                "ConfigJson" TEXT NOT NULL,
                "AutoProvisionEnabled" INTEGER NOT NULL DEFAULT 0,
                "AllowedDomains" TEXT NULL,
                "LastTestedOn" TEXT NULL,
                "LastTestSuccess" INTEGER NULL
            );

            CREATE UNIQUE INDEX IF NOT EXISTS "IX_EAPC_PROVIDER"
                ON "HCL.CS.SF_ExternalAuthProviderConfig" ("ProviderName");

            CREATE INDEX IF NOT EXISTS "IX_EAPC_PROVIDER_ENABLED"
                ON "HCL.CS.SF_ExternalAuthProviderConfig" ("ProviderName", "IsEnabled");
            """);
    }

    private static void ExecuteSqlScript(ApplicationDbContext dbContext, string scriptPath)
    {
        if (!File.Exists(scriptPath))
            throw new FileNotFoundException("SQL script not found.", scriptPath);

        var sql = File.ReadAllText(scriptPath);
        dbContext.Database.ExecuteSqlRaw(sql);
    }

    private static void ExecuteSqliteStatementIgnoringDuplicateColumn(ApplicationDbContext dbContext, string sql)
    {
        try
        {
            dbContext.Database.ExecuteSqlRaw(sql);
        }
        catch (SqliteException ex) when (ex.Message.Contains("duplicate column name", StringComparison.OrdinalIgnoreCase))
        {
            // SQLite does not support IF NOT EXISTS for ALTER TABLE ADD COLUMN on older engines.
        }
    }

    private static void SeedIdentityResources(ApplicationDbContext dbContext)
    {
        var utcNow = DateTime.UtcNow;
        dbContext.IdentityResources.AddRange(
            new IdentityResources
            {
                Id = Guid.NewGuid(),
                Name = "openid",
                DisplayName = "Your user identifier",
                Description = "Your user identifier",
                Enabled = true,
                Required = true,
                Emphasize = true,
                CreatedBy = "Seed",
                ModifiedBy = "Seed",
                CreatedOn = utcNow,
                IdentityClaims = new List<IdentityClaims>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Type = "sub",
                        AliasType = "sub",
                        CreatedBy = "Seed",
                        ModifiedBy = "Seed",
                        CreatedOn = utcNow
                    }
                }
            },
            new IdentityResources
            {
                Id = Guid.NewGuid(),
                Name = "profile",
                DisplayName = "User profile",
                Description = "Your user profile information",
                Enabled = true,
                Required = false,
                Emphasize = true,
                CreatedBy = "Seed",
                ModifiedBy = "Seed",
                CreatedOn = utcNow,
                IdentityClaims = new List<IdentityClaims>
                {
                    new()
                    {
                        Id = Guid.NewGuid(), Type = "name", AliasType = "username", CreatedBy = "Seed",
                        ModifiedBy = "Seed", CreatedOn = utcNow
                    },
                    new()
                    {
                        Id = Guid.NewGuid(), Type = "family_name", AliasType = "lastname", CreatedBy = "Seed",
                        ModifiedBy = "Seed", CreatedOn = utcNow
                    },
                    new()
                    {
                        Id = Guid.NewGuid(), Type = "given_name", AliasType = "firstname", CreatedBy = "Seed",
                        ModifiedBy = "Seed", CreatedOn = utcNow
                    },
                    new()
                    {
                        Id = Guid.NewGuid(), Type = "birthdate", AliasType = "dateofbirth", CreatedBy = "Seed",
                        ModifiedBy = "Seed", CreatedOn = utcNow
                    },
                    new()
                    {
                        Id = Guid.NewGuid(), Type = "street", AliasType = "street", CreatedBy = "Seed",
                        ModifiedBy = "Seed", CreatedOn = utcNow
                    },
                    new()
                    {
                        Id = Guid.NewGuid(), Type = "city", AliasType = "city", CreatedBy = "Seed",
                        ModifiedBy = "Seed", CreatedOn = utcNow
                    },
                    new()
                    {
                        Id = Guid.NewGuid(), Type = "pincode", AliasType = "pincode", CreatedBy = "Seed",
                        ModifiedBy = "Seed", CreatedOn = utcNow
                    }
                }
            },
            new IdentityResources
            {
                Id = Guid.NewGuid(),
                Name = "email",
                DisplayName = "Your email address",
                Description = "Your email address",
                Enabled = true,
                Required = false,
                Emphasize = true,
                CreatedBy = "Seed",
                ModifiedBy = "Seed",
                CreatedOn = utcNow,
                IdentityClaims = new List<IdentityClaims>
                {
                    new()
                    {
                        Id = Guid.NewGuid(), Type = "email", AliasType = "email", CreatedBy = "Seed",
                        ModifiedBy = "Seed", CreatedOn = utcNow
                    },
                    new()
                    {
                        Id = Guid.NewGuid(), Type = "email_verified", AliasType = "emailconfirmed", CreatedBy = "Seed",
                        ModifiedBy = "Seed", CreatedOn = utcNow
                    }
                }
            },
            new IdentityResources
            {
                Id = Guid.NewGuid(),
                Name = "phone",
                DisplayName = "Your phone number",
                Description = "Your phone number",
                Enabled = true,
                Required = false,
                Emphasize = true,
                CreatedBy = "Seed",
                ModifiedBy = "Seed",
                CreatedOn = utcNow,
                IdentityClaims = new List<IdentityClaims>
                {
                    new()
                    {
                        Id = Guid.NewGuid(), Type = "phone_number", AliasType = "phonenumber", CreatedBy = "Seed",
                        ModifiedBy = "Seed", CreatedOn = utcNow
                    },
                    new()
                    {
                        Id = Guid.NewGuid(), Type = "phone_number_verified", AliasType = "phonenumberconfirmed",
                        CreatedBy = "Seed",
                        ModifiedBy = "Seed", CreatedOn = utcNow
                    }
                }
            },
            new IdentityResources
            {
                Id = Guid.NewGuid(),
                Name = "address",
                DisplayName = "Your postal address",
                Description = "Your postal address",
                Enabled = true,
                Required = false,
                Emphasize = false,
                CreatedBy = "Seed",
                ModifiedBy = "Seed",
                CreatedOn = utcNow,
                IdentityClaims = new List<IdentityClaims>
                {
                    new()
                    {
                        Id = Guid.NewGuid(), Type = "address", AliasType = "street", CreatedBy = "Seed",
                        ModifiedBy = "Seed", CreatedOn = utcNow
                    }
                }
            });
    }

    private static void SeedApiResources(ApplicationDbContext dbContext)
    {
        var utcNow = DateTime.UtcNow;
        var resourceNames = new[]
        {
            "HCL.CS.SF.client",
            "HCL.CS.SF.user",
            "HCL.CS.SF.role",
            "HCL.CS.SF.apiresource",
            "HCL.CS.SF.identityresource",
            "HCL.CS.SF.adminuser",
            "HCL.CS.SF.securitytoken"
        };

        foreach (var resourceName in resourceNames)
            dbContext.ApiResources.Add(new ApiResources
            {
                Id = Guid.NewGuid(),
                Name = resourceName,
                DisplayName = resourceName,
                Description = $"{resourceName} resource",
                Enabled = true,
                CreatedBy = "Seed",
                ModifiedBy = "Seed",
                CreatedOn = utcNow,
                ApiResourceClaims = new List<ApiResourceClaims>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Type = "permission",
                        CreatedBy = "Seed",
                        ModifiedBy = "Seed",
                        CreatedOn = utcNow
                    }
                },
                ApiScopes = new List<ApiScopes>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Name = resourceName,
                        DisplayName = resourceName,
                        Description = $"{resourceName} scope",
                        Required = false,
                        Emphasize = false,
                        CreatedBy = "Seed",
                        ModifiedBy = "Seed",
                        CreatedOn = utcNow,
                        ApiScopeClaims = new List<ApiScopeClaims>
                        {
                            new()
                            {
                                Id = Guid.NewGuid(),
                                Type = "permission",
                                CreatedBy = "Seed",
                                ModifiedBy = "Seed",
                                CreatedOn = utcNow
                            }
                        }
                    }
                }
            });
    }

    private static void SeedSecurityQuestions(ApplicationDbContext dbContext)
    {
        var utcNow = DateTime.UtcNow;
        dbContext.SecurityQuestions.AddRange(
            new SecurityQuestions
            {
                Id = Guid.NewGuid(), Question = "What is your age?", CreatedBy = "Seed",
                ModifiedBy = "Seed", CreatedOn = utcNow
            },
            new SecurityQuestions
            {
                Id = Guid.NewGuid(), Question = "What is your first phone number?", CreatedBy = "Seed",
                ModifiedBy = "Seed", CreatedOn = utcNow
            },
            new SecurityQuestions
            {
                Id = Guid.NewGuid(), Question = "What is your office name?", CreatedBy = "Seed",
                ModifiedBy = "Seed", CreatedOn = utcNow
            },
            new SecurityQuestions
            {
                Id = Guid.NewGuid(), Question = "What is your favourite color?", CreatedBy = "Seed",
                ModifiedBy = "Seed", CreatedOn = utcNow
            });
    }

    private void SeedClients(ApplicationDbContext dbContext)
    {
        var utcNow = DateTime.UtcNow;
        var seedClients = new[]
        {
            new ClientSeedDefinition("HCL.CS.SF Plain PKCE Client", OpenIdConstants.Algorithms.RsaSha256, true, true, 3600,
                3600, 7200, 1900, 180),
            new ClientSeedDefinition("HCL.CS.SF S256 Client", OpenIdConstants.Algorithms.RsaSha256, true, true, 3600, 3600,
                7200, 1900, 180),
            new ClientSeedDefinition("HCL.CS.SF Early Token Expire Client", OpenIdConstants.Algorithms.RsaSha256, true,
                true, 1800, 1800, 1800, 1800, 180),
            new ClientSeedDefinition("Client Secret Expire Client", OpenIdConstants.Algorithms.RsaSha256, true, true,
                3600, 3600, 7200, 1900, -1),
            new ClientSeedDefinition("HCL.CS.SF HS256", OpenIdConstants.Algorithms.HmacSha256, true, true, 3600, 3600,
                7200, 1900, 180),
            new ClientSeedDefinition("HCL.CS.SF HS512", OpenIdConstants.Algorithms.HmacSha512, true, true, 3600, 3600,
                7200, 1900, 180),
            new ClientSeedDefinition("HCL.CS.SF HS384", OpenIdConstants.Algorithms.HmacSha384, true, true, 3600, 3600,
                7200, 1900, 180),
            new ClientSeedDefinition("HCL.CS.SF RS256", OpenIdConstants.Algorithms.RsaSha256, true, true, 3600, 3600, 7200,
                1900, 180),
            new ClientSeedDefinition("HCL.CS.SF RS512", OpenIdConstants.Algorithms.RsaSha512, true, true, 3600, 3600, 7200,
                1900, 180),
            new ClientSeedDefinition("HCL.CS.SF RS384", OpenIdConstants.Algorithms.RsaSha384, true, true, 3600, 3600, 7200,
                1900, 180),
            new ClientSeedDefinition("HCL.CS.SF PS256", OpenIdConstants.Algorithms.RsaSsaPssSha256, true, true, 3600, 3600,
                7200, 1900, 180),
            new ClientSeedDefinition("HCL.CS.SF PS512", OpenIdConstants.Algorithms.RsaSsaPssSha512, true, true, 3600, 3600,
                7200, 1900, 180),
            new ClientSeedDefinition("HCL.CS.SF PS384", OpenIdConstants.Algorithms.RsaSsaPssSha384, true, true, 3600, 3600,
                7200, 1900, 180),
            new ClientSeedDefinition("HCL.CS.SF ES256", OpenIdConstants.Algorithms.EcdsaSha256, true, true, 3600, 3600,
                7200, 1900, 180),
            new ClientSeedDefinition("HCL.CS.SF ES512", OpenIdConstants.Algorithms.EcdsaSha512, true, true, 3600, 3600,
                7200, 1900, 180),
            new ClientSeedDefinition("HCL.CS.SF ES384", OpenIdConstants.Algorithms.EcdsaSha384, true, true, 3600, 3600,
                7200, 1900, 180),
            new ClientSeedDefinition("HCL.CS.SF ES256 Algorithm Client", OpenIdConstants.Algorithms.EcdsaSha256, true,
                true, 3600, 3600, 7200, 1900, 180)
        };

        var allowedScopes = string.Join(
            " ", "openid", "email", "profile", "phone", "address", "offline_access", "HCL.CS.SF.client", "HCL.CS.SF.user",
            "HCL.CS.SF.role", "HCL.CS.SF.apiresource", "HCL.CS.SF.identityresource", "HCL.CS.SF.adminuser", "HCL.CS.SF.securitytoken");

        var supportedGrantTypes = string.Join(
            " ", OpenIdConstants.GrantTypes.AuthorizationCode, OpenIdConstants.GrantTypes.ClientCredentials,
            OpenIdConstants.GrantTypes.Password, OpenIdConstants.GrantTypes.RefreshToken, "hybrid");

        var supportedResponseTypes = string.Join(
            " ", OpenIdConstants.ResponseTypes.Code, OpenIdConstants.ResponseTypes.IdToken,
            OpenIdConstants.ResponseTypes.Token);

        foreach (var seedClient in seedClients)
        {
            if (!clientMasterData.TryGetValue(seedClient.ClientName, out var credentials)) continue;

            var clientId = Guid.NewGuid();
            dbContext.Clients.Add(new Clients
            {
                Id = clientId,
                ClientId = credentials.ClientId,
                ClientSecret = credentials.ClientSecret,
                ClientName = seedClient.ClientName,
                ClientUri = "https://identity:5002",
                LogoUri = "https://localhost:5002/logo.png",
                TermsOfServiceUri = "https://localhost:5002/tos",
                PolicyUri = "https://localhost:5002/policy",
                ClientIdIssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                ClientSecretExpiresAt = DateTimeOffset.UtcNow.AddDays(seedClient.ClientSecretExpirationInDays)
                    .ToUnixTimeSeconds(),
                RefreshTokenExpiration = seedClient.RefreshTokenExpiration,
                AccessTokenExpiration = seedClient.AccessTokenExpiration,
                IdentityTokenExpiration = seedClient.IdentityTokenExpiration,
                LogoutTokenExpiration = 3600,
                AuthorizationCodeExpiration = seedClient.AuthorizationCodeExpiration,
                AccessTokenType = AccessTokenType.JWT,
                RequirePkce = true,
                IsPkceTextPlain = seedClient.IsPkceTextPlain,
                RequireClientSecret = seedClient.RequireClientSecret,
                IsFirstPartyApp = true,
                AllowOfflineAccess = true,
                AllowedScopes = allowedScopes,
                AllowAccessTokensViaBrowser = true,
                ApplicationType = ApplicationType.RegularWeb,
                AllowedSigningAlgorithm = seedClient.Algorithm,
                SupportedGrantTypes = supportedGrantTypes,
                SupportedResponseTypes = supportedResponseTypes,
                FrontChannelLogoutSessionRequired = true,
                FrontChannelLogoutUri = "https://localhost:5002/signout-oidc",
                BackChannelLogoutSessionRequired = true,
                BackChannelLogoutUri = "https://localhost:5002/backchannel-logout",
                CreatedBy = "Seed",
                ModifiedBy = "Seed",
                CreatedOn = utcNow,
                RedirectUris = CreateClientRedirectUris(clientId, utcNow),
                PostLogoutRedirectUris = CreateClientPostLogoutRedirectUris(clientId, utcNow)
            });
        }
    }

    private static List<ClientRedirectUris> CreateClientRedirectUris(Guid clientId, DateTime utcNow)
    {
        return new List<ClientRedirectUris>
        {
            new()
            {
                Id = Guid.NewGuid(), ClientId = clientId, RedirectUri = "http://127.0.0.1:63562/", CreatedBy = "Seed",
                ModifiedBy = "Seed", CreatedOn = utcNow
            },
            new()
            {
                Id = Guid.NewGuid(), ClientId = clientId, RedirectUri = "https://localhost:44300/index.html",
                CreatedBy = "Seed",
                ModifiedBy = "Seed", CreatedOn = utcNow
            },
            new()
            {
                Id = Guid.NewGuid(), ClientId = clientId, RedirectUri = "https://localhost:5002/callback.html",
                CreatedBy = "Seed",
                ModifiedBy = "Seed", CreatedOn = utcNow
            },
            new()
            {
                Id = Guid.NewGuid(), ClientId = clientId, RedirectUri = "https://localhost:5002/signin-oidc",
                CreatedBy = "Seed",
                ModifiedBy = "Seed", CreatedOn = utcNow
            }
        };
    }

    private static List<ClientPostLogoutRedirectUris> CreateClientPostLogoutRedirectUris(Guid clientId, DateTime utcNow)
    {
        return new List<ClientPostLogoutRedirectUris>
        {
            new()
            {
                Id = Guid.NewGuid(), ClientId = clientId,
                PostLogoutRedirectUri = "https://localhost:5002/callback.html", CreatedBy = "Seed",
                ModifiedBy = "Seed", CreatedOn = utcNow
            },
            new()
            {
                Id = Guid.NewGuid(), ClientId = clientId, PostLogoutRedirectUri = "https://localhost:5002/signout-oidc",
                CreatedBy = "Seed",
                ModifiedBy = "Seed", CreatedOn = utcNow
            },
            new()
            {
                Id = Guid.NewGuid(), ClientId = clientId,
                PostLogoutRedirectUri = "https://localhost:5002/signout-callback-oidc", CreatedBy = "Seed",
                ModifiedBy = "Seed", CreatedOn = utcNow
            },
            new()
            {
                Id = Guid.NewGuid(), ClientId = clientId, PostLogoutRedirectUri = BaseUrl + "/security/logout",
                CreatedBy = "Seed",
                ModifiedBy = "Seed", CreatedOn = utcNow
            }
        };
    }

    private static async Task SeedRolesAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManagerWrapper<Roles>>();
        await EnsureRoleAsync(
            roleManager,
            "GeneralUser",
            "General access role");
        await EnsureRoleAsync(
            roleManager,
            "SystemAdmin",
            "System administration role");
        await EnsureRoleAsync(
            roleManager,
            "ClientAdmin",
            "Client management role");
        await EnsureRoleAsync(
            roleManager,
            "RoleAdmin",
            "Role management role");
        await EnsureRoleAsync(
            roleManager,
            "ResourceAdmin",
            "Resource management role");
        await EnsureRoleAsync(roleManager, "Guest", "Guest role");
    }

    private static async Task SeedRoleClaimsAsync(IServiceProvider serviceProvider)
    {
        var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var utcNow = DateTime.UtcNow;

        var rolePermissions = new Dictionary<string, string[]>
        {
            ["GeneralUser"] = new[]
            {
                ApiPermissionConstants.UserManage
            },
            ["SystemAdmin"] = new[]
            {
                ApiPermissionConstants.AdminManage,
                ApiPermissionConstants.UserManage,
                ApiPermissionConstants.RoleManage,
                ApiPermissionConstants.ClientManage,
                ApiPermissionConstants.ApiResourceManage,
                ApiPermissionConstants.IdentityResourceManage,
                ApiPermissionConstants.SecurityTokenManage,
                ApiPermissionConstants.AuditManage
            },
            ["ClientAdmin"] = new[]
            {
                ApiPermissionConstants.ClientManage
            },
            ["RoleAdmin"] = new[]
            {
                ApiPermissionConstants.RoleManage
            },
            ["ResourceAdmin"] = new[]
            {
                ApiPermissionConstants.ApiResourceManage,
                ApiPermissionConstants.IdentityResourceManage
            }
        };

        foreach (var rolePermission in rolePermissions)
        {
            var role = await dbContext.Roles
                .IgnoreQueryFilters()
                .SingleOrDefaultAsync(x => x.Name == rolePermission.Key);
            if (role == null) continue;

            foreach (var permission in rolePermission.Value)
            {
                var isMapped = await dbContext.RoleClaims
                    .IgnoreQueryFilters()
                    .AnyAsync(x =>
                        x.RoleId == role.Id &&
                        x.ClaimType == OpenIdConstants.ClaimTypes.Permission &&
                        x.ClaimValue == permission &&
                        !x.IsDeleted);
                if (isMapped) continue;

                dbContext.RoleClaims.Add(new RoleClaims
                {
                    RoleId = role.Id,
                    ClaimType = OpenIdConstants.ClaimTypes.Permission,
                    ClaimValue = permission,
                    IsDeleted = false,
                    CreatedOn = utcNow,
                    CreatedBy = "Seed",
                    ModifiedBy = "Seed",
                    RowVersion = new byte[] { 1 }
                });
            }
        }
    }

    private static async Task EnsureRoleAsync(
        RoleManagerWrapper<Roles> roleManager,
        string roleName,
        string description)
    {
        var role = await roleManager.FindByNameAsync(roleName);
        if (role == null)
        {
            role = new Roles
            {
                Id = Guid.NewGuid(),
                Name = roleName,
                NormalizedName = roleName.ToUpperInvariant(),
                Description = description,
                ConcurrencyStamp = Guid.NewGuid().ToString("N"),
                CreatedBy = "Seed",
                ModifiedBy = "Seed",
                CreatedOn = DateTime.UtcNow,
                IsDeleted = false
            };

            var createResult = await roleManager.CreateAsync(role);
            EnsureIdentityResult(createResult, $"Role '{roleName}' creation failed");
        }
    }

    private async Task SeedUsersAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManagerWrapper<Users>>();

        await EnsureUserAsync(
            userManager,
            new Users
            {
                Id = Guid.NewGuid(),
                UserName = "checktest",
                Email = "checktest@HCL.CS.SF.local",
                PhoneNumber = "+12055550001",
                FirstName = "Check",
                LastName = "Test",
                DateOfBirth = new DateTime(1989, 5, 1),
                IdentityProviderType = IdentityProvider.Local,
                TwoFactorEnabled = false,
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                LockoutEnabled = true,
                RequiresDefaultPasswordChange = false,
                CreatedBy = "Seed",
                ModifiedBy = "Seed",
                CreatedOn = DateTime.UtcNow,
                IsDeleted = false,
                SecurityStamp = Guid.NewGuid().ToString("N"),
                ConcurrencyStamp = Guid.NewGuid().ToString("N")
            },
            "Test@123456789",
            Array.Empty<Claim>(),
            Array.Empty<string>());

        await EnsureUserAsync(
            userManager,
            new Users
            {
                Id = Guid.Parse(userMasterData["BobUser"].UserID),
                UserName = "BobUser",
                Email = "bobuser@HCL.CS.SF.local",
                PhoneNumber = "+12055550002",
                FirstName = "Bob",
                LastName = "User",
                DateOfBirth = new DateTime(1989, 5, 1),
                IdentityProviderType = IdentityProvider.Local,
                TwoFactorEnabled = false,
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                LockoutEnabled = true,
                RequiresDefaultPasswordChange = false,
                CreatedBy = "Seed",
                ModifiedBy = "Seed",
                CreatedOn = DateTime.UtcNow,
                IsDeleted = false,
                SecurityStamp = Guid.NewGuid().ToString("N"),
                ConcurrencyStamp = Guid.NewGuid().ToString("N")
            },
            "Test@123456789",
            Array.Empty<Claim>(),
            Array.Empty<string>());

        await EnsureUserAsync(
            userManager,
            new Users
            {
                Id = Guid.Parse(userMasterData["JacobIsmail"].UserID),
                UserName = "JacobIsmail",
                Email = "jacobismail@HCL.CS.SF.local",
                PhoneNumber = "+12055550003",
                FirstName = "Jacob",
                LastName = "Ismail",
                DateOfBirth = new DateTime(1989, 5, 1),
                IdentityProviderType = IdentityProvider.Local,
                TwoFactorEnabled = false,
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                LockoutEnabled = true,
                RequiresDefaultPasswordChange = false,
                CreatedBy = "Seed",
                ModifiedBy = "Seed",
                CreatedOn = DateTime.UtcNow,
                IsDeleted = false,
                SecurityStamp = Guid.NewGuid().ToString("N"),
                ConcurrencyStamp = Guid.NewGuid().ToString("N")
            },
            "Test@123456789",
            Array.Empty<Claim>(),
            Array.Empty<string>());

        await EnsureUserAsync(
            userManager,
            new Users
            {
                Id = Guid.Parse("763C134B-B796-41F5-B22E-08D9C46BAFAF"),
                UserName = "adminUser",
                Email = "adminuser@HCL.CS.SF.local",
                PhoneNumber = "+12055550004",
                FirstName = "Admin",
                LastName = "User",
                DateOfBirth = new DateTime(1985, 1, 1),
                IdentityProviderType = IdentityProvider.Local,
                TwoFactorEnabled = false,
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                LockoutEnabled = true,
                RequiresDefaultPasswordChange = false,
                CreatedBy = "Seed",
                ModifiedBy = "Seed",
                CreatedOn = DateTime.UtcNow,
                IsDeleted = false,
                SecurityStamp = Guid.NewGuid().ToString("N"),
                ConcurrencyStamp = Guid.NewGuid().ToString("N")
            },
            "Test@123",
            Array.Empty<Claim>(),
            Array.Empty<string>());
    }

    private static async Task SeedUserRolesAsync(IServiceProvider serviceProvider)
    {
        var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var utcNow = DateTime.UtcNow;

        var userRoleMappings = new Dictionary<string, string[]>
        {
            ["checktest"] = new[] { "SystemAdmin" },
            ["BobUser"] = new[] { "GeneralUser" },
            ["JacobIsmail"] = new[] { "GeneralUser" },
            ["adminUser"] = new[] { "SystemAdmin" }
        };

        foreach (var userRoleMapping in userRoleMappings)
        {
            var user = await dbContext.Users
                .IgnoreQueryFilters()
                .SingleOrDefaultAsync(x => x.UserName == userRoleMapping.Key);
            if (user == null) continue;

            foreach (var roleName in userRoleMapping.Value)
            {
                var role = await dbContext.Roles
                    .IgnoreQueryFilters()
                    .SingleOrDefaultAsync(x => x.Name == roleName);
                if (role == null) continue;

                var isMapped = await dbContext.UserRoles
                    .IgnoreQueryFilters()
                    .AnyAsync(x =>
                        x.UserId == user.Id &&
                        x.RoleId == role.Id &&
                        !x.IsDeleted);
                if (isMapped) continue;

                dbContext.UserRoles.Add(new UserRoles
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    RoleId = role.Id,
                    IsDeleted = false,
                    CreatedOn = utcNow,
                    CreatedBy = "Seed",
                    ModifiedBy = "Seed",
                    RowVersion = new byte[] { 1 }
                });
            }
        }
    }

    private static async Task EnsureUserAsync(
        UserManagerWrapper<Users> userManager,
        Users seedUser,
        string password,
        IEnumerable<Claim> claims,
        IEnumerable<string> roles)
    {
        var user = await userManager.FindByNameAsync(seedUser.UserName);
        if (user == null)
        {
            var createResult = await userManager.CreateAsync(seedUser, password);
            EnsureIdentityResult(createResult, $"User '{seedUser.UserName}' creation failed");
            user = seedUser;
        }

        if (!user.EmailConfirmed || !user.PhoneNumberConfirmed)
        {
            user.EmailConfirmed = true;
            user.PhoneNumberConfirmed = true;
            user.LockoutEnabled = true;
            var updateResult = await userManager.UpdateAsync(user);
            EnsureIdentityResult(updateResult, $"User '{user.UserName}' update failed");
        }

        var existingClaims = await userManager.GetClaimsAsync(user);
        foreach (var claim in claims)
        {
            if (existingClaims.Any(existingClaim =>
                    existingClaim.Type == claim.Type &&
                    existingClaim.Value == claim.Value))
                continue;

            var addClaimResult = await userManager.AddClaimAsync(user, claim);
            EnsureIdentityResult(addClaimResult,
                $"Adding claim '{claim.Type}:{claim.Value}' for '{user.UserName}' failed");
        }

        _ = roles;
    }

    private static void EnsureIdentityResult(IdentityResult result, string errorMessage)
    {
        if (result.Succeeded) return;

        var errors = string.Join(", ", result.Errors.Select(error => error.Description));
        throw new InvalidOperationException($"{errorMessage}. {errors}");
    }

    private static NotificationTemplateSettings LoadNotificationTemplateSettings(string integrationRootPath)
    {
        var notificationTemplatePath = Path.Combine(
            integrationRootPath,
            "Configurations",
            "NotificationTemplateSettings.json");

        if (!File.Exists(notificationTemplatePath)) return new NotificationTemplateSettings();

        var configuration = new ConfigurationBuilder()
            .AddJsonFile(notificationTemplatePath)
            .Build();

        var settings = new NotificationTemplateSettings();
        configuration.GetSection("NotificationTemplateSettings").Bind(settings);
        return settings;
    }

    private static TokenSettings CreateTokenSettings()
    {
        var tokenSettings = new TokenSettings();
        tokenSettings.TokenConfig.IssuerUri = "security.HCL.CS.SF.com";
        tokenSettings.TokenConfig.ApiIdentifier = TokenEndpoint;
        tokenSettings.TokenConfig.CachingLifetime = 3600;
        tokenSettings.TokenConfig.ShowKeySet = true;
        tokenSettings.TokenConfig.TokenExpiration = 60;
        tokenSettings.TokenConfig.ClientSecretLength = 60;
        tokenSettings.TokenConfig.ClientSecretExpirationInDays = 30;

        tokenSettings.InputLengthRestrictionsConfig.Nonce = 300;

        tokenSettings.UserInteractionConfig.LoginUrl = "/security/login";
        tokenSettings.UserInteractionConfig.LogoutUrl = "/security/logout";
        tokenSettings.UserInteractionConfig.ErrorUrl = "/home/error";

        tokenSettings.EndpointsConfig.FrontchannelLogoutSupported = true;
        tokenSettings.EndpointsConfig.FrontchannelLogoutSessionRequired = false;
        tokenSettings.EndpointsConfig.BackchannelLogoutSupported = true;
        tokenSettings.EndpointsConfig.BackchannelLogoutSessionRequired = true;

        return tokenSettings;
    }

    private static SystemSettings CreateSystemSettings(string databasePath)
    {
        var directoryPath = Path.GetDirectoryName(databasePath);
        if (!string.IsNullOrWhiteSpace(directoryPath)) Directory.CreateDirectory(directoryPath);

        var logDirectoryPath = Path.Combine(IntegrationRuntimeDirectory, "logs");
        Directory.CreateDirectory(logDirectoryPath);

        var systemSettings = new SystemSettings();
        systemSettings.DBConfig.Database = DbTypes.SQLite;
        systemSettings.DBConfig.DBConnectionString = BuildSqliteConnectionString(databasePath);

        systemSettings.UserConfig.MinUserNameLength = 8;
        systemSettings.UserConfig.MaxUserNameLength = 255;
        systemSettings.UserConfig.MinFirstAndLastNameLength = 2;
        systemSettings.UserConfig.MaxFirstAndLastNameLength = 255;
        systemSettings.UserConfig.MinPhoneNumberLength = 4;
        systemSettings.UserConfig.MaxPhoneNumberLength = 17;
        systemSettings.UserConfig.MinNoOfQuestions = 1;
        systemSettings.UserConfig.MinSecurityAnswersLength = 3;
        systemSettings.UserConfig.MinDOBYear = 18;
        systemSettings.UserConfig.MaxDOBYear = 100;
        systemSettings.UserConfig.AccessFailedCount = 3;
        systemSettings.UserConfig.RequiredRecoveryCodes = 2;
        systemSettings.UserConfig.RequireUniqueEmail = true;
        systemSettings.UserConfig.RequireConfirmedEmail = true;
        systemSettings.UserConfig.RequireConfirmedPhoneNumber = true;
        systemSettings.UserConfig.LockOutAllowedForNewUsers = true;
        systemSettings.UserConfig.DefaultLockoutTimeSpanMin = 10;
        systemSettings.UserConfig.MaxFailedAccessAttempts = 3;
        systemSettings.UserConfig.EmailTokenExpiry = 5;
        systemSettings.UserConfig.OTPTokenExpiry = 5;
        systemSettings.UserConfig.PasswordResetTokenExpiry = 5;
        systemSettings.UserConfig.UserTokenExpiry = 5;
        systemSettings.UserConfig.LockAccountPeriod = 30;

        systemSettings.PasswordConfig.MinPasswordLength = 8;
        systemSettings.PasswordConfig.MaxPasswordLength = 40;
        systemSettings.PasswordConfig.RequiredUniqueChars = 1;
        systemSettings.PasswordConfig.RequireDigit = true;
        systemSettings.PasswordConfig.RequireLowercase = true;
        systemSettings.PasswordConfig.RequireUppercase = true;
        systemSettings.PasswordConfig.RequireSpecialChar = true;
        systemSettings.PasswordConfig.MaxLimitPasswordReuse = 3;
        systemSettings.PasswordConfig.MaxPasswordExpiry = 30;
        systemSettings.PasswordConfig.PasswordNotificationBeforeExpiry = 3;

        systemSettings.CryptoConfig.RandomStringLength = 32;

        systemSettings.LogConfig.WriteLogTo = WriteLogTo.File;
        systemSettings.LogConfig.LogFileConfig.FilePath = Path.Combine(logDirectoryPath, "HCL.CS.SF-integration.log");
        systemSettings.LogConfig.LogFileConfig.RestrictedToMinimumLevel = Log.Debug;
        systemSettings.LogConfig.LogFileConfig.MinimumConfiguration = Log.Debug;
        systemSettings.LogConfig.LogFileConfig.SetLogFileSize = true;
        systemSettings.LogConfig.LogFileConfig.FileSizeInBytes = 5242880;

        return systemSettings;
    }

    private static string BuildSqliteConnectionString(string databasePath)
    {
        return $"Data Source={databasePath}";
    }

    private static void AddCookieAuthentication(IServiceCollection services)
    {
        services.AddAuthentication(config =>
            {
                config.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                config.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.Cookie.SameSite = SameSiteMode.None;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.IsEssential = true;
            });
    }

    public void ConfigureApp(IApplicationBuilder app)
    {
        app.UseHCLCSSFEndpoint();
        app.UseHCLCSSFApi();

        // UI endpoints
        //app.Map(LoginUrl, path =>
        //{
        //    path.Run(ctx => OnLogin(ctx));
        //});

        app.Map(LogoutUrl, path => { path.Run(ctx => OnLogout(ctx)); });

        app.Map(ErrorUrl, path => { path.Run(ctx => OnError()); });
    }

    //private async Task OnLogin(HttpContext context)
    //{
    //    LoginPageCalled = true;
    //    //if (User != null)
    //    //{
    //    //    var data = new
    //    //    {
    //    //        user_name = User.UserName,
    //    //        password = Password,
    //    //    };

    //    //    var url = BaseUrl + ApiRoutePathConstants.PasswordSignIn;
    //    //    var response = await FrontChannelClient.PostAsync(
    //    //        url,
    //    //        new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json"));
    //    //    var content = await response.Content.ReadAsStringAsync();
    //    //    var model = JsonConvert.DeserializeObject<SignInResponseModel>(content);
    //    //    if (!model.Succeeded)
    //    //    {
    //    //        _ = new Exception(model.Message);
    //    //    }

    //    //    //await context.SignInAsync(IdentityConstants.ApplicationScheme, Subject, props);
    //    //    //context.User = Subject;
    //    //}
    //}

    private Task OnError()
    {
        //ErrorCalled = true;
        return Task.CompletedTask;
    }

    private async Task OnLogout(HttpContext context)
    {
        LogoutPageCalled = true;
        var logoutId = context.Request.Query["logoutId"].ToString();
        //LogoutRequest = await logoutId.ParseLogoutQueryMessageAsync();
        var dict = new Dictionary<string, string>
        {
            { "endSessionId", logoutId }
        };
        context.Response.Redirect(EndSessionCallbackEndpoint.AddQueryString(dict));
        await context.SignOutAsync(IdentityConstants.ApplicationScheme, props);
    }

    private List<AsymmetricKeyInfoModel> LoadAsymmetricKey()
    {
        lock (AsymmetricKeyLock)
        {
            if (cachedAsymmetricKeys != null)
            {
                AsymmetricKeys = cachedAsymmetricKeys;
                return cachedAsymmetricKeys;
            }

            var keyInfos = new List<AsymmetricKeyInfoModel>
            {
                new()
                {
                    Certificate =
                        CreateSelfSignedEcdsaCertificate(ECCurve.NamedCurves.nistP256, HashAlgorithmName.SHA256),
                    Algorithm = SigningAlgorithm.ES256, KeyId = GenerateRandomSalt(16)
                },
                new()
                {
                    Certificate =
                        CreateSelfSignedEcdsaCertificate(ECCurve.NamedCurves.nistP384, HashAlgorithmName.SHA384),
                    Algorithm = SigningAlgorithm.ES384, KeyId = GenerateRandomSalt(16)
                },
                new()
                {
                    Certificate =
                        CreateSelfSignedEcdsaCertificate(ECCurve.NamedCurves.nistP521, HashAlgorithmName.SHA512),
                    Algorithm = SigningAlgorithm.ES512, KeyId = GenerateRandomSalt(16)
                },
                new()
                {
                    Certificate = CreateSelfSignedRsaCertificate(HashAlgorithmName.SHA256),
                    Algorithm = SigningAlgorithm.RS256, KeyId = GenerateRandomSalt(16)
                },
                new()
                {
                    Certificate = CreateSelfSignedRsaCertificate(HashAlgorithmName.SHA384),
                    Algorithm = SigningAlgorithm.RS384, KeyId = GenerateRandomSalt(16)
                },
                new()
                {
                    Certificate = CreateSelfSignedRsaCertificate(HashAlgorithmName.SHA512),
                    Algorithm = SigningAlgorithm.RS512, KeyId = GenerateRandomSalt(16)
                },
                new()
                {
                    Certificate = CreateSelfSignedRsaCertificate(HashAlgorithmName.SHA256),
                    Algorithm = SigningAlgorithm.PS256, KeyId = GenerateRandomSalt(16)
                },
                new()
                {
                    Certificate = CreateSelfSignedRsaCertificate(HashAlgorithmName.SHA384),
                    Algorithm = SigningAlgorithm.PS384, KeyId = GenerateRandomSalt(16)
                },
                new()
                {
                    Certificate = CreateSelfSignedRsaCertificate(HashAlgorithmName.SHA512),
                    Algorithm = SigningAlgorithm.PS512, KeyId = GenerateRandomSalt(16)
                }
            };

            cachedAsymmetricKeys = keyInfos;
            AsymmetricKeys = keyInfos;
            return keyInfos;
        }
    }

    private static X509Certificate2 CreateSelfSignedRsaCertificate(HashAlgorithmName hashAlgorithm)
    {
        using var rsa = RSA.Create(3072);
        var request = new CertificateRequest(
            $"CN=HCLCSSFIntegrationRSA-{Guid.NewGuid():N}",
            rsa,
            hashAlgorithm,
            RSASignaturePadding.Pkcs1);

        request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));
        request.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(request.PublicKey, false));

        var certificate = request.CreateSelfSigned(
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddYears(10));

        return new X509Certificate2(
            certificate.Export(X509ContentType.Pfx),
            string.Empty,
            X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);
    }

    private static X509Certificate2 CreateSelfSignedEcdsaCertificate(ECCurve curve, HashAlgorithmName hashAlgorithm)
    {
        using var ecdsa = ECDsa.Create(curve);
        var request = new CertificateRequest(
            $"CN=HCLCSSFIntegrationECDSA-{Guid.NewGuid():N}",
            ecdsa,
            hashAlgorithm);

        request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));
        request.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(request.PublicKey, false));

        var certificate = request.CreateSelfSigned(
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddYears(10));

        return new X509Certificate2(
            certificate.Export(X509ContentType.Pfx),
            string.Empty,
            X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);
    }

    private static string GetIntegrationTestsRootPath()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, "Configurations")) &&
                Directory.Exists(Path.Combine(directory.FullName, "Certificates")))
                return directory.FullName;

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Unable to resolve IntegrationTests root path.");
    }

    public static string GenerateRandomSalt(int size)
    {
        var rng = RandomNumberGenerator.Create();
        var bytes = new byte[size];
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    public void Initialize(string basePath = null)
    {
        if (basePath != null)
            IssueUrl = BaseUrl + basePath;
        else
            IssueUrl = BaseUrl;

        var builder = new WebHostBuilder();
        builder.ConfigureServices(ConfigureServices);
        builder.Configure(app => { ConfigureApp(app); });
        var server = new TestServer(builder);
        var handler = server.CreateHandler();

        FrontChannelClient = new UserAgent(new UserAgentHandler(handler));
        BackChannelClient = new HttpClient(handler);
    }

    public async Task LoginAsync(UserModel user)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));

        var data = new
        {
            user_name = user.UserName,
            password = user.Password
        };

        var url = BaseUrl + ApiRoutePathConstants.PasswordSignIn;
        var response = await FrontChannelClient.PostAsync(
            url,
            new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json"));
        var content = await response.Content.ReadAsStringAsync();
        var model = JsonConvert.DeserializeObject<SignInResponseModel>(content);
        if (!model.Succeeded)
            throw new InvalidOperationException($"Test login failed for '{user.UserName}': {model?.Message}");

        // this.Subject = new FrameworkUser(user.Id.ToString(), user.UserName).CreatePrincipal();
        //GlobalConfiguration.UserName = user.UserName.ToString();
        //GlobalConfiguration.UserId = user.Id.ToString();

        //var oldSetting = FrontChannelClient.AllowAutoRedirect;
        //FrontChannelClient.AllowAutoRedirect = false;
        //await FrontChannelClient.GetAsync(LoginPage);
        //FrontChannelClient.AllowAutoRedirect = oldSetting;
    }

    //public async Task LoginAsync(ClaimsPrincipal subject)
    //{
    //    var oldSetting = FrontChannelClient.AllowAutoRedirect;
    //    FrontChannelClient.AllowAutoRedirect = false;
    //    Subject = subject;
    //    await FrontChannelClient.GetAsync(LoginPage);
    //    FrontChannelClient.AllowAutoRedirect = oldSetting;
    //}

    public string CreateAuthorizeRequestUrl(
        string clientId = null,
        string responseType = null,
        string scope = null,
        string redirectUri = null,
        string state = null,
        string nonce = null,
        string prompt = null,
        string maxAge = null,
        string loginHint = null,
        string acrValues = null,
        string responseMode = null,
        string codeChallenge = null,
        string codeChallengeMethod = null)
    {
        var dict = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(clientId)) dict.Add(OpenIdConstants.AuthorizeRequest.ClientId, clientId);

        if (!string.IsNullOrEmpty(responseType)) dict.Add(OpenIdConstants.AuthorizeRequest.ResponseType, responseType);

        if (!string.IsNullOrEmpty(scope)) dict.Add(OpenIdConstants.AuthorizeRequest.Scope, scope);

        if (!string.IsNullOrEmpty(redirectUri)) dict.Add(OpenIdConstants.AuthorizeRequest.RedirectUri, redirectUri);

        if (!string.IsNullOrEmpty(state)) dict.Add(OpenIdConstants.AuthorizeRequest.State, state);

        if (!string.IsNullOrEmpty(nonce)) dict.Add(OpenIdConstants.AuthorizeRequest.Nonce, nonce);

        if (!string.IsNullOrEmpty(nonce)) dict.Add(OpenIdConstants.AuthorizeRequest.Prompt, prompt);

        if (!string.IsNullOrEmpty(nonce)) dict.Add(OpenIdConstants.AuthorizeRequest.MaxAge, maxAge);

        if (!string.IsNullOrEmpty(loginHint)) dict.Add("login_hint", loginHint);

        if (!string.IsNullOrEmpty(acrValues)) dict.Add("acr_values", acrValues);

        if (!string.IsNullOrEmpty(responseMode)) dict.Add(OpenIdConstants.AuthorizeRequest.ResponseMode, responseMode);

        if (!string.IsNullOrEmpty(codeChallenge))
            dict.Add(OpenIdConstants.AuthorizeRequest.CodeChallenge, codeChallenge);

        if (!string.IsNullOrEmpty(codeChallengeMethod))
            dict.Add(OpenIdConstants.AuthorizeRequest.CodeChallengeMethod, codeChallengeMethod);

        var url = AuthorizeEndpoint.AddQueryString(dict);
        return url;
    }

    public Dictionary<string, string> CreateTokenRequest(
        string clientId = null,
        string clientSecret = null,
        string code = null,
        string redirectUri = null,
        string grantType = null,
        string codeVerifier = null,
        string userName = null,
        string password = null,
        string scope = null,
        string refreshToken = null,
        string clientAssertionType = null,
        string clientAssertion = null)
    {
        var dict = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(clientId)) dict.Add(OpenIdConstants.TokenRequest.ClientId, clientId);

        if (!string.IsNullOrEmpty(clientSecret)) dict.Add(OpenIdConstants.TokenRequest.ClientSecret, clientSecret);

        if (!string.IsNullOrEmpty(code)) dict.Add(OpenIdConstants.TokenRequest.Code, code);

        if (!string.IsNullOrEmpty(redirectUri)) dict.Add(OpenIdConstants.TokenRequest.RedirectUri, redirectUri);

        if (!string.IsNullOrEmpty(grantType)) dict.Add(OpenIdConstants.TokenRequest.GrantType, grantType);

        if (!string.IsNullOrEmpty(codeVerifier)) dict.Add(OpenIdConstants.TokenRequest.CodeVerifier, codeVerifier);

        if (!string.IsNullOrEmpty(userName)) dict.Add(OpenIdConstants.TokenRequest.UserName, userName);

        if (!string.IsNullOrEmpty(password)) dict.Add(OpenIdConstants.TokenRequest.Password, password);

        if (!string.IsNullOrEmpty(scope)) dict.Add(OpenIdConstants.TokenRequest.Scope, scope);

        if (!string.IsNullOrEmpty(refreshToken)) dict.Add(OpenIdConstants.TokenRequest.RefreshToken, refreshToken);

        if (!string.IsNullOrEmpty(clientAssertionType))
            dict.Add(OpenIdConstants.TokenRequest.ClientAssertionType, clientAssertionType);

        if (!string.IsNullOrEmpty(clientAssertion))
            dict.Add(OpenIdConstants.TokenRequest.ClientAssertion, clientAssertion);

        dict.Add("Content-Type", "application/x-www-form-urlencoded");
        return dict;
    }

    public Dictionary<string, string> CreateRevocationRequest(
        string clientId = null,
        string clientSecret = null,
        string token = null,
        string tokentypehint = null)
    {
        var dict = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(clientId)) dict.Add(OpenIdConstants.TokenRequest.ClientId, clientId);

        if (!string.IsNullOrEmpty(clientSecret)) dict.Add(OpenIdConstants.TokenRequest.ClientSecret, clientSecret);

        if (!string.IsNullOrEmpty(token)) dict.Add(OpenIdConstants.RevocationRequest.Token, token);

        if (!string.IsNullOrEmpty(tokentypehint))
            dict.Add(OpenIdConstants.RevocationRequest.TokenTypeHint, tokentypehint);

        dict.Add("Content-Type", "application/x-www-form-urlencoded");
        return dict;
    }

    public Dictionary<string, string> CreateEndSessionRequest(
        string id_token_hint = null,
        string post_logout_redirect_uri = null,
        string state = null
    )
    {
        var dict = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(id_token_hint))
            dict.Add(OpenIdConstants.EndSessionRequest.IdTokenHint, id_token_hint);

        if (!string.IsNullOrEmpty(post_logout_redirect_uri))
            dict.Add(OpenIdConstants.EndSessionRequest.PostLogoutRedirectUri, post_logout_redirect_uri);

        if (!string.IsNullOrEmpty(state)) dict.Add(OpenIdConstants.EndSessionRequest.State, state);

        dict.Add("Content-Type", "application/x-www-form-urlencoded");
        return dict;
    }

    public Dictionary<string, string> CreateIntroSpecRequest(
        string clientId = null,
        string clientSecret = null,
        string token = null,
        string tokenTypeHint = null,
        string scope = null
    )
    {
        var dict = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(clientId)) dict.Add(OpenIdConstants.TokenRequest.ClientId, clientId);

        if (!string.IsNullOrEmpty(clientSecret)) dict.Add(OpenIdConstants.TokenRequest.ClientSecret, clientSecret);

        if (!string.IsNullOrEmpty(token)) dict.Add(OpenIdConstants.IntrospectionRequest.Token, token);

        if (!string.IsNullOrEmpty(tokenTypeHint))
            dict.Add(OpenIdConstants.IntrospectionRequest.TokenHintType, tokenTypeHint);

        if (!string.IsNullOrEmpty(scope)) dict.Add(OpenIdConstants.IntrospectionRequest.Scope, scope);

        dict.Add("Content-Type", "application/x-www-form-urlencoded");
        return dict;
    }

    public string CreateTokenRequestUrl(string clientId = null, string clientSecret = null, string code = null,
        string redirectUri = null)
    {
        var dict = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(clientId)) dict.Add(OpenIdConstants.TokenRequest.ClientId, clientId);

        if (!string.IsNullOrEmpty(clientSecret)) dict.Add(OpenIdConstants.TokenRequest.ClientSecret, clientSecret);

        if (!string.IsNullOrEmpty(code)) dict.Add(OpenIdConstants.TokenRequest.Code, code);

        if (!string.IsNullOrEmpty(redirectUri)) dict.Add(OpenIdConstants.TokenRequest.RedirectUri, redirectUri);

        var url = TokenEndpoint.AddQueryString(dict);
        return url;
    }

    public Task<ClientsModel> FetchClientDetails(string clientName)
    {
        ClientsModel clientModel = null;
        var client = clientMasterData;

        foreach (var item in client)
            if (item.Key.Contains(clientName))
            {
                var clientsModel = new ClientsModel();
                var clientId = item.Value.ClientId;
                var clientSecret = item.Value.ClientSecret;
                clientsModel.ClientId = clientId;
                clientsModel.ClientSecret = clientSecret;
                clientModel = clientsModel;
            }

        return Task.FromResult(clientModel);
    }

    public async Task<TokenResponseResultModel> GetAccessToken()
    {
        TokenResponseResultModel tokenResponseResultModel = null;
        // Login User.
        await LoginAsync(User);
        positiveCaseClientName = hCLCSS256ClientName;
        var clientModel = await FetchClientDetails(positiveCaseClientName);
        if (clientModel != null)
        {
            // Authorize endpoint calls.
            var nonce = Guid.NewGuid().ToString();
            var codeVerifier = 32.RandomString();
            var codeChallengeString = codeVerifier.GenerateCodeChallenge();
            FrontChannelClient.AllowAutoRedirect = false;
            var url = CreateAuthorizeRequestUrl(
                clientModel.ClientId,
                "code",
                "openid email profile offline_access phone HCL.CS.SF.apiresource HCL.CS.SF.client HCL.CS.SF.user HCL.CS.SF.role HCL.CS.SF.identityresource HCL.CS.SF.adminuser HCL.CS.SF.securitytoken",
                responseMode: "query",
                prompt: "none",
                codeChallenge: codeChallengeString, // Codeverifier
                codeChallengeMethod: "S256", // Plain
                maxAge: "60",
                redirectUri: "http://127.0.0.1:63562/",
                nonce: nonce);
            var returnQuery = await FrontChannelClient.GetAsync(url);

            // Token endpoint calls.
            var response = returnQuery.Headers.Location.ToString().ParseQueryString();
            //var response1 = returnQuery.Headers.Location.ToString().ParseErrorQueryStringAsync();
            var dict = CreateTokenRequest(
                clientModel.ClientId,
                clientModel.ClientSecret,
                response.Code,
                "http://127.0.0.1:63562/",
                OpenIdConstants.GrantTypes.AuthorizationCode,
                codeVerifier); // Code Challenge
            var tokenClient = BackChannelClient;
            var tokenResponse = await tokenClient.PostAsync(TokenEndpoint, new FormUrlEncodedContent(dict));
            var tokenResponseContent = await tokenResponse.Content.ReadAsStringAsync();
            if (tokenResponse.StatusCode != HttpStatusCode.OK)
                throw new InvalidOperationException(
                    $"Token endpoint failure ({(int)tokenResponse.StatusCode}): {tokenResponseContent}");

            tokenResponseResultModel = JsonConvert.DeserializeObject<TokenResponseResultModel>(tokenResponseContent);
            tokenResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            tokenResponseResultModel.access_token.Should().NotBeNullOrEmpty();
            tokenResponseResultModel.id_token.Should().NotBeNullOrEmpty();
            tokenResponseResultModel.token_type.Should().NotBeNullOrEmpty();
            tokenResponseResultModel.refresh_token.Should().NotBeNullOrEmpty();
        }

        return tokenResponseResultModel;
    }

    private sealed class ClientSeedDefinition
    {
        public ClientSeedDefinition(
            string clientName,
            string algorithm,
            bool isPkceTextPlain,
            bool requireClientSecret,
            int accessTokenExpiration,
            int identityTokenExpiration,
            int refreshTokenExpiration,
            int authorizationCodeExpiration,
            int clientSecretExpirationInDays)
        {
            ClientName = clientName;
            Algorithm = algorithm;
            IsPkceTextPlain = isPkceTextPlain;
            RequireClientSecret = requireClientSecret;
            AccessTokenExpiration = accessTokenExpiration;
            IdentityTokenExpiration = identityTokenExpiration;
            RefreshTokenExpiration = refreshTokenExpiration;
            AuthorizationCodeExpiration = authorizationCodeExpiration;
            ClientSecretExpirationInDays = clientSecretExpirationInDays;
        }

        public string ClientName { get; }

        public string Algorithm { get; }

        public bool IsPkceTextPlain { get; }

        public bool RequireClientSecret { get; }

        public int AccessTokenExpiration { get; }

        public int IdentityTokenExpiration { get; }

        public int RefreshTokenExpiration { get; }

        public int AuthorizationCodeExpiration { get; }

        public int ClientSecretExpirationInDays { get; }
    }
}
