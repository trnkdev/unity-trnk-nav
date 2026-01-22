using NekoLib.Extensions;
using NekoNav.Internals.Smoothing;
using UnityEngine;

namespace NekoNav.Physics2D
{
    [RequireComponent(typeof(Rigidbody2D))]
    [DisallowMultipleComponent]
    public sealed class Physics2DAgentMotor : MonoBehaviour
    {
        private const float DefaultStoppingDistance = 0.05f;
        private const float MinSqrMove = 0.000001f;

        [SerializeField] private Physics2DGridSurface _surface;
        [SerializeField] private float _speed = 3.5f;
        [SerializeField] private float _stoppingDistance = DefaultStoppingDistance;

        private Rigidbody2D _rb;

        private GridPath _gridPath;
        private GridPos[] _tmpA;
        private GridPos[] _tmpB;

        private Vector2[] _waypoints;
        private int _waypointCount;
        private int _waypointIndex;

        public Vector2 MoveDirection2D { get; private set; }
        public bool IsMoving { get; private set; }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.bodyType = RigidbodyType2D.Kinematic;
        }

        public void SetDestinationWorld(Vector3 world)
        {
            if (_surface == null) return;

            if (!_surface.TryWorldToCell(transform.position, out GridPos start)) return;
            if (!_surface.TryWorldToCell(world, out GridPos goal)) return;

            if (_surface.TryFindPath(start, goal, out _gridPath))
            {
                BuildWaypoints(_gridPath);
                _waypointIndex = 0;
            }
            else
            {
                _waypointCount = 0;
                _waypointIndex = 0;
            }
        }

        private void BuildWaypoints(GridPath path)
        {
            int count = path.Count;
            EnsureTmp(ref _tmpA, count);
            EnsureTmp(ref _tmpB, count);

            for (int i = 0; i < count; i++)
                _tmpA[i] = path[i];

            int aCount = PathSmoother.SimplifyCollinear(_tmpA, count, _tmpB);
            var grid = _surface.GetGridDataUnsafe();
            int bCount = PathSmoother.ShortcutLineOfSight(grid, _tmpB, aCount, _tmpA);

            EnsureWaypoints(bCount);
            _waypointCount = bCount;

            for (int i = 0; i < bCount; i++)
                _waypoints[i] = _surface.CellToWorldCenter(_tmpA[i]);
        }

        private static void EnsureTmp(ref GridPos[] arr, int size)
        {
            if (arr.IsNullOrEmpty() || arr.Length < size)
                arr = new GridPos[size];
        }

        private void EnsureWaypoints(int size)
        {
            if (_waypoints.IsNullOrEmpty() || _waypoints.Length < size)
                _waypoints = new Vector2[size];
        }

        private void FixedUpdate()
        {
            if (_surface == null || _waypointIndex >= _waypointCount)
            {
                ClearMotion();
                return;
            }

            Vector2 pos = _rb.position;
            Vector2 target = _waypoints[_waypointIndex];

            Vector2 to = target - pos;
            float dist = to.magnitude;

            if (dist <= _stoppingDistance)
            {
                _waypointIndex++;
                if (_waypointIndex >= _waypointCount)
                {
                    ClearMotion();
                    return;
                }

                target = _waypoints[_waypointIndex];
                to = target - pos;
            }

            Vector2 dir = to.sqrMagnitude > MinSqrMove ? to.normalized : Vector2.zero;

            IsMoving = dir != Vector2.zero;
            MoveDirection2D = dir;

            Vector2 next = pos + dir * (_speed * Time.fixedDeltaTime);
            _rb.MovePosition(next);
        }

        private void ClearMotion()
        {
            IsMoving = false;
            MoveDirection2D = Vector2.zero;
        }
    }
}
