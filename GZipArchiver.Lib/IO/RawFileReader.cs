using System;
using System.IO;

namespace GZipArchiver.Lib.IO
{
    internal class RawFileReader : IFileReader
    {
        private readonly FileStream _inputStream;
        private readonly byte[] _buffer;
        private readonly int _bufferSize;

        public RawFileReader(FileStream inputStream, int bufferSize)
        {
            if (inputStream == null)
            {
                throw new ArgumentException(nameof(inputStream));
            }

            if (bufferSize <= 0)
            {
                throw new ArgumentException(nameof(bufferSize));
            }

            _inputStream = inputStream;
            _bufferSize = bufferSize;
            _buffer = new byte[bufferSize];
        }

        public int GetIterationCount()
        {
            long fileLen = _inputStream.Length;
            int iterationCount = Convert.ToInt32(fileLen / _bufferSize) +
                                 (fileLen % _bufferSize == 0 ? 0 : 1);
            return iterationCount;
        }

        public FileChunkItem ReadFileChunk()
        {
            int length = _inputStream.Read(_buffer, 0, _bufferSize);

            if (length > 0)
            {
                byte[] content = new byte[length];
                Array.Copy(_buffer, content, length);
                FileChunkItem item = new FileChunkItem()
                {
                    Content = content,
                    Length = length
                };

                return item;
            }

            return null;
        }
    }
}
