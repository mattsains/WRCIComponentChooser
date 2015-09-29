﻿using System;
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
            return r.NextDouble();
        }

        public static int Next(int maxValue = int.MaxValue)
        {
            return r.Next(maxValue);
        }

        public static int Next(int minValue, int maxValue)
        {
            return r.Next(minValue, maxValue);
        }
    }
}
