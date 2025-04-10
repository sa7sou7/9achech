namespace WebApplication5.Services
{
    public interface ISyncService
    {
        Task<string> SynchronizeCommercialsAsync();
        Task<string> SynchronizeClientsAsync();
        Task<string> SynchronizeSalesAsync();

    }
}