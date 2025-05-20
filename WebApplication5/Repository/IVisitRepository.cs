using System.Collections.Generic;
using System.Threading.Tasks;
using WebApplication5.Dtos;
using WebApplication5.Models;

namespace WebApplication5.Repositories
{
    public interface IVisitRepository
    {
        Task<Visit> GetByIdAsync(int id);
        Task<VisitDto> GetDtoByIdAsync(int id);
        Task<IEnumerable<Visit>> GetAllAsync();
        Task<IEnumerable<VisitDto>> GetAllDtosAsync();
        Task<IEnumerable<Visit>> GetByCommercialAsync(string commercialCref);
        Task<IEnumerable<VisitDto>> GetDtosByCommercialAsync(string commercialCref);
        Task AddAsync(Visit visit);
        Task UpdateAsync(Visit visit);
        Task DeleteAsync(int id);
        Task<bool> CommercialExistsAsync(string commercialCref);
        Task<bool> VisitExistsAsync(int id);
        Task UpdateVisitStatusAsync(int visitId, VisitStatus status);
        Task<bool> AreAllChecklistItemsCompletedAsync(int visitId);
        Task<bool> TiersExistsAsync(int tiersId);
        Task<IEnumerable<VisitDto>> GetAllDtosByCommercialCrefAsync(string commercialCref);
        Task<string> GetClientNameByTiersIdAsync(int tiersId);
    }
}