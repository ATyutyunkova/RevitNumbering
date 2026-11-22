using System.Collections.Generic;
using System.Linq;

namespace RevitNumbering.Core
{
    public static class NumberingPlanner
    {
        public const string NoLevelName = "Без уровня";

        public static NumberingPlan Build(
            IEnumerable<ElementSnapshot> elements,
            NumberTemplate template,
            double rowTolerance)
        {
            var groups = elements
                .GroupBy(e => e.LevelName ?? "")
                .Select(g => new
                {
                    Name = g.Key,
                    Elevation = g.Max(e => e.LevelElevation),
                    Elements = g.ToList()
                })
                .OrderBy(g => g.Name.Length == 0 ? 1 : 0)
                .ThenBy(g => g.Elevation)
                .ToList();

            var levels = new List<NumberingLevel>(groups.Count);

            foreach (var group in groups)
            {
                var displayName = group.Name.Length == 0 ? NoLevelName : group.Name;
                var ordered = SpatialOrderer.Order(group.Elements, rowTolerance);
                var entries = new List<NumberedElement>(ordered.Count);

                for (var index = 0; index < ordered.Count; index++)
                {
                    var sequence = index + 1;
                    entries.Add(new NumberedElement(
                        ordered[index],
                        sequence,
                        displayName,
                        template.Format(sequence, displayName)));
                }

                levels.Add(new NumberingLevel(displayName, group.Name.Length == 0, group.Elevation, entries));
            }

            return new NumberingPlan(levels, levels.SelectMany(level => level.Elements).ToList());
        }
    }

    public sealed class NumberingPlan
    {
        public NumberingPlan(IReadOnlyList<NumberingLevel> levels, IReadOnlyList<NumberedElement> elements)
        {
            Levels = levels;
            Elements = elements;
        }

        public IReadOnlyList<NumberingLevel> Levels { get; }

        public IReadOnlyList<NumberedElement> Elements { get; }
    }

    public sealed class NumberingLevel
    {
        public NumberingLevel(
            string name,
            bool isWithoutLevel,
            double elevation,
            IReadOnlyList<NumberedElement> elements)
        {
            Name = name;
            IsWithoutLevel = isWithoutLevel;
            Elevation = elevation;
            Elements = elements;
        }

        public string Name { get; }

        public bool IsWithoutLevel { get; }

        public double Elevation { get; }

        public IReadOnlyList<NumberedElement> Elements { get; }
    }

    public sealed class NumberedElement
    {
        public NumberedElement(ElementSnapshot element, int sequence, string levelName, string newValue)
        {
            Element = element;
            Sequence = sequence;
            LevelName = levelName;
            NewValue = newValue;
        }

        public ElementSnapshot Element { get; }

        public int Sequence { get; }

        public string LevelName { get; }

        public string NewValue { get; }
    }
}
