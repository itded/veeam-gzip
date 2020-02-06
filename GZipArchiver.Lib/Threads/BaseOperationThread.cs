using System;
using System.Threading;
using GZipArchiver.Lib.Operations;

namespace GZipArchiver.Lib.Threads
{
    /// <summary>
    /// Thread execution wrapper
    /// </summary>
    internal abstract class BaseOperationThread
    {
        private readonly Thread _thread;

        protected Action BeforeExecution;

        protected Action<OperationResult> AfterExecution;

        public string Name
        {
            get => _thread.Name;
            set => _thread.Name = value;
        }

        protected BaseOperationThread()
        {
            _thread = new Thread(Execute);
        }

        /// <summary>
        /// Performs a callback inside the thread before main execution.
        /// </summary>
        /// <param name="beforeExecution">Callback</param>
        public void SetBeforeStartCallback(Action beforeExecution)
        { 
            BeforeExecution = beforeExecution;
        }

        /// <summary>
        /// Performs a callback inside the thread after main execution.
        /// </summary>
        /// <param name="afterExecution">Callback</param>
        public void SetAfterStartCallback(Action<OperationResult> afterExecution)
        {
            AfterExecution = afterExecution;
        }


        public void Start()
        {
            _thread.Start();
        }

        protected abstract void Execute();

        /// <summary>
        /// Result of the thread execution.
        /// </summary>
        public OperationResult Result
        {
            get;
            protected set;
        }
    }

}
