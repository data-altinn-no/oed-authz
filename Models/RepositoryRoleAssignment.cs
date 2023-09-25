namespace oed_authz.Models;
public class RepositoryRoleAssignment
{
    public long Id { get; set; }
    public string RoleCode { get; set; } = string.Empty;
    public string EstateSsn { get; set; } = string.Empty;
    public string? HeirSsn { get; set; }
    public string Recipient { get; set; } = string.Empty;
    public DateTimeOffset Created { get; set; }
}
