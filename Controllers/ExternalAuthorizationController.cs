using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using oed_authz.Interfaces;
using oed_authz.Models;
using oed_authz.Settings;

namespace oed_authz.Controllers;

[ApiController]
[Route("api/v1/authorization")]
public class ExternalAuthorizationController : Controller
{
    private readonly IPolicyInformationPointService _pipService;

    public ExternalAuthorizationController(IPolicyInformationPointService pipService)
    {
        _pipService = pipService;
    }

    [HttpPost]
    [Route("roles")]
    [Authorize(Policy = Constants.AuthorizationPolicyExternal)]
    public async Task<ActionResult<ExternalAuthorizationResponse>> GetRoles([FromBody] ExternalAuthorizationRequest externalAuthorizationRequest)
    {
        try
        {
            return Ok(await HandleRequest(externalAuthorizationRequest, !HasAllRolesScope()));
        }
        catch (ArgumentException ex)
        {
            return Problem(
                title: "Bad Input",
                detail: ex.GetType().Name + ": " + ex.Message,
                statusCode: StatusCodes.Status400BadRequest
            );
        }
    }

    private bool HasAllRolesScope()
    {
        var scopeClaim = User.Claims.FirstOrDefault(x => x.Type.Equals("scope", StringComparison.OrdinalIgnoreCase));
        if (scopeClaim == null)
        {
            throw new ArgumentException("Missing scope claim");
        }

        var scopes = scopeClaim.Value.Split(' ');
        return scopes.Contains(Constants.ScopeAllRoles);
    }

    private async Task<ExternalAuthorizationResponse> HandleRequest(ExternalAuthorizationRequest externalAuthorizationRequest, bool probateOnly)
    {
        var pipRequest = new PipRequest
        {
            From = externalAuthorizationRequest.EstateSsn,
            To = externalAuthorizationRequest.RecipientSsn
        };

        var pipResponse = await _pipService.HandlePipRequest(pipRequest);

        RemoveHeirRoles(pipResponse);

        if (probateOnly)
        {
            pipResponse.RoleAssignments = pipResponse.RoleAssignments.Where(x => x.RoleCode == Constants.ProbateRoleCode).ToList();
        }

        var externalRoleAssignments =
            pipResponse.RoleAssignments.Select(pipRoleAssignment =>
                new ExternalRoleAssignment
                {
                    EstateSsn = pipRoleAssignment.From,
                    RecipientSsn = pipRoleAssignment.To,
                    Role = pipRoleAssignment.RoleCode,
                    Created = pipRoleAssignment.Created
                }).ToList();

        var externalAuthorizationResponse = new ExternalAuthorizationResponse
        {
            RoleAssignments = externalRoleAssignments
        };

        return externalAuthorizationResponse;
    }

    private void RemoveHeirRoles(PipResponse pipResponse)
    {
        pipResponse.RoleAssignments = pipResponse.RoleAssignments.Where(x => !x.RoleCode.StartsWith(Constants.HeirRoleCodePrefix)).ToList();
    }
}
