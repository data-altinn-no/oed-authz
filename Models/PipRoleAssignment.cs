using System.Text.Json.Serialization;

namespace oed_authz.Models;
public class PipRoleAssignment
{
    public long Id { get; set; }

    public string RoleCode { get; init; } = string.Empty;

    public string From { get; init; } = string.Empty;

    public string To { get; init; } = string.Empty;

    public DateTimeOffset Created { get; set; }
}
