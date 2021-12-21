using PageOfBob.Parsing;
using static PageOfBob.Parsing.Rules;
using static PageOfBob.Parsing.StringRules;

namespace PageOfBob.Advent2021.App.Days
{
    public static class Day16
    {
        private const string HEX = "0123456789ABCDEF";

        public static void Execute()
        {
            var bits = Utilities.GetEmbeddedData("16").HexToBits().ToList();
            var source = Sources.ListSource(bits);

            /*
            foreach (var bit in bits)
                Console.Write(bit ? "1" : "0");
            Console.WriteLine();
            // */

            var ON = Match(true);
            var OFF = Match(false);
            var BIT = Any(ON, OFF);


            Rule<bool, Packet>? PACKET = null;
            var PACKET_REF = Reference(() => PACKET);

            var PACKETS = PACKET_REF.Many(true);

            var VERSION = BIT.Times(3).AsUlong();

            var LITERAL_TYPE_ID = Sequence(new[] { ON, OFF, OFF });
            var LITERAL_GROUP_WITH_MORE = ON.ThenKeep(BIT.Times(4));
            var LITERAL_GROUP_WITHOUT_MORE = OFF.ThenKeep(BIT.Times(4));
            var LITERAL = LITERAL_TYPE_ID
                .ThenKeep(LITERAL_GROUP_WITH_MORE.Many())
                .Then(LITERAL_GROUP_WITHOUT_MORE, (moreGroups, endGroup)
                    => moreGroups.SelectMany(x => x).Concat(endGroup).ToArray())
                .AsUlong()
                .Map(value => (IPacketValue)new Literal(value));

            // We check for literal first, so any other type should match this.
            var OPERATOR_TYPE_ID = BIT.Times(3).AsUlong();
            var TYPE_0_LENGTH = OFF.ThenKeep(BIT.Times(15)).AsUlong(); // Total bits
            var TYPE_1_LENGTH = ON.ThenKeep(BIT.Times(11)).AsUlong(); // Total packets

            var OPERATOR_TYPE_0 = TYPE_0_LENGTH
                .SubparseByLength(PACKET_REF.Many());

            var OPERATOR_TYPE_1 = TYPE_1_LENGTH
                .SubparseByTimes(PACKET_REF);

            var OPERATOR = OPERATOR_TYPE_ID
                .Then(Any(new [] { OPERATOR_TYPE_0, OPERATOR_TYPE_1 }), (type, subpackets) => (IPacketValue)new Operator(type, subpackets));

            var PACKET_VALUE = Any(new[] { LITERAL, OPERATOR });

            PACKET = VERSION
                .Then(PACKET_VALUE, (ver, val) => new Packet(ver, val));

            var ZERO_PADDING = OFF.Many();

            var ONE_PACKET = PACKET.ThenIgnore(ZERO_PADDING).ThenEnd();

            var packet = ONE_PACKET(source).Match(
                fail => throw new NotImplementedException(fail.Message),
                success => {
                    Console.WriteLine(success.Value.ToString()); 
                    // var versionSum = AddUpPacketVersionNumbers(success.Value);
                    // Console.WriteLine(versionSum);
                    return success.Value; 
                });

            var value = GetPacketValue(packet);
            Console.WriteLine(value);
        }

        public static ulong GetPacketValue(Packet packet)
        {
            return packet.Value switch
            {
                Literal l => (ulong)l.Value,
                Operator op => op.TypeId switch
                {
                    0 => op.SubPackets.Select(GetPacketValue).Cast<ulong>().Aggregate(0ul, (acc, v) => acc + v),
                    1 => op.SubPackets.Select(GetPacketValue).Cast<ulong>().Aggregate(1ul, (acc, v) => acc * v),
                    2 => op.SubPackets.Select(GetPacketValue).Cast<ulong>().Min(),
                    3 => op.SubPackets.Select(GetPacketValue).Cast<ulong>().Max(),
                    5 => GetPacketValue(op.SubPackets[0]) > GetPacketValue(op.SubPackets[1]) ? 1ul : 0,
                    6 => GetPacketValue(op.SubPackets[0]) < GetPacketValue(op.SubPackets[1]) ? 1ul : 0,
                    7 => GetPacketValue(op.SubPackets[0]) == GetPacketValue(op.SubPackets[1]) ? 1ul : 0,
                    _ => throw new NotImplementedException()
                },
                _ => throw new NotImplementedException()
            };
        }

