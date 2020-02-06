using System;
using System.IO;

namespace GZipArchiver.App.Helpers
{
    public static class InputValidator
    {
        public static void StringReadValidation(string[] args)
        {
            bool invalidArgLen = args.Length != 3;
            if (invalidArgLen)
            {
                ThrowUnknownCommandException();
            }

            bool isCompress = args[0].ToLower() == "compress";
            bool isDecompress = args[0].ToLower() == "decompress";
            bool invalidMode = !isCompress && !isDecompress;
            if (invalidMode)
            {
                ThrowUnknownCommandException();
            }

            string inputFileName = args[1];
            string outputFileName = args[2];

            if (inputFileName.Length == 0)
            {
                if (isCompress)
                {
                    throw new ApplicationException("Original file name is empty.");
                }

                if (isDecompress)
                {
                    throw new ApplicationException("Archive file name is empty.");
                }
            }

            if (outputFileName.Length == 0)
            {
                if (isCompress)
                {
                    throw new ApplicationException("Archive file name is empty.");
                }

                if (isDecompress)
                {
                    throw new ApplicationException("Decompressing file name is empty.");
                }
            }

            if (!File.Exists(inputFileName))
            {
                if (isCompress)
                {
                    throw new ApplicationException("Original file was not found.");
                }

                if (isDecompress)
                {
                    throw new ApplicationException("Decompressing file was not found.");
                }
            }

            string inputFullName = new FileInfo(inputFileName).FullName;
            string outputFullName = new FileInfo(outputFileName).FullName;
            if (inputFullName.Equals(outputFullName, StringComparison.InvariantCultureIgnoreCase))
            {
                if (isCompress)
                {
                    throw new ApplicationException("The original file references to the archive file.");
                }

                if (isDecompress)
                {
                    throw new ApplicationException("The archive file references to references to the decompressing file.");
                }
            }
        }

        private static void ThrowUnknownCommandException()
        {
            string fileName = Path.GetFileName(System.Reflection.Assembly.GetEntryAssembly()?.Location ?? "GZipTest.exe");
            throw new ApplicationException($"Unknown command.\nPlease use one of the next commands:\n" +
                                           $"{fileName} compress [original file name] [archive file name]\n" +
                                           $"{fileName} decompress [archive file name] [decompressing file name]");
        }
    }
}
