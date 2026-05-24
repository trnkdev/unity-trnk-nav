using System.Collections;
using System.Collections.Generic;

namespace TRnK.Nav
{
    public readonly struct GridPath : IEnumerable<GridPos>
    {
        internal readonly GridPos[] Cells;
        public readonly int Count;

        internal GridPath(GridPos[] cells, int count)
        {
            Cells = cells;
            Count = count;
        }

        public bool IsValid => Cells != null && Count > 0;

        public GridPos this[int index] => Cells[index];

        public Enumerator GetEnumerator() => new(Cells, Count);
        IEnumerator<GridPos> IEnumerable<GridPos>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public struct Enumerator : IEnumerator<GridPos>
        {
            private readonly GridPos[] _cells;
            private readonly int _count;
            private int _index;

            internal Enumerator(GridPos[] cells, int count)
            {
                _cells = cells;
                _count = count;
                _index = -1;
            }

            public readonly GridPos Current => _cells[_index];
            readonly object IEnumerator.Current => Current;

            public bool MoveNext()
            {
                _index++;
                return _index < _count;
            }

            public void Reset() => _index = -1;
            public readonly void Dispose() { }
        }
    }
}
