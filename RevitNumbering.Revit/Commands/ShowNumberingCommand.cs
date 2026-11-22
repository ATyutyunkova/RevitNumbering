using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitNumbering.Revit.Commands
{
    [Transaction(TransactionMode.Manual)]
    public sealed class ShowNumberingCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                NumberingWindowController.Show(commandData.Application);
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                Logger.Error("Не удалось открыть окно нумерации", ex);
                TaskDialog.Show("Нумерация элементов", "Не удалось открыть окно. Подробности в логе.");
                return Result.Failed;
            }
        }
    }
}
