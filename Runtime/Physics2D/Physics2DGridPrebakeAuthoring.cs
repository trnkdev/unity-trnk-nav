using NekoLib.Logger;
using NekoNav.Internals;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace NekoNav.Physics2D
{
    public sealed class Physics2DGridPrebakeAuthoring : MonoBehaviour
    {
        [SerializeField] private Tilemap _navigableTilemap;
        [SerializeField] private Tilemap _obstacleTilemap;
        [SerializeField] private GridBakeAsset _bakeAsset;

        public Tilemap NavigableTilemap => _navigableTilemap;
        public Tilemap ObstacleTilemap => _obstacleTilemap;

        public GridBakeAsset BakeAsset
        {
            get => _bakeAsset;
            set => _bakeAsset = value;
        }

        public bool CanOperate => _navigableTilemap != null;

#if UNITY_EDITOR
        /// <summary>Bake tilemap walkability into the assigned bake asset.</summary>
        public void BakeToAsset()
        {
            if (_navigableTilemap == null)
            {
                Log.Error("Physics2DGridPrebakeAuthoring.BakeToAsset: NavigableTilemap is null.", this);
                return;
            }

            if (_bakeAsset == null)
            {
                Log.Error("Physics2DGridPrebakeAuthoring.BakeToAsset: BakeAsset is null. Click Create in the inspector first.", this);
                return;
            }

            _navigableTilemap.CompressBounds();
            BoundsInt b = _navigableTilemap.cellBounds;

            int minX = b.xMin;
            int minY = b.yMin;
            int width = b.size.x;
            int height = b.size.y;
            int bitCount = width * height;

            GridData grid = new();
            grid.Resize(minX, minY, width, height);

            for (int y = b.yMin; y < b.yMax; y++)
            {
                for (int x = b.xMin; x < b.xMax; x++)
                {
                    int idx = grid.ToIndex(x, y);

                    bool exists = _navigableTilemap.HasTile(new Vector3Int(x, y, 0));
                    grid.Valid.Set(idx, exists);

                    bool blocked = false;
                    if (exists && _obstacleTilemap != null)
                        blocked = _obstacleTilemap.HasTile(new Vector3Int(x, y, 0));

                    grid.BaseWalkable.Set(idx, exists && !blocked);
                }
            }

            Vector3 origin = _navigableTilemap.transform.position;
            Vector2 cellSize = (Vector2)_navigableTilemap.layoutGrid.cellSize;

            _bakeAsset.EditorSetData(
                GridPlane.XY,
                origin,
                cellSize,
                minX,
                minY,
                width,
                height,
                bitCount,
                grid.Valid.ToBytes(),
                grid.BaseWalkable.ToBytes());
        }
#endif
    }
}
