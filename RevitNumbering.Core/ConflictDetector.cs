using System;
using System.Collections.Generic;
using System.Linq;

namespace RevitNumbering.Core
{
    public static class ConflictDetector
    {
        public static IReadOnlyList<NumberConflict> Detect(
            NumberingPlan plan,
            IEnumerable<ElementSnapshot> allCategoryElements)
        {
            var scopeIds = new HashSet<int>(plan.Elements.Select(e => e.Element.ElementId));

            var outsideByValue = new Dictionary<string, List<int>>(StringComparer.Ordinal);
            foreach (var element in allCategoryElements)
            {
                if (scopeIds.Contains(element.ElementId))
                {
                    continue;
                }

                var value = element.CurrentValue ?? "";
                if (!outsideByValue.TryGetValue(value, out var ids))
                {
                    ids = new List<int>();
                    outsideByValue.Add(value, ids);
                }

                ids.Add(element.ElementId);
            }

            var planByValue = new Dictionary<string, List<int>>(StringComparer.Ordinal);
            foreach (var entry in plan.Elements)
            {
                if (!planByValue.TryGetValue(entry.NewValue, out var ids))
                {
                    ids = new List<int>();
                    planByValue.Add(entry.NewValue, ids);
                }

                ids.Add(entry.Element.ElementId);
            }

            var conflicts = new List<NumberConflict>();

            foreach (var entry in plan.Elements)
            {
                var conflictingIds = new SortedSet<int>();

                if (outsideByValue.TryGetValue(entry.NewValue, out var outsideIds))
                {
                    foreach (var id in outsideIds)
                    {
                        conflictingIds.Add(id);
                    }
                }

                if (planByValue.TryGetValue(entry.NewValue, out var scopedIds))
                {
                    foreach (var id in scopedIds)
                    {
                        if (id != entry.Element.ElementId)
                        {
                            conflictingIds.Add(id);
                        }
                    }
                }

                if (conflictingIds.Count > 0)
                {
                    conflicts.Add(new NumberConflict(
                        entry.Element.ElementId,
                        entry.NewValue,
                        conflictingIds.ToList()));
                }
            }

            return conflicts;
        }
    }

    public sealed class NumberConflict
    {
        public NumberConflict(int elementId, string newValue, IReadOnlyList<int> conflictingElementIds)
        {
            ElementId = elementId;
            NewValue = newValue;
            ConflictingElementIds = conflictingElementIds;
        }

        public int ElementId { get; }

        public string NewValue { get; }

        public IReadOnlyList<int> ConflictingElementIds { get; }
    }
}
