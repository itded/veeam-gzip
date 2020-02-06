using GZipArchiver.Lib.Operations;

namespace GZipArchiver.Lib.Processors
{
    interface IProcessor
    {
        OperationResult Process();

        void CancelAllOperations();
    }
}
