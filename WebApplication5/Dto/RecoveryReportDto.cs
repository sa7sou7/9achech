using WebApplication5.Models;

namespace WebApplication5.Dtos
{
    public class RecoveryReportDto
    {
        public int VisitId { get; set; }
        public int ChecklistId { get; set; }
        public string ClientNom { get; set; }
        public string CommercialCref { get; set; }
        public decimal ExpectedRecoveryAmount { get; set; }
        public decimal RemainingRecoveryAmount { get; set; }
        public decimal CollectedAmount { get; set; }
        public VisitStatus VisitStatus { get; set; }
        public DateTime? LastCollectionDate { get; set; }
    }
}