using System.Linq;
using RevitNumbering.Core;

namespace RevitNumbering.Revit.ViewModels
{
    public sealed class RowViewModel
    {
        private readonly NumberedElement _entry;

        public RowViewModel(NumberedElement entry, NumberConflict conflict)
        {
            _entry = entry;
            ConflictText = conflict == null
                ? ""
                : "Конфликт: " + string.Join(", ", conflict.ConflictingElementIds.Select(id => "#" + id));
        }

        public int ElementId => _entry.Element.ElementId;

        public string Name => _entry.Element.Name;

        public string LevelName => _entry.LevelName;

        public string CurrentValue => _entry.Element.CurrentValue;

        public string NewValue => _entry.NewValue;

        public bool HasConflict => ConflictText.Length > 0;

        public string ConflictText { get; }

        public ElementSnapshot Element => _entry.Element;
    }
}
