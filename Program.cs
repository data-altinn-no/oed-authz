using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using oed_authz.Interfaces;
using oed_authz.Services;
using oed_authz.Settings;


var host = new HostBuilder()
    .ConfigureAppConfiguration((hostContext, config) =>
    {
        config
            .AddEnvironmentVariables()
            .AddJsonFile("worker.json");

        if (hostContext.HostingEnvironment.IsDevelopment())
        {
            config.AddUserSecrets<EventHandler>(false);
        }
    })
    // TODO Workaround for https://github.com/Azure/azure-functions-dotnet-worker/issues/1090
    .ConfigureLogging(loggingBuilder =>
    {
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
        {
            loggingBuilder.AddSimpleConsole(options =>
            {
                options.ColorBehavior = LoggerColorBehavior.Enabled;
                options.SingleLine = true;
            });
        }
    })
    .ConfigureFunctionsWorkerDefaults(builder =>
    {
        builder
            // Using preview package Microsoft.Azure.Functions.Worker.ApplicationInsights, see https://github.com/Azure/azure-functions-dotnet-worker/pull/944
            // Requires APPLICATIONINSIGHTS_CONNECTION_STRING being set. Note that host.json logging settings will have to be replicated to worker.json
            .AddApplicationInsights()
            .AddApplicationInsightsLogger();

    }, options =>
    {
        //options.Serializer = new NewtonsoftJsonObjectSerializer();
    })
    .ConfigureServices((context, services) =>
    {
        services.Configure<ConnectionStrings>(context.Configuration.GetSection("ConnectionStrings"));

        services.AddSingleton<IEventHandlerService, EventHandlerService>();
        services.AddSingleton<IOedRoleRepositoryService, OedRoleRepositoryService>();
        services.AddSingleton<IPolicyInformationPointService, PolicyInformationPointService>();

    })
    .Build();

host.Run();

