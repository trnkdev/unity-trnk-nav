using UnityEngine;

namespace NekoNav.BoardGrid
{
    [DisallowMultipleComponent]
    public sealed class BoardGridAgentMotor : MonoBehaviour
    {
        private const float DefaultStoppingDistance = 0.02f;
        private const float MinSqrMove = 0.000001f;

        [SerializeField] private BoardGridSurface _surface;
        [SerializeField] private float _speed = 5f;
        [SerializeField] private float _stoppingDistance = DefaultStoppingDistance;
        [SerializeField] private float _turnSpeed = 720f;

        private GridPath _path;
        private int _pathIndex;

        public Vector3 MoveDirectionWorld { get; private set; }
        public Vector2 MoveDirection2D { get; private set; }
        public bool IsMoving { get; private set; }

        public void SetDestinationCell(Vector2Int targetCell)
        {
            if (_surface == null) return;
            if (!_surface.TryWorldToCell(transform.position, out GridPos start)) return;

            GridPos goal = new(targetCell.x, targetCell.y);
            if (_surface.TryFindPath(start, goal, out _path))
                _pathIndex = 0;
            else
                _pathIndex = int.MaxValue;
        }

        private void Update()
        {
            if (_surface == null || !_path.IsValid || _pathIndex >= _path.Count)
            {
                ClearMotion();
                return;
            }

            Vector3 target = _surface.CellToWorldCenter(_path[_pathIndex]);
            Vector3 pos = transform.position;

            Vector3 to = target - pos;
            float dist = to.magnitude;

            if (dist <= _stoppingDistance)
            {
                _pathIndex++;
                if (_pathIndex >= _path.Count)
                {
                    ClearMotion();
                    return;
                }

                target = _surface.CellToWorldCenter(_path[_pathIndex]);
                to = target - pos;
            }

            Vector3 dir = to.sqrMagnitude > MinSqrMove ? to.normalized : Vector3.zero;

            IsMoving = dir != Vector3.zero;
            MoveDirectionWorld = dir;

            MoveDirection2D = _surface.Plane == GridPlane.XY
                ? new Vector2(dir.x, dir.y)
                : new Vector2(dir.x, dir.z);

            transform.position = pos + dir * (_speed * Time.deltaTime);

            if (_surface.Plane == GridPlane.XZ && dir != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(new Vector3(dir.x, 0f, dir.z));
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, _turnSpeed * Time.deltaTime);
            }
        }

        private void ClearMotion()
        {
            IsMoving = false;
            MoveDirectionWorld = Vector3.zero;
            MoveDirection2D = Vector2.zero;
        }
    }
}
