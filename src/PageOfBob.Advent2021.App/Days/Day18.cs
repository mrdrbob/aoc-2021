using PageOfBob.Parsing;
using static PageOfBob.Parsing.Rules;

namespace PageOfBob.Advent2021.App.Days
{
    public static class Day18
    {
        private static Rule<char, Node>? ELEMENT = null;

        static Day18()
        {
            // Parser
            var LBRACKET = Match('[');
            var RBRACKET = Match(']');
            var COMMA = Match(',');


            var DEF_ELEMENT = Day16.Reference(() => ELEMENT);

            var VALUE_ELEMENT = Match<char>(char.IsDigit).Map(x => Node.ValueNode(int.Parse(x.ToString())));
            var PAIR_ELEMENT = LBRACKET
                .ThenKeep(DEF_ELEMENT)
                .ThenIgnore(COMMA)
                .Then(DEF_ELEMENT, (left, right) => Node.PairNode(left, right))
                .ThenIgnore(RBRACKET);

            ELEMENT = Any(VALUE_ELEMENT, PAIR_ELEMENT);
        }

        public static void Execute()
        {

            // Parse the input
            var input = Utilities.GetEmbeddedData("18").Lines();
            var parsedInput = input.Select(Parse).ToList();


            /* Part 1
            // Now process
            var root = parsedInput.First();
            foreach (var node in parsedInput.Skip(1))
            {
                var newNode = root.Add(node);
                newNode.FullyReduce();
                root = newNode;
            }

            Console.WriteLine(root);
            Console.WriteLine(root.Magnitude());
            // */

            ulong? maxMagnitude = null;
            for (var x = 0; x < parsedInput.Count; x++)
            {
                for (var y = 0; y < parsedInput.Count; y++)
                {
                    if (x == y)
                        continue;

                    var node = parsedInput[x].Add(parsedInput[y]).Clone();
                    node.FullyReduce();
                    var magnitude = node.Magnitude();
                    if (!maxMagnitude.HasValue || magnitude > maxMagnitude.Value)
                        maxMagnitude = magnitude;
                }
            }

            Console.WriteLine(maxMagnitude);

        }

        private static Node Clone(this Node node)
            => node.Match(
                v => Node.ValueNode(v),
                (left, right) => Node.PairNode(left.Clone(), right.Clone()));

        private static ulong Magnitude(this Node node)
            => node.Match(
                v => (ulong)v,
                (left, right) => (3ul * left.Magnitude()) + (2ul * right.Magnitude()));

        private static void FullyReduce(this Node root)
        {
            while (Reduce(root)) {
                // Console.WriteLine(root);
            }
        }

        private static bool Reduce(this Node root)
        {
            if (ExplodePairs(root))
                return true;

            if (SplitValues(root))
                return true;

            return false;
        }

        private static bool SplitValues(this Node root)
        {
            var tooLarge = root.FindNode(x => x >= 10);
            if (tooLarge == null)
                return false;

            var currentValue = tooLarge.AssumeValue();
            var leftValue = (int)Math.Floor(currentValue / 2f);
            var rightValue = (int)Math.Ceiling(currentValue / 2f);

            tooLarge.ConvertToPairs(Node.ValueNode(leftValue), Node.ValueNode(rightValue));

            return true;
        }

        private static bool ExplodePairs(this Node root)
        {
            var tooHigh = root.FindDepth(4)?.ToList();
            if (tooHigh == null)
                return false;

            var (node, _) = tooHigh.First();
            var (leftNode, rightNode) = node.AssumePair();

            var (left, _) = tooHigh.FirstOrDefault(x => x.side == Side.Right);
            if (left != null)
            {
                var closestLeftNode = left.AssumePair().left.ClimbRight();
                if (closestLeftNode != null)
                {
                    var newValue = closestLeftNode.AssumeValue() + leftNode.AssumeValue();
                    closestLeftNode.ConvertToValue(newValue);
                }
            }

            var (right, _) = tooHigh.FirstOrDefault(y => y.side == Side.Left);
            if (right != null)
            {
                var closestRightNode = right.AssumePair().right.ClimbLeft();
                if (closestRightNode != null)
                {
                    var newValue = closestRightNode.AssumeValue() + rightNode.AssumeValue();
                    closestRightNode.ConvertToValue(newValue);
                }
            }

            node.ConvertToValue(0);
            return true;
        }

        private static Node ClimbRight(this Node node) => node.Match(_ => node, (_, right) => right.ClimbRight());
        private static Node ClimbLeft(this Node node) => node.Match(_ => node, (left, right) => left.ClimbLeft());

        private static IEnumerable<(Node node, Side side)>? FindDepth(this Node node, int depth)
            => node.Match(
                v => null,
                (left, right) =>
                {
                    if (depth == 0)
                    {
                        return new[] { (node, Side.None) };
                    }

                    var leftResult = FindDepth(left, depth - 1);
                    if (leftResult != null)
                        return leftResult.Append((node, Side.Left));
                    var rightResult = FindDepth(right, depth - 1);
                    if (rightResult != null)
                        return rightResult.Append((node, Side.Right));
                    return null;
                });

        private static Node? FindNode(this Node node, Func<int, bool> match)
            => node.Match(
                v => match(v) ? node : null,
                (left, right) =>
                {
                    var leftFound = FindNode(left, match);
                    if (leftFound != null)
                        return leftFound;
                    
                    var rightFound = FindNode(right, match);
                    if (rightFound != null)
                        return rightFound;
                    
                    return null;
                });

        public static (Node left, Node right) AssumePair(this Node node)
            => node.Match(
                v => throw new NotImplementedException(),
                (l, r) => (left: l, right: r));

        public static int AssumeValue(this Node node)
            => node.Match(
                v => v,
                (_, _) => throw new NotImplementedException());

        public enum Side
        {
            None,
            Left,
            Right,
        }

        private static Node Add(this Node left, Node right)
            => Node.PairNode(left, right);

        private static Node Parse(string value)
        {
            if (ELEMENT == null)
                throw new InvalidOperationException();

            var source = Sources.CharSource(value);
            return ELEMENT.Invoke(source).Match(
                f => throw new NotImplementedException(),
                success => success.Value);
        }

        public class Node
        {
            private Node(int? value, Node? left, Node? right)
            {
                Value = value;
                Left = left;
                Right = right;
            }

            public static Node ValueNode(int value) => new Node(value, null, null);
            public static Node PairNode(Node left, Node right) => new Node(null, left, right);

            public int? Value { get; set; }
            public Node? Left { get; set; }
            public Node? Right { get; set; }

            public T Match<T>(Func<int, T> matchValue, Func<Node, Node, T> matchPair)
            {
                if (Value.HasValue)
                    return matchValue(Value.Value);
                else if (Left != null && Right != null)
                    return matchPair(Left, Right);
                else
                    throw new NotImplementedException();
            }

            public void ConvertToValue(int value)
            {
                Value = value;
                Left = null;
                Right = null;
            }

            public void ConvertToPairs(Node left, Node right)
            {
                Value = null;
                Left = left;
                Right = right;
            }

            public override string ToString()
                => Match(x => x.ToString(), (left, right) => $"[{left},{right}]");
        }
    }
}
