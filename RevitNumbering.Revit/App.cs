using System;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;

namespace RevitNumbering.Revit
{
    public sealed class App : IExternalApplication
    {
        private const string TabName = "Нумерация";

        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                AddinContext.Initialize();
                CreateRibbon(application);
                application.ViewActivated += OnViewActivated;
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка запуска плагина", ex);
            }

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            if (application != null)
            {
                application.ViewActivated -= OnViewActivated;
            }

            return Result.Succeeded;
        }

        private static void OnViewActivated(object sender, ViewActivatedEventArgs e)
        {
            AddinContext.DocumentTracker.SetActiveDocument(e.Document);
        }

        private static void CreateRibbon(UIControlledApplication application)
        {
            try
            {
                application.CreateRibbonTab(TabName);
            }
            catch (Autodesk.Revit.Exceptions.ArgumentException)
            {
            }

            var panel = application.CreateRibbonPanel(TabName, "Инструменты");
            var assemblyPath = typeof(App).Assembly.Location;
            var button = new PushButtonData(
                "RevitNumberingShow",
                "Нумерация\nэлементов",
                assemblyPath,
                typeof(Commands.ShowNumberingCommand).FullName)
            {
                ToolTip = "Просмотр и нумерация элементов выбранной категории"
            };

            panel.AddItem(button);
        }
    }
}
