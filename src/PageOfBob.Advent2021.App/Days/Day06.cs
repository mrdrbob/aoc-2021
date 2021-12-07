using System.Linq;

namespace PageOfBob.Advent2021.App.Days
{
    public static class Day06
    {


        public static void Execute()
        {
            var numbers = Utilities.GetEmbeddedData("06").Split(',', StringSplitOptions.RemoveEmptyEntries).AsNumbers();
            var population = new FishPopulations(8, 7);
            foreach (var number in numbers)
                population.AddFishAtAge(number);

            for (var x = 0; x < 256; x++)
                population.Iterate();

            var total = population.TotalPopulation();
            Console.WriteLine(total);
        }


        public class FishPopulations
        {
            private readonly int maxAge;
            private readonly int reproductionAge;
            private List<ulong> populations;

            public FishPopulations(int maxAge, int reproductionAge)
            {
                this.maxAge = maxAge;
                this.reproductionAge = reproductionAge;
                populations = Enumerable.Repeat(0ul, maxAge + 1).ToList();
            }

            public void AddFishAtAge(int age)
            {
                populations[age] += 1;
            }

            public void Iterate()
            {
                ulong zero = populations[0];
                for (int age = 1; age <= maxAge; age++)
                {
                    populations[age - 1] = populations[age];
                    if (age == reproductionAge)
                        populations[age - 1] += zero;
                }
                populations[maxAge] = zero;
            }

            public ulong TotalPopulation() => populations.Aggregate(0ul, (acc, v) => acc + v);
        }

        /* Part 1
        public static void Execute()
        {
            var numbers = Utilities.GetEmbeddedData("06").Split(',', StringSplitOptions.RemoveEmptyEntries).AsNumbers();
            var tank = numbers.Select(x => new Fish(x)).ToList();

            for (int day = 0; day < 80; day++)
            {
                Console.WriteLine("{0} - {1}", day, tank.Count);

                for (int x = tank.Count - 1; x >= 0; x--)
                {
                    var (me, child) = tank[x].Next();
                    tank[x] = me;
                    if (child.HasValue)
                    {
                        tank.Add(child.Value);
                    }
                }
            }

            Console.WriteLine(tank.Count);
        }

        public record struct Fish(int Counter)
        {
            public (Fish, Fish?) Next()
            {
                if (Counter == 0)
                {
                    return (new Fish(6), new Fish(8));
                }
                else
                {
                    return (new Fish(Counter - 1), null);
                }
            }
        }
        */
    }
}
