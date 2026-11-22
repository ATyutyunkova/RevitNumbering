using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using RevitNumbering.Core;

namespace RevitNumbering.Tests
{
    [TestFixture]
    public class SpatialOrdererTests
    {
        [Test]
        public void Orders_rows_top_down_and_left_to_right()
        {
            var elements = new[]
            {
                At(1, 10, 10),
                At(2, 1, 10),
                At(3, 10, 9.5),
                At(4, 1, 9.5)
            };

            var ordered = SpatialOrderer.Order(elements, 0.4);

            Assert.That(Ids(ordered), Is.EqualTo(new[] { 2, 1, 4, 3 }));
        }

        [Test]
        public void Elements_within_tolerance_share_a_row()
        {
            var elements = new[]
            {
                At(1, 5, 10),
                At(2, 1, 9.1),
                At(3, 3, 8.99)
            };

            var ordered = SpatialOrderer.Order(elements, 1);

            Assert.That(Ids(ordered), Is.EqualTo(new[] { 2, 1, 3 }));
        }

        [Test]
        public void Lower_row_starts_after_tolerance()
        {
            var elements = new[]
            {
                At(1, 1, 10),
                At(2, 1, 8.99)
            };

            var ordered = SpatialOrderer.Order(elements, 1);

            Assert.That(Ids(ordered), Is.EqualTo(new[] { 1, 2 }));
        }

        [Test]
        public void Elements_without_location_go_last_by_id()
        {
            var elements = new[]
            {
                At(5, 1, 1),
                NoLocation(2),
                At(7, 2, 2),
                NoLocation(9)
            };

            var ordered = SpatialOrderer.Order(elements, 0.5);

            Assert.That(Ids(ordered), Is.EqualTo(new[] { 7, 5, 2, 9 }));
        }

        private static ElementSnapshot At(int id, double x, double y)
        {
            return new ElementSnapshot
            {
                ElementId = id,
                HasLocation = true,
                X = x,
                Y = y
            };
        }

        private static ElementSnapshot NoLocation(int id)
        {
            return new ElementSnapshot { ElementId = id, HasLocation = false };
        }

        private static int[] Ids(IEnumerable<ElementSnapshot> elements)
        {
            return elements.Select(e => e.ElementId).ToArray();
        }
    }
}
