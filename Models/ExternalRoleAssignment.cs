namespace oed_authz.Models;

public class ExternalRoleAssignment
{
    public string Role { get; init; } = string.Empty;
    public DateTimeOffset Created { get; set; }
}
