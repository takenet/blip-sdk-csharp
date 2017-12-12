using System;
using System.Text.RegularExpressions;

namespace Take.Blip.Builder.Models
{
    /// <summary>
    /// Define the comparison methods for condition values.
    /// </summary>
    public enum ConditionComparison
    {
        /// <summary>
        /// Check for value equality.
        /// </summary>
        Equals,

        /// <summary>
        /// Check for a not equal value.
        /// </summary>
        NotEquals,

        /// <summary>
        /// Check a substring value.
        /// </summary>
        Contains,

        /// <summary>
        /// Check if a string starts with the provided value.
        /// </summary>
        StartsWith,

        /// <summary>
        /// Check if a string ends with the provided value.
        /// </summary>
        EndsWith,

        /// <summary>
        /// Check if a string matches the provided regular expression.
        /// </summary>
        Matches,

        /// <summary>
        /// Check if a string value is approximate to the other specified value.
        /// </summary>
        ApproximateTo
    }

    public static class ConditionComparisonExtensions
    {
        public static Func<string, string, bool> ToDelegate(this ConditionComparison conditionComparison)
        {
            switch (conditionComparison)
            {
                case ConditionComparison.Equals:
                    return (v1, v2) => v1.Equals(v2, StringComparison.OrdinalIgnoreCase);

                case ConditionComparison.NotEquals:
                    return (v1, v2) => !v1.Equals(v2, StringComparison.OrdinalIgnoreCase);

                case ConditionComparison.Contains:
                    return (v1, v2) => v1.ToLowerInvariant().Contains(v2.ToLowerInvariant());

                case ConditionComparison.StartsWith:
                    return (v1, v2) => v1.StartsWith(v2, StringComparison.OrdinalIgnoreCase);

                case ConditionComparison.EndsWith:
                    return (v1, v2) => v1.EndsWith(v2, StringComparison.OrdinalIgnoreCase);

                case ConditionComparison.Matches:
                    return Regex.IsMatch;

                case ConditionComparison.ApproximateTo:
                    // Allows the difference of 25% of the string.
                    return (v1, v2) => v1.CalculateLevenshteinDistance(v2) <= Math.Ceiling(v1.Length * 0.25);

                default:
                    throw new ArgumentOutOfRangeException(nameof(conditionComparison));
            }
            
        }
    }
}
