using System;

namespace NueGames.NueDeck.Scripts.Utils
{
    /// <summary>
    /// Use this attribute to provide a human-friendly display name for enum values.
    /// Example: [EnumDisplayName("God's Angel")] GodsAngel
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class EnumDisplayNameAttribute : Attribute
    {
        public string DisplayName { get; }
        public EnumDisplayNameAttribute(string displayName) => DisplayName = displayName;
    }
}
