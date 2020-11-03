using System;
using System.Collections.Generic;
using System.Linq;

namespace CovidStatsCH.Components
{
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
        public static List<ComparedExtendedDataPoint> Compare(this List<ExtendedDataPoint> current, Dictionary<DateTime, List<ExtendedDataPoint>> all)
        {
            var previous = DataPointProvider.Previous(current, all);
            if (previous == default)
            {
                return current.Select(c => new ComparedExtendedDataPoint(c)).ToList();
            }
            return current.Select(c => new ComparedExtendedDataPoint(c, previous.FirstOrDefault(p => p.Date.ToShortDateString() == c.Date.ToShortDateString()))).ToList();
        }
    }
}
