namespace PageOfBob.Advent2021.App.Days
{
    public static class Day08
    {
        public static readonly char[] PossibleSegments = new [] { 'a', 'b', 'c', 'd', 'e', 'f', 'g' };

        public static readonly Dictionary<string, int> SegmentLookup = new Dictionary<string, int>
        {
            { "abcefg", 0 },
            { "cf", 1 },
            { "acdeg", 2 },
            { "acdfg", 3 },
            { "bcdf", 4 },
            { "abdfg", 5 },
            { "abdefg", 6 },
            { "acf", 7 },
            { "abcdefg", 8 },
            { "abcdfg", 9 },
        };

        public static void Execute()
        {
            var uniqueLengths = new HashSet<int> { 2, 4, 3, 7 };
            var numbers = Utilities.GetEmbeddedData("08").Lines().Select(Display.FromLine).ToList();

            // Part 1
            // var uniques = numbers.Select(numbers => numbers.OutputValues.Where(v => uniqueLengths.Contains(v.Length)).Count()).Sum();
            // Console.WriteLine(uniques);

            var output = numbers.Select(x => x.Solve()).Sum();
            Console.WriteLine(output);

        }

        public record Display(string[] UniquePatterns, string[] OutputValues)
        {
            public static Display FromLine(string line)
            {
                var (first, second) = line.Split(" | ");
                return new Display(
                    first.Split(' ', StringSplitOptions.RemoveEmptyEntries),
                    second.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                );
            }

            public int Solve()
            {
                // How many times does each letter appear in the unique patterns
                var frequencies = PossibleSegments.Select(x => (Character: x, Freqency: UniquePatterns.Count(y => y.Contains(x)))).ToList();

                // Final mapping of mixed-up segment -> correct segment
                var lookup = new Dictionary<char, char>();

                // B, E, and F appear a known number of times
                lookup[frequencies.Single(x => x.Freqency == 6).Character] = 'b';
                lookup[frequencies.Single(x => x.Freqency == 4).Character] = 'e';
                lookup[frequencies.Single(x => x.Freqency == 9).Character] = 'f';

                // A is in seven, but not one.
                var seven = UniquePatterns.Single(x => x.Length == 3);
                var one = UniquePatterns.Single(x => x.Length == 2);
                lookup[seven.Single(c => !one.Contains(c))] = 'a';

                // D appears 7 times and is in the four pattern
                var four = UniquePatterns.Single(x => x.Length == 4);
                lookup[frequencies.Single(x => x.Freqency == 7 && four.Contains(x.Character)).Character] = 'd';

                // C is matched 8 times and appears in the one pattern
                lookup[frequencies.Single(x => x.Freqency == 8 && one.Contains(x.Character)).Character] = 'c';

                // Eh, G is whatever's left
                lookup[PossibleSegments.Single(x => !lookup.ContainsKey(x))] = 'g';

                // Correct the wiring to the correct segments.
                var corrected = OutputValues.Select(x => new string(x.Select(y => lookup[y]).OrderBy(t => t).ToArray()));

                // Lookup what the segments should be and build a number.
                var output = corrected.Select(x => SegmentLookup[x]).Aggregate(0, (acc, x) => (acc * 10) + x);

                // Console.WriteLine("{0} | {1}", string.Join(" ", OutputValues), output);
                return output;
            }
        }
    }
}
