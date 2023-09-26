using System.ComponentModel.DataAnnotations;

namespace oed_authz.Models.Dto;

public class RolesSearchRequestDto
{
    [Required]
    public string EstateSsn { get; set; } = null!;
}
