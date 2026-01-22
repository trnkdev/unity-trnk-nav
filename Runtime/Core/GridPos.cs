using System;
using UnityEngine;

namespace NekoNav
{
    [Serializable]
    public readonly struct GridPos : IEquatable<GridPos>
    {
        private const int HashMultiplier = 397;

        public readonly int X;
        public readonly int Y;

        public GridPos(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Vector2Int ToVector2Int() => new(X, Y);

        public bool Equals(GridPos other) => X == other.X && Y == other.Y;
        public override bool Equals(object obj) => obj is GridPos other && Equals(other);
        public override int GetHashCode() => (X * HashMultiplier) ^ Y;
        public override string ToString() => $"({X},{Y})";

        public static bool operator ==(GridPos a, GridPos b) => a.Equals(b);
        public static bool operator !=(GridPos a, GridPos b) => !a.Equals(b);

        public static explicit operator Vector2Int(GridPos p) => new(p.X, p.Y);
        public static explicit operator GridPos(Vector2Int v) => new(v.x, v.y);
    }
}
