using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Collections.Generic;

namespace RabbitMQ.DLX.POC
{
    public class ProcessRabbitMQMessages
    {

        private static readonly string RabbitMqHostName = "localhost";
        private static readonly string RabbitMqUserName = "guest";
        private static readonly string RabbitMqPassword = "guest";
        private static readonly string RabbitMqQueueName = "my_queue";
        private static readonly string RabbitMqDlxQueueName = "my_dlx_queue";

        [FunctionName("ProcessRabbitMQMessages")]
        public static void Run([TimerTrigger("0 */1 * * * *")] TimerInfo myTimer, ILogger log)
        {
            var factory = new ConnectionFactory()
            {
                HostName = RabbitMqHostName,
                UserName = RabbitMqUserName,
                Password = RabbitMqPassword
            };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                // Ensure the primary queue exists with DLX configuration
                channel.QueueDeclare(queue: RabbitMqQueueName,
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: new Dictionary<string, object>
                                     {
                                     { "x-dead-letter-exchange", "my_dlx_exchange" }
                                     });

                // Ensure the DLX queue exists
                channel.QueueDeclare(queue: RabbitMqDlxQueueName,
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false);

                // Consume messages from the primary queue
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, messageObject) =>
                {
                    var body = messageObject.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);

                    log.LogInformation($"Received message: {message}");

                    // Simulate processing
                    try
                    {
                        // Process message
                        if (message.Contains("Reject"))
                            throw new Exception("Rejected Message");

                        else
                            channel.BasicAck(messageObject.DeliveryTag, false);
                    }
                    catch (Exception ex)
                    {
                        log.LogError($"Error processing message: {ex.Message}");
                        // Reject the message and move it to DLX
                        channel.BasicNack(messageObject.DeliveryTag, multiple: false, requeue: false);
                    }
                };

                channel.BasicConsume(queue: RabbitMqQueueName,
                                     autoAck: false,
                                     consumer: consumer);

                // To keep the function running to receive messages
                Console.ReadLine();
            }
        }
    }
}
