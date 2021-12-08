namespace PageOfBob.Advent2021.App.Days
{
    public static class Day07
    {
        public static void Execute()
        {
            var numbers = Utilities.GetEmbeddedData("07").Split(',', StringSplitOptions.RemoveEmptyEntries).AsNumbers().ToList();

            var start = numbers.Min();
            var end = numbers.Max();


            var totalCostsToCalculate = end - start;
            var totals = new Dictionary<int, int>();
            var currentTotal = 0;
            for (var x = 0; x <= totalCostsToCalculate; x++)
            {
                var total = currentTotal + x;
                totals.Add(x, total);
                currentTotal = total;
            }

            var smallestDistance = Utilities.RangeFromTo(start, end)
                .Select(index => numbers.Select(n => totals[Math.Abs(index - n)]).Sum())
                .Min();

            Console.WriteLine(smallestDistance);
        }
    }
}
