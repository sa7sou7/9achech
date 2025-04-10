namespace WebApplication5.Dto
{
    public class VisitDto
    {
        public string CommercialId { get; set; }
        public int TiersId { get; set; }
        public string ScheduledDate { get; set; }
        public string? CompletedDate { get; set; }
        public string Report { get; set; }
        public bool IsValidated { get; set; }
        public List<VisitChecklistDto> Objectives { get; set; } = new List<VisitChecklistDto>();
    }

    public class VisitReportDto
    {
        public string Report { get; set; }
    }

    public class VisitChecklistDto
    {
        public string ObjectiveType { get; set; }
        public bool IsSelected { get; set; }
        public decimal? Encours { get; set; }
        public bool? ReglementPret { get; set; }
        public bool? Recupere { get; set; }
        public List<VisitOrderItemDto> OrderItems { get; set; } = new List<VisitOrderItemDto>();
    }

    public class VisitOrderItemDto
    {
        public int ArticleId { get; set; }
        public string ArticleCode { get; set; } = string.Empty;
        public string ArticleDesignation { get; set; } = string.Empty;
        public decimal UnitPriceHT { get; set; }
        public decimal Discount { get; set; }
        public int Quantity { get; set; }
        public int AvailableQuantity { get; set; }
        public decimal TotalHT => Quantity * UnitPriceHT - Discount;
        public decimal TotalTTC => TotalHT * 1.19m; // Assuming 19% VAT
    }
}