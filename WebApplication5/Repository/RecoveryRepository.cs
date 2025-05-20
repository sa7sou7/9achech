using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication5.Data;
using WebApplication5.Models;

namespace WebApplication5.Repositories
{
    public class RecoveryRepository : IRecoveryRepository
    {
        private readonly AppDbContext _context;

        public RecoveryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Recovery> GetByIdAsync(int id)
        {
            return await _context.Recoveries.FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<Recovery> GetByVisitIdAsync(int visitId)
        {
            return await _context.Recoveries
                .Where(r => r.VisitId == visitId)
                .OrderByDescending(r => r.CollectionDate)
                .FirstOrDefaultAsync();
        }
        public async Task<IEnumerable<Recovery>> GetByCommercialAsync(string commercialCref)
        {
            return await _context.Recoveries
                .Include(r => r.Visit)
                .Where(r => r.Visit.CommercialCref == commercialCref)
                .ToListAsync();
        }

        public async Task<Recovery> CreateAsync(Recovery recovery)
        {
            await _context.Recoveries.AddAsync(recovery);
            await _context.SaveChangesAsync();
            return recovery;
        }

        public async Task UpdateAsync(Recovery recovery)
        {
            _context.Recoveries.Update(recovery);
            await _context.SaveChangesAsync();
        }
    }
}