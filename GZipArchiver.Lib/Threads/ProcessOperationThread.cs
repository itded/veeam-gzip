using System;
using GZipArchiver.Lib.Enums;
using GZipArchiver.Lib.Operations;

namespace GZipArchiver.Lib.Threads
{
    internal class ProcessOperationThread : BaseOperationThread
    {
        private readonly ProcessChunksOperation _processChunksOperation;

        public ProcessOperationThread(ProcessChunksOperation readFileChunksOperation)
        {
            _processChunksOperation = readFileChunksOperation;
        }

        protected override void Execute()
        {
            OperationResult result = null;
            try
            {
                BeforeExecution?.Invoke();

                result = _processChunksOperation.Execute();
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