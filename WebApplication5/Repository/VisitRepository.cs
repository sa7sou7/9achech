using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication5.Data;
using WebApplication5.Dto;
using WebApplication5.Dtos;
using WebApplication5.Models;

namespace WebApplication5.Repositories
{
    public class VisitRepository : IVisitRepository
    {
        private readonly AppDbContext _context;

        public VisitRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<VisitDto>> GetAllDtosByCommercialCrefAsync(string commercialCref)
        {
            return await _context.Visits
                .Include(v => v.Client)
                .Include(v => v.Commercial)
                .Include(v => v.Checklists)
                .Include(v => v.Recoveries)
                .Include(v => v.CompetitorProducts)
                .Include(v => v.Orders).ThenInclude(o => o.OrderLines)
                .Where(v => v.CommercialCref == commercialCref || (v.Commercial != null && v.Commercial.Cref == commercialCref))
                .Select(v => new VisitDto
                {
                    Id = v.Id,
                    Date = v.Date,
                    TiersId = v.TiersId,
                    ClientNom = v.Client != null ? v.Client.Nom : "Client inconnu",
                    Note = v.Note,
                    CommercialCref = v.Commercial != null ? v.Commercial.Cref : v.CommercialCref ?? "N/A",
                    CommercialNom = v.Commercial != null ? v.Commercial.Cnom : "Commercial inconnu",
                    Status = v.Status,
                    Title = $"Visit with {(v.Client != null ? v.Client.Nom : "Unknown")}",
                    Start = v.Date,
                    Checklists = v.Checklists.Select(c => new ChecklistRapportDto
                    {
                        Id = c.Id,
                        VisitId = c.VisitId,
                        Libelle = c.Libelle,
                        Commentaire = c.Commentaire,
                        IsCompleted = c.IsCompleted,
                        ExpectedRecoveryAmount = c.ExpectedRecoveryAmount,
                        RemainingRecoveryAmount = c.RemainingRecoveryAmount
                    }).ToList(),
                    Recoveries = v.Recoveries.Select(r => new RecoveryDto
                    {
                        Id = r.Id,
                        VisitId = r.VisitId,
                        AmountCollected = r.AmountCollected,
                        CollectionDate = r.CollectionDate,
                        Notes = r.Notes
                    }).ToList(),
                    CompetingProducts = v.CompetitorProducts.Select(cp => new CompetitorProductDto
                    {
                        Id = cp.Id,
                        VisitId = cp.VisitId,
                        ProductName = cp.ProductName,
                        Price = cp.Price,
                        ImageUrl = cp.ImageUrl,
                        Notes = cp.Notes
                    }).ToList(),
                    Orders = v.Orders.Select(o => new OrderDto
                    {
                        Id = o.Id,
                        VisitId = o.VisitId,
                        OrderRef = o.OrderRef,
                        TotalAmount = o.TotalAmount,
                        OrderDate = o.OrderDate,
                        OrderLines = o.OrderLines.Select(ol => new OrderLineDto
                        {
                            Id = ol.Id,
                            ArticleId = ol.ArticleId,
                            Quantity = ol.Quantity,
                            UnitPrice = ol.UnitPrice
                        }).ToList()
                    }).ToList()
                })
                .ToListAsync();
        }

