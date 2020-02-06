using System;
using System.IO;

namespace GZipArchiver.Lib.IO
{
    internal class CompressedFileWriter : IFileWriter
    {
        private readonly FileStream _outputStream;

        public CompressedFileWriter(FileStream outputStream)
        {
            if (outputStream == null)
            {
                throw new ArgumentException(nameof(outputStream));
            }

            _outputStream = outputStream;
        }

        public void WriteFileChunk(FileChunkItem item)
        {
            var lenBytes = BitConverter.GetBytes(item.Length);
            _outputStream.Write(lenBytes, 0, lenBytes.Length);
            _outputStream.Write(item.Content, 0, item.Length);
            _outputStream.Flush();
        }

        public void WriteIterationCount(int iterationCount)
        {
            var countBytes = BitConverter.GetBytes(iterationCount);
            _outputStream.Write(countBytes, 0, countBytes.Length);
            _outputStream.Flush();
        }
    }
}
