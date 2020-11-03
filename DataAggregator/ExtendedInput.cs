using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace CovidStatsCH.Components
{
    public class ExtendedInput : Input
    {
        public double? SevenDayAverage { get; set; }
        public double? SevenDayAverageAWeekEarlier { get; set; }
        public int Unreliable { get; set; }
        public int SemiUnreliable { get; set; }
        [JsonConstructor]
        public ExtendedInput()
        {
        }
        public ExtendedInput(List<Input> range, ExtendedInput? seven_days_ago)
        {
            New = range.Last().New;
            Cumulative = range.Last().Cumulative;
            SevenDayAverage = range.Count < 7 ? null : range.Average(e => e.New);
            SevenDayAverageAWeekEarlier = seven_days_ago?.SevenDayAverage;
        }
    }
}
