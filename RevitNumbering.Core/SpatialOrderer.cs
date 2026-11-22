using System;
using System.Collections.Generic;
using System.Linq;

namespace RevitNumbering.Core
{
    public static class SpatialOrderer
    {
        public static IReadOnlyList<ElementSnapshot> Order(IEnumerable<ElementSnapshot> elements, double rowTolerance)
        {
            if (elements == null)
            {
                throw new ArgumentNullException(nameof(elements));
            }

            var tolerance = rowTolerance < 0 ? 0 : rowTolerance;
            var all = elements.ToList();

            var located = all
                .Where(e => e.HasLocation && IsFinite(e.X) && IsFinite(e.Y))
                .OrderByDescending(e => e.Y)
                .ThenBy(e => e.X)
                .ThenBy(e => e.ElementId)
                .ToList();

            var result = new List<ElementSnapshot>(all.Count);
            var index = 0;

            while (index < located.Count)
            {
                var anchorY = located[index].Y;
                var row = new List<ElementSnapshot> { located[index] };
                index++;

                while (index < located.Count && anchorY - located[index].Y <= tolerance)
                {
                    row.Add(located[index]);
                    index++;
                }

                result.AddRange(row.OrderBy(e => e.X).ThenBy(e => e.ElementId));
            }

            result.AddRange(all
                .Where(e => !e.HasLocation || !IsFinite(e.X) || !IsFinite(e.Y))
                .OrderBy(e => e.ElementId));

            return result;
        }

        private static bool IsFinite(double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value);
        }
    }
}
