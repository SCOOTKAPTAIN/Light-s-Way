using System;
using System.ComponentModel;
using System.Reflection;

namespace NueGames.NueDeck.Scripts.Utils
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum value)
        {
            if (value == null) return string.Empty;
            var fi = value.GetType().GetField(value.ToString());
            if (fi == null) return value.ToString();
            var attrs = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attrs.Length > 0 ? attrs[0].Description : value.ToString();
        }
    }
}
