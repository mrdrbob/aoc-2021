namespace PageOfBob.Advent2021.App.Days
{
    public static class Day03
    {
        public const int BitLength = 12;

        public static void Execute()
        {
            var bitList = Utilities.GetEmbeddedData("03").Lines().AsBits();
            var positionCounts = new Dictionary<int, BitCount>();
            foreach (var bits in bitList)
            {
                for (int x = 0; x < bits.Length; x++)
                {
                    var bitCount = positionCounts.TryGetValue(x, out var existingBitCount) ? existingBitCount : new BitCount();
                    positionCounts[x] = bitCount.Add(bits[x]);
                }
            }

            var gamma = Enumerable.Range(0, BitLength).Aggregate(0u, (output, position) => positionCounts[position].MostCommonBit ? output.WithBitSetAtPosition(position) : output);
            
            // Invert and mask off only the left-most 12 bits
            var epislon = ~gamma & 0x0FFF;

            var output = gamma * epislon;
            Console.WriteLine(output);
        }

        public static uint WithBitSetAtPosition(this uint value, int position)
            => value | (1u << (BitLength - position - 1));

        public record struct BitCount(int On, int Off)
        {
            public BitCount Add(bool value) => new BitCount(this.On + (value ? 1 : 0), this.Off + (value ? 0 : 1));
            public bool MostCommonBit => On > Off;
        }

        public static IEnumerable<bool[]> AsBits(this IEnumerable<string> lines)
            => lines.Select(x => x.Select(y => y == '0' ? false : true).ToArray());

    }
}
