using System.IO.Compression;
using GZipArchiver.Lib.Processors;

namespace GZipArchiver.Lib.Factories
{
    internal interface IWorkItemFactory
    {
        IWorkItem CreateWorkItem(CompressionMode compressionMode);
    }
}
