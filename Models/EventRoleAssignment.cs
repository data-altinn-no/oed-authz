using System.Text.Json.Serialization;

namespace oed_authz.Models;
public class EventRoleAssignment
{
    [JsonPropertyName("nin")]
    public string Nin { get; set; } = string.Empty;

    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;
}
