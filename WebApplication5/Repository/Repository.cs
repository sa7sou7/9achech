using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication5.Data;
using WebApplication5.Models;

namespace WebApplication5.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(AppDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task<T> CreateAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<T> DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<T> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<T> UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<Tiers> GetTiersWithContactsAsync(int id)
        {
            return await _context.Tiers
                .Include(t => t.Contacts)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<Tiers>> GetAllWithContactsAsync()
        {
            return await _context.Tiers
                .Include(t => t.Contacts)
                .ToListAsync();
        }

        public async Task<IEnumerable<Tiers>> GetTiersByCommercialIdAsync(string commercialId)
        {
            var tiersIds = await _context.Sales
                .Where(s => s.DocRepresentant == commercialId)
                .Select(s => s.TiersId)
                .Distinct()
                .ToListAsync();

            if (!tiersIds.Any())
            {
                return Enumerable.Empty<Tiers>();
            }

            return await _context.Tiers
                .Include(t => t.Contacts)
                .Where(t => tiersIds.Contains(t.Id))
                .ToListAsync();
        }
    }
}