using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace RevitNumbering.Core
{
    /// <summary>
    /// Загрузка и атомарное сохранение настроек в JSON.
    /// Если файла нет или он повреждён, возвращаются значения по умолчанию и текст ошибки.
    /// </summary>
    public static class SettingsStore
    {
        /// <summary>
        /// Читает настройки по указанному пути. <paramref name="error"/> = null при успехе.
        /// </summary>
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

        /// <summary>
        /// Сохраняет настройки: пишет временный файл и заменяет им целевой,
        /// чтобы не оставить повреждённый JSON при сбое.
        /// </summary>
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
