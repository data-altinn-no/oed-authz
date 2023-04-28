using System.ComponentModel.DataAnnotations;

namespace oed_authz.Models;

public class ExternalAuthorizationRequest
{
    [Required]
    public string Deceased { get; set; } = null!;
    [Required]
    public string Heir { get; set; } = null!;
}
