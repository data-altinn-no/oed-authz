using Microsoft.AspNetCore.Mvc;
using oed_authz.Interfaces;
using oed_authz.Models;

namespace oed_authz.Controllers;

[Route("api/eventhandler")]
[ApiController]
public class EventController : Controller
{
    private readonly IAltinnEventHandlerService _altinnEventHandlerService;

    public EventController(IAltinnEventHandlerService altinnEventHandlerService)
    {
        _altinnEventHandlerService = altinnEventHandlerService;
    }

    [HttpPost]
    public async Task<IActionResult> Index([FromBody] CloudEventRequestModel daEvent)
    {
        await _altinnEventHandlerService.HandleDaEvent(daEvent);
        return new OkResult();
    }
}