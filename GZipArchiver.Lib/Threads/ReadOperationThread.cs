using System;
using GZipArchiver.Lib.Enums;
using GZipArchiver.Lib.Operations;

namespace GZipArchiver.Lib.Threads
{
    internal class ReadOperationThread : BaseOperationThread
    {
        private readonly ReadFileChunksOperation _readFileChunksOperation;

        public ReadOperationThread(ReadFileChunksOperation readFileChunksOperation)
        {
            _readFileChunksOperation = readFileChunksOperation;
        }

        protected override void Execute()
        {
            OperationResult result = null;
            try
            {
                BeforeExecution?.Invoke();

                result = _readFileChunksOperation.Execute();
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
