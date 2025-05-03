namespace WebApplication5.Dto
{
    public class OrderCreateDto
    {
        public string OrderRef { get; set; }
        public List<OrderLineCreateDto> OrderLines { get; set; }

    }
}
