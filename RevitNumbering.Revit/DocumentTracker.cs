using System;
using Autodesk.Revit.DB;

namespace RevitNumbering.Revit
{
    /// <summary>
    /// Хранит активный документ и уведомляет окно о переключении.
    /// Сравнение выполняется через <see cref="DocumentIdentity"/>, а не по ссылке.
    /// </summary>
    internal sealed class DocumentTracker
    {
        public event Action<Document> ActiveDocumentChanged;

        public Document ActiveDocument { get; private set; }

        public void SetActiveDocument(Document document)
        {
            if (DocumentIdentity.AreSame(ActiveDocument, document))
            {
                ActiveDocument = document;
                return;
            }

            ActiveDocument = document;
            Logger.Info($"Активный документ: {document?.Title ?? "нет"}");
            ActiveDocumentChanged?.Invoke(document);
        }
    }
}
