using System;
using System.IO;
using System.Linq;
using GZipArchiver.App.Helpers;
using GZipArchiver.App.Settings;
using GZipArchiver.Lib;
using GZipArchiver.Lib.Enums;

namespace GZipArchiver
{
    class Program
    {
        private const string DefaultExtension = ".vgz";

        static int Main(string[] args)
        {
            try
            {
                InputValidator.StringReadValidation(args);
            }
            catch (ApplicationException validationException)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(validationException.Message);
                Console.ForegroundColor = ConsoleColor.White;
                return 1;
            }

            bool isCompress = args[0].ToLower() == "compress";
            var archiver = CreateArchiver(isCompress);
            var inputFileInfo = new FileInfo(args[1]);
            var outputFileInfo = new FileInfo(GetOutputFilePath(args[2], isCompress));
            var result = archiver.Process(inputFileInfo, outputFileInfo);

            switch (result.OperationState)
            {
                case ProcessState.Error:
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("An error occurred.");
                    foreach (string error in result.Errors.Distinct())
                    {
                        Console.Write("\t");
                        Console.WriteLine(error);
                    }
                    
                    Console.ForegroundColor = ConsoleColor.White;
                    return 1;
                }
                case ProcessState.Finished:
                {
                    Console.WriteLine("Successfully finished.");
                    return 0;
                }
                default:
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Unknown error.");
                    Console.ForegroundColor = ConsoleColor.White;
                    return 1;
                }
            }
        }

        static BaseFileArchiver CreateArchiver(bool isCompress)
        {
            var settings = SettingsReader<ArchiverSettings>.ReadSettings();
            int threadCount = settings.WorkThreadCount > 0 ? settings.WorkThreadCount : Environment.ProcessorCount - 2;
            int bufferSize = settings.BlockSize > 0 ? settings.BlockSize : 1024 * 1024;
            int inputQueueCapacity = settings.QueueInSize > 0 ? settings.QueueInSize : 100;
            int outputQueueCapacity = settings.QueueOutSize > 0 ? settings.QueueOutSize : 100;

            BaseFileArchiver archiver;
            if (isCompress)
            {
                archiver = new FileCompressor(threadCount, bufferSize, inputQueueCapacity, outputQueueCapacity);
            }
            else
            {
                archiver = new FileDecompressor(threadCount, bufferSize, inputQueueCapacity, outputQueueCapacity);
            }

            return archiver;
        }

        static string GetOutputFilePath(string fileName, bool isCompress)
        {
            if (isCompress && string.IsNullOrEmpty(Path.GetExtension(fileName)))
            {
                return fileName + DefaultExtension;
            }

            return fileName;
        }
    }
}
