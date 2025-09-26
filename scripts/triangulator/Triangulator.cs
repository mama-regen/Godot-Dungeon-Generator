using Godot;
using Godot.Collections;
using SeidelTest.triangulator.logic;
using SeidelTest.triangulator.models;
using System;
using System.Collections.Generic;
using System.Linq;
using Point = SeidelTest.triangulator.models.Point;

namespace SeidelTest.triangulator
{
    public class Triangulator
    {
        private List<Triangle> Tris = new List<Triangle>();
        private List<Edge> Edges = new List<Edge>();

        public static Array<Vector3> Triangulate(IEnumerable<IEnumerable<Vector2>> room)
        {
            var self = new Triangulator();
            var idx = 0;
            var room_mapped = room.Select((ring) => ring.Select((point) => new Point(idx++, point.X, point.Y)).ToArray()).ToArray();

            foreach (var ring in room_mapped) {
                Point? p = null;
                Point? q = null;
                for (int i = 0; i < ring.Count(); i++)
                {
                    var j = (i + 1) % ring.Count();
                    p = i > 0 ? q : Shear(ring.ElementAt(i));
                    q = Shear(ring.ElementAt(j));
                    self.Edges.Add(p!.X > q.X ? new Edge(q, p) : new Edge(p, q));
                }
            }

            var map = new TrapezoidMap();
            foreach (var edge in self.Edges) map.AddEdge(edge);
            map.CollectPoints();

            foreach (var edge in self.Edges)
            {
                if (edge.Poly != null && edge.Poly.Length > 0) self.TriangulateMount(edge);
            }

            var result = new Array<Vector3>();
            foreach (var tri in self.Tris) result.Add(new Vector3(tri.P, tri.Q, tri.R));
            return result;
        }

        private Triangulator() { }

        private void TriangulateMount(Edge edge)
        {
            var p = edge.P;
            var q = edge.Q;
            var poly = edge.Poly;
            var next = poly!.First!.Next!;

            if (poly.Length < 3) throw new Exception("Polygons must have at least 3 points");
            if (poly.Length == 3)
            {
                Tris.Add(new Triangle(p, next.Point, q));
                return;
            }

            var points = new Queue<PointNode>();
            var positive = next.Point.Cross(q, p) > 0;

            while (next != poly.Last)
            {
                AddEar(ref points, next!, poly, positive);
                next = next!.Next;
            }

            while (points.Count > 0)
            {
                var ear = points.Dequeue();
                var prevP = ear.Prev!;
                var nextP = ear.Next!;

                Tris.Add(new Triangle(prevP.Point, ear.Point, nextP.Point));
                poly.Remove(ear);
                AddEar(ref points, prevP, poly, positive);
                AddEar(ref points, nextP, poly, positive);
            }
        }

        private void AddEar(ref Queue<PointNode> points, PointNode point, Polygon poly, bool positive)
        {
            if (!point.IsEar && point != poly.First && point != poly.Last && IsConvex(point, positive))
            {
                point.IsEar = true;
                points.Enqueue(point);
            }
        }

        private bool IsConvex(PointNode point, bool positive)
        {
            return positive == (point.Next!.Point.Cross(point.Prev!.Point, point.Point) > 0);
        }

        private static Point Shear(Point point)
        {
            return new Point(point.Id, (float)(point.X + point.Y * 1e-10), point.Y);
        }
    }
}
