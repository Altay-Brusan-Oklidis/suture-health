using System;
using System.Linq.Expressions;

namespace SutureHealth.Linq
{
    public interface IMatchingRule
    {
        string Description { get; }
        bool IsDbRule { get; }
        int PositiveScore { get; }
        int NegativeScore { get; }
    }

    public interface IMatchingRule<T> : IMatchingRule
        where T : class
    { }

    public class MatchingRule : IMatchingRule
    {
        public MatchingRule() { }
        public MatchingRule(string description) => Description = description;

        public string Description { get; internal set; }
        public int PositiveScore { get; set; }
        public int NegativeScore { get; set; }
        public bool IsDbRule { get; set; }
    }

    public class MatchingRule<TDomainObject> : MatchingRule, IMatchingRule<TDomainObject>
        where TDomainObject : class
    {
        private static readonly System.Reflection.MethodInfo methodInfo = typeof(System.StringExtensions).GetMethod("EqualsIgnoreCase", new Type[] { typeof(string), typeof(string) });

        public Expression<Func<TDomainObject, bool>> Condition { get; internal set; }
        public Expression<Func<TDomainObject, bool>> IEnumerableMatchEvaluation { get; set; }
        public Expression<Func<TDomainObject, bool>> IQueryableMatchEvaluation { get; set; }
        public Expression<Func<TDomainObject, bool>> NullMatchEvaluation { get; internal set; }

        public static MatchingRule<TDomainObject> CreateMatchingRule
        (
            Expression<Func<TDomainObject, string>> enumerableMatchingExpression,
            string value,
            int positiveScore = 10,
            int negativeScore = -10,
            bool isDbRule = true
        )
        {
            MatchingRule<TDomainObject> rule = null;

            if (value != null)
            {
                rule = new MatchingRule<TDomainObject>
                {
                    IEnumerableMatchEvaluation = Expression.Lambda<Func<TDomainObject, bool>>(Expression.Call(methodInfo, enumerableMatchingExpression.Body, Expression.Constant(value)), enumerableMatchingExpression.Parameters),
                    IQueryableMatchEvaluation = Expression.Lambda<Func<TDomainObject, bool>>(Expression.Equal(enumerableMatchingExpression.Body, Expression.Constant(value)), enumerableMatchingExpression.Parameters),
                    NullMatchEvaluation = Expression.Lambda<Func<TDomainObject, bool>>(Expression.NotEqual(enumerableMatchingExpression.Body, Expression.Constant(null)), enumerableMatchingExpression.Parameters),
                    PositiveScore = positiveScore,
                    NegativeScore = negativeScore,
                    IsDbRule = isDbRule,
                    Description = $"MatchRule: {enumerableMatchingExpression.Body.ToString()}.{methodInfo.Name}(\"{value}\")",
                    Condition = null,
                };
            }

            return rule;
        }

        public static MatchingRule<TDomainObject> CreateMatchingRule
        (
            Expression<Func<TDomainObject, bool>> enumerableMatchingExpression,
            Expression<Func<TDomainObject, bool>> queryableMatchingExpression = null,
            Expression<Func<TDomainObject, bool>> nullMatchingExpression = null,
            Expression<Func<TDomainObject, bool>> condition = null,
            int positiveMatchScore = 10,
            int negativeMatchScore = -10,
            string description = null,
            bool isDbRule = true
        ) 
        {
            var rule = new MatchingRule<TDomainObject>
            {
                IEnumerableMatchEvaluation = enumerableMatchingExpression,
                IQueryableMatchEvaluation = queryableMatchingExpression,
                NullMatchEvaluation = x => nullMatchingExpression == null || nullMatchingExpression.Compile()(x),
                Condition = condition,
                Description = $"MatchRule: {description ?? enumerableMatchingExpression.Body.ToString()}{(condition == null ? string.Empty : $" AND {condition.Body.ToString()}")}",
                IsDbRule = isDbRule,
                PositiveScore = positiveMatchScore,
                NegativeScore = negativeMatchScore
            };

            return rule;
        }
    }
}
