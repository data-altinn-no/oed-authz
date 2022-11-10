using System.ComponentModel.DataAnnotations;

namespace oed_authz.Models;

public class PipRequest
{
    [Required]
    public string From { get; set; } = null!;
    [Required]
    public string To { get; set; } = null!;
}
