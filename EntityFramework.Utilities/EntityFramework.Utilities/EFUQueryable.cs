using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace EntityFramework.Utilities
{
    public class EFUQueryable<T> : IOrderedQueryable<T>, IIncludeContainer<T>
    {
        private Expression expression = null;
        private EFUQueryProvider<T> provider = null;
        private List<IncludeExecuter<T>> includes = new List<IncludeExecuter<T>>();

        public IEnumerable<IncludeExecuter<T>> Includes { get { return includes; } }

        public EFUQueryable(IQueryable source)
        {
            expression = Expression.Constant(this);
            provider = new EFUQueryProvider<T>(source);
        }

        public EFUQueryable(IQueryable source, Expression e)
        {
            if (e == null) throw new ArgumentNullException("e");
            expression = e;
            provider = new EFUQueryProvider<T>(source);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return provider.ExecuteEnumerable(this.expression).Cast<T>().GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return provider.ExecuteEnumerable(this.expression).GetEnumerator();
        }

        public EFUQueryable<T> Include(IncludeExecuter<T> include)
        {
            this.includes.Add(include);
            return this;
        }

        public Type ElementType
        {
            get { return typeof(T); }
        }

        public Expression Expression
        {
            get { return expression; }
        }

        public System.Linq.IQueryProvider Provider
        {
            get { return provider; }
        }
    }
}
