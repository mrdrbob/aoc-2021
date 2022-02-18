using PageOfBob.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PageOfBob.Parsing.Rules;
using static PageOfBob.Parsing.StringRules;

namespace PageOfBob.Advent2021.App.Days
{
    public static class Day24
    {
        public static void Execute()
        {
            // Parser
            var DIGIT = Match<char>(char.IsDigit);
            var MINUS = Match('-');
            var SPACE = Match(' ');
            var WS = Match<char>(char.IsWhiteSpace);

            var INT_VALUE = MINUS.Map(x => true).Optional(false)
                .Then(DIGIT.ManyAsString(true), (n, digit) => int.Parse(digit) * (n ? -1 : 1))
                .Map(x => new IntValue(x));

            var VAR = Any(Match('w'), Match('x'), Match('y'), Match('z')).Map(x => new Variable(x));
            var VALUE = Any(
                INT_VALUE.Map(x => (IValue)x),
                VAR.Map(x => (IValue)x)
            );

            var INP = Text("inp ").ThenKeep(VAR).Map(x => new Inp(x));
            var ADD = Text("add ").ThenKeep(VAR)
                .ThenIgnore(SPACE).Then(VALUE, (var, val) => new Add(var, val));
            var MUL = Text("mul ").ThenKeep(VAR)
                .ThenIgnore(SPACE).Then(VALUE, (var, val) => new Mul(var, val));
            var DIV = Text("div ").ThenKeep(VAR)
                .ThenIgnore(SPACE).Then(VALUE, (var, val) => new Div(var, val));
            var MOD = Text("mod ").ThenKeep(VAR)
                .ThenIgnore(SPACE).Then(VALUE, (var, val) => new Mod(var, val));
            var EQL = Text("eql ").ThenKeep(VAR)
                .ThenIgnore(SPACE).Then(VALUE, (var, val) => new Eql(var, val));

            var INSTRUCTION = Any(
                INP.Map(x => (IInstruction)x),
                ADD.Map(x => (IInstruction)x),
                MUL.Map(x => (IInstruction)x),
                DIV.Map(x => (IInstruction)x),
                MOD.Map(x => (IInstruction)x),
                EQL.Map(x => (IInstruction)x)
            );

            var LINE = INSTRUCTION.ThenIgnore(WS);

            var source = Sources.CharSource(Utilities.GetEmbeddedData("24"));
            var instructions = LINE.Tokenize(source, true).ToList();

            // Group instructions into sets of 18.
            var groups = instructions.Select((x, i) => (Value: x, Index: i)).GroupBy(t => t.Index / 18)
                .Select(x => x.Select(x => x.Value).ToArray())
                .ToList();

            // These groups create one of two possible "processor" units.
            var processors = groups.Select(group =>
            {
                var asSimple = SimpleProcessor.FromInstructions(group);
                if (asSimple.HasValue)
                    return (IProcessor)asSimple.Value;

                var asComplex = ComplexProcessor.FromInstructions(group);
                if (asComplex.HasValue)
                    return asComplex.Value;

                throw new NotImplementedException();
            }).ToList();

            /* Part 1
            var allPossibleNumbers = Utilities.RangeFromTo(9999999, 1111111)
                .Select(x => ToNumbers(x, 7).ToList())
                .Where(x => !x.Contains(0));
            */

            // Part 2
            var allPossibleNumbers = Utilities.RangeFromTo(1111111, 9999999)
                .Select(x => ToNumbers(x, 7).ToList())
                .Where(x => !x.Contains(0));

            foreach (var possibleNumber in allPossibleNumbers)
            {
                var result = TestNumber(possibleNumber, processors);
                if (result != null)
                {
                    Console.WriteLine(result);
                    break;
                }
            }
        }

        private static string? TestNumber(List<int> possibleNumber, List<IProcessor> processors)
        {
            // Z is the only value that carries over from processor to processor.
            int z = 0;
            var outputNumber = new List<int>();
            int digit = 0;

            for (int i = 0; i < 14; i++)
            {
                var processor = processors[i];
                switch (processor)
                {
                    case SimpleProcessor simple:
                        // Simple processors just consume a digit and calculate a new Z value.
                        var input = possibleNumber[digit++];
                        z = simple.Next(z, input);
                        outputNumber.Add(input);
                        break;
                    case ComplexProcessor complex:
                        // For a model to match, instruction 7 of this processor must be true (otherwise Z gets too high)
                        // We can calculate what input digit would make this true (if any).
                        var output = complex.Next(z);
                        if (!output.HasValue) // No valid input found
                        {
                            return null;
                        }

                        // Otherwise, add this found digit to the output number and calculate the new Z value.
                        var (newZ, newDigit) = output.Value;
                        z = newZ;
                        outputNumber.Add(newDigit);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            if (z == 0)
                return string.Join("", outputNumber.Select(x => x.ToString()));
            else
                return null;
        }

        public static IEnumerable<int> ToNumbers(int number, int digits)
        {
            while (digits > 0)
            {
                digits--;
                
                var n = (digits < 0) ? number : number / (int)Math.Pow(10, digits);
                yield return (n % 10);
            }
        }

        #region Higher order processors
        public interface IProcessor { }
        public record struct SimpleProcessor(int Addition) : IProcessor
        {
            public static SimpleProcessor? FromInstructions(IInstruction[] instructions)
            {
                var div = instructions[4];
                if (!(div is Div divIn) || !(divIn.Value is IntValue divVal) || divVal.Value != 1)
                    return null;
                var add = instructions[15];
                if (!(add is Add addIn) || addIn.Variable.Name != 'y' || !(addIn.Value is IntValue addValue))
                    return null;
                return new SimpleProcessor(addValue.Value);
            }

            public int Next(int z, int digit) => (z * 26) + digit + Addition;
        }

        public record struct ComplexProcessor(int Addition) : IProcessor
        {
            public static ComplexProcessor? FromInstructions(IInstruction[] instructions)
            {
                var div = instructions[4];
                if (!(div is Div divIn) || !(divIn.Value is IntValue divVal) || divVal.Value != 26)
                    return null;
                var add = instructions[5];
                if (!(add is Add addIn) || addIn.Variable.Name != 'x' || !(addIn.Value is IntValue addValue))
                    return null;
                return new ComplexProcessor(addValue.Value);
            }

            // Calculates what input value could be valid, and the resulting z value
            public (int Z, int Input)? Next(int z)
            {
                var expectedInput = (z % 26) + Addition;
                if (expectedInput < 1 || expectedInput > 9)
                    return null;
                var nextZ = z / 26;
                return (nextZ, expectedInput);
            }
        }
        #endregion

        #region Input
        public interface IValue { }
        public record Variable(char Name) : IValue;
        public record IntValue(int Value) : IValue;

        public interface IInstruction
        {
            Variable Variable { get; }
        }
        public record Inp(Variable Variable) : IInstruction;


        public interface IOperation : IInstruction
        {
            IValue Value { get; }
        }
        public record Add(Variable Variable, IValue Value) : IOperation;
        public record Mul(Variable Variable, IValue Value) : IOperation;
        public record Div(Variable Variable, IValue Value) : IOperation;
        public record Mod(Variable Variable, IValue Value) : IOperation;
        public record Eql(Variable Variable, IValue Value) : IOperation;
        #endregion
    }
}
