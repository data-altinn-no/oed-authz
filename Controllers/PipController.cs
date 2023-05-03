using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using oed_authz.Authorization;
using oed_authz.Interfaces;
using oed_authz.Models;
using oed_authz.Settings;

namespace oed_authz.Controllers;
[ApiController]
public class PipController : Controller
{
    private readonly IPolicyInformationPointService _pipService;

    public PipController(IPolicyInformationPointService pipService)
    {
        _pipService = pipService;
    }

    [HttpPost]
    [Authorize(Policy = Constants.AuthorizationPolicyForPlatformAuthorization)]
    [Route("api/v1/pip/platform")]
    public async Task<ActionResult<PipResponse>> HandlePlatformPipRequest([FromBody] PipRequest pipRequest)
        => await HandlePipRequest(pipRequest);

    [HttpPost]
    [Authorize(Policy = Constants.AuthorizationPolicyForDdApp)]
    [Route("api/v1/pip/app")]
    public async Task<ActionResult<PipResponse>> HandleAppPipRequest([FromBody] PipRequest pipRequest)
        => await HandlePipRequest(pipRequest);

    private async Task<ActionResult<PipResponse>> HandlePipRequest(PipRequest pipRequest)
    {
        try
        {
            return Ok(await _pipService.HandlePipRequest(pipRequest));
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



}
