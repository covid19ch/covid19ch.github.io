using System;
using System.Collections.Generic;
using System.Linq;

namespace CovidStatsCH.Components
{
    public class Input
    {
        public int New { get; set; }
        public int Cumulative { get; set; }
        public static implicit operator Input((int New, int Cumulative) values)
        {
            return new Input
            {
                New = values.New,
                Cumulative = values.Cumulative
            };
        }
    }
    public class ExtendedInput : Input
    {
        public double? SevenDayAverage { get; set; }
        public double? SevenDayAverageAWeekEarlier { get; set; }
        public int Unreliable { get; set; }
        public int SemiUnreliable { get; set; }
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
    public static class DataPointExtension
    {
        public static IEnumerable<ExtendedDataPoint> SevenDayAverages(this IEnumerable<DataPoint> points)
        {
            var queue = new Queue<DataPoint>(7);
            var extended_queue = new Queue<ExtendedDataPoint>(14);
            foreach (var point in points)
            {
                queue.Enqueue(point);
                var seven_days_ago = extended_queue.Count >= 7 ? extended_queue.Dequeue() : null;
                var extended_item = new ExtendedDataPoint(queue, seven_days_ago);
                extended_queue.Enqueue(extended_item);
                yield return extended_item;
                if (queue.Count >= 7)
                {
                    queue.Dequeue();
                }
            }
        }
        public static IEnumerable<ExtendedDataPoint> PrependUpTo(this IEnumerable<ExtendedDataPoint> points, DateTime date)
        {
            var enumerator = points.GetEnumerator();
            enumerator.MoveNext();
            while (enumerator.Current.Date < date.Date)
            {
                date = date.AddDays(-1);
                yield return new ExtendedDataPoint(date.Date, enumerator.Current);
            }
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }
        }
    }
    public class ExtendedDataPoint
    {
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
    public class DataPoint
    {
        public DateTime Date { get; set; }
        public Input Cases { get; set; }
        public Input Hospitalisations { get; set; }
        public Input Deaths { get; set; }
    }
    public partial class DataPointProvider
    {
        public int Population = 8_570_000;
    }
}
