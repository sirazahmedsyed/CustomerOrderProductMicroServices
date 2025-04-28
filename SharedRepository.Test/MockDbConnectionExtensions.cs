using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Dapper;
using Moq;
using Moq.Language.Flow;

namespace SharedRepository.Test
{
    public static class MockDbConnectionExtensions
    {
        public static ISetup<IDbConnection, Task<T>> SetupDapperAsync<T>(
            this Mock<IDbConnection> mock,
            Expression<Func<IDbConnection, Task<T>>> expression)
        {
            return mock.Setup(expression);
        }

        public static ISetup<IDbConnection, Task<int>> SetupDapperAsync(
            this Mock<IDbConnection> mock,
            Expression<Func<IDbConnection, Task<int>>> expression)
        {
            return mock.Setup(expression);
        }

        public static ISetup<IDbConnection, Task<IEnumerable<T>>> SetupDapperAsync<T>(
            this Mock<IDbConnection> mock,
            Expression<Func<IDbConnection, Task<IEnumerable<T>>>> expression)
        {
            return mock.Setup(expression);
        }
    }
}
