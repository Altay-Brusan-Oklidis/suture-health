using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Fastenshtein;
using SutureHealth.Authorities;

namespace SutureHealth.Linq
{
    public static class Matching
    {
        public static bool IsUsedForMatching(this IIdentifier identifier)
        {
            return !string.IsNullOrWhiteSpace(identifier.Value) && (identifier.IsSocialSecurityIdentifier()
                || identifier.IsMedicaidType()
                || identifier.IsMedicareBeneficiaryNumber()
                || identifier.IsUniqueExternalIdentifier());
        }

        private static Expression<Func<T, bool>> And<T>(Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return (Expression.Lambda<Func<T, bool>>(Expression.AndAlso(expr1.Body, invokedExpr), expr1.Parameters));
        }

        private static Expression<Func<T, bool>> Or<T>(Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return (Expression.Lambda<Func<T, bool>>(Expression.OrElse(expr1.Body, invokedExpr), expr1.Parameters));
        }

        public static MatchingResponse<TDomain> AsMatchResponse<TDomain>
        (
            this IEnumerable<MatchingResult<TDomain>> matches,
            int threshold = 100,
            Func<MatchingResult<TDomain>, MatchingResult<TDomain>, int, MatchLevel> determineMatchLevel = null
        )
            where TDomain : class
        {
            determineMatchLevel = determineMatchLevel ?? MatchingResult.GetDefaultMatchLevel;

            var top = matches.FirstOrDefault();
            var next = matches.Skip(1).FirstOrDefault();

            var response = new MatchingResponse<TDomain>
            {
                Threshold = threshold,
                MatchResults = matches,
                TopMatch = MatchingResult.IsAboveThresholdAndNotTheSameScore(top, next, threshold) ? top.Match : null,
                MatchLevel = determineMatchLevel == null ? MatchingResult.GetDefaultMatchLevel(top, next, threshold) : determineMatchLevel.Invoke(top, next, threshold)
            };
            return response;
        }

        public static ICollection<IMatchingRule<TDomainObject>> CreateFuzzyRule<TDomainObject>(this ICollection<IMatchingRule<TDomainObject>> rules, Expression<Func<TDomainObject, string>> property,
            string value,
            int positiveScore = 10,
            int negativeScore = -10,
            float multiplier = 0.5f,
            bool isDbRule = true
        )
            where TDomainObject : class
        {
            rules.Add(MatchingRule<TDomainObject>.CreateMatchingRule(property, value, positiveScore, negativeScore, isDbRule));
            rules.Add(FuzzyMatchingRule<TDomainObject>.CreateMatchingRule(property, value, positiveScore, negativeScore, multiplier));
            return rules;
        }

        public static ICollection<IMatchingRule<TDomainObject>> CreateMatchingRule<TDomainObject>(this ICollection<IMatchingRule<TDomainObject>> rules, Expression<Func<TDomainObject, bool>> enumerableMatchingExpression,
            Expression<Func<TDomainObject, bool>> queryableMatchingExpression = null,
            Expression<Func<TDomainObject, bool>> nullMatchingExpression = null,
            Expression<Func<TDomainObject, bool>> condition = null,
            int positiveMatchScore = 10,
            int negativeMatchScore = -10,
            string description = null,
            bool isDbRule = true
        )
            where TDomainObject : class
        {
            rules.Add(MatchingRule<TDomainObject>.CreateMatchingRule(enumerableMatchingExpression,
                queryableMatchingExpression,
                nullMatchingExpression,
                condition,
                positiveMatchScore,
                negativeMatchScore,
                description,
                isDbRule));
            return rules;
        }

        public static IEnumerable<MatchingResult<T>> Match<T>
        (
            this IQueryable<T> queryable,
            Action<ICollection<IMatchingRule<T>>> rules
        )
            where T : class
        {
            var list = new List<IMatchingRule<T>>();
            rules(list);
            return Match(queryable, rules: list.ToArray());
        }

        public static IEnumerable<FuzzyMatchingRule<T>> FuzzyRules<T>(this IEnumerable<IMatchingRule<T>> rules)
            where T : class => rules.OfType<FuzzyMatchingRule<T>>();

        public static IEnumerable<MatchingResult<T>> Match<T>(this IQueryable<T> queryable, params IMatchingRule<T>[] rules)
            where T : class => Match<T, object>(queryable, null, rules: rules);

