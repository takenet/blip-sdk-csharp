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
        ApproximateTo,

        /// <summary>
        /// Check if value exists.
        /// </summary>
        Exists,

        /// <summary>
        /// Check if value does not exist.
        /// </summary>
        NotExists,
    }

    /// <summary>
    /// Define the delegation method for comparison types.
    /// </summary>
    public enum ComparisonType
    {
        /// <summary>
        /// Check for unary comparison.
        /// </summary>
        Unary,

        /// <summary>
        /// Check for binary comparison.
        /// </summary>
        Binary,
    }

    public static class ConditionComparisonExtensions
    {
        public static ComparisonType GetComparisonType(this ConditionComparison conditionComparison)
        {
            switch (conditionComparison)
            {
                case ConditionComparison.Exists:
                case ConditionComparison.NotExists:
                    return ComparisonType.Unary;

                case ConditionComparison.Equals:
                case ConditionComparison.NotEquals:
                case ConditionComparison.Contains:
                case ConditionComparison.StartsWith:
                case ConditionComparison.EndsWith:
                case ConditionComparison.GreaterThan:
                case ConditionComparison.LessThan:
                case ConditionComparison.GreaterThanOrEquals:
                case ConditionComparison.LessThanOrEquals:
                case ConditionComparison.Matches:
                case ConditionComparison.ApproximateTo:
                    return ComparisonType.Binary;

                default:
                    throw new ArgumentOutOfRangeException(nameof(conditionComparison));
            }
        }

        public static Func<string, bool> ToUnaryDelegate(this ConditionComparison conditionComparison)
        {
            switch (conditionComparison)
            {
                case ConditionComparison.Exists:
                    return (v) => !string.IsNullOrEmpty(v);

                case ConditionComparison.NotExists:
                    return string.IsNullOrEmpty;
                
                case ConditionComparison.Equals:
                case ConditionComparison.NotEquals:
                case ConditionComparison.Contains:
                case ConditionComparison.StartsWith:
                case ConditionComparison.EndsWith:
                case ConditionComparison.Matches:
                case ConditionComparison.ApproximateTo:
                case ConditionComparison.GreaterThan:
                case ConditionComparison.LessThan:
                case ConditionComparison.GreaterThanOrEquals:
                case ConditionComparison.LessThanOrEquals:
                    throw new ArgumentException("The provided value is not an unary comparison", nameof(conditionComparison));

                default:
                    throw new ArgumentOutOfRangeException(nameof(conditionComparison));
            }
        }

        public static Func<string, string, bool> ToBinaryDelegate(this ConditionComparison conditionComparison)
        {
            switch (conditionComparison)
            {
                case ConditionComparison.Equals:
                    return (v1, v2) => string.Compare(v1, v2, CultureInfo.InvariantCulture, CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase) == 0;

                case ConditionComparison.NotEquals:
                    return (v1, v2) => string.Compare(v1, v2, CultureInfo.InvariantCulture, CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreCase) != 0;

                case ConditionComparison.Contains:
                    return (v1, v2) => v1 != null && v2 != null && v1.IndexOf(v2, StringComparison.OrdinalIgnoreCase) >= 0;

                case ConditionComparison.StartsWith:
                    return (v1, v2) => v1 != null && v2 != null && v1.StartsWith(v2, StringComparison.OrdinalIgnoreCase);

                case ConditionComparison.EndsWith:
                    return (v1, v2) => v1 != null && v2 != null && v1.EndsWith(v2, StringComparison.OrdinalIgnoreCase);

                case ConditionComparison.Matches:
                    return (v1, v2) => v1 != null && v2 != null && Regex.IsMatch(v1, v2, default, Constants.REGEX_TIMEOUT);

                case ConditionComparison.ApproximateTo:
                    // Allows the difference of 25% of the string.
                    return (v1, v2) => v1 != null && v2 != null && v1.ToLowerInvariant().CalculateLevenshteinDistance(v2.ToLowerInvariant()) <= Math.Ceiling(v1.Length * 0.25);

                case ConditionComparison.GreaterThan:
                    return (v1, v2) => decimal.TryParse(v1, out var n1) && decimal.TryParse(v2, out var n2) && n1 > n2;
                    
                case ConditionComparison.LessThan:
                    return (v1, v2) => decimal.TryParse(v1, out var n1) && decimal.TryParse(v2, out var n2) && n1 < n2;

                case ConditionComparison.GreaterThanOrEquals:
                    return (v1, v2) => decimal.TryParse(v1, out var n1) && decimal.TryParse(v2, out var n2) && n1 >= n2;

                case ConditionComparison.LessThanOrEquals:
                    return (v1, v2) => decimal.TryParse(v1, out var n1) && decimal.TryParse(v2, out var n2) && n1 <= n2;

                case ConditionComparison.Exists:
                case ConditionComparison.NotExists:
                    throw new ArgumentException("The provided value is not a binary comparison", nameof(conditionComparison));

                default:
                    throw new ArgumentOutOfRangeException(nameof(conditionComparison));
            }
            
        }
    }
}
