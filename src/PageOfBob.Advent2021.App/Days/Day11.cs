using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PageOfBob.Advent2021.App.Days
{
    public static class Day11
    {
        public static void Execute()
        {
            var initial = Utilities.GetEmbeddedData("11").Replace("\n", "").Replace("\r", "").Select(x => int.Parse(x.ToString())).Cast<int?>().ToList();
            var map = new Map<int?>(10, 10, initial);

            /* Part 1
            var total = Enumerable.Range(0, 100).Select(_ => map.ProcessEnergy()).Sum();
            Console.WriteLine(total);
            */

            // Part 2
            int step = 0;
            while (true)
            {
                step++;
                int totalFlashes = map.ProcessEnergy();
                if (totalFlashes >= 100)
                    break;
            }

            Console.WriteLine(step);
        }

        public record Map<T>(int Width, int Height, List<T> Data)
        {
            public int DataPosition(Position pos) => pos.Y * Width + pos.X;
            public T Get(Position pos) => Data[DataPosition(pos)];
            public void Set(Position pos, T value) => Data[DataPosition(pos)] = value;
            public void Modify(Position pos, Func<T, T> modify) => Set(pos, modify(Get(pos)));
        }

        public static IEnumerable<Position> GetAllPositions<T>(this Map<T> map)
        {
            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    yield return new Position(x, y);
                }
            }
        }

        public static void Print(this Map<int?> map)
        {
            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    Console.Write(map.Get(new Position(x, y)));
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        public static void Mutate<T>(this Map<T> map, Func<Position, T, T> modify)
        {
            foreach (var pos in map.GetAllPositions())
            {
                map.Set(pos, modify(pos, map.Get(pos)));
            }
        }

        public static IEnumerable<Position> GetFlashes(this Map<int?> map)
            => map.GetAllPositions().Where(pos => map.Get(pos) > 9);
        public static IEnumerable<Position> GetSurroundingPoints(this Position position, int width, int height)
            => Utilities.RangeFromTo(position.X - 1, position.X + 1)
                .SelectMany(x => Utilities.RangeFromTo(position.Y - 1, position.Y + 1).Select(y => new Position(x, y)))
                .Where(pos => pos.X >= 0 && pos.X < width
                           && pos.Y >= 0 && pos.Y < height
                           && !(pos.X == position.X && pos.Y == position.Y));

        public static int ProcessEnergy(this Map<int?> map)
        {
            // Add one to everything.
            map.Mutate((_, v) => v.HasValue ? v.Value + 1 : v);
            int totalFlashes = 0;

            // Process flashes
            var flahes = map.GetFlashes().ToList();
            while (flahes.Any()) {
                totalFlashes += flahes.Count;

                // Null out any nines (can only be flashes)
                map.Mutate((pos, v) => flahes.Contains(pos) ? null : v);

                // Get all points to add to.
                var points = flahes.SelectMany(pos => pos.GetSurroundingPoints(map.Width, map.Height));

                // Add 1 to any point that should have it.
                foreach (var point in points)
                {
                    map.Modify(point, (t) => t.HasValue ? t.Value + 1 : t);
                }

                // Get a fresh list of nines
                flahes = map.GetFlashes().ToList();
            }

            // Reset every flashed element to 0
            map.Mutate((_, v) => v.HasValue ? v : 0);

            return totalFlashes;
        }

        public record struct Position(int X, int Y);
    }
}
