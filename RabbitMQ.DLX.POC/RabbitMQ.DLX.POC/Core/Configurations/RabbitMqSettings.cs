namespace RabbitMQ.DLX.POC
{
    public class RabbitMqSettings
    {
        public string ConnectionString { get; set; }
        public string QueueName { get; set; }
        public string ErrorQueueName { get; set; }
        public string ErrorExchangeName { get; set; }
        public string ExchangeName { get;  set; }
    }
}
