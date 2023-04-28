using System.ComponentModel.DataAnnotations;

namespace oed_authz.Models;

public class PipRequest
{
    public string? From { get; init; }
    public string? To { get; init; }
}
