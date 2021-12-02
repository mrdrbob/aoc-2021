namespace PageOfBob.Advent2021.App.Days
{
    public static class Day01
    {
        public static void Execute()
        {
            var text = Utilities.GetEmbeddedData("01");
            var (_, total) = text.Lines().AsNumbers().SlidingWindowSum(3).Aggregate(Accumulator.Default(), (acc, line) => acc.Add(line));
            Console.WriteLine(total);
        }

        public static IEnumerable<int> SlidingWindowSum(this IEnumerable<int> values, int bufferSize)
        {
            var queue = new Queue<int>();
            foreach (var value in values)
            {
                queue.Enqueue(value);
                if (queue.Count == bufferSize)
                {
                    yield return queue.Sum();
                    queue.Dequeue();
                }
            }
        }

        public record struct Accumulator(int? LastNumber, int Total)
        {
            public static Accumulator Default() => new Accumulator(null, 0);

            public Accumulator Add(int number)
            {
                int newTotal = (LastNumber.HasValue && LastNumber.Value < number) ? Total + 1 : Total;
                return new Accumulator(number, newTotal);
            }
        }
    }
}
