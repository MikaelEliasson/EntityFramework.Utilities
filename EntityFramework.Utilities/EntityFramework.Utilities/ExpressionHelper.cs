using System;
using System.Linq.Expressions;

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

    public static class ExpressionHelper
    {
        public static Expression<Func<T, bool>> CombineExpressions<T, TP>(Expression<Func<T, TP>> prop, Expression<Func<T, TP>> modifier) where T : class
        {
            var propRewritten = new ReplaceVisitor(prop.Parameters[0], modifier.Parameters[0]).Visit(prop.Body);
            var expr = Expression.Equal(propRewritten, modifier.Body);
            var final = Expression.Lambda<Func<T, bool>>(expr, modifier.Parameters[0]);
            return final;
        }

        public static string GetPropertyName<TSource, TProperty>(this Expression<Func<TSource, TProperty>> propertyLambda)
        {
            Type type = typeof(TSource);

            var temp = propertyLambda.Body;
            while (temp is UnaryExpression)
            {
                temp = (temp as UnaryExpression).Operand;
            }
            MemberExpression member = temp as MemberExpression;
            return member.Member.Name;
        }

        //http://stackoverflow.com/a/2824409/507279
        public static Action<T, TP> PropertyExpressionToSetter<T, TP>(Expression<Func<T, TP>> prop)
        {

            // re-write in .NET 4.0 as a "set"
            var member = (MemberExpression)prop.Body;
            var param = Expression.Parameter(typeof(TP), "value");
            var set = Expression.Lambda<Action<T, TP>>(
                Expression.Assign(member, param), prop.Parameters[0], param);

            // compile it
            return set.Compile();
        }

        public static Action<T, object> PropertyNameToSetter<T>(string propertyName)
        {
            var method = typeof(T).GetProperty(propertyName).GetSetMethod();

            var obj = Expression.Parameter(typeof(T), "o");
            var value = Expression.Parameter(typeof(object));

            Expression<Action<T, object>> expr =
                Expression.Lambda<Action<T, object>>(
                    Expression.Call(
                        Expression.Convert(obj, method.DeclaringType),
                        method,
                        Expression.Convert(value, method.GetParameters()[0].ParameterType)),
                    obj,
                    value);

            return expr.Compile();
        }
    }
}
