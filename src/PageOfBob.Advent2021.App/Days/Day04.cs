using System.Text;

namespace PageOfBob.Advent2021.App.Days
{
    public static class Day04
    {
        public static void Execute()
        {
            var lines = Utilities.GetEmbeddedData("04").Split("\n\n");
            var picks = lines.First().Split(',').AsNumbers().ToList();

            var cards = lines.Skip(1).Select(BingoCard.FromString).ToList();

            var (winner, winningPick) = FindWinner(picks, cards) ?? throw new NotImplementedException();
            var sum = winner.SumOfAllUnmarked();
            Console.WriteLine(sum * winningPick);

        }

        private static (BingoCard, int)? FindWinner(IEnumerable<int> picks, IEnumerable<BingoCard> cards)
        {
            foreach (var pick in picks)
            {
                foreach (var card in cards)
                {
                    card.Add(pick);
                    if (card.AnyWins())
                        return (card, pick);
                }
            }

            return null;
        }

        public class BingoCard
        {
            public Dictionary<Position, int> Numbers { get; } = new Dictionary<Position, int>();
            public HashSet<int> Matches { get; } = new HashSet<int>();

            public int SumOfAllUnmarked() => Numbers.Values.Where(x => !Matches.Contains(x)).Sum();

            public static BingoCard FromString(string cardString)
            {
                var card = new BingoCard();
                int y = 0;
                foreach (var line in cardString.Lines())
                {
                    int x = 0;
                    foreach (var number in line.Split(' ', StringSplitOptions.RemoveEmptyEntries).AsNumbers())
                    {
                        card.Numbers.Add(new Position(x, y), number);
                        x++;
                    }
                    y++;
                }
                return card;
            }

            public int NumberAt(Position pos) => Numbers[pos];
            public bool MatchAt(Position pos) => Matches.Contains(NumberAt(pos));

            public void Add(int position)
            {
                if (Numbers.Values.Contains(position))
                {
                    Matches.Add(position);
                }
            }

            public bool AnyWins()
                => Enumerable.Range(0, 5).Any(y => Enumerable.Range(0, 5).All(x => MatchAt(new Position(x, y)))) ||
                    Enumerable.Range(0, 5).Any(x => Enumerable.Range(0, 5).All(y => MatchAt(new Position(x, y))));

            public override string ToString()
            {
                var buffer = new StringBuilder();

                foreach (int y in Enumerable.Range(0, 5))
                {
                    foreach (int x in Enumerable.Range(0, 5))
                    {
                        buffer.Append(string.Format("{0:##}", NumberAt(new Position(x, y))));
                        if (MatchAt(new Position(x, y)))
                        {
                            buffer.Append("* ");
                        }
                        else
                        {
                            buffer.Append("  ");
                        }
                    }
                    buffer.Append(Environment.NewLine);
                }

                return buffer.ToString();
            }
        }

        public record struct Position(int X, int Y);
    }
}
