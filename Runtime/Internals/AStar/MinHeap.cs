namespace NekoNav.Internals.AStar
{
    internal sealed class MinHeap
    {
        private int[] _nodes;
        private int[] _prio;
        private int _count;

        public int Count => _count;

        public void EnsureCapacity(int capacity)
        {
            if (_nodes == null || _nodes.Length < capacity)
            {
                _nodes = new int[capacity];
                _prio = new int[capacity];
            }

            _count = 0;
        }

        public void Push(int node, int priority)
        {
            int i = _count++;
            _nodes[i] = node;
            _prio[i] = priority;

            while (i > 0)
            {
                int p = (i - 1) >> 1;
                if (_prio[p] <= priority) break;

                Swap(i, p);
                i = p;
            }
        }

        public int Pop(out int priority)
        {
            int rootNode = _nodes[0];
            priority = _prio[0];

            _count--;
            if (_count > 0)
            {
                _nodes[0] = _nodes[_count];
                _prio[0] = _prio[_count];
                HeapifyDown(0);
            }

            return rootNode;
        }

        private void HeapifyDown(int i)
        {
            while (true)
            {
                int left = (i << 1) + 1;
                if (left >= _count) break;

                int right = left + 1;
                int best = (right < _count && _prio[right] < _prio[left]) ? right : left;

                if (_prio[i] <= _prio[best]) break;
                Swap(i, best);
                i = best;
            }
        }

        private void Swap(int a, int b)
        {
            int n = _nodes[a]; _nodes[a] = _nodes[b]; _nodes[b] = n;
            int p = _prio[a]; _prio[a] = _prio[b]; _prio[b] = p;
        }
    }
}
