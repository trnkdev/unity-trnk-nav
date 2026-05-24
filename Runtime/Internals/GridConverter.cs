using UnityEngine;

namespace TRnK.Nav.Internals
{
    internal static class GridConverter
    {
        public static bool TryWorldToCellXY(in Vector3 world, in Vector3 origin, in Vector2 cellSize, out GridPos cell)
        {
            if (cellSize.x <= 0f || cellSize.y <= 0f)
            {
                cell = default;
                return false;
            }

            float dx = world.x - origin.x;
            float dy = world.y - origin.y;

            int x = Mathf.FloorToInt(dx / cellSize.x);
            int y = Mathf.FloorToInt(dy / cellSize.y);
            cell = new GridPos(x, y);
            return true;
        }

        public static bool TryWorldToCellXZ(in Vector3 world, in Vector3 origin, in Vector2 cellSize, out GridPos cell)
        {
            if (cellSize.x <= 0f || cellSize.y <= 0f)
            {
                cell = default;
                return false;
            }

            float dx = world.x - origin.x;
            float dz = world.z - origin.z;

            int x = Mathf.FloorToInt(dx / cellSize.x);
            int y = Mathf.FloorToInt(dz / cellSize.y);
            cell = new GridPos(x, y);
            return true;
        }

        public static Vector3 CellToWorldCenterXY(in GridPos cell, in Vector3 origin, in Vector2 cellSize)
        {
            float x = origin.x + (cell.X + 0.5f) * cellSize.x;
            float y = origin.y + (cell.Y + 0.5f) * cellSize.y;
            return new Vector3(x, y, origin.z);
        }

        public static Vector3 CellToWorldCenterXZ(in GridPos cell, in Vector3 origin, in Vector2 cellSize)
        {
            float x = origin.x + (cell.X + 0.5f) * cellSize.x;
            float z = origin.z + (cell.Y + 0.5f) * cellSize.y;
            return new Vector3(x, origin.y, z);
        }
    }
}
