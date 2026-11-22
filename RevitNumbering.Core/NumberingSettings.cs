using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RevitNumbering.Core
{
    public enum NumberingScope
    {
        All,
        Selection
    }

    public sealed class NumberingSettings
    {
        public int Version { get; set; } = 1;

        public double RowToleranceMm { get; set; } = 1000;

        [JsonConverter(typeof(StringEnumConverter))]
        public NumberingScope Scope { get; set; } = NumberingScope.All;

        public string LastCategory { get; set; } = "";

        public Dictionary<string, string> Templates { get; set; } = new Dictionary<string, string>();
    }
}
