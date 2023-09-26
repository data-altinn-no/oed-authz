using System.Text.Json.Serialization;

namespace oed_authz.Models.Dto;

public class ProxySearchAssignmentDto
{
    public string From { get; set; } = null!;

    public string To { get; set; } = null!;

    [JsonPropertyName("urn:digitaltdodsbo:rolecode")]
    public string Role { get; set; } = null!;

    public DateTimeOffset? Created { get; set; }
}
