using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using oed_authz.Interfaces;
using oed_authz.Models;

namespace oed_authz
{
    public class AltinnEventHandler
    {
        private readonly IAltinnEventHandlerService _altinnEventHandlerService;
        private readonly ILogger _logger;

        public AltinnEventHandler(ILoggerFactory loggerFactory, IAltinnEventHandlerService altinnEventHandlerService)
        {
            _altinnEventHandlerService = altinnEventHandlerService;
            _logger = loggerFactory.CreateLogger<AltinnEventHandler>();
        }

        [Function(nameof(AltinnEventHandler))]
        public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
        {

            var daEvent = await req.ReadFromJsonAsync<CloudEventRequestModel>();
            if (daEvent == null)
            {
                _logger.LogError("Unable to deserialize event, was null");
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            await _altinnEventHandlerService.HandleDaEvent(daEvent);

            return req.CreateResponse(HttpStatusCode.OK);
        }
    }
}
