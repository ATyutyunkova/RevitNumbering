using System.Collections.Generic;
using Autodesk.Revit.DB;

namespace RevitNumbering.Revit.Model
{
    /// <summary>
    /// Описание нумеруемой категории: что читать и в какой параметр писать.
    /// Новая категория добавляется одной записью в <see cref="All"/>.
    /// </summary>
    public sealed class CategoryDefinition
    {
        public CategoryDefinition(BuiltInCategory category, string displayName, BuiltInParameter parameter)
        {
            Category = category;
            DisplayName = displayName;
            Parameter = parameter;
        }

        public BuiltInCategory Category { get; }

        public string DisplayName { get; }

        public BuiltInParameter Parameter { get; }

        public static IReadOnlyList<CategoryDefinition> All { get; } = new[]
        {
            new CategoryDefinition(BuiltInCategory.OST_Walls, "Стены", BuiltInParameter.ALL_MODEL_MARK),
            new CategoryDefinition(BuiltInCategory.OST_Doors, "Двери", BuiltInParameter.ALL_MODEL_MARK),
            new CategoryDefinition(BuiltInCategory.OST_Windows, "Окна", BuiltInParameter.ALL_MODEL_MARK),
            new CategoryDefinition(BuiltInCategory.OST_Rooms, "Помещения", BuiltInParameter.ROOM_NUMBER)
        };
    }
}
