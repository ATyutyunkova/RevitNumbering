using System;
using System.IO;

namespace RevitNumbering.Revit
{
    internal static class Logger
    {
        private static readonly object Sync = new object();
        private static string _directory;

        public static void Initialize()
        {
            _directory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "RevitNumbering",
                "logs");

            Directory.CreateDirectory(_directory);
        }

        public static void Info(string message)
        {
            Write("INFO", message);
        }

        public static void Error(string message, Exception exception = null)
        {
            Write("ERROR", exception == null ? message : message + Environment.NewLine + exception);
        }

        private static void Write(string level, string message)
        {
            try
            {
                lock (Sync)
                {
                    if (_directory == null)
                    {
                        Initialize();
                    }

                    var path = Path.Combine(_directory, $"log-{DateTime.Now:yyyyMMdd}.log");
                    File.AppendAllText(
                        path,
                        $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}{Environment.NewLine}");
                }
            }
            catch
            {
            }
        }
    }
}
