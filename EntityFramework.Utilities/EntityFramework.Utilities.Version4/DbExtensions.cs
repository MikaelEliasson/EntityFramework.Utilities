using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Metadata.Edm;
using System.Data.Objects;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace EntityFramework.Utilities.Version4
{
    public static class DbExtensions
    {

        /// <summary>
        /// Inserts all items in a batch using the SqlBulkCopy class. NOTE: There is no contraint checking
        /// </summary>
        public static void InsertAll<T>(this DbContext source, IEnumerable<T> items) where T : class
        {


            var context = (source as IObjectContextAdapter).ObjectContext;
            var con = context.Connection as System.Data.EntityClient.EntityConnection;
            if (con == null)
            {
                Configuration.Log("No provider could be found because the Connection didn't implement System.Data.EntityClient.EntityConnection");
                DefaultInsertAll(context, items);
            }

            var provider = Configuration.Providers.FirstOrDefault(p => p.CanHandle(con.StoreConnection));
            if (provider != null && provider.CanInsert)
            {
                var set = context.CreateObjectSet<T>();
                var queryInformation = GetQueryInformation<T>(set);
                var sSpaceTables = context.MetadataWorkspace.GetItems<EntityType>(DataSpace.SSpace);
                var oSpaceTables = context.MetadataWorkspace.GetItems<EntityType>(DataSpace.OSpace);
                var sfirst = sSpaceTables.Single(t => t.Name == typeof(T).Name); //Use single to avoid any problems with multiple tables using the same type
                var ofirst = oSpaceTables.Single(t => t.Name == typeof(T).Name); //Use single to avoid any problems with multiple tables using the same type

                var properties = sfirst.Properties.Zip(ofirst.Properties, (s, o) => new ColumnMapping { NameInDatabase = s.Name, NameOnObject = o.Name }).ToList();

                provider.InsertItems(items, queryInformation.Table, properties, con.StoreConnection);
            }
            else
            {
                DefaultInsertAll(context, items);
            }

        }

        private static void DefaultInsertAll<T>(ObjectContext context, IEnumerable<T> items) where T : class
        {
            if (Configuration.DisableDefaultFallback)
            {
                throw new InvalidOperationException("No provider supporting the InsertAll operation for this datasource was found");
            }
            Configuration.Log("No provider found. Using default insert method");

            var set = context.CreateObjectSet<T>();
            foreach (var item in items)
            {
                set.AddObject(item);
            }
            context.SaveChanges();
        }

        /// <summary>
        /// Creates a query against the database to delete all entities matching the predicate without first loading them into memory. Results in a single database call. 
        /// NOTE: There is no contraint checking and the context might be left in a stale state
        /// </summary>
        /// <param name="predicate">A lambda like p => p.Created > someDate . Might not work with joins</param>
        /// <returns>The number of rows affected</returns>
        public static int DeleteAll<T>(this DbContext source, Expression<Func<T, bool>> predicate) where T : class
        {

            var context = (source as IObjectContextAdapter).ObjectContext;
            var query = ((ObjectQuery<T>)context.CreateObjectSet<T>().Where(predicate));

            var queryInfo = GetQueryInformation<T>(query);

            var parameters = query.Parameters.Select(p => new SqlParameter { Value = p.Value, ParameterName = p.Name }).ToArray<object>();

            var result = context.ExecuteStoreCommand(string.Format("DELETE FROM [{0}].[{1}] {2}", queryInfo.Schema, queryInfo.Table, queryInfo.WhereSql), parameters);
            return result;
        }

        /// <summary>
        /// Creates a query against the database to update all entities matching the predicate without first loading them into memory. Results in a single database call. 
        /// NOTE: There is no contraint checking and the context might be left in a stale state
        /// </summary>
        /// <param name="predicate">A lambda like p => p.Created > someDate . Might not work with joins</param>
        /// <param name="modifier">Simple operations like p => p.Reads * 2</param>
        /// <returns>The number of rows affected</returns>
        public static int UpdateAll<T>(this DbContext source, Expression<Func<T, bool>> predicate, Expression<Func<T, object>> modifier) where T : class
        {

            string update = null;
            if (modifier.Body.NodeType == ExpressionType.Convert)
            {
                var expr = modifier.Body as UnaryExpression;
                var statement = expr.Operand as BinaryExpression;

                update = ParseUpdate(update, statement);
            }
            else if (modifier.Body is BinaryExpression)
            {
                update = ParseUpdate(update, modifier.Body as BinaryExpression);
            }
            else
            {
                throw new NotSupportedException("UpdateAll do not support modifiers of type:" + modifier.GetType().Name);
            }

            var context = (source as IObjectContextAdapter).ObjectContext;
            var query = ((ObjectQuery<T>)context.CreateObjectSet<T>().Where(predicate));
            var queryInfo = GetQueryInformation<T>(query);

            var parameters = query.Parameters.Select(p => new SqlParameter { Value = p.Value, ParameterName = p.Name }).ToArray<object>();

            var result = context.ExecuteStoreCommand(string.Format("UPDATE [{0}].[{1}] SET {2} {3}", queryInfo.Schema, queryInfo.Table, update, queryInfo.WhereSql), parameters);
            return result;
        }

        private static string ParseUpdate(string update, BinaryExpression statement)
        {
            var left = statement.Left as MemberExpression;
            if (left == null)
            {
                throw new InvalidOperationException("The member operator has to be to the left. For example b => b.Reads + 5 will work but not b => 5 + b.Reads");
            }
            var right = statement.Right as ConstantExpression;

            var fieldName = "[" + left.Member.Name + "]";
            var parameter = right.Value;
            if (parameter is string)
            {
                parameter = "'" + parameter + "'";
            }
            var op = GetOperation(statement.NodeType);

            update = string.Format("{0} = {0} {1} {2}", fieldName, op, parameter);
            return update;
        }

        private static QueryInformation GetQueryInformation<T>(ObjectQuery<T> query) where T : class
        {
            var fromRegex = new Regex(@"FROM \[([^\]]+)\]\.\[([^\]]+)\] AS (\[[^\]]+\])", RegexOptions.IgnoreCase);

            var queryInfo = new QueryInformation();

            var str = query.ToTraceString();
            var match = fromRegex.Match(str);
            queryInfo.Schema = match.Groups[1].Value;
            queryInfo.Table = match.Groups[2].Value;
            queryInfo.Alias = match.Groups[3].Value;

            var i = str.IndexOf("WHERE");
            if (i > 0)
            {
                var whereClause = str.Substring(i);
                queryInfo.WhereSql = whereClause.Replace(queryInfo.Alias + ".", "");
            }
            return queryInfo;
        }


        private static string GetOperation(ExpressionType type)
        {
            switch (type)
            {
                case ExpressionType.Add:
                    return "+";
                case ExpressionType.Subtract:
                    return "-";
                case ExpressionType.Multiply:
                    return "*";
                case ExpressionType.Divide:
                    return "/";
                default:
                    throw new NotImplementedException("Updates with ExpressionType " + type + " is not supported");
            }
        }
    }
}
