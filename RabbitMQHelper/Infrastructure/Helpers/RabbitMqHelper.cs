using MassTransit;
using RabbitMQHelper.Infrastructure.DTOs;

namespace RabbitMQHelper.Infrastructure.Helpers
{
    public class RabbitMqHelper : IRabbitMQHelper
    {
        private readonly IPublishEndpoint _publishEndpoint;
        public RabbitMqHelper(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public async Task<bool> AuditResAsync(AuditMessageDto auditDto)
        {
            try
            {
                var auditMessage = new AuditMessageDto
                {
                    OprtnTyp = auditDto.OprtnTyp,
                    UsrNm = auditDto.UsrNm,
                    UsrNo = auditDto.UsrNo,
                    LogDsc = auditDto.LogDsc,
                    LogTyp = auditDto.LogTyp,
                    LogDate = auditDto.LogDate,
                    ScreenName = auditDto.ScreenName,
                    ObjectName = auditDto.ObjectName,
                    ScreenPk = auditDto.ScreenPk
                };

                await _publishEndpoint.Publish(auditMessage, context =>
                {
                    context.SetRoutingKey("audit.message");
                });

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error publishing audit message: {ex.Message}");
                return false;
            }
        }
    }
}
