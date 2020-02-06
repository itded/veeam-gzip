namespace GZipArchiver.Lib.IO
{
    internal class FileChunkItem : IIndexedItem
    {
        public int Index { get; set; }

        public byte[] Content { get; set; }

        public int Length { get; set; }

        public static int SizeOfLength => sizeof(int);

        public override string ToString()
        {
            return Index.ToString();
        }
    }
}
