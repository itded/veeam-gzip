using System.IO;
using System.Linq;
using GZipArchiver.Lib.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GZipArchiver.Lib.Tests
{
    [TestClass]
    public class InvalidFileTests
    {
        private const string InputDecompressionDir = "DecompressionInvalid";
        private const string OutputDecompressionDir = InputDecompressionDir + "Output";

        private FileDecompressor _fileDecompressor = null;

        [TestInitialize]
        public void Init()
        {
            if (!Directory.Exists(OutputDecompressionDir))
            {
                Directory.CreateDirectory(OutputDecompressionDir);
            }

            _fileDecompressor = new FileDecompressor(4, 4096, 10, 10);
        }

        [TestMethod]
        public void ExtractPdfFile()
        {
            FileInfo inputFileInfo = new FileInfo(Path.Combine(InputDecompressionDir, "quick-guide-gplv3-inv.vgz"));
            FileInfo outputFileInfo = new FileInfo(Path.Combine(OutputDecompressionDir, "quick-guide-gplv3-inv.pdf"));

            var resultState = _fileDecompressor.Process(inputFileInfo, outputFileInfo);

            Assert.AreEqual(ProcessState.Error, resultState.OperationState);
            Assert.AreEqual(true, resultState.Errors.Any());
        }
    }
}
