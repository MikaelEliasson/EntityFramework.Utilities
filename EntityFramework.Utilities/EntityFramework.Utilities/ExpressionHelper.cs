using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace EntityFramework.Utilities
{
    class ReplaceVisitor : ExpressionVisitor
    {
        private readonly Expression from, to;
        public ReplaceVisitor(Expression from, Expression to)
        {
            this.from = from;
            this.to = to;
        }
        public override Expression Visit(Expression node)
        {
            return node == from ? to : base.Visit(node);
        }
    }

    static class ExpressionHelper
    {
        internal static Expression<Func<T, bool>> CombineExpressions<T, TP>(Expression<Func<T, TP>> prop, Expression<Func<T, TP>> modifier) where T : class
        {
            var propRewritten = new ReplaceVisitor(prop.Parameters[0], modifier.Parameters[0]).Visit(prop.Body);
            var expr = Expression.Equal(propRewritten, modifier.Body);
            var final = Expression.Lambda<Func<T, bool>>(expr, modifier.Parameters[0]);
            return final;
        }

        //http://stackoverflow.com/a/2824409/507279
        internal static Action<T, TP> PropertyExpressionToSetter<T, TP>(Expression<Func<T, TP>> prop)
        {

            // re-write in .NET 4.0 as a "set"
            var member = (MemberExpression)prop.Body;
            var param = Expression.Parameter(typeof(TP), "value");
            var set = Expression.Lambda<Action<T, TP>>(
                Expression.Assign(member, param), prop.Parameters[0], param);

            // compile it
            return set.Compile();
        }
    }
}
