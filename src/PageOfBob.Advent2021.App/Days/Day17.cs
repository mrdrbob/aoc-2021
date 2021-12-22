using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PageOfBob.Advent2021.App.Days
{
    public static class Day17
    {

        // /*
        public static void Execute()
        {
            // target area: x=211..232, y=-124..-69
            // You know what? I'm going to be lazy and not parse this.
            var target = new TargetArea(new Range(211, 232), new Range(-124, -69));
            // Example data
            // var target = new TargetArea(new Range(20, 30), new Range(-10, -5));

            // bool t = Simulate2(target, new Vector(0, 7), new Vector(0, -1));

            var matching = FindAllTargetVelocities(target);
            /*
            foreach (var match in matching)
                Console.WriteLine("{0},{1}", match.X, match.Y);
            */

            Console.WriteLine(matching.Count());
        }
        // */

        public static IEnumerable<(int X, int Y)> FindAllTargetVelocities(TargetArea target)
        {
            for (var xVel = 1; xVel <= target.X.End; xVel++)
            {
                for (var yVel = target.Y.Start * 5; yVel <= -target.Y.Start; yVel++)
                {
                    var result = Simulate2(target, new Vector(0, xVel), new Vector(0, yVel));
                    if (result)
                        yield return (xVel, yVel);
                }
            }
        }

        public static bool Simulate2(TargetArea target, Vector x, Vector y)
        {
            while (true)
            {
                // Console.WriteLine("{0}, {1}", x.Position, y.Position);

                bool isInTarget = target.IsWithin(x, y);
                if (isInTarget)
                    return true;

                bool overShot = target.Overshot(x, y);
                if (overShot)
                    return false;

                x = x.Drag();
                y = y.Gravity(-1);
            }
        }

        /* Part 1
        public static void Execute()
        {
            // target area: x=211..232, y=-124..-69
            // You know what? I'm going to be lazy and not parse this.
            var target = new TargetArea(new Range(211, 232), new Range(-124, -69));
            // Example data
            // var target = new TargetArea(new Range(20, 30), new Range(-10, -5));

            // We only care about Y, so let's just do Y
            target = target with { X = new Range(-10, 10) };
            var xVector = new Vector(0, 0);
            var yVector = new Vector(0, 1);

            int? result = null;
            while (result == null)
            {
                result = Simulate(target, xVector, yVector);
                yVector = yVector with { Velocity = yVector.Velocity + 1 };
            }

            int? lastHighest = result;
            while (result != null)
            {
                lastHighest = result;
                result = Simulate(target, xVector, yVector);
                yVector = yVector with { Velocity = yVector.Velocity + 1 };
            }

            // Console.WriteLine(lastHighest);

            int attemptsRemaining = yVector.Velocity * 5; // So lame
            while (attemptsRemaining > 0)
            {
                result = Simulate(target, xVector, yVector);
                yVector = yVector with { Velocity = yVector.Velocity + 1 };
                attemptsRemaining--;

                if (result.HasValue && result.Value > lastHighest)
                    lastHighest = result.Value;
            }


            Console.WriteLine(lastHighest);
        }
        // */

        public static int? Simulate(TargetArea target, Vector x, Vector y)
        {
            int maxY = 0;

            while (true)
            {
                // Console.WriteLine("{0}, {1}", x.Position, y.Position);

                bool isInTarget = target.IsWithin(x, y);
                if (isInTarget)
                {
                    Console.WriteLine(y.Velocity);
                    return maxY;
                }

                bool overShot = target.Overshot(x, y);
                if (overShot)
                    return null;

                x = x.Drag();
                y = y.Gravity(-1);

                if (y.Position > maxY)
                    maxY = y.Position;
            }
        }
        
        public record struct Vector(int Position, int Velocity)
        {
            public Vector Gravity(int gravity)
            {
                return new Vector(Position + Velocity, Velocity + gravity);
            }

            public Vector Drag()
            {
                if (this.Velocity == 0)
                    return this;

                return new Vector(Position + Velocity, Velocity + (Velocity > 0 ? - 1: 1));
            }
        }

        public record struct Range(int Start, int End)
        {
            public bool IsWithin(int position) => position >= Start && position <= End;
        }

        public record struct TargetArea(Range X, Range Y)
        {
            public bool IsWithin(int x, int y) => X.IsWithin(x) && Y.IsWithin(y);
            public bool IsWithin(Vector x, Vector y) => IsWithin(x.Position, y.Position);
            public bool Overshot(Vector x, Vector y)
            {
                if (y.Velocity < 0 && y.Position < Y.Start)
                    return true;

                if (x.Velocity < 0 && x.Position < X.End)
                    return true;

                if (x.Velocity > 0 && x.Position > X.End)
                    return true;

                return false;
            }
        }
    }
}
