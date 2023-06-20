namespace oed_authz.Models;

public class ExternalRoleAssignment
{
    public string EstateSsn { get; init; } = string.Empty;
    public string RecipientSsn { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public DateTimeOffset Created { get; set; }
}
