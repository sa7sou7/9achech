using System;

namespace WebApplication5.Dtos
{
    public class RecoveryDto
    {
        public int Id { get; set; }
        public int VisitId { get; set; }
        public decimal AmountCollected { get; set; }
        public DateTime CollectionDate { get; set; }
        public string Notes { get; set; }
    }
}