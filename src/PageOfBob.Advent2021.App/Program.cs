
var stopwatch = new System.Diagnostics.Stopwatch();

stopwatch.Start();
PageOfBob.Advent2021.App.Days.Day25.Execute();

stopwatch.Stop();
Console.WriteLine("Time spent: {0}", stopwatch.Elapsed.TotalMilliseconds);
