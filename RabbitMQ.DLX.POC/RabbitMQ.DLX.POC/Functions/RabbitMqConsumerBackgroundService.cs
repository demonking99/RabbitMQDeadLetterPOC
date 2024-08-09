using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace RabbitMQ.DLX.POC
{
    public class RabbitMqConsumerBackgroundService : BackgroundService
    {
        private readonly IRabbitMqService _rabbitMqService;
        private readonly ILogger<RabbitMqConsumerBackgroundService> _logger;

        public RabbitMqConsumerBackgroundService(IRabbitMqService rabbitMqService, ILogger<RabbitMqConsumerBackgroundService> logger)
        {
            _rabbitMqService = rabbitMqService;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            _logger.LogInformation("RabbitMQ Consumer Service started."); // Log service start
            // Set up queues using the existing method
            _rabbitMqService.SetupQueues();

            // Start consuming messages using the existing ConsumeMessages method
            _rabbitMqService.ConsumeMessages(async message =>
            {
                try
                {
                    _logger.LogInformation($"Received message: {message}");

                    // Implement your message processing logic here
                    bool result = await ProcessMessage(message);

                    _logger.LogInformation($"Message processed: {result}");

                    return result;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Message processing failed: {ex.Message}");
                    return false;
                }
            });

            return Task.CompletedTask;
        }
        private Task<bool> ProcessMessage(string message)
        {
            _logger.LogInformation($"Processing message: {message}");
            // Implement your message processing logic here

            if (message.ToLower().Contains("reject"))
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(true); // Return true if successful, false otherwise
        }

        public override void Dispose()
        {
            _rabbitMqService.Dispose();
            base.Dispose();
        }
    }
}
