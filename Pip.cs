using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using oed_authz.Interfaces;
using oed_authz.Models;

namespace oed_authz
{
    public class Pip
    {
        private readonly IPolicyInformationPointService _pipService;
        private readonly ILogger _logger;

        public Pip(ILoggerFactory loggerFactory, IPolicyInformationPointService pipService)
        {
            _pipService = pipService;
            _logger = loggerFactory.CreateLogger<Pip>();
        }

        [Function(nameof(Pip))]
        public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
        {
            var pipRequest = await req.ReadFromJsonAsync<PipRequest>();
            if (pipRequest == null)
            {
                _logger.LogError("Unable to deserialize request, was null");
                return req.CreateResponse(HttpStatusCode.BadRequest);
            }

            var pipResponse = await _pipService.HandlePipRequest(pipRequest);

            var response =  req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(pipResponse);
            return response;
        }
    }
}
