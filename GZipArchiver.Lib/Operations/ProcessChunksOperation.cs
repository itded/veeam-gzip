using System;
using System.Security.Cryptography;
using GZipArchiver.Lib.Collections;
using GZipArchiver.Lib.IO;
using GZipArchiver.Lib.Processors;

namespace GZipArchiver.Lib.Operations
{
    internal class ProcessChunksOperation : BaseOperation
    {
        private readonly SyncQueue<FileChunkItem> _inputFileChunks;
        private readonly SyncTapeArray<FileChunkItem> _outputFileChunks;
        private readonly IWorkItem _workItem;

        private readonly object _operationLocker = new object();

        private readonly OperationContext _context;

        public ProcessChunksOperation(SyncQueue<FileChunkItem> inputFileChunks,
            SyncTapeArray<FileChunkItem> outputFileChunks,
            IWorkItem workItem, OperationContext context)
        {
            _inputFileChunks = inputFileChunks;
            _outputFileChunks = outputFileChunks;
            _workItem = workItem;
            _context = context;
        }

        public void Cancel()
        {
            if (IsOperationCancelled)
            {
                return;
            }

            lock (_operationLocker)
            {
                IsOperationCancelled = true;
            }
        }

        protected override void ExecuteInner()
        {
            FileChunkItem inputItem = null;
            FileChunkItem outputItem = null;
            bool dequeueNextItem = true;
            bool processNextItem = true;
            bool addNextItem = true;

            while (_context.ProcessCount > 0 && !IsOperationCancelled)
            {
                // 3 step iteration = dequeuing, processing and adding of processed item
                if (dequeueNextItem)
                {
                    processNextItem = _inputFileChunks.TryDequeue(out inputItem, Timeouts.CollectionTimeout);
                    addNextItem = false;
                }

                if (processNextItem)
                {
                    outputItem = ProcessFileChunkItem(inputItem);
                    _context.DecreaseProcessCount();
                    addNextItem = true;
                }

                if (addNextItem)
                {
                    bool addResult =
                        _outputFileChunks.TryAddToArray(outputItem, outputItem.Index, Timeouts.CollectionTimeout);
                    dequeueNextItem = addResult;
                    processNextItem = addResult;
                }
            }
        }

        private FileChunkItem ProcessFileChunkItem(FileChunkItem fileChunk)
        {
            FileChunkItem outputItem = _workItem.Process(fileChunk);

            if (outputItem == null)
            {
                throw new InvalidOperationException("Empty file chunk is not expected.");
            }

            return outputItem;
        }
    }
}
