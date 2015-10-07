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
            for (int i = 0; i < Genome.Length; Genome[i++] = R.NextDouble() * 100000) ;
        }

        public Species(Species s)
        {
            Genome = new double[s.Genome.Length];
            for (int i = 0; i < s.Genome.Length; i++)
                Genome[i] = s.Genome[i];
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
            const double scale = 1;
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
            PopulationSize = populationSize;

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

                        for (int j = 0; j < randomIndices.Length; )
                        {
                            int r = R.Next(parents.Count);
                            if (randomIndices.Contains(r)) { continue; }
                            randomIndices[j++] = r;
                        }

                        List<Species> tournament = new List<Species>(randomIndices.Length);
                        for (int j = 0; j < randomIndices.Length; j++)
                            tournament.Add(parents[randomIndices[j]]);
                        pair[i] = tournament.OrderByDescending(s => s.Fitness).First();
                    }
                    //now have two parents.
                    Population.Add(~(pair[0] + pair[1]));
                }

                foreach (Species s in Population)
                    s.Fitness = FitnessFunction(s.Genome);
                Population.Sort((s, t) => t.Fitness.CompareTo(s.Fitness));

                Console.Clear();
                Console.WriteLine(BestIndividual.Fitness);
                foreach (double w in BestIndividual.Genome)
                    Console.WriteLine(w);
            }
        }
    }
}
