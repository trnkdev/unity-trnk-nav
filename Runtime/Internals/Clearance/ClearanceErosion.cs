using UnityEngine;

namespace TRnK.Nav.Internals.Clearance
{
    internal static class ClearanceErosion
    {
        private const int MaxRadiusCells = 32;

        public static void Apply(GridData grid, int radiusCells)
        {
            radiusCells = Mathf.Clamp(radiusCells, 0, MaxRadiusCells);
            if (radiusCells <= 0) return;

            grid.Walkable.CopyFrom(grid.BaseWalkable);

            int r2 = radiusCells * radiusCells;

            for (int y = grid.MinY; y < grid.MinY + grid.Height; y++)
            {
                for (int x = grid.MinX; x < grid.MinX + grid.Width; x++)
                {
                    int idx = grid.ToIndex(x, y);
                    if (!grid.Valid.Get(idx)) continue;

                    if (grid.BaseWalkable.Get(idx))
                        continue;

                    for (int oy = -radiusCells; oy <= radiusCells; oy++)
                    {
                        for (int ox = -radiusCells; ox <= radiusCells; ox++)
                        {
                            int d2 = (ox * ox) + (oy * oy);
                            if (d2 > r2) continue;

                            int nx = x + ox;
                            int ny = y + oy;
                            if (!grid.Contains(nx, ny)) continue;

                            int nIdx = grid.ToIndex(nx, ny);
                            if (!grid.Valid.Get(nIdx)) continue;

                            grid.Walkable.Set(nIdx, false);
                        }
                    }
                }
            }
        }
    }
}
