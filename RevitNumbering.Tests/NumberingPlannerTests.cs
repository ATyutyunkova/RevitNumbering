using System.Linq;
using NUnit.Framework;
using RevitNumbering.Core;

namespace RevitNumbering.Tests
{
    [TestFixture]
    public class NumberingPlannerTests
    {
        [Test]
        public void Resets_sequence_on_each_level_and_orders_levels_by_elevation()
        {
            var elements = new[]
            {
                Element(1, "B", 3, 1, 1),
                Element(2, "A", 0, 1, 1),
                Element(3, "A", 0, 1, 2)
            };
            var template = NumberTemplate.Parse("{Уровень}-{N}");

            var plan = NumberingPlanner.Build(elements, template, 0.5);

            Assert.That(plan.Levels.Select(l => l.Name), Is.EqualTo(new[] { "A", "B" }));
            Assert.That(plan.Elements.Select(e => e.NewValue), Is.EqualTo(new[] { "A-1", "A-2", "B-1" }));
            Assert.That(plan.Elements.Select(e => e.Element.ElementId), Is.EqualTo(new[] { 3, 2, 1 }));
        }

        [Test]
        public void Elements_without_level_go_last_with_own_counter()
        {
            var elements = new[]
            {
                Element(1, "", 0, 1, 1),
                Element(2, "1", 0, 1, 1)
            };
            var template = NumberTemplate.Parse("{Уровень}-{N:00}");

            var plan = NumberingPlanner.Build(elements, template, 0.5);

            Assert.That(plan.Elements.Select(e => e.NewValue), Is.EqualTo(new[] { "1-01", "Без уровня-01" }));
        }

        [Test]
        public void Elements_without_location_are_numbered_last_within_level_by_id()
        {
            var elements = new[]
            {
                Element(10, "1", 0, 1, 5),
                NoLocation(2, "1"),
                NoLocation(3, "1")
            };
            var template = NumberTemplate.Parse("{N}");

            var plan = NumberingPlanner.Build(elements, template, 0.5);

            Assert.That(plan.Elements.Select(e => e.Element.ElementId), Is.EqualTo(new[] { 10, 2, 3 }));
            Assert.That(plan.Elements.Select(e => e.Sequence), Is.EqualTo(new[] { 1, 2, 3 }));
        }

        [Test]
        public void Sequence_continues_across_rows_within_level()
        {
            var elements = new[]
            {
                Element(1, "1", 0, 1, 10),
                Element(2, "1", 0, 2, 10),
                Element(3, "1", 0, 1, 5)
            };
            var template = NumberTemplate.Parse("{N}");

            var plan = NumberingPlanner.Build(elements, template, 0.5);

            Assert.That(plan.Elements.Select(e => e.NewValue), Is.EqualTo(new[] { "1", "2", "3" }));
        }

        [Test]
        public void Empty_input_returns_empty_plan()
        {
            var plan = NumberingPlanner.Build(new ElementSnapshot[0], NumberTemplate.Parse("{N}"), 0.5);

            Assert.That(plan.Levels, Is.Empty);
            Assert.That(plan.Elements, Is.Empty);
        }

        private static ElementSnapshot Element(int id, string level, double elevation, double x, double y)
        {
            return new ElementSnapshot
            {
                ElementId = id,
                LevelName = level,
                LevelElevation = elevation,
                HasLocation = true,
                X = x,
                Y = y
            };
        }

        private static ElementSnapshot NoLocation(int id, string level)
        {
            return new ElementSnapshot
            {
                ElementId = id,
                LevelName = level,
                HasLocation = false
            };
        }
    }
}
