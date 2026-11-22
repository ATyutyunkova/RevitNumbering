using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace RevitNumbering.Core
{
    public static class SettingsStore
    {
        public static NumberingSettings Load(string path, out string error)
        {
            error = null;

            try
            {
                if (!File.Exists(path))
                {
                    return new NumberingSettings();
                }

                var json = File.ReadAllText(path, Encoding.UTF8);
                var settings = JsonConvert.DeserializeObject<NumberingSettings>(json);
                if (settings == null)
                {
                    error = "Файл настроек пуст.";
                    return new NumberingSettings();
                }

                settings.Templates = settings.Templates ?? new Dictionary<string, string>();
                return settings;
            }
            catch (Exception ex) when (
                ex is IOException ||
                ex is UnauthorizedAccessException ||
                ex is JsonException)
            {
                error = ex.Message;
                return new NumberingSettings();
            }
        }

        public static void Save(string path, NumberingSettings settings)
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            var tempPath = path + ".tmp";
            File.WriteAllText(tempPath, json, new UTF8Encoding(false));

            if (File.Exists(path))
            {
                File.Replace(tempPath, path, null);
            }
            else
            {
                File.Move(tempPath, path);
            }
        }
    }
}
