using System.Collections.Generic;
using System.Linq;

namespace CovidStatsCH.Components
{
    public partial class DataPointProvider
    {
        public static readonly int Population = 8_570_000;
        public static List<ExtendedDataPoint> Previous(List<ExtendedDataPoint> current)
        {
            var match = ExtendedAll.FirstOrDefault(kv => kv.Value == current);
            if (match.Key == default)
            {
                return default;
            }
            var current_date = match.Key;
            var previous_key = ExtendedAll.Keys.OrderBy(k => k).TakeWhile(k => k < current_date).LastOrDefault();
            if (previous_key == default)
            {
                return default;
            }
            return ExtendedAll[previous_key];
        }
    }
}
