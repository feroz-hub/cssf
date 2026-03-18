/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

using HCL.CS.SF.DemoClientMvc.Extension;
using HCL.CS.SF.DemoClientMvc.Interface;
using HCL.CS.SF.Domain.Constants;
using HCL.CS.SF.Domain.Models.Api;

namespace HCL.CS.SF.DemoClientMvc.Helper;

public class UserManagementHelper(IHttpService httpService)
{
    public IList<SecurityQuestionModel> LoadSecurityQuestionCombo()
    {
        var securityQuestionList = httpService.PostAsync<IList<SecurityQuestionModel>>(
            ApiRoutePathConstants.GetAllSecurityQuestions,
            string.Empty).GetAwaiter().GetResult();
        if (!securityQuestionList.ContainsAny()) return null;
        securityQuestionList.Insert(0, new SecurityQuestionModel
        {
            Question = "----Select----"
            //Id = new System.Guid()
        });

        return securityQuestionList;
    }
}
