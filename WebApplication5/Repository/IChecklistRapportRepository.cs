using System.Collections.Generic;
using System.Threading.Tasks;
using WebApplication5.Dtos;
using WebApplication5.Models;

namespace WebApplication5.Repositories
{
    public interface IChecklistRapportRepository
    {
        Task AddAsync(ChecklistRapport checklistRapport);
        Task<ChecklistRapportDto> GetDtoByIdAsync(int id);
        Task<IEnumerable<ChecklistRapportDto>> GetDtosByVisitAsync(int visitId);
        Task<IEnumerable<ChecklistRapportDto>> GetDtosByCommercialAsync(string commercialCref);
        Task<IEnumerable<ChecklistRapport>> GetByCommercialAsync(string commercialCref);
        Task<bool> ChecklistRapportExistsAsync(int id);
        Task<ChecklistRapport> GetByIdAsync(int id);
        Task UpdateAsync(ChecklistRapport checklistRapport);
        Task UpdateChecklistCompletionAsync(int checklistId, bool isCompleted);
    }
}