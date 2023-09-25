using System.Text.Json.Serialization;

namespace oed_authz.Models.Dto;
public class EventRoleAssignmentDataDto
{
    [JsonPropertyName("caseId")]
    public string DaCaseId  { get; set; } = string.Empty;

    [JsonPropertyName("heirRoles")]
    public List<EventRoleAssignmentDto> HeirRoles  { get; set; } = new();
}
