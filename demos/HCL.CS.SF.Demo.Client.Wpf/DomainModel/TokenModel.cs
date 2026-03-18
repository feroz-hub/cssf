/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace HCL.CS.SF.DemoClientWpfApp.DomainModel
{
    public class TokenResponseResultModel
    {
        public string id_token { get; set; }

        public string access_token { get; set; }

        public int expires_in { get; set; }

        public string token_type { get; set; }

        public string refresh_token { get; set; }

        public string scope { get; set; }

        public ErrorResponseResultModel ErrorResponseResult { get; set; }
    }
}


