using Autodesk.Revit.DB;

namespace RevitNumbering.Revit.Model
{
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
