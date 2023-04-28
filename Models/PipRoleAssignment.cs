using System.Text.Json.Serialization;

namespace oed_authz.Models;
public class PipRoleAssignment
{
    [JsonPropertyName("urn:oed:rolecode")]
    public string RoleCode { get; init; } = string.Empty;
    public DateTimeOffset Created { get; set; }
}
