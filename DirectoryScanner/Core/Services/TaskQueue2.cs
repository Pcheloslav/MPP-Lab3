using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Services
{
    using System.Collections.Concurrent;

    namespace Core.Services
    {
        public class TaskQueue2 : ITaskQueue
        {
            private ConcurrentQueue<Task> _waitQueue;
            private readonly SemaphoreSlim _sem;
            private readonly CancellationToken _token;

            public TaskQueue2(CancellationTokenSource tokenSource, ushort maxThreadCount)
            {
                _waitQueue = new ConcurrentQueue<Task>();
                _sem = new SemaphoreSlim(maxThreadCount);
                _token = tokenSource.Token;
            }

            public void Enqueue(Task task)
            {
                var wrapper = WrapTask(task);
                _waitQueue.Enqueue(wrapper);
                wrapper.Start();
            }

            public void StartAndWaitAll()
            {
                while (!_waitQueue.IsEmpty)
                {
                    var currentCount = _waitQueue.Count;
                    _waitQueue.ToList().ForEach(task => task.Wait());
                    for (var i = 0; i < currentCount; i++) { 
                        _waitQueue.TryDequeue(out _);
                    }
                }
            }

            private Task WrapTask(Task task)
            {
                return new Task(() =>
                {
                    var acquired = false;
                    try
                    {
                        if (!_token.IsCancellationRequested)
                        {
                            try
                            {
                                _sem.Wait(_token);
                                acquired = true;
                                if (task.Status == TaskStatus.Created) {
                                    task.RunSynchronously();
                                }
                            }
                            catch (Exception e) when (e is OperationCanceledException || e is ObjectDisposedException) {
                            }
                        }
                    }
                    finally
                    {
                        if (acquired)
                        {
                            _sem.Release();
                        }
                    }
                });
            }
        }
    }
}
