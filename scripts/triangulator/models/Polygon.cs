namespace SeidelTest.triangulator.models
{
    public class Polygon
    {
        public int Length { get; private set; } = 0;
        public PointNode? First { get; private set; } = null;
        public PointNode? Last { get; private set; } = null;

        public void Add(Point p)
        {
            var node = new PointNode(p);

            if (Length == 0)
            {
                First = node;
                Last = node;
            } else
            {
                Last!.Next = node;
                node.Prev = Last;
                Last = node;
            }

            Length++;
        }

        public void Remove(PointNode node)
        {
            if (Length == 0) return;

            if (node == First)
            {
                First = First.Next;
                if (First == null) Last = null;
                else First.Prev = null;
            } else if (node == Last)
            {
                Last = Last.Prev;
                Last!.Next = null;
            } else
            {
                node.Prev!.Next = node.Next;
                node.Next!.Prev = node.Prev;
            }

            node.Prev = null;
            node.Next = null;

            Length--;
        }

        public void InsertBefore(Point p, PointNode node)
        {
            var pNode = new PointNode(p);
            pNode.Prev = node.Prev;
            pNode.Next = node;

            if (node.Prev == null) First = pNode;
            else node.Prev.Next = pNode;

            node.Prev = pNode;

            Length++;
        }
    }
}
