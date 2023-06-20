namespace oed_authz.Models;

public class ExternalAuthorizationResponse
{
    public List<ExternalRoleAssignment> RoleAssignments { get; set; } = new();
}

