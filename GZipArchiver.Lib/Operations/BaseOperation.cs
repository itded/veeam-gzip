using System;
using GZipArchiver.Lib.Enums;

namespace GZipArchiver.Lib.Operations
{
    internal abstract class BaseOperation
    {
        /// <summary>
        /// Set true to cancel the operation and return a <see cref="ProcessState.Cancelled" /> operation result.
        /// </summary>
        protected bool IsOperationCancelled = false;

        /// <summary>
        /// Execute the operation.
        /// </summary>
        /// <returns>Result of execution.</returns>
        public OperationResult Execute()
        {
            OperationResult result;

            try
            {
                ExecuteInner();
                result = !IsOperationCancelled
                    ? new OperationResult(ProcessState.Finished, null)
                    : new OperationResult(ProcessState.Cancelled, null);
            }
            catch (Exception executeException)
            {
                result = new OperationResult(ProcessState.Error, new[] { executeException.Message });
            }

            return result;
        }

        protected abstract void ExecuteInner();
    }
}
