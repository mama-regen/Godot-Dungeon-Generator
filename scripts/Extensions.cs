using Godot;
using System.Collections.Generic;

namespace DungeonGenerator.scripts
{
    public static class Extensions
    {
        public static IEnumerable<T> ScanChildren<T>(this Node node) where T : Node
        {
            var children = new List<T>();
            return node._ScanChildren(ref children);
        }

        private static IEnumerable<T> _ScanChildren<T>(this Node node, ref List<T> children) where T : Node
        {
            var _children = node.GetChildren();
            if (_children.Count > 0)
            {
                foreach (var child in _children)
                {
                    if (child.GetType() == typeof(T)) children.Add((T)child);
                    child._ScanChildren(ref children);
                }
            }
            return children;
        }
    }
}
