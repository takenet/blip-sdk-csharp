using System;
using System.Text.RegularExpressions;

namespace Take.Blip.Builder
{
    public enum ConditionComparison
    {
        Equals,
        NotEquals,
        Contains,
        StartsWith,
        EndsWith,
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
