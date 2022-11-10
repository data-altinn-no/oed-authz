namespace oed_authz.Models;
public class OedRoleAssignment
{
    public string RoleCode { get; set; } = string.Empty;
    public string EstateSsn = string.Empty;
    public string Recipient = string.Empty;
    public DateTimeOffset Created { get; set; }
}
