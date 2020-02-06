using System;
using System.IO.Compression;
using GZipArchiver.Lib.Processors;

namespace GZipArchiver.Lib.Factories
{
    internal class WorkItemFactory: IWorkItemFactory
    {
        public IWorkItem CreateWorkItem(CompressionMode compressionMode)
        {
            switch (compressionMode)
            {
                case CompressionMode.Compress:
                {
                    return new CompressWorkItem();
                }

                case CompressionMode.Decompress:
                {
                    return new DecompressWorkItem();
                }

                default:
                {
                    throw new ArgumentException($"{nameof(compressionMode)}. Only {CompressionMode.Compress.ToString()} and {CompressionMode.Decompress} modes are supported.");
                }
            }
        }
    }
}
