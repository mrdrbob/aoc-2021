using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PageOfBob.Advent2021.App.Days
{
    public static class Day19
    {
        public static void Execute()
        {
            var scanners = Utilities.GetEmbeddedData("19").Split("\n\n")
                .Select(scannerRaw =>
                {
                    var lines = scannerRaw.Lines();
                    // --- scanner 0 ---
                    var firstLine = lines.First();
                    var id = int.Parse(firstLine.Substring("--- scanner ".Length, firstLine.Length - "--- scanner  ---".Length));

                    var points = lines.Skip(1).Select(line => new Beacon(line.Split(',').Select(int.Parse).ToArray())).ToHashSet();
                    return new Scanner(id, points);
                }).ToList();


            var solved = scanners.First().Beacons.ToHashSet();
            var unsolved = scanners.Skip(1).ToList();

            // For part 2, we also keep track of scanner positions.
            // Using a list of "beacons", but really, we're just using them
            // as points.
            var knownScanners = new List<Beacon>();
            // The first scanner is always at the origin, as this scanner is the basis for
            // our global solved space.
            knownScanners.Add(new Beacon(0, 0, 0));

            while (unsolved.Any())
            {
                Console.WriteLine($"Solved beacons: {solved.Count}, Unsolved scanners: {unsolved.Count}");

                // Create a list of all unsolved scanners and possible orientations for each scanner.
                // Mostly doing this to cut down on nested for loops, and also making this list allows me
                // to iterate it while modifying the `unsolved` list.
                var toCheck = AllOrientations().SelectMany(orientation => unsolved.Select(scanner => (Orientation: orientation, Scanner: scanner))).ToList();

                // Check each unsolved scanner in all the possible orientations
                foreach (var (orientation, scanner) in toCheck)
                {
                    // Orient the beacons
                    var orientedBeacons = scanner.GetOrientedBeacons(orientation);

                    // Now calculate all the deltas between all the unsolved beacons and the known beacons.
                    // Count how frequently each delta comes up. If any delta comes up more than 12 
                    // times, that means we're likely oriented correctly, and that delta is likely the correct offset.
                    var distances = CountMatchingDeltas(orientedBeacons, solved)
                        .Where(x => x.Value >= 12)
                        .Select(x => x.Key)
                        .ToList();

                    // Loop through any deltas that occured 12 or more times and try them all.
                    foreach (var offset in distances)
                    {
                        // Translate the oriented beacons with this offset and see how many match
                        // beacons in the solved space.
                        var translatedBeacons = orientedBeacons.GetTranslatedBeacons(offset).ToHashSet();

                        var matched = solved.Intersect(translatedBeacons).Count();
                        if (matched >= 12)
                        {
                            // A match! We can remove this scanner from the list.
                            // Then move all the oriented/translated beacons into the solved set.
                            unsolved.Remove(scanner);
                            foreach (var translated in translatedBeacons)
                                solved.Add(translated);

                            // Translate the scanner into the appropriate place. We don't care about
                            // orientation here, so just use the offset. Save it in the list.
                            var translatedScannerPosition = new Beacon(0, 0, 0).Translate(offset);
                            knownScanners.Add(translatedScannerPosition);
                        }
                    }
                }
            }

            Console.WriteLine($"Solved beacons: {solved.Count}, Unsolved scanners: {unsolved.Count}");

            // Now we just compare all the scanners distances to each other and find the largest one.
            int? largestDistance = null;
            for (int x = 0; x < knownScanners.Count; x++)
            {
                // var y = x + 1 so we're not doing the same comparisons twice.
                for (var y = x + 1; y < knownScanners.Count; y++)
                {
                    var distance = knownScanners[x].Delta(knownScanners[y]).AsManhattanDistance();
                    if (!largestDistance.HasValue || distance > largestDistance.Value)
                        largestDistance = distance;
                }
            }

            // Or you could do this.
            // var maxDistance = Enumerable.Range(0, knownScanners.Count)
            //    .SelectMany(x => Enumerable.Range(x + 1, knownScanners.Count - x - 1)
            //        .Select(y => knownScanners[x].Delta(knownScanners[y]).AsManhatanDistance())
            //    ).Max();

            Console.WriteLine($"Largest distance between scanners is: {largestDistance} ");
        }

        public static Dictionary<Beacon, int> CountMatchingDeltas(IEnumerable<Beacon> one, IEnumerable<Beacon> two)
        {
            var output = new Dictionary<Beacon, int>();

            foreach (var beacon in one)
            {
                foreach (var other in two)
                {
                    var delta = other.Delta(beacon);
                    output.Plus(delta);
                }
            }

            return output;
        }

        public static IEnumerable<Beacon> GetOrientedBeacons(this Scanner scanner, Orientation orientation)
            => scanner.Beacons.Select(b => b.Orient(orientation));

        public static IEnumerable<Beacon> GetTranslatedBeacons(this IEnumerable<Beacon> beacons, Beacon offset)
            => beacons.Select(b => b.Translate(offset));


        public record Scanner(int Id, ISet<Beacon> Beacons);

        public record struct Beacon(int X, int Y, int Z)
        {
            public Beacon(int[] array) : this(array[0], array[1], array[2]) { }
        }

        public static int Distance(this Beacon pt, Beacon pt2)
            => Math.Abs(pt.X - pt2.X) + Math.Abs(pt.Y - pt2.Y) + Math.Abs(pt.Z - pt2.Z);

        public static IEnumerable<Orientation> AllOrientations()
            => Enumerable.Range(0, 6).SelectMany(direction => Enumerable.Range(0, 4).Select(rotation => new Orientation(direction, rotation)));

        public record struct Orientation(int Direction, int Rotation);

        public static Beacon Orient(this Beacon beacon, Orientation orientation)
            => beacon.OrientDirection(orientation.Direction)
                .Rotate(orientation.Rotation);

        public static Beacon OrientDirection(this Beacon beacon, int direction)
            => direction switch
            {
                0 => beacon,
                1 => new Beacon(-beacon.Z, beacon.Y, beacon.X),
                2 => new Beacon(-beacon.Z, -beacon.X, beacon.Y),
                3 => new Beacon(beacon.Z, beacon.Y, -beacon.X),
                4 => new Beacon(beacon.X, beacon.Z, -beacon.Y),
                5 => new Beacon(beacon.X, -beacon.Y, -beacon.Z),
                _ => throw new NotImplementedException()
            };

        public static Beacon Rotate(this Beacon beacon, int rotation)
            => rotation switch
            {
                0 => beacon,
                1 => new Beacon(beacon.Y, -beacon.X, beacon.Z),
                2 => new Beacon(-beacon.X, -beacon.Y, beacon.Z),
                3 => new Beacon(-beacon.Y, beacon.X, beacon.Z),
                _ => throw new NotImplementedException()
            };

        public static Beacon Delta(this Beacon one, Beacon two)
            => new Beacon(one.X - two.X, one.Y - two.Y, one.Z - two.Z);

        public static Beacon Translate(this Beacon one, Beacon two)
            => new Beacon(one.X + two.X, one.Y + two.Y, one.Z + two.Z);

        public static int AsManhattanDistance(this Beacon beacon)
            => Math.Abs(beacon.X) + Math.Abs(beacon.Y) + Math.Abs(beacon.Z);
    }
}
