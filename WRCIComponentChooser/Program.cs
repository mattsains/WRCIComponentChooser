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
            GA ga = new GA(6, 500, Fitness);
            ga.Train(100);
            Console.WriteLine(ga.BestIndividual.Fitness);
        }


        static double Fitness(double[] values, int pid, bool delete)
        {
            if (values.Length != 6)
                throw new ArgumentException("Should have six component values");

            string[] names = new string[] { "r1v", "r2v", "r3v", "r4v", "c1v", "c2v" };

            using (var sw = new StreamWriter("multivibrator." + pid + ".net"))
            {

                using (var sr = new StreamReader("multivibrator.net"))
                {
                    while (!sr.EndOfStream)
                    {
                        string l = sr.ReadLine();
                        for (int i = 0; i < 6; i++)
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
                using (var sw = new StreamWriter("wave." + pid + ".csv"))
                    while (!sr.EndOfStream)
                    {
                        string s = sr.ReadLine();
                        double time = double.Parse(s.Substring(s.IndexOf('\t') + 1));

                        double v = double.Parse(sr.ReadLine());

                        simValues.Add(new Vector2(time, v));
                        sw.WriteLine("{0};{1}", time, v);
                    }
            }
            const double desiredFrequency = 1;
            const double desiredDutyCycle = 0.5;
            const double desiredPeak = 5;

            //calculate duty cycle and frequency
            int zeroCrossings = 0;
            double timeUnderZero = 0;
            double timeOverZero = 0;
            double average = simValues.Average(v => v.V);

            for (int i = 0; i + 1 < simValues.Count; i++)
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



            double dutyCycle = timeOverZero / (timeOverZero + timeUnderZero);
            double frequency = (zeroCrossings / (simValues.Last().T - simValues.First().T)) / 2;



            double fitness = Math.Abs(desiredPeak - average / dutyCycle) / desiredPeak + Math.Abs(desiredFrequency - frequency) / desiredFrequency + Math.Abs(desiredDutyCycle - dutyCycle) / desiredDutyCycle; //Math.Abs(desiredFrequency - frequency) / desiredFrequency;// +Math.Abs(desiredDutyCycle - dutyCycle); //Math.Sqrt(Math.Pow(desiredFrequency - frequency, 2) + Math.Pow(desiredDutyCycle - dutyCycle, 2));

            if (!delete)
                Console.WriteLine("{0}\tFrequency: {1:N3}   Duty Cycle: {2:N4}   Peak: {3:N4}   Fitness: {4:N4}", pid, frequency, dutyCycle, average / dutyCycle, fitness);

            if (delete)
            {
                File.Delete("multivibrator." + pid + ".net");
                File.Delete("multivibrator." + pid + ".log");
                File.Delete("multivibrator." + pid + ".raw");
                File.Delete("multivibrator." + pid + ".op.raw");
                File.Delete("wave." + pid + ".csv");
            }
            return -fitness;
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
