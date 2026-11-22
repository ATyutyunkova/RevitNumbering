using System.Collections.Generic;
using RevitNumbering.Core;

namespace RevitNumbering.Revit.Model
{
    /// <summary>
    /// Результат чтения категории: <see cref="All"/> — вся категория (для поиска конфликтов),
    /// <see cref="Scoped"/> — только элементы в охвате нумерации.
    /// </summary>
    internal sealed class CategoryLoadResult
    {
        public CategoryLoadResult(IReadOnlyList<ElementSnapshot> all, IReadOnlyList<ElementSnapshot> scoped)
        {
            All = all;
            Scoped = scoped;
        }

        public IReadOnlyList<ElementSnapshot> All { get; }

        public IReadOnlyList<ElementSnapshot> Scoped { get; }
    }
}
