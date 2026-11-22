using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitNumbering.Core;

namespace RevitNumbering.Revit.Model
{
    internal static class RevitElementLoader
    {
        public static CategoryLoadResult Load(
            UIDocument uiDocument,
            CategoryDefinition category,
            NumberingScope scope)
        {
            var document = uiDocument.Document;
            var all = new List<ElementSnapshot>();
            var selected = new List<ElementSnapshot>();
            var selectedIds = new HashSet<ElementId>(uiDocument.Selection.GetElementIds());

            var elements = new FilteredElementCollector(document)
                .OfCategory(category.Category)
                .WhereElementIsNotElementType();

            foreach (var element in elements)
            {
                var snapshot = ToSnapshot(document, element, category);
                all.Add(snapshot);

                if (selectedIds.Contains(element.Id))
                {
                    selected.Add(snapshot);
                }
            }

            return new CategoryLoadResult(
                all,
                scope == NumberingScope.Selection ? selected : all);
        }

        private static ElementSnapshot ToSnapshot(
            Document document,
            Element element,
            CategoryDefinition category)
        {
            var snapshot = new ElementSnapshot
            {
                ElementId = GetElementId(element),
                Name = element.Name ?? "",
                CurrentValue = ReadValue(element, category.Parameter)
            };

            var level = document.GetElement(element.LevelId) as Level;
            if (level != null)
            {
                snapshot.LevelName = level.Name;
                snapshot.LevelElevation = level.Elevation;
            }

            SetLocation(element, snapshot);
            return snapshot;
        }

        private static string ReadValue(Element element, BuiltInParameter parameter)
        {
            var value = element.get_Parameter(parameter);
            if (value == null)
            {
                return "";
            }

            return value.AsString() ?? value.AsValueString() ?? "";
        }

        private static void SetLocation(Element element, ElementSnapshot snapshot)
        {
            try
            {
                if (element.Location is LocationPoint point)
                {
                    SetPoint(snapshot, point.Point);
                    return;
                }

                if (element.Location is LocationCurve curve)
                {
                    SetPoint(snapshot, curve.Curve.Evaluate(0.5, true));
                    return;
                }
            }
            catch
            {
            }

            try
            {
                var box = element.get_BoundingBox(null);
                if (box != null)
                {
                    SetPoint(snapshot, (box.Min + box.Max) * 0.5);
                }
            }
            catch
            {
            }
        }

        private static void SetPoint(ElementSnapshot snapshot, XYZ point)
        {
            if (double.IsNaN(point.X)
                || double.IsNaN(point.Y)
                || double.IsInfinity(point.X)
                || double.IsInfinity(point.Y))
            {
                return;
            }

            snapshot.HasLocation = true;
            snapshot.X = point.X;
            snapshot.Y = point.Y;
        }

        private static int GetElementId(Element element)
        {
#if REVIT2024
            return (int) element.Id.Value;
#else
            return element.Id.IntegerValue;
#endif
        }
    }
}
