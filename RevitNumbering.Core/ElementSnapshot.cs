namespace RevitNumbering.Core
{
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
