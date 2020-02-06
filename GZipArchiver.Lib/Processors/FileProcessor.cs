using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using GZipArchiver.Lib.Collections;
using GZipArchiver.Lib.Enums;
using GZipArchiver.Lib.Factories;
using GZipArchiver.Lib.IO;
using GZipArchiver.Lib.Operations;
using GZipArchiver.Lib.Threads;

namespace GZipArchiver.Lib.Processors
{
    internal class FileProcessor : IProcessor, IDisposable
    {
        #region Fields, Constants, Properties

        private const int ThreadCountLimit = 1;
        private const int InputQueueCapacityLimit = 1;
        private const int OutputQueueCapacityLimit = 1;

        private readonly int _workThreadCount;

        private readonly IFileReader _inputFileReader;
        private readonly IFileWriter _outputFileWriter;

        private readonly SyncQueue<FileChunkItem> _inputFileChunks;
        private readonly SyncTapeArray<FileChunkItem> _outputFileChunks;

        private readonly CompressionMode _compressionMode;
        private readonly IWorkItemFactory _workItemFactory;

        private readonly CountdownEvent _allThreadsCountdown = null;
        private readonly ManualResetEvent _startHandle = new ManualResetEvent(false);

        private ReadFileChunksOperation _readFileChunksOperation = null;
        private WriteFileChunksOperation _writeFileChunksOperation = null;
        private ProcessChunksOperation[] _processChunksOperations = null;

        private OperationContext _operationContext = null;

        private IWorkItem[] _workItems = null;
        private ProcessOperationThread[] _processThreads = null;
        private ReadOperationThread _readThread = null;
        private WriteOperationThread _writeThread = null;

        #endregion

        #region Ctor

        public FileProcessor(IFileReader inputFileReader, IFileWriter outputFileWriter, CompressionMode compressionMode,
            int workThreadCount, int inputQueueCapacity, int outputQueueCapacity)
        {
            #region Input arguments validation

            if (workThreadCount < ThreadCountLimit)
            {
                throw new ArgumentOutOfRangeException(nameof(workThreadCount),
                    $"Number of working threads must be greater than or equal to {ThreadCountLimit}.");
            }

            if (inputQueueCapacity < InputQueueCapacityLimit)
            {
                throw new ArgumentOutOfRangeException(nameof(workThreadCount),
                    $"The capacity of the processor's input queue must be greater than or equal to {InputQueueCapacityLimit}.");
            }

            if (outputQueueCapacity < OutputQueueCapacityLimit)
            {
                throw new ArgumentOutOfRangeException(nameof(workThreadCount),
                    $"The capacity of the processor's input queue must be greater than or equal to {OutputQueueCapacityLimit}.");
            }

            #endregion

            _compressionMode = compressionMode;
            _workThreadCount = workThreadCount;
            _inputFileReader = inputFileReader;
            _outputFileWriter = outputFileWriter;

            _inputFileChunks = new SyncQueue<FileChunkItem>(inputQueueCapacity);
            _outputFileChunks = new SyncTapeArray<FileChunkItem>(outputQueueCapacity);

            _workItemFactory = new WorkItemFactory();

            _workItems = _workThreadCount > 0
                ? new IWorkItem[_workThreadCount]
                : new IWorkItem[] { };

            _processChunksOperations = _workThreadCount > 0
                ? new ProcessChunksOperation[_workThreadCount]
                : new ProcessChunksOperation[] { };

            _allThreadsCountdown = new CountdownEvent(_workThreadCount + 3); //// works + read + write + main
        }

        #endregion

        #region Public methods

        public OperationResult Process()
        {
            try
            {
                CreateOperationsAndContext();
                CreateOperationThreads();
                StartOperationThreads();

                var result = AggregateOperationResult();
                return result;
            }
            catch (Exception processException)
            {
                return new OperationResult(ProcessState.Error, new[] {processException.Message});
            }
        }

        public void CancelAllOperations()
        {
            System.Diagnostics.Debug.WriteLine($"[{nameof(FileProcessor).ToUpper()}] {nameof(CancelAllOperations)}");

            _readFileChunksOperation?.Cancel();
            _writeFileChunksOperation?.Cancel();

            if (_processChunksOperations != null)
            {
                foreach (var processChunksOperation in _processChunksOperations)
                {
                    processChunksOperation?.Cancel();
                }
            }
        }

        public void Dispose()
        {
            if (_workItems == null) return;

            foreach (var workItem in _workItems)
            {
                workItem?.Dispose();
            }
        }

        #endregion

        #region Private methods

