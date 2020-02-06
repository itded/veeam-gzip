using System.IO;
using System.IO.Compression;
using GZipArchiver.Lib.IO;

namespace GZipArchiver.Lib
{
    public class FileDecompressor : BaseFileArchiver
    {
        public FileDecompressor(int? threadCount, int? bufferSize, int? inputQueueCapacity, int? outputQueueCapacity) 
            : base(CompressionMode.Decompress, threadCount, bufferSize, inputQueueCapacity, outputQueueCapacity)
        {
        }

        internal override IFileReader CreateFileReader(FileStream inputFileStream, int bufferSize)
        {
            return new CompressedFileReader(inputFileStream, bufferSize);
        }

        internal override IFileWriter CreateFileWriter(FileStream outputFileStream)
        {
            return new RawFileWriter(outputFileStream);
        }
    }
}