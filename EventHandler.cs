using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using oed_authz.Interfaces;
using oed_authz.Models;

namespace oed_authz
{
    public class EventHandler
    {
        private readonly IEventHandlerService _eventHandlerService;
        private readonly ILogger _logger;

        public EventHandler(ILoggerFactory loggerFactory, IEventHandlerService eventHandlerService)
        {
            _eventHandlerService = eventHandlerService;
            _logger = loggerFactory.CreateLogger<EventHandler>();
        }

        [Function(nameof(EventHandler))]
        public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
        {

            var daEvent = await req.ReadFromJsonAsync<CloudEventRequestModel>();
            if (daEvent == null)
            {
                _logger.LogError("Unable to deserialize event, was null");
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            await _eventHandlerService.HandleDaEvent(daEvent);

            return req.CreateResponse(HttpStatusCode.OK);
        }
    }
}
