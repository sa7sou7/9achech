using Microsoft.EntityFrameworkCore;
using WebApplication5.Data;
using WebApplication5.Models;

namespace WebApplication5.Repository
{
    public class ArticleRepository : IArticleRepository
    {
        private readonly AppDbContext _context;

        public ArticleRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Article?> GetByIdAsync(int id)
        {
            return await _context.Articles.FindAsync(id);
        }

        public async Task<List<Article>> GetAllAsync()
        {
            return await _context.Articles.ToListAsync();
        }

        public async Task<Article> UpdateAsync(Article article)
        {
            var existingArticle = await _context.Articles.FindAsync(article.Id);
            if (existingArticle == null)
            {
                throw new KeyNotFoundException($"Article with ID {article.Id} not found.");
            }

            existingArticle.Code = article.Code;
            existingArticle.Designation = article.Designation;
            existingArticle.Famille = article.Famille;
            existingArticle.PrixAchat = article.PrixAchat;
            existingArticle.PrixVente = article.PrixVente;
            existingArticle.StockQuantity = article.StockQuantity;

            await _context.SaveChangesAsync();
            return existingArticle;
        }
    }
}