using System.Diagnostics.CodeAnalysis;

namespace PageOfBob.Advent2021.App.Days
{
    public static class Day23
    {
        public static void Execute()
        {
            var lines = Utilities.GetEmbeddedData("23").Lines().Select(x => x.Substring(1)).Skip(1).ToArray();
            var amphipods = 
                Utilities.RangeFromTo(0, 10).Select(location => FromChar(location, 0, lines[0][location]))
                .Concat(
                    Enumerable.Range(1, 2).SelectMany(depth =>
                        new[] { 2, 4, 6, 8 }.Select(location => FromChar(location, depth, lines[depth][location]))
                    )
                )
                .WithValue().ToArray();

            var currentState = new State(amphipods);

            // currentState.Print();
            /*
            foreach (var state in currentState.AllPossibleNextStates())
            {
                state.Item1.Print();
                Console.ReadLine();
            }
            // */


            // /*
            var queue = new PriorityQueue<Node, int>();
            var explored = new Dictionary<State, Node>(new StateComparitor());

            var startNode = new Node(currentState, null, 0);
            queue.Enqueue(startNode, 0);

            while (queue.TryDequeue(out Node? node, out int cost))
            {
                var state = node.State;

                if (state.IsSolved())
                {
                    Console.WriteLine($"Cost is: {cost}");

                    /*
                    var list = new List<Node>();
                    list.Add(node);
                    while (node.Via != null)
                    {
                        list.Add(node.Via);
                        node = node.Via;
                    }
                    list.Reverse();

                    foreach (var n in list)
                    {
                        n.State.Print();
                    }
                    */
                    break;
                }

                if (explored.ContainsKey(state))
                    continue;

                var allAdditionalStates = state.AllPossibleNextStates();
                foreach (var additionalState in allAdditionalStates)
                {
                    var (newState, newCost) = additionalState;
                    if (!explored.TryGetValue(newState, out Node? oldNode) || oldNode.Cost > cost + newCost)
                    {
                        var newNode = new Node(newState, node, cost + newCost);
                        queue.Enqueue(newNode, cost + newCost);
                    }
                }

                explored.Add(state, node);
            }
            // */

        }

        // Location: X pos in hallway
        // Depth: 0 = In hallway, > 0 = in room
        // Also got sick of typing "amphipod". These are Pods now.
        public record struct Pod(int Location, int Depth, char Type);

        public static Pod? FromChar(int location, int depth, char c) => c switch
        {
            '.' => (Pod?)null,
            _ => new Pod(location, depth, c)
        };

        public static bool IsInRoom(this Pod pod) => pod.Depth > 0;

        public static int DesiredRoomLocation(this Pod pod) => pod.Type switch
        {
            'A' => 2,
            'B' => 4,
            'C' => 6,
            'D' => 8,
            _ => throw new NotImplementedException()
        };

        public static int Cost(this Pod pod) => pod.Type switch
        {
            'A' => 1,
            'B' => 10,
            'C' => 100,
            'D' => 1000,
            _ => throw new NotImplementedException()
        };


        public record struct State(Pod[] Pods);

        public class StateComparitor : IEqualityComparer<State>
        {
            public bool Equals(State left, State right)
            {
                if (left.Pods.Length != right.Pods.Length)
                    return false;

                for (var i = 0; i < left.Pods.Length; i++)
                {
                    if (left.Pods[i] != right.Pods[i])
                        return false;
                }

                return true;
            }

            public int GetHashCode([DisallowNull] State state)
            {
                int hash = 17;
                for (int i = 0; i < state.Pods.Length; i++)
                {
                    unchecked
                    {
                        hash = hash * 23 + state.Pods[i].GetHashCode();
                    }
                }
                return hash;
            }
        }

        public static void Print(this State state)
        {
            var hallway = new string(Utilities.RangeFromTo(0, 10).Select(x => state.PodCharAt(x, 0)).ToArray());

            // Console.WriteLine($"Current cost: {state.CurrentConst}");
            Console.WriteLine("#############");
            Console.WriteLine($"#{hallway}#");
            Console.WriteLine($"###{state.PodCharAt(2, 1)}#{state.PodCharAt(4, 1)}#{state.PodCharAt(6, 1)}#{state.PodCharAt(8, 1)}###");
            Console.WriteLine($"###{state.PodCharAt(2, 2)}#{state.PodCharAt(4, 2)}#{state.PodCharAt(6, 2)}#{state.PodCharAt(8, 2)}###");
            Console.WriteLine("  #########");
            Console.WriteLine();
        }

        public static State NewState(this State state, Pod newPod, int oldIndex)
        {
            var newPods = (Pod[])state.Pods.Clone();
            newPods[oldIndex] = newPod;
            return new State(newPods);
        }

        public static IEnumerable<(State, int)> AllPossibleNextStates(this State state)
            => state.Pods.SelectMany((pod, i) => state.PossibleMoves(i));

