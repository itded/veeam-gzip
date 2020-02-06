using System;
using System.IO;
using System.IO.Compression;
using GZipArchiver.Lib.Enums;
using GZipArchiver.Lib.IO;
using GZipArchiver.Lib.Operations;
using GZipArchiver.Lib.Processors;

namespace GZipArchiver.Lib
{
    public abstract class BaseFileArchiver
    {
        private const int BufferSize = 1024 * 1024; //// 1 Mb
        private const int InputQueueCapacity = 100; //// TODO: varies on available memory
        private const int OutputQueueCapacity = 100;

        private readonly CompressionMode _compressionMode;
        private readonly int _processThreadCount;
        private readonly int _bufferSize;
        private readonly int _inputQueueCapacity;
        private readonly int _outputQueueCapacity;

        public BaseFileArchiver(CompressionMode compressionMode, int? threadCount, int? bufferSize, int? inputQueueCapacity, int? outputQueueCapacity)
        {
            _compressionMode = compressionMode;
            _processThreadCount = threadCount ?? Environment.ProcessorCount;
            _bufferSize = bufferSize ?? BufferSize;
            _inputQueueCapacity = inputQueueCapacity ?? InputQueueCapacity;
            _outputQueueCapacity = outputQueueCapacity ?? OutputQueueCapacity;
        }

        public OperationResult Process(FileInfo inputFileInfo, FileInfo outputFileInfo)
        {
            System.Diagnostics.Debug.WriteLine(string.Format("[INFO] {0} Archiver - Process. Threads {1}, Buffer {2}, Input {3}, Output {4}", 
                GetType().Name, _processThreadCount, _bufferSize, _inputQueueCapacity, _outputQueueCapacity));

            FileStream inputFileStream = null;
            FileStream outputFileStream = null;
            FileProcessor processor = null;
            OperationResult processResult = null;

            try
            {
                inputFileStream = inputFileInfo.OpenRead();
                outputFileStream = outputFileInfo.OpenWrite();

                IFileReader fileReader = CreateFileReader(inputFileStream, _bufferSize);
                IFileWriter fileWriter = CreateFileWriter(outputFileStream);
                processor = new FileProcessor(fileReader, fileWriter, _compressionMode, _processThreadCount,
                    _inputQueueCapacity, _outputQueueCapacity);
                processResult = processor.Process();

            }
            catch (Exception processException)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] {processException.Message}");
                processResult = new OperationResult(ProcessState.Error, new[] { processException.Message});
            }
            finally
            {
                System.Diagnostics.Debug.WriteLine(string.Format("[INFO] {0} Archiver - Process Finished",
                    GetType().Name));

                processor?.Dispose();
                inputFileStream?.Dispose();
                outputFileStream?.Dispose();
            }

            return processResult;
        }

        internal abstract IFileReader CreateFileReader(FileStream inputFileStream, int bufferSize);

        internal abstract IFileWriter CreateFileWriter(FileStream outputFileStream);
    }
}
