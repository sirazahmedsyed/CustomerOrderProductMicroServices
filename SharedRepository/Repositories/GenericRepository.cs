using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using SharedRepository.Audit;
using System.Globalization;
using System.Linq.Expressions;

namespace SharedRepository.Repositories
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {
        protected readonly DbContext _context;
        protected readonly DbSet<TEntity> _dbSet;
        private readonly string dbconnection = "Host=dpg-ctuh03lds78s73fntmag-a.oregon-postgres.render.com;Database=order_management_db;Username=netconsumer;Password=wv5ZjPAcJY8ICgPJF0PZUV86qdKx2r7d";
        public GenericRepository(DbContext context)
        {
            _context = context;
            _dbSet = context.Set<TEntity>();
        }

        public virtual async Task<TEntity> GetByIdAsync(object id)
        {
            return await _dbSet.FindAsync(id);
        }


        public async Task<TEntity> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync(
                Expression<Func<TEntity, bool>> filter = null,
                Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
                Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null)
        {
            IQueryable<TEntity> query = _dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (include != null)
            {
                query = include(query);
            }

            if (orderBy != null)
            {
                return await orderBy(query).ToListAsync();
            }
            else
            {
                return await query.ToListAsync();
            }
        }

        public async Task<TEntity> GetByIdAsync(Guid id, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> include = null)
        {
            IQueryable<TEntity> query = _dbSet;

            if (include != null)
            {
                query = include(query);
            }

            return await query.FirstOrDefaultAsync(e => EF.Property<Guid>(e, "OrderId") == id);
        }


        public virtual async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public virtual async Task AddAsync(TEntity entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        public virtual void Update(TEntity entity)
        {
            _dbSet.Update(entity);
        }

        public virtual void Remove(TEntity entity)
        {
            _dbSet.Remove(entity);
        }

        public virtual void RemoveRange(IEnumerable<TEntity> entities)
        {
            _dbSet.RemoveRange(entities);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task<bool> CustomerExistsAsync(Guid customerId)
        {
            using var connection = new NpgsqlConnection(dbconnection);
            connection.Open();
            Console.WriteLine($"connection opened : {connection}");
            //var connection = _context.Database.GetDbConnection(); // Get the underlying database connection
            var query = "SELECT COUNT(1) FROM \"public\".\"Customers\" WHERE \"CustomerId\" = @CustomerId";
            var result = await connection.QuerySingleAsync<int>(query, new { CustomerId = customerId });
            Console.WriteLine($"CustomerExistsAsync result: {result}");
            return result > 0;
        }

        public async Task<(decimal Price, decimal TaxPercentage)> GetProductDetailsAsync(int productId)
        {
            try
            {
                using var connection = new NpgsqlConnection(dbconnection);
                connection.Open();
                Console.WriteLine($"connection opened : {connection}");
                
                var result = await connection.QuerySingleOrDefaultAsync<(decimal Price, decimal TaxPercentage)>($"SELECT price, tax_percentage FROM products WHERE product_id = '{productId}'");

                if (result == default)
                {
                    throw new ArgumentException("Product does not exist");
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving product details: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<object>> GetTransformedAuditsAsync()
        {
            var audits = await _dbSet.ToListAsync();
            var transformedAudits = new List<object>();

            foreach (var audit in audits)
            {
                if (audit is Auditing auditEntity)
                {
                    var auditData = JsonConvert.DeserializeObject<dynamic>(auditEntity.AuditJson);

                    var transformedAudit = new
                    {
                        OprtnTyp = (int)auditData.OprtnTyp,
                        UsrNm = (string)auditData.UsrNm,
                        UsrNo = (int)auditData.UsrNo,
                        LogDsc = auditData.LogDsc is JArray logDscArray
                                  ? logDscArray.Select(x => x.ToString()).ToList()
                                  : new List<string> { auditData.LogDsc.ToString() },
                        LogTyp = (int)auditData.LogTyp,
                        LogDate = DateTime.ParseExact(auditData.LogDate.ToString(), "dd/MM/yyyy HH:mm:ss.fff", CultureInfo.InvariantCulture)
                                  .ToString("dd/MM/yyyy HH:mm:ss.fff", CultureInfo.InvariantCulture)
                    };
                    transformedAudits.Add(transformedAudit);
                }
            }
            return transformedAudits;
        }
    }
}
