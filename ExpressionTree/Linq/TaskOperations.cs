using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Core.Linq;

public static class TaskOperations
{
    public static T WaitAndGet<T>(this Task<T> task)
    {
        task.Wait();
        return task.Result;
    }
}