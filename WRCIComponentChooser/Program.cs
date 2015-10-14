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
            GA ga = new GA(3, 500, Fitness);
            ga.Train(100);
            Console.WriteLine(ga.BestIndividual.Fitness);
        }


        static double Fitness(double[] values, int pid, bool delete)
        {
            if (values.Length != 3)
                throw new ArgumentException("Should have six component values");

            string[] names = new string[] { "r1v", "r2v", "cv" };

            using (var sw = new StreamWriter("multivibrator." + pid + ".net"))
            {

                using (var sr = new StreamReader("multivibrator.net"))
                {
                    while (!sr.EndOfStream)
                    {
                        string l = sr.ReadLine();
                        for (int i = 0; i < 3; i++)
                        {
                            if (l.Contains(names[i]))
                            {
                                double v = values[i];
                                if (names[i].Contains('c'))
                                    v /= 1000000;
                                l = l.Replace(names[i], v.ToString());
                            }
                        }
                        sw.WriteLine(l);
                    }
                }
            }

            using (Process p = System.Diagnostics.Process.Start(new ProcessStartInfo("scad3.exe", "-ascii -b multivibrator." + pid + ".net")))
            {
                p.WaitForExit();
            }

            List<Vector2> simValues = new List<Vector2>();

            using (var sr = new StreamReader("multivibrator." + pid + ".raw"))
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

            const decimal desiredPeriod = 5M;
            const decimal desiredDutyCycle = 0.5M;

            double error = 0;
            using (StreamWriter w = new StreamWriter("wave." + pid + ".csv"))
            {
                foreach (Vector2 p in simValues)
                {
                    double expected = 5 * (((decimal)p.T % desiredPeriod < desiredDutyCycle * desiredPeriod) ? 1 : 0);
                    w.WriteLine("{0}\t{1}\t{2}", p.T, expected, p.V);
                    error += Math.Abs(expected - p.V);
                }
            }

            double fitness = -error / simValues.Count;

            Console.WriteLine("{0}\tFitness: {1:N4}", pid, fitness);
            if (delete)
            {
                File.Delete("multivibrator." + pid + ".net");
                File.Delete("multivibrator." + pid + ".log");
                File.Delete("multivibrator." + pid + ".raw");
                File.Delete("multivibrator." + pid + ".op.raw");
                File.Delete("wave." + pid + ".csv");
            }
            return fitness;
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

        public override string ToString()
        {
            return "{" + T + " " + V + "}";
        }
    }
}
