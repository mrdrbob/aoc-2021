using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PageOfBob.Advent2021.App.Days
{
    public static class Day10
    {
        public static Dictionary<char, char> Brackets = new Dictionary<char, char>()
        {
            { '(', ')' },
            { '[', ']' },
            { '{', '}' },
            { '<', '>' },
        };

        public static Dictionary<char, int> Scores = new Dictionary<char, int>()
        {
            { ')', 3 },
            { ']', 57 },
            { '}', 1197 },
            { '>', 25137 },
        };

        public static Dictionary<char, int> Scores2 = new Dictionary<char, int>()
        {
            { ')', 1 },
            { ']', 2 },
            { '}', 3 },
            { '>', 4 },
        };

        public static void Execute()
        {
            /* Part 1
            var totalScore = Utilities.GetEmbeddedData("10").Lines().Select(FindIllegalCharacter).Select(x => x.Score).Sum();
            Console.WriteLine(totalScore);
            */

            // Part 2
            var scores = Utilities.GetEmbeddedData("10").Lines().Select(CalculatePart2Score).Where(x => x.HasValue).Select(x => x.Value).ToList();
            scores.Sort();
            Console.WriteLine(scores.Count);
            scores.ForEach(Console.WriteLine);
            var middle = scores[scores.Count / 2];
            Console.WriteLine(middle);
        }

        public static ulong? CalculatePart2Score(string line)
        {
            var (part1Score, remainingStack) = FindIllegalCharacter(line);
            if (part1Score > 0)
                return null;

            ulong score = 0;
            while (remainingStack.Count > 0)
            {
                var closing = Brackets[remainingStack.Pop()];
                var value = (ulong)Scores2[closing];
                score = (score * 5) + value;
                Console.Write(closing);
            }
            Console.WriteLine(" {0}", score);
            return score;
        }

        public static (int Score, Stack<char> Remaining) FindIllegalCharacter(string line)
        {
            var stack = new Stack<char>();
            foreach (char c in line)
            {
                if (Brackets.ContainsKey(c)) // It's an opening, push it on the stack
                {
                    stack.Push(c);
                }
                else if (stack.Count == 0)
                {
                    return (0, stack);
                }
                else
                {
                    var expectedOpen = stack.Pop();
                    var expectedClose = Brackets[expectedOpen];
                    if (c != expectedClose) {
                        var score = Scores[c];
                        // Console.WriteLine("{0} - Expected {1}, found {2} | {3}", line, expectedClose, c, score);
                        return (score, stack);
                    }
                }
            }

            return (0, stack);
        }
    }
}
