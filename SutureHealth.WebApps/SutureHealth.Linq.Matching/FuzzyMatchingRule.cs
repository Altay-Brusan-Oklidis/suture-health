using System;
using System.Linq.Expressions;

namespace SutureHealth.Linq
{
    public class FuzzyMatchingRule<TDomainObject> : MatchingRule<TDomainObject>
        where TDomainObject : class
    {
        private static readonly System.Reflection.MethodInfo methodInfo = typeof(StringExtensions).GetMethod("EqualsIgnoreCase", new Type[] { typeof(string), typeof(string) });

        public Expression<Func<TDomainObject, string>> Property { get; set; }
        public string Value { get; set; }
        public float Multiplier { get; set; } = 0.5f;

        public static FuzzyMatchingRule<TDomainObject> CreateMatchingRule
        (
            Expression<Func<TDomainObject, string>> property,
            string value,
            int positiveScore = 10,
            int negativeScore = -10,
            float multiplier = 0.5f
        )
        {
            FuzzyMatchingRule<TDomainObject> rule = null;

            if (value != null)
            {
                rule = new FuzzyMatchingRule<TDomainObject>
                {
                    IEnumerableMatchEvaluation = Expression.Lambda<Func<TDomainObject, bool>>(Expression.Call(methodInfo, property.Body, Expression.Constant(value)), property.Parameters),
                    IQueryableMatchEvaluation = Expression.Lambda<Func<TDomainObject, bool>>(Expression.Equal(property.Body, Expression.Constant(value)), property.Parameters),
                    NullMatchEvaluation = Expression.Lambda<Func<TDomainObject, bool>>(Expression.NotEqual(property.Body, Expression.Constant(null)), property.Parameters),
                    PositiveScore = positiveScore,
                    NegativeScore = negativeScore,
                    IsDbRule = false,
                    Property = property,
                    Value = value,
                    Description = $"FuzzyRule: {property.Body.ToString()} ~= \"{value}\"",
                    Condition = null,
                    Multiplier = multiplier
                };
            }

            return rule;
        }
    }
}
