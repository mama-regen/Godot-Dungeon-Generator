namespace SeidelTest.triangulator.models
{
    public class QueryNode
    {
        public Point? Point { get; private set; } = null;
        public Edge? Edge { get; private set; } = null;
        public QueryNode? LeftChild { get; private set; } = null;
        public QueryNode? RightChild { get; private set; } = null;
        public Trapezoid? Trapezoid { get; private set; } = null;

        public QueryNode YNode(Edge edge, QueryNode leftChild, QueryNode rightChild)
        {
            Edge = edge;
            LeftChild = leftChild;
            RightChild = rightChild;
            Trapezoid = null;
            return this;
        }

        public QueryNode XNode(Point point, QueryNode leftChild, QueryNode rightChild)
        {
            Point = point;
            LeftChild = leftChild;
            RightChild = rightChild;
            Trapezoid = null;
            return this;
        }

        public QueryNode Sink(Trapezoid trap)
        {
            Trapezoid = trap;
            Trapezoid.Sink = this;
            return this;
        }

        public static QueryNode GetSink(Trapezoid trap)
        {
            return trap.Sink ?? new QueryNode().Sink(trap);
        }
    }
}
