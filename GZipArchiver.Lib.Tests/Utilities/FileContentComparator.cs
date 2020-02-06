using System;
using System.IO;

namespace GZipArchiver.Lib.Tests.Utilities
{
    public static class FileContentComparator
    {
        private const int BufferSize = sizeof(long);

        public static bool AreFileContentEqual(FileInfo file1, FileInfo file2)
        {
            if (file1.Length != file2.Length)
            {
                return false;
            }

            long fileLen = file1.Length;
            if (fileLen == 0)
            {
                return true;
            }

            int iterationCount = Convert.ToInt32(fileLen / BufferSize) +
                                 (fileLen % BufferSize == 0 ? 0 : 1);

            byte[] buffer1 = new byte[BufferSize];
            byte[] buffer2 = new byte[BufferSize];

            using (FileStream fileStream1 = file1.OpenRead())
            {
                using (FileStream fileStream2 = file2.OpenRead())
                {
                    for (int i = 0; i < iterationCount; i++)
                    {
                        fileStream1.Read(buffer1, 0, BufferSize);
                        fileStream2.Read(buffer2, 0, BufferSize);

                        long l1 = BitConverter.ToInt64(buffer1, 0);
                        long l2 = BitConverter.ToInt64(buffer2, 0);

                        if (l1 != l2)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }
    }
}
