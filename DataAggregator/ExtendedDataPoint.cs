using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace CovidStatsCH.Components
{
    public class ExtendedDataPoint
    {
        [JsonConstructor]
        public ExtendedDataPoint()
        {
        }
        public ExtendedDataPoint(DateTime date, ExtendedDataPoint current)
        {
            Date = date;
            Cases = new ExtendedInput() { Cumulative = current.Cases.Cumulative };
            Hospitalisations = new ExtendedInput() { Cumulative = current.Hospitalisations.Cumulative };
            Deaths = new ExtendedInput() { Cumulative = current.Deaths.Cumulative };
        }
        public ExtendedDataPoint(Queue<DataPoint> queue, ExtendedDataPoint? seven_days_ago)
        {
            Date = queue.Last().Date;
            Cases = new ExtendedInput(queue.Select(e => e.Cases).ToList(), seven_days_ago?.Cases) { Unreliable = 3, SemiUnreliable = 5 };
            Hospitalisations = new ExtendedInput(queue.Select(e => e.Hospitalisations).ToList(), seven_days_ago?.Hospitalisations) { Unreliable = 3, SemiUnreliable = 5 };
            Deaths = new ExtendedInput(queue.Select(e => e.Deaths).ToList(), seven_days_ago?.Deaths) { Unreliable = 7, SemiUnreliable = 14 };
        }

        public DateTime Date { get; set; }
        public ExtendedInput Cases { get; set; }
        public ExtendedInput Hospitalisations { get; set; }
        public ExtendedInput Deaths { get; set; }
    }
}
