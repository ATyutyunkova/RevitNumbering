using NUnit.Framework;
using RevitNumbering.Core;

namespace RevitNumbering.Tests
{
    [TestFixture]
    public class ElementSearchTests
    {
        private readonly ElementSnapshot _element = new ElementSnapshot
        {
            ElementId = 123456,
            Name = "Базовая стена",
            LevelName = "Уровень 1",
            CurrentValue = "СТ-01"
        };

        [Test]
        public void Empty_query_matches_any_row()
        {
            Assert.That(ElementSearch.Matches(_element, "Д-1", null), Is.True);
            Assert.That(ElementSearch.Matches(_element, "Д-1", ""), Is.True);
            Assert.That(ElementSearch.Matches(_element, "Д-1", "   "), Is.True);
        }

        [Test]
        public void Matches_name_case_insensitively_by_partial_text()
        {
            Assert.That(ElementSearch.Matches(_element, "Д-1", "стена"), Is.True);
        }

        [Test]
        public void Matches_level_name()
        {
            Assert.That(ElementSearch.Matches(_element, "Д-1", "уровень"), Is.True);
        }

        [Test]
        public void Matches_current_and_new_values()
        {
            Assert.That(ElementSearch.Matches(_element, "Д-1", "ст-01"), Is.True);
            Assert.That(ElementSearch.Matches(_element, "Д-1", "д-1"), Is.True);
        }

        [Test]
        public void Matches_element_id()
        {
            Assert.That(ElementSearch.Matches(_element, "Д-1", "2345"), Is.True);
        }

        [Test]
        public void Returns_false_when_nothing_matches()
        {
            Assert.That(ElementSearch.Matches(_element, "Д-1", "окно"), Is.False);
        }
    }
}
