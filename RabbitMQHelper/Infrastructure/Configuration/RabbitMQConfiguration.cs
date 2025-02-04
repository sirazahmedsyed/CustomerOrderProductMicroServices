using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQHelper.Infrastructure.Configuration
{
    public class RabbitMQConfiguration
    {
        public string Host { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int Port { get; set; }
        public string AuditQueueName { get; set; }
        public string AuditExchangeName { get; set; }
    }
}
