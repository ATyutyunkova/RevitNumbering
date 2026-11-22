using Autodesk.Revit.DB;

namespace RevitNumbering.Revit.Model
{
    /// <summary>
    /// Документ и прочитанные из него элементы: документ нужен для проверки перед записью.
    /// </summary>
    internal sealed class LoadPayload
    {
        public LoadPayload(Document document, CategoryLoadResult result)
        {
            Document = document;
            Result = result;
        }

        public Document Document { get; }

        public CategoryLoadResult Result { get; }
    }
}
