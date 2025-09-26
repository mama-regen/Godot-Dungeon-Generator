namespace SeidelTest.triangulator.models
{
    public class Triangle
    {
        private Point _P;
        public int P { get { return _P.Id; } }
        private Point _Q;
        public int Q { get { return _Q.Id; } }
        private Point _R;
        public int R { get { return _R.Id; } }

        public Triangle(Point p, Point q, Point r)
        {
            _P = p;
            _Q = q;
            _R = r;
        }
    }
}
