using PageOfBob.Parsing;
using static PageOfBob.Parsing.Rules;

namespace PageOfBob.Advent2021.App.Days
{
    public static class Day22
    {

        private static class Parser
        {
            public static readonly Rule<char, Cuboid> CuboidParser;

            static Parser()
            {
                var ON = StringRules.Text("on");
                var OFF = StringRules.Text("off");
                var DOTDOT = StringRules.Text("..");

                var SPACE = Match(' ');
                var X = Match('x');
                var Y = Match('y');
                var Z = Match('z');
                var EQ = Match('=');
                var COMMA = Match(',');

                var OnOrOff = Any(
                    ON.Map(_ => true),
                    OFF.Map(_ => false)
                );

                // on x=-17..30,y=-5..41,z=-33..14

                var Num = Match<char>(x => x == '-' || char.IsDigit(x))
                    .ManyAsString(required: true)
                    .Map(int.Parse);

                Rule<char, Range> Range(Rule<char, char> name)
                {
                    return name.ThenIgnore(EQ)
                        .ThenKeep(Num)
                        .ThenIgnore(DOTDOT)
                        .Then(Num, (min, max) => new Range(min, max));
                }

                var cube = Range(X)
                    .ThenIgnore(COMMA)
                    .Then(Range(Y), (acc, y) => (X: acc, Y: y))
                    .ThenIgnore(COMMA)
                    .Then(Range(Z), (acc, z) => new Cube(acc.X, acc.Y, z));

                CuboidParser = OnOrOff
                    .ThenIgnore(SPACE)
                    .Then(cube, (on, cube) => new Cuboid(on, cube))
                    .ThenIgnore(Match<char>(char.IsWhiteSpace));
            }
        }

        public static void Execute()
        {
            var instructions = Parser.CuboidParser.Tokenize(Sources.CharSource(Utilities.GetEmbeddedData("22")), true).ToList();
            // Console.WriteLine(cuboids.Count); // 420, nice
            var axisConstraint = new Range(-50, 50);
            var worldConstraint = new Cube(axisConstraint, axisConstraint, axisConstraint);

            IList<Cube> onCuboids = new List<Cube>();
            foreach (var instruction in instructions)
            {
                onCuboids = ApplyRule(onCuboids, instruction);
            }

            var totalSize = onCuboids
                // .Select(c => c.Constrain(worldConstraint))
                .Where(c => !c.IsEmpty())
                .Select(x => x.Size()).Sum();
            Console.WriteLine(totalSize);
        }

        private static IList<Cube> ApplyRule(IEnumerable<Cube> cuboids, Cuboid instruction)
        {
            var output = new List<Cube>();

            foreach (var cube in cuboids)
            {
                output.AddRange(cube.Subtract(instruction.Cube));
            }

            if (instruction.state)
                output.Add(instruction.Cube);

            return output;
        }

        private record struct Range(int Min, int Max);

        private static long Size(this Range self) => self.Max - self.Min + 1;

        private static bool Overlaps(this Range self, Range other)
            => !(other.Min > self.Max || other.Max < self.Min);

        private static bool IsEmpty(this Range self)
            => self.Size() <= 0;

        private static Range Constrain(this Range self, Range other)
            => new Range(Math.Max(self.Min, other.Min), Math.Min(self.Max, other.Max));

        private record struct Cube(Range Width, Range Height, Range Depth);
        
        private static bool Overlaps(this Cube self, Cube other)
            => self.Width.Overlaps(other.Width)
                && self.Height.Overlaps(other.Height)
                && self.Depth.Overlaps(other.Depth);

        private static bool IsEmpty(this Cube self)
            => self.Width.IsEmpty() || self.Height.IsEmpty() || self.Depth.IsEmpty();

        private static IEnumerable<Cube> Split(this Cube self, Cube other)
        {
            yield return new Cube(self.Width with { Max = other.Width.Min - 1 }, self.Height, self.Depth);
            yield return new Cube(self.Width with { Min = other.Width.Max + 1 }, self.Height, self.Depth);
            yield return new Cube(self.Width.Constrain(other.Width), self.Height with { Max = other.Height.Min - 1 }, self.Depth);
            yield return new Cube(self.Width.Constrain(other.Width), self.Height with { Min = other.Height.Max + 1 }, self.Depth);
            yield return new Cube(self.Width.Constrain(other.Width), self.Height.Constrain(other.Height), self.Depth with { Max = other.Depth.Min - 1 });
            yield return new Cube(self.Width.Constrain(other.Width), self.Height.Constrain(other.Height), self.Depth with { Min = other.Depth.Max + 1 });
        }

        private static IEnumerable<Cube> Subtract(this Cube self, Cube other)
        {
            if (!self.Overlaps(other))
                return new[] { self };
            return self.Split(other).Where(x => !x.IsEmpty());
        }

        private static long Size(this Cube self) => self.Width.Size() * self.Height.Size() * self.Depth.Size();

        private static Cube Constrain(this Cube self, Cube constraint)
            => new Cube(self.Width.Constrain(constraint.Width), self.Height.Constrain(constraint.Height), self.Depth.Constrain(constraint.Depth));

        private record Cuboid(bool state, Cube Cube);


    }
}
