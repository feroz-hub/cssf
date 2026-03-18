namespace HCL.CS.SF.Admin.UI.Models.Api;

public sealed class UserSessionsViewModel
{
    public string UserId { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public List<SecurityTokensModel> Sessions { get; set; } = new();
}

