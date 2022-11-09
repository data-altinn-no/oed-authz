using Microsoft.AspNetCore.Mvc;
using Npgsql;
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
        try
        {
            await _altinnEventHandlerService.HandleDaEvent(daEvent);
        }
        catch (ArgumentException ex)
        {
            return Problem(
                title: "Bad Input",
                detail: ex.GetType().Name + ": " + ex.Message,
                statusCode: StatusCodes.Status400BadRequest
            );
        }
        catch (PostgresException ex)
        {
            if (ex.Message.Contains("duplicate"))
            {
                // Handle duplicate role assignments gracefully 
                return new OkResult();
            }

            throw;
        }
        
        return new OkResult();
    }
}