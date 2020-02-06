using System.IO;
using System.IO.Compression;
using GZipArchiver.Lib.IO;

namespace GZipArchiver.Lib.Processors
{
    internal class DecompressWorkItem : IWorkItem
    {
        private MemoryStream _workMemoryStream = null;
        private MemoryStream _resultMemoryStream = null;

        private bool _inited = false;

        public FileChunkItem Process(FileChunkItem item)
        {
            if (!_inited)
            {
                _inited = true;
                _workMemoryStream = new MemoryStream();
                _resultMemoryStream = new MemoryStream();
            }
            else
            {
                // clear internal buffer.
                // gzip stream does not support this operation,
                // so we do not reuse it.
                _workMemoryStream.SetLength(0);
                _resultMemoryStream.SetLength(0);
            }

            GZipStream workGZipStream = null;
            try
            {
                _workMemoryStream.Write(item.Content, 0, item.Length);
                _workMemoryStream.Seek(0, SeekOrigin.Begin);

                workGZipStream = new GZipStream(_workMemoryStream, CompressionMode.Decompress, true);
                workGZipStream.CopyTo(_resultMemoryStream);
            }
            catch
            {
                return null;
            }
            finally
            {
                workGZipStream?.Dispose();
            }

            var result = _resultMemoryStream.ToArray();

            return new FileChunkItem()
            {
                Index = item.Index,
                Content = result,
                Length = result.Length
            };
        }

        public void Dispose()
        {
            _resultMemoryStream?.Dispose();
            _workMemoryStream?.Dispose();
        }
    }
}
