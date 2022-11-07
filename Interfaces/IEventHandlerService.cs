using oed_authz.Models;

namespace oed_authz.Interfaces;
public interface IEventHandlerService
{
    public Task HandleDaEvent(CloudEventRequestModel daEvent);
}
    