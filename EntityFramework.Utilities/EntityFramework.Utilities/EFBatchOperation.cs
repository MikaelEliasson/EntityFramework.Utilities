using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;

namespace EntityFramework.Utilities
{

    public interface IEFBatchOperationBase<TContext, T> where T : class
    {
        void InsertAll(IEnumerable<T> items);
        IEFBatchOperationFiltered<TContext, T> Where(Expression<Func<T, bool>> predicate);
    }

    public interface IEFBatchOperationFiltered<TContext, T>
    {
        int Delete();
        int Update<TP>(Expression<Func<T, TP>> prop, Expression<Func<T, TP>> modifier);
    }
    public static class EFBatchOperation
    {
        public static IEFBatchOperationBase<TContext, T> For<TContext, T>(TContext context, IDbSet<T> set)
            where TContext : DbContext
            where T : class
        {
            return EFBatchOperation<TContext, T>.For(context, set);
        }
    }
    public class EFBatchOperation<TContext, T> : IEFBatchOperationBase<TContext, T>, IEFBatchOperationFiltered<TContext, T> where T : class
    {
        private ObjectContext context;
        private IDbSet<T> set;
        private Expression<Func<T, bool>> predicate;

        private EFBatchOperation(TContext context, IDbSet<T> set)
        {
            this.context = (context as IObjectContextAdapter).ObjectContext;
            this.set = set;
        }

        public static IEFBatchOperationBase<TContext, T> For<TContext, T>(TContext context, IDbSet<T> set)
            where TContext : DbContext
            where T : class
        {
            return new EFBatchOperation<TContext, T>(context, set);
        }

        public void InsertAll(IEnumerable<T> items)
        {
            var con = context.Connection as EntityConnection;
            if (con == null)
            {
                Configuration.Log("No provider could be found because the Connection didn't implement System.Data.EntityClient.EntityConnection");
                Fallbacks.DefaultInsertAll(context, items);
            }

            var provider = Configuration.Providers.FirstOrDefault(p => p.CanHandle(con.StoreConnection));
            if (provider != null && provider.CanInsert)
            {
                var set = context.CreateObjectSet<T>();
                var queryInformation = provider.GetQueryInformation<T>(set);
                var sSpaceTables = context.MetadataWorkspace.GetItems<EntityType>(DataSpace.SSpace);
                var oSpaceTables = context.MetadataWorkspace.GetItems<EntityType>(DataSpace.OSpace);
                var sfirst = sSpaceTables.Single(t => t.Name == typeof(T).Name); //Use single to avoid any problems with multiple tables using the same type
                var ofirst = oSpaceTables.Single(t => t.Name == typeof(T).Name); //Use single to avoid any problems with multiple tables using the same type

                var properties = sfirst.Properties.Zip(ofirst.Properties, (s, o) => new ColumnMapping { NameInDatabase = s.Name, NameOnObject = o.Name }).ToList();

                provider.InsertItems(items, queryInformation.Table, properties, con.StoreConnection);
            }
            else
            {
                Configuration.Log("Found provider: " + (provider == null ? "[]" : provider.GetType().Name) + " for " + con.StoreConnection.GetType().Name);
                Fallbacks.DefaultInsertAll(context, items);
            }
        }

        public IEFBatchOperationFiltered<TContext, T> Where(Expression<Func<T, bool>> predicate)
        {
            this.predicate = predicate;
            return this;
        }

        public int Delete()
        {
            var con = context.Connection as EntityConnection;
            if (con == null)
            {
                Configuration.Log("No provider could be found because the Connection didn't implement System.Data.EntityClient.EntityConnection");
                return Fallbacks.DefaultDelete(context, this.predicate);
            }

            var provider = Configuration.Providers.FirstOrDefault(p => p.CanHandle(con.StoreConnection));
            if (provider != null && provider.CanDelete)
            {
                var set = context.CreateObjectSet<T>();
                var query = (ObjectQuery<T>)set.Where(this.predicate);
                var queryInformation = provider.GetQueryInformation<T>(query);

                var delete = provider.GetDeleteQuery(queryInformation);
                var parameters = query.Parameters.Select(p => new SqlParameter { Value = p.Value, ParameterName = p.Name }).ToArray<object>();
                return context.ExecuteStoreCommand(delete, parameters);
            }
            else
            {
                Configuration.Log("Found provider: " + (provider == null ? "[]" : provider.GetType().Name ) + " for " + con.StoreConnection.GetType().Name);
                return Fallbacks.DefaultDelete(context, this.predicate);
            }
        }

        public int Update<TP>(Expression<Func<T, TP>> prop, Expression<Func<T, TP>> modifier)
        {
            var con = context.Connection as EntityConnection;
            if (con == null)
            {
                Configuration.Log("No provider could be found because the Connection didn't implement System.Data.EntityClient.EntityConnection");
                return Fallbacks.DefaultUpdate(context, this.predicate, prop, modifier);
            }

            var provider = Configuration.Providers.FirstOrDefault(p => p.CanHandle(con.StoreConnection));
            if (provider != null && provider.CanUpdate)
            {
                var set = context.CreateObjectSet<T>();

                var query = (ObjectQuery<T>)set.Where(this.predicate);
                var queryInformation = provider.GetQueryInformation<T>(query);

                var updateExpression = ExpressionHelper.CombineExpressions<T, TP>(prop, modifier);

                var mquery = ((ObjectQuery<T>)context.CreateObjectSet<T>().Where(updateExpression));
                var mqueryInfo = provider.GetQueryInformation<T>(mquery);

                var update = provider.GetUpdateQuery(queryInformation, mqueryInfo);
                
                var parameters = query.Parameters
                    .Concat(mquery.Parameters)
                    .Select(p => new SqlParameter { Value = p.Value, ParameterName = p.Name })
                    .ToArray<object>();

                return context.ExecuteStoreCommand(update, parameters);
            }
            else
            {
                Configuration.Log("Found provider: " + (provider == null ? "[]" : provider.GetType().Name) + " for " + con.StoreConnection.GetType().Name);
                return Fallbacks.DefaultUpdate(context, this.predicate, prop, modifier);
            }
        }
    }
}
