using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using oed_authz.Interfaces;
using oed_authz.Models;
using oed_authz.Models.Dto;
using oed_authz.Settings;

namespace oed_authz.Controllers;

[ApiController]
[Route("api/v1/authorization")]
public class AuthorizationController : Controller
{
    private readonly IPolicyInformationPointService _pipService;
    private readonly IPolicyAdministrationPointService _papService;

    public AuthorizationController(IPolicyInformationPointService pipService, IPolicyAdministrationPointService papService)
    {
        _pipService = pipService;
        _papService = papService;
    }

    [HttpPost]
    [Route("roles/search")]
    [Authorize(Policy = Constants.AuthorizationPolicyExternal)]
    public async Task<ActionResult<RolesSearchResponseDto>> GetRoles([FromBody] RolesSearchRequestDto rolesSearchRequestDto)
    {
        try
        {
            return Ok(await HandleRequest(rolesSearchRequestDto));
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

    [HttpPost]
    [Route("proxies/search")]
    [Authorize(Policy = Constants.AuthorizationPolicyExternal)]
    public async Task<ActionResult<ProxySearchResponseDto>> GetProxies([FromBody] ProxySearchRequestDto proxySearchRequestDto)
    {
        try
        {
            return Ok(await HandleRequest(proxySearchRequestDto));
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

    [HttpPost]
    [Route("proxies/add")]
    [Authorize(Policy = Constants.AuthorizationPolicyInternal)]
    public async Task<ActionResult> AddProxy([FromBody] ProxyAddRequestDto proxyAddRequestDto)
    {
        try
        {
            await HandleRequest(proxyAddRequestDto);
            return new ObjectResult(null) { StatusCode = StatusCodes.Status201Created };
        }
        catch (ArgumentException ex)
        {
            return Problem(
                title: "Bad Input",
                detail: ex.GetType().Name + ": " + ex.Message,
                statusCode: StatusCodes.Status400BadRequest
            );
        }
        catch (PostgresException ex) when (ex.SqlState == "23505")
        {
            return Problem(
                title: "Constraint Violation",
                detail: "There can only be one proxy assignment per estate-heir-recipient-role combination.",
                statusCode: StatusCodes.Status409Conflict
            );
        }
    }

    [HttpPost]
    [Route("proxies/remove")]
    [Authorize(Policy = Constants.AuthorizationPolicyInternal)]
    public async Task<ActionResult> RemoveProxy([FromBody] ProxyRemoveRequestDto proxyRemoveRequestDto)
    {
        try
        {
            await HandleRequest(proxyRemoveRequestDto);
            return NoContent();
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

    private async Task<RolesSearchResponseDto> HandleRequest(RolesSearchRequestDto rolesSearchRequestDto)
    {
        var pipRequest = new PipRequest
        {
            EstateSsn = rolesSearchRequestDto.EstateSsn
        };

        var pipResponse = await _pipService.HandlePipRequest(pipRequest);

        RemoveHeirRoles(pipResponse);

        var roleAssignmentDtos =
            pipResponse.RoleAssignments.Select(pipRoleAssignment =>
                new RoleAssignmentDto()
                {
                    RecipientSsn = pipRoleAssignment.To,
                    Role = pipRoleAssignment.RoleCode,
                    Created = pipRoleAssignment.Created
                }).ToList();

        var rolesSearchResponseDto = new RolesSearchResponseDto()
        {
            EstateSsn = rolesSearchRequestDto.EstateSsn,
            RoleAssignments = roleAssignmentDtos
        };

        return rolesSearchResponseDto;
    }

    private async Task<ProxySearchResponseDto> HandleRequest(ProxySearchRequestDto proxySearchRequestDto)
    {
        var pipRequest = new PipRequest
        {
            EstateSsn = proxySearchRequestDto.EstateSsn
        };

        var pipResponse = await _pipService.HandlePipRequest(pipRequest);

        RemoveNonProxyRoles(pipResponse);

        var proxyAssignmentDtos =
            pipResponse.RoleAssignments.Select(pipRoleAssignment =>
                new ProxySearchAssignmentDto()
                {
                    To = pipRoleAssignment.To,
                    From = pipRoleAssignment.From,
                    Role = pipRoleAssignment.RoleCode,
                    Created = pipRoleAssignment.Created
                }).ToList();

        var proxySearchResponseDto = new ProxySearchResponseDto()
        {
            EstateSsn = proxySearchRequestDto.EstateSsn,
            ProxyAssignments = proxyAssignmentDtos
        };

        return proxySearchResponseDto;
    }

    private async Task HandleRequest(ProxyAddRequestDto proxyAddRequestDto)
    {
        var papRequest = new PapRequest
        {
            EstateSsn = proxyAddRequestDto.Add.EstateSsn,
            RoleAssignment = new PapRoleAssignment
            {
                From = proxyAddRequestDto.Add.From,
                To = proxyAddRequestDto.Add.To,
                RoleCode = proxyAddRequestDto.Add.Role,
                Created = DateTime.UtcNow
            }
        };

        await _papService.Add(papRequest);
    }

    private async Task HandleRequest(ProxyRemoveRequestDto proxyRemoveRequestDto)
    {
        var papRequest = new PapRequest
        {
            EstateSsn = proxyRemoveRequestDto.Remove.EstateSsn,
            RoleAssignment = new PapRoleAssignment
            {
                From = proxyRemoveRequestDto.Remove.From,
                To = proxyRemoveRequestDto.Remove.To,
                RoleCode = proxyRemoveRequestDto.Remove.Role
            }
        };

        await _papService.Remove(papRequest);
    }

    private void RemoveHeirRoles(PipResponse pipResponse)
    {
        pipResponse.RoleAssignments = pipResponse.RoleAssignments.Where(x => !x.RoleCode.StartsWith(Constants.HeirRoleCodePrefix)).ToList();
    }

    private void RemoveNonProxyRoles(PipResponse pipResponse)
    {
        pipResponse.RoleAssignments = pipResponse.RoleAssignments.Where(x => x.RoleCode.StartsWith(Constants.ProxyRoleCodePrefix)).ToList();
    }
}
