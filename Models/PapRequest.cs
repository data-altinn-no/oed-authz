namespace oed_authz.Models;

public class PapRequest
{
    public string EstateSsn { get; set; } = string.Empty;
    public PapRoleAssignment RoleAssignment { get; set; } = new();
}
