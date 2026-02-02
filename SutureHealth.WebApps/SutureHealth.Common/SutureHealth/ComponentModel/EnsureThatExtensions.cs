using System;
using System.Linq.Expressions;

namespace SutureHealth.ComponentModel
{
    public static class EnsureThis
    {
        public static string GetName<T>(this Expression<Func<T>> action)
        {
            return GetNameFromMemberExpression(action.Body);
        }

        static string GetNameFromMemberExpression(Expression expression)
        {
            if (expression is MemberExpression)
            {
                return (expression as MemberExpression).Member.Name;
            }
            else if (expression is UnaryExpression)
            {
                return GetNameFromMemberExpression((expression as UnaryExpression).Operand);
            }

            return "MemberNameUnknown";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="member"></param>
        /// <exception cref="ArgumentException"
        public static void IsNotNull (Expression<Func<object>> member, string memberName = null)
        {
            var memberValue = member.Compile()();
            if (memberValue == null)
            {
                throw new ArgumentException($"This property cannot be null.", memberName ?? member.GetName());
            }
        }

        public static void IsNotNullOrWhiteSpace(Expression<Func<object>> member)
        {
            var memberValue = member.Compile()();
            if (string.IsNullOrWhiteSpace(memberValue?.ToString()))
            {
                throw new ArgumentException($"This property cannot be null or empty.", member.GetName());
            }
        }

        public static void IsOfType<T>(Expression<Func<object>> member)
        {
            var memberValue = member.Compile()();
            if (!(memberValue is T))
            {
                throw new ArgumentException($"This property should be of type {typeof(T).Name}.", member.GetName());
            }
        }
    }
}
