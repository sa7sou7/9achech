using WebApplication5.Models;

namespace WebApplication5.Repository
{
    public interface IArticleRepository
    {
        Task<Article> CreateArticleAsync(Article article);
        Task<Article?> GetArticleByIdAsync(int id);
        Task<Article?> GetArticleByCodeAsync(string code);
        Task<IEnumerable<Article>> SearchArticlesAsync(string searchTerm);
        Task<IEnumerable<Article>> GetAllArticlesAsync();
        Task<bool> UpdateArticleAsync(Article article);
        Task<bool> DeleteArticleAsync(int id);
    }
}