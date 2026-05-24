using UnityEngine;

namespace TRnK.Nav
{
    public sealed class GridBakeAsset : ScriptableObject
    {
        [SerializeField] private GridPlane _plane = GridPlane.XY;
        [SerializeField] private Vector3 _origin;
        [SerializeField] private Vector2 _cellSize = Vector2.one;

        [SerializeField] private int _minX;
        [SerializeField] private int _minY;
        [SerializeField] private int _width;
        [SerializeField] private int _height;

        [SerializeField] private int _bitCount;
        [SerializeField] private byte[] _validBits;
        [SerializeField] private byte[] _baseWalkableBits;

        public GridPlane Plane => _plane;
        public Vector3 Origin => _origin;
        public Vector2 CellSize => _cellSize;

        public int MinX => _minX;
        public int MinY => _minY;
        public int Width => _width;
        public int Height => _height;

        internal int BitCount => _bitCount;
        internal byte[] ValidBits => _validBits;
        internal byte[] BaseWalkableBits => _baseWalkableBits;

#if UNITY_EDITOR
        /// <summary>Write baked data into this asset (editor-only).</summary>
        public void EditorSetData(
            GridPlane plane,
            Vector3 origin,
            Vector2 cellSize,
            int minX,
            int minY,
            int width,
            int height,
            int bitCount,
            byte[] validBits,
            byte[] baseWalkableBits)
        {
            _plane = plane;
            _origin = origin;
            _cellSize = cellSize;

            _minX = minX;
            _minY = minY;
            _width = width;
            _height = height;

            _bitCount = bitCount;
            _validBits = validBits;
            _baseWalkableBits = baseWalkableBits;
        }
#endif
    }
}
