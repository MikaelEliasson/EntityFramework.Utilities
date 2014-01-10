using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace EntityFramework.Utilities
{

    public interface IIncludeContainer<T>
    {
        IEnumerable<IncludeExecuter<T>> Includes { get;  }
    }

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

            var efuQuery = GetIncludeContainer(expression);
            var first = efuQuery.Includes.First();
            Expression translated = this.Visit(expression);
            var list = new List<object>();
            foreach (var item in source.Provider.CreateQuery(translated)){
                list.Add(item);
            }
            var data = first.Loader(null, list).ToList();

            return list;
        }

        private IIncludeContainer<T> GetIncludeContainer(Expression expression)
        {
            Expression temp = expression;
            while (temp is MethodCallExpression)
            {
                temp = (temp as MethodCallExpression).Arguments[0];
            }

            return ((temp as ConstantExpression).Value as IIncludeContainer<T>);
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
            var cSpaceTables = octx.MetadataWorkspace.GetItems<EntityType>(DataSpace.CSpace);
            var cSpaceType = cSpaceTables.Single(t => t.Name == typeof(T).Name); //Use single to avoid any problems with multiple tables using the same type
            var keys = cSpaceType.KeyProperties;
            if (keys.Count > 1)
            {
                throw new InvalidOperationException("The include method only works on single key entities");
            }

            var cSpaceChildType = cSpaceTables.Single(t => t.Name == typeof(TChild).Name); //Use single to avoid any problems with multiple tables using the same type
            var fk = cSpaceChildType.NavigationProperties.First(n => n.ToEndMember.GetEntityType().Name == typeof(T).Name).GetDependentProperties().First();
            var fkInfo = typeof(TChild).GetProperty(fk.Name);
            var fkGetter = MakeGetterDelegate<TChild>(fkInfo);

            PropertyInfo info = typeof(T).GetProperty(keys.First().Name);
            var method = typeof(EFDataReader<T>).GetMethod("MakeDelegate");
            var generic = method.MakeGenericMethod(info.PropertyType);
            var getter = (Func<T, object>)generic.Invoke(null, new object[] { info.GetGetMethod(true) });

            var childProp = (collectionSelector.Body as MemberExpression).Member as PropertyInfo;
            var setter = MakeSetterDelegate<T>(childProp);


            var e = new IncludeExecuter<T>
            {
                ElementType = typeof(TChild),
                Loader = (ctx, parents) =>
                {
                    var set = octx.CreateObjectSet<TChild>();
                    var dict = set.AsNoTracking().ToLookup(fkGetter);
                    var list = parents.Cast<T>().ToList();

                    foreach (var parent in list)
                    {
                        var prop = getter(parent);
                        var childs = dict.Contains(prop) ? dict[prop].ToList() : new List<TChild>();
                        setter(parent, childs);
                    }

                    return dict.SelectMany(d => d);
                }
            };

            return new EFUQueryable<T>(query.AsNoTracking()).Include(e);
        }

        static Action<T, object> MakeSetterDelegate<T>(PropertyInfo property)
        {
            MethodInfo setMethod = property.GetSetMethod();
            if (setMethod != null && setMethod.GetParameters().Length == 1)
            {
                var target = Expression.Parameter(typeof(T));
                var value = Expression.Parameter(typeof(object));
                var body = Expression.Call(target, setMethod,
                    Expression.Convert(value, property.PropertyType));
                return Expression.Lambda<Action<T, object>>(body, target, value)
                    .Compile();
            }
            else
            {
                return null;
            }
        }

        static Func<X, object> MakeGetterDelegate<X>(PropertyInfo property)
        {
            MethodInfo getMethod = property.GetGetMethod();
            if (getMethod != null)
            {
                var target = Expression.Parameter(typeof(X));
                var body = Expression.Call(target, getMethod);
                Expression conversion = Expression.Convert(body, typeof(object));
                return Expression.Lambda<Func<X, object>>(conversion, target)
                    .Compile();
            }
            else
            {
                return null;
            }
        }
    }

    public class IncludeExecuter<T>
    {
        internal Type ElementType { get; set; }
        internal Func<ObjectContext, IEnumerable, IEnumerable<object>> Loader { get; set; }
    }

     
}
