using MassTransit;
using Newtonsoft.Json;
using SharedRepository.Audit;
using SharedRepository.Repositories;

namespace AuditSrvice.API.Infrastructure.Consumers
{
    public class OptimizedAuditConsumer : IConsumer<AuditMessage>
    {
        private readonly ILogger<OptimizedAuditConsumer> _logger;
        private readonly IGenericRepository<Auditing> _auditRepository;

        public OptimizedAuditConsumer(ILogger<OptimizedAuditConsumer> logger, IGenericRepository<Auditing> auditRepository)
        {
            _logger = logger;
            _auditRepository = auditRepository;
        }

        public async Task Consume(ConsumeContext<AuditMessage> context)
        {
            var auditMessage = context.Message;

            // Transform the AuditMessage into the desired JSON format
            var auditJson = new
            {
                oprtnTyp = auditMessage.OprtnTyp,
                usrNm = auditMessage.UsrNm,
                usrNo = auditMessage.UsrNo,
                logDsc = auditMessage.LogDsc,
                logTyp = auditMessage.LogTyp,
                logDate = auditMessage.LogDate.ToString("dd/MM/yyyy HH:mm:ss.fff")
            };

            // Serialize the object to JSON
            var auditJsonString = JsonConvert.SerializeObject(auditJson);

            // Create the Auditing entity
            var auditingEntity = new Auditing
            {
                ScreenName = auditMessage.ScreenName,
                ObjectName = auditMessage.ObjectName,
                ScreenPk = auditMessage.ScreenPk,
                AuditJson = auditJsonString
            };

            // Save the entity to the database
            await _auditRepository.AddAsync(auditingEntity);
            await _auditRepository.SaveChangesAsync();

            _logger.LogInformation("Audit message consumed and saved to the database: {AuditJson}", auditJsonString);
        }
    }
}