using WebApplication5.Models;

namespace WebApplication5.Dtos
{
    public class ChecklistRapportCreateDto
    {
        public ChecklistLibelle Libelle { get; set; }
        public string Commentaire { get; set; } = string.Empty;
        public decimal? ExpectedRecoveryAmount { get; set; }
    }
}