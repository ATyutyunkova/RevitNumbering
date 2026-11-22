using System;
using System.Globalization;

namespace RevitNumbering.Core
{
    public static class ElementSearch
    {
        public static bool Matches(ElementSnapshot element, string newValue, string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return true;
            }

            return Contains(element.Name, query)
                   || Contains(element.LevelName, query)
                   || Contains(element.CurrentValue, query)
                   || Contains(newValue, query)
                   || Contains(element.ElementId.ToString(CultureInfo.InvariantCulture), query);
        }

        private static bool Contains(string value, string query)
        {
            return !string.IsNullOrEmpty(value)
                   && value.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
