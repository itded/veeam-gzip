using System.Collections.Generic;
using System.Linq;
using GZipArchiver.Lib.Enums;

namespace GZipArchiver.Lib.Operations
{
    public class OperationResult
    {
        public ProcessState OperationState { get; }

        public string[] Errors { get; }

        public OperationResult(ProcessState operationState, IEnumerable<string> errors)
        {
            OperationState = operationState;
            Errors = errors != null ? errors.ToArray() : new string[] { };
        }
    }
}
