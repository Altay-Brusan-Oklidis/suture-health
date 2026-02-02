using System.Collections.Generic;
using System.Runtime.Serialization;

namespace SutureHealth.Linq
{
    public enum MatchLevel
    {
        NonMatch,
        Similar,
        SimilarHighRisk,
        Match
    }

    public interface IMatchingResponse<T>
        where T : class
    {
        int Threshold { get; }
        T TopMatch { get; }
        IEnumerable<IMatchingResult<T>> MatchResults { get; }
        MatchLevel MatchLevel { get; }
    }

    /// <summary>
    /// Response for a Match Request
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MatchingResponse<T> : IMatchingResponse<T>
        where T : class
    {
        /// <summary>
        /// An entity must score at or above this value to be considered a match.
        /// </summary>
        [DataMember(Name = "threshold", EmitDefaultValue = false)]
        public int Threshold { get; set; }

        /// <summary>
        /// The entity with the top score.
        /// If this value is null, either no entity scored at or above the <see cref="Threshold"/> or more than one entity had the same top score.
        /// </summary>
        [DataMember(Name = "top-match", EmitDefaultValue = false)]
        public T TopMatch { get; set; }

        /// <summary>
        /// All match results, if requested.
        /// </summary>
        [DataMember(Name = "match-results", EmitDefaultValue = false)]
        public IEnumerable<MatchingResult<T>> MatchResults { get; set; }

        IEnumerable<IMatchingResult<T>> IMatchingResponse<T>.MatchResults => MatchResults;

        /// <summary>
        /// An entity must score at or above this value to be considered a match.
        /// </summary>
        [DataMember(Name = "match-level", EmitDefaultValue = false)]
        public MatchLevel MatchLevel { get; set; }
    }
}
