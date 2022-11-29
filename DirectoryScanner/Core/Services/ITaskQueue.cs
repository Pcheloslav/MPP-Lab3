namespace Core.Services.Core.Services
{
    public interface ITaskQueue
    {
        void Enqueue(Task task);
        void StartAndWaitAll();
    }
}
