using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using WebApplication5.Data;
using WebApplication5.Models;

namespace WebApplication5.Repositories
{
    public class CompetitorProductRepository : ICompetitorProductRepository
    {
        private readonly AppDbContext _context;

        public CompetitorProductRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CompetitorProduct> CreateAsync(CompetitorProduct competitorProduct)
        {
            await _context.CompetitorProducts.AddAsync(competitorProduct);
            await _context.SaveChangesAsync();
            return competitorProduct;
        }
        public async Task<IEnumerable<CompetitorProduct>> GetByVisitIdAsync(int visitId)
        {
            return await _context.CompetitorProducts
                .Where(p => p.VisitId == visitId)
                .ToListAsync();
        }
        public async Task<CompetitorProduct> GetByIdAsync(int id)
        {
            return await _context.CompetitorProducts
                .Include(cp => cp.Visit)
                .FirstOrDefaultAsync(cp => cp.Id == id);
        }
    }
}