        public static IEnumerable<(State, int)> PossibleMoves(this State state, int podIndex)
        {
            var pod = state.Pods[podIndex];

            // Are we where we need to be? Then no more moves.
            if (state.IsPodAtHome(pod))
                yield break;

            if (pod.IsInRoom())
            {
                // We're in the room, so we have to go into the hallway
                // Can we?
                if (!state.CanPodExitRoom(pod))
                    yield break;

                var costOfExitingTheRoom = pod.Depth * pod.Cost();
                foreach (var (location, cost) in state.AvailableHallwayLocations(pod))
                {
                    yield return (state.NewState(new Pod(location, 0, pod.Type), podIndex), costOfExitingTheRoom + cost);
                }
            }
            else
            {
                // In the hallway, must move to a room.
                if (!state.CanPodEnterRoom(pod))
                    yield break;

                // Otherwise, calculate the cost.
                var roomLocation = pod.DesiredRoomLocation();
                var hallwayCost = Math.Abs(pod.Location - roomLocation) * pod.Cost();
                var roomDepth = Utilities.RangeFromTo(1, 2).Last(l => !state.IsPodAt(roomLocation, l));
                var roomCost = roomDepth * pod.Cost();

                var costOfEnteringTheRoom = hallwayCost + roomCost;

                yield return (state.NewState(new Pod(roomLocation, roomDepth, pod.Type), podIndex), costOfEnteringTheRoom);
            }
        }

        public static Pod? PodAt(this State state, int location, int depth)
            => state.Pods.Cast<Pod?>().FirstOrDefault(pod => pod.HasValue && pod.Value.Location == location && pod.Value.Depth == depth);

        public static bool IsPodAt(this State state, int location, int depth)
            => state.Pods.Any(pod => pod.Location == location && pod.Depth == depth);

        public static char PodCharAt(this State state, int location, int depth)
        {
            var pod = state.PodAt(location, depth);
            if (!pod.HasValue)
                return '.';
            return pod.Value.Type;
        }

        public static Pod?[] PodsInRoom(this State state, int location)
            => new[] 
            {
                state.PodAt(location, 1),
                state.PodAt(location, 2),
            };

        public static bool CanPodExitRoom(this State state, Pod pod)
        {
            if (!pod.IsInRoom())
                return false;

            // At the top? You can exit.
            if (pod.Depth == 1)
                return true;

            // Otherwise, check if there are any other pods in the way.
            var anyBlockers = Utilities.RangeFromTo(pod.Depth - 1, 1).Any(depth => state.IsPodAt(pod.Location, depth));
            return !anyBlockers;
        }

        public static bool CanPodEnterRoom(this State state, Pod pod)
        {
            if (pod.IsInRoom())
                return false;

            // Which room?
            var location = pod.DesiredRoomLocation();

            // Any dissimilar pods in that room?
            var pods = state.PodsInRoom(location);
            var anyNonMatching = pods.Any(x => x.HasValue && x.Value.Type != pod.Type);
            if (anyNonMatching)
                return false;

            // Anyone in the way?
            var anyBlockers = Utilities.RangeFromTo(pod.Location, location)
                .Any(l =>
                {
                    var p = state.PodAt(l, 0);
                    if (!p.HasValue)
                        return false;
                    return p.Value != pod;
                });
            if (anyBlockers)
                return false;

            // Otherwise, looks clear to get there.
            return true;
        }

        public static IEnumerable<(int Location, int Cost)> AvailableHallwayLocations(this State state, Pod pod)
        {
            var goingLeft = pod.Location == 0 ? Enumerable.Empty<int>() : Utilities.RangeFromTo(pod.Location - 1, 0);
            var goingRight = pod.Location == 10 ? Enumerable.Empty<int>() : Utilities.RangeFromTo(pod.Location + 1, 10);

            foreach (var direction in new[] { goingLeft, goingRight })
            {
                int cost = 0;
                foreach (var location in direction)
                {
                    cost += pod.Cost();

                    // If anyone is here, we can't go there and can bail out.
                    if (state.IsPodAt(location, 0))
                        break;

                    // If this is outside a room, not a valid location to stop, so skip it.
                    var isLocationOutsideARoom = location == 2 || location == 4 || location == 6 || location == 8;
                    if (isLocationOutsideARoom)
                        continue;

                    // Otherwise, this is a valid place
                    yield return (location, cost);
                }
            }
        }

        public static bool IsPodAtHome(this State state, Pod pod)
        {
            if (!pod.IsInRoom())
                return false;

            if (pod.Location != pod.DesiredRoomLocation())
                return false;

            var anyNonTypePods = Utilities.RangeFromTo(1, 2)
                .Any(x =>
                {
                    var p = state.PodAt(pod.Location, x);
                    if (!p.HasValue)
                        return false;

                    return p.Value.Type != pod.Type;
                });

            if (anyNonTypePods)
                return false;

            return true;
        }

        public static bool IsSolved(this State state)
            => new[] { ('A', 2), ('B', 4), ('C', 6), ('D', 8) }.All(x =>
            {
                var (type, location) = x;

                return Utilities.RangeFromTo(1, 2).All(depth =>
                {
                    var pod = state.PodAt(location, depth);
                    return pod.HasValue && pod.Value.Type == type;
                });
            });

        public record Node(State State, Node? Via, int Cost);
    }
}
