using System;
using GZipArchiver.Lib.Collections;
using GZipArchiver.Lib.IO;

namespace GZipArchiver.Lib.Operations
{
    internal class WriteFileChunksOperation : BaseOperation
    {
        private readonly IFileWriter _outputFileWriter;
        private readonly SyncTapeArray<FileChunkItem> _outputFileChunks;
        private readonly OperationContext _context;

        private readonly object _operationLocker = new object();

        public WriteFileChunksOperation(IFileWriter outputFileWriter, SyncTapeArray<FileChunkItem> outputFileChunks, OperationContext context)
        {
            _outputFileWriter = outputFileWriter;
            _outputFileChunks = outputFileChunks;
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
            int writeCount = _context.WriteCount;

            _outputFileWriter.WriteIterationCount(writeCount);

            while (writeCount > 0 && !IsOperationCancelled)
            {
                // execute iteration - try to get a next range and write it

                FileChunkItem[] lastFileChunks;
                var writeNextChunks = _outputFileChunks.TryGetNextRange(out lastFileChunks, Timeouts.CollectionTimeout);

                if (writeNextChunks)
                {
                    WriteChunkRange(lastFileChunks);
                    writeCount -= lastFileChunks.Length;
                }
            }
        }

        private void WriteChunkRange(FileChunkItem[] fileChunks)
        {
            if (fileChunks == null || fileChunks.Length == 0)
            {
                throw new InvalidOperationException("Empty file chunks are not expected.");
            }

            foreach (var fileChunk in fileChunks)
            {
                if (fileChunk == null)
                {
                    throw new InvalidOperationException("Empty file chunk is not expected.");
                }

                // try to write a new file chunk
                _outputFileWriter.WriteFileChunk(fileChunk);
            }
        }
    }
}
