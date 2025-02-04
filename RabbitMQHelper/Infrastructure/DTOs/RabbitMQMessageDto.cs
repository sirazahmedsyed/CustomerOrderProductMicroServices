using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQHelper.Infrastructure.DTOs
{
    public class RabbitMQMessageDto
    {
        public string MessageType { get; set; }
        public DateTime MessageDate { get; set; }
        //public string ExchangeName { get; set; }
        //public string QueueName { get; set; }
        public object Payload { get; set; }
    }
}
