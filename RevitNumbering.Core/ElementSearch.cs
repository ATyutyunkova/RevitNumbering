using System;
using System.Globalization;

namespace RevitNumbering.Core
{
    /// <summary>
    /// Фильтр строк таблицы: пустой запрос пропускает все элементы, иначе ищет подстроку
    /// без учёта регистра в имени, уровне, текущем и новом значении, ElementId.
    /// </summary>
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
