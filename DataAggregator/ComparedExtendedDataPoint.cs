using System;
using System.Text.Json.Serialization;

namespace CovidStatsCH.Components
{
    public class ComparedExtendedDataPoint
    {
        public DateTime Date { get; set; }
        public ComparedExtendedInput Cases { get; set; }
        public ComparedExtendedInput Hospitalisations { get; set; }
        public ComparedExtendedInput Deaths { get; set; }
        [JsonConstructor]
        public ComparedExtendedDataPoint()
        {
        }
        public ComparedExtendedDataPoint(ExtendedDataPoint current)
        {
            Date = current.Date;
            Cases = new ComparedExtendedInput(current.Cases);
            Hospitalisations = new ComparedExtendedInput(current.Hospitalisations);
            Deaths = new ComparedExtendedInput(current.Deaths);
        }
        public ComparedExtendedDataPoint(ExtendedDataPoint current, ExtendedDataPoint? reference)
        {
            Date = current.Date;
            Cases = new ComparedExtendedInput(current.Cases, reference?.Cases);
            Hospitalisations = new ComparedExtendedInput(current.Hospitalisations, reference?.Hospitalisations);
            Deaths = new ComparedExtendedInput(current.Deaths, reference?.Deaths);
        }
    }
}
