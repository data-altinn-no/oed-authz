using Microsoft.AspNetCore.Mvc;
using oed_authz.Interfaces;
using oed_authz.Models;

namespace oed_authz.Controllers;

[Route("api/pip")]
[ApiController]
public class PipController : Controller
{
    private readonly IPolicyInformationPointService _pipService;

    public PipController(IPolicyInformationPointService pipService)
    {
        _pipService = pipService;
    }

    [HttpPost]
    public async Task<ActionResult<PipResponse>> Index([FromBody] PipRequest pipRequest)
    {
        var pipResponse = await _pipService.HandlePipRequest(pipRequest);
        return new OkObjectResult(pipResponse);
    }
}