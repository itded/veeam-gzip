namespace GZipArchiver.Lib.IO
{
    internal interface IFileReader
    {
        int GetIterationCount();

        FileChunkItem ReadFileChunk();
    }
}
