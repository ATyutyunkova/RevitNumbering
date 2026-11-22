using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using RevitNumbering.Core;

namespace RevitNumbering.Tests
{
    [TestFixture]
    public class SettingsStoreTests
    {
        private string _directory;
        private string _path;

        [SetUp]
        public void SetUp()
        {
            _directory = Path.Combine(Path.GetTempPath(), "RevitNumberingTests", Guid.NewGuid().ToString("N"));
            _path = Path.Combine(_directory, "settings.json");
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_directory))
            {
                Directory.Delete(_directory, true);
            }
        }

        [Test]
        public void Load_returns_defaults_when_file_is_missing()
        {
            var settings = SettingsStore.Load(_path, out var error);

            Assert.That(error, Is.Null);
            Assert.That(settings.RowToleranceMm, Is.EqualTo(1000));
            Assert.That(settings.Scope, Is.EqualTo(NumberingScope.All));
            Assert.That(settings.Templates, Is.Empty);
        }

        [Test]
        public void Save_and_load_roundtrip()
        {
            var settings = new NumberingSettings
            {
                RowToleranceMm = 750,
                Scope = NumberingScope.Selection,
                LastCategory = "OST_Walls",
                Templates = new Dictionary<string, string> { ["OST_Walls"] = "С-{Уровень}-{N:000}" }
            };

            SettingsStore.Save(_path, settings);
            var loaded = SettingsStore.Load(_path, out var error);

            Assert.That(error, Is.Null);
            Assert.That(loaded.RowToleranceMm, Is.EqualTo(750));
            Assert.That(loaded.Scope, Is.EqualTo(NumberingScope.Selection));
            Assert.That(loaded.LastCategory, Is.EqualTo("OST_Walls"));
            Assert.That(loaded.Templates["OST_Walls"], Is.EqualTo("С-{Уровень}-{N:000}"));
        }

        [Test]
        public void Save_creates_parent_directory()
        {
            SettingsStore.Save(_path, new NumberingSettings());

            Assert.That(File.Exists(_path), Is.True);
        }

        [Test]
        public void Load_returns_defaults_for_corrupt_file()
        {
            Directory.CreateDirectory(_directory);
            File.WriteAllText(_path, "{ not json");

            var settings = SettingsStore.Load(_path, out var error);

            Assert.That(error, Is.Not.Null.And.Not.Empty);
            Assert.That(settings.RowToleranceMm, Is.EqualTo(1000));
            Assert.That(settings.Scope, Is.EqualTo(NumberingScope.All));
        }
    }
}
