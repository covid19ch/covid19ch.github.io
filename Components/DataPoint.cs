using System;

namespace CovidStatsCH.Components
{
    public class DataPoint
    {
        public DateTime Date { get; set; }
        public Input Cases { get; set; }
        public Input Hospitalisations { get; set; }
        public Input Deaths { get; set; }
    }
}
