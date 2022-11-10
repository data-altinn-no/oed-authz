using System.Text.Json.Serialization;

namespace oed_authz.Models;
public class PipRoleAssignment
{
    [JsonPropertyName("urn:oed:rolecode")]
    public string RoleCode { get; set; } = string.Empty;
}
