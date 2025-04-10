namespace WebApplication5.Services
{
    public interface IArticleSyncService
    {
        Task<bool> SynchronizeArticlesAsync();
    }
}