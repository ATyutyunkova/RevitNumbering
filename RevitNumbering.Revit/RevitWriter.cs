using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using RevitNumbering.Core;

namespace RevitNumbering.Revit
{
    internal static class RevitWriter
    {
        public static ApplyResult Apply(
            Document document,
            BuiltInParameter parameter,
            IReadOnlyList<NumberedElement> entries)
        {
            var result = new ApplyResult();

            using (var transaction = new Transaction(document, "Нумерация элементов"))
            {
                var options = transaction.GetFailureHandlingOptions();
                options.SetFailuresPreprocessor(new WarningSuppressor());
                transaction.SetFailureHandlingOptions(options);

                transaction.Start();
                try
                {
                    foreach (var entry in entries)
                    {
                        ApplyOne(document, parameter, entry, result);
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.RollBack();
                    throw;
                }
            }

            return result;
        }

        private static void ApplyOne(
            Document document,
            BuiltInParameter parameter,
            NumberedElement entry,
            ApplyResult result)
        {
            try
            {
                var element = document.GetElement(ToElementId(entry.Element.ElementId));
                if (element == null)
                {
                    result.Skipped.Add($"{entry.Element.ElementId}: элемент удалён");
                    return;
                }

                var value = element.get_Parameter(parameter);
                if (value == null)
                {
                    result.Skipped.Add($"{entry.Element.ElementId}: параметр не найден");
                    return;
                }

                if (value.IsReadOnly)
                {
                    result.Skipped.Add($"{entry.Element.ElementId}: параметр только для чтения");
                    return;
                }

                if ((value.AsString() ?? "") == entry.NewValue)
                {
                    result.Unchanged++;
                    return;
                }

                if (!value.Set(entry.NewValue))
                {
                    result.Skipped.Add($"{entry.Element.ElementId}: Revit отклонил изменение");
                    return;
                }

                result.Applied++;
            }
            catch (Exception ex)
            {
                result.Skipped.Add($"{entry.Element.ElementId}: {ex.Message}");
            }
        }

        private static ElementId ToElementId(int id)
        {
#if REVIT2024
            return new ElementId((long) id);
#else
            return new ElementId(id);
#endif
        }

        private sealed class WarningSuppressor : IFailuresPreprocessor
        {
            public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
            {
                foreach (var failure in failuresAccessor.GetFailureMessages())
                {
                    if (failure.GetSeverity() == FailureSeverity.Warning)
                    {
                        failuresAccessor.DeleteWarning(failure);
                    }
                }

                return FailureProcessingResult.Continue;
            }
        }
    }

    internal sealed class ApplyResult
    {
        public int Applied { get; set; }

        public int Unchanged { get; set; }

        public List<string> Skipped { get; } = new List<string>();
    }
}