        private void CreateOperationsAndContext()
        {
            _operationContext = new OperationContext();

            _readFileChunksOperation = new ReadFileChunksOperation(_inputFileReader, _inputFileChunks, _operationContext);
            _writeFileChunksOperation = new WriteFileChunksOperation(_outputFileWriter, _outputFileChunks, _operationContext);

            for (int wtIndex = 0; wtIndex < _workThreadCount; wtIndex++)
            {
                var workItem = _workItemFactory.CreateWorkItem(_compressionMode);
                _workItems[wtIndex] = workItem;

                var processChunksOperation = new ProcessChunksOperation(_inputFileChunks, _outputFileChunks, workItem, _operationContext);
                _processChunksOperations[wtIndex] = processChunksOperation;
            }
        }

        private void OnOperationThreadFinish(OperationResult operationResult)
        {
            if (operationResult.OperationState == ProcessState.Error)
            {
                // to wake up other threads if they wait
                _startHandle.Set();

                CancelAllOperations();
            }

            _allThreadsCountdown.Signal();
        }

        private void CreateOperationThreads()
        {
            _readThread = CreateReadThread();
            _writeThread = CreateWriteThread();
            _processThreads = CreateProcessThreads();
        }

        private ReadOperationThread CreateReadThread()
        {
            var readThread = new ReadOperationThread(_readFileChunksOperation);
            readThread.Name = "Read thread";
            readThread.SetBeforeStartCallback(() =>
            {
                // set counters
                int iterationCount = _inputFileReader.GetIterationCount();
                _operationContext.SetCounters(iterationCount);
                _startHandle.Set();
            });
            readThread.SetAfterStartCallback(OnOperationThreadFinish);
            return readThread;
        }

        private WriteOperationThread CreateWriteThread()
        {
            var writeThread = new WriteOperationThread(_writeFileChunksOperation);
            writeThread.Name = "Write thread";
            writeThread.SetBeforeStartCallback(() =>
            {
                if (!_operationContext.IsCountSet)
                {
                    _startHandle.WaitOne();
                }
            });
            writeThread.SetAfterStartCallback(OnOperationThreadFinish);
            return writeThread;
        }

        private ProcessOperationThread[] CreateProcessThreads()
        {
            var processThreads = _workThreadCount > 0
                ? new ProcessOperationThread[_workThreadCount]
                : new ProcessOperationThread[] { };
            for (int wtIndex = 0; wtIndex < _workThreadCount; wtIndex++)
            {
                var processThread = new ProcessOperationThread(_processChunksOperations[wtIndex]);
                processThread.Name = "Work Thread #" + wtIndex;
                processThread.SetBeforeStartCallback(() =>
                {
                    if (!_operationContext.IsCountSet)
                    {
                        _startHandle.WaitOne();
                    }
                });
                processThread.SetAfterStartCallback(OnOperationThreadFinish);
                processThreads[wtIndex] = processThread;
            }

            return processThreads;
        }

        /// <summary>
        /// Starts threads and waits.
        /// </summary>
        private void StartOperationThreads()
        {
            _readThread.Start();
            _writeThread.Start();

            foreach (var workThread in _processThreads)
            {
                workThread.Start();
            }

            _allThreadsCountdown.Signal();
            _allThreadsCountdown.Wait();
        }

        private OperationResult AggregateOperationResult()
        {
            // finish
            if (_readThread.Result.OperationState == ProcessState.Finished
                && _writeThread.Result.OperationState == ProcessState.Finished
                && _processThreads.All(x => x.Result.OperationState == ProcessState.Finished))
            {
                return new OperationResult(ProcessState.Finished, null);
            }

            // error + list of errors
            if (_readThread.Result.OperationState == ProcessState.Error
                || _writeThread.Result.OperationState == ProcessState.Error
                || _processThreads.Any(x => x.Result.OperationState == ProcessState.Error))
            {
                var errors = new List<string>();

                if (_readThread.Result.OperationState == ProcessState.Error)
                {
                    errors.AddRange(_readThread.Result.Errors);
                }

                if (_writeThread.Result.OperationState == ProcessState.Error)
                {
                    errors.AddRange(_writeThread.Result.Errors);
                }

                var processErrors = _processThreads
                    .Where(x => x.Result.OperationState == ProcessState.Error)
                    .SelectMany(x => x.Result.Errors);
                errors.AddRange(processErrors);

                return new OperationResult(ProcessState.Error, errors);
            }

            // cancel
            return new OperationResult(ProcessState.Cancelled, null);
        }

        #endregion
    }
}
