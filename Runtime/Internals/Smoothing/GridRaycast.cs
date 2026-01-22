namespace NekoNav.Internals.Smoothing
{
    internal static class GridRaycast
    {
        public static bool HasLineOfSight(GridData grid, in GridPos a, in GridPos b)
        {
            int x0 = a.X;
            int y0 = a.Y;
            int x1 = b.X;
            int y1 = b.Y;

            int dx = x1 - x0;
            int dy = y1 - y0;

            int sx = dx >= 0 ? 1 : -1;
            int sy = dy >= 0 ? 1 : -1;

            dx = dx >= 0 ? dx : -dx;
            dy = dy >= 0 ? dy : -dy;

            int x = x0;
            int y = y0;

            if (!IsWalkable(grid, x, y)) return false;
            if (dx == 0 && dy == 0) return true;

            if (dx >= dy)
            {
                int err = dx / 2;
                for (int i = 0; i < dx; i++)
                {
                    x += sx;
                    err -= dy;
                    if (err < 0)
                    {
                        y += sy;
                        err += dx;
                    }

                    if (!IsWalkable(grid, x, y)) return false;
                }
            }
            else
            {
                int err = dy / 2;
                for (int i = 0; i < dy; i++)
                {
                    y += sy;
                    err -= dx;
                    if (err < 0)
                    {
                        x += sx;
                        err += dy;
                    }

                    if (!IsWalkable(grid, x, y)) return false;
                }
            }

            return true;
        }

        private static bool IsWalkable(GridData grid, int x, int y)
        {
            if (!grid.Contains(x, y)) return false;
            int idx = grid.ToIndex(x, y);
            return grid.Valid.Get(idx) && grid.Walkable.Get(idx);
        }
    }
}
