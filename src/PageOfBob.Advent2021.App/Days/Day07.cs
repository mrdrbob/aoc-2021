namespace PageOfBob.Advent2021.App.Days
{
    public static class Day07
    {
        public static void Execute()
        {
            var numbers = Utilities.GetEmbeddedData("07").Split(',', StringSplitOptions.RemoveEmptyEntries).AsNumbers().ToList();

            var start = numbers.Min();
            var end = numbers.Max();

            var smallestDistance = Utilities.RangeFromTo(start, end)
                .Select(index => numbers.Select(n => Math.Abs(index - n)).Sum())
                .Min();

            Console.WriteLine(smallestDistance);
        }
    }
}
