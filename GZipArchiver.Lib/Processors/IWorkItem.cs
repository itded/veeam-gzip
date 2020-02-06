using System;
using GZipArchiver.Lib.IO;

namespace GZipArchiver.Lib.Processors
{
    internal interface IWorkItem : IDisposable
    {
        FileChunkItem Process(FileChunkItem item);
    }
}
