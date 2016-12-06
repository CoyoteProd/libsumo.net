using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSumo.Net.Math
{
    class MovingAverage
    {
        private Queue<long> window = new Queue<long>();
        private long windowSize;
        private long sum = 0;

        public MovingAverage(long windowSize)
        {
            this.windowSize = windowSize;
        }

        public void add(long value)
        {
            sum = sum + value;
            window.Enqueue(value);
            if (window.Count() > windowSize)
            {
                sum = sum - window.Dequeue();
            }
        }

        public long getAverage()
        {
            if (window.Count==0) return 0;
            long divisor = window.Count();
            return sum/divisor;
        }

        private static readonly DateTime Jan1st1970 = new DateTime (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long CurrentTimeMillis()
        {
            return (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
        }
    }
}
