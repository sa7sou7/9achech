namespace WebApplication5.Dto
{
    public class OrderLineCreateDto
    {
        public int ArticleId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

    }
}