using oed_authz.Interfaces;
using oed_authz.Models;

namespace oed_authz.Services;

public class PapService : IPolicyAdministrationPointService
{
    private readonly IOedRoleRepositoryService _oedRoleRepositoryService;

    public PapService(IOedRoleRepositoryService oedRoleRepositoryService)
    {
        _oedRoleRepositoryService = oedRoleRepositoryService;
    }

    public async Task Add(PapRequest papRequest)
    {
        ValidateRequest(papRequest);

        var roleAssignment = new RepositoryRoleAssignment
        {
            EstateSsn = papRequest.EstateSsn,
            HeirSsn = papRequest.RoleAssignment.From == papRequest.EstateSsn ? null : papRequest.RoleAssignment.From,
            Recipient = papRequest.RoleAssignment.To,
            RoleCode = papRequest.RoleAssignment.RoleCode,
            Created = papRequest.RoleAssignment.Created ?? DateTimeOffset.Now
        };

        await _oedRoleRepositoryService.AddRoleAssignment(roleAssignment);
    }

    public async Task Remove(PapRequest papRequest)
    {
        ValidateRequest(papRequest);

        var roleAssignment = new RepositoryRoleAssignment
        {
            EstateSsn = papRequest.EstateSsn,
            HeirSsn =  papRequest.RoleAssignment.From == papRequest.EstateSsn ? null : papRequest.RoleAssignment.From,
            Recipient = papRequest.RoleAssignment.To,
            RoleCode = papRequest.RoleAssignment.RoleCode
        };

        await _oedRoleRepositoryService.RemoveRoleAssignment(roleAssignment);
    }

    private void ValidateRequest(PapRequest papRequest)
    {
        if (!Utils.IsValidSsn(papRequest.EstateSsn))
        {
            throw new ArgumentException(nameof(papRequest.RoleAssignment.To));
        }

        if (!Utils.IsValidSsn(papRequest.RoleAssignment.To))
        {
            throw new ArgumentException(nameof(papRequest.RoleAssignment.To));
        }

        if (!Utils.IsValidSsn(papRequest.RoleAssignment.From))
        {
            throw new ArgumentException(nameof(papRequest.RoleAssignment.From));
        }
    }
}
