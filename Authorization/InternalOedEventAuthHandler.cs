using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using oed_authz.Settings;

namespace oed_authz.Authorization;

public class InternalOedEventAuthHandler : AuthorizationHandler<InternalOedEventAuthRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly GeneralSettings _settings;
    private readonly Secrets _secrets;

    public InternalOedEventAuthHandler(IHttpContextAccessor httpContextAccessor, IOptions<Secrets> secrets, IOptions<GeneralSettings> settings)
    {
        _httpContextAccessor = httpContextAccessor;
        _settings = settings.Value;
        _secrets = secrets.Value;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, InternalOedEventAuthRequirement requirement)
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext is not null && httpContext.Request.Query.TryGetValue(_settings.OedEventAuthQueryParameter, out var queryValue))
        {
            if (queryValue == _secrets.OedEventAuthKey)
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }
        }
        else
        {
            context.Fail();
        }

        return Task.CompletedTask;
    }
}
