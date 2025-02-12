using MassTransit;
using Newtonsoft.Json;
using SharedRepository.Repositories;
using SharedRepository.Audit;
using AuditSrvice.API.Infrastructure.DTOs;

namespace AuditSrvice.API.Infrastructure.Consumers
{
    public class AuditConsumer : IConsumer<AuditMessageDto>
    {
        private readonly IAuditRepository _auditRepository;
        private readonly ILogger<AuditConsumer> _logger;

        public AuditConsumer(IAuditRepository auditRepository, ILogger<AuditConsumer> logger)
        {
            _auditRepository = auditRepository;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<AuditMessageDto> context)
        {
            var message = context.Message;

            try
            {
                var auditDto = new AuditLogDto
                {
                    OprtnTyp = message.OprtnTyp,
                    UsrNm = message.UsrNm,
                    UsrNo = message.UsrNo,
                    LogDsc = message.LogDsc,
                    LogTyp = message.LogTyp,
                    LogDate = message.LogDate
                };

                var auditEntry = new Auditing
                {
                    ScreenName = message.ScreenName,
                    ObjectName = message.ObjectName,
                    ScreenPk = message.ScreenPk,
                    AuditJson = JsonConvert.SerializeObject(auditDto, new JsonSerializerSettings
                    {
                        Formatting = Formatting.None,
                        NullValueHandling = NullValueHandling.Ignore,
                        DateFormatString = "dd/MM/yyyy HH:mm:ss.fff"
                    })
                };
                await _auditRepository.AddAsync(auditEntry);
                await _auditRepository.SaveChangesAsync();
                _logger.LogInformation($"Audit record created for {message.ObjectName} with ID {message.ScreenPk}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error saving audit entry for {message.ObjectName} with ID {message.ScreenPk}");
            }
        }
    }
}
