using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace oed_authz.Models;
public class PipRoleAssignment
{
    public string CoveredBy { get; set; } = string.Empty;
    public string OfferedBy { get; set; } = string.Empty;
    public string RoleCode { get; set; } = string.Empty;
}
