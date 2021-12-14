using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PageOfBob.Advent2021.App.Days
{
    public static class Day12
    {
        public static void Execute()
        {
            var lines = Utilities.GetEmbeddedData("12").Lines();
            var system = new CaveSystem();

            foreach (var line in lines)
                system.AddUnparsedCave(line);

            /*
            var allPaths = system.GetValidPaths("start").ToList();
            foreach (var path in allPaths)
                Console.WriteLine(path);
            */
            var count = system.GetValidPaths("start").Count();
            Console.WriteLine(count);
        }

        public class CaveSystem
        {
            public Dictionary<string, Cave> Caves { get; } = new Dictionary<string, Cave>();

            public IEnumerable<string> GetValidPaths(string rootPath)
            {
                var pathsToCheck = new Stack<string>();
                pathsToCheck.Push(rootPath);

                while (pathsToCheck.Count > 0)
                {
                    var currentPath = pathsToCheck.Pop();
                    var currentCave = currentPath.Split(',').Last();
                    if (currentCave == "end")
                    {
                        yield return currentPath;
                    }
                    else
                    {
                        var cave = Caves[currentCave];
                        var visitedCaves = currentPath.Split(',');
                        foreach (var childName in cave.ConnectedCaves)
                        {
                            var childCave = Caves[childName];
                            if (childCave.IsBigCave || !visitedCaves.Contains(childName))
                            {
                                pathsToCheck.Push(currentPath + "," + childName);
                            }
                        }
                    }
                }
            }

            public void AddUnparsedCave(string line)
            {
                var (from, to) = line.Split('-');
                var fromCave = Caves.GetOrCreate(from, Cave.Create);
                var toCave = Caves.GetOrCreate(to, Cave.Create);
                fromCave.ConnectedCaves.Add(to);
                toCave.ConnectedCaves.Add(from);
            }
        }

        public class Cave
        {
            public static Cave Create(string name) => new Cave(name);

            public Cave(string name)
            {
                Name = name;
            }

            public string Name { get; }
            public HashSet<string> ConnectedCaves { get; } = new HashSet<string>();
            public bool IsBigCave => Name.All(c => char.IsUpper(c));
        }
    }
}
