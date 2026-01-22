# Neko Nav

Lightweight grid navigation for Unity.

- **BoardGrid**: explicit valid cells + dynamic blocked cells (supports XY and XZ).
- **Physics2D**: top-down 2D navigation on a baked grid (tilemap prebake or runtime bake).

## Installation (Unity Package Manager)

1. Install NekoLib:

```
https://github.com/boobosua/unity-nekolib.git
```

2. Install NekoNav:

```
https://github.com/boobosua/unity-neko-nav.git
```

Package name: `com.nekoindie.nekounity.nav`

> NekoNav uses NekoLib for logging (`NekoLib.Logger.Log`) and shared utilities/extensions.

## Logging

NekoNav logs through `NekoLib.Logger.Log`.

- Logs appear in **Editor**, **Development builds**, or when `NEKOLIB_LOG` is defined.
- In non-development player builds, logs may be compiled out (by design in NekoLib).

## Quick start (Physics2D)

Use this when your walkable space is mostly static (tilemap-based levels).

### Tilemap meanings

- **Navigable Tilemap**: tiles define which grid cells exist.
- **Obstacle Tilemap** (optional): tiles that block those cells.

> Current approach is tile-occupancy based (no collider baking).

### Editor setup (static prebake)

1. Create (or locate) your Tilemap objects:
   - Navigable tilemap (tiles exist = cells exist)
   - Optional obstacle tilemap (tiles exist = blocked)

2. Create an empty GameObject (example: `GridBakeAuthoring`) and add:
   - `Physics2DGridPrebakeAuthoring`

3. In the inspector:
   - Assign **Navigable Tilemap**
   - Assign **Obstacle Tilemap** (optional)
   - On **Bake Asset** row:
     - Click **Create** (first time) to create a `GridBakeAsset`
     - Click **Bake** to write the latest tilemap data into that asset

4. Create a runtime surface:
   - Add `Physics2DGridSurface` to a GameObject (example: `NavSurface`)
   - Assign the same `GridBakeAsset` into **Bake Asset**

5. Create an agent:
   - Add `Physics2DAgentMotor` to your agent GameObject
   - Ensure it has a `Rigidbody2D` (required)
   - Assign the `Physics2DGridSurface` reference

### Script usage example (click to move)

```csharp
using NekoLib.Utilities;
using NekoNav.Physics2D;
using UnityEngine;

public sealed class ClickToMove : MonoBehaviour
{
    [SerializeField] private Physics2DAgentMotor _agent;

    private void Update()
    {
        if (_agent == null)
            return;

        if (!Input.GetMouseButtonDown(0))
            return;

        Vector3 world = Utils.GetMousePosition2D();
        _agent.SetDestinationWorld(world);
    }
}
```

### Runtime bake (fast iteration)

If you do not want to create a bake asset yet:

1. Add `Physics2DGridSurface`
2. Leave **Bake Asset** empty
3. Assign **Navigable Tilemap** (and optional **Obstacle Tilemap**) on the surface

The surface will bake in `Awake()`.

## Quick start (BoardGrid)

Use this for dynamic boards (block puzzles, tactics grids, levels where cells change).

### Scene setup

1. Add `BoardGridSurface` to a GameObject (example: `BoardSurface`)
2. Configure:
   - **Plane**: XY (2D) or XZ (3D)
   - **Origin**: world origin of the grid
   - **Cell Size**: size of a grid cell in world units

3. Add `BoardGridAgentMotor` to an agent GameObject
4. Assign the `BoardGridSurface` reference

### Script usage example (define valid cells + block/unblock)

```csharp
using System.Collections.Generic;
using NekoNav.BoardGrid;
using UnityEngine;

public sealed class BoardBootstrap : MonoBehaviour
{
    [SerializeField] private BoardGridSurface _surface;

    private void Start()
    {
        if (_surface == null)
            return;

        // Define all valid cells (example: 10x8).
        var valid = new List<Vector2Int>();
        for (int y = 0; y < 8; y++)
            for (int x = 0; x < 10; x++)
                valid.Add(new Vector2Int(x, y));

        _surface.SetValidCells(valid);

        // Mark a few blocked cells.
        _surface.SetBlockedCells(new List<Vector2Int>
        {
            new Vector2Int(4, 3),
            new Vector2Int(4, 4),
            new Vector2Int(4, 5),
        });

        // Toggle a single cell.
        _surface.SetCellBlocked(new Vector2Int(5, 5), blocked: true);
    }
}
```

### Script usage example (valid cells from transforms)

```csharp
using System.Collections.Generic;
using NekoNav.BoardGrid;
using UnityEngine;

public sealed class ValidCellsFromMarkers : MonoBehaviour
{
    [SerializeField] private BoardGridSurface _surface;
    [SerializeField] private List<Transform> _markers;

    private void Start()
    {
        if (_surface == null)
            return;

        // Converts marker positions to grid cells using BoardGridSurface.TryWorldToCell.
        _surface.SetValidCells(_markers);
    }
}
```

### Script usage example (move an agent)

```csharp
using NekoNav.BoardGrid;
using UnityEngine;

public sealed class BoardMoveExample : MonoBehaviour
{
    [SerializeField] private BoardGridAgentMotor _agent;

    private void Start()
    {
        if (_agent == null)
            return;

        _agent.SetDestinationCell(new Vector2Int(9, 7));
    }
}
```

## API (quick reference)

### IGridSurface

- `TryWorldToCell(world, out GridPos cell)`
- `CellToWorldCenter(cell)`
- `TryFindPath(start, goal, out GridPath path)`
- `Version` (incremented when the surface rebakes)

### BoardGridSurface

- `SetValidCells(IReadOnlyList<Vector2Int> cells)`
- `SetValidCells(IEnumerable<Transform> transforms)`
- `SetValidCells(IEnumerable<GameObject> gameObjects)`
- `SetValidCellsFromWorldPositions(IEnumerable<Vector3> worldPositions)`
- `SetBlockedCells(IReadOnlyList<Vector2Int> cells)`
- `SetCellBlocked(Vector2Int cell, bool blocked)`

### Physics2DGridSurface

- `BakeAsset` (optional) + `LoadBake(GridBakeAsset asset)`
- `BakeFromTilemaps(Tilemap navigableTilemap, Tilemap obstacleTilemap)`
- `ApplyClearance()`

## Requirements

- Unity 2021.3+
- NekoLib
