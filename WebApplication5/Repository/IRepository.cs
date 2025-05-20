using System.Collections.Generic;
using System.Threading.Tasks;
using WebApplication5.Models;

namespace WebApplication5.Repository
{
    public interface IRepository<T> where T : class
    {
        Task<T> CreateAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task<T> GetByIdAsync(int id);
        Task<T> DeleteAsync(T entity);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<Tiers>> GetAllWithContactsAsync();
        Task<Tiers> GetTiersWithContactsAsync(int id);
        Task<IEnumerable<Tiers>> GetTiersByCommercialIdAsync(string commercialId); // New method
    }
}