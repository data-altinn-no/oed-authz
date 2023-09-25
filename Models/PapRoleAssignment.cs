namespace oed_authz.Models;

public class PapRoleAssignment
{
    public string From { get; init; } = string.Empty;
    public string To { get; init; } = string.Empty;
    public string RoleCode { get; init; } = string.Empty;
    public DateTimeOffset? Created { get; init; } = DateTimeOffset.Now;
}
