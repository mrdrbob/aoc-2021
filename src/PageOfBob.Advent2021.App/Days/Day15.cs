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
                .Select(x => new Node(x, map.Get(x)));

            var explored = new Dictionary<Position, Node>();
            var unexplored = new Dictionary<Position, Node>();

            foreach (var node in nodes)
                unexplored.Add(node.Position, node);

            var zero = new Position(0, 0);
            var end = new Position(map.Width - 1, map.Height - 1);

            var startNode = unexplored[zero];
            startNode.Estimate = 0;

            var priorityQueue = new PriorityQueue<Position, int>();
            priorityQueue.Enqueue(zero, 0);


            while (true)
            {
                var position = priorityQueue.Dequeue();

                if (explored.ContainsKey(position))
                    continue;

                var node = unexplored[position];

                if (node.Position == end)
                {
                    while (node.Via.HasValue)
                    {
                        yield return (node.Position, node.Risk);
                        node = explored[node.Via.Value];
                    }

                    yield return (node.Position, node.Risk);
                    yield break;
                }

                var nodesToVisit = node.Position.GetOrdinalPositions(map.Width, map.Height)
                    .Where(p => unexplored.ContainsKey(p))
                    .Select(p => unexplored[p]);

                foreach (var n in nodesToVisit)
                {
                    var totalEstimate = node.Estimate + n.Risk;
                    if (totalEstimate < n.Estimate)
                    {
                        n.Estimate = totalEstimate;
                        n.Via = node.Position;
                        priorityQueue.Enqueue(n.Position, n.Estimate);
                    }
                }

                explored.Add(position, node);
                unexplored.Remove(position);
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

            public int Estimate { get; set; } = int.MaxValue;
            public Position? Via { get; set; }
        }
    }
}
