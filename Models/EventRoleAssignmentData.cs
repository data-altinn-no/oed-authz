using System.Text.Json.Serialization;

namespace oed_authz.Models;
public class EventRoleAssignmentData
{
    [JsonPropertyName("recipient")]
    public string Recipient { get; set; } = string.Empty;

    [JsonPropertyName("roleCode")]
    public string RoleCode { get; set; } = string.Empty;
}
