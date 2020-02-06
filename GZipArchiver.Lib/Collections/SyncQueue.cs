using System.Collections.Generic;
using System.Threading;

namespace GZipArchiver.Lib.Collections
{
    internal class SyncQueue<T>
    {
        private readonly int _maxCapacity;
        private readonly Queue<T> _queue;
        private readonly object _locker = new object();

        public SyncQueue(int maxCapacity)
        {
            _maxCapacity = maxCapacity;
            _queue = new Queue<T>(maxCapacity);
        }

        public bool TryEnqueue(T item, int millisecondsTimeout)
        {
            lock (_locker)
            {
                while (_queue.Count >= _maxCapacity)
                {
                    bool isReacquired = Monitor.Wait(_locker, millisecondsTimeout);
                    if (!isReacquired)
                    {
                        return false;
                    }
                }

                _queue.Enqueue(item);
                if (_queue.Count == 1)
                {
                    Monitor.PulseAll(_locker);
                }

                return true;
            }
        }

        public bool TryDequeue(out T item, int millisecondsTimeout)
        {
            lock (_locker)
            {
                while (_queue.Count == 0)
                {
                    bool isReacquired = Monitor.Wait(_locker, millisecondsTimeout);
                    if (!isReacquired)
                    {
                        item = default(T);
                        return false;
                    }
                }

                item = _queue.Dequeue();

                if (_queue.Count == _maxCapacity - 1)
                {
                    Monitor.PulseAll(_locker);
                }

                return true;
            }
        }

        public int Count => _queue.Count;
    }
}
