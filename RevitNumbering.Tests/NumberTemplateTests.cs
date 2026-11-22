using NUnit.Framework;
using RevitNumbering.Core;

namespace RevitNumbering.Tests
{
    [TestFixture]
    public class NumberTemplateTests
    {
        [Test]
        public void Formats_level_and_zero_padded_sequence()
        {
            var template = NumberTemplate.Parse("Д-{Уровень}-{N:000}");

            Assert.That(template.IsValid, Is.True, template.Error);
            Assert.That(template.Format(7, "2"), Is.EqualTo("Д-2-007"));
        }

        [Test]
        public void Formats_plain_sequence_without_padding()
        {
            var template = NumberTemplate.Parse("ОК-{N}");

            Assert.That(template.IsValid, Is.True, template.Error);
            Assert.That(template.Format(12, "1"), Is.EqualTo("ОК-12"));
        }

        [Test]
        public void Supports_standard_numeric_format()
        {
            var template = NumberTemplate.Parse("{N:D3}");

            Assert.That(template.IsValid, Is.True, template.Error);
            Assert.That(template.Format(5, "1"), Is.EqualTo("005"));
        }

        [Test]
        public void Token_names_are_case_insensitive()
        {
            var template = NumberTemplate.Parse("{уровень}/{n:00}");

            Assert.That(template.IsValid, Is.True, template.Error);
            Assert.That(template.Format(3, "L1"), Is.EqualTo("L1/03"));
        }

        [Test]
        public void Empty_level_name_is_allowed()
        {
            var template = NumberTemplate.Parse("{Уровень}-{N}");

            Assert.That(template.IsValid, Is.True, template.Error);
            Assert.That(template.Format(1, null), Is.EqualTo("-1"));
        }

        [Test]
        public void Rejects_template_without_sequence_token()
        {
            var template = NumberTemplate.Parse("Д-{Уровень}");

            Assert.That(template.IsValid, Is.False);
            Assert.That(template.Error, Does.Contain("{N}"));
        }

        [Test]
        public void Rejects_unknown_token()
        {
            var template = NumberTemplate.Parse("Д-{Здание}-{N}");

            Assert.That(template.IsValid, Is.False);
            Assert.That(template.Error, Does.Contain("{Здание}"));
        }

        [Test]
        public void Rejects_unclosed_token()
        {
            var template = NumberTemplate.Parse("Д-{N:000");

            Assert.That(template.IsValid, Is.False);
            Assert.That(template.Error, Is.Not.Empty);
        }

        [Test]
        public void Rejects_invalid_number_format()
        {
            var template = NumberTemplate.Parse("{N:ZZZ}");

            Assert.That(template.IsValid, Is.False);
            Assert.That(template.Error, Is.Not.Empty);
        }

        [Test]
        public void Rejects_empty_template()
        {
            var template = NumberTemplate.Parse("");

            Assert.That(template.IsValid, Is.False);
            Assert.That(template.Error, Is.Not.Empty);
        }

        [Test]
        public void Format_throws_for_invalid_template()
        {
            var template = NumberTemplate.Parse("ABC");

            Assert.Throws<System.InvalidOperationException>(() => template.Format(1, "1"));
        }
    }
}
