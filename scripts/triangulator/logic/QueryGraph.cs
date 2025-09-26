using SeidelTest.triangulator.models;
using Point = SeidelTest.triangulator.models.Point;

namespace SeidelTest.triangulator.logic
{
    public class QueryGraph
    {
        private QueryNode Head;

        public QueryGraph(Trapezoid head)
        {
            Head = QueryNode.GetSink(head);
        }

        public Trapezoid? Locate(Point point, float slope)
        {
            var node = Head;

            while (node != null)
            {
                if (node.Trapezoid != null) return node.Trapezoid;
                if (!(node.Point is null)) node = (point.X >= node.Point.X) ? node.RightChild : node.LeftChild;
                else if (node.Edge != null)
                {
                    var orient = node.Edge.Orient(point);
                    if (orient != 0) node = (orient < 0) ? node.RightChild : node.LeftChild;
                    else node = (slope < node.Edge.Slope) ? node.RightChild : node.LeftChild;
                }
            }

            return null;
        }

        public void Case1(QueryNode sink, Edge edge, Trapezoid trap1, Trapezoid trap2, Trapezoid trap3, Trapezoid trap4)
        {
            var yNode = new QueryNode().YNode(edge, QueryNode.GetSink(trap2), QueryNode.GetSink(trap3));
            var xNode = new QueryNode().XNode(edge.Q, yNode, QueryNode.GetSink(trap4));
            sink.XNode(edge.P, QueryNode.GetSink(trap1), xNode);
        }

        public void Case2(QueryNode sink, Edge edge, Trapezoid trap1, Trapezoid trap2, Trapezoid trap3)
        {
            var yNode = new QueryNode().YNode(edge, QueryNode.GetSink(trap2), QueryNode.GetSink(trap3));
            sink.XNode(edge.P, QueryNode.GetSink(trap1), yNode);
        }

        public void Case3(QueryNode sink, Edge edge, Trapezoid trap1, Trapezoid trap2)
        {
            sink.YNode(edge, QueryNode.GetSink(trap1), QueryNode.GetSink(trap2));
        }

        public void Case4(QueryNode sink, Edge edge, Trapezoid trap1, Trapezoid trap2, Trapezoid trap3)
        {
            var yNode = new QueryNode().YNode(edge, QueryNode.GetSink(trap1), QueryNode.GetSink(trap2));
            sink.XNode(edge.Q, yNode, QueryNode.GetSink(trap3));
        }
    }
}
