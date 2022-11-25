using Core.Interfaces;
using Core.Models;
using Core.Services.Core.Services;

namespace Core.Services
{
    public class DirectoryScanner : IDirectoryScanner
    {
        private CancellationTokenSource _cancelTokenSource;
        private TaskQueue? _taskQueue;
        public bool IsScanning { get; private set; }

        public DirectoryScanner()
        {
            _cancelTokenSource = new CancellationTokenSource();
        }

        public FileTree Scan(string path, ushort maxThreadCount)
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
                throw new ArgumentException($"Max thread count should be greater than 0");
            }

            IsScanning = true;
            _cancelTokenSource = new CancellationTokenSource();
            _taskQueue = new TaskQueue(_cancelTokenSource, maxThreadCount);
            var token = _cancelTokenSource.Token;
            var directoryInfo = new DirectoryInfo(path);
            var root = new Node(directoryInfo.FullName, directoryInfo.Name, true);
            var rootTask = new Task(() => Start(root), token);
            _taskQueue.Enqueue(rootTask);
            _taskQueue.StartAndWaitAll();
            IsScanning = false;
            return new FileTree(root);
        }

        public void Stop()
        {
            _cancelTokenSource.Cancel();
            IsScanning = false;
        }

        private void Start(Node node)
        {
            node.Children = new List<Node>();
            var directoryInfo = new DirectoryInfo(node.FullName);
            var token = _cancelTokenSource.Token;

            DirectoryInfo[]? directories;
            try
            {
                directories = directoryInfo.GetDirectories().
                    Where(info => info.LinkTarget == null).ToArray(); ;
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

                    Node childNode = new Node(directory.FullName, directory.Name, true);
                    node.Children.Add(childNode);
                    Task task = new Task(() => Start(childNode), token);
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
                    Node childNode = new Node(file.FullName, file.Name, file.Length);
                    node.Children.Add(childNode);
                }
            }
        }
    }
}
