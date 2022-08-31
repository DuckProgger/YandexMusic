namespace Yandex.Music.Core;

public class TimeshiftMetricTopic
{
    private readonly LinkedList<TimeshiftMetric> list;

    public int Capacity { get; set; }

    public TimeshiftMetricTopic(int capacity) {
        Capacity = capacity;
        list = new LinkedList<TimeshiftMetric>();
    }

    public void AddMetric(DateTime dateTime, long value) {
        TimeshiftMetric currentMetric = new() {
            DateTime = dateTime,
            Value = value,
        };
        lock (list) {
            if (list.Count == Capacity) {
                list.RemoveFirst();
            }
            list.AddLast(currentMetric);
        }
    }

    public void AddAccumulativeMetric(DateTime dateTime, long value) {
        TimeshiftMetric currentMetric = new() {
            DateTime = dateTime,
            Value = value,
        };
        TimeshiftMetric prevMetric;
        lock (list) {
            if (list.Count == Capacity) {
                list.RemoveFirst();
            }
            prevMetric = list.LastOrDefault();
            list.AddLast(currentMetric);
        }
        if (prevMetric != null) {
            currentMetric.Prev = prevMetric;
        }
    }

    public double GetAverage(int milliseconds) {
        TimeshiftMetric first = null;
        TimeshiftMetric last = null;
        double average;
        int count;
        lock (list) {
            first = list.First();
            last = list.Last();
            average = list.Average(x => x.Value - x.Prev.Value);
            count = list.Count();
        }
        double totalMilliseconds = (last.DateTime.Ticks - first.Prev.DateTime.Ticks) / 10000.0;
        double result = average / (totalMilliseconds / milliseconds / count);
        return result;
    }


    private class TimeshiftMetric
    {
        public DateTime DateTime { get; set; }

        public long Value { get; set; }

        public TimeshiftMetric Prev {
            get => prev ?? this;
            set => prev = value;
        }


        private TimeshiftMetric prev;
    }
}
