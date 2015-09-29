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
        public double Fitness;

        public Species(int genomeLength)
        {
            Genome = new double[genomeLength];
            for (int i = 0; i < Genome.Length; Genome[i++] = R.NextDouble()) ;
        }

        public Species(Species s)
        {
            Genome = new double[s.Genome.Length];
            for (int i = 0; i < s.Genome.Length; Genome[i++] = s.Genome[i]) ;
        }

        public static Species operator +(Species a, Species b)
        {
            if (a.Genome.Length != b.Genome.Length)
                throw new ArgumentException("Can't crossover species of different genome length");

            Species result = new Species(a);
            int cut = R.Next(a.Genome.Length + 1);
            for (int i = cut; i < a.Genome.Length; i++)
                a.Genome[i] = b.Genome[i];
            return result;
        }

        public static Species operator ~(Species a)
        {
            Species result = new Species(a);
            const double scale = 0.1;
            for (int i = 0; i < a.Genome.Length; i++)
                a.Genome[i] += R.NextDouble() * 2 * scale - scale;
            return result;
        }
    }

    class GA
    {
        public int GenomeLength { get; private set; }
        public int PopulationSize { get; private set; }

        //Keep this sorted by fitness descending
        public List<Species> Population { get; private set; }

        protected Func<double[], double> FitnessFunction;

        public Species BestIndividual
        {
            get { return Population.First(); }
        }

        public GA(int genomeLength, int populationSize, Func<double[], double> fitnessFunction)
        {
            Population = new List<Species>(populationSize);
            FitnessFunction = fitnessFunction;

            for (int i = 0; i < populationSize; i++)
                Population.Add(new Species(genomeLength));

            foreach (Species s in Population)
                s.Fitness = FitnessFunction(s.Genome);
            Population.Sort((s, t) => t.Fitness.CompareTo(s.Fitness));
        }

        public void Train(int iterations = 1)
        {
            while (iterations-- > 0)
            {
                List<Species> parents = new List<Species>();

                parents.AddRange(Population.OrderByDescending(s => s.Fitness).Take((int)Math.Sqrt(PopulationSize)).Select(s => ~s));

                Population.Clear();
                for (int i = 0; i < parents.Count; i++)
                    for (int j = i + 1; j < parents.Count; j++)
                        Population.Add(parents[i] + parents[j]);

                foreach (Species s in Population)
                    s.Fitness = FitnessFunction(s.Genome);
                Population.Sort((s, t) => t.Fitness.CompareTo(s.Fitness));
            }
        }
    }
}
