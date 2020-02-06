namespace GZipArchiver.Lib.IO
{
    internal interface IFileWriter
    {
        /// <summary>
        /// Seeks position in the output stream and writes item to it.
        /// </summary>
        /// <param name="item">Item to be written.</param>
        void WriteFileChunk(FileChunkItem item);

        void WriteIterationCount(int iterationCount);
    }
}
