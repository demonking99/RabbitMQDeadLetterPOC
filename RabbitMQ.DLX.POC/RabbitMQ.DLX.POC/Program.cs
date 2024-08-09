using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.DLX.POC;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders(); // Optional: Clears default logging providers
        logging.AddConsole(); // Ensure console logging is enabled
        logging.SetMinimumLevel(LogLevel.Information); // Set minimum log level
    })
    .ConfigureServices((context, services) =>
    {
        //services.AddApplicationInsightsTelemetryWorkerService();
        //services.ConfigureFunctionsApplicationInsights();

        // Configure RabbitMQ settings
        services.Configure<RabbitMqSettings>(context.Configuration.GetSection("RabbitMqSettings"));

        // Register RabbitMQ service
        services.AddSingleton<IRabbitMqService, RabbitMqService>();
        services.AddHostedService<RabbitMqConsumerBackgroundService>();
    })
    .Build();

host.Run();
