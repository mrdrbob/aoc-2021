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

        public static void Execute()
        {
            var totalScore = Utilities.GetEmbeddedData("10").Lines().Select(FindIllegalCharacter).Sum();
            Console.WriteLine(totalScore);
        }

        public static int FindIllegalCharacter(string line)
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
                    return 0;
                }
                else
                {
                    var expectedOpen = stack.Pop();
                    var expectedClose = Brackets[expectedOpen];
                    if (c != expectedClose) {
                        var score = Scores[c];
                        // Console.WriteLine("{0} - Expected {1}, found {2} | {3}", line, expectedClose, c, score);
                        return score;
                    }
                }
            }

            return 0;
        }
    }
}
