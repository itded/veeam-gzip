using System;
using System.IO;

namespace GZipArchiver.Lib.IO
{
    internal class RawFileWriter : IFileWriter
    {
        private readonly FileStream _outputStream;

        public RawFileWriter(FileStream outputStream)
        {
            if (outputStream == null)
            {
                throw new ArgumentException(nameof(outputStream));
            }

            _outputStream = outputStream;
        }

        public void WriteFileChunk(FileChunkItem item)
        {
            _outputStream.Write(item.Content, 0, item.Length);
            _outputStream.Flush();
        }

        public void WriteIterationCount(int iterationCount)
        {
            // nothing
        }
    }
}
