using System.Linq.Expressions;
using System.Reflection;

namespace System.Linq
{
    public static class IQueryableExtensions
    {
        private static readonly MethodInfo ContainsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
        private static readonly MethodInfo StartsWithMethod = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
        private static readonly MethodInfo EndsWithMethod = typeof(string).GetMethod("EndsWith", new[] { typeof(string) });

        public static Expression<Func<TSource, bool>> LikeExpression<TSource, TMember>(Expression<Func<TSource, TMember>> property, string value)
        {
            var param = Expression.Parameter(typeof(TSource), "t");
            var memberInfo = GetMemberInfo(property);

            MemberExpression member;
            if (memberInfo is PropertyInfo)
                member = Expression.Property(param, memberInfo.Name);
            else if (memberInfo is FieldInfo)
                member = Expression.Field(param, memberInfo.Name);
            else
                throw new InvalidOperationException("Unable to determine propery or field info from expression.");

            var startWith = value.StartsWith("%");
            if (startWith)
                value = value.Remove(0, 1);

            var endsWith = value.EndsWith("%");
            if (endsWith)
                value = value.Remove(value.Length - 1, 1);

            var constant = Expression.Constant(value);

            Expression exp;
            if (endsWith && startWith)
                exp = Expression.Call(member, ContainsMethod, constant);
            else if (startWith)
                exp = Expression.Call(member, EndsWithMethod, constant);
            else if (endsWith)
                exp = Expression.Call(member, StartsWithMethod, constant);
            else
                exp = Expression.Equal(member, constant);

            return Expression.Lambda<Func<TSource, bool>>(exp, param);
        }

        public static IQueryable<TSource> Like<TSource, TMember>(this IQueryable<TSource> source, Expression<Func<TSource, TMember>> parameter, string value)
        {
            return source.Where(LikeExpression(parameter, value));
        }

        private static MemberInfo GetMemberInfo(Expression expression)
        {
            var lambda = expression as LambdaExpression;
            if (lambda == null)
                throw new ArgumentNullException("expression");

            MemberExpression memberExpr = null;
            switch (lambda.Body.NodeType)
            {
                case ExpressionType.Convert:
                    memberExpr = ((UnaryExpression)lambda.Body).Operand as MemberExpression;
                    break;

                case ExpressionType.MemberAccess:
                    memberExpr = lambda.Body as MemberExpression;
                    break;
            }

            if (memberExpr == null)
                throw new InvalidOperationException("Specified expression is invalid. Unable to determine member info from expression.");

            var output = memberExpr.Member as MemberInfo;
            if (output == null)
                throw new InvalidOperationException("Specified expression is invalid. Unable to determine member info from expression.");

            return output;
        }
    }
}
