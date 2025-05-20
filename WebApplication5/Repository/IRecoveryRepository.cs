using System.Collections.Generic;
using System.Threading.Tasks;
using WebApplication5.Models;

namespace WebApplication5.Repositories
{
    public interface IRecoveryRepository
    {
        Task<Recovery> GetByIdAsync(int id);
        Task<Recovery> GetByVisitIdAsync(int visitId);
        Task<IEnumerable<Recovery>> GetByCommercialAsync(string commercialCref);
        Task<Recovery> CreateAsync(Recovery recovery);
        Task UpdateAsync(Recovery recovery);
    }
}