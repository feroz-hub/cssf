namespace HCL.CS.SF.Admin.UI.Models.Api;

public sealed class ClientSecretsViewModel
{
    public string ClientId { get; set; } = string.Empty;

    public string ClientName { get; set; } = string.Empty;

    public DateTime? SecretExpiresAt { get; set; }
}

