using System;
using GZipArchiver.Lib.Collections;
using GZipArchiver.Lib.IO;

namespace GZipArchiver.Lib.Operations
{
    internal class ReadFileChunksOperation : BaseOperation
    {
        private readonly IFileReader _inputFileReader;
        private readonly SyncQueue<FileChunkItem> _inputFileChunks;
        private readonly OperationContext _context;

        private readonly object _operationLocker = new object();

        public ReadFileChunksOperation(IFileReader inputFileReader, SyncQueue<FileChunkItem> inputFileChunks, OperationContext context)
        {
            _inputFileReader = inputFileReader;
            _inputFileChunks = inputFileChunks;
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
            int readCount = _context.ReadCount;

            int currentStep = 0;
            FileChunkItem lastChunk = null;
            bool readNextChunk = true;

            while (currentStep < readCount && !IsOperationCancelled)
            {
                // execute iteration - read a chunk and try to enqueue it
                if (readNextChunk)
                {
                    lastChunk = ReadFileChunk();
                    lastChunk.Index = currentStep;
                }

                readNextChunk = _inputFileChunks.TryEnqueue(lastChunk, Timeouts.CollectionTimeout);

                if (readNextChunk)
                {
                    currentStep++;
                }
            }
        }

        private FileChunkItem ReadFileChunk()
        {
            // try to read a new file chunk
            FileChunkItem fileChunk = _inputFileReader.ReadFileChunk();
            if (fileChunk == null)
            {
                throw new InvalidOperationException("Empty file chunk is not expected.");
            }

            return fileChunk;
        }
    }
}
