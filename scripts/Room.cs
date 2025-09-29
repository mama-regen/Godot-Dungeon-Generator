using DungeonGenerator.scripts;
using DungeonGenerator.scripts.enums;
using Godot;
using SeidelTest.triangulator;
using System;
using System.Collections.Generic;
using System.Linq;

[Tool]
public partial class Room : Node3D
{
    private Polygon2D? _border = null;
    private IEnumerable<Polygon2D> _holes = Array.Empty<Polygon2D>();

    [Export]
    public string RoomName { get; set; } = "";
    [Export(PropertyHint.Enum, "Floor,Ceiling,Both,Column")]
    public HoleOption Holes { get; set; } = HoleOption.Both;
    [Export]
    public float ReducePointScale { get; set; } = 50.0F;
    [Export]
    public float WallHeight { get; set; } = 1.0F;
    [Export]
    public bool SaveMesh { get; set; } = false;
    [ExportToolButton("Rebuild Mesh")]
    public Callable RebuildButton => Callable.From(RebuildMesh);

    public override void _Ready()
    {
        base._Ready();

        if (ResourceLoader.Exists("res://room_" + RoomName))
        {
            ArrayMesh aMesh = ResourceLoader.Load<ArrayMesh>("res://room_" + RoomName);
            AddMesh(aMesh);
            return;
        }

        BuildMesh();
    }

    private void RebuildMesh()
    {
        var existing = this.ScanChildren<MeshInstance3D>();
        if (existing != null && existing.Count() > 0) { RemoveChild(existing.ElementAt(0)); }
        BuildMesh();
    }

