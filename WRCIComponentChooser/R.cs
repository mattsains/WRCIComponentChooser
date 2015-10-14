using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WRCIComponentChooser
{
    static class R
    {
        static Random r = new Random();

        public static double NextDouble()
        {
            lock (r)
                return r.NextDouble();
        }

        public static int Next(int maxValue = int.MaxValue)
        {
            lock (r)
                return r.Next(maxValue);
        }

        public static int Next(int minValue, int maxValue)
        {
            lock (r)
                return r.Next(minValue, maxValue);
        }

        public static int NextNot(int maxValue, params int[] notThese)
        {
            int i;
            do
            {
                lock (r)
                    i = r.Next(maxValue);
            } while (notThese.Contains(i));
            return i;
        }
    }
}
