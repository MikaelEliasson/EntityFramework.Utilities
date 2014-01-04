using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace EntityFramework.Utilities
{

    public interface IIncludeContainer
    {
        IEnumerable<IncludeExecuter> Includes { get;  }
    }

    public class EFUQueryable<T> : IOrderedQueryable<T>, IIncludeContainer
    {
        private Expression expression = null;
        private EFUQueryProvider<T> provider = null;
        private List<IncludeExecuter> includes = new List<IncludeExecuter>();

        public IEnumerable<IncludeExecuter> Includes { get { return includes; } }

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
            return ((IEnumerable<T>)provider.ExecuteEnumerable(this.expression)).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return provider.ExecuteEnumerable(this.expression).GetEnumerator();
        }

        public EFUQueryable<T> Include(IncludeExecuter include)
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

    public class EFUQueryProvider<T> : ExpressionVisitor, System.Linq.IQueryProvider
    {
        internal IQueryable source;

        public EFUQueryProvider(IQueryable source)
        {
            if (source == null) throw new ArgumentNullException("source");
            this.source = source;
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            if (expression == null) throw new ArgumentNullException("expression");

            return new EFUQueryable<TElement>(source, expression) as IQueryable<TElement>;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            if (expression == null) throw new ArgumentNullException("expression");
            Type elementType = expression.Type.GetGenericArguments().First();
            IQueryable result = (IQueryable)Activator.CreateInstance(typeof(EFUQueryable<>).MakeGenericType(elementType),
                new object[] { source, expression });
            return result;
        }

        public TResult Execute<TResult>(Expression expression)
        {
            if (expression == null) throw new ArgumentNullException("expression");
            object result = this.Execute(expression);
            return (TResult)result;
        }

        public object Execute(Expression expression)
        {
            if (expression == null) throw new ArgumentNullException("expression");

            Expression translated = this.Visit(expression);
            return source.Provider.Execute(translated);
        }

        //private ObjectContext GetContext(System.Linq.IQueryProvider provider)
        //{

        //    var f = provider
        //   .GetType()
        //   .BaseType
        //   .GetProperty("InternalContext", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        //    object internalContext = f.GetGetMethod().Invoke(provider, null);
        //    return (ObjectContext)internalContext
        //    .GetType()
        //    .GetProperty("ObjectContext",BindingFlags.Instance|BindingFlags.Public)
        //    .GetValue(internalContext,null); 

        //}

        internal IEnumerable ExecuteEnumerable(Expression expression)
        {
            if (expression == null) throw new ArgumentNullException("expression");
            var p = source.Provider;



            //var t = ((((expression as ConstantExpression).Value as EFUQueryable<Contact>).Includes.First() as LambdaExpression).Body as MemberExpression).Type.GetGenericArguments()[0];

            var first = ((expression as ConstantExpression).Value as IIncludeContainer).Includes.First();
            var data = first.Accessor(null).ToList();

            Expression translated = this.Visit(expression);
            return source.Provider.CreateQuery(translated);
        }

        #region Visitors
        protected override Expression VisitConstant(ConstantExpression c)
        {
            // fix up the Expression tree to work with EF again
            if (c.Type == typeof(EFUQueryable<T>))
            {
                return source.Expression;
            }
            else
            {
                return base.VisitConstant(c);
            }
        }
        #endregion
    }

    public static class EFQueryHelpers
    {

        public static EFUQueryable<T> IncludeEFU<T, TChild>(this IQueryable<T> query, DbContext context, Expression<Func<T, IEnumerable<TChild>>> collectionSelector)
            where T : class
            where TChild : class
        {
            var octx = (context as IObjectContextAdapter).ObjectContext;
            var e = new IncludeExecuter
            {
                ElementType = typeof(TChild),
                Accessor = ctx =>
                {
                    var set = octx.CreateObjectSet<TChild>();
                    return set.ToList();
                }
            };

            return new EFUQueryable<T>(query).Include(e);
        }
    }

    public class IncludeExecuter
    {
        internal Type ElementType { get; set; }
        internal Func<ObjectContext, IEnumerable<object>> Accessor { get; set; }
    }
}