    private void BuildMesh()
    {
        //Scan for polygons
        var polygons = this.ScanChildren<Polygon2D>();
        _border = polygons.FirstOrDefault();
        _holes = polygons.Skip(1);

        if (_border == null) return;

        //Init array to hold border + holes corrected values
        Vector2[][] _shapeV = new Vector2[_holes.Count() + 1][];

        //Iterate through points, setting shapeV and correcting using the shape position, offset and scale;
        for (int i = 0; i < _shapeV.Length; i++)
        {
            //var target = (i == 0 ? _border : _holes.ElementAt(i - 1));

            Polygon2D target;
            if (i == 0) target = _border;
            else if (i - 1 < _holes.Count()) target = _holes.ElementAt(i - 1);
            else return;

            _shapeV[i] = new Vector2[target.Polygon.Length];

            for (int j = 0; j < target!.Polygon.Length; j++)
            {
                _shapeV[i][j] = (target.Polygon[j] + target.Position + target.Offset) * target.Scale;
            }
        }

        //Find min and max length and with to place origin in middle
        Vector2 max = new Vector2(float.MinValue, float.MinValue);
        Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
        for (int i = 0; i < _shapeV[0].Length; i++)
        {
            max.X = Math.Max(max.X, _shapeV[0][i].X);
            max.Y = Math.Max(max.Y, _shapeV[0][i].Y);
            min.X = Math.Min(min.X, _shapeV[0][i].X);
            min.Y = Math.Min(min.Y, _shapeV[0][i].Y);
        }

        //Move points over by origin
        GD.Print("SHAPE V LENGTH: [" + _shapeV.Length.ToString() + "]");
        for (int i = 0; i < _shapeV.Length; i++)
        {
            for (int j = 0; j < _shapeV[i].Length; j++) _shapeV[i][j] = _shapeV[i][j] + min - max;
        }

        //Generate triangles for solid shape and shape with holes included
        var tris = Triangulator.Triangulate(_shapeV);
        var sans_holes = (int)Holes < 2 ? Triangulator.Triangulate(new Vector2[1][] { _shapeV[0] }) : new Godot.Collections.Array<Vector3>();

        Godot.Collections.Array surfaceArr = new Godot.Collections.Array();
        surfaceArr.Resize((int)Mesh.ArrayType.Max);

        //Move max over so min/max values start at 0
        max -= min;

        //Init mesh components
        var verts = new Godot.Collections.Array<Vector3>();
        var uvs = new Godot.Collections.Array<Vector2>();
        var normals = new Godot.Collections.Array<Vector4>();
        var indices = new Godot.Collections.Array<int>();        

        //And verts with offset for floor and ceiling, keeping origin at 0;
        var offset = 0;
        for (int f = 0; f < 2; f++)
        {
            int limit;
            if (f == 0) limit = Holes == HoleOption.CeilingOnly ? 1 : _shapeV.Length;
            else limit = Holes == HoleOption.FloorOnly ? 1 : _shapeV.Length;
            for (int i = 0; i < limit; i++)
            {
                foreach (var p in _shapeV[i])
                {
                    var point = new Vector3(p.X / ReducePointScale, WallHeight * (f == 0 ? -0.5F : 0.5F), p.Y / ReducePointScale);
                    verts.Add(point);
                    normals.Add(new Vector4(0, f == 0 ? 1 : -1, 0, 1));
                    uvs.Add(p / max);
                }
            }
            if (f == 0) offset = verts.Count;
        }

        //Add UVs and triangles for floor and ceiling
        for (int f = 0; f < 2; f++)
        {
            Godot.Collections.Array<Vector3> target;
            if (f == 0 && Holes == HoleOption.CeilingOnly) target = sans_holes;
            else if (f == 1 && Holes == HoleOption.FloorOnly) target = sans_holes;
            else target = tris;
            foreach (Vector3 tri in target)
            {
                var p = (int)tri.X + offset * f;
                var q = (int)tri.Y + offset * f;
                var r = (int)tri.Z + offset * f;
 
                if (Clockwise(verts[p], verts[q], verts[r]) == (f == 0))
                {
                    var _p = p;
                    p = r;
                    r = _p;
                }

                indices.Add(p);
                indices.Add(q);
                indices.Add(r);
            }
        }

        //Add triangles for walls and calculate normals per face
        //TODO: Figure out UVs for walls, and in general
        for (int x = 0; x < (Holes == HoleOption.Column ? _shapeV.Length : 1); x++)
        {
            for (int i = 0; i < _shapeV[x].Length; i++)
            {
                var p = i;
                var q = p + offset;
                var r = (i + 1) % _shapeV[x].Length;
                var s = r + offset;

                var norm = CalcNormal(verts[p], verts[q], verts[r]);
                var norm4 = new Vector4(norm.X, norm.Y, norm.Z, 1);

                normals[p] += norm4;
                normals[q] += norm4;
                normals[r] += norm4;

                indices.AddRange(new[] { s, p, q });
                indices.AddRange(new[] { r, p, s });
            }
        }

        //Assign mesh data
        surfaceArr[(int)Mesh.ArrayType.Vertex] = verts.ToArray();
        surfaceArr[(int)Mesh.ArrayType.TexUV] = uvs.ToArray();
        surfaceArr[(int)Mesh.ArrayType.Normal] = normals.Select(v4 => new Vector3(v4.X, v4.Y, v4.Z) / v4.W).ToArray();
        surfaceArr[(int)Mesh.ArrayType.Index] = indices.ToArray();

        //Add mesh to node
        var aMesh = new ArrayMesh();
        aMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, surfaceArr);
        AddMesh(aMesh);

        //Save mesh if willing and able
        if (SaveMesh && !String.IsNullOrWhiteSpace(RoomName)) ResourceSaver.Save(aMesh, "res://room_" + RoomName, ResourceSaver.SaverFlags.Compress);
    }

    private void AddMesh(ArrayMesh aMesh)
    {
        var meshInst = new MeshInstance3D();
        meshInst.Mesh = aMesh;
        AddChild(meshInst);
    }

    private Vector3 CalcNormal(Vector3 p, Vector3 q, Vector3 r)
    {
        var a = q - p;
        var b = r - p;
        return new Vector3(
            a.Y * b.Z - a.Z * b.Y,
            a.Z * b.X - a.X * b.Z,
            a.X * b.Y - a.Y * b.X
        );
    }

    private bool Clockwise(Vector3 p, Vector3 q, Vector3 r)
    {
        return p.X * q.Z - p.Z * q.X + q.X * r.Z - q.Z * r.X + r.X * p.Z - r.Z * p.X < 0;
    }
}
