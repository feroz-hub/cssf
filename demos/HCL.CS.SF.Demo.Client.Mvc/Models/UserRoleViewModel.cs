/*
 * Copyright (c) 2021 HCL CORPORATION.
 * All rights reserved. HCL source code is an unpublished work and the use of a copyright notice does not imply otherwise.
 * This source code contains confidential, trade secret material of HCL. Any attempt or participation in deciphering,
 * decoding, reverse engineering or in any way altering the source code is strictly prohibited, unless the prior written consent of
 * HCL is obtained. This is proprietary and confidential to HCL.
 */

namespace HCL.CS.SF.DemoClientMvc.Models;

public class UserRoleViewModel
{
    public int Id { get; set; }
    public List<ManageUsers> ManageUsers { get; set; }
}

public abstract class ManageUsers
{
    public string UserId { get; set; }
    public string UserName { get; set; }
    public string LockStatus { get; set; }
}
