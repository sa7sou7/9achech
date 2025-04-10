namespace WebApplication5.Services
{
    
        public interface IApiService
        {
        Task<string> GetDataAsync(string endpoint);
    }
}
    
