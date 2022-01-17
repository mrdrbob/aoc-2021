namespace PageOfBob.Advent2021.App.Days
{
    public static class Day21
    {
        public static void Execute()
        {
            var players = Utilities.GetEmbeddedData("21").Lines()
                .Select(x => x.Substring("Player 1 starting position: ".Length))
                .AsNumbers()
                .Select(x => new Player(x))
                .ToList();

            // Example
            // players = new List<Player> { new Player(4), new Player(8) };

            var die = new DeterministicDie();
            var game = new Game(players, die, 1000);
            Player? winner = null;

            while ((winner = game.Turn()) == null) ;

            var loser = game.Players.Single(x => x.Score != winner.Score);
            Console.WriteLine(loser.Score * die.TotalRolls);
        }

        private class Game
        {
            private readonly IList<Player> players;
            private readonly IDie die;
            private readonly int maxScore;

            public Game(IList<Player> players, IDie die, int maxScore)
            {
                this.players = players;
                this.die = die;
                this.maxScore = maxScore;
            }

            public Player? Turn()
            {
                foreach (var player in players)
                {
                    var newScore = player.Roll(die);
                    if (newScore >= maxScore)
                        return player;
                }

                return null;
            }

            public IEnumerable<Player> Players => players;
        }

        private class Player
        {
            public Player(int position)
            {
                Position = position - 1;
                Score = 0;
                TotalRolls = 0;
            }

            public int Position { get; private set; }
            public int Score { get; private set; }
            public int TotalRolls { get; private set; }

            public int Roll(IDie die)
            {
                var move = die.RollTimes(3);
                TotalRolls += 3;
                Position = (Position + move) % 10;
                Score += (Position + 1);
                return Score;
            }
        }

        private interface IDie
        {
            int TotalRolls { get; }
            int Next();
        }

        private static int RollTimes(this IDie die, int times)
            => Enumerable.Range(0, times).Sum(x => die.Next());

        private class DeterministicDie : IDie
        {
            private int current = 0;
            public int Next() => (current++ % 100) + 1;
            public int TotalRolls => current;
        }
    }
}
