namespace SeidelTest.triangulator.models
{
    public class Point
    {
        public int Id { get; private set; }
        public float X { get; private set; }
        public float Y { get; private set; }

        public Point(int id, float x, float y)
        {
            Id = id;
            X = x;
            Y = y;
        }

        public float Cross(Point a, Point b)
        {
            return (X - b.X) * (a.Y - b.Y) - (Y - b.Y) * (a.X - b.X);
        }

        public static bool operator == (Point left, Point right)
        {
            return left.Equals(right);
        }

        public static bool operator != (Point left, Point right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            var xP = obj.GetType().GetProperty("X");
            if (xP == null) return false;
            var yP = obj.GetType().GetProperty("Y");
            if (yP == null) return false;
            var xV = xP.GetValue(obj);
            if (xV == null ) return false;
            var yV = yP.GetValue(obj);
            if (yV == null) return false;
            return X == (float)xV && Y == (float)yV;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
