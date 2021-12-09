using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PageOfBob.Advent2021.App.Days
{
    public static class Day09
    {
        public static void Execute()
        {
            var heights = Utilities.GetEmbeddedData("09").Replace("\n", "").Replace("\r", "").Select(x => int.Parse(x.ToString())).ToArray();
            var map = new HeightMap(100, 100, heights);

            /* Part 1
            var points = map.ScanLowPoints().Select(x => x.value + 1).Sum();
            Console.WriteLine(points);
            */

            // Part 2
            var total = map.ScanLowPoints().Select(pt => map.CalculateBasin(pt.x, pt.y))
                .OrderByDescending(x => x)
                .Take(3)
                .Aggregate(1, (acc, x) => x * acc);

            Console.WriteLine(total);
        }

        public record HeightMap(int Width, int Height, int[] Map)
        {
            public int? GetAt(int x, int y)
            {
                if (x < 0 || x >= Width)
                    return null;

                if (y < 0 || y >= Height)
                    return null;

                return GetAtAssumeValidPosition(x, y);
            }

            public int GetAtAssumeValidPosition(int x, int y)
                => Map[y * Width + x];

            bool PointAtIsGreaterThan(int x, int y, int number)
            {
                var numberAtPosition = GetAt(x, y);
                if (numberAtPosition == null)
                    return true;

                return (numberAtPosition.Value > number);
            }

            public bool IsLowPoint(int x, int y)
            {
                var number = GetAtAssumeValidPosition(x, y);
                return PointAtIsGreaterThan(x - 1, y, number)
                    && PointAtIsGreaterThan(x + 1, y, number)
                    && PointAtIsGreaterThan(x, y + 1, number)
                    && PointAtIsGreaterThan(x, y - 1, number);
            }

            public IEnumerable<(int x, int y, int value)> ScanLowPoints()
            {
                for (var y = 0; y < Height; y++)
                {
                    for (var x = 0; x < Width; x++)
                    {
                        if (IsLowPoint(x, y))
                            yield return (x, y, GetAtAssumeValidPosition(x, y));
                    }
                }
            }

            public int CalculateBasin(int posX, int posY)
            {
                var stack = new Stack<(int, int)>();
                var checkedPoints = new HashSet<(int, int)>();

                var total = 0;
                stack.Push((posX, posY));

                while (stack.Count > 0)
                {
                    var (x, y) = stack.Pop();
                    if (checkedPoints.Contains((x, y)))
                        continue;

                    checkedPoints.Add((x, y));

                    var valueAtPoint = GetAt(x, y);
                    if (!valueAtPoint.HasValue)
                        continue;

                    if (valueAtPoint.Value == 9)
                        continue;

                    total++;
                    stack.Push((x + 1, y));
                    stack.Push((x - 1, y));
                    stack.Push((x, y + 1));
                    stack.Push((x, y - 1));
                }

                return total;
            }
        }
    }
}
