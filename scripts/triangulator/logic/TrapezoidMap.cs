using SeidelTest.triangulator.models;
using System;
using System.Collections.Generic;

namespace SeidelTest.triangulator.logic
{
    public class TrapezoidMap
    {
        private List<Trapezoid> Items;
        private Trapezoid Root;
        private QueryGraph Graph;
        private Edge? BottomCross = null;
        private Edge? TopCross = null;

        public TrapezoidMap()
        {
            var top = new Edge(
                new models.Point(-1, float.MinValue, float.MaxValue),
                new models.Point(-1, float.MaxValue, float.MaxValue)
            );
            var bot = new Edge(
                new models.Point(-1, float.MinValue, float.MinValue),
                new models.Point(-1, float.MaxValue, float.MinValue)
            );
            Root = new Trapezoid(bot.P, top.Q, top, bot);
            Items = new List<Trapezoid>() { Root };
            Graph = new QueryGraph(Root);
        }

        public void AddEdge(Edge edge)
        {
            var trap = Graph.Locate(edge.P, edge.Slope);
            var containsP = false;
            var containsQ = false;

            while(trap != null)
            {
                containsP = !containsP && trap.Contains(edge.P);
                containsQ = !containsQ && trap.Contains(edge.Q);

                if (containsP)
                {
                    if (containsQ)
                    {
                        Case1(trap, edge);
                        trap = null;
                    }
                    else trap = Case2(trap, edge);
                } else
                {
                    if (containsQ) trap = Case4(trap, edge);
                    else trap = Case3(trap, edge);
                }
            }

            BottomCross = null;
            TopCross = null;
        }

        public void CollectPoints()
        {
            foreach (var trap in Items)
            {
                if (trap.IsRemoved) continue;
                if (trap.Top == Root.Top && trap.Bottom.Below != null && !trap.Bottom.Below.IsRemoved)
                {
                    trap.Bottom.Below.MarkInside();
                    break;
                }
                if (trap.Bottom == Root.Bottom && trap.Top.Above != null && !trap.Top.Above.IsRemoved)
                {
                    trap.Top.Above.MarkInside();
                    break;
                }
            }

            foreach (var trap in Items)
            {
                if (!trap.IsRemoved && trap.IsInside) trap.AddPoints();
            }
        }

        private Trapezoid? NextTrap(Trapezoid trap, Edge edge)
        {
            if (edge.Q.X <= trap.RightPoint.X) return null;
            var result = edge > trap.RightPoint ? trap.UpperRight : trap.LowerRight;
            if (result == null) throw new Exception("NextTrap found unexpected NULL");
            return result;
        }

        private void Case1(Trapezoid trap, Edge edge)
        {
            var trap2 = new Trapezoid(edge.P, edge.Q, trap.Top, edge);
            var trap3 = new Trapezoid(edge.P, edge.Q, edge, trap.Bottom);
            var trap4 = new Trapezoid(edge.Q, trap.RightPoint, trap.Top, trap.Bottom);

            trap4.UpdateRight(trap.UpperRight, trap.LowerRight);
            trap4.UpdateLeft(trap2, trap3);

            trap.RightPoint = edge.P;
            trap.UpdateRight(trap2, trap3);

            var sink = trap.Sink!;
            trap.Sink = null;
            Graph.Case1(sink, edge, trap, trap2, trap3, trap4);

            Items.AddRange(new Trapezoid[] { trap2, trap3, trap4 });
        }

        private Trapezoid? Case2(Trapezoid trap, Edge edge)
        {
            var next = NextTrap(trap, edge);
            var trap2 = new Trapezoid(edge.P, trap.RightPoint, trap.Top, edge);
            var trap3 = new Trapezoid(edge.P, trap.RightPoint, edge, trap.Bottom);

            trap.RightPoint = edge.P;

            trap.UpdateLeft(trap.UpperLeft, trap.LowerLeft);
            trap2.UpdateSides(trap, null, trap.UpperRight, null);
            trap3.UpdateSides(null, trap, null, trap.LowerRight);

            BottomCross = trap.Bottom;
            TopCross = trap.Top;

            edge.Above = trap2;
            edge.Below = trap3;

            var sink = trap.Sink!;
            trap.Sink = null;
            Graph.Case2(sink, edge, trap, trap2, trap3);

            Items.AddRange(new Trapezoid[] { trap2, trap3 });

            return next;
        }

        private Trapezoid? Case3(Trapezoid trap, Edge edge)
        {
            var next = NextTrap(trap, edge);
            var bottom = trap.Bottom;
            var top = trap.Top;
            var lowerRight = trap.LowerRight;
            var lowerLeft = trap.LowerLeft;

            Trapezoid trap1;
            Trapezoid trap2;

            if (TopCross == top)
            {
                trap1 = trap.UpperLeft!;
                trap1.UpdateRight(trap.UpperRight, null);
                trap1.RightPoint = trap.RightPoint;
            } else
            {
                trap1 = trap;
                trap1.Bottom = edge;
                trap1.LowerLeft = edge.Above;
                if (edge.Above != null) edge.Above.LowerRight = trap1;
                trap1.LowerRight = null;
            }

            if (BottomCross == bottom)
            {
                trap2 = lowerLeft!;
                trap2.UpdateRight(null, lowerRight);
                trap2.RightPoint = trap.RightPoint;
            } else if (trap1 == trap)
            {
                trap2 = new Trapezoid(trap.LeftPoint, trap.RightPoint, edge, bottom);
                trap2.UpdateSides(edge.Below, lowerLeft, null, lowerRight);
                Items.Add(trap2);
            } else
            {
                trap2 = trap;
                trap2.Top = edge;
                trap2.UpperLeft = edge.Below;
                if (edge.Below != null) edge.Below.UpperRight = trap2;
                trap2.UpperRight = null;
            }

            if (trap != trap1 && trap != trap2) trap.IsRemoved = true;

            BottomCross = bottom;
            TopCross = top;

            edge.Above = trap1;
            edge.Below = trap2;

            var sink = trap.Sink!;
            trap.Sink = null;
            Graph.Case3(sink, edge, trap1, trap2);

            return next;
        }

        private Trapezoid? Case4(Trapezoid trap, Edge edge)
        {
            var next = NextTrap(trap, edge);
            Trapezoid trap1;
            Trapezoid trap2;

            if (TopCross == trap.Top)
            {
                trap1 = trap.UpperLeft!;
                trap1.RightPoint = edge.Q;
            } else
            {
                trap1 = new Trapezoid(trap.LeftPoint, edge.Q, trap.Top, edge);
                trap1.UpdateLeft(trap.UpperLeft, edge.Above);
                Items.Add(trap1);
            }

            if (BottomCross == trap.Bottom)
            {
                trap2 = trap.LowerLeft!;
                trap2.RightPoint = edge.Q;
            } else
            {
                trap2 = new Trapezoid(trap.LeftPoint, edge.Q, edge, trap.Bottom);
                trap2.UpdateLeft(edge.Below, trap.LowerLeft);
                Items.Add(trap2);
            }

            trap.LeftPoint = edge.Q;
            trap.UpdateLeft(trap1, trap2);

            var sink = trap.Sink!;
            trap.Sink = null;
            Graph.Case4(sink, edge, trap1, trap2, trap);

            return next;
        }
    }
}
