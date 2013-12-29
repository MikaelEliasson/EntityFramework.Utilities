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
using System.Reflection;
using System.Text.RegularExpressions;

namespace EntityFramework.Utilities
{
    public static class DbExtensions
    {

        public static T SetTo<T>(this T source, T value)
        {
            return value;
        }

        /// <summary>
        /// Inserts all items in a batch using the SqlBulkCopy class. NOTE: There is no contraint checking
        /// </summary>
        public static void InsertAll<T>(this DbContext source, IEnumerable<T> items) where T : class
        {
            var context = (source as IObjectContextAdapter).ObjectContext;
            var con = context.Connection as EntityConnection;
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
            var context = (source as IObjectContextAdapter).ObjectContext;
            string update = null;
            var updateParameters = new List<SqlParameter>();
            if (modifier.Body.NodeType == ExpressionType.Convert)
            {
                var expr = modifier.Body as UnaryExpression;
                if (expr.Operand is MethodCallExpression)
                {
                    var statement = expr.Operand as MethodCallExpression;
                    if (statement.Method.Name == "SetTo" && statement.Method.DeclaringType == typeof(DbExtensions))
                    {
                        var name = ((MemberExpression)statement.Arguments[0]).Member.Name;

                        var storageName = GetStorageTranslatedName<T>(context, name);
                        
                        var arg1 = statement.Arguments[1];
                        var parameter = ExtractValue(arg1);
                        updateParameters.Add(new SqlParameter("update_1", parameter));
                        update = string.Format("[{0}] = @update_1", storageName);
                    }
                    else
                    {
                        throw new NotSupportedException("UpdateAll does not support method calls");
                    }

                }
                else
                {
                    var statement = expr.Operand as BinaryExpression;
                    update = ParseRelativeUpdate<T>(context, update, statement, updateParameters);
                }
            }
            else if (modifier.Body is BinaryExpression)
            {
                update = ParseRelativeUpdate<T>(context, update, modifier.Body as BinaryExpression, updateParameters);
            }
            else
            {
                throw new NotSupportedException("UpdateAll does not support modifiers of type:" + modifier.GetType().Name);
            }

            var query = ((ObjectQuery<T>)context.CreateObjectSet<T>().Where(predicate));
            var queryInfo = GetQueryInformation<T>(query);

            var parameters = query.Parameters
                .Select(p => new SqlParameter { Value = p.Value, ParameterName = p.Name })
                .Concat(updateParameters)
                .ToArray<object>();

            var result = context.ExecuteStoreCommand(string.Format("UPDATE [{0}].[{1}] SET {2} {3}", queryInfo.Schema, queryInfo.Table, update, queryInfo.WhereSql), parameters);
            return result;
        }

        private static string GetStorageTranslatedName<T>(ObjectContext ctx, string name) where T : class
        {
            var set = ctx.CreateObjectSet<T>();
            var queryInformation = GetQueryInformation<T>(set);
            var sSpaceTables = ctx.MetadataWorkspace.GetItems<EntityType>(DataSpace.SSpace);
            var oSpaceTables = ctx.MetadataWorkspace.GetItems<EntityType>(DataSpace.OSpace);
            var sfirst = sSpaceTables.Single(t => t.Name == typeof(T).Name); //Use single to avoid any problems with multiple tables using the same type
            var ofirst = oSpaceTables.Single(t => t.Name == typeof(T).Name); //Use single to avoid any problems with multiple tables using the same type

            var index = ofirst.DeclaredProperties.Select(p => p.Name).ToList().IndexOf(name);
            var storageName = sfirst.DeclaredProperties[index].Name;
            return storageName;
        }

        private static string ParseRelativeUpdate<T>(ObjectContext source, string update, BinaryExpression statement, List<SqlParameter> updateParameters) where T : class
        {
            var left = statement.Left as MemberExpression;
            if (left == null)
            {
                throw new InvalidOperationException("The member operator has to be to the left. For example b => b.Reads + 5 will work but not b => 5 + b.Reads");
            }

            var fieldName = left.Member.Name;
            fieldName = GetStorageTranslatedName<T>(source, fieldName);
            var parameter = ExtractValue(statement.Right);
            var op = GetOperation(statement.NodeType);

            updateParameters.Add(new SqlParameter("update_1", parameter));
            update = string.Format("[{0}] = {0} {1} @update_1", fieldName, op);
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

        // http://stackoverflow.com/questions/1527363/reflection-vs-compile-to-get-the-value-of-memberexpression
        private static object ExtractValue(Expression expression)
        {
            if (expression == null)
            {
                return null;
            }

            var ce = expression as ConstantExpression;
            if (ce != null)
            {
                return ce.Value;
            }

            var ma = expression as MemberExpression;
            if (ma != null)
            {
                var se = ma.Expression;
                object val = null;
                if (se != null)
                {
                    val = ExtractValue(se);
                }

                var fi = ma.Member as FieldInfo;
                if (fi != null)
                {
                    return fi.GetValue(val);
                }
                else
                {
                    var pi = ma.Member as PropertyInfo;
                    if (pi != null)
                    {
                        return pi.GetValue(val, null);
                    }
                }
            }

            var mce = expression as MethodCallExpression;
            if (mce != null)
            {
                return mce.Method.Invoke(ExtractValue(mce.Object), mce.Arguments.Select(ExtractValue).ToArray());
            }

            var le = expression as LambdaExpression;
            if (le != null)
            {
                if (le.Parameters.Count == 0)
                {
                    return ExtractValue(le.Body);
                }
                else
                {
                    return le.Compile().DynamicInvoke();
                }
            }

            var dynamicInvoke = Expression.Lambda(expression).Compile().DynamicInvoke();
            return dynamicInvoke;
        }
    }
}