        public static ulong AddUpPacketVersionNumbers(Packet packet)
        {
            var additional = packet.Value switch
            {
                Operator op => op.SubPackets.Select(AddUpPacketVersionNumbers).Aggregate(0ul, (acc, v) => acc + v),
                _ => 0u
            };

            return packet.Version + additional;
        }

        public static Rule<TToken, TOutput> Reference<TToken, TOutput>(Func<Rule<TToken, TOutput>?> getRule)
            => (source) =>
            {
                var rule = getRule.Invoke();
                if (rule == null)
                    throw new NotImplementedException();
                return rule(source);
            };

        public static Rule<TToken, TOutput> SubparseByLength<TToken, TOutput>(this Rule<TToken, ulong> sizeRule, Rule<TToken, TOutput> ruleToExecute)
            => (source) => sizeRule(source).Match(
                fail => fail.Convert<TOutput>(),
                success =>
                {
                    var subRule = Match<TToken>(_ => true).Times((int)success.Value);
                    return subRule(success.Next).Match(
                        fail2 => fail2.Convert<TOutput>(),
                        success2 =>
                        {
                            var subSource = Sources.ListSource(success2.Value.ToList());
                            return ruleToExecute(subSource).Match(
                                fail3 => fail3,
                                success3 => Result.Success(success3.Value, success2.Next));
                        });
                });

        public static Rule<TToken, TOutput[]> SubparseByTimes<TToken, TOutput>(this Rule<TToken, ulong> sizeRule, Rule<TToken, TOutput> ruleToExecute)
            => (source) => sizeRule(source).Match(
                fail => fail.Convert<TOutput[]>(),
                success =>
                {
                    var subRule = ruleToExecute.Times((int)success.Value);
                    return subRule(success.Next);
                });

        public static Rule<bool, ulong> AsUlong(this Rule<bool, bool[]> rule)
            => rule.Map(b => b.ToUlong(b.Length));

        public static Rule<T, K[]> Times<T, K>(this Rule<T, K> rule, int times, string? message = null)
            => source =>
            {
                var list = new List<K>();

                IResult<T, K>? result = null;
                int count = 0;
                do
                {
                    result = rule(source).Match(
                        fail => (IResult<T, K>)fail,
                        success =>
                        {
                            count++;
                            list.Add(success.Value);
                            source = success.Next;
                            return success;
                        });
                } while (count < times && result is Success<T, K>);

                if (list.Count != count)
                    return Result.Fail<T, K[]>(message ?? "Not matched enough times");

                return Result.Success(list.ToArray(), source);
            };

        /*
        private static Rule<bool, int> ToInt(this Rule<bool, bool[]> rule)
            => Rules.Map<bool, bool[], int>((arr) =>
            {

            });
        */

        private static IEnumerable<bool> HexToBits(this string line)
            => line.SelectMany(CharToBits);

        private static IEnumerable<bool> CharToBits(char c)
        {
            var index = HEX.IndexOf(c);
            if (index < 0)
                yield break;

            yield return (index & (1 << 3)) != 0;
            yield return (index & (1 << 2)) != 0;
            yield return (index & (1 << 1)) != 0;
            yield return (index & (1 << 0)) != 0;
        }

        public record Packet(ulong Version, IPacketValue Value)
        {
            public override string ToString()
            {
                return string.Format("Version {0} | {1}", Version, Value.ToString());
            }
        }

        public interface IPacketValue { }

        public record Literal(ulong Value) : IPacketValue
        {
            public override string ToString()
            {
                return string.Format("Literal: {0}", Value);
            }
        }

        public record Operator(ulong TypeId, Packet[] SubPackets) : IPacketValue
        {
            public override string ToString()
            {
                string subTypes = string.Join(", ", SubPackets.Select(x => x.ToString()));
                return string.Format("Operator: {0} ({1})", TypeId, subTypes);
            }

        }

    }
}
