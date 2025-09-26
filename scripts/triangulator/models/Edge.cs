namespace SeidelTest.triangulator.models
{
    public class Edge
    {
        public Point P { get; set; }
        public Point Q { get; set; }
        public float Slope { get; set; }
        public Polygon? Poly { get; set; } = null;
        public Trapezoid? Below { get; set; } = null;
        public Trapezoid? Above { get; set; } = null;

        public Edge(Point p, Point q)
        {
            P = p;
            Q = q;
            Slope = (q.Y - p.Y) / (q.X - p.X);
        }

        public float Orient(Point p) { return P.Cross(Q, p); }
        public static bool operator > (Edge e, Point p) { return e.Orient(p) < 0.0; }
        public static bool operator < (Edge e, Point p) { return e.Orient(p) > 0.0; }
    }
}
