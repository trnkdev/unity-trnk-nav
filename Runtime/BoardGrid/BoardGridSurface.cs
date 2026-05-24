using System.Collections.Generic;
using TRnK.Extensions;
using TRnK.Logger;
using TRnK.Nav.Internals;
using TRnK.Nav.Internals.AStar;
using UnityEngine;

namespace TRnK.Nav.BoardGrid
{
    public sealed class BoardGridSurface : MonoBehaviour, IGridSurface
    {
        [SerializeField] private GridPlane _plane = GridPlane.XY;
        [SerializeField] private Vector3 _origin = Vector3.zero;
        [SerializeField] private Vector2 _cellSize = Vector2.one;
        [SerializeField] private bool _allowDiagonal = false;

        private readonly GridData _grid = new();
        private readonly AStarSolver _solver = new();

        private HashSet<GridPos> _valid;
        private HashSet<GridPos> _blocked;

        private GridPos[] _pathBuffer;
        private int _version;

        public GridPlane Plane => _plane;
        public Vector3 Origin => _origin;
        public Vector2 CellSize => _cellSize;
        public int Version => _version;

        public void SetValidCells(IEnumerable<Transform> transforms)
        {
            if (transforms == null)
            {
                Log.Warn("BoardGridSurface.SetValidCells(transforms) called with null.", this);
                return;
            }

            List<Vector2Int> cells = null;

            foreach (Transform t in transforms)
            {
                if (t == null) continue;
                if (!TryWorldToCell(t.position, out GridPos cell)) continue;
                cells ??= new List<Vector2Int>();
                cells.Add((Vector2Int)cell);
            }

            if (cells.IsNullOrEmpty())
                Log.Warn("BoardGridSurface.SetValidCells(transforms): no cells were converted; surface not baked.", this);
            SetValidCells(cells);
        }

        public void SetValidCells(IEnumerable<GameObject> gameObjects)
        {
            if (gameObjects == null)
            {
                Log.Warn("BoardGridSurface.SetValidCells(gameObjects) called with null.", this);
                return;
            }

            List<Vector2Int> cells = null;

            foreach (GameObject go in gameObjects)
            {
                if (go == null) continue;
                if (!TryWorldToCell(go.transform.position, out GridPos cell)) continue;
                cells ??= new List<Vector2Int>();
                cells.Add((Vector2Int)cell);
            }

            if (cells.IsNullOrEmpty())
                Log.Warn("BoardGridSurface.SetValidCells(gameObjects): no cells were converted; surface not baked.", this);
            SetValidCells(cells);
        }

        public void SetValidCellsFromWorldPositions(IEnumerable<Vector3> worldPositions)
        {
            if (worldPositions == null)
            {
                Log.Warn("BoardGridSurface.SetValidCellsFromWorldPositions called with null.", this);
                return;
            }

            List<Vector2Int> cells = null;

            foreach (Vector3 world in worldPositions)
            {
                if (!TryWorldToCell(world, out GridPos cell)) continue;
                cells ??= new List<Vector2Int>();
                cells.Add((Vector2Int)cell);
            }

            if (cells.IsNullOrEmpty())
                Log.Warn("BoardGridSurface.SetValidCellsFromWorldPositions: no cells were converted; surface not baked.", this);
            SetValidCells(cells);
        }

        public void SetValidCells(IReadOnlyList<Vector2Int> cells)
        {
            if (cells.IsNullOrEmpty())
            {
                Log.Warn("BoardGridSurface.SetValidCells(cells) called with null/empty; surface not baked.", this);
                return;
            }

            _valid ??= new HashSet<GridPos>(cells.Count);
            _valid.Clear();

            int minX = int.MaxValue, minY = int.MaxValue;
            int maxX = int.MinValue, maxY = int.MinValue;

            for (int i = 0; i < cells.Count; i++)
            {
                Vector2Int c = cells[i];
                GridPos p = new(c.x, c.y);
                _valid.Add(p);

                if (c.x < minX) minX = c.x;
                if (c.y < minY) minY = c.y;
                if (c.x > maxX) maxX = c.x;
                if (c.y > maxY) maxY = c.y;
            }

            int width = maxX - minX + 1;
            int height = maxY - minY + 1;
            _grid.Resize(minX, minY, width, height);

            Bake();
        }

        public void SetBlockedCells(IReadOnlyList<Vector2Int> cells)
        {
            int capacity = cells.IsNullOrEmpty() ? 0 : cells.Count;

            _blocked ??= new HashSet<GridPos>(capacity);
            _blocked.Clear();

            if (cells != null)
            {
                for (int i = 0; i < cells.Count; i++)
                {
                    Vector2Int c = cells[i];
                    _blocked.Add(new GridPos(c.x, c.y));
                }
            }

            Bake();
        }

        public void SetCellBlocked(Vector2Int cell, bool blocked)
        {
            _blocked ??= new HashSet<GridPos>();
            GridPos p = new(cell.x, cell.y);

            if (blocked) _blocked.Add(p);
            else _blocked.Remove(p);

            Bake();
        }

        /// <summary>Bake valid/walkable bits from provided cell sets.</summary>
        public void Bake()
        {
            if (_valid.IsNullOrEmpty())
            {
                Log.Warn("BoardGridSurface.Bake called without any valid cells; ignoring.", this);
                return;
            }

            _grid.Valid.ClearAll();
            _grid.BaseWalkable.ClearAll();

            foreach (GridPos p in _valid)
            {
                if (!_grid.Contains(p.X, p.Y)) continue;
                int idx = _grid.ToIndex(p.X, p.Y);
                _grid.Valid.Set(idx, true);
                _grid.BaseWalkable.Set(idx, true);
            }

            if (_blocked != null)
            {
                foreach (GridPos p in _blocked)
                {
                    if (!_grid.Contains(p.X, p.Y)) continue;
                    int idx = _grid.ToIndex(p.X, p.Y);
                    if (_grid.Valid.Get(idx))
                        _grid.BaseWalkable.Set(idx, false);
                }
            }

            _grid.Walkable.CopyFrom(_grid.BaseWalkable);
            _version++;
        }

        public bool TryWorldToCell(in Vector3 world, out GridPos cell)
        {
            return _plane == GridPlane.XY
                ? GridConverter.TryWorldToCellXY(world, _origin, _cellSize, out cell)
                : GridConverter.TryWorldToCellXZ(world, _origin, _cellSize, out cell);
        }

        public Vector3 CellToWorldCenter(in GridPos cell)
        {
            return _plane == GridPlane.XY
                ? GridConverter.CellToWorldCenterXY(cell, _origin, _cellSize)
                : GridConverter.CellToWorldCenterXZ(cell, _origin, _cellSize);
        }

        public bool TryFindPath(in GridPos start, in GridPos goal, out GridPath path)
        {
            bool ok = _solver.FindPath(_grid, start, goal, _allowDiagonal, ref _pathBuffer, out int count);
            path = ok ? new GridPath(_pathBuffer, count) : default;
            return ok;
        }
    }
}
