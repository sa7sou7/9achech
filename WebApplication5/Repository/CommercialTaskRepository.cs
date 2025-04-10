using Microsoft.EntityFrameworkCore;
using WebApplication5.Data;
using WebApplication5.Models;

namespace WebApplication5.Repository
{
    public class CommercialTaskRepository : ICommercialTaskRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CommercialTaskRepository> _logger;

        public CommercialTaskRepository(AppDbContext context, ILogger<CommercialTaskRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<CommercialTask> CreateTaskAsync(CommercialTask task)
        {
            _logger.LogInformation($"Creating task for commercial {task.CommercialId}");
            _context.CommercialTasks.Add(task);
            await _context.SaveChangesAsync();
            return task;
        }

        public async Task<CommercialTask?> GetTaskByIdAsync(int id)
        {
            return await _context.CommercialTasks
                .Include(t => t.Commercial)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<CommercialTask>> GetTasksByCommercialAsync(string commercialId)
        {
            return await _context.CommercialTasks
                .Include(t => t.Commercial)
                .Where(t => t.CommercialId == commercialId)
                .ToListAsync();
        }

        public async Task<bool> UpdateTaskAsync(CommercialTask task)
        {
            var existing = await _context.CommercialTasks.FindAsync(task.Id);
            if (existing == null) return false;

            existing.Type = task.Type;
            existing.Description = task.Description;
            existing.Status = task.Status;
            existing.DueDate = task.DueDate;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteTaskAsync(int id)
        {
            var task = await _context.CommercialTasks.FindAsync(id);
            if (task == null) return false;

            _context.CommercialTasks.Remove(task);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}