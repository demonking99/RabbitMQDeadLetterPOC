using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.DLX.POC;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((context, services) =>
    {
        //services.AddApplicationInsightsTelemetryWorkerService();
        //services.ConfigureFunctionsApplicationInsights();

        // Configure RabbitMQ settings
        services.Configure<RabbitMqSettings>(context.Configuration.GetSection("RabbitMqSettings"));

        // Register RabbitMQ service
        services.AddSingleton<IRabbitMqService, RabbitMqService>();
    })
    .Build();

host.Run();
