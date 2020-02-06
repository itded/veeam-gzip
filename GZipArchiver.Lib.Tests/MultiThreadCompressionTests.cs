using System.IO;
using GZipArchiver.Lib.Enums;
using GZipArchiver.Lib.Tests.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GZipArchiver.Lib.Tests
{
    [TestClass]
    public class MultiThreadCompressionTests
    {
        private const string InputCompressionDir = "CompressionSmall";
        private const string OutputCompressionDir = InputCompressionDir + "Output";
        private const string InputDecompressionDir = "DecompressionSmall";
        private const string OutputDecompressionDir = InputDecompressionDir + "Output";

        private FileCompressor _fileCompressor = null;
        private FileDecompressor _fileDecompressor = null;

        [TestInitialize]
        public void Init()
        {
            foreach (string outputDir in new[] { OutputCompressionDir, OutputDecompressionDir })
            {
                if (!Directory.Exists(outputDir))
                {
                    Directory.CreateDirectory(outputDir);
                }
            }

            _fileCompressor = new FileCompressor(4, 4096, 10, 10);
            _fileDecompressor = new FileDecompressor(4, 4096, 10, 10);
        }

        [TestMethod]
        public void CompressEmptyFile()
        {
            FileInfo inputFileInfo = new FileInfo(Path.Combine(InputCompressionDir, "empty.txt"));
            FileInfo outputFileInfo = new FileInfo(Path.Combine(OutputCompressionDir, "empty.vgz"));

            Assert.AreEqual(0, inputFileInfo.Length);

            var resultState = _fileCompressor.Process(inputFileInfo, outputFileInfo);

            // write only iteration size
            Assert.AreEqual(sizeof(int), outputFileInfo.Length);
            Assert.AreEqual(ProcessState.Finished, resultState.OperationState);
        }

        [TestMethod]
        public void CompressOneLetterFile()
        {
            FileInfo inputFileInfo = new FileInfo(Path.Combine(InputCompressionDir, "one-letter.txt"));
            FileInfo outputFileInfo = new FileInfo(Path.Combine(OutputCompressionDir, "one-letter.vgz"));

            Assert.AreEqual(4, inputFileInfo.Length);

            var resultState = _fileCompressor.Process(inputFileInfo, outputFileInfo);

            Assert.AreEqual(32, outputFileInfo.Length);
            Assert.AreEqual(ProcessState.Finished, resultState.OperationState);
        }

        [TestMethod]
        public void CompressPdfFile()
        {
            FileInfo inputFileInfo = new FileInfo(Path.Combine(InputCompressionDir, "quick-guide-gplv3.pdf"));
            FileInfo outputFileInfo = new FileInfo(Path.Combine(OutputCompressionDir, "quick-guide-gplv3.vgz"));

            var resultState = _fileCompressor.Process(inputFileInfo, outputFileInfo);

            Assert.AreNotEqual(0, outputFileInfo.Length);
            Assert.AreEqual(ProcessState.Finished, resultState.OperationState);
        }

        [TestMethod]
        public void CompressSmallFiles()
        {
            var fileNames = Directory.EnumerateFiles(InputCompressionDir);
            foreach (var fileName in fileNames)
            {
                FileInfo inputFileInfo = new FileInfo(fileName);
                FileInfo outputFileInfo = new FileInfo(Path.Combine(OutputCompressionDir, Path.GetFileNameWithoutExtension(fileName) + ".vgz"));

                var resultState = _fileCompressor.Process(inputFileInfo, outputFileInfo);

                // write operation must insert iteration count
                Assert.AreNotEqual(0, outputFileInfo.Length);
                Assert.AreEqual(ProcessState.Finished, resultState.OperationState);
            }
        }

        [TestMethod]
        public void DecompressSmallFile()
        {
            FileInfo inputFileInfo = new FileInfo(Path.Combine(InputDecompressionDir, "one-letter.vgz"));
            FileInfo outputFileInfo = new FileInfo(Path.Combine(OutputDecompressionDir, "one-letter.pdf"));

            var resultState = _fileDecompressor.Process(inputFileInfo, outputFileInfo);

            Assert.AreEqual(4, outputFileInfo.Length);
            Assert.AreEqual(ProcessState.Finished, resultState.OperationState);
        }

        [TestMethod]
        public void DecompressPdfFile()
        {
            FileInfo inputFileInfo = new FileInfo(Path.Combine(InputDecompressionDir, "quick-guide-gplv3.vgz"));
            FileInfo outputFileInfo = new FileInfo(Path.Combine(OutputDecompressionDir, "quick-guide-gplv3.pdf"));

            var resultState = _fileDecompressor.Process(inputFileInfo, outputFileInfo);

            Assert.AreEqual(104908, outputFileInfo.Length);
            Assert.AreEqual(ProcessState.Finished, resultState.OperationState);
        }

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
