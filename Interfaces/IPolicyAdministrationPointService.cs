using oed_authz.Models;

namespace oed_authz.Interfaces;
public interface IPolicyAdministrationPointService
{
    public Task Add(PapRequest papRequest);
    public Task Remove(PapRequest papRequest);
}
