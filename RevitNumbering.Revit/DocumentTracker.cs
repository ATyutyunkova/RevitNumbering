using System;
using Autodesk.Revit.DB;

namespace RevitNumbering.Revit
{
    internal sealed class DocumentTracker
    {
        public event Action<Document> ActiveDocumentChanged;

        public Document ActiveDocument { get; private set; }

        public void SetActiveDocument(Document document)
        {
            if (ReferenceEquals(ActiveDocument, document))
            {
                return;
            }

            ActiveDocument = document;
            ActiveDocumentChanged?.Invoke(document);
        }
    }
}
