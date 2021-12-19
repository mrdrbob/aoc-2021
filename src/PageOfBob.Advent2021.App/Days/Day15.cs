namespace PageOfBob.Advent2021.App.Days
{
    public static class Day15
    {
        public static void Execute()
        {
            var rawMap = Utilities.GetEmbeddedData("15").Replace("\n", "").Replace("\r", "").Select(x => int.Parse(x.ToString())).ToList();
            var size = (int)Math.Sqrt(rawMap.Count);
            var map = new Map<int>(size, size, rawMap);
            var totalRisk = FindShortestPath(map).Select(x => x.Risk).Sum() - map.Get(new Position(0, 0));
            Console.WriteLine(totalRisk);
        }

        private static IEnumerable<(Position Position, int Risk)> FindShortestPath(Map<int> map)
        {
            var nodes = map.GetAllPositions()
                .Select(x => new Node(x, map.Get(x)))
                .ToList();

            var zero = new Position(0, 0);
            var end = new Position(map.Width - 1, map.Height - 1);

            var startNode = nodes.Single(x => x.Position == zero);
            startNode.Estimate = 0;


            while (true)
            {
                var node = nodes.Where(x => !x.Explored).OrderBy(x => x.Estimate).First();
                if (node.Position == end)
                {
                    while (node.Via.HasValue)
                    {
                        yield return (node.Position, node.Risk);
                        node = nodes.Single(x => x.Position == node.Via);
                    }

                    yield return (node.Position, node.Risk);
                    yield break;
                }

                var nodesToVisit = node.Position.GetOrdinalPositions(map.Width, map.Height)
                    .Select(p => nodes.Single(n => n.Position == p))
                    .Where(n => !n.Explored);

                foreach (var n in nodesToVisit)
                {
                    var totalEstimate = node.Estimate + n.Risk;
                    if (totalEstimate < n.Estimate)
                    {
                        n.Estimate = totalEstimate;
                        n.Via = node.Position;
                    }
                }

                node.Explored = true;
            }
        }

        public class Node
        {
            public Node(Position position, int risk)
            {
                Position = position;
                Risk = risk;
            }

            public Position Position { get; }
            public int Risk { get; }

            public bool Explored { get; set; }
            public int Estimate { get; set; } = int.MaxValue;
            public Position? Via { get; set; }
        }
    }
}
