using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WRCIComponentChooser
{
    class Species
    {
        public double[] Genome;
        public virtual double Fitness { get; set; }

        public Species(int genomeLength)
        {
            Genome = new double[genomeLength];
            for (int i = 0; i < Genome.Length; Genome[i++] = R.NextDouble() * 10000) ;
        }

        public Species(Species s)
        {
            Genome = new double[s.Genome.Length];
            for (int i = 0; i < s.Genome.Length; i++)
                Genome[i] = s.Genome[i];
        }

        public static Species operator ^(Species a, Species b)
        {
            if (a.Genome.Length != b.Genome.Length)
                throw new ArgumentException("Can't crossover species of different genome length");

            Species result = new Species(a);
            int cut = R.Next(a.Genome.Length + 1);
            for (int i = cut; i < a.Genome.Length; i++)
                result.Genome[i] = b.Genome[i];
            return result;
        }

        public static Species operator ~(Species a)
        {
            Species result = new Species(a);
            const double scale = 10;
            for (int i = 0; i < a.Genome.Length; i++)
                if (R.NextDouble() < 0.4)
                    a.Genome[i] = Math.Abs(a.Genome[i] + R.NextDouble() * 2 * scale - scale);
            return result;
        }

        //Vector operations:
        public static Species operator +(Species a, Species b)
        {
            if (a.Genome.Length != b.Genome.Length)
                throw new ArgumentException("Can't vector add species of different genome length");

            Species result = new Species(a);
            for (int i = 0; i < a.Genome.Length; i++)
                result.Genome[i] += b.Genome[i];
            return result;
        }

        public static Species operator *(double c, Species a)
        {
            Species result = new Species(a);
            for (int i = 0; i < a.Genome.Length; i++)
                result.Genome[i] *= c;
            return result;
        }

        public static Species operator -(Species a)
        {
            Species result = new Species(a);
            for (int i = 0; i < a.Genome.Length; i++)
                result.Genome[i] = -a.Genome[i];
            return result;
        }

        public static Species operator -(Species a, Species b)
        {
            return a + (-b);
        }

        public void Clamp()
        {
            double[] ClampMax = new double[6] { 1e6, 1e6, 1e6, 1e6, 1000, 1000 };
            double[] ClampMin = new double[6] { 1, 1, 1, 1, 1e-6, 1e-6 };
            for (int i = 0; i < Genome.Length; i++)
            {
                if (Genome[i] > ClampMax[i])
                    Genome[i] = ClampMax[i];
                if (Genome[i] < ClampMin[i])
                    Genome[i] = ClampMin[i];
            }
        }
    }

    class GA
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

        public GA(int genomeLength, int populationSize, Func<double[], int, bool, double> fitnessFunction)
        {
            Population = new List<Species>(populationSize);
            FitnessFunction = fitnessFunction;
            PopulationSize = populationSize;

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
            while (iterations-- > 0)
            {
                List<Species> parents = Population;
                Population = new List<Species>(PopulationSize);
                Population.Add(parents.First());

                //Tournament Selection
                while (Population.Count < PopulationSize)
                {
                    Species[] pair = new Species[2];

                    for (int i = 0; i < 2; i++)
                    {
                        int[] randomIndices = new int[10];

                        for (int j = 0; j < randomIndices.Length; j++)
                            randomIndices[j] = int.MaxValue;

                        for (int j = 0; j < randomIndices.Length; j++)
                        {
                            int r = R.NextNot(parents.Count, randomIndices);
                            randomIndices[j] = r;
                        }

                        List<Species> tournament = new List<Species>(randomIndices.Length);
                        for (int j = 0; j < randomIndices.Length; j++)
                            tournament.Add(parents[randomIndices[j]]);
                        pair[i] = tournament.OrderByDescending(s => s.Fitness).First();
                    }
                    //now have two parents.
                    Species sp = ~(pair[0] ^ pair[1]);
                    sp.Clamp();
                    Population.Add(sp);
                }
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

                Console.Clear();
                Console.WriteLine(BestIndividual.Fitness);
                FitnessFunction(BestIndividual.Genome, -iterations, false);
            }
        }
    }
}
