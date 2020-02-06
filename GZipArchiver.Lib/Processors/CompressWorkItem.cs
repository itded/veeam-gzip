using System.IO;
using System.IO.Compression;
using GZipArchiver.Lib.IO;

namespace GZipArchiver.Lib.Processors
{
    internal class CompressWorkItem : IWorkItem
    {
        private MemoryStream _workMemoryStream = null;

        private bool _inited = false;

        public CompressWorkItem()
        {
        }

        public FileChunkItem Process(FileChunkItem item)
        {
            if (!_inited)
            {
                _inited = true;
                _workMemoryStream = new MemoryStream();
            }
            else
            {
                // clear internal buffer.
                // gzip stream does not support this operation,
                // so we do not reuse it.
                _workMemoryStream.SetLength(0);
            }

            GZipStream workGZipStream = null;
            try
            {
                workGZipStream = new GZipStream(_workMemoryStream, CompressionMode.Compress, true);
                workGZipStream.Write(item.Content, 0, item.Length);
            }
            catch
            {
                return null;
            }
            finally
            {
                workGZipStream?.Dispose();
            }

            var result = _workMemoryStream.ToArray();

            return new FileChunkItem()
            {
                Index = item.Index,
                Content = result,
                Length = result.Length
            };
        }

        public void Dispose()
        {
            _workMemoryStream?.Dispose();
        }
    }
}
