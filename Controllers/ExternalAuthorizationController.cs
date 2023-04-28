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
    public async Task<ActionResult<ExternalAuthorizationResponse>> ProbateOnly([FromBody] ExternalAuthorizationRequest externalAuthorizationRequest)
    {
        return await HandleRequest(externalAuthorizationRequest, true);
    }

    [HttpPost]
    [Route("roles")]
    [Authorize(Policy = Constants.AuthorizationPolicyForExternalsAllRoles)]
    public async Task<ActionResult<ExternalAuthorizationResponse>> AllRoles([FromBody] ExternalAuthorizationRequest externalAuthorizationRequest)
    {
        return await HandleRequest(externalAuthorizationRequest, false);
    }

    private async Task<ActionResult<ExternalAuthorizationResponse>> HandleRequest(ExternalAuthorizationRequest externalAuthorizationRequest, bool probateOnly)
    {
        var pipRequest = new PipRequest
        {
            From = externalAuthorizationRequest.EstateSsn,
            To = externalAuthorizationRequest.RecipientSsn
        };

        var pipResponse = await _pipService.HandlePipRequest(pipRequest);

        if (probateOnly)
        {
            pipResponse.RoleAssignments = pipResponse.RoleAssignments.Where(x => x.RoleCode == Constants.ProbateRoleCode).ToList();
        }

        var externalRoleAssignments =
            pipResponse.RoleAssignments.Select(pipRoleAssignment =>
                new ExternalRoleAssignment { Role = pipRoleAssignment.RoleCode, Created = pipRoleAssignment.Created }).ToList();

        var externalAuthorizationResponse = new ExternalAuthorizationResponse
        {
            EstateSsn = externalAuthorizationRequest.EstateSsn,
            RecipientSsn = externalAuthorizationRequest.RecipientSsn,
            RoleAssignments = externalRoleAssignments
        };

        return Ok(externalAuthorizationResponse);
    }
}
