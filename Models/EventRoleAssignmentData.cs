using System.Text.Json.Serialization;

namespace oed_authz.Models;
public class EventRoleAssignmentData
{
    [JsonPropertyName("nin")]
    public string Nin { get; set; } = string.Empty;

    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;
}
