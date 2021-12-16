namespace PageOfBob.Advent2021.App.Days
{
    public static class Day13
    {
        public static void Execute()
        {
            var (pointsUnparsed, foldsUnparsed) = Utilities.GetEmbeddedData("13").Split("\n\n");
            var points = pointsUnparsed.Lines().Select(Position.FromString).ToList();

            var folds = foldsUnparsed.Lines().Select(Fold.FromSting).ToList();
            
            var width = points.Max(x => x.X) + 1;
            var height = points.Max(y => y.Y) + 1;
            var map = Map.Empty<bool>(width, height);
            foreach (var point in points)
                map.Set(point, true);

            foreach (var fold in folds)
                map = map.FoldMap(fold);

            map.PrintMap();
        }

        public static void PrintMap(this Map<bool> map)
            => map.Print(x => x ? "#" : ".");

        public enum Orientation
        {
            Horizontal,
            Vertical
        }

        public record Fold(Orientation Orientation, int Position)
        {
            // 012345678901
            // fold along x=655
            public static Fold FromSting(string raw)
            {
                var (or, pos)  = raw.Substring(11).Split('=');
                var orientation = or == "x" ? Orientation.Vertical : Orientation.Horizontal;
                return new Fold(orientation, int.Parse(pos));
            }
        }

        public static Map<bool> FoldMap(this Map<bool> map, Fold fold)
        {
            switch (fold.Orientation)
            {
                case Orientation.Horizontal: return map.FoldHorizontal(fold.Position, (source, dest) => source || dest);
                case Orientation.Vertical: return map.FoldVertical(fold.Position, (source, dest) => source || dest);
                default: throw new NotImplementedException();
            }
        }

        public static Map<bool> ShrinkMap(this Map<bool> map)
        {
            var maxX = map.GetAllPositions().Where(pos => map.Get(pos)).Max(x => x.X);
            var maxY = map.GetAllPositions().Where(pos => map.Get(pos)).Max(x => x.Y);
            var newMap = Map.Empty<bool>(maxX + 1, maxY + 1);
            foreach (var pos in newMap.GetAllPositions())
                newMap.Set(pos, map.Get(pos));
            return newMap;
        }
    }

}
