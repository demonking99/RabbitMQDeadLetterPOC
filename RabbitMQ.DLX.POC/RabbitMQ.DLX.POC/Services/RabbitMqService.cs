using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQ.DLX.POC
{
    public class RabbitMqService : IRabbitMqService, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly RabbitMqSettings _settings;

        public RabbitMqService(IOptions<RabbitMqSettings> options)
        {
            _settings = options.Value;

            var factory = new ConnectionFactory
            {
                Uri = new Uri(_settings.ConnectionString)
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        public void SetupQueues()
        {
            // Declare the exchanges
            _channel.ExchangeDeclare(exchange: _settings.ExchangeName, type: "direct");
            _channel.ExchangeDeclare(exchange: _settings.ErrorExchangeName, type: "direct");

            // Declare the primary queue with DLX parameters
            _channel.QueueDeclare(queue: _settings.QueueName,
                                  durable: true,
                                  exclusive: false,
                                  autoDelete: false,
                                  arguments: new Dictionary<string, object>
                                  {
                                      { "x-dead-letter-exchange", _settings.ErrorExchangeName }
                                  });

            // Declare the DLX queue
            _channel.QueueDeclare(queue: _settings.ErrorQueueName,
                                  durable: true,
                                  exclusive: false,
                                  autoDelete: false);

            // Bind the primary queue to the main exchange
            _channel.QueueBind(queue: _settings.QueueName,
                               exchange: _settings.ExchangeName,
                               routingKey: "");

            // Bind the DLX queue to the DLX exchange
            _channel.QueueBind(queue: _settings.ErrorQueueName,
                               exchange: _settings.ErrorExchangeName,
                               routingKey: "");
        }

        public void ConsumeMessages(Func<string, Task<bool>> processMessage)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, messageObject) =>
            {
                var body = messageObject.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                try
                {
                    var success = await processMessage(message);
                    if (success)
                        _channel.BasicAck(messageObject.DeliveryTag, false);
                    else
                        throw new Exception("Processing failed");
                }
                catch (Exception)
                {
                    _channel.BasicNack(messageObject.DeliveryTag, multiple: false, requeue: false);
                }
            };

            _channel.BasicConsume(queue: _settings.QueueName,
                                  autoAck: false,
                                  consumer: consumer);
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}
