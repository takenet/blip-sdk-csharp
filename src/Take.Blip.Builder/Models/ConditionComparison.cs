using System;
using System.Globalization;
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
        /// Check if the numeric value is greater than the provided value.
        /// </summary>
        GreaterThan,

        /// <summary>
        /// Check if the numeric value is less than the provided value.
        /// </summary>
        LessThan,

        /// <summary>
        /// Check if the numeric value is greater than or equals the provided value.
        /// </summary>
        GreaterThanOrEquals,

        /// <summary>
        /// Check if the numeric value is less than or equals the provided value.
        /// </summary>
        LessThanOrEquals,

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
                    return (v1, v2) => string.Compare(v1, v2, CultureInfo.InvariantCulture, CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase) == 0;

                case ConditionComparison.NotEquals:
                    return (v1, v2) => string.Compare(v1, v2, CultureInfo.InvariantCulture, CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase) != 0;

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
                    return (v1, v2) => v1.ToLowerInvariant().CalculateLevenshteinDistance(v2.ToLowerInvariant()) <= Math.Ceiling(v1.Length * 0.25);

                case ConditionComparison.GreaterThan:
                    return (v1, v2) => decimal.TryParse(v1, out var n1) && decimal.TryParse(v2, out var n2) && n1 > n2;
                    
                case ConditionComparison.LessThan:
                    return (v1, v2) => decimal.TryParse(v1, out var n1) && decimal.TryParse(v2, out var n2) && n1 < n2;

                case ConditionComparison.GreaterThanOrEquals:
                    return (v1, v2) => decimal.TryParse(v1, out var n1) && decimal.TryParse(v2, out var n2) && n1 >= n2;

                case ConditionComparison.LessThanOrEquals:
                    return (v1, v2) => decimal.TryParse(v1, out var n1) && decimal.TryParse(v2, out var n2) && n1 <= n2;

                default:
                    throw new ArgumentOutOfRangeException(nameof(conditionComparison));
            }
            
        }
    }
}
