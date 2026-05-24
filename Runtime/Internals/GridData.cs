namespace TRnK.Nav.Internals
{
    internal sealed class GridData
    {
        public int MinX { get; private set; }
        public int MinY { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public int Count => Width * Height;

        public Bitset Valid;
        public Bitset BaseWalkable;
        public Bitset Walkable;

        public void Resize(int minX, int minY, int width, int height)
        {
            MinX = minX;
            MinY = minY;
            Width = width;
            Height = height;

            int count = width * height;
            Valid.Resize(count);
            BaseWalkable.Resize(count);
            Walkable.Resize(count);
        }

        public int ToIndex(int x, int y) => (y - MinY) * Width + (x - MinX);

        public bool Contains(int x, int y)
        {
            int lx = x - MinX;
            int ly = y - MinY;
            return (uint)lx < (uint)Width && (uint)ly < (uint)Height;
        }
    }
}
