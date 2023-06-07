using System.Text.Json.Serialization;

namespace oed_authz.Models;
public class EventRoleAssignmentData
{
    [JsonPropertyName("daCaseId")]
    public string DaCaseId  { get; set; } = string.Empty;

    [JsonPropertyName("heirRoles")]
    public List<EventRoleAssignment> HeirRoles  { get; set; } = new();
}
