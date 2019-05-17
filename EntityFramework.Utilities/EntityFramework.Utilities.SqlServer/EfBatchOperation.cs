using Microsoft.EntityFrameworkCore;

namespace EntityFramework.Utilities.SqlServer
{
    public class EFBatchOperation
    {
        public static ISqlServerBatchOperationBase<TContext, T> For<TContext, T>(TContext context, DbSet<T> set)
            where TContext : DbContext
            where T : class
            {
                return new SqlServerBatchOperation<TContext, T>(context, set);
        }
    }
}
