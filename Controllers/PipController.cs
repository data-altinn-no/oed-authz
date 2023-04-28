using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using oed_authz.Interfaces;
using oed_authz.Models;
using oed_authz.Settings;

namespace oed_authz.Controllers;


[ApiController]
[Route("api/pip")]
public class PipController : Controller
{
    private readonly IPolicyInformationPointService _pipService;

    public PipController(IPolicyInformationPointService pipService)
    {
        _pipService = pipService;
    }

    [HttpPost]
    [Authorize(Policy = Constants.AuthorizationPolicyForPlatformAuthorization)]
    public async Task<ActionResult<PipResponse>> Index([FromBody] PipRequest pipRequest)
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
