using Core.Interfaces;
using Core.Models;
using Core.Services.Core.Services;
using Microsoft.VisualBasic.FileIO;

namespace Core.Services
{
    public class DirectoryScanner : IDirectoryScanner
    {
        private CancellationTokenSource? _cancelTokenSource;
        private ITaskQueue? _taskQueue;
        public bool IsScanning { get; private set; }

        public DirectoryScanner()
        {
        }

        public FileTree Scan(string path, ushort maxThreadCount, Action<string> action)
        {
            if (File.Exists(path))
            {
                var fileInfo = new FileInfo(path);
                return new FileTree(new Node(fileInfo.FullName, fileInfo.Name, fileInfo.Length));
            }

            if (!Directory.Exists(path))
            {
                throw new ArgumentException($"Directory {path} does not exist");
            }

            if (maxThreadCount == 0)
            {
                throw new ArgumentException("Max thread count should be greater than 0");
            }

            IsScanning = true;
            _cancelTokenSource = new CancellationTokenSource();
            try {
                var token = _cancelTokenSource.Token;
                var directoryInfo = new DirectoryInfo(path);
                var root = new Node(directoryInfo.FullName, directoryInfo.Name, true);
                var rootTask = new Task(() => Start(root, token, action), token);
                _taskQueue = new TaskQueue2(_cancelTokenSource, maxThreadCount);
                _taskQueue.Enqueue(rootTask);
                _taskQueue.StartAndWaitAll();
                return new FileTree(root);
            }
            finally
            {
                IsScanning = false;
                if (_cancelTokenSource != null)
                {
                    _cancelTokenSource.Dispose();
                    _cancelTokenSource = null;
                }
            }
        }

        public void Stop()
        {
            if (_cancelTokenSource != null)
            {
                _cancelTokenSource.Cancel();
                _cancelTokenSource.Dispose();
                _cancelTokenSource = null;
            }
            IsScanning = false;
        }

        private void Start(Node node, CancellationToken token, Action<string> onScanStart)
        {
            node.Children = new List<Node>();
            var directoryInfo = new DirectoryInfo(node.FullName);
            onScanStart(node.FullName);

            DirectoryInfo[]? directories;
            try
            {
                directories = directoryInfo.GetDirectories().
                    Where(info => info.LinkTarget == null).ToArray();
            }
            catch (Exception)
            {
                directories = null;
            }

            if (directories != null)
            {
                foreach (var directory in directories)
                {
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }

                    Node childNode = new(directory.FullName, directory.Name, true);
                    node.Children.Add(childNode);
                    Task task = new(() => Start(childNode, token, onScanStart), token);
                    _taskQueue!.Enqueue(task);
                }
            }

            FileInfo[]? files;
            try
            {
                files = directoryInfo.GetFiles()
                    .Where(info => info.LinkTarget == null).ToArray();
            }
            catch
            {
                files = null;
            }

            if (files != null)
            {
                foreach (var file in files)
                {
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }
                    Node childNode = new(file.FullName, file.Name, file.Length);
                    node.Children.Add(childNode);
                }
            }
            Thread.Sleep(100);
        }
    }
}
