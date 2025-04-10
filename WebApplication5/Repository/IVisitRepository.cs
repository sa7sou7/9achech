using WebApplication5.Models;

namespace WebApplication5.Repository
{
    public interface IVisitRepository
    {
        Task<Visit> CreateVisitAsync(Visit visit);
        Task<Visit?> GetVisitByIdAsync(int id);
        Task<IEnumerable<Visit>> GetVisitsByCommercialAsync(string commercialId);
        Task<bool> UpdateVisitAsync(Visit visit);
        Task<bool> DeleteVisitAsync(int id);
    }
}