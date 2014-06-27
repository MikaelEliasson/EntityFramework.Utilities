using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace EntityFramework.Utilities
{
    public static class ContextExtensionMethods
    {
        public class AttachAndModifyContext<T> where T : class{
            private DbSet<T> set;
            private System.Data.Entity.Infrastructure.DbEntityEntry<T> entry;

            public AttachAndModifyContext(DbSet<T> set, System.Data.Entity.Infrastructure.DbEntityEntry<T> entry)
            {
                this.set = set;
                this.entry = entry;
            }
            public AttachAndModifyContext<T> Set<TProp>(Expression<Func<T, TProp>> property, TProp value){

                var setter = ExpressionHelper.PropertyExpressionToSetter(property);
                setter(entry.Entity, value);
                entry.Property(property).IsModified = true;
                return this;
            }
        }
        public static AttachAndModifyContext<T> AttachAndModify<T>(this DbContext source, T item) where T : class
        {
            var set = source.Set<T>();
            set.Attach(item);
            var entry = source.Entry(item);

            return new AttachAndModifyContext<T>(set, entry);
        }
    }
}
