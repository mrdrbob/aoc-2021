using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PageOfBob.Advent2021.App.Days
{
    public static class Day25
    {
        private static char[] EmptyData;
        private static Stack<char[]> MapPool = new Stack<char[]>();

        public static void Execute()
        {
            var data = Utilities.GetEmbeddedData("25").Where(x => !char.IsWhiteSpace(x)).ToArray();

            var map = new Map(data, 139, 137);
            EmptyData = Enumerable.Repeat('.', map.Width * map.Height).ToArray();

            var done = false;
            int step = 0;

           //map.Print();

            while (!done)
            {
                step++;

                var (newMap, moved) = map.Step();
                done = moved == 0;
                map = newMap;
                //Console.WriteLine($"{step} steps, {moved} moved");
                //newMap.Print();
                //Console.ReadLine();
            }
            Console.WriteLine(step);
        }

        public record struct Vector2(int X, int Y);

        public static Vector2 GoEast(this Vector2 position) => new Vector2(position.X + 1, position.Y);
        public static Vector2 GoSouth(this Vector2 position) => new Vector2(position.X, position.Y + 1);

        public record Map(char[] Data, int Width, int Height);

        public static void Print(this Map map)
        {
            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    Console.Write(map.CharAt(x, y));
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        // => new Map(Enumerable.Repeat('.', map.Width * map.Height).ToArray(), map.Width, map.Height);
        public static Map Empty(this Map map)
        {
            if (!MapPool.Any())
            {
                MapPool.Push(new char[map.Width * map.Height]);
            }

            var data = MapPool.Pop();
            Array.Copy(EmptyData, data, EmptyData.Length);
            return new Map(data, map.Width, map.Height);
        }

        public static void Return(this Map map)
        {
            MapPool.Push(map.Data);
        }

        public static IEnumerable<Vector2> AllPositions(this Map map)
        {
            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    yield return new Vector2(x, y);
                }
            }
        }

        public static char CharAt(this Map map, Vector2 position)
            => CharAt(map, position.X, position.Y);

        public static char CharAt(this Map map, int x, int y)
        {
            while (x < 0)
                x += map.Width;
            while (y < 0)
                y += map.Height;
            while (x >= map.Width)
                x -= map.Width;
            while (y >= map.Height)
                y -= map.Height;

            return map.Data[y * map.Width + x];
        }

        public static void SetAt(this Map map, Vector2 position, char c)
            => SetAt(map, position.X, position.Y, c);

        public static void SetAt(this Map map, int x, int y, char c)
        {
            while (x < 0)
                x += map.Width;
            while (y < 0)
                y += map.Height;
            while (x >= map.Width)
                x -= map.Width;
            while (y >= map.Height)
                y -= map.Height;

            map.Data[y * map.Width + x] = c;
        }

        public static (Map map, int TotalMoves) Step(this Map map)
        {
            var (eastMap, eastMove) = MoveHeard(map, 'v', '>', GoEast);
            var (southMap, southMove) = MoveHeard(eastMap, '>', 'v', GoSouth);
            return (southMap, eastMove + southMove);
        }

        public static (Map Map, int TotalMoves) MoveHeard(this Map map, char stationaryGroup, char movingGroup, Func<Vector2, Vector2> movement)
        {
            int moves = 0;

            var output = map.Empty();
            // Copy over stationary group
            foreach (var pos in map.AllPositions())
            {
                if (map.CharAt(pos) == stationaryGroup)
                {
                    output.SetAt(pos, stationaryGroup);
                }
            }

            // Move eastbound if possible
            foreach (var pos in map.AllPositions())
            {
                if (map.CharAt(pos) == movingGroup)
                {
                    var destination = movement(pos);
                    if (map.CharAt(destination) == '.')
                    {
                        output.SetAt(destination, movingGroup);
                        moves++;
                    }
                    else
                    {
                        output.SetAt(pos, movingGroup);
                    }
                }
            }

            map.Return();
            return (output, moves);
        }
    }
}
