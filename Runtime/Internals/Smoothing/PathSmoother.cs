using UnityEngine;

namespace TRnK.Nav.Internals.Smoothing
{
    internal static class PathSmoother
    {
        private const int MaxLosChecks = 64;
        private const int MaxLosJump = 32;

        public static int SimplifyCollinear(GridPos[] input, int count, GridPos[] output)
        {
            if (count <= 0) return 0;
            if (count <= 2)
            {
                for (int i = 0; i < count; i++) output[i] = input[i];
                return count;
            }

            int outCount = 0;
            output[outCount++] = input[0];

            GridPos prev = input[0];
            Vector2Int prevDir = Vector2Int.zero;

            for (int i = 1; i < count; i++)
            {
                GridPos cur = input[i];
                Vector2Int dir = new(cur.X - prev.X, cur.Y - prev.Y);

                dir.x = Mathf.Clamp(dir.x, -1, 1);
                dir.y = Mathf.Clamp(dir.y, -1, 1);

                if (i == 1)
                {
                    prevDir = dir;
                }
                else
                {
                    if (dir != prevDir)
                    {
                        output[outCount++] = input[i - 1];
                        prevDir = dir;
                    }
                }

                prev = cur;
            }

            output[outCount++] = input[count - 1];
            return outCount;
        }

        public static int ShortcutLineOfSight(GridData grid, GridPos[] input, int count, GridPos[] output)
        {
            if (count <= 0) return 0;
            if (count <= 2)
            {
                for (int i = 0; i < count; i++) output[i] = input[i];
                return count;
            }

            int outCount = 0;
            int i0 = 0;
            output[outCount++] = input[i0];

            int checks = 0;

            while (i0 < count - 1)
            {
                int best = i0 + 1;
                int max = Mathf.Min(count - 1, i0 + MaxLosJump);

                for (int j = max; j > i0 + 1; j--)
                {
                    checks++;
                    if (checks > MaxLosChecks)
                        break;

                    if (GridRaycast.HasLineOfSight(grid, input[i0], input[j]))
                    {
                        best = j;
                        break;
                    }
                }

                i0 = best;
                output[outCount++] = input[i0];

                if (checks > MaxLosChecks)
                    break;
            }

            if (output[outCount - 1] != input[count - 1])
                output[outCount++] = input[count - 1];

            return outCount;
        }
    }
}
