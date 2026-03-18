/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HCLCSSFInstallerMVC.Infrastructure.Persistence.Migrations.Sqlite
{
    public partial class HCLCSSFSqliteV1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HCL.CS.SF_ApiResources",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    ModifiedOn = table.Column<DateTime>(nullable: true),
                    CreatedBy = table.Column<string>(maxLength: 255, nullable: false),
                    ModifiedBy = table.Column<string>(maxLength: 255, nullable: true),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true),
                    Name = table.Column<string>(maxLength: 255, nullable: false),
                    DisplayName = table.Column<string>(maxLength: 255, nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Enabled = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HCL.CS.SF_ApiResources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HCL.CS.SF_AuditTrail",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 255, nullable: false),
                    ActionType = table.Column<int>(maxLength: 50, nullable: false),
                    TableName = table.Column<string>(maxLength: 255, nullable: true),
                    OldValue = table.Column<string>(nullable: true),
                    NewValue = table.Column<string>(nullable: true),
                    AffectedColumn = table.Column<string>(nullable: true),
                    ActionName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HCL.CS.SF_AuditTrail", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HCL.CS.SF_Clients",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    ModifiedOn = table.Column<DateTime>(nullable: true),
                    CreatedBy = table.Column<string>(maxLength: 255, nullable: false),
                    ModifiedBy = table.Column<string>(maxLength: 255, nullable: true),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true),
                    ClientId = table.Column<string>(maxLength: 128, nullable: false),
                    ClientName = table.Column<string>(maxLength: 255, nullable: true),
                    ClientUri = table.Column<string>(nullable: true),
                    ClientIdIssuedAt = table.Column<long>(nullable: false),
                    ClientSecretExpiresAt = table.Column<long>(nullable: false),
                    ClientSecret = table.Column<string>(maxLength: 128, nullable: true),
                    LogoUri = table.Column<string>(nullable: true),
                    TermsOfServiceUri = table.Column<string>(nullable: true),
                    PolicyUri = table.Column<string>(nullable: true),
                    RefreshTokenExpiration = table.Column<int>(nullable: false),
                    AccessTokenExpiration = table.Column<int>(nullable: false),
                    IdentityTokenExpiration = table.Column<int>(nullable: false),
                    LogoutTokenExpiration = table.Column<int>(nullable: false),
                    AuthorizationCodeExpiration = table.Column<int>(nullable: false),
                    AccessTokenType = table.Column<int>(nullable: false),
                    RequirePkce = table.Column<bool>(nullable: false),
                    IsPkceTextPlain = table.Column<bool>(nullable: false),
                    RequireClientSecret = table.Column<bool>(nullable: false),
                    IsFirstPartyApp = table.Column<bool>(nullable: false),
                    AllowOfflineAccess = table.Column<bool>(nullable: false),
                    AllowedScopes = table.Column<string>(nullable: true),
                    AllowAccessTokensViaBrowser = table.Column<bool>(nullable: false),
                    ApplicationType = table.Column<int>(nullable: false),
                    AllowedSigningAlgorithm = table.Column<string>(nullable: true),
                    SupportedGrantTypes = table.Column<string>(nullable: true),
                    SupportedResponseTypes = table.Column<string>(nullable: true),
                    FrontChannelLogoutSessionRequired = table.Column<bool>(nullable: false),
                    FrontChannelLogoutUri = table.Column<string>(nullable: true),
                    BackChannelLogoutSessionRequired = table.Column<bool>(nullable: false),
                    BackChannelLogoutUri = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HCL.CS.SF_Clients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HCL.CS.SF_IdentityResources",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    ModifiedOn = table.Column<DateTime>(nullable: true),
                    CreatedBy = table.Column<string>(maxLength: 255, nullable: false),
                    ModifiedBy = table.Column<string>(maxLength: 255, nullable: true),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true),
                    Name = table.Column<string>(maxLength: 255, nullable: false),
                    DisplayName = table.Column<string>(maxLength: 255, nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Enabled = table.Column<bool>(nullable: false),
                    Required = table.Column<bool>(nullable: false),
                    Emphasize = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HCL.CS.SF_IdentityResources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HCL.CS.SF_Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(maxLength: 255, nullable: false),
                    NormalizedName = table.Column<string>(maxLength: 255, nullable: false),
                    ConcurrencyStamp = table.Column<string>(maxLength: 255, nullable: false),
                    Description = table.Column<string>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    ModifiedOn = table.Column<DateTime>(nullable: true),
                    CreatedBy = table.Column<string>(maxLength: 255, nullable: false),
                    ModifiedBy = table.Column<string>(maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HCL.CS.SF_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HCL.CS.SF_SecurityQuestions",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    ModifiedOn = table.Column<DateTime>(nullable: true),
                    CreatedBy = table.Column<string>(maxLength: 255, nullable: false),
                    ModifiedBy = table.Column<string>(maxLength: 255, nullable: true),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true),
                    Question = table.Column<string>(maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HCL.CS.SF_SecurityQuestions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HCL.CS.SF_SecurityTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    ModifiedOn = table.Column<DateTime>(nullable: true),
                    CreatedBy = table.Column<string>(maxLength: 255, nullable: false),
                    ModifiedBy = table.Column<string>(maxLength: 255, nullable: true),
                    Key = table.Column<string>(nullable: true),
                    TokenType = table.Column<string>(nullable: true),
                    TokenValue = table.Column<string>(nullable: true),
                    ClientId = table.Column<string>(nullable: true),
                    SessionId = table.Column<string>(nullable: true),
                    SubjectId = table.Column<string>(nullable: true),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    ExpiresAt = table.Column<int>(nullable: false),
                    ConsumedTime = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HCL.CS.SF_SecurityTokens", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HCL.CS.SF_Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    UserName = table.Column<string>(maxLength: 255, nullable: false),
                    NormalizedUserName = table.Column<string>(maxLength: 255, nullable: false),
                    Email = table.Column<string>(maxLength: 255, nullable: false),
                    NormalizedEmail = table.Column<string>(maxLength: 255, nullable: false),
                    EmailConfirmed = table.Column<bool>(nullable: false),
                    PasswordHash = table.Column<string>(nullable: false),
                    SecurityStamp = table.Column<string>(maxLength: 255, nullable: true),
                    ConcurrencyStamp = table.Column<string>(maxLength: 255, nullable: true),
                    PhoneNumber = table.Column<string>(maxLength: 15, nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(nullable: false),
                    TwoFactorEnabled = table.Column<bool>(nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(nullable: true),
                    LockoutEnabled = table.Column<bool>(nullable: false),
                    AccessFailedCount = table.Column<int>(nullable: false),
                    FirstName = table.Column<string>(maxLength: 255, nullable: false),
                    LastName = table.Column<string>(maxLength: 255, nullable: true),
                    DateOfBirth = table.Column<DateTime>(nullable: true),
                    TwoFactorType = table.Column<int>(nullable: false),
                    LastPasswordChangedDate = table.Column<DateTime>(nullable: true),
                    RequiresDefaultPasswordChange = table.Column<bool>(nullable: true),
                    LastLoginDateTime = table.Column<DateTime>(nullable: true),
                    LastLogoutDateTime = table.Column<DateTime>(nullable: true),
                    IdentityProviderType = table.Column<int>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    ModifiedOn = table.Column<DateTime>(nullable: true),
                    CreatedBy = table.Column<string>(maxLength: 255, nullable: false),
                    ModifiedBy = table.Column<string>(maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HCL.CS.SF_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HCL.CS.SF_ApiResourceClaims",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    ModifiedOn = table.Column<DateTime>(nullable: true),
                    CreatedBy = table.Column<string>(maxLength: 255, nullable: false),
                    ModifiedBy = table.Column<string>(maxLength: 255, nullable: true),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true),
                    ApiResourceId = table.Column<Guid>(nullable: false),
                    Type = table.Column<string>(maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HCL.CS.SF_ApiResourceClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HCL.CS.SF_ApiResourceClaims_HCL.CS.SF_ApiResources_ApiResourceId",
                        column: x => x.ApiResourceId,
                        principalTable: "HCL.CS.SF_ApiResources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HCL.CS.SF_ApiScopes",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    ModifiedOn = table.Column<DateTime>(nullable: true),
                    CreatedBy = table.Column<string>(maxLength: 255, nullable: false),
                    ModifiedBy = table.Column<string>(maxLength: 255, nullable: true),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true),
                    ApiResourceId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(maxLength: 255, nullable: false),
                    DisplayName = table.Column<string>(maxLength: 255, nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Required = table.Column<bool>(nullable: false),
                    Emphasize = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HCL.CS.SF_ApiScopes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HCL.CS.SF_ApiScopes_HCL.CS.SF_ApiResources_ApiResourceId",
                        column: x => x.ApiResourceId,
                        principalTable: "HCL.CS.SF_ApiResources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HCL.CS.SF_ClientPostLogoutRedirectUris",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    ModifiedOn = table.Column<DateTime>(nullable: true),
                    CreatedBy = table.Column<string>(maxLength: 255, nullable: false),
                    ModifiedBy = table.Column<string>(maxLength: 255, nullable: true),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true),
                    ClientId = table.Column<Guid>(nullable: false),
                    PostLogoutRedirectUri = table.Column<string>(maxLength: 510, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HCL.CS.SF_ClientPostLogoutRedirectUris", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HCL.CS.SF_ClientPostLogoutRedirectUris_HCL.CS.SF_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "HCL.CS.SF_Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HCL.CS.SF_ClientRedirectUris",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    ModifiedOn = table.Column<DateTime>(nullable: true),
                    CreatedBy = table.Column<string>(maxLength: 255, nullable: false),
                    ModifiedBy = table.Column<string>(maxLength: 255, nullable: true),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true),
                    ClientId = table.Column<Guid>(nullable: false),
                    RedirectUri = table.Column<string>(maxLength: 510, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HCL.CS.SF_ClientRedirectUris", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HCL.CS.SF_ClientRedirectUris_HCL.CS.SF_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "HCL.CS.SF_Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HCL.CS.SF_IdentityClaims",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    ModifiedOn = table.Column<DateTime>(nullable: true),
                    CreatedBy = table.Column<string>(maxLength: 255, nullable: false),
                    ModifiedBy = table.Column<string>(maxLength: 255, nullable: true),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true),
                    IdentityResourceId = table.Column<Guid>(nullable: false),
                    Type = table.Column<string>(maxLength: 255, nullable: false),
                    AliasType = table.Column<string>(maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HCL.CS.SF_IdentityClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HCL.CS.SF_IdentityClaims_HCL.CS.SF_IdentityResources_IdentityResourceId",
                        column: x => x.IdentityResourceId,
                        principalTable: "HCL.CS.SF_IdentityResources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HCL.CS.SF_RoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoleId = table.Column<Guid>(nullable: false),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    ModifiedOn = table.Column<DateTime>(nullable: true),
                    CreatedBy = table.Column<string>(maxLength: 255, nullable: false),
                    ModifiedBy = table.Column<string>(maxLength: 255, nullable: true),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HCL.CS.SF_RoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HCL.CS.SF_RoleClaims_HCL.CS.SF_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "HCL.CS.SF_Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HCL.CS.SF_Notification",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    ModifiedOn = table.Column<DateTime>(nullable: true),
                    CreatedBy = table.Column<string>(maxLength: 255, nullable: false),
                    ModifiedBy = table.Column<string>(maxLength: 255, nullable: true),
                    UserId = table.Column<Guid>(nullable: false),
                    MessageId = table.Column<string>(maxLength: 255, nullable: false),
                    Type = table.Column<int>(nullable: false),
                    Activity = table.Column<string>(maxLength: 255, nullable: true),
                    Status = table.Column<int>(maxLength: 255, nullable: false),
                    Sender = table.Column<string>(maxLength: 255, nullable: false),
                    Recipient = table.Column<string>(maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HCL.CS.SF_Notification", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HCL.CS.SF_Notification_HCL.CS.SF_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "HCL.CS.SF_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HCL.CS.SF_PasswordHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 255, nullable: false),
                    UserID = table.Column<Guid>(nullable: false),
                    ChangedOn = table.Column<DateTime>(nullable: false),
                    PasswordHash = table.Column<string>(maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HCL.CS.SF_PasswordHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HCL.CS.SF_PasswordHistory_HCL.CS.SF_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "HCL.CS.SF_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HCL.CS.SF_UserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<Guid>(nullable: false),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true),
                    IsAdminClaim = table.Column<bool>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    ModifiedOn = table.Column<DateTime>(nullable: true),
                    CreatedBy = table.Column<string>(maxLength: 255, nullable: false),
                    ModifiedBy = table.Column<string>(maxLength: 255, nullable: true),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HCL.CS.SF_UserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HCL.CS.SF_UserClaims_HCL.CS.SF_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "HCL.CS.SF_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HCL.CS.SF_UserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(maxLength: 256, nullable: false),
                    ProviderKey = table.Column<string>(maxLength: 256, nullable: false),
                    UserId = table.Column<Guid>(nullable: false),
                    ProviderDisplayName = table.Column<string>(nullable: true),
                    Id = table.Column<Guid>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    ModifiedOn = table.Column<DateTime>(nullable: true),
                    CreatedBy = table.Column<string>(maxLength: 255, nullable: false),
                    ModifiedBy = table.Column<string>(maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HCL.CS.SF_UserLogins", x => new { x.LoginProvider, x.ProviderKey, x.UserId });
                    table.ForeignKey(
                        name: "FK_HCL.CS.SF_UserLogins_HCL.CS.SF_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "HCL.CS.SF_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HCL.CS.SF_UserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(nullable: false),
                    RoleId = table.Column<Guid>(nullable: false),
                    Id = table.Column<Guid>(nullable: false),
                    ValidFrom = table.Column<DateTime>(nullable: true),
                    ValidTo = table.Column<DateTime>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    ModifiedOn = table.Column<DateTime>(nullable: true),
                    CreatedBy = table.Column<string>(maxLength: 255, nullable: false),
                    ModifiedBy = table.Column<string>(maxLength: 255, nullable: true),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HCL.CS.SF_UserRoles", x => new { x.Id, x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_HCL.CS.SF_UserRoles_HCL.CS.SF_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "HCL.CS.SF_Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HCL.CS.SF_UserRoles_HCL.CS.SF_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "HCL.CS.SF_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HCL.CS.SF_UserSecurityQuestions",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    ModifiedOn = table.Column<DateTime>(nullable: true),
                    CreatedBy = table.Column<string>(maxLength: 255, nullable: false),
                    ModifiedBy = table.Column<string>(maxLength: 255, nullable: true),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true),
                    UserId = table.Column<Guid>(nullable: false),
                    SecurityQuestionId = table.Column<Guid>(nullable: false),
                    Answer = table.Column<string>(maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HCL.CS.SF_UserSecurityQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HCL.CS.SF_UserSecurityQuestions_HCL.CS.SF_SecurityQuestions_SecurityQuestionId",
                        column: x => x.SecurityQuestionId,
                        principalTable: "HCL.CS.SF_SecurityQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HCL.CS.SF_UserSecurityQuestions_HCL.CS.SF_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "HCL.CS.SF_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HCL.CS.SF_UserTokens",
                columns: table => new
                {
                    UserId = table.Column<Guid>(nullable: false),
                    LoginProvider = table.Column<string>(maxLength: 255, nullable: false),
                    Name = table.Column<string>(maxLength: 255, nullable: false),
                    Value = table.Column<string>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HCL.CS.SF_UserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_HCL.CS.SF_UserTokens_HCL.CS.SF_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "HCL.CS.SF_Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HCL.CS.SF_ApiScopeClaims",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    ModifiedOn = table.Column<DateTime>(nullable: true),
                    CreatedBy = table.Column<string>(maxLength: 255, nullable: false),
                    ModifiedBy = table.Column<string>(maxLength: 255, nullable: true),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true),
                    ApiScopeId = table.Column<Guid>(nullable: false),
                    Type = table.Column<string>(maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HCL.CS.SF_ApiScopeClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HCL.CS.SF_ApiScopeClaims_HCL.CS.SF_ApiScopes_ApiScopeId",
                        column: x => x.ApiScopeId,
                        principalTable: "HCL.CS.SF_ApiScopes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_APIRES_CLM_RESID_TYPE",
                table: "HCL.CS.SF_ApiResourceClaims",
                columns: new[] { "ApiResourceId", "Type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_APIRES_NAME",
                table: "HCL.CS.SF_ApiResources",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_APISCO_CLM_SCOID_TYPE",
                table: "HCL.CS.SF_ApiScopeClaims",
                columns: new[] { "ApiScopeId", "Type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_APISCO_SCOID_NAME",
                table: "HCL.CS.SF_ApiScopes",
                columns: new[] { "ApiResourceId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AUD_CBBY_ACTY",
                table: "HCL.CS.SF_AuditTrail",
                columns: new[] { "CreatedBy", "ActionType" });

            migrationBuilder.CreateIndex(
                name: "IX_AUD_CRON_ACTY",
                table: "HCL.CS.SF_AuditTrail",
                columns: new[] { "CreatedOn", "ActionType" });

            migrationBuilder.CreateIndex(
                name: "IX_AUD_CRON_CBBY",
                table: "HCL.CS.SF_AuditTrail",
                columns: new[] { "CreatedOn", "CreatedBy" });

            migrationBuilder.CreateIndex(
                name: "IX_HCL.CS.SF_ClientPostLogoutRedirectUris_ClientId_PostLogoutRedirectUri",
                table: "HCL.CS.SF_ClientPostLogoutRedirectUris",
                columns: new[] { "ClientId", "PostLogoutRedirectUri" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HCL.CS.SF_ClientRedirectUris_ClientId_RedirectUri",
                table: "HCL.CS.SF_ClientRedirectUris",
                columns: new[] { "ClientId", "RedirectUri" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CLI_CLID_CLSEC",
                table: "HCL.CS.SF_Clients",
                columns: new[] { "ClientId", "ClientSecret" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IDRESCLM_IDRESID_TYPE",
                table: "HCL.CS.SF_IdentityClaims",
                columns: new[] { "IdentityResourceId", "Type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IDRES_NAME",
                table: "HCL.CS.SF_IdentityResources",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NOTI_TYPE",
                table: "HCL.CS.SF_Notification",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_HCL.CS.SF_Notification_UserId",
                table: "HCL.CS.SF_Notification",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_HCL.CS.SF_PasswordHistory_UserID",
                table: "HCL.CS.SF_PasswordHistory",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_HCL.CS.SF_RoleClaims_RoleId",
                table: "HCL.CS.SF_RoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "HCL.CS.SF_Roles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SEC_QUESTION",
                table: "HCL.CS.SF_SecurityQuestions",
                column: "Question",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HCL.CS.SF_UserClaims_UserId",
                table: "HCL.CS.SF_UserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_HCL.CS.SF_UserLogins_UserId",
                table: "HCL.CS.SF_UserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_HCL.CS.SF_UserRoles_RoleId",
                table: "HCL.CS.SF_UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_HCL.CS.SF_UserRoles_UserId",
                table: "HCL.CS.SF_UserRoles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "HCL.CS.SF_Users",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "HCL.CS.SF_Users",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_USRSEC_QUEID",
                table: "HCL.CS.SF_UserSecurityQuestions",
                column: "SecurityQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_USRSEC_UID_QUEID",
                table: "HCL.CS.SF_UserSecurityQuestions",
                columns: new[] { "UserId", "SecurityQuestionId" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HCL.CS.SF_ApiResourceClaims");

            migrationBuilder.DropTable(
                name: "HCL.CS.SF_ApiScopeClaims");

            migrationBuilder.DropTable(
                name: "HCL.CS.SF_AuditTrail");

            migrationBuilder.DropTable(
                name: "HCL.CS.SF_ClientPostLogoutRedirectUris");

            migrationBuilder.DropTable(
                name: "HCL.CS.SF_ClientRedirectUris");

            migrationBuilder.DropTable(
                name: "HCL.CS.SF_IdentityClaims");

            migrationBuilder.DropTable(
                name: "HCL.CS.SF_Notification");

            migrationBuilder.DropTable(
                name: "HCL.CS.SF_PasswordHistory");

            migrationBuilder.DropTable(
                name: "HCL.CS.SF_RoleClaims");

            migrationBuilder.DropTable(
                name: "HCL.CS.SF_SecurityTokens");

            migrationBuilder.DropTable(
                name: "HCL.CS.SF_UserClaims");

            migrationBuilder.DropTable(
                name: "HCL.CS.SF_UserLogins");

            migrationBuilder.DropTable(
                name: "HCL.CS.SF_UserRoles");

            migrationBuilder.DropTable(
                name: "HCL.CS.SF_UserSecurityQuestions");

            migrationBuilder.DropTable(
                name: "HCL.CS.SF_UserTokens");

            migrationBuilder.DropTable(
                name: "HCL.CS.SF_ApiScopes");

            migrationBuilder.DropTable(
                name: "HCL.CS.SF_Clients");

            migrationBuilder.DropTable(
                name: "HCL.CS.SF_IdentityResources");

            migrationBuilder.DropTable(
                name: "HCL.CS.SF_Roles");

            migrationBuilder.DropTable(
                name: "HCL.CS.SF_SecurityQuestions");

            migrationBuilder.DropTable(
                name: "HCL.CS.SF_Users");

            migrationBuilder.DropTable(
                name: "HCL.CS.SF_ApiResources");
        }
    }
}


