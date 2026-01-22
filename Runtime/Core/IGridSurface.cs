using UnityEngine;

namespace NekoNav
{
    public interface IGridSurface
    {
        GridPlane Plane { get; }
        Vector3 Origin { get; }
        Vector2 CellSize { get; }

        bool TryWorldToCell(in Vector3 world, out GridPos cell);
        Vector3 CellToWorldCenter(in GridPos cell);

        bool TryFindPath(in GridPos start, in GridPos goal, out GridPath path);
        int Version { get; }
    }
}
