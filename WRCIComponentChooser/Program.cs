using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace WRCIComponentChooser
{
    class Program
    {
        static void Main(string[] args)
        {
            Fitness(new double[] { 1, 2, 3, 4, 5, 6 });
        }

        static double Fitness(double[] values)
        {
            if (values.Length != 6)
                throw new ArgumentException("Should have six component values");

            string[] names = new string[] { "r1v", "r2v", "r3v", "r4v", "c1v", "c2v" };

            using (var sw = new StreamWriter("params.lib"))
            {
                for (int i = 0; i < 6; i++)
                    sw.WriteLine(".param {0}={1}", names[i], values[i]);
            }

            Process p = System.Diagnostics.Process.Start(new ProcessStartInfo("scad3.exe", "-ascii -b multivibrator.net"));

            p.WaitForExit();

            List<Vector2> simValues = new List<Vector2>();

            using (var sr = new StreamReader("multivibrator.raw"))
            {
                for (int i = 0; i < 12; i++)
                    sr.ReadLine();

                while (!sr.EndOfStream)
                {
                    string s = sr.ReadLine();
                    double time = double.Parse(s.Substring(s.IndexOf('\t') + 1));

                    double v = double.Parse(sr.ReadLine());

                    simValues.Add(new Vector2(time, v));
                }
            }

            //calculate duty cycle and frequency
            int zeroCrossings = 0;
            double timeUnderZero = 0;
            double timeOverZero = 0;
            double average = simValues.Average(v => v.V);

            for (int i = 0; i + 1 < simValues.Count; i++)
            {
                if (simValues[i].V <= average && simValues[i + 1].V >= average)
                {
                    //TODO: interpolate a crossing point and add to time under/over zero
                    zeroCrossings++;
                }
                else if (simValues[i].V >= average && simValues[i + 1].V <= average)
                {
                    //TODO: interpolate a crossing point and add to time under/over zero
                    zeroCrossings++;
                }
                else if (simValues[i].V > average && simValues[i].V > average)
                {
                    timeOverZero += (simValues[i + 1].T - simValues[i].T);
                }
                else if (simValues[i].V < average && simValues[i].V < average)
                {
                    timeUnderZero += (simValues[i + 1].T - simValues[i].T);
                }
                else
                {
                    throw new Exception("My logic must be wrong");
                }

            }
            //TODO: Calculate frequency, duty cycle
            //TODO: Calculate fitness
        }
    }

    struct Vector2
    {
        public double T;
        public double V;

        public Vector2(double t, double v)
        {
            T = t;
            V = v;
        }
    }
}
