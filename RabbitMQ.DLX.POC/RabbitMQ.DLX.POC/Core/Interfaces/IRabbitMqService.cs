using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQ.DLX.POC
{
    public interface IRabbitMqService
    {
        void SetupQueues();
        void ConsumeMessages(Func<string, Task<bool>> processMessage);
    }
}
