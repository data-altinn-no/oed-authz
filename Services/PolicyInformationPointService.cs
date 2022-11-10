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
        if (!Utils.IsValidSsn(pipRequest.CoveredBy))
        {
            throw new ArgumentException(nameof(pipRequest.CoveredBy));
        }

        if (!Utils.IsValidSsn(pipRequest.OfferedBy))
        {
            throw new ArgumentException(nameof(pipRequest.OfferedBy));
        }

        var results = await _oedRoleRepositoryService.GetRoleAssignmentsForUser(pipRequest.CoveredBy, pipRequest.OfferedBy);

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
