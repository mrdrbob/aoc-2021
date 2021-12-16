using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PageOfBob.Advent2021.App
{
    public record Map<T>(int Width, int Height, List<T> Data)
    {
        public int DataPosition(Position pos) => pos.Y * Width + pos.X;
        public T Get(Position pos) => Data[DataPosition(pos)];
        public void Set(Position pos, T value) => Data[DataPosition(pos)] = value;
        public void Modify(Position pos, Func<T, T> modify) => Set(pos, modify(Get(pos)));
    }

    public static class Map
    {
        public static Map<T> Empty<T>(int width, int height) where T : struct => new Map<T>(width, height, Enumerable.Repeat(default(T), width * height).ToList());
    }

    public record struct Position(int X, int Y)
    {
        public static Position FromString(string line)
        {
            var (x, y) = line.Split(',');
            return new Position(int.Parse(x), int.Parse(y));
        }
    }

    public static class MapExtensions
    {
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

        public static void Print<T>(this Map<T> map, Func<T, string> toString)
        {
            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    Console.Write(toString(map.Get(new Position(x, y))));
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

        public static Map<T> FoldHorizontal<T>(this Map<T> map, int y, Func<T, T, T> mixFlipped) where T : struct
        {
            var newMap = Map.Empty<T>(map.Width, y);

            foreach (var pos in map.GetAllPositions().Where(pos => pos.Y > y))
            {
                var sourceValue = map.Get(pos);

                // var dest = new Position(pos.X, map.Height - pos.Y - 1);
                var dest = new Position(pos.X, y - (pos.Y - y));
                var destValue = map.Get(dest);

                var finalValue = mixFlipped(sourceValue, destValue);
                newMap.Set(dest, finalValue);
            }

            return newMap;
        }

        public static Map<T> FoldVertical<T>(this Map<T> map, int x, Func<T, T, T> mixFlipped) where T : struct
        {
            var newMap = Map.Empty<T>(x, map.Height);

            foreach (var pos in map.GetAllPositions().Where(pos => pos.X > x))
            {
                var sourceValue = map.Get(pos);

                // var dest = new Position(map.Width - pos.X - 1, pos.Y);
                var dest = new Position(x - (pos.X - x), pos.Y);
                var destValue = map.Get(dest);

                var finalValue = mixFlipped(sourceValue, destValue);
                newMap.Set(dest, finalValue);
            }

            return newMap;
        }
    }
}
