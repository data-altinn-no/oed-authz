namespace oed_authz.Models;

public class ExternalAuthorizationResponse
{
    public string Deceased { get; set; } = null!;
    public string Heir { get; set; } = null!;
    public List<ExternalRoleAssignment> RoleAssignments { get; set; } = new();
}

