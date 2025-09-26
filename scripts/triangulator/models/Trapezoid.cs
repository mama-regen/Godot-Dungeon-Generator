using System.Collections.Generic;

namespace SeidelTest.triangulator.models
{
    public class Trapezoid
    {
        public Point LeftPoint { get; set; }
        public Point RightPoint { get; set; }
        public Edge Top {  get; set; }
        public Edge Bottom { get; set; }
        public bool IsInside { get; set; } = false;
        public bool IsRemoved { get; set; } = false;
        public QueryNode? Sink { get; set; } = null;
        public Trapezoid? UpperLeft { get; set; } = null;
        public Trapezoid? UpperRight { get; set; } = null;
        public Trapezoid? LowerLeft { get; set; } = null;
        public Trapezoid? LowerRight { get; set; } = null;

        public Trapezoid(Point leftPoint, Point rightPoint, Edge top, Edge bottom)
        {
            LeftPoint = leftPoint;
            RightPoint = rightPoint;
            Top = top;
            Bottom = bottom;
        }

        public void UpdateLeft(Trapezoid? upperLeft, Trapezoid? lowerLeft)
        {
            UpperLeft = upperLeft;
            LowerLeft = lowerLeft;
            if (UpperLeft != null) UpperLeft.UpperRight = this;
            if (LowerLeft != null) LowerLeft.LowerRight = this;
        }

        public void UpdateRight(Trapezoid? upperRight, Trapezoid? lowerRight)
        {
            UpperRight = upperRight;
            LowerRight = lowerRight;
            if (UpperRight != null) UpperRight.UpperLeft = this;
            if (LowerRight != null) LowerRight.LowerLeft = this;
        }

        public void UpdateSides(Trapezoid? upperLeft, Trapezoid? lowerLeft, Trapezoid? upperRight, Trapezoid? lowerRight)
        {
            UpdateLeft(upperLeft, lowerLeft);
            UpdateRight(upperRight, lowerRight);
        }

        public void MarkInside()
        {
            var stack = new Stack<Trapezoid>();
            stack.Push(this);

            do
            {
                var trap = stack.Pop();
                if (trap != null && !trap.IsInside)
                {
                    trap.IsInside = true;
                    if (trap.UpperLeft != null) stack.Push(trap.UpperLeft);
                    if (trap.LowerLeft != null) stack.Push(trap.LowerLeft);
                    if (trap.UpperRight != null) stack.Push(trap.UpperRight);
                    if (trap.LowerRight != null) stack.Push(trap.LowerRight);
                }
            } while (stack.Count > 0);
        }

        public bool Contains(Point p)
        {
            return p.X > LeftPoint.X && p.X < RightPoint.X && Top > p && Bottom < p;
        }

        public void AddPoint(Edge edge, Point p)
        {
            var poly = edge.Poly;
            if (poly == null)
            {
                if (p != edge.P && p != edge.Q)
                {
                    edge.Poly = new Polygon();
                    poly = edge.Poly;
                    poly.Add(edge.P);
                    poly.Add(p);
                    poly.Add(edge.Q);
                }
            } else
            {
                var v = poly.First;
                while (v != null)
                {
                    if (p == v.Point) return;
                    if (p.X < v.Point.X)
                    {
                        poly.InsertBefore(p, v);
                        return;
                    }
                    v = v.Next;
                }
                poly.Add(p);
            }
        }

        public void AddPoints()
        {
            if (LeftPoint.Id != Bottom.P.Id) AddPoint(Bottom, LeftPoint);
            if (RightPoint.Id != Bottom.Q.Id) AddPoint(Bottom, RightPoint);
            if (LeftPoint.Id != Top.P.Id) AddPoint(Top, LeftPoint);
            if (RightPoint.Id != Top.Q.Id) AddPoint(Top, RightPoint);
        }
    }
}
