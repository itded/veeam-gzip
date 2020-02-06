using System;
using System.IO;
using GZipArchiver.Lib.Enums;
using GZipArchiver.Lib.Tests.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GZipArchiver.Lib.Tests
{
    [TestClass]
    public class LargeFileCompressionTests
    {
        private const string InputCompressionDir = "CompressionLarge";
        private const string OutputCompressionDir = InputCompressionDir + "Output";

        private FileCompressor _fileCompressor = null;
        private FileDecompressor _fileDecompressor = null;

        [TestInitialize]
        public void Init()
        {
            foreach (string dir in new[] {OutputCompressionDir, InputCompressionDir})
            {
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
            }

            _fileCompressor = new FileCompressor(Environment.ProcessorCount, 1024 * 1024, 100, 100);
            _fileDecompressor = new FileDecompressor(Environment.ProcessorCount, 1024 * 1024, 100, 100);
        }

        /// <summary>
        /// Please copy any file to CompressionLarge folder.
        /// </summary>
        [TestMethod]
        public void CompressAndDecompressSmallFiles()
        {
            var fileNames = Directory.EnumerateFiles(InputCompressionDir);
            foreach (var fileName in fileNames)
            {
                FileInfo inputFileInfo = new FileInfo(fileName);
                FileInfo compressedFileInfo = new FileInfo(Path.Combine(OutputCompressionDir, Path.GetFileNameWithoutExtension(fileName) + ".vgz"));
                FileInfo decompressedFileInfo = new FileInfo(Path.Combine(OutputCompressionDir, Path.GetFileNameWithoutExtension(fileName) + ".dat"));

                var compressResultState = _fileCompressor.Process(inputFileInfo, compressedFileInfo);

                // write operation must insert iteration count
                Assert.AreNotEqual(0, compressedFileInfo.Length);
                Assert.AreEqual(ProcessState.Finished, compressResultState.OperationState);

                var decompressResultState = _fileDecompressor.Process(compressedFileInfo, decompressedFileInfo);

                Assert.AreEqual(ProcessState.Finished, decompressResultState.OperationState);

                bool compareContentResult =
                    FileContentComparator.AreFileContentEqual(inputFileInfo, decompressedFileInfo);
                Assert.AreEqual(true, compareContentResult);
            }
        }
    }
}