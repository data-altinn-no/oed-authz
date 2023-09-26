namespace oed_authz.Models.Dto;

public class ProxySearchResponseDto
{
    public string EstateSsn { get; set; } = null!;
    public List<ProxySearchAssignmentDto> ProxyAssignments { get; set; } = new();
}
