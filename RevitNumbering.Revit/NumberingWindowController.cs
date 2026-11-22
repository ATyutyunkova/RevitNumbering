using System.Windows.Interop;
using Autodesk.Revit.UI;
using RevitNumbering.Revit.Views;

namespace RevitNumbering.Revit
{
    internal static class NumberingWindowController
    {
        private static NumberingWindow _window;

        public static void Show(UIApplication application)
        {
            if (_window != null)
            {
                _window.Activate();
                return;
            }

            AddinContext.DocumentTracker.SetActiveDocument(application.ActiveUIDocument?.Document);

            _window = new NumberingWindow();
            new WindowInteropHelper(_window) { Owner = application.MainWindowHandle };
            _window.Closed += (sender, args) => _window = null;
            _window.Show();
        }
    }
}