        public static IEnumerable<MatchingResult<T>> Match<T, TPrimary>
        (
            this IQueryable<T> queryable,
            Expression<Func<T, TPrimary>> primaryKeyProperty = null,
            params IMatchingRule<T>[] rules
        )
            where T : class
        {
            var results = new ConcurrentBag<MatchingRuleResult<T>>();
            IQueryable<T> exp = null;

            foreach (var rule in rules.OfType<MatchingRule<T>>().Where(x => x.IsDbRule))
            {
                var ruleExp = rule.Condition == null ? rule.IQueryableMatchEvaluation ?? rule.IEnumerableMatchEvaluation : And(rule.IQueryableMatchEvaluation ?? rule.IEnumerableMatchEvaluation, rule.Condition);
                if (exp == null)
                    exp = queryable.Where(ruleExp);
                else
                    exp = exp.Union(queryable.Where(ruleExp));
            }

            if (exp == null)
            {
                exp = queryable;
            }

            var memoryMatches = exp.ToArray().AsQueryable();
            Parallel.ForEach(rules.OfType<MatchingRule<T>>().Where(r => r.IEnumerableMatchEvaluation != null && !(r is FuzzyMatchingRule<T>)), rule =>
            {
                var ruleMatches = rule.Condition != null ? memoryMatches.Where(rule.IEnumerableMatchEvaluation)
                                                                        .Where(rule.Condition)
                                                         : memoryMatches.Where(rule.IEnumerableMatchEvaluation);
                ruleMatches.Select(x => new MatchingRuleResult<T>
                {
                    Rule = rule,
                    Match = x,
                    Score = rule.PositiveScore
                }).ToList().ForEach(x => results.Add(x));
            });

            Parallel.ForEach(results.Select(x => x.Match).Distinct().ToList(), match =>
            {
                foreach(var rule in rules.OfType<MatchingRule<T>>().Where(r => r.IEnumerableMatchEvaluation != null))
                {
                    if (!rule.IEnumerableMatchEvaluation.Compile()(match) && rule.NullMatchEvaluation.Compile()(match))
                    {
                        if (!(rule is FuzzyMatchingRule<T>))
                        {
                            if (rule.Condition == null || rule.Condition.Compile()(match))
                            {
                                results.Add(new MatchingRuleResult<T>
                                {
                                    Rule = rule,
                                    Match = match,
                                    Score = rule.NegativeScore
                                });
                            }
                        }

                        if (rule is FuzzyMatchingRule<T> fuzzy)
                        {
                            var leven = new Levenshtein(fuzzy.Value);
                            var property = fuzzy.Property.Compile()(match);
                            var maxLength = System.Math.Max(property.Length, fuzzy.Value.Length);
                            var result = (maxLength - leven.DistanceFrom(property)) / (float)maxLength;
                            var score = result >= .8f ? System.Math.Abs(fuzzy.NegativeScore) + (fuzzy.Multiplier * result * fuzzy.PositiveScore) : 0;

                            results.Add(new MatchingRuleResult<T>
                            {
                                MatchLog = $"PercentageMatch: {(result * 100f).ToString("00.00")}%",
                                Rule = fuzzy,
                                Match = match,
                                Score = score
                            });
                        }
                    }
                }
            });

            return results.GroupBy(x => x.Match)
                          .Select(x => new MatchingResult<T>
                          {
                              Match = x.Key,
                              Rules = x.Select(g => new MatchingRuleResult<T>
                              {
                                  Rule = g.Rule,
                                  MatchLog = g.MatchLog,
                                  Score = g.Score
                              }),
                              Score = x.Sum(g => g.Score)
                          })
                          .OrderByDescending(x => x.Score)
                          .ToArray();
        }

        public static string GetMatchSummaryString<T>(this IMatchingResult<T> result)
            where T : class
        {
            var output = new StringBuilder();
            output.AppendLine($"Match Result: {result.Match}");
            output.AppendLine($"  Type: {result.Match.GetType().Name}");
            output.AppendLine($"  Score: {result.Score}");
            output.AppendLine($"  Rules (score - description):");

            foreach (var rule in result.Rules)
            {
                output.AppendLine($"    {rule.Score.ToString(" 000.00;-000.00")} - {rule.Rule.Description}{(rule.MatchLog.IsNullOrEmpty() ? string.Empty : $" - {rule.MatchLog}")}");
            }

            return output.ToString();
        }

        public static string GetMatchesSummaryString<T>(this IEnumerable<IMatchingResult<T>> results, string matchLevel = null, int maxMatches = 5)
            where T : class
        {
            if (results == null || results.Any() == false)
            {
                return "No matches found.";
            }

            var resultCount = results.Count();
            if (maxMatches == 0)
            {
                maxMatches = resultCount;
            }

            return $"{matchLevel} Number of Matches: {resultCount}{Environment.NewLine}{results.Take(maxMatches).Select(x => x.GetMatchSummaryString()).Aggregate((seed, add) => seed += add)}";
        }
    }
}
