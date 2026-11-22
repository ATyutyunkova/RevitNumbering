using System;
using System.IO;
using RevitNumbering.Core;

namespace RevitNumbering.Revit
{
    internal static class AddinContext
    {
        public static RevitEventExecutor Executor { get; private set; }

        public static DocumentTracker DocumentTracker { get; private set; }

        public static NumberingSettings Settings { get; private set; }

        public static string SettingsPath { get; private set; }

        public static void Initialize()
        {
            Logger.Initialize();

            SettingsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "RevitNumbering",
                "settings.json");

            Settings = SettingsStore.Load(SettingsPath, out var error);
            if (!string.IsNullOrEmpty(error))
            {
                Logger.Error("Не удалось загрузить настройки: " + error);
            }

            Executor = new RevitEventExecutor();
            DocumentTracker = new DocumentTracker();
        }

        public static void SaveSettings()
        {
            try
            {
                SettingsStore.Save(SettingsPath, Settings);
            }
            catch (Exception ex)
            {
                Logger.Error("Не удалось сохранить настройки", ex);
            }
        }
    }
}