        public async Task<VisitDto> GetDtoByIdAsync(int id)
        {
            return await _context.Visits
                .Include(v => v.Client)
                .Include(v => v.Commercial)
                .Include(v => v.Checklists)
                .Include(v => v.Recoveries)
                .Include(v => v.CompetitorProducts)
                .Include(v => v.Orders).ThenInclude(o => o.OrderLines)
                .Where(v => v.Id == id)
                .Select(v => new VisitDto
                {
                    Id = v.Id,
                    Date = v.Date,
                    TiersId = v.TiersId,
                    ClientNom = v.Client != null ? v.Client.Nom : "Client inconnu",
                    Note = v.Note,
                    CommercialCref = v.Commercial != null ? v.Commercial.Cref : v.CommercialCref ?? "N/A",
                    CommercialNom = v.Commercial != null ? v.Commercial.Cnom : "Commercial inconnu",
                    Status = v.Status,
                    Title = $"Visit with {(v.Client != null ? v.Client.Nom : "Unknown")}",
                    Start = v.Date,
                    Checklists = v.Checklists.Select(c => new ChecklistRapportDto
                    {
                        Id = c.Id,
                        VisitId = c.VisitId,
                        Libelle = c.Libelle,
                        Commentaire = c.Commentaire,
                        IsCompleted = c.IsCompleted,
                        ExpectedRecoveryAmount = c.ExpectedRecoveryAmount,
                        RemainingRecoveryAmount = c.RemainingRecoveryAmount
                    }).ToList(),
                    Recoveries = v.Recoveries.Select(r => new RecoveryDto
                    {
                        Id = r.Id,
                        VisitId = r.VisitId,
                        AmountCollected = r.AmountCollected,
                        CollectionDate = r.CollectionDate,
                        Notes = r.Notes
                    }).ToList(),
                    CompetingProducts = v.CompetitorProducts.Select(cp => new CompetitorProductDto
                    {
                        Id = cp.Id,
                        VisitId = cp.VisitId,
                        ProductName = cp.ProductName,
                        Price = cp.Price,
                        ImageUrl = cp.ImageUrl,
                        Notes = cp.Notes
                    }).ToList(),
                    Orders = v.Orders.Select(o => new OrderDto
                    {
                        Id = o.Id,
                        VisitId = o.VisitId,
                        OrderRef = o.OrderRef,
                        TotalAmount = o.TotalAmount,
                        OrderDate = o.OrderDate,
                        OrderLines = o.OrderLines.Select(ol => new OrderLineDto
                        {
                            Id = ol.Id,
                            ArticleId = ol.ArticleId,
                            Quantity = ol.Quantity,
                            UnitPrice = ol.UnitPrice
                        }).ToList()
                    }).ToList()
                })
                .FirstOrDefaultAsync();
        }

