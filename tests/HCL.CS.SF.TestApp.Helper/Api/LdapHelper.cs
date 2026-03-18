/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.Domain.Configurations.Api;

namespace TestApp.Helper.Api;

public static class LdapHelper
{
    public static LdapConfig GetTestLdapConfigUnsecure()
    {
        return new LdapConfig
        {
            IsSecureConnection = false,
            LdapHostName = "HCL.CS.SF",
            LdapPort = 10389
        };
    }

    public static LdapConfig GetTestLdapConfigSecure()
    {
        return new LdapConfig
        {
            IsSecureConnection = true,
            LdapHostName = "HCL.CS.SF",
            LdapPort = 10645
        };
    }
}
