using System.Reflection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using oed_authz.Interfaces;
using oed_authz.Services;
using oed_authz.Settings;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<ConnectionStrings>(builder.Configuration.GetSection(Constants.ConfigurationSectionConnectionStrings));
builder.Services.Configure<GeneralSettings>(builder.Configuration.GetSection(Constants.ConfigurationSectionGeneralSettings));

builder.Services.AddSingleton<IAltinnEventHandlerService, AltinnEventHandlerService>();
builder.Services.AddSingleton<IOedRoleRepositoryService, OedRoleRepositoryService>();
builder.Services.AddSingleton<IPolicyInformationPointService, PolicyInformationPointService>();

builder.Services.AddControllers();
builder.Services.AddLogging();
builder.Services.AddProblemDetails();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.MetadataAddress =
        builder.Configuration.GetSection(Constants.ConfigurationSectionGeneralSettings)[
            nameof(GeneralSettings.Oauth2WellKnownEndpoint)]!;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        ValidateIssuer = false,
        ValidateAudience = false,
        RequireExpirationTime = true,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Constants.AuthenticationPolicyForPlatformAuthorization, configurePolicy =>
    {
        configurePolicy
            .RequireAuthenticatedUser()
            .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
            .RequireClaim(Constants.AuthenticationClaimTypeApp, Constants.AppPlatformAuthorization)
            .Build();
    });

    options.AddPolicy(Constants.AuthenticationPolicyForPlatformEvents, configurePolicy =>
    {
        configurePolicy
            .RequireAuthenticatedUser()
            .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
            .RequireClaim(Constants.AuthenticationClaimTypeApp, Constants.AppPlatformEvents)
            .Build();
    });
});

if (builder.Environment.IsDevelopment())
{
    IdentityModelEventSource.ShowPII = true;
    builder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly());
}

builder.WebHost.ConfigureKestrel((context, options) =>
{
    options.ListenAnyIP(443, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1AndHttp2AndHttp3;
        listenOptions.UseHttps();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseExceptionHandler("/error-development");
}
else
{
    app.UseExceptionHandler("/error");
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

// TODO Ensure subscription is active upon app start 
app.MapControllers();

app.Run();
