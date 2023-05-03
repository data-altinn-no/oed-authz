using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using oed_authz.Authorization;
using oed_authz.Helpers;
using oed_authz.Interfaces;
using oed_authz.Services;
using oed_authz.Settings;
using Yuniql.AspNetCore;
using Yuniql.PostgreSql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<Secrets>(builder.Configuration.GetSection(Constants.ConfigurationSectionSecrets));
builder.Services.Configure<GeneralSettings>(builder.Configuration.GetSection(Constants.ConfigurationSectionGeneralSettings));

builder.Services.AddSingleton<IAltinnEventHandlerService, AltinnEventHandlerService>();
builder.Services.AddSingleton<IOedRoleRepositoryService, OedRoleRepositoryService>();
builder.Services.AddSingleton<IPolicyInformationPointService, PolicyInformationPointService>();
builder.Services.AddScoped<IAuthorizationHandler, InternalOedEventAuthHandler>();

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddLogging();
builder.Services.AddProblemDetails();
builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = Constants.AuthenticationSchemeInternal;
    })
    // Add support for the Oauth2 with Altinn as issuer for internal requests
    .AddJwtBearer(Constants.AuthenticationSchemeInternal, options =>
    {
        options.MetadataAddress =
            builder.Configuration.GetSection(Constants.ConfigurationSectionGeneralSettings)[
                nameof(GeneralSettings.AltinnOauth2WellKnownEndpoint)]!;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidateIssuer = false,
            ValidateAudience = false,
            RequireExpirationTime = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    })
    // Add support for Oauth2 with Maskinporten as issuer for external requests
    .AddJwtBearer(Constants.AuthenticationSchemeExternal, options =>
    {
        options.MetadataAddress =
            builder.Configuration.GetSection(Constants.ConfigurationSectionGeneralSettings)[
                nameof(GeneralSettings.MaskinportenOauth2WellKnownEndpoint)]!;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidateIssuer = false,
            ValidateAudience = false,
            RequireExpirationTime = false,
            ValidateLifetime = false,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization(options =>
{
    // Token/claim-based policy for platform requests to the PIP api
    options.AddPolicy(Constants.AuthorizationPolicyForPlatformAuthorization, configurePolicy =>
    {
        configurePolicy
            .RequireAuthenticatedUser()
            .AddAuthenticationSchemes(Constants.AuthenticationSchemeInternal)
            .RequireClaim(Constants.TokenClaimTypeApp, Constants.AppPlatformAuthorization)
            .Build();
    });

    // Secret-in-header based policy for internal requests to the events endpoint (sent from oed-inbound)
    options.AddPolicy(Constants.AuthorizationPolicyForEvents, configurePolicy =>
    {
        configurePolicy.Requirements.Add(new InternalOedEventAuthRequirement());
        configurePolicy.Build();
    });

    // Maskinporten scope requirements for external requests.
    options.AddPolicy(Constants.AuthorizationPolicyForExternals, configurePolicy =>
    {
        configurePolicy
            .RequireAuthenticatedUser()
            .AddAuthenticationSchemes(Constants.AuthenticationSchemeExternal)
            .RequireClaim(Constants.TokenClaimTypeScope, Constants.ScopeProbateOnly, Constants.ScopeAllRoles)
            .Build();
    });

    // Maskinporten scope requirements for internal DD apps.
    options.AddPolicy(Constants.AuthorizationPolicyForDdApp, configurePolicy =>
    {
        configurePolicy
            .RequireAuthenticatedUser()
            .AddAuthenticationSchemes(Constants.AuthenticationSchemeExternal)
            .RequireClaim(Constants.TokenClaimTypeScope, Constants.ScopeInternal)
            .Build();
    });
});

if (builder.Environment.IsDevelopment())
{
    IdentityModelEventSource.ShowPII = true;
    builder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly());
}

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseExceptionHandler(app.Environment.IsDevelopment() ? "/error-development" : "/error");

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

var traceService = new ConsoleTraceService { IsDebugEnabled = true };
app.UseYuniql(
    new PostgreSqlDataService(traceService),
    new PostgreSqlBulkImportService(traceService),
    traceService,
    new Configuration
    {
        Workspace = Path.Combine(Environment.CurrentDirectory, "Migrations"),
        ConnectionString = builder.Configuration.GetSection(Constants.ConfigurationSectionSecrets)[
            nameof(Secrets.PostgreSqlAdminConnectionString)]!,
        IsAutoCreateDatabase = false,
        IsDebug = true
    });

app.Run();
