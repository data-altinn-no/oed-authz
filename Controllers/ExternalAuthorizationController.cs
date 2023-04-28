using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using oed_authz.Interfaces;
using oed_authz.Models;
using oed_authz.Settings;

namespace oed_authz.Controllers;

[ApiController]
[Route("api/authorization")]
public class ExternalAuthorizationController : Controller
{
    private readonly IPolicyInformationPointService _pipService;

    public ExternalAuthorizationController(IPolicyInformationPointService pipService)
    {
        _pipService = pipService;
    }

    [HttpPost]
    [Route("probate")]
    [Authorize(Policy = Constants.AuthorizationPolicyForExternalsProbateOnly)]
    public async Task<ActionResult<List<ExternalAuthorizationResponse>>> ProbateOnly([FromBody] ExternalAuthorizationRequest externalAuthorizationRequest)
    {
        return await HandleRequest(externalAuthorizationRequest, true);
    }

    [HttpPost]
    [Route("roles")]
    [Authorize(Policy = Constants.AuthorizationPolicyForExternalsAllRoles)]
    public async Task<ActionResult<List<ExternalAuthorizationResponse>>> AllRoles([FromBody] ExternalAuthorizationRequest externalAuthorizationRequest)
    {
        return await HandleRequest(externalAuthorizationRequest, false);
    }

    private async Task<ActionResult<List<ExternalAuthorizationResponse>>> HandleRequest(ExternalAuthorizationRequest externalAuthorizationRequest, bool probateOnly)
    {
        var pipRequest = new PipRequest
        {
            From = externalAuthorizationRequest.Deceased,
            To = externalAuthorizationRequest.Heir
        };

        var pipResponse = await _pipService.HandlePipRequest(pipRequest);

        if (probateOnly)
        {
            pipResponse = pipResponse.Where(x => x.RoleCode == Constants.ProbateRoleCode).ToList();
        }

        var externalRoleAssignments =
            pipResponse.Select(pipRoleAssignment =>
                new ExternalRoleAssignment { Role = pipRoleAssignment.RoleCode, Created = pipRoleAssignment.Created }).ToList();

        var externalAuthorizationResponse = new ExternalAuthorizationResponse
        {
            Deceased = externalAuthorizationRequest.Deceased,
            Heir = externalAuthorizationRequest.Heir,
            RoleAssignments = externalRoleAssignments
        };

        return Ok(externalAuthorizationResponse);
    }
}
