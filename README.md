# RabbitMQDeadLetterPOC

## 1. Define the Exchanges
Ensure you create both the main exchange and the DLX exchange.
```shell
rabbitmqadmin declare exchange name=my_exchange type=direct
rabbitmqadmin declare exchange name=my_dlx_exchange type=direct
```
## 2. Define the Queues
Create the primary queue with DLX parameters and the DLX queue:
```shell
rabbitmqadmin declare queue name=my_queue durable=true arguments='{"x-dead-letter-exchange":"my_dlx_exchange"}'
rabbitmqadmin declare queue name=my_dlx_queue durable=true
```
## 3. Bind the Queues to the Exchanges
Bind the primary queue to the main exchange and the DLX queue to the DLX exchange:
```shell
rabbitmqadmin declare binding source=my_exchange destination=my_queue
rabbitmqadmin declare binding source=my_dlx_exchange destination=my_dlx_queue
```

### Check Code inside for AzureFunction Implementation.

## local.settings.json
```json
{
    "IsEncrypted": false,
    "Values": {
        "AzureWebJobsStorage": "UseDevelopmentStorage=true",
        "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
        "RabbitMqSettings__ConnectionString": "amqp://guest:guest@localhost:5672/",
        "RabbitMqSettings__QueueName": "my_queue",
        "RabbitMqSettings__ExchangeName": "my_exchange",
        "RabbitMqSettings__ErrorQueueName": "my_dlx_queue",
        "RabbitMqSettings__ErrorExchangeName": "my_dlx_exchange"
    }
}
```