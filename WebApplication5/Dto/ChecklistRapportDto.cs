using WebApplication5.Models;

namespace WebApplication5.Dtos
{
    public class ChecklistRapportDto
    {
        public int Id { get; set; }
        public int VisitId { get; set; }
        public ChecklistLibelle Libelle { get; set; }
        public string Commentaire { get; set; }
        public bool IsCompleted { get; set; }
        public decimal? ExpectedRecoveryAmount { get; set; }
        public decimal? RemainingRecoveryAmount { get; set; }
    }
}