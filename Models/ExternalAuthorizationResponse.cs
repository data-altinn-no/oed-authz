namespace oed_authz.Models;

public class ExternalAuthorizationResponse
{
    public string EstateSsn { get; set; } = null!;
    public string RecipientSsn { get; set; } = null!;
    public List<ExternalRoleAssignment> RoleAssignments { get; set; } = new();
}

