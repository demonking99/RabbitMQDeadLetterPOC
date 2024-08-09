using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
namespace RabbitMQ.DLX.POC.Functions
{
    public class ProcessRabbitMQMessages
    {
        private readonly IRabbitMqService _rabbitMqService;
        private readonly ILogger<ProcessRabbitMQMessages> _logger;

        public ProcessRabbitMQMessages(IRabbitMqService rabbitMqService, ILogger<ProcessRabbitMQMessages> logger)
        {
            _rabbitMqService = rabbitMqService;
            _logger = logger;
        }


        [Function("ProcessRabbitMQMessages")]
        public void Run([TimerTrigger("0 */1 * * * *")] TimerInfo myTimer, ILogger log)
        {
            try
            {
                _rabbitMqService.SetupQueues();

                _rabbitMqService.ConsumeMessages(async (message) =>
                {
                    _logger.LogInformation($"Received message: {message}");

                    // Simulate processing
                    if (message.ToUpper().Contains("Reject"))
                    {
                        _logger.LogError($"Message processing failed: {message}");
                        return false;
                    }

                    return true;
                });
                // To keep the function running to receive messages
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing messages: {ex.Message}");
            }
        }
    }
}
