using System.IO;
using System.IO.Compression;
using GZipArchiver.Lib.IO;

namespace GZipArchiver.Lib
{
    public class FileCompressor : BaseFileArchiver
    {
        public FileCompressor(int? threadCount, int? bufferSize, int? inputQueueCapacity, int? outputQueueCapacity) 
            : base(CompressionMode.Compress, threadCount, bufferSize, inputQueueCapacity, outputQueueCapacity)
        {
        }

        internal override IFileReader CreateFileReader(FileStream inputFileStream, int bufferSize)
        {
            return new RawFileReader(inputFileStream, bufferSize);
        }

        internal override IFileWriter CreateFileWriter(FileStream outputFileStream)
        {
            return new CompressedFileWriter(outputFileStream);
        }
    }
}