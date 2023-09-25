namespace oed_authz.Models.Dto;

public class RolesSearchResponseDto
{
    public string EstateSsn { get; set; } = null!;
    public List<RoleAssignmentDto> RoleAssignments { get; set; } = new();
}
