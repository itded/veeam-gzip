using System;
using System.IO;

namespace GZipArchiver.Lib.IO
{
    internal class CompressedFileReader : IFileReader
    {
        private readonly FileStream _inputStream;
        private byte[] _buffer;
        private readonly byte[] _chunkSizeBuffer;

        public CompressedFileReader(FileStream inputStream, int bufferSize)
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
            _buffer = new byte[bufferSize];
            _chunkSizeBuffer = new byte[FileChunkItem.SizeOfLength];
        }

        public int GetIterationCount()
        {
            var buffer = new byte[sizeof(int)];
            int readCount = _inputStream.Read(buffer, 0 , buffer.Length);
            if (readCount == 0)
            {
                throw new ArgumentException("Empty iteration block.");
            }
            return BitConverter.ToInt32(buffer, 0);
        }

        public FileChunkItem ReadFileChunk()
        {
            int length = _inputStream.Read(_chunkSizeBuffer, 0, _chunkSizeBuffer.Length);

            if (length > 0)
            {
                int readCount = BitConverter.ToInt32(_chunkSizeBuffer, 0);

                // extend buffer
                if (readCount > _buffer.Length)
                {
                    _buffer = new byte[readCount];
                }

                _inputStream.Read(_buffer, 0, readCount);
                byte[] content = new byte[readCount];
                Array.Copy(_buffer, content, readCount);

                FileChunkItem item = new FileChunkItem()
                {
                    Content = content,
                    Length = readCount
                };

                return item;
            }

            return null;
        }
    }
}
