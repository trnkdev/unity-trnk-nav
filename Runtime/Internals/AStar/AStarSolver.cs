using UnityEngine;

namespace TRnK.Nav.Internals.AStar
{
    internal sealed class AStarSolver
    {
        private const int NoParent = -1;
        private const int StraightCost = 10;
        private const int DiagonalCost = 14;

        private static readonly GridPos[] s_neighbors4 =
        {
            new(1, 0), new(-1, 0), new(0, 1), new(0, -1),
        };

        private static readonly GridPos[] s_neighbors8 =
        {
            new(1, 0), new(-1, 0), new(0, 1), new(0, -1),
            new(1, 1), new(1, -1), new(-1, 1), new(-1, -1),
        };

        private readonly MinHeap _open = new();

        private int[] _g;
        private int[] _parent;
        private uint[] _stamp;
        private uint _stampId;

        public bool FindPath(GridData grid, in GridPos start, in GridPos goal, bool allowDiagonal, ref GridPos[] buffer, out int count)
        {
            count = 0;

            if (!grid.Contains(start.X, start.Y) || !grid.Contains(goal.X, goal.Y))
                return false;

            int startIdx = grid.ToIndex(start.X, start.Y);
            int goalIdx = grid.ToIndex(goal.X, goal.Y);

            if (!grid.Valid.Get(startIdx) || !grid.Walkable.Get(startIdx)) return false;
            if (!grid.Valid.Get(goalIdx) || !grid.Walkable.Get(goalIdx)) return false;

            EnsureArrays(grid.Count);
            _stampId++;

            _open.EnsureCapacity(grid.Count);

            Mark(startIdx);
            _g[startIdx] = 0;
            _parent[startIdx] = NoParent;

            _open.Push(startIdx, Heuristic(grid, startIdx, goalIdx));

            GridPos[] neighbors = allowDiagonal ? s_neighbors8 : s_neighbors4;

            while (_open.Count > 0)
            {
                int current = _open.Pop(out _);
                if (current == goalIdx)
                {
                    BuildPath(grid, startIdx, goalIdx, ref buffer, out count);
                    return count > 0;
                }

                int cx = grid.MinX + (current % grid.Width);
                int cy = grid.MinY + (current / grid.Width);

                int baseG = _g[current];

                for (int i = 0; i < neighbors.Length; i++)
                {
                    GridPos d = neighbors[i];
                    int nx = cx + d.X;
                    int ny = cy + d.Y;

                    if (!grid.Contains(nx, ny)) continue;

                    int nIdx = grid.ToIndex(nx, ny);
                    if (!grid.Valid.Get(nIdx) || !grid.Walkable.Get(nIdx)) continue;

                    int stepCost = (d.X != 0 && d.Y != 0) ? DiagonalCost : StraightCost;
                    int newG = baseG + stepCost;

                    if (!IsMarked(nIdx))
                    {
                        Mark(nIdx);
                        _g[nIdx] = newG;
                        _parent[nIdx] = current;
                        _open.Push(nIdx, newG + Heuristic(grid, nIdx, goalIdx));
                        continue;
                    }

                    if (newG < _g[nIdx])
                    {
                        _g[nIdx] = newG;
                        _parent[nIdx] = current;
                        _open.Push(nIdx, newG + Heuristic(grid, nIdx, goalIdx));
                    }
                }
            }

            return false;
        }

        private void BuildPath(GridData grid, int startIdx, int goalIdx, ref GridPos[] buffer, out int count)
        {
            int len = 0;
            int cur = goalIdx;

            while (cur != NoParent)
            {
                len++;
                if (cur == startIdx) break;
                cur = _parent[cur];
            }

            if (cur == NoParent)
            {
                count = 0;
                return;
            }

            EnsureBuffer(ref buffer, len);

            cur = goalIdx;
            for (int i = len - 1; i >= 0; i--)
            {
                int x = grid.MinX + (cur % grid.Width);
                int y = grid.MinY + (cur / grid.Width);
                buffer[i] = new GridPos(x, y);
                cur = _parent[cur];
            }

            count = len;
        }

        private static int Heuristic(GridData grid, int aIdx, int bIdx)
        {
            int ax = grid.MinX + (aIdx % grid.Width);
            int ay = grid.MinY + (aIdx / grid.Width);
            int bx = grid.MinX + (bIdx % grid.Width);
            int by = grid.MinY + (bIdx / grid.Width);

            int dx = Mathf.Abs(ax - bx);
            int dy = Mathf.Abs(ay - by);
            return (dx + dy) * StraightCost;
        }

        private void EnsureArrays(int size)
        {
            if (_g == null || _g.Length != size)
            {
                _g = new int[size];
                _parent = new int[size];
                _stamp = new uint[size];
                _stampId = 1;
            }
        }

        private static void EnsureBuffer(ref GridPos[] buffer, int size)
        {
            if (buffer == null || buffer.Length < size)
                buffer = new GridPos[size];
        }

        private bool IsMarked(int idx) => _stamp[idx] == _stampId;
        private void Mark(int idx) => _stamp[idx] = _stampId;
    }
}
