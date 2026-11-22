using System;
using Autodesk.Revit.DB;

namespace RevitNumbering.Revit
{
    /// <summary>
    /// Сравнивает документы по пути и имени. Revit может вернуть разные managed-обёртки
    /// одного документа, поэтому сравнивать ссылки на объект нельзя.
    /// </summary>
    internal static class DocumentIdentity
    {
        public static bool AreSame(Document first, Document second)
        {
            if (ReferenceEquals(first, second))
            {
                return true;
            }

            if (first == null || second == null || !first.IsValidObject || !second.IsValidObject)
            {
                return false;
            }

            return string.Equals(first.PathName, second.PathName, StringComparison.OrdinalIgnoreCase)
                   && string.Equals(first.Title, second.Title, StringComparison.Ordinal);
        }
    }
}
