namespace PageOfBob.Advent2021.App.Days
{
    using Rules = Dictionary<(char, char), char>;

    public static class Day14
    {
        public static void Execute()
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
