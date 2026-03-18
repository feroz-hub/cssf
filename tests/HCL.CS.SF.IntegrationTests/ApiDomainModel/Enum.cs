/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace IntegrationTests.ApiDomainModel;

public enum DbTypes
{
    SqlServer = 1,

    MySql = 2
}

public enum WriteLogTo
{
    File = 0,

    DataBase = 1
}

public enum RollingIntervalType
{
    Month = 0,

    Day = 1,

    Hour = 2,

    Minute = 3
}

public enum Log
{
    Debug = 0,

    Error = 1,

    Fatal = 2,

    Information = 3,

    Verbose = 4,

    Warning = 5
}

public enum ResultStatus
{
    Success = 0,

    Failed = 1
}

public enum CrudMode
{
    Add = 0,

    Update = 1,

    Delete = 2
}

public enum ApplicationType
{
    RegularWeb = 1,

    SinglePageApp = 2,

    Native = 3,

    Service = 4
}

public enum GrantType
{
    AuthorizationCode = 1,

    Password = 2,

    ClientCredentials = 3,

    RefreshToken = 4
}

public enum AccessTokenType
{
    JWT = 1 // TODO: Enum JWT is never used.
    // Reference = 2  // Planned for V2 Release.
}

public enum CspLevel
{
    One = 0,

    Two = 1
}

public enum SigningAlgorithm
{
    RS256 = 1,

    RS384 = 2,

    RS512 = 3,

    HS256 = 4,

    HS384 = 5,

    HS512 = 6,

    ES256 = 7,

    ES384 = 8,

    ES512 = 9,

    PS256 = 10,

    PS384 = 11,

    PS512 = 12
}

public enum ParseMethods
{
    Basic = 0,

    Post = 1,

    JwtSecret = 2
}

public enum QueryOption
{
    Date = 1,

    ChangeByAndDate = 2,

    ChangeByAndBetweenDates = 3,

    ChangeBywithActionAndBetweenDates = 4,

    None = 5
}

public enum NotificationStatus
{
    Initiated = 1,

    Delivered = 2,

    Failed = 3,

    Delayed = 4,

    Relayed = 5,

    Expanded = 6,

    Queued = 7,

    Sending = 8,

    Sent = 9,

    Undelivered = 10,

    Receiving = 11,

    Received = 12,

    Accepted = 13,

    Scheduled = 14,

    Read = 15,

    Partially = 16
}

public enum NotificationTypes
{
    Email = 1,

    SMS = 2,

    Both = 3
}

public enum EmailNotificationType
{
    Token = 1,

    Link = 2
}

public enum IdentityProvider
{
    Local = 1,

    Ldap = 2,

    Google = 3
}

public enum AuditType
{
    None = 0,

    Create = 1,

    Update = 2,

    Delete = 3
}

public enum TwoFactorType
{
    None = 0,

    Email = 1,

    Sms = 2,

    AuthenticatorApp = 3
}
