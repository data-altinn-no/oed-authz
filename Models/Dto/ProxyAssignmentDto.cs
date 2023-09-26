using System.Text.Json.Serialization;

namespace oed_authz.Models.Dto;

public class ProxyAssignmentDto
{
    [JsonRequired]
    public string EstateSsn { get; set; } = null!;

    [JsonRequired]
    public string From { get; set; } = null!;

    [JsonRequired]
    public string To { get; set; } = null!;

    [JsonRequired]
    [JsonPropertyName("urn:digitaltdodsbo:rolecode")]
    public string Role { get; set; } = null!;

    public DateTimeOffset? Created { get; set; }
}
