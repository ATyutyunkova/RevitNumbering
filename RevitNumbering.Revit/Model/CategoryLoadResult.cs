using System.Collections.Generic;
using RevitNumbering.Core;

namespace RevitNumbering.Revit.Model
{
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
