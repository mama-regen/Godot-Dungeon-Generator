namespace SeidelTest.triangulator.models
{
    public class PointNode
    {
        public Point Point { get; set; }
        public PointNode? Next { get; set; } = null;
        public PointNode? Prev { get; set; } = null;
        public bool IsEar { get; set; } = false;

        public PointNode(Point point) { Point = point; }
    }
}
