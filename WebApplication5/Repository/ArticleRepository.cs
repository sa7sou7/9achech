using Microsoft.EntityFrameworkCore;
using WebApplication5.Data;
using WebApplication5.Models;

namespace WebApplication5.Repository
{
    public class ArticleRepository : IArticleRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ArticleRepository> _logger;

        public ArticleRepository(AppDbContext context, ILogger<ArticleRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Article> CreateArticleAsync(Article article)
        {
            _logger.LogInformation($"Creating article with code {article.Code}");
            _context.Articles.Add(article);
            await _context.SaveChangesAsync();
            return article;
        }

        public async Task<Article?> GetArticleByIdAsync(int id)
        {
            return await _context.Articles
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<Article?> GetArticleByCodeAsync(string code)
        {
            return await _context.Articles
                .FirstOrDefaultAsync(a => a.Code == code);
        }

        public async Task<IEnumerable<Article>> SearchArticlesAsync(string searchTerm)
        {
            return await _context.Articles
                .Where(a => a.Code.Contains(searchTerm) || a.Designation.Contains(searchTerm))
                .ToListAsync();
        }

        public async Task<IEnumerable<Article>> GetAllArticlesAsync()
        {
            return await _context.Articles
                .ToListAsync();
        }

        public async Task<bool> UpdateArticleAsync(Article article)
        {
            var existing = await _context.Articles.FindAsync(article.Id);
            if (existing == null) return false;

            existing.Code = article.Code;
            existing.Designation = article.Designation;
            existing.Famille = article.Famille;
            existing.PrixAchat = article.PrixAchat;
            existing.PrixVente = article.PrixVente;
            existing.StockQuantity = article.StockQuantity;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteArticleAsync(int id)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null) return false;

            _context.Articles.Remove(article);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}