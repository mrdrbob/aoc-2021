namespace PageOfBob.Advent2021.App.Days
{
    public static class Day20
    {
        public static void Execute()
        {
            var lines = Utilities.GetEmbeddedData("20").Lines();
            var algorithm = lines.First().Select(x => x == '#').ToArray();

            var points = lines.Skip(2).SelectMany((line, y) => line.Select((v, x) => (X: x, Y: y, Light: v == '#')));
            var lightPoints = points.Where(pt => pt.Light).Select(pt => new Vector2(pt.X, pt.Y)).ToHashSet();

            var world = new BoundingRectangle(new Range(0, 100), new Range(0, 100));

            var field = new Field(lightPoints, world, 1);
            // field.Print();

            /* Part 1
            field = field.Enhance(algorithm);
            // field.Print();

            field = field.Enhance(algorithm);
            // field.Print();
            */

            for (var x = 0; x < 50; x++)
                field = field.Enhance(algorithm);

            Console.WriteLine(field.Points.Count);
        }

        public static Field Enhance(this Field field, IList<bool> algorithm)
        {
            var points = new HashSet<Vector2>();
            var runs = field.TotalRuns + 1;
            var world = field.World.Expand(1);

            foreach (var point in world.GetAllPoints())
            {
                var index = GetAlgorithmIndex(field, point);
                if (algorithm[index])
                {
                    points.Add(point);
                }
            }

            return new Field(points, world, runs);
        }

        public static int GetAlgorithmIndex(this Field field, Vector2 point)
        {
            int result = 0;
            foreach (var pt in point.SurroundingPoints())
            {
                result <<= 1;
                result |= field.IsPointSet(pt) ? 1 : 0;
            }
            return result;
        }

        public static bool IsPointSet(this Field field, Vector2 point)
            => field.World.Contains(point) ? field.Points.Contains(point) : field.TotalRuns % 2 == 0;

        public static void Print(this Field field)
        {
            int lastY = 0;
            foreach (var pt in field.World.GetAllPoints())
            {
                if (pt.Y != lastY)
                    Console.WriteLine();
                lastY = pt.Y;
                Console.Write(field.Points.Contains(pt) ? '#' : '.');
            }
            Console.WriteLine();
        }

        public record Field(HashSet<Vector2> Points, BoundingRectangle World, int TotalRuns);

        public record struct Vector2(int X, int Y);

        public static IEnumerable<Vector2> SurroundingPoints(this Vector2 center)
            => Enumerable.Range(center.Y - 1, 3).SelectMany(y => Enumerable.Range(center.X - 1, 3).Select(x => new Vector2(x, y)));

        public record struct Range(int Min, int Max);

        public static Range Expand(this Range range, int amount = 1)
            => new Range(range.Min - amount, range.Max + amount);

        public static bool Contains(this Range range, int value)
            => value >= range.Min && value < range.Max;

        public static IEnumerable<int> GetAllValues(this Range range)
            => Enumerable.Range(range.Min, range.Max - range.Min);

        public record struct BoundingRectangle(Range Width, Range Height);

        public static IEnumerable<Vector2> GetAllPoints(this BoundingRectangle rectangle)
            => rectangle.Height.GetAllValues().SelectMany(y => rectangle.Width.GetAllValues().Select(x => new Vector2(x, y)));

        public static BoundingRectangle Expand(this BoundingRectangle rectangle, int amount = 1)
            => new BoundingRectangle(rectangle.Width.Expand(amount), rectangle.Height.Expand(amount));

        public static bool Contains(this BoundingRectangle rectangle, Vector2 point)
            => rectangle.Width.Contains(point.X) && rectangle.Height.Contains(point.Y);
    }
}
