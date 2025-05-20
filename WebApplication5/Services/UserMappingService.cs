using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApplication5.Data;

namespace WebApplication5.Services
{
    public class UserMappingService
    {
        private readonly AppDbContext _context;

        public UserMappingService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<string> GetUserIdFromCrefAsync(string commercialCref)
        {
            var commercial = await _context.Commercials
                .Where(c => c.Cref == commercialCref)
                .Select(c => c.UserId) // Assume UserId links to Identity User
                .FirstOrDefaultAsync();
            return commercial ?? throw new Exception($"Commercial with Cref {commercialCref} not found.");
        }
    }
}