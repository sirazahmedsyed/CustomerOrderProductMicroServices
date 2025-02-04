using MassTransit;
using Newtonsoft.Json;
using RabbitMQHelper.Infrastructure.DTOs;

namespace RabbitMQHelper.Infrastructure.Helpers
{
    public class RabbitMqHelper : IRabbitMQHelper
    {
        private readonly IPublishEndpoint _publishEndpoint;
        //private const string AUDIT_EXCHANGE = "audit_exchange";
        //private const string AUDIT_QUEUE = "audit_queue";

        public RabbitMqHelper(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public async Task<bool> AuditResAsync(AuditMessageDto auditDto)
        {
            try
            {
                // Create the RabbitMQ message
                var rabbitMessage = new RabbitMQMessageDto
                {
                    MessageType = "AuditMessage",
                    MessageDate = DateTime.UtcNow,
                    //ExchangeName = AUDIT_EXCHANGE,
                    //QueueName = AUDIT_QUEUE,
                    Payload = new
                    {
                        OperationType = auditDto.OprtnTyp,
                        UserName = auditDto.UsrNm,
                        UserNumber = auditDto.UsrNm,
                        LogDescription = auditDto.LogDsc,
                        LogType = auditDto.LogTyp,
                        LogDate = auditDto.LogDate,
                        ScreenName = auditDto.ScreenName,
                        ObjectName = auditDto.ObjectName,
                        ScreenPk = auditDto.ScreenPk
                    }
                };

                // Publish the message
                await _publishEndpoint.Publish(rabbitMessage);
                
                return true;
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error publishing audit message: {ex.Message}");
                return false;
            }
        }
    }
}
