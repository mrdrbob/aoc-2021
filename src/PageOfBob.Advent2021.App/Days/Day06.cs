using System.Linq;

namespace PageOfBob.Advent2021.App.Days
{
    public static class Day06
    {

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
    }
}
