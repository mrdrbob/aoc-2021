namespace PageOfBob.Advent2021.App.Days
{
    public static class Day05
    {
        public static void Execute()
        {
            var lines = Utilities.GetEmbeddedData("05").Lines().Select(Line.FromString).ToList();
            var pointCounts = new Dictionary<Position, int>();

            foreach (var line in lines)
            {
                foreach (var position in line.GetLine())
                {
                    pointCounts[position] = (pointCounts.TryGetValue(position, out int value) ? value : 0) + 1;
                }
            }

            var overlapCounts = pointCounts.Values.Count(x => x >= 2);
            Console.WriteLine(overlapCounts);
        }

        public record struct Line(Position Start, Position End)
        {
            public static Line FromString(string line)
            {
                var (start, end) = line.Split(" -> ", StringSplitOptions.RemoveEmptyEntries);
                return new Line(Position.FromString(start), Position.FromString(end));
            }

            public IEnumerable<Position> GetLine()
            {
                if (Start.X == End.X)
                {
                    var x = Start.X;
                    return Utilities.RangeFromTo(Start.Y, End.Y).Select(y => new Position(x, y));
                }
                else if (Start.Y == End.Y)
                {
                    var y = Start.Y;
                    return Utilities.RangeFromTo(Start.X, End.X).Select(x => new Position(x, y));
                }
                else
                {
                    var xRange = Utilities.RangeFromTo(Start.X, End.X);
                    var yRange = Utilities.RangeFromTo(Start.Y, End.Y);
                    return xRange.Zip(yRange, (x, y) => new Position(x, y));
                }
            }
        }

        public record struct Position(int X, int Y)
        {
            public static Position FromString(string line)
            {
                var (x,y) = line.Split(',', StringSplitOptions.RemoveEmptyEntries);
                return new Position(int.Parse(x), int.Parse(y));
            }
        }
    }
}
