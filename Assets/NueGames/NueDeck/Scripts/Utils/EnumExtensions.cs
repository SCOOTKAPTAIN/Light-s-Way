using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace NueGames.NueDeck.Scripts.Utils
{
    public static class EnumExtensions
    {
        /// <summary>
        /// Returns the EnumDisplayName if present, otherwise a prettified version of the identifier.
        /// </summary>
        public static string ToDisplayName(this Enum value)
        {
            if (value == null) return string.Empty;

            var member = value.GetType().GetMember(value.ToString()).FirstOrDefault();
            if (member != null)
            {
                var attr = member.GetCustomAttribute<EnumDisplayNameAttribute>();
                if (attr != null && !string.IsNullOrEmpty(attr.DisplayName))
                    return attr.DisplayName;
            }

            // Fallback: insert spaces before capitals (e.g. GodsAngel -> Gods Angel)
            var s = value.ToString();
            // Also replace underscore with space
            s = s.Replace("_", " ");
            s = Regex.Replace(s, "([a-z])([A-Z])", "$1 $2");
            return s;
        }
    }
}
