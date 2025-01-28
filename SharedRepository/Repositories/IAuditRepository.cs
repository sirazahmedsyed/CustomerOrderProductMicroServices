using SharedRepository.Audit;

namespace SharedRepository.Repositories
{
    public interface IAuditRepository : IGenericRepository<Auditing>
    {
        // You can add any specific methods for Audit entity if needed
    }
}
