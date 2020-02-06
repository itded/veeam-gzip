using System.Threading;

namespace GZipArchiver.Lib.Collections
{
    /// <summary>
    /// Represents unlimited tape with <see cref="SyncTapeArray.FrameSize" /> number of visible elements available for writing (the frame).
    /// If the beginning of frame is filled then it is possible to call <see cref="SyncTapeArray.GetNextRange" /> to read elements and move the frame further.
    /// </summary>
    /// <typeparam name="T">Type of element.</typeparam>
    internal class SyncTapeArray<T>
    {
        private readonly object _locker = new object();

        private readonly int _frameArraySize;
        private readonly T[] _frameArray;
        private readonly bool[] _frameHasElementArray;

        private int _allowedItemStart;
        private int _allowedItemEnd;
        private int _readPointerPosition;

        public SyncTapeArray(int frameSize)
        {
            _readPointerPosition = 0;
            _allowedItemStart = 0;
            _allowedItemEnd = frameSize - 1;
            _frameArraySize = frameSize;

            _frameArray = new T[frameSize];
            _frameHasElementArray = new bool[frameSize];
        }

        public int FrameSize => _frameArraySize;

        public bool TryAddToArray(T item, int index, int millisecondsTimeout)
        {
            if (index < _allowedItemStart)
            {
                return false;
            }

            lock (_locker)
            {
                while (index > _allowedItemEnd)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[{nameof(SyncTapeArray<T>).ToUpper()}] {Thread.CurrentThread.Name} waits in {nameof(TryAddToArray)}. Index #{index}.");
                    bool isReacquired = Monitor.Wait(_locker, millisecondsTimeout);
                    if (!isReacquired)
                    {
                        return false;
                    }
                }

                // set item and flag
                int mappedIndex = index % _frameArraySize;
                _frameArray[mappedIndex] = item;
                _frameHasElementArray[mappedIndex] = true;

                // if we want read the current item or we finished in processing
                System.Diagnostics.Debug.WriteLine(
                    $"[{nameof(SyncTapeArray<T>).ToUpper()}] {Thread.CurrentThread.Name} adds to Frame {index}");
                if (_readPointerPosition == index)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[{nameof(SyncTapeArray<T>).ToUpper()}] {Thread.CurrentThread.Name} wakes up other in {nameof(TryAddToArray)}. Index #{index}.");
                    Monitor.PulseAll(_locker);
                }

                return true;
            }
        }

        public bool TryGetNextRange(out T[] range, int millisecondsTimeout)
        {
            lock (_locker)
            {
                // find a sequence to take starting with the current position
                int length = GetSequenceLength();
                while (length == 0)
                {
                    System.Diagnostics.Debug.WriteLine($"[{nameof(SyncTapeArray<T>).ToUpper()}] {Thread.CurrentThread.Name} waits other in {nameof(TryGetNextRange)}.");

                    bool isReacquired = Monitor.Wait(_locker, millisecondsTimeout);
                    if (!isReacquired)
                    {
                        range = null;
                        return false;
                    }

                    length = GetSequenceLength();
                }

                // get sequential located items
                range = new T[length];
                for (int readIndex = 0; readIndex < length; readIndex++)
                {
                    int arrayIndex = (readIndex + _readPointerPosition) % _frameArraySize;
                    // set false flag to the next items on tape
                    range[readIndex] = _frameArray[arrayIndex];
                    _frameHasElementArray[arrayIndex] = false;
                }

                // move the tape and the cursor
                _readPointerPosition += length;
                _allowedItemStart += length;
                _allowedItemEnd += length;

                Monitor.PulseAll(_locker);

                return true;
            }
        }

        private int GetSequenceLength()
        {
            int length = 0;
            for (int readIndex = _readPointerPosition; readIndex < _readPointerPosition + _frameArraySize; readIndex++)
            {
                int arrayIndex = readIndex % _frameArraySize;
                if (_frameHasElementArray[arrayIndex])
                {
                    length++;
                }
                else
                {
                    break;
                }
            }

            return length;
        } 
    }
}
