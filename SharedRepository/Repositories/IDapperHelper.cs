using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedRepository.Repositories
{
    public interface IDapperHelper
    {
        Task<T> QuerySingleOrDefaultAsync<T>(IDbConnection connection, string query, object parameters = null);
    }
}
