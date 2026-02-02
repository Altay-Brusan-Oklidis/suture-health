using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace System.Linq
{
    public static class ExpressionExtensions
    {
        static readonly Random random = new Random();

        public static Expression<Func<T, bool>> AndAll<T>(this IEnumerable<Expression<Func<T, bool>>> expressions, string parameterName = null)
            => Combine(expressions, (e1, e2) => Expression.AndAlso(e1, e2), parameterName);
        public static Expression<Func<T, bool>> OrAll<T>(this IEnumerable<Expression<Func<T, bool>>> expressions, string parameterName = null)
            => Combine(expressions, (e1, e2) => Expression.OrElse(e1, e2), parameterName);

        private static Expression<Func<T, bool>> Combine<T>(this IEnumerable<Expression<Func<T, bool>>> expressions, Func<Expression, Expression, Expression> combinationFunc, string parameterName = null)
        {
            if (expressions == null)
            {
                throw new ArgumentNullException("expressions");
            }
            if (expressions.Count() == 0)
            {
                return t => true;
            }

            if (parameterName.IsNullOrWhiteSpace())
            {
                // private static Random random = new Random();
                Func<int, string> randomString = length =>
                {
                    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
                    return new string(Enumerable.Repeat(chars, length)
                      .Select(s => s[random.Next(s.Length)]).ToArray());
                };

                parameterName = $"_{randomString(3)}";
            }


            var parameter = Expression.Parameter(typeof(T), parameterName);
            var combined = new ParameterReplacer<T>(parameter).Visit(expressions.Select(e => e.Body)
                                                                                .Aggregate(combinationFunc));
            return Expression.Lambda<Func<T, bool>>(combined, parameter);
        }

        public class ParameterReplacer<T> : ExpressionVisitor
        {
            readonly ParameterExpression parameter;

            public  ParameterReplacer(ParameterExpression parameter)
            {
                this.parameter = parameter;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                if (node.Type.IsAssignableFrom(typeof(T)))
                    return parameter;
                else
                    return node;
            }
        }
    }
}
