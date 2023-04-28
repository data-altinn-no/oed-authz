using oed_authz.Interfaces;
using oed_authz.Models;

namespace oed_authz.Services;
public class PolicyInformationPointService : IPolicyInformationPointService
{
    private readonly IOedRoleRepositoryService _oedRoleRepositoryService;

    public PolicyInformationPointService(IOedRoleRepositoryService oedRoleRepositoryService)
    {
        _oedRoleRepositoryService = oedRoleRepositoryService;
    }

    public async Task<PipResponse> HandlePipRequest(PipRequest pipRequest)
    {
        if (pipRequest.To is not null && !Utils.IsValidSsn(pipRequest.To))
        {
            throw new ArgumentException(nameof(pipRequest.To));
        }

        if (pipRequest.From is not null && !Utils.IsValidSsn(pipRequest.From))
        {
            throw new ArgumentException(nameof(pipRequest.From));
        }

        List<OedRoleAssignment> roleAssignments;
        if (pipRequest.To is not null && pipRequest.From is not null)
        {
            roleAssignments = await _oedRoleRepositoryService.GetRoleAssignmentsForPerson(pipRequest.To, pipRequest.From);
        }
        else if (pipRequest.To is not null)
        {
            roleAssignments = await _oedRoleRepositoryService.GetRoleAssignmentsForPerson(pipRequest.To);
        }
        else if (pipRequest.From is not null)
        {
            roleAssignments = await _oedRoleRepositoryService.GetRoleAssignmentsForEstate(pipRequest.From);
        }
        else
        {
            throw new ArgumentNullException(nameof(pipRequest), "Both recipientSsn and estateSsn cannot be null");
        }

        var pipRoleAssignments = new List<PipRoleAssignment>();
        foreach (var result in roleAssignments)
        {
            pipRoleAssignments.Add(new PipRoleAssignment
            {
                RoleCode = result.RoleCode,
                Created = result.Created,
                From = result.EstateSsn,
                To = result.Recipient
            });
        }

        return new PipResponse { RoleAssignments = pipRoleAssignments };
    }
}
