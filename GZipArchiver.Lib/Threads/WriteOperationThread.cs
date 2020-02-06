using System;
using GZipArchiver.Lib.Enums;
using GZipArchiver.Lib.Operations;

namespace GZipArchiver.Lib.Threads
{
    internal class WriteOperationThread : BaseOperationThread
    {
        private readonly WriteFileChunksOperation _writeFileChunksOperation;

        public WriteOperationThread(WriteFileChunksOperation writeFileChunksOperation)
        {
            _writeFileChunksOperation = writeFileChunksOperation;
        }

        protected override void Execute()
        {
            OperationResult result = null;
            try
            {
                BeforeExecution?.Invoke();

                result = _writeFileChunksOperation.Execute();
            }
            catch (Exception operationException)
            {
                result = new OperationResult(ProcessState.Error, new[] { operationException.Message });
            }
            finally
            {
                Result = result;
                AfterExecution?.Invoke(result);
            }
        }
    }
}