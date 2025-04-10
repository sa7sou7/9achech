namespace WebApplication5.Dto
{
    // DTO for synchronization from external API
    public class ArticleSyncDto
    {
        public string Code { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public string Famille { get; set; } = string.Empty;
        public string PrixAchat { get; set; } = "0"; // Kept as string to match API response
        public string PrixVente { get; set; } = "0"; // Kept as string to match API response
    }

    // DTO for CRUD operations (includes StockQuantity)
    public class ArticleDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public string Famille { get; set; } = string.Empty;
        public decimal PrixAchat { get; set; }
        public decimal PrixVente { get; set; }
        public int StockQuantity { get; set; }
    }
}