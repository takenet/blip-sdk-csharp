using System;
using System.Text.RegularExpressions;

namespace Take.Blip.Builder
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
        Matches
    }

    public static class ConditionComparisonExtensions
    {
        public static Func<string, string, bool> ToDelegate(this ConditionComparison conditionComparison)
        {
            switch (conditionComparison)
            {
                case ConditionComparison.Equals:
                    return (v1, v2) => v1 == v2;

                case ConditionComparison.NotEquals:
                    return (v1, v2) => v1 != v2;

                case ConditionComparison.Contains:
                    return (v1, v2) => v1.Contains(v2);

                case ConditionComparison.StartsWith:
                    return (v1, v2) => v1.StartsWith(v2);

                case ConditionComparison.EndsWith:
                    return (v1, v2) => v1.EndsWith(v2);

                case ConditionComparison.Matches:
                    return Regex.IsMatch;

                default:
                    throw new ArgumentOutOfRangeException(nameof(conditionComparison));
            }
            
        }
    }
}
