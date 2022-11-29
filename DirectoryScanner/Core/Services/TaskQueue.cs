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
        public class TaskQueue : ITaskQueue
        {
            private readonly CancellationToken _token;
            private readonly SemaphoreSlim _sem;
            private readonly Task _startNextTask;
            private readonly Task _waitForNextTask;
            private ConcurrentQueue<Task> _waitQueue;

            public TaskQueue(CancellationTokenSource tokenSource, ushort maxThreadCount)
            {
                _waitQueue = new ConcurrentQueue<Task>();
                _sem = new SemaphoreSlim(maxThreadCount);
                _token = tokenSource.Token;
                _startNextTask = new Task(() => StartNext(), _token);
                _waitForNextTask = new Task(() => WaitForNextTask(), _token);
            }

            public void Enqueue(Task task)
            {
                _waitQueue.Enqueue(task);
            }

            public void StartAndWaitAll()
            {
                _startNextTask.Start();
                _waitForNextTask.Start();
                try
                {
                    _startNextTask.Wait(_token);
                    _waitForNextTask.Wait(_token);
                }
                catch (OperationCanceledException)
                {
                    return;
                }
            }

            private void WaitForNextTask()
            {
                Task? task;
                while (!_waitQueue.IsEmpty && !_token.IsCancellationRequested)
                {
                    bool result = _waitQueue.TryPeek(out task);
                    if (result && task != null)
                    {
                        try
                        {
                            task.Wait(_token);
                        }
                        catch (OperationCanceledException)
                        {
                            break;
                        }
                        finally
                        {
                            _sem.Release();
                            _waitQueue.TryDequeue(out _);
                        }
                    }
                }
            }

            private void StartNext()
            {
                Task? task;
                while (!_waitForNextTask.IsCompleted && !_token.IsCancellationRequested)
                {
                    task = _waitQueue.Where(t => t.Status == TaskStatus.Created).FirstOrDefault();
                    if (task != null)
                    {
                        try
                        {
                            _sem.Wait(_token);
                            task.Start();
                        }
                        catch (OperationCanceledException)
                        {
                            break;
                        }
                    }
                }
            }
        }
    }
}
