namespace WebApplication5.Dto
{
    public class OrderLineDto
    {
        public int Id { get; set; }
        public int ArticleId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}