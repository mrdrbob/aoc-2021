namespace PageOfBob.Advent2021.App.Days
{
    using Rules = Dictionary<(char, char), char>;
    using Rules2 = Dictionary<string, string[]>;

    public static class Day14
    {

        public static void Execute()
        {
            var counts = new Dictionary<string, ulong>();
            var rules = new Rules2();

            var lines = Utilities.GetEmbeddedData("14").Lines();
            var firstLine = lines.First();
            var firstCharacter = firstLine[0];

            // "Parse" the input
            foreach (var pair in firstLine.ToPairs())
            {
                counts.Plus(pair);
            }

            foreach (var line in lines.Skip(2))
            {
                var key = line.Substring(0, 2);
                var newChar = line[6].ToString();
                rules[key] = new[] { key[0].ToString() + newChar, newChar + key[1].ToString() };
            }

            // Apply the rules
            for (int x = 0; x < 40; x++)
            {
                counts = counts.ApplyRules2(rules);
            }

            // Sum up the second character from each pair.
            // The first character will always happen exactly once at the start.
            var characterCounts = new Dictionary<char, ulong>();
            characterCounts[firstCharacter] = 1;
            foreach (var kvp in counts)
            {
                characterCounts.Plus(kvp.Key[1], kvp.Value);
            }

            var min = characterCounts.Values.Min();
            var max = characterCounts.Values.Max();
            Console.WriteLine(max - min);
        }

        public static Dictionary<string, ulong> ApplyRules2(this Dictionary<string, ulong> count, Rules2 rules)
        {
            var output = new Dictionary<string, ulong>();

            foreach (var kvp in count)
            {
                foreach (var insertion in rules[kvp.Key])
                {
                    output.Plus(insertion, kvp.Value);
                }
            }


            return output;
        }

        public static IEnumerable<string> ToPairs(this string line)
        {
            for (int x = 0; x < line.Length - 1; x++)
            {
                yield return line.Substring(x, 2);
            }
        }



        // Part 1
        public static void ExecutePartOne()
        {
            var lines = Utilities.GetEmbeddedData("14").Lines();
            var template = lines.First().ToList();
            var rules = new Rules();
            foreach (var line in lines.Skip(2))
            {
                rules[(line[0], line[1])] = line[6];
            }

            for (int x = 0; x < 10; x++)
            {
                // Console.WriteLine(new String(template.ToArray()));
                Console.WriteLine("{0} {1}", x, template.Count);
                template.ApplyRules(rules);
            }

            var counts = template.GroupBy(x => x).Select(x => (Key: x.Key, Count: x.Count())).ToList();
            var min = counts.Min(x => x.Count);
            var max = counts.Max(x => x.Count);
            Console.WriteLine(max - min);
        }

        public static void ApplyRules(this List<char> template, Rules rules)
        {
            for (int i = template.Count - 2; i >= 0; i--)
            {
                var ruleKey = (template[i], template[i + 1]);
                var injected = rules[ruleKey];
                template.Insert(i + 1, injected);
            }
        }
    }
}
