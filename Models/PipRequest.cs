using System.ComponentModel.DataAnnotations;

namespace oed_authz.Models;

public class PipRequest
{
    [Required]
    public string CoveredBy { get; set; } = null!;
    [Required]
    public string OfferedBy { get; set; } = null!;
}

