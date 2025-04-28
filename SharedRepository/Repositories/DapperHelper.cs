using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedRepository.Repositories
{
    public class DapperHelper : IDapperHelper
    {
        public async Task<T> QuerySingleOrDefaultAsync<T>(IDbConnection connection, string query, object parameters = null)
        {
            return await connection.QuerySingleOrDefaultAsync<T>(query, parameters);
        }
    }
}
