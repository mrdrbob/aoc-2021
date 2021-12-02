namespace PageOfBob.Advent2021.App.Days
{
    public static class Day02
    {
        public static void Execute()
        {
            var finalPosition = Utilities.GetEmbeddedData("02").Lines().Aggregate(new Position(), (acc, line) => {
                var instruction = Instruction.FromString(line);
                switch (instruction.Direction)
                {
                    case "forward": return acc with { Horizontal = acc.Horizontal + instruction.Distance, Depth = acc.Depth + (acc.Aim * instruction.Distance) };
                    case "down": return acc with { Aim = acc.Aim + instruction.Distance };
                    case "up": return acc with { Aim = acc.Aim - instruction.Distance };
                    default: throw new NotImplementedException();
                }
            });

            Console.Write(finalPosition.Horizontal * finalPosition.Depth);
        }

        public record Instruction(string Direction, int Distance)
        {
            public static Instruction FromString(string line)
            {
                var split = line.Split(' ');
                return new Instruction(split[0], int.Parse(split[1]));
            }
        }

        public record struct Position(int Horizontal, int Depth, int Aim);
    }
}
