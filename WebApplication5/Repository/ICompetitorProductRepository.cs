using System.Threading.Tasks;
using WebApplication5.Models;

namespace WebApplication5.Repositories
{
    public interface ICompetitorProductRepository
    {
        Task<CompetitorProduct> CreateAsync(CompetitorProduct competitorProduct);
        Task<CompetitorProduct> GetByIdAsync(int id);
        Task<IEnumerable<CompetitorProduct>> GetByVisitIdAsync(int visitId);
    }
}