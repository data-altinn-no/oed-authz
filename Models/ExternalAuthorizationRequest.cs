using System.ComponentModel.DataAnnotations;

namespace oed_authz.Models;

public class ExternalAuthorizationRequest
{
    [Required]
    public string EstateSsn { get; set; } = null!;
    [Required]
    public string RecipientSsn { get; set; } = null!;
}
