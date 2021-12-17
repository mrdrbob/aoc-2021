namespace PageOfBob.Advent2021.App
{
    public static class Utilities
    {
        public static string GetEmbeddedData(string day)
        {
            using (var stream = typeof(Utilities).Assembly.GetManifestResourceStream($"PageOfBob.Advent2021.App.Data.{day}.txt"))
            using (var textReader = new StreamReader(stream ?? throw new NotImplementedException()))
            {
                return textReader.ReadToEnd();
            }
        }

        public static IEnumerable<string> Lines(this string value)
        {
            var reader = new StringReader(value) ?? throw new NotImplementedException();
            string? line = null;
            while ((line = reader.ReadLine()) != null)
            {
                yield return line;
            }
        }

        public static IEnumerable<int> AsNumbers(this IEnumerable<string> lines)
            => lines.Select(int.Parse);

        public static void Deconstruct<T>(this T[] split, out T first, out T second)
        {
            first = split[0];
            second = split[1];
        }

        public static IEnumerable<int> RangeFromTo(int start, int end)
        {
            int step = start > end ? -1 : 1;
            int pos = start;
            while (pos != end)
            {
                yield return pos;
                pos += step;
            }

            yield return pos;
        }

        public static T GetOrCreate<K, T>(this Dictionary<K, T> dictionary, K key, Func<K, T> create)
            where K : notnull
        {
            if (dictionary.TryGetValue(key, out T? existingValue))
                return existingValue;

            var newValue = create(key);
            dictionary.Add(key, newValue);
            return newValue;
        }

        public static void Plus<T>(this Dictionary<T, int> dictionary, T key, int? amount = null)
            where T : notnull
            => dictionary[key] = dictionary.TryGetValue(key, out int existingValue) ? existingValue + (amount.HasValue ? amount.Value : 1) : (amount.HasValue ? amount.Value : 1);

        public static void Plus<T>(this Dictionary<T, ulong> dictionary, T key, ulong? amount = null)
            where T : notnull
            => dictionary[key] = dictionary.TryGetValue(key, out ulong existingValue) ? existingValue + (amount.HasValue ? amount.Value : 1) : (amount.HasValue ? amount.Value : 1);
    }
}
