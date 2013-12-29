using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;

namespace EntityFramework.Utilities
{
    class Fallbacks
    {
        internal static void DefaultInsertAll<T>(ObjectContext context, IEnumerable<T> items) where T : class
        {
            if (Configuration.DisableDefaultFallback)
            {
                throw new InvalidOperationException("No provider supporting the InsertAll operation for this datasource was found");
            }

            var set = context.CreateObjectSet<T>();
            foreach (var item in items)
            {
                set.AddObject(item);
            }
            context.SaveChanges();
        }

        internal static int DefaultDelete<T>(ObjectContext context, System.Linq.Expressions.Expression<Func<T, bool>> predicate) where T : class
        {
            if (Configuration.DisableDefaultFallback)
            {
                throw new InvalidOperationException("No provider supporting the Delete operation for this datasource was found");
            }
            var set = context.CreateObjectSet<T>();
            var items = set.Where(predicate).ToList();
            foreach (var item in items)
            {
                set.DeleteObject(item);
            }
            context.SaveChanges();
            return items.Count;
        }


        internal static int DefaultUpdate<T, TP>(ObjectContext context, System.Linq.Expressions.Expression<Func<T, bool>> predicate, System.Linq.Expressions.Expression<Func<T, TP>> prop, System.Linq.Expressions.Expression<Func<T, TP>> modifier) where T : class
        {
            if (Configuration.DisableDefaultFallback)
            {
                throw new InvalidOperationException("No provider supporting the Update operation for this datasource was found");
            }
            var set = context.CreateObjectSet<T>();
            var items = set.Where(predicate).ToList();

            var setter = ExpressionHelper.PropertyExpressionToSetter(prop);
            var compiledModifer = modifier.Compile();
            foreach (var item in items)
            {
                setter(item, compiledModifer(item));
            }
            context.SaveChanges();
            return items.Count;
        }
    }
}
