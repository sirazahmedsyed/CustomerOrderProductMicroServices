using Microsoft.AspNetCore.Mvc;
using SharedRepository.Audit;
using SharedRepository.Repositories;

namespace AuditSrvice.API.Infrastructure.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuditsController : ControllerBase
    {
        private readonly IGenericRepository<Auditing> _auditRepository;
        private readonly ILogger<AuditsController> _logger;
        public AuditsController(IGenericRepository<Auditing> auditRepository, ILogger<AuditsController> logger)
        {
            _auditRepository = auditRepository;
            _logger = logger;
        }

        [HttpGet]
        [Route("GetTransformedAudits")]
        public async Task<IActionResult> GetTransformedAudits()
        {
            try
            {
                _logger.LogInformation("Getting transformed audits");
                var transformedAudits = await _auditRepository.GetTransformedAuditsAsync();
                return Ok(transformedAudits);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting transformed audits");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        [Route("GetAllAudits")]
        public async Task<IActionResult> GetAllAudits()
        {
            try
            {
                _logger.LogInformation("Getting all audits");
                var audits = await _auditRepository.GetAllAsync();
                return Ok(audits);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all audits");
                return StatusCode(500, "Internal server error");
            }

        }
        [HttpGet]
        [Route("GetAuditById/{id:guid}")]
        public async Task<IActionResult> GetAuditById(Guid id)
        {
            try
            {
                var audit = await _auditRepository.GetByIdAsync(id);
                if (audit == null)
                    return NotFound();

                return Ok(audit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting audit with ID {AuditId}", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
