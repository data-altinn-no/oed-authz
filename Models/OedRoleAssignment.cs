namespace oed_authz.Models;
public class OedRoleAssignment
{
    public string RoleCode { get; set; } = string.Empty;
    public string EstateSsn { get; set; }= string.Empty;
    public string Recipient { get; set; } = string.Empty;
    public DateTimeOffset Created { get; set; }
}
