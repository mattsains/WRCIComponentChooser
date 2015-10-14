using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WRCIComponentChooser
{
    class DE
    {
        public int GenomeLength { get; private set; }
        public int PopulationSize { get; private set; }

        //Keep this sorted by fitness descending
        public List<Species> Population { get; private set; }

        protected Func<double[], int, bool, double> FitnessFunction;

        public Species BestIndividual
        {
            get { return Population.First(); }
        }

        int pid = 0;
        public DE(int genomeLength, int populationSize, Func<double[], int, bool, double> fitnessFunction)
        {
            GenomeLength = genomeLength;
            PopulationSize = populationSize;
            FitnessFunction = fitnessFunction;

            Population = new List<Species>(PopulationSize);

            for (int i = 0; i < populationSize; i++)
                Population.Add(new Species(genomeLength));

            List<Task> tasks = new List<Task>();

            foreach (Species s in Population)
            {
                int pidd = pid++;
                Task t = new Task(() =>
                {
                    s.Fitness = FitnessFunction(s.Genome, pidd, true);
                });
                tasks.Add(t);
                t.Start();
            }
            tasks.ForEach(t => t.Wait());
            Population.Sort((s, t) => t.Fitness.CompareTo(s.Fitness));
        }

        public void Train(int iterations = 1)
        {
            const double beta = 0.5;
            const double crossoverRate = 0.5;
            while (iterations-- > 0)
            {
                List<Species> newPop = new List<Species>(PopulationSize);
                List<Task> tasks = new List<Task>();

                for (int i = 0; i < PopulationSize; i++)
                {
                    int ppid = pid++;
                    int actuali = i;
                    Task t = new Task(() =>
                    {
                        Species xnew;
                        Species x;
                        lock (Population)
                        {
                            x = Population[actuali];

                            int i1 = R.NextNot(PopulationSize, actuali);
                            int i2 = R.NextNot(PopulationSize, actuali, i1);
                            int i3 = R.NextNot(PopulationSize, actuali, i1, i2);
                            Species x1 = Population[i1];
                            Species x2 = Population[i2];
                            Species x3 = Population[i3];

                            Species u = x1 + beta * (x2 - x3);
                            u.Clamp();

                            xnew = new Species(x);
                            for (int j = 0; j < GenomeLength; j++)
                                if (R.NextDouble() < crossoverRate)
                                    xnew.Genome[j] = u.Genome[j];
                        }
                        xnew.Fitness = FitnessFunction(xnew.Genome, ppid, true);
                        lock (newPop)
                        {
                            if (x.Fitness < xnew.Fitness)
                                newPop.Add(xnew);
                            else newPop.Add(x);
                        }
                    });
                    tasks.Add(t);
                    t.Start();
                }
                tasks.ForEach(t => t.Wait());
                Population = newPop;

                Population.Sort((x, y) => y.Fitness.CompareTo(x.Fitness));

                Console.Clear();
                Console.WriteLine(BestIndividual.Fitness);
                FitnessFunction(BestIndividual.Genome, -iterations, false);
            }

        }
    }
}