        public async Task AddAsync(Visit visit)
        {
            await _context.Visits.AddAsync(visit);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> CommercialExistsAsync(string commercialCref)
        {
            return await _context.Commercials.AnyAsync(c => c.Cref == commercialCref);
        }

        public async Task<bool> TiersExistsAsync(int tiersId)
        {
            return await _context.Tiers.AnyAsync(t => t.Id == tiersId);
        }

        public async Task<bool> VisitExistsAsync(int id)
        {
            return await _context.Visits.AnyAsync(v => v.Id == id);
        }

        public async Task<IEnumerable<VisitDto>> GetAllDtosAsync()
        {
            return await _context.Visits
                .Include(v => v.Client)
                .Include(v => v.Commercial)
                .Include(v => v.Checklists)
                .Include(v => v.Recoveries)
                .Include(v => v.CompetitorProducts)
                .Include(v => v.Orders).ThenInclude(o => o.OrderLines)
                .Select(v => new VisitDto
                {
                    Id = v.Id,
                    Date = v.Date,
                    TiersId = v.TiersId,
                    ClientNom = v.Client != null ? v.Client.Nom : "Client inconnu",
                    Note = v.Note,
                    CommercialCref = v.Commercial != null ? v.Commercial.Cref : v.CommercialCref ?? "N/A",
                    CommercialNom = v.Commercial != null ? v.Commercial.Cnom : "Commercial inconnu",
                    Status = v.Status,
                    Title = $"Visit with {(v.Client != null ? v.Client.Nom : "Unknown")}",
                    Start = v.Date,
                    Checklists = v.Checklists.Select(c => new ChecklistRapportDto
                    {
                        Id = c.Id,
                        VisitId = c.VisitId,
                        Libelle = c.Libelle,
                        Commentaire = c.Commentaire,
                        IsCompleted = c.IsCompleted,
                        ExpectedRecoveryAmount = c.ExpectedRecoveryAmount,
                        RemainingRecoveryAmount = c.RemainingRecoveryAmount
                    }).ToList(),
                    Recoveries = v.Recoveries.Select(r => new RecoveryDto
                    {
                        Id = r.Id,
                        VisitId = r.VisitId,
                        AmountCollected = r.AmountCollected,
                        CollectionDate = r.CollectionDate,
                        Notes = r.Notes
                    }).ToList(),
                    CompetingProducts = v.CompetitorProducts.Select(cp => new CompetitorProductDto
                    {
                        Id = cp.Id,
                        VisitId = cp.VisitId,
                        ProductName = cp.ProductName,
                        Price = cp.Price,
                        ImageUrl = cp.ImageUrl,
                        Notes = cp.Notes
                    }).ToList(),
                    Orders = v.Orders.Select(o => new OrderDto
                    {
                        Id = o.Id,
                        VisitId = o.VisitId,
                        OrderRef = o.OrderRef,
                        TotalAmount = o.TotalAmount,
                        OrderDate = o.OrderDate,
                        OrderLines = o.OrderLines.Select(ol => new OrderLineDto
                        {
                            Id = ol.Id,
                            ArticleId = ol.ArticleId,
                            Quantity = ol.Quantity,
                            UnitPrice = ol.UnitPrice
                        }).ToList()
                    }).ToList()
                })
                .ToListAsync();
        }

        public async Task UpdateVisitStatusAsync(int visitId, VisitStatus status)
        {
            var visit = await _context.Visits.FindAsync(visitId);
            if (visit != null)
            {
                visit.Status = status;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> AreAllChecklistItemsCompletedAsync(int visitId)
        {
            var checklists = await _context.ChecklistRapports
                .Where(c => c.VisitId == visitId)
                .ToListAsync();
            return checklists.Any() && checklists.All(c => c.IsCompleted);
        }

        public async Task<Visit> GetByIdAsync(int id)
        {
            return await _context.Visits
                .Include(v => v.Client)
                .Include(v => v.Commercial)
                .Include(v => v.Checklists)
                .Include(v => v.Recoveries)
                .Include(v => v.CompetitorProducts)
                .Include(v => v.Orders).ThenInclude(o => o.OrderLines)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<IEnumerable<Visit>> GetAllAsync()
        {
            return await _context.Visits
                .Include(v => v.Client)
                .Include(v => v.Commercial)
                .Include(v => v.Checklists)
                .Include(v => v.Recoveries)
                .Include(v => v.CompetitorProducts)
                .Include(v => v.Orders).ThenInclude(o => o.OrderLines)
                .ToListAsync();
        }

        public async Task<IEnumerable<Visit>> GetByCommercialAsync(string commercialCref)
        {
            return await _context.Visits
                .Include(v => v.Client)
                .Include(v => v.Commercial)
                .Include(v => v.Checklists)
                .Include(v => v.Recoveries)
                .Include(v => v.CompetitorProducts)
                .Include(v => v.Orders).ThenInclude(o => o.OrderLines)
                .Where(v => v.CommercialCref == commercialCref)
                .ToListAsync();
        }

        public async Task<IEnumerable<VisitDto>> GetDtosByCommercialAsync(string commercialCref)
        {
            return await GetAllDtosByCommercialCrefAsync(commercialCref);
        }

        public async Task UpdateAsync(Visit visit)
        {
            _context.Visits.Update(visit);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var visit = await _context.Visits.FindAsync(id);
            if (visit != null)
            {
                _context.Visits.Remove(visit);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<string> GetClientNameByTiersIdAsync(int tiersId)
        {
            var client = await _context.Tiers
                .Where(t => t.Id == tiersId)
                .Select(t => t.Nom)
                .FirstOrDefaultAsync();
            return client ?? "Client inconnu";
        }

        public async Task FixInvalidCommercialCrefsAsync()
        {
            var visits = await _context.Visits
                .Include(v => v.Commercial)
                .Where(v => v.CommercialCref == "N/A" || v.CommercialCref == null)
                .ToListAsync();

            foreach (var visit in visits)
            {
                if (visit.Commercial != null && !string.IsNullOrEmpty(visit.Commercial.Cref))
                {
                    visit.CommercialCref = visit.Commercial.Cref;
                }
                else
                {
                    Console.WriteLine($"Visite ID {visit.Id} n'a pas de Commercial associé ou de Cref valide.");
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}