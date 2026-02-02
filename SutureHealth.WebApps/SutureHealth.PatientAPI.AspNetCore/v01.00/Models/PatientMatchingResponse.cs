using System.Collections.Generic;

namespace SutureHealth.Patients.v0100.Models
{
    public enum MatchLevel
    {
        NonMatch,
        Similar,
        SimilarHighRisk,
        Match
    }

    public class MatchingResult
    {
        public Patient Match { get; set; }
        public IEnumerable<MatchingRuleResult> Rules { get; set; }
        public float Score { get; set; }
    }

    public class MatchingRuleResult
    {
        public string MatchLog { get; set; }
        public MatchingRule Rule { get; set; }
        public float Score { get; set; }
    }

    public class MatchingRule
    {
        public string Description { get; set; }
        public int PositiveScore { get; set; }
        public int NegativeScore { get; set; }
        public bool IsDbRule { get; set; }
    }

    public class PatientMatchingResponse
    {
        public int Threshold { get; set; }
        public Patient TopMatch { get; set; }
        public IEnumerable<MatchingResult> MatchResults { get; set; }
        public MatchLevel MatchLevel { get; set; }
    }
}
