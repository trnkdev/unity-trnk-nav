using NekoLib.Logger;
using NekoNav.Internals;
using NekoNav.Internals.AStar;
using NekoNav.Internals.Clearance;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace NekoNav.Physics2D
{
    public sealed class Physics2DGridSurface : MonoBehaviour, IGridSurface
    {
        private const float DefaultAgentRadius = 0.35f;

        [SerializeField] private GridBakeAsset _bakeAsset;

        [Header("Runtime Bake (optional)")]
        [SerializeField] private Tilemap _navigableTilemap;
        [SerializeField] private Tilemap _obstacleTilemap;

        [Header("Pathfinding")]
        [SerializeField] private float _agentRadius = DefaultAgentRadius;
        [SerializeField] private bool _allowDiagonal = true;

        private readonly GridData _grid = new();
        private readonly AStarSolver _solver = new();

        private GridPos[] _pathBuffer;
        private int _version;

        public GridPlane Plane => GridPlane.XY;
        public Vector3 Origin { get; private set; }
        public Vector2 CellSize { get; private set; }
        public int Version => _version;

        public GridBakeAsset BakeAsset
        {
            get => _bakeAsset;
            set => _bakeAsset = value;
        }

        public Tilemap NavigableTilemap => _navigableTilemap;
        public Tilemap ObstacleTilemap => _obstacleTilemap;

        private void Awake()
        {
            if (_bakeAsset != null)
            {
                LoadBake(_bakeAsset);
                return;
            }

            if (_navigableTilemap != null)
                BakeFromTilemaps(_navigableTilemap, _obstacleTilemap);
            else
                Log.Warn("Physics2DGridSurface has no BakeAsset and no NavigableTilemap; surface will stay unbaked.", this);
        }

        public void LoadBake(GridBakeAsset asset)
        {
            if (asset == null)
            {
                Log.Error("Physics2DGridSurface.LoadBake called with null asset.", this);
                return;
            }

            Origin = asset.Origin;
            CellSize = asset.CellSize;

            _grid.Resize(asset.MinX, asset.MinY, asset.Width, asset.Height);
            _grid.Valid.FromBytes(asset.BitCount, asset.ValidBits);
            _grid.BaseWalkable.FromBytes(asset.BitCount, asset.BaseWalkableBits);

            ApplyClearance();
            _version++;
        }

        /// <summary>Bake grid from tilemaps for quick iteration.</summary>
        public void BakeFromTilemaps(Tilemap navigableTilemap, Tilemap obstacleTilemap)
        {
            if (navigableTilemap == null)
            {
                Log.Error("Physics2DGridSurface.BakeFromTilemaps called with null navigableTilemap.", this);
                return;
            }

            navigableTilemap.CompressBounds();
            BoundsInt b = navigableTilemap.cellBounds;

            int minX = b.xMin;
            int minY = b.yMin;
            int width = b.size.x;
            int height = b.size.y;
            int bitCount = width * height;

            Origin = navigableTilemap.transform.position;
            CellSize = (Vector2)navigableTilemap.layoutGrid.cellSize;

            _grid.Resize(minX, minY, width, height);

            for (int y = b.yMin; y < b.yMax; y++)
            {
                for (int x = b.xMin; x < b.xMax; x++)
                {
                    int idx = _grid.ToIndex(x, y);

                    bool exists = navigableTilemap.HasTile(new Vector3Int(x, y, 0));
                    _grid.Valid.Set(idx, exists);

                    bool blocked = false;
                    if (exists && obstacleTilemap != null)
                        blocked = obstacleTilemap.HasTile(new Vector3Int(x, y, 0));

                    _grid.BaseWalkable.Set(idx, exists && !blocked);
                }
            }

            ApplyClearance();
            _version++;
        }

        /// <summary>Rebuild walkable from base walkable using current agent radius.</summary>
        public void ApplyClearance()
        {
            _grid.Walkable.CopyFrom(_grid.BaseWalkable);

            if (CellSize.x <= 0f || CellSize.y <= 0f)
            {
                Log.Error($"Physics2DGridSurface.CellSize is invalid: {CellSize}. Clearance will be skipped.", this);
                return;
            }

            float min = Mathf.Min(CellSize.x, CellSize.y);
            int radiusCells = min > 0f ? Mathf.CeilToInt(_agentRadius / min) : 0;
            ClearanceErosion.Apply(_grid, radiusCells);
        }

        public bool TryWorldToCell(in Vector3 world, out GridPos cell)
        {
            return GridConverter.TryWorldToCellXY(world, Origin, CellSize, out cell);
        }

        public Vector3 CellToWorldCenter(in GridPos cell)
        {
            return GridConverter.CellToWorldCenterXY(cell, Origin, CellSize);
        }

        public bool TryFindPath(in GridPos start, in GridPos goal, out GridPath path)
        {
            bool ok = _solver.FindPath(_grid, start, goal, _allowDiagonal, ref _pathBuffer, out int count);
            path = ok ? new GridPath(_pathBuffer, count) : default;
            return ok;
        }

        internal GridData GetGridDataUnsafe() => _grid;
    }
}
