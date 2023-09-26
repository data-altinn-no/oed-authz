using System.ComponentModel.DataAnnotations;

namespace oed_authz.Models.Dto;

public class ProxySearchRequestDto
{
    [Required]
    public string EstateSsn { get; set; } = null!;
}
