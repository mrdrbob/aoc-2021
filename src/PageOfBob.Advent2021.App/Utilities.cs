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
    }
}
