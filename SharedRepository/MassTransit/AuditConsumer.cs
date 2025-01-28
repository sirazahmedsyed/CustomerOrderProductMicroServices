using MassTransit;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SharedRepository.Audit;
using SharedRepository.Repositories;

namespace SharedRepository.MassTransit
{
    public class AuditConsumer : IConsumer<AuditMessage>
    {
        private readonly IGenericRepository<Auditing> _auditRepository;
        private readonly ILogger<AuditConsumer> _logger;

        public AuditConsumer(IGenericRepository<Auditing> auditRepository, ILogger<AuditConsumer> logger)
        {
            _auditRepository = auditRepository;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<AuditMessage> context)
        {
            var message = context.Message;

            try
            {
                var auditEntry = new Auditing
                {
                    ScreenName = message.ScreenName,
                    ObjectName = message.ObjectName,
                    ScreenPk = message.ScreenPk,
                    AuditJson = JsonConvert.SerializeObject(message)
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
