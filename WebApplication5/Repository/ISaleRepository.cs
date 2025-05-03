using WebApplication5.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace WebApplication5.Repository
{
    public interface ISaleRepository
    {
        Task<IEnumerable<Sale>> GetAllAsync();
        Task<Sale?> GetByIdAsync(int id);
        Task<Sale> AddAsync(Sale sale);
        Task<bool> UpdateAsync(Sale sale);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<Tiers>> GetTiersByCommercialAsync(string commercialId);
        Task<IEnumerable<Sale>> GetSalesByCommercialAsync(string commercialId);
    }
}