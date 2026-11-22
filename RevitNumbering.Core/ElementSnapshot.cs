namespace RevitNumbering.Core
{
    /// <summary>
    /// Снимок элемента для расчётов без обращений к Revit API.
    /// Координаты — во внутренних единицах источника; в адаптере Revit это футы.
    /// </summary>
    public sealed class ElementSnapshot
    {
        public int ElementId { get; set; }

        public string Name { get; set; } = "";

        public string LevelName { get; set; } = "";

        public double LevelElevation { get; set; }

        public string CurrentValue { get; set; } = "";

        public bool HasLocation { get; set; }

        public double X { get; set; }

        public double Y { get; set; }
    }
}
