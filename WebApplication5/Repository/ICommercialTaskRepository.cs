using WebApplication5.Models;

namespace WebApplication5.Repository
{
    public interface ICommercialTaskRepository
    {
        Task<CommercialTask> CreateTaskAsync(CommercialTask task);
        Task<CommercialTask?> GetTaskByIdAsync(int id);
        Task<IEnumerable<CommercialTask>> GetTasksByCommercialAsync(string commercialId);
        Task<bool> UpdateTaskAsync(CommercialTask task);
        Task<bool> DeleteTaskAsync(int id);
    }
}