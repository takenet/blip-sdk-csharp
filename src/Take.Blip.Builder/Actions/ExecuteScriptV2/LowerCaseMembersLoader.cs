using System.Linq;
using System.Reflection;
using Microsoft.ClearScript;

namespace Take.Blip.Builder.Actions.ExecuteScriptV2
{
    /// <summary>
    /// Global transformation to use lower case .NET members in script
    /// </summary>
    public class LowerCaseMembersLoader : CustomAttributeLoader
    {
        /// <summary>
        /// Loads custom attributes from a resource using lower case member names
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="inherit"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public override T[] LoadCustomAttributes<T>(ICustomAttributeProvider resource, bool inherit)
        {
            var declaredAttributes = base.LoadCustomAttributes<T>(resource, inherit);
            if (!declaredAttributes.Any() && typeof(T) == typeof(ScriptMemberAttribute) &&
                resource is MemberInfo member)
            {
                var lowerCamelCaseName =
                    char.ToLowerInvariant(member.Name[0]) + member.Name.Substring(1);
                return new[] { new ScriptMemberAttribute(lowerCamelCaseName) } as T[];
            }

            return declaredAttributes;
        }
    }
}