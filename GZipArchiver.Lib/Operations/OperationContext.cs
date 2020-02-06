namespace GZipArchiver.Lib.Operations
{
    /// <summary>
    /// Shared operation context.
    /// </summary>
    internal class OperationContext
    {
        private readonly object _contextLocker = new object();

        private int _readCount = 0;
        private int _writeCount = 0;
        private int _processCount = 0;
        private bool _isCountSet = false;

        /// <summary>
        /// Counter for Read operations. Do not change after first set.
        /// </summary>
        public int ReadCount => _readCount;

        /// <summary>
        /// Counter for Write operations. Do not change after first set.
        /// </summary>
        public int WriteCount => _writeCount;

        /// <summary>
        /// Counter for Process operations. Decrease only after first set
        /// </summary>
        public int ProcessCount => _processCount;

        /// <summary>
        /// Counters are set.
        /// </summary>
        public bool IsCountSet => _isCountSet;

        /// <summary>
        /// Set counters. Cannot be changed after the first set.
        /// </summary>
        /// <param name="value">Counter value</param>
        /// <returns>Set successfully</returns>
        public bool SetCounters(int value)
        {
            if (_isCountSet)
            {
                return false;
            }

            lock (_contextLocker)
            {
                _readCount = value;
                _writeCount = value;
                _processCount = value;
                _isCountSet = true;
            }

            return true;
        }

        /// <summary>
        /// Decrease the process counter by 1.
        /// The counter cannot be negative.
        /// </summary>
        /// <returns>Set successfully</returns>
        public bool DecreaseProcessCount()
        {
            if (_processCount <= 0)
            {
                return false;
            }

            lock (_contextLocker)
            {
                _processCount--;
            }

            return true;
        }
    }
}
