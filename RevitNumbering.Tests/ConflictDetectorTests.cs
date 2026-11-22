using System.Linq;
using NUnit.Framework;
using RevitNumbering.Core;

namespace RevitNumbering.Tests
{
    [TestFixture]
    public class ConflictDetectorTests
    {
        [Test]
        public void Flags_conflict_with_element_outside_scope()
        {
            var scoped = new[] { Element(1, "1", "A", 1) };
            var all = new[]
            {
                Element(1, "1", "A", 1),
                Element(2, "1", "Д-1", 1)
            };
            var plan = NumberingPlanner.Build(scoped, NumberTemplate.Parse("Д-{N}"), 0.5);

            var conflicts = ConflictDetector.Detect(plan, all);

            Assert.That(conflicts, Has.Count.EqualTo(1));
            Assert.That(conflicts[0].ElementId, Is.EqualTo(1));
            Assert.That(conflicts[0].NewValue, Is.EqualTo("Д-1"));
            Assert.That(conflicts[0].ConflictingElementIds, Is.EqualTo(new[] { 2 }));
        }

        [Test]
        public void Ignores_current_values_of_elements_inside_scope()
        {
            var scoped = new[]
            {
                Element(1, "1", "X", 2),
                Element(2, "1", "Д-1", 1)
            };
            var plan = NumberingPlanner.Build(scoped, NumberTemplate.Parse("Д-{N}"), 0.5);

            var conflicts = ConflictDetector.Detect(plan, scoped);

            Assert.That(conflicts, Is.Empty);
        }

        [Test]
        public void Flags_duplicate_new_values_inside_plan()
        {
            var scoped = new[]
            {
                Element(1, "A", "", 1),
                Element(2, "B", "", 1)
            };
            var plan = NumberingPlanner.Build(scoped, NumberTemplate.Parse("{N}"), 0.5);

            var conflicts = ConflictDetector.Detect(plan, scoped);

            Assert.That(conflicts, Has.Count.EqualTo(2));
            Assert.That(conflicts.Single(c => c.ElementId == 1).ConflictingElementIds, Is.EqualTo(new[] { 2 }));
            Assert.That(conflicts.Single(c => c.ElementId == 2).ConflictingElementIds, Is.EqualTo(new[] { 1 }));
        }

        [Test]
        public void Returns_no_conflicts_when_values_are_unique()
        {
            var scoped = new[] { Element(1, "1", "", 1) };
            var plan = NumberingPlanner.Build(scoped, NumberTemplate.Parse("Д-{N}"), 0.5);

            var conflicts = ConflictDetector.Detect(plan, scoped);

            Assert.That(conflicts, Is.Empty);
        }

        private static ElementSnapshot Element(int id, string level, string currentValue, double y)
        {
            return new ElementSnapshot
            {
                ElementId = id,
                LevelName = level,
                CurrentValue = currentValue,
                HasLocation = true,
                X = 1,
                Y = y
            };
        }
    }
}
