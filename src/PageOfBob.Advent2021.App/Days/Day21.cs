namespace PageOfBob.Advent2021.App.Days
{
    public static class Day21
    {
        private static readonly IEnumerable<DieRollOutcome> AllPossibleDieOutcomes;
        public const int EndGameScore = 21;

        static Day21() {
            AllPossibleDieOutcomes = PrecalculatePossibleDieRollOutcomes();
        }

        public static void Execute()
        {
            var players = Utilities.GetEmbeddedData("21").Lines()
                .Select(x => x.Substring("Player 1 starting position: ".Length))
                .AsNumbers()
                .Select(x => new Player(x, 0))
                .ToList();

            // Example
            // players = new List<Player> { new Player(4, 0), new Player(8, 0) };

            // Part 1
            /*
            var die = new DeterministicDie();
            var game = new Game(players, die, 1000);
            Player? winner = null;

            while ((winner = game.Turn()) == null) ;

            var loser = game.Players.Single(x => x.Score != winner.Score);
            Console.WriteLine(loser.Score * die.TotalRolls);
            */

            // Track how many universes each game state appears in.
            var universes = new Dictionary<Game, ulong>();

            // Total number of universes in which each player wins.
            var totalWinsByPlayer = new Dictionary<int, ulong>();

            // The initial game. It occurs in exactly one universe.
            var initial = new Game(players[0], players[1]);
            universes.Plus(initial, 1);

            while (universes.Count > 0)
            {
                // Play rounds until all possible games have been won.
                universes = PlayRound(universes, totalWinsByPlayer);
            }

            var winner = Math.Max(totalWinsByPlayer[1], totalWinsByPlayer[2]);
            Console.WriteLine(winner);
        }

        public static Dictionary<Game,ulong> PlayRound(Dictionary<Game, ulong> state, Dictionary<int, ulong> winsByPlayer)
        {
            // Going to rebuild the count of how many universes in which each game appears.
            // Games are "removed" as their played (by virtual of not being re-added)
            // New permutations are added to the list.
            var newState = new Dictionary<Game, ulong>();

            // Go through all known possible game states.
            foreach (var kvp in state)
            {
                // The gamestate and how many universes this particular state exists in.
                var game = kvp.Key;
                var totalUniverses = kvp.Value;

                // Start by playing player one's turns. Use `AllPossibleDieOutcomes`
                // to simulate all possible die rolls.
                foreach (var oneDie in AllPossibleDieOutcomes)
                {
                    // How many universes did player one roll this result.
                    var universesForPlayerOne = totalUniverses * oneDie.UniversesOccurred;

                    // Move player one.
                    var playerOneResult = game.One.Move(oneDie.TotalValue);

                    // If player one won this round, we add their total universes to their
                    // win count and bail out of this branch.
                    if (playerOneResult.Score >= EndGameScore)
                    {
                        winsByPlayer.Plus(1, universesForPlayerOne);
                        continue;
                    }

                    // Player one didn't win, so player two takes their turn.
                    // Same basic idea. Play all possible outcomes for their roll.
                    foreach (var twoDie in AllPossibleDieOutcomes)
                    {
                        // How many universes did this roll occur in?
                        var universesForPlayerTwo = universesForPlayerOne * twoDie.UniversesOccurred;

                        // Move player tow
                        var playerTwoResult = game.Two.Move(twoDie.TotalValue);

                        // Track a win and bail as appropriate
                        if (playerTwoResult.Score >= EndGameScore)
                        {
                            winsByPlayer.Plus(2, universesForPlayerTwo);
                            continue;
                        }

                        // Otherwise, neither player won, so we'll continue this game.
                        // Add it to the new list of known possible game states, along with
                        // the number of universes in which it occurs.
                        var newGameState = new Game(playerOneResult, playerTwoResult);
                        newState.Plus(newGameState, universesForPlayerTwo);
                    }
                }
            }

            // Return the new list of known possible game states/universes.
            return newState;
        }

        public record struct DieRollOutcome(int TotalValue, ulong UniversesOccurred);

        // Calculate all possible outcomes of rolling a 3-sided die three times, and how frequently each outcome occurs.
        public static IEnumerable<DieRollOutcome> PrecalculatePossibleDieRollOutcomes()
        {
            var allDieRolls = Enumerable.Range(1, 3).SelectMany(a =>
                Enumerable.Range(1, 3).SelectMany(b =>
                    Enumerable.Range(1, 3).Select(c => a + b + c)
                )
            );
            var counts = allDieRolls.CountFrequencyUlong();
            return counts.Select(kvp => new DieRollOutcome(kvp.Key, kvp.Value)).ToList();
        }

        public record struct Player(int Position, int Score)
        {
            public Player Move(int steps)
            {
                var newPosition = (((Position - 1) + steps) % 10) + 1;
                var newScore = Score + newPosition;

                return new Player(newPosition, newScore);
            }
        }

        public record struct Game(Player One, Player Two);


        /*
         * Part 1
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
        */
    }
}
