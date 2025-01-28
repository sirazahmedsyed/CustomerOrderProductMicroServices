using AuditSrvice.API.Infrastructure.DBContext;
using SharedRepository.Audit;
using SharedRepository.Repositories;

namespace AuditSrvice.API.Infrastructure.Repository
{
    public class AuditRepository : GenericRepository<Auditing>, IAuditRepository
    {
        public AuditRepository(ApplicationDbContext context) : base(context)
        {
            // Constructor
        }
    }
}
