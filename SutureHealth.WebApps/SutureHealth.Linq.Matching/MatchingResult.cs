using System;
using System.Collections.Generic;

namespace SutureHealth.Linq
{
    public interface IMatchingResult
    {
        float Score { get; }
        IEnumerable<IMatchingRuleResult> Rules { get; }
    }

    public interface IMatchingResult<T> : IMatchingResult
        where T : class
    {
        T Match { get; }
    }

    public abstract class MatchingResult
    {
        public static bool AreNotSameScore(IMatchingResult topMatch, IMatchingResult secondMatch)
            => secondMatch == null || !topMatch.Score.NearlyEqual(secondMatch.Score, 0.001f);
        public static bool IsAboveThreshold(IMatchingResult topMatch, int threshold)
            => topMatch != null && topMatch.Score >= threshold;
        public static bool IsAboveThresholdAndNotTheSameScore(IMatchingResult topMatch, IMatchingResult secondMatch, int threshold)
            => IsAboveThreshold(topMatch, threshold) && AreNotSameScore(topMatch, secondMatch);

        public static MatchLevel GetDefaultMatchLevel(IMatchingResult topMatch, IMatchingResult secondMatch, int threshold)
        {
            if (IsAboveThresholdAndNotTheSameScore(topMatch, secondMatch, threshold))
                return MatchLevel.Match;
            if (topMatch != null && topMatch.Score > 0 && !AreNotSameScore(topMatch, secondMatch))
                return MatchLevel.Similar;

            return MatchLevel.NonMatch;
        }
    }

    public class MatchingResult<T> : MatchingResult, IMatchingResult<T>
        where T : class
    {
        public T Match { get; set; }
        public IEnumerable<MatchingRuleResult<T>> Rules { get; set; }
        public float Score { get; set; }

        IEnumerable<IMatchingRuleResult> IMatchingResult.Rules => Rules;


        public override bool Equals(object obj)
            => (obj is MatchingResult<T> match) && Equals(match.Match.Equals(Match));

        public override int GetHashCode()
            => Match.GetHashCode();
    }

    public interface IMatchingRuleResult
    {
        string MatchLog { get; }
        IMatchingRule Rule { get; }
        float Score { get; }
    }

    public interface IMatchingRuleResult<T> : IMatchingRuleResult
        where T : class
    {
        T Match { get; }
    }

    public class MatchingRuleResult<T> : IMatchingRuleResult<T>
        where T : class
    {
        public string MatchLog { get; set; }
        public MatchingRule Rule { get; set; }
        public float Score { get; set; }
        public T Match { get; set; }

        IMatchingRule IMatchingRuleResult.Rule => Rule;
    }

    public class GoldenTicketRuleResult<T> : MatchingRuleResult<T>
        where T : class
    { 
        public bool IsOverrideRule => true;
    }
}
