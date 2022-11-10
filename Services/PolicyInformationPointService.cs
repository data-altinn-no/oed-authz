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

    public async Task<List<PipRoleAssignment>> HandlePipRequest(PipRequest pipRequest)
    {
        if (!Utils.IsValidSsn(pipRequest.To))
        {
            throw new ArgumentException(nameof(pipRequest.To));
        }

        if (!Utils.IsValidSsn(pipRequest.From))
        {
            throw new ArgumentException(nameof(pipRequest.From));
        }

        var results = await _oedRoleRepositoryService.GetRoleAssignmentsForPerson(pipRequest.To, pipRequest.From);

        var pipResponse = new List<PipRoleAssignment>();
        foreach (var result in results)
        {
            pipResponse.Add(new PipRoleAssignment
            {
                RoleCode = result.RoleCode
            });
        }

        return pipResponse;
    }
}